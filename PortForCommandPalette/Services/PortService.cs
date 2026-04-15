using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using PortForCommandPalette.Classes;
using PortForCommandPalette.Enums;
using PortForCommandPalette.Helpers;

namespace PortForCommandPalette.Services;

public class PortService
{
    private const int AF_INET = 2;
    private const int AF_INET6 = 23;
    private const uint ERROR_INSUFFICIENT_BUFFER = 122;
    private const uint NO_ERROR = 0;

    private readonly Dictionary<int, (string Name, string Path)> _globalProcessCache = [];
    private readonly object _cacheLock = new();

    public List<PortInfo> GetPorts(FilterType filter = FilterType.All)
    {
        var ports = new List<PortInfo>();

        bool isAll = filter == FilterType.All;
        bool hasTcpState = (filter & (FilterType.Listen | FilterType.Established | FilterType.TimeWait | FilterType.CloseWait | FilterType.Other)) != 0;

        if (isAll || (filter & FilterType.TCPv4) != 0 || hasTcpState)
        {
            ports.AddRange(GetTcpPorts());
        }

        if (isAll || (filter & FilterType.TCPv6) != 0 || hasTcpState)
        {
            ports.AddRange(GetTcp6Ports());
        }

        if (isAll || (filter & FilterType.UDPv4) != 0)
        {
            ports.AddRange(GetUdpPorts());
        }

        if (isAll || (filter & FilterType.UDPv6) != 0)
        {
            ports.AddRange(GetUdp6Ports());
        }

        return ports;
    }

    public void ResolveProcessNames(IEnumerable<PortInfo> ports)
    {
        var needsResolution = new List<PortInfo>();
        lock (_cacheLock)
        {
            foreach (var p in ports)
            {
                if (_globalProcessCache.TryGetValue(p.ProcessId, out var cachedInfo) && !string.IsNullOrEmpty(cachedInfo.Name))
                {
                    p.ProcessName = cachedInfo.Name;
                    p.ProcessPath = cachedInfo.Path;
                }
                else if (string.IsNullOrEmpty(p.ProcessName) || p.ProcessName == "Unknown")
                {
                    needsResolution.Add(p);
                }
            }
        }

        if (needsResolution.Count == 0) return;

        var allProcessNames = new Dictionary<int, string>();
        foreach (var p in Process.GetProcesses())
        {
            allProcessNames[p.Id] = p.ProcessName;
        }

        lock (_cacheLock)
        {
            foreach (var port in needsResolution)
            {
                if (port.ProcessId == 0) port.ProcessName = "System Idle Process";
                else if (port.ProcessId == 4) port.ProcessName = "System";
                else if (allProcessNames.TryGetValue(port.ProcessId, out var name)) port.ProcessName = name;
                else port.ProcessName = "Unknown";

                if (!_globalProcessCache.TryGetValue(port.ProcessId, out var existing) || string.IsNullOrEmpty(existing.Name))
                {
                    _globalProcessCache[port.ProcessId] = (port.ProcessName, existing.Path ?? string.Empty);
                }
            }
        }
    }

    public void ResolveProcessInfo(IEnumerable<PortInfo> ports)
    {
        var portsList = new List<PortInfo>(ports);
        if (portsList.Count == 0) return;

        // Ensure names are resolved first
        ResolveProcessNames(portsList);

        for (int i = 0; i < portsList.Count; i++)
        {
            var port = portsList[i];
            if (port.ProcessId <= 4) continue;
            if (!string.IsNullOrEmpty(port.ProcessPath)) continue;

            string? path = null;
            bool foundInCache = false;

            lock (_cacheLock)
            {
                if (_globalProcessCache.TryGetValue(port.ProcessId, out var cachedInfo) && !string.IsNullOrEmpty(cachedInfo.Path))
                {
                    path = cachedInfo.Path;
                    foundInCache = true;
                }
            }

            if (foundInCache)
            {
                port.ProcessPath = path ?? string.Empty;
                continue;
            }

            path = GetProcessPath(port.ProcessId);
            port.ProcessPath = path;

            lock (_cacheLock)
            {
                _globalProcessCache[port.ProcessId] = (port.ProcessName, port.ProcessPath);
            }
        }
    }

