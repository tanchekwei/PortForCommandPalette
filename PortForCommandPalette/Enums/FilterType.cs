// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;

namespace PortForCommandPalette.Enums
{
    [Flags]
    public enum FilterType
    {
        All = 0,
        TCPv4 = 1 << 0,
        TCPv6 = 1 << 1,
        TCP = TCPv4 | TCPv6,
        UDPv4 = 1 << 2,
        UDPv6 = 1 << 3,
        UDP = UDPv4 | UDPv6,
        Listen = 1 << 4,
        Established = 1 << 5,
        TimeWait = 1 << 6,
        CloseWait = 1 << 7,
        Other = 1 << 8
    }
}