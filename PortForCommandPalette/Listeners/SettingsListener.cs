// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using PortForCommandPalette.Classes;
using PortForCommandPalette.Enums;

namespace PortForCommandPalette.Listeners
{
    public class SettingsListener
    {
        private readonly SettingsManager? _settingsManager;
        private SortBy _previousSortBy;
        private int _previousPollingInterval;
        private bool _previousShowDetails;
        private bool _previousShowProtocolTag;
        private bool _previousShowStateTag;
        private bool _previousSearchProcessName;
        private bool _previousSearchLocalAddress;
        private bool _previousSearchLocalPort;
        private bool _previousSearchRemoteAddress;
        private bool _previousSearchRemotePort;

        public event EventHandler? PageSettingsChanged;
        public event EventHandler? SortSettingsChanged;
        public event EventHandler? PollingIntervalChanged;

        public SettingsListener(SettingsManager settingsManager)
        {
            try
            {
                _settingsManager = settingsManager;
                _previousSortBy = _settingsManager.SortBy;
                _previousPollingInterval = _settingsManager.PollingIntervalMilliseconds;
                _previousShowDetails = _settingsManager.ShowDetails;
                _previousShowProtocolTag = _settingsManager.ShowProtocolTag;
                _previousShowStateTag = _settingsManager.ShowStateTag;
                _previousSearchProcessName = _settingsManager.SearchProcessName;
                _previousSearchLocalAddress = _settingsManager.SearchLocalAddress;
                _previousSearchLocalPort = _settingsManager.SearchLocalPort;
                _previousSearchRemoteAddress = _settingsManager.SearchRemoteAddress;
                _previousSearchRemotePort = _settingsManager.SearchRemotePort;
                _settingsManager.Settings.SettingsChanged += OnSettingsChanged;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }

        private void OnSettingsChanged(object? sender, Settings e)
        {
            try
            {
                if (_settingsManager == null)
                {
                    return;
                }

                var currentSortBy = _settingsManager.SortBy;
                if (currentSortBy != _previousSortBy)
                {
                    SortSettingsChanged?.Invoke(this, EventArgs.Empty);
                    _previousSortBy = currentSortBy;
                }

                var currentPollingInterval = _settingsManager.PollingIntervalMilliseconds;
                if (currentPollingInterval != _previousPollingInterval)
                {
                    PollingIntervalChanged?.Invoke(this, EventArgs.Empty);
                    _previousPollingInterval = currentPollingInterval;
                }

                var currentShowDetails = _settingsManager.ShowDetails;
                var currentShowProtocolTag = _settingsManager.ShowProtocolTag;
                var currentShowStateTag = _settingsManager.ShowStateTag;
                var currentSearchProcessName = _settingsManager.SearchProcessName;
                var currentSearchLocalAddress = _settingsManager.SearchLocalAddress;
                var currentSearchLocalPort = _settingsManager.SearchLocalPort;
                var currentSearchRemoteAddress = _settingsManager.SearchRemoteAddress;
                var currentSearchRemotePort = _settingsManager.SearchRemotePort;

                if (currentShowDetails != _previousShowDetails ||
                    currentShowProtocolTag != _previousShowProtocolTag ||
                    currentShowStateTag != _previousShowStateTag ||
                    currentSearchProcessName != _previousSearchProcessName ||
                    currentSearchLocalAddress != _previousSearchLocalAddress ||
                    currentSearchLocalPort != _previousSearchLocalPort ||
                    currentSearchRemoteAddress != _previousSearchRemoteAddress ||
                    currentSearchRemotePort != _previousSearchRemotePort)
                {
                    PageSettingsChanged?.Invoke(this, EventArgs.Empty);
                    _previousShowDetails = currentShowDetails;
                    _previousShowProtocolTag = currentShowProtocolTag;
                    _previousShowStateTag = currentShowStateTag;
                    _previousSearchProcessName = currentSearchProcessName;
                    _previousSearchLocalAddress = currentSearchLocalAddress;
                    _previousSearchLocalPort = currentSearchLocalPort;
                    _previousSearchRemoteAddress = currentSearchRemoteAddress;
                    _previousSearchRemotePort = currentSearchRemotePort;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }
    }
}