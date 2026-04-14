// Copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using PortForCommandPalette.Classes;
using PortForCommandPalette.Commands;

namespace PortForCommandPalette.Pages
{
    public static class StaticHelpItems
    {
        public static readonly ListItem OpenSettingsFolder = new(
            new OpenInExplorerCommand(Constant.SettingsFolderPath, "Open extension settings / logs folder")
        );

        public static readonly ListItem ViewSource = new(
            new Commands.OpenUrlCommand("https://github.com/tanchekwei/PortForCommandPalette", "View source code", Icon.GitHub)
        );

        public static readonly ListItem ReportBug = new(
            new Commands.OpenUrlCommand("https://github.com/tanchekwei/PortForCommandPalette/issues/new", "Report issue", Icon.GitHub)
        );

        public static readonly ListItem ExtensionVersion = new()
        {
            Title = Constant.AssemblyVersion,
            Subtitle = "Extension Version",
            Icon = Icon.Extension
        };

        public static ListItem SettingsItem { get; private set; } = null!;

        public static void Initialize(Dependencies deps)
        {
            try
            {
                SettingsItem = new ListItem(deps.Get<SettingsManager>().Settings.SettingsPage)
                {
                    Title = "Setting",
                    Icon = Icon.Setting
                };
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }
    }
}
