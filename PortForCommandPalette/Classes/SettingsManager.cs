// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CommandPalette.Extensions.Toolkit;
using PortForCommandPalette.Classes;
using PortForCommandPalette.Enums;

namespace PortForCommandPalette;

public class SettingsManager : JsonSettingsManager
{
    private static readonly string _namespace = "port";

    private static string Namespaced(string propertyName) => $"{_namespace}.{propertyName}";
    private static readonly List<ChoiceSetSetting.Choice> _sortByChoices =
    [
        new ChoiceSetSetting.Choice(
            "Process Name\n" +
            "- Sorted alphabetically by the process name.",
            nameof(SortBy.ProcessName)
        ),
        new ChoiceSetSetting.Choice(
            "Local Port\n" +
            "- Sorted ascending by local port number.",
            nameof(SortBy.LocalPort)
        ),
        new ChoiceSetSetting.Choice(
            "Protocol\n" +
            "- Sorted alphabetically by protocol (TCP, UDP, etc.).",
            nameof(SortBy.Protocol)
        ),
        new ChoiceSetSetting.Choice(
            "State\n" +
            "- Sorted alphabetically by connection state.",
            nameof(SortBy.State)
        ),
    ];

    private readonly ToggleSetting _enableLogging = new(
        Namespaced(nameof(EnableLogging)),
        "Enable Logging",
        "Enables diagnostic logging for troubleshooting.",
        false);

    private readonly TextSetting _pageSize = new(
        Namespaced(nameof(PageSize)),
        "Page Size",
        "Number of items to load per page.",
        "16");

    private readonly TextSetting _pollingIntervalMilliseconds = new(
        Namespaced(nameof(PollingIntervalMilliseconds)),
        "Polling Interval",
        "Interval in milliseconds for port status updates while the page is visible.\nPolling stops when the page is unloaded / hidden / unfocus.",
        "1000");

    private readonly ChoiceSetSetting _sortBy = new(
        Namespaced(nameof(SortBy)),
        "Sort By",
        "Determines how items are sorted when no search text is entered.",
        _sortByChoices);

    private readonly ToggleSetting _showDetails = new(
        Namespaced(nameof(ShowDetails)),
        "Show Details",
        "Show the details pane for selected items.",
        true);

    private readonly ToggleSetting _searchProcessName = new(
        Namespaced(nameof(SearchProcessName)),
        "Search Process Name",
        "Filter items by process name when searching.",
        true);

    private readonly ToggleSetting _searchLocalAddress = new(
        Namespaced(nameof(SearchLocalAddress)),
        "Search Local Address",
        "Filter items by local address when searching.",
        true);

    private readonly ToggleSetting _searchLocalPort = new(
        Namespaced(nameof(SearchLocalPort)),
        "Search Local Port",
        "Filter items by local port when searching.",
        true);

    private readonly ToggleSetting _searchRemoteAddress = new(
        Namespaced(nameof(SearchRemoteAddress)),
        "Search Remote Address",
        "Filter items by remote address when searching.",
        true);

    private readonly ToggleSetting _searchRemotePort = new(
        Namespaced(nameof(SearchRemotePort)),
        "Search Remote Port",
        "Filter items by remote port when searching.",
        true);

    public SortBy SortBy
    {
        get
        {
            if (Enum.TryParse<SortBy>(_sortBy.Value, out var result))
            {
                return result;
            }
            return SortBy.LocalPort;
        }
    }

    public bool EnableLogging => _enableLogging.Value;

    public int PageSize
    {
        get
        {
            if (int.TryParse(_pageSize.Value, out int size) && size > 0)
            {
                return size;
            }
            return 16;
        }
    }

    public int PollingIntervalMilliseconds
    {
        get
        {
            if (int.TryParse(_pollingIntervalMilliseconds.Value, out int interval) && interval >= 0)
            {
                return interval;
            }
            return 1000;
        }
    }

    public bool ShowDetails => _showDetails.Value;
    public bool SearchProcessName => _searchProcessName.Value;
    public bool SearchLocalAddress => _searchLocalAddress.Value;
    public bool SearchLocalPort => _searchLocalPort.Value;
    public bool SearchRemoteAddress => _searchRemoteAddress.Value;
    public bool SearchRemotePort => _searchRemotePort.Value;

    internal static string SettingsJsonPath()
    {
        try
        {
            var directory = Utilities.BaseSettingsPath(Constant.AppName);
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, "settings.json");
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            throw;
        }
    }

    public SettingsManager()
    {
        try
        {
            FilePath = SettingsJsonPath();

            Settings.Add(_pageSize);
            Settings.Add(_pollingIntervalMilliseconds);
            Settings.Add(_sortBy);
            Settings.Add(_showDetails);
            Settings.Add(_searchProcessName);
            Settings.Add(_searchLocalAddress);
            Settings.Add(_searchLocalPort);
            Settings.Add(_searchRemoteAddress);
            Settings.Add(_searchRemotePort);
#if DEBUG
            Settings.Add(_enableLogging);
#endif
            LoadSettings();

            Settings.SettingsChanged += (s, a) =>
            {
                SaveSettings();
            };
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
        }
    }
}