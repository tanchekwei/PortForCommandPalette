// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace PortForCommandPalette.Classes;

public static class Constant
{
#if DEBUG
    public const string AppName = "PortForCommandPaletteDev";
#else
    public const string AppName = "PortForCommandPalette";
#endif
#if DEBUG
    public const string DisplayName = "Port (Dev)";
#else
    public const string DisplayName = "Port";
#endif
#if DEBUG
    public const string PageName = "View Ports (Dev)";
#else
    public const string PageName = "View Ports";
#endif
    public static readonly string AssemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
    public static readonly string SettingsFolderPath = Utilities.BaseSettingsPath(AppName);
}
