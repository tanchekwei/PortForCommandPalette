using System;
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
}
