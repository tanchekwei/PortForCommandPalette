using System;
using System.Diagnostics;
using System.IO;
using Microsoft.CommandPalette.Extensions.Toolkit;
using PortForCommandPalette.Classes;

namespace PortForCommandPalette.Commands;

public partial class OpenProcessLocationCommand : InvokableCommand
{
    private readonly string _path;
    public OpenProcessLocationCommand(string path)
    {
        _path = path;
        Icon = Classes.Icon.FileExplorer;
    }

    public override CommandResult Invoke()
    {
        try
        {
            if (File.Exists(_path))
            {
                Process.Start("explorer.exe", $"/select,\"{_path}\"");
            }
            return CommandResult.Dismiss();
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return CommandResult.KeepOpen();
        }
    }
}