using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using PortForCommandPalette.Classes;
using PortForCommandPalette.Commands;
using PortForCommandPalette.Enums;
using PortForCommandPalette.Helpers;
using PortForCommandPalette.Pages;
using Windows.System;

namespace PortForCommandPalette.Workspaces;

public static class PortItemFactory
{
    public static ListItem Create(PortInfo port, CommandContextItem refreshCommandContextItem, SettingsManager settingsManager)
    {
        try
        {
            var tags = BuildTags(port);
            var processIcon = GetIconForProcess(port.ProcessPath);
            var details = new Details()
            {
                Title = port.ProcessName,
                HeroImage = processIcon,
                Metadata = [
                    new DetailsElement() { Key = "Tags", Data = new DetailsTags() { Tags = [.. tags] } },
                    new DetailsElement() { Key = "Process", Data = new DetailsLink() { Text = port.ProcessName } },
                    new DetailsElement() { Key = "Process Path", Data = new DetailsLink() { Text = port.ProcessPath } },
                    new DetailsElement() { Key = "PID", Data = new DetailsLink() { Text = port.ProcessId.ToString(CultureInfo.InvariantCulture) } },
                    new DetailsElement() { Key = "Protocol", Data = new DetailsLink() { Text = port.Protocol } },
                    new DetailsElement() { Key = "Local Address", Data = new DetailsLink() { Text = port.LocalAddressDisplay } },
                    new DetailsElement() { Key = "Remote Address", Data = new DetailsLink() { Text = port.RemoteAddressDisplay } },
                    new DetailsElement() { Key = "State", Data = new DetailsLink() { Text = port.State } },
                ]
            };

            var moreCommands = new List<ICommandContextItem>();

            if (port.ProcessId > 4)
            {
                moreCommands.Add(new CommandContextItem(new KillProcessCommand(port.ProcessId)));
            }

            if (!string.IsNullOrEmpty(port.ProcessPath))
            {
                moreCommands.Add(new CommandContextItem(new OpenInExplorerCommand(port.ProcessPath)));
            }
            moreCommands.Add(new CommandContextItem(new HelpPage(settingsManager, port))
            {
                RequestedShortcut = KeyChordHelpers.FromModifiers(false, false, false, false, (int)VirtualKey.F1, 0),
            });
            moreCommands.Add(refreshCommandContextItem);

            var listItem = new ListItem(new NoOpCommand()
            {
                Id = port.Id,
                Icon = processIcon,
            })
            {
                Title = $"{port.LocalAddressDisplay} -> {port.RemoteAddressDisplay}",
                Subtitle = $"{port.ProcessName} ({port.ProcessId})",
                Icon = processIcon,
                Details = details,
                Tags = [.. tags],
                MoreCommands = moreCommands.ToArray()
            };
            return listItem;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static List<Tag> BuildTags(PortInfo port)
    {
        List<Tag> tags = [new Tag(port.Protocol)];
        if (!string.IsNullOrWhiteSpace(port.State))
        {
            tags.Add(new Tag(port.State));
        }
        return tags;
    }

    private static IconInfo? GetIconForProcess(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return Icon.Shield;
        }

        return IconExtractor.GetIconForPath(path);
    }

    public static IconInfo GetIconForState(string state)
    {
        if (string.IsNullOrEmpty(state)) return Icon.Web;

        return state switch
        {
            nameof(FilterType.Listen) => Icon.Listen,
            nameof(FilterType.Established) => Icon.Established,
            nameof(FilterType.TimeWait) => Icon.TimeWait,
            nameof(FilterType.CloseWait) => Icon.CloseWait,
            _ => Icon.Web,
        };
    }
}