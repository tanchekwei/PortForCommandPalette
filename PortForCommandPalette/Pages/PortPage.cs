// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.Indexer.Indexer;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using PortForCommandPalette.Classes;
using PortForCommandPalette.Commands;
using PortForCommandPalette.Enums;
using PortForCommandPalette.Listeners;
using PortForCommandPalette.Pages;
using PortForCommandPalette.Services;
using PortForCommandPalette.Workspaces;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.System;

namespace PortForCommandPalette;

public sealed partial class PortsPage : DynamicListPage, INotifyItemsChanged, IDisposable
{
    private readonly List<PortInfo> _allPorts = [];
    private readonly Dictionary<string, PortInfo> _allPortsById = [];
    private readonly SettingsManager _settingsManager;
    private readonly SettingsListener _settingsListener;
    private readonly PortService _portService;
    public SettingsManager SettingsManager { get; } = new();
    private readonly List<ListItem> _visibleItems = [];
    private readonly Dictionary<string, ListItem> _listItemCache = [];
    private List<PortInfo> _cachedFilteredPorts = [];

    private readonly object _itemsLock = new();
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);
    private bool _isDisposed;

    private CancellationTokenSource? _pollingCts;
    private int _subscriberCount;
    private int _remainingPollingDelay;
    private const int LoadingPulseMilliseconds = 100;
    private RefreshCommand _refreshCommand;
    private CommandContextItem _refreshCommandContextItem;
    private CommandContextItem _helpCommandContextItem;
    event TypedEventHandler<object, IItemsChangedEventArgs> INotifyItemsChanged.ItemsChanged
    {
        add
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            lock (_itemsLock)
            {
                ItemsChanged += value;
                _subscriberCount++;
                if (_subscriberCount == 1 && _settingsManager.PollingIntervalMilliseconds > 0)
                {
                    _pollingCts?.Cancel();
                    _pollingCts = new CancellationTokenSource();
                    _ = PollAsync(_pollingCts.Token);
                }
            }
        }
        remove
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            lock (_itemsLock)
            {
                ItemsChanged -= value;
                if (_subscriberCount > 0)
                {
                    _subscriberCount--;
                    if (_subscriberCount == 0)
                    {
                        _pollingCts?.Cancel();
                        _pollingCts = null;
                    }
                }
            }
        }
    }
    public PortsPage
    (
        SettingsManager settingsManager,
        SettingsListener settingsListener,
        RefreshCommand refreshCommand,
        PortService portService
    )
    {
        _portService = portService;
        _settingsManager = settingsManager;
        SettingsManager = _settingsManager;

        Name = Constant.PageName;
        Icon = Classes.Icon.Extension;
        Id = $"{Package.Current.Id.Name}.{nameof(PortsPage)}";
        ShowDetails = _settingsManager.ShowDetails;

        UpdateUIStrings();

        var filters = new SearchFilters();
        filters.PropChanged += Filters_PropChanged;
        Filters = filters;

        // _pinService = pinService;
        _helpCommandContextItem = new CommandContextItem(new HelpPage(settingsManager, null));
        _refreshCommand = refreshCommand;
        _refreshCommand.TriggerRefresh += (s, e) => StartRefresh();
        _refreshCommandContextItem = new CommandContextItem(_refreshCommand)
        {
            MoreCommands = [
                _helpCommandContextItem,
            ],
            RequestedShortcut = KeyChordHelpers.FromModifiers(false, false, false, false, (int)VirtualKey.F5, 0),
        };
        _settingsListener = settingsListener;
        _settingsListener.PageSettingsChanged += OnPageSettingsChanged;
        _settingsListener.SortSettingsChanged += OnSortSettingsChanged;
        _settingsListener.PollingIntervalChanged += OnPollingIntervalChanged;
        UpdateRemainingPollingDelay();

        _ = RefreshPortsAsync(true);
    }

    private void UpdateUIStrings()
    {
        PlaceholderText = PortFilter.GetSearchPlaceholder(_settingsManager);
        EmptyContent = new CommandItem(new NoOpCommand())
        {
            Icon = Icon,
            Title = "No matching connections found",
            Subtitle = "Try a different search term or check your settings.",
        };
    }

    public void StartRefresh()
    {
        _ = RefreshPortsAsync(isUserInitiated: true);
    }

    public override IListItem[] GetItems()
    {
        lock (_itemsLock)
        {
            if (_allPorts.Count == 0 && !IsLoading)
            {
                _ = RefreshPortsAsync(isUserInitiated: false);
            }
            
            return _visibleItems.ToArray();
        }
    }

    public IListItem? GetCommandItem(string id)
    {
        lock (_itemsLock)
        {
            if (_allPorts.Count == 0 && !IsLoading)
            {
                _ = RefreshPortsAsync(isUserInitiated: false);
            }

            if (_allPortsById.TryGetValue(id, out var port))
            {
                return GetOrCreateListItem(port, true);
            }
        }
        return null;
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        FilterAndRefreshVisibleItems();
    }

    public override void LoadMore()
    {
        lock (_itemsLock)
        {
            var currentCount = _visibleItems.Count;
            var itemsToLoadCount = Math.Min(_cachedFilteredPorts.Count - currentCount, _settingsManager.PageSize);
            var nextBatch = _cachedFilteredPorts.GetRange(currentCount, itemsToLoadCount);

            _portService.ResolveProcessInfo(nextBatch);

            foreach (var port in nextBatch)
            {
                _visibleItems.Add(GetOrCreateListItem(port));
            }

            HasMoreItems = _visibleItems.Count < _cachedFilteredPorts.Count;
        }
        RaiseItemsChanged(_visibleItems.Count);
    }

    public ListItem GetOrCreateListItem(PortInfo port, bool isTopLevelCommand = false)
    {
        lock (_itemsLock)
        {
            if (!_listItemCache.TryGetValue(port.Id, out var listItem))
            {
                listItem = PortItemFactory.Create(port, _refreshCommandContextItem, _settingsManager);
                _listItemCache[port.Id] = listItem;
            }

            if (isTopLevelCommand)
            {
                var newMoreCommands = new List<ICommandContextItem>();
                if (listItem.MoreCommands != null)
                {
                    foreach (var mc in listItem.MoreCommands)
                    {
                        if (mc is CommandContextItem cci)
                        {
                            if (cci.Command is HelpPage || cci.Command is RefreshCommand)
                            {
                                continue;
                            }
                            newMoreCommands.Add(cci);
                        }
                    }
                }

                return new ListItem(listItem.Command ?? new NoOpCommand())
                {
                    Title = listItem.Title,
                    Subtitle = listItem.Subtitle,
                    Details = listItem.Details,
                    Icon = listItem.Icon,
                    Tags = listItem.Tags,
                    MoreCommands = newMoreCommands.ToArray()
                };
            }
            return listItem;
        }
    }

    private async Task RefreshPortsAsync(bool isUserInitiated, bool isBackground = false)
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif
        if (_isDisposed || !_refreshSemaphore.Wait(0)) return;

        try
        {
            if (!isBackground)
            {
                IsLoading = true;
            }

            var filterId = Filters?.CurrentFilterId ?? string.Empty;
            _ = Enum.TryParse(filterId, out FilterType filterType);

            var ports = await Task.Run(() => _portService.GetPorts(filterType));

            lock (_itemsLock)
            {
                _allPorts.Clear();
                _allPortsById.Clear();

                var activeIds = new HashSet<string>();
                foreach (var port in ports)
                {
                    _allPorts.Add(port);
                    _allPortsById[port.Id] = port;
                    activeIds.Add(port.Id);
                }

                var keysToRemove = new List<string>();
                foreach (var key in _listItemCache.Keys)
                {
                    if (!activeIds.Contains(key)) keysToRemove.Add(key);
                }
                foreach (var key in keysToRemove) _listItemCache.Remove(key);
            }

            FilterAndRefreshVisibleItems();
            if (isUserInitiated) new ToastStatusMessage($"Refreshed {ports.Count} items").Show();
        }
        finally
        {
            if (!isBackground)
            {
                IsLoading = false;
            }

            _refreshSemaphore.Release();
        }
    }

    private async Task PollAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                IsLoading = true;

                // Wait for the refresh to complete and ensure the loading indicator is visible for at least the pulse duration
                await Task.WhenAll(RefreshPortsAsync(isUserInitiated: false, isBackground: true), Task.Delay(LoadingPulseMilliseconds, token));

                IsLoading = false;
                await Task.Delay(_remainingPollingDelay, token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // Ignore errors and keep polling until cancelled
            }
        }
    }

    private void OnPageSettingsChanged(object? sender, EventArgs e)
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif
        ShowDetails = _settingsManager.ShowDetails;
        UpdateUIStrings();
        UpdateSearchText(string.Empty, SearchText);
    }

    private void OnSortSettingsChanged(object? sender, EventArgs e)
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif
        UpdateSearchText(string.Empty, SearchText);
    }

    private void OnPollingIntervalChanged(object? sender, EventArgs e)
    {
        UpdateRemainingPollingDelay();
        lock (_itemsLock)
        {
            if (_subscriberCount > 0)
            {
                if (_settingsManager.PollingIntervalMilliseconds > 0)
                {
                    if (_pollingCts == null)
                    {
                        _pollingCts = new CancellationTokenSource();
                        _ = PollAsync(_pollingCts.Token);
                    }
                }
                else
                {
                    _pollingCts?.Cancel();
                    _pollingCts = null;
                }
            }
        }
    }

    private void UpdateRemainingPollingDelay()
    {
        _remainingPollingDelay = Math.Max(0, _settingsManager.PollingIntervalMilliseconds - LoadingPulseMilliseconds);
    }

    private void Filters_PropChanged(object sender, IPropChangedEventArgs args)
    {
        _ = RefreshPortsAsync(isUserInitiated: false);
    }

    public void ClearAllItems()
    {
        _listItemCache.Clear();
    }

    public void SetSearchText(string query)
    {
        SearchText = query;
    }

    public void Dispose()
    {
        lock (_itemsLock)
        {
            if (_isDisposed) return;
            _isDisposed = true;
        }

        _pollingCts?.Cancel();
        _pollingCts?.Dispose();
        _settingsListener.PageSettingsChanged -= OnPageSettingsChanged;
        _settingsListener.SortSettingsChanged -= OnSortSettingsChanged;
        _settingsListener.PollingIntervalChanged -= OnPollingIntervalChanged;
        _refreshSemaphore.Dispose();
    }

    private void FilterAndRefreshVisibleItems()
    {
        lock (_itemsLock)
        {
            var filterId = Filters?.CurrentFilterId ?? string.Empty;
            _ = Enum.TryParse(filterId, out FilterType filterType);

            _cachedFilteredPorts = PortFilter.ApplyFilterAndSort(
                _allPorts,
                SearchText,
                filterType,
                _settingsManager,
                ports => _portService.ResolveProcessNames(ports));

            _visibleItems.Clear();
            var visibleBatchCount = Math.Min(_cachedFilteredPorts.Count, _settingsManager.PageSize);
            var visibleBatch = _cachedFilteredPorts.GetRange(0, visibleBatchCount);

            _portService.ResolveProcessInfo(visibleBatch);

            foreach (var port in visibleBatch)
            {
                _visibleItems.Add(GetOrCreateListItem(port));
            }

            HasMoreItems = _cachedFilteredPorts.Count > _visibleItems.Count;
        }

        RaiseItemsChanged(_visibleItems.Count);
    }
}