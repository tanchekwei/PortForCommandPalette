// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using PortForCommandPalette.Classes;
using PortForCommandPalette.Enums;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer;

internal sealed partial class SearchFilters : Filters
{
    public SearchFilters()
    {
        CurrentFilterId = nameof(FilterType.All);
    }

    public override IFilterItem[] GetFilters()
    {
        return [
            new Filter() { Id = nameof(FilterType.All), Name = "All", Icon = Icon.FilterIcon },
            new Separator("Protocol"),
            new Filter() { Id = nameof(FilterType.TCP), Name = "TCP", Icon = Icon.FilterIcon },
            new Filter() { Id = nameof(FilterType.TCPv4), Name = nameof(FilterType.TCPv4), Icon = Icon.FilterIcon },
            new Filter() { Id = nameof(FilterType.TCPv6), Name = nameof(FilterType.TCPv6), Icon = Icon.FilterIcon },
            new Filter() { Id = nameof(FilterType.UDP), Name = "UDP", Icon = Icon.FilterIcon },
            new Filter() { Id = nameof(FilterType.UDPv4), Name = nameof(FilterType.UDPv4), Icon = Icon.FilterIcon },
            new Filter() { Id = nameof(FilterType.UDPv6), Name = nameof(FilterType.UDPv6), Icon = Icon.FilterIcon },
            new Separator("State"),
            new Filter() { Id = nameof(FilterType.Listen), Name = nameof(FilterType.Listen), Icon = Icon.Listen },
            new Filter() { Id = nameof(FilterType.Established), Name = nameof(FilterType.Established), Icon = Icon.Established },
            new Filter() { Id = nameof(FilterType.TimeWait), Name = "Time Wait", Icon = Icon.TimeWait },
            new Filter() { Id = nameof(FilterType.CloseWait), Name = "Close Wait", Icon = Icon.CloseWait },
            new Filter() { Id = nameof(FilterType.Other), Name = "Other", Icon = Icon.Web },
        ];
    }
}