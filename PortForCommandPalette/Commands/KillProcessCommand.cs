using System;
using System.Diagnostics;
using Microsoft.CommandPalette.Extensions.Toolkit;
using PortForCommandPalette.Classes;

namespace PortForCommandPalette.Commands;

public partial class KillProcessCommand : InvokableCommand
{
    private readonly int _pid;
    public KillProcessCommand(int pid)
    {
        _pid = pid;
        Name = "Kill Process";
        Icon = Classes.Icon.CloseWait;
    }

    public override CommandResult Invoke()
    {
        try
        {
            var process = Process.GetProcessById(_pid);
            process.Kill();
            return CommandResult.KeepOpen();
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            new ErrorCommandPaletteMessage(ex.Message).Show();
            return CommandResult.KeepOpen();
        }
    }
}