    private string GetProcessPath(int pid)
    {
        IntPtr hProcess = NativeMethods.OpenProcess(NativeMethods.PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
        if (hProcess != IntPtr.Zero)
        {
            try
            {
                var buffer = new StringBuilder(260);
                int size = buffer.Capacity;
                if (NativeMethods.QueryFullProcessImageName(hProcess, 0, buffer, ref size))
                {
                    return buffer.ToString();
                }
            }
            finally
            {
                NativeMethods.CloseHandle(hProcess);
            }
        }

        // Fallback to older method if QueryFullProcessImageName fails or for compatibility, 
        // though limited information should usually work.
        try
        {
            using var process = Process.GetProcessById(pid);
            return process.MainModule?.FileName ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private List<PortInfo> GetTcpPorts()
    {
        var results = new List<PortInfo>();
        int bufferSize = 0;
        uint ret = NativeMethods.GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);
        
        IntPtr tcpTablePtr = IntPtr.Zero;
        try
        {
            while (ret == ERROR_INSUFFICIENT_BUFFER || tcpTablePtr == IntPtr.Zero)
            {
                if (tcpTablePtr != IntPtr.Zero) Marshal.FreeHGlobal(tcpTablePtr);
                tcpTablePtr = Marshal.AllocHGlobal(bufferSize);
                ret = NativeMethods.GetExtendedTcpTable(tcpTablePtr, ref bufferSize, true, AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);
                if (ret == NO_ERROR) break;
                if (ret != ERROR_INSUFFICIENT_BUFFER) return results;
            }

            if (ret == NO_ERROR)
            {
                int rowCount = Marshal.ReadInt32(tcpTablePtr);
                IntPtr rowPtr = tcpTablePtr + 4;
                int rowSize = Marshal.SizeOf<MIB_TCPROW_OWNER_PID>();

                for (int i = 0; i < rowCount; i++)
                {
                    var row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);
                    results.Add(new PortInfo
                    {
                        Protocol = nameof(FilterType.TCPv4),
                        LocalAddress = new IPAddress(row.localAddr).ToString(),
                        LocalPort = row.LocalPort,
                        RemoteAddress = new IPAddress(row.remoteAddr).ToString(),
                        RemotePort = row.RemotePort,
                        State = ((TcpState)row.state).ToString(),
                        ProcessId = (int)row.owningPid,
                        ProcessName = string.Empty,
                        ProcessPath = string.Empty
                    });
                    rowPtr += rowSize;
                }
            }
        }
        finally
        {
            if (tcpTablePtr != IntPtr.Zero) Marshal.FreeHGlobal(tcpTablePtr);
        }
        return results;
    }

    private List<PortInfo> GetTcp6Ports()
    {
        var results = new List<PortInfo>();
        int bufferSize = 0;
        uint ret = NativeMethods.GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET6, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);
        
        IntPtr tcpTablePtr = IntPtr.Zero;
        try
        {
            while (ret == ERROR_INSUFFICIENT_BUFFER || tcpTablePtr == IntPtr.Zero)
            {
                if (tcpTablePtr != IntPtr.Zero) Marshal.FreeHGlobal(tcpTablePtr);
                tcpTablePtr = Marshal.AllocHGlobal(bufferSize);
                ret = NativeMethods.GetExtendedTcpTable(tcpTablePtr, ref bufferSize, true, AF_INET6, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);
                if (ret == NO_ERROR) break;
                if (ret != ERROR_INSUFFICIENT_BUFFER) return results;
            }

            if (ret == NO_ERROR)
            {
                int rowCount = Marshal.ReadInt32(tcpTablePtr);
                IntPtr rowPtr = tcpTablePtr + 4;
                int rowSize = Marshal.SizeOf<MIB_TCP6ROW_OWNER_PID>();

                for (int i = 0; i < rowCount; i++)
                {
                    var row = Marshal.PtrToStructure<MIB_TCP6ROW_OWNER_PID>(rowPtr);
                    results.Add(new PortInfo
                    {
                        Protocol = nameof(FilterType.TCPv6),
                        LocalAddress = new IPAddress(row.localAddr).ToString(),
                        LocalPort = row.LocalPort,
                        RemoteAddress = new IPAddress(row.remoteAddr).ToString(),
                        RemotePort = row.RemotePort,
                        State = ((TcpState)row.state).ToString(),
                        ProcessId = (int)row.owningPid,
                        ProcessName = string.Empty,
                        ProcessPath = string.Empty
                    });
                    rowPtr += rowSize;
                }
            }
        }
        finally
        {
            if (tcpTablePtr != IntPtr.Zero) Marshal.FreeHGlobal(tcpTablePtr);
        }
        return results;
    }

