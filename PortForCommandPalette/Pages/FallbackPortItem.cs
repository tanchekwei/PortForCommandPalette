// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.ApplicationModel;

namespace PortForCommandPalette.Pages;

internal sealed partial class FallbackPortItem : FallbackCommandItem
{
    private readonly PortsPage _page;
    private readonly NoOpCommand _emptyCommand = new NoOpCommand();

    public FallbackPortItem(
        PortsPage page
    )
        : base(new NoOpCommand(), string.Empty, $"{Package.Current.Id.Name}.{nameof(FallbackPortItem)}")
    {
        _page = page;
        Title = string.Empty;
        Subtitle = string.Empty;
        Icon = Classes.Icon.Extension;
    }

    public override void UpdateQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || !int.TryParse(query, out var portNumber) || portNumber < 0 || portNumber > 65535)
        {
            Command = _emptyCommand;
            Title = string.Empty;
            Subtitle = string.Empty;
            return;
        }

        _page.SetSearchText(query);
        Title = $"Search for Port \"{query}\"";
        Icon = Classes.Icon.Extension;
        Command = _page;
    }
}