// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using PortForCommandPalette.Commands;
using PortForCommandPalette.Enums;
using PortForCommandPalette.Pages;

namespace PortForCommandPalette.Classes;

public static class PortFilter
{
    internal static bool MatchesFilter(PortInfo port, FilterType filterType)
    {
        return filterType switch
        {
            FilterType.TCPv4 => string.Equals(port.Protocol, nameof(FilterType.TCPv4), StringComparison.OrdinalIgnoreCase),
            FilterType.TCPv6 => string.Equals(port.Protocol, nameof(FilterType.TCPv6), StringComparison.OrdinalIgnoreCase),
            FilterType.TCP => string.Equals(port.Protocol, nameof(FilterType.TCPv4), StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(port.Protocol, nameof(FilterType.TCPv6), StringComparison.OrdinalIgnoreCase),
            FilterType.UDPv4 => string.Equals(port.Protocol, nameof(FilterType.UDPv4), StringComparison.OrdinalIgnoreCase),
            FilterType.UDPv6 => string.Equals(port.Protocol, nameof(FilterType.UDPv6), StringComparison.OrdinalIgnoreCase),
            FilterType.UDP => string.Equals(port.Protocol, nameof(FilterType.UDPv4), StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(port.Protocol, nameof(FilterType.UDPv6), StringComparison.OrdinalIgnoreCase),
            FilterType.Listen => string.Equals(port.State, nameof(FilterType.Listen), StringComparison.OrdinalIgnoreCase),
            FilterType.Established => string.Equals(port.State, nameof(FilterType.Established), StringComparison.OrdinalIgnoreCase),
            FilterType.TimeWait => string.Equals(port.State, nameof(FilterType.TimeWait), StringComparison.OrdinalIgnoreCase),
            FilterType.CloseWait => string.Equals(port.State, nameof(FilterType.CloseWait), StringComparison.OrdinalIgnoreCase),
            FilterType.Other => !string.Equals(port.State, nameof(FilterType.Listen), StringComparison.OrdinalIgnoreCase) &&
                               !string.Equals(port.State, nameof(FilterType.Established), StringComparison.OrdinalIgnoreCase) &&
                               !string.Equals(port.State, nameof(FilterType.TimeWait), StringComparison.OrdinalIgnoreCase) &&
                               !string.Equals(port.State, nameof(FilterType.CloseWait), StringComparison.OrdinalIgnoreCase),
            _ => true,
        };
    }

    internal static int CalculateSearchScore(PortInfo item, string searchText, SettingsManager settingsManager)
    {
        if (string.IsNullOrWhiteSpace(searchText)) return 0;

        int bestScore = 0;
        if (settingsManager.SearchProcessName)
        {
            var score = FuzzyStringMatcher.ScoreFuzzy(searchText, item.ProcessName ?? string.Empty);
            bestScore = Math.Max(bestScore, score);
        }

        if (settingsManager.SearchLocalAddress)
        {
            bestScore = Math.Max(bestScore, ScoreSubstring(item.LocalAddress, searchText));
        }
        if (settingsManager.SearchLocalPort)
        {
            bestScore = Math.Max(bestScore, ScoreSubstring(item.LocalPort.ToString(CultureInfo.InvariantCulture), searchText));
        }
        if (settingsManager.SearchLocalAddress && settingsManager.SearchLocalPort)
        {
            bestScore = Math.Max(bestScore, ScoreSubstring(item.LocalAddressDisplay, searchText));
        }

        if (settingsManager.SearchRemoteAddress)
        {
            bestScore = Math.Max(bestScore, ScoreSubstring(item.RemoteAddress, searchText));
        }
        if (settingsManager.SearchRemotePort)
        {
            bestScore = Math.Max(bestScore, ScoreSubstring(item.RemotePort.ToString(CultureInfo.InvariantCulture), searchText));
        }
        if (settingsManager.SearchRemoteAddress && settingsManager.SearchRemotePort)
        {
            bestScore = Math.Max(bestScore, ScoreSubstring(item.RemoteAddressDisplay, searchText));
        }

        return bestScore;
    }

    private static int ScoreSubstring(string text, string query)
    {
        if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(text)) return 0;
        if (text.Equals(query, StringComparison.OrdinalIgnoreCase)) return 100;
        if (text.StartsWith(query, StringComparison.OrdinalIgnoreCase)) return 95;
        if (text.Contains(query, StringComparison.OrdinalIgnoreCase)) return 80;
        return 0;
    }

    internal static List<PortInfo> ApplyFilterAndSort(
        List<PortInfo> allPorts,
        string searchText,
        FilterType filterType,
        SettingsManager settingsManager,
        Action<List<PortInfo>> resolveNamesAction)
    {
        var isSearching = !string.IsNullOrWhiteSpace(searchText);
        if (isSearching)
        {
            resolveNamesAction(allPorts);
        }

        var hasFilter = filterType != FilterType.All;
        var filteredResult = new List<PortInfo>();

        if (isSearching)
        {
            var matchedItems = new List<(PortInfo item, int score)>();
            for (int i = 0; i < allPorts.Count; i++)
            {
                var item = allPorts[i];
                if (hasFilter && !MatchesFilter(item, filterType))
                {
                    continue;
                }

                var score = CalculateSearchScore(item, searchText, settingsManager);
                if (score > 0)
                {
                    matchedItems.Add((item, score));
                }
            }

            matchedItems.Sort((a, b) => b.score.CompareTo(a.score));
            for (int i = 0; i < matchedItems.Count; i++)
            {
                filteredResult.Add(matchedItems[i].item);
            }
        }
        else
        {
            for (int i = 0; i < allPorts.Count; i++)
            {
                var item = allPorts[i];
                if (hasFilter && !MatchesFilter(item, filterType))
                {
                    continue;
                }
                filteredResult.Add(item);
            }

            if (settingsManager.SortBy == SortBy.ProcessName)
            {
                resolveNamesAction(filteredResult);
            }

            SortPorts(filteredResult, settingsManager.SortBy);
        }
        return filteredResult;
    }

    internal static void SortPorts(List<PortInfo> ports, SortBy sortBy)
    {
        ports.Sort((a, b) =>
        {
            int res = sortBy switch
            {
                SortBy.ProcessName => string.Compare(a.ProcessName, b.ProcessName, StringComparison.OrdinalIgnoreCase),
                SortBy.Protocol => string.Compare(a.Protocol, b.Protocol, StringComparison.OrdinalIgnoreCase),
                SortBy.State => string.Compare(a.State, b.State, StringComparison.OrdinalIgnoreCase),
                _ => a.LocalPort.CompareTo(b.LocalPort), // Default to LocalPort
            };

            if (res == 0 && sortBy != SortBy.LocalPort)
            {
                res = a.LocalPort.CompareTo(b.LocalPort);
            }
            return res;
        });
    }

    internal static string GetSearchPlaceholder(SettingsManager settingsManager)
    {
        var enabledFields = new List<string>();
        if (settingsManager.SearchProcessName) enabledFields.Add("process");
        if (settingsManager.SearchLocalAddress || settingsManager.SearchRemoteAddress) enabledFields.Add("address");
        if (settingsManager.SearchLocalPort || settingsManager.SearchRemotePort) enabledFields.Add("port");

        if (enabledFields.Count > 0)
        {
            return $"Search {string.Join(", ", enabledFields)}...";
        }
        else
        {
            return "Search disabled (enable in settings)";
        }
    }

    internal static ListItem CreateTopLevelListItem(ListItem listItem)
    {
        var newMoreCommands = new List<ICommandContextItem>();
        if (listItem.MoreCommands != null)
        {
            foreach (var mc in listItem.MoreCommands)
            {
                if (mc is CommandContextItem cci && !(cci.Command is HelpPage || cci.Command is RefreshCommand))
                {
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
}