    private List<PortInfo> GetUdpPorts()
    {
        var results = new List<PortInfo>();
        int bufferSize = 0;
        uint ret = NativeMethods.GetExtendedUdpTable(IntPtr.Zero, ref bufferSize, true, AF_INET, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID);
        
        IntPtr udpTablePtr = IntPtr.Zero;
        try
        {
            while (ret == ERROR_INSUFFICIENT_BUFFER || udpTablePtr == IntPtr.Zero)
            {
                if (udpTablePtr != IntPtr.Zero) Marshal.FreeHGlobal(udpTablePtr);
                udpTablePtr = Marshal.AllocHGlobal(bufferSize);
                ret = NativeMethods.GetExtendedUdpTable(udpTablePtr, ref bufferSize, true, AF_INET, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID);
                if (ret == NO_ERROR) break;
                if (ret != ERROR_INSUFFICIENT_BUFFER) return results;
            }

            if (ret == NO_ERROR)
            {
                int rowCount = Marshal.ReadInt32(udpTablePtr);
                IntPtr rowPtr = udpTablePtr + 4;
                int rowSize = Marshal.SizeOf<MIB_UDPROW_OWNER_PID>();

                for (int i = 0; i < rowCount; i++)
                {
                    var row = Marshal.PtrToStructure<MIB_UDPROW_OWNER_PID>(rowPtr);
                    results.Add(new PortInfo
                    {
                        Protocol = nameof(FilterType.UDPv4),
                        LocalAddress = new IPAddress(row.localAddr).ToString(),
                        LocalPort = row.LocalPort,
                        State = string.Empty,
                        ProcessId = (int)row.owningPid,
                        ProcessName = string.Empty,
                        ProcessPath = string.Empty
                    });
                    rowPtr += rowSize;
                }
            }
        }
        finally
        {
            if (udpTablePtr != IntPtr.Zero) Marshal.FreeHGlobal(udpTablePtr);
        }
        return results;
    }

    private List<PortInfo> GetUdp6Ports()
    {
        var results = new List<PortInfo>();
        int bufferSize = 0;
        uint ret = NativeMethods.GetExtendedUdpTable(IntPtr.Zero, ref bufferSize, true, AF_INET6, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID);
        
        IntPtr udpTablePtr = IntPtr.Zero;
        try
        {
            while (ret == ERROR_INSUFFICIENT_BUFFER || udpTablePtr == IntPtr.Zero)
            {
                if (udpTablePtr != IntPtr.Zero) Marshal.FreeHGlobal(udpTablePtr);
                udpTablePtr = Marshal.AllocHGlobal(bufferSize);
                ret = NativeMethods.GetExtendedUdpTable(udpTablePtr, ref bufferSize, true, AF_INET6, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID);
                if (ret == NO_ERROR) break;
                if (ret != ERROR_INSUFFICIENT_BUFFER) return results;
            }

            if (ret == NO_ERROR)
            {
                int rowCount = Marshal.ReadInt32(udpTablePtr);
                IntPtr rowPtr = udpTablePtr + 4;
                int rowSize = Marshal.SizeOf<MIB_UDP6ROW_OWNER_PID>();

                for (int i = 0; i < rowCount; i++)
                {
                    var row = Marshal.PtrToStructure<MIB_UDP6ROW_OWNER_PID>(rowPtr);
                    results.Add(new PortInfo
                    {
                        Protocol = nameof(FilterType.UDPv6),
                        LocalAddress = new IPAddress(row.localAddr).ToString(),
                        LocalPort = row.LocalPort,
                        State = string.Empty,
                        ProcessId = (int)row.owningPid,
                        ProcessName = string.Empty,
                        ProcessPath = string.Empty
                    });
                    rowPtr += rowSize;
                }
            }
        }
        finally
        {
            if (udpTablePtr != IntPtr.Zero) Marshal.FreeHGlobal(udpTablePtr);
        }
        return results;
    }
}