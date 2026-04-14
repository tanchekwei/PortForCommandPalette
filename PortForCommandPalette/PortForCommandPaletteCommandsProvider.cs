// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using PortForCommandPalette.Classes;
using PortForCommandPalette.Commands;
using PortForCommandPalette.Listeners;
using Windows.ApplicationModel;

namespace PortForCommandPalette;

public partial class PortForCommandPaletteCommandsProvider : CommandProvider
{
    private readonly SettingsManager _settingsManager;
    private readonly SettingsListener _settingsListener;
    private readonly PortsPage _page;
    private readonly IFallbackCommandItem[] _fallbacks;
    public PortForCommandPaletteCommandsProvider(
        SettingsManager settingsManager,
        SettingsListener settingsListener,
        PortsPage page,
        RefreshCommand refreshCommand)
    {
        try
        {
            Id = Package.Current.Id.Name;
#if DEBUG
            using var logger = new TimeLogger();
#endif
            _settingsManager = settingsManager;
            DisplayName = Constant.DisplayName;
            Icon = Classes.Icon.Extension;
            Settings = _settingsManager.Settings;

            _settingsListener = settingsListener;

            _page = page;

            _fallbacks = [];
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            _settingsManager = null!;
            _settingsListener = null!;
            _page = null!;
            _fallbacks = null!;
            throw;
        }
    }

    public override ICommandItem[] TopLevelCommands()
    {
        try
        {
            return [
                new CommandItem(_page) {
                    MoreCommands = [
                        new CommandContextItem(Settings!.SettingsPage),
                    ],
                },
            ];
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return [];
        }
    }
    public override IFallbackCommandItem[] FallbackCommands() => _fallbacks;

    public override ICommandItem? GetCommandItem(string id)
    {
        return _page.GetCommandItem(id);
    }
}
