// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable SA1649, CA1051, CA1707, CA1028, CA1714, CA1069, SA1402

namespace PortForCommandPalette.Helpers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore", Justification = "These are the names used by win32.")]
    internal static partial class NativeMethods
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        public static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TCP_TABLE_CLASS tableClass, uint reserved = 0);

        [DllImport("iphlpapi.dll", SetLastError = true)]
        public static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int dwOutBufLen, bool sort, int ipVersion, UDP_TABLE_CLASS tableClass, uint reserved = 0);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern int GetDIBits(IntPtr hdc, IntPtr hbm, uint uStartScan, uint cScanLines, [Out] byte[] lpvBits, ref BITMAPINFO lpbi, uint uUsage);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0;
        public const uint SHGFI_SMALLICON = 0x1;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x10;

        public const uint DIB_RGB_COLORS = 0;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ICONINFO
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            public uint bmiColors;
        }
    }

    internal enum TCP_TABLE_CLASS
    {
        TCP_TABLE_BASIC_LISTENER,
        TCP_TABLE_BASIC_CONNECTIONS,
        TCP_TABLE_BASIC_ALL,
        TCP_TABLE_OWNER_PID_LISTENER,
        TCP_TABLE_OWNER_PID_CONNECTIONS,
        TCP_TABLE_OWNER_PID_ALL,
        TCP_TABLE_OWNER_MODULE_LISTENER,
        TCP_TABLE_OWNER_MODULE_CONNECTIONS,
        TCP_TABLE_OWNER_MODULE_ALL
    }

    internal enum UDP_TABLE_CLASS
    {
        UDP_TABLE_BASIC,
        UDP_TABLE_OWNER_PID,
        UDP_TABLE_OWNER_MODULE
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MIB_TCPROW_OWNER_PID
    {
        public uint state;
        public uint localAddr;
        public byte localPort1;
        public byte localPort2;
        public byte localPort3;
        public byte localPort4;
        public uint remoteAddr;
        public byte remotePort1;
        public byte remotePort2;
        public byte remotePort3;
        public byte remotePort4;
        public uint owningPid;

        public int LocalPort => (localPort1 << 8) + localPort2;
        public int RemotePort => (remotePort1 << 8) + remotePort2;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MIB_TCP6ROW_OWNER_PID
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] localAddr;
        public uint localScopeId;
        public byte localPort1;
        public byte localPort2;
        public byte localPort3;
        public byte localPort4;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] remoteAddr;
        public uint remoteScopeId;
        public byte remotePort1;
        public byte remotePort2;
        public byte remotePort3;
        public byte remotePort4;
        public uint state;
        public uint owningPid;

        public int LocalPort => (localPort1 << 8) + localPort2;
        public int RemotePort => (remotePort1 << 8) + remotePort2;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MIB_UDPROW_OWNER_PID
    {
        public uint localAddr;
        public byte localPort1;
        public byte localPort2;
        public byte localPort3;
        public byte localPort4;
        public uint owningPid;

        public int LocalPort => (localPort1 << 8) + localPort2;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MIB_UDP6ROW_OWNER_PID
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] localAddr;
        public uint localScopeId;
        public byte localPort1;
        public byte localPort2;
        public byte localPort3;
        public byte localPort4;
        public uint owningPid;

        public int LocalPort => (localPort1 << 8) + localPort2;
    }
}