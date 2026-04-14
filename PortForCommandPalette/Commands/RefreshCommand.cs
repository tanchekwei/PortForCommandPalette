// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using PortForCommandPalette.Classes;

namespace PortForCommandPalette.Commands;

public sealed partial class RefreshCommand : InvokableCommand
{
    public override string Name => "Refresh";
    private readonly SettingsManager _settingsManager;
    public event EventHandler? TriggerRefresh;

    public RefreshCommand(SettingsManager settingsManager)
    {
        try
        {
            Icon = new IconInfo("\xE72C"); // Refresh icon
            _settingsManager = settingsManager;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            throw;
        }
    }

    public override CommandResult Invoke()
    {
        try
        {
            TriggerRefresh?.Invoke(this, EventArgs.Empty);
            return CommandResult.KeepOpen();
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return CommandResult.KeepOpen();
        }
    }
}
