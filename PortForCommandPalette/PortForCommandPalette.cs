// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CommandPalette.Extensions;
using Microsoft.Extensions.DependencyInjection;
using PortForCommandPalette.Classes;
using PortForCommandPalette.Commands;
using PortForCommandPalette.Listeners;
using PortForCommandPalette.Pages;
using PortForCommandPalette.Services;

namespace PortForCommandPalette;

#if DEBUG
[Guid("280ba87a-8349-422c-9dc1-53adef99e402")]
#else
    [Guid("53203121-cb21-4661-baa4-c6659f76218f")]
#endif
public sealed partial class PortForCommandPalette : IExtension, IDisposable
{
    private readonly ManualResetEvent _extensionDisposedEvent;

    private readonly PortForCommandPaletteCommandsProvider _provider;

    public PortForCommandPalette(ManualResetEvent extensionDisposedEvent)
    {
        try
        {
            this._extensionDisposedEvent = extensionDisposedEvent;
            var services = new ServiceCollection();

            services.AddSingleton<SettingsManager>();
            services.AddSingleton<SettingsListener>();
            services.AddSingleton<PortsPage>();
            services.AddSingleton<PortService>();
            services.AddSingleton<RefreshCommand>();
            services.AddSingleton<Dependencies>();
            services.AddSingleton<PortForCommandPaletteCommandsProvider>();

            var provider = services.BuildServiceProvider();

            StaticHelpItems.Initialize(provider.GetRequiredService<Dependencies>());

            _provider = provider.GetRequiredService<PortForCommandPaletteCommandsProvider>();
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            throw;
        }
    }

    public object? GetProvider(ProviderType providerType)
    {
        try
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            return providerType switch
            {
                ProviderType.Commands => _provider,
                _ => null,
            };
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return null;
        }
    }

    public void Dispose()
    {
        try
        {
            this._extensionDisposedEvent.Set();
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
        }
    }
}
