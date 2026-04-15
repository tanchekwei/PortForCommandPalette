// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Globalization;
using Microsoft.CommandPalette.Extensions.Toolkit;
using PortForCommandPalette.Enums;

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
}