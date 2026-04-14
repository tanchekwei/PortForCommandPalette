// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace PortForCommandPalette.Helpers;

internal static class IconExtractor
{
    private static readonly ConcurrentDictionary<string, IconInfo> _iconCache = new();

    public static IconInfo? GetIconForPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        if (_iconCache.TryGetValue(path, out var cachedIcon))
        {
            return cachedIcon;
        }

        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var shinfo = new NativeMethods.SHFILEINFO();
            var res = NativeMethods.SHGetFileInfo(
                path,
                0,
                ref shinfo,
                (uint)Marshal.SizeOf(shinfo),
                NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_LARGEICON);

            if (res != IntPtr.Zero && shinfo.hIcon != IntPtr.Zero)
            {
                var bitmap = CreateSoftwareBitmapFromHIcon(shinfo.hIcon);
                NativeMethods.DestroyIcon(shinfo.hIcon);

                if (bitmap != null)
                {
                    var stream = Task.Run(() => ConvertToStream(bitmap)).GetAwaiter().GetResult();
                    if (stream != null)
                    {
                        var icon = IconInfo.FromStream(stream);
                        _iconCache[path] = icon;
                        return icon;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Classes.ErrorLogger.LogError(ex);
        }

        return null;
    }

    private static SoftwareBitmap? CreateSoftwareBitmapFromHIcon(IntPtr hIcon)
    {
        if (hIcon == IntPtr.Zero) return null;

        if (!NativeMethods.GetIconInfo(hIcon, out var iconInfo)) return null;

        var hBitmap = iconInfo.hbmColor;
        if (hBitmap == IntPtr.Zero)
        {
            if (iconInfo.hbmMask != IntPtr.Zero) NativeMethods.DeleteObject(iconInfo.hbmMask);
            return null;
        }

        try
        {
            int size = 32;
            var pixels = new byte[size * size * 4];

            var bmi = new NativeMethods.BITMAPINFO
            {
                bmiHeader = new NativeMethods.BITMAPINFOHEADER
                {
                    biSize = (uint)Marshal.SizeOf<NativeMethods.BITMAPINFOHEADER>(),
                    biWidth = size,
                    biHeight = -size,
                    biPlanes = 1,
                    biBitCount = 32,
                    biCompression = 0,
                },
            };

            var hdc = NativeMethods.GetDC(IntPtr.Zero);
            try
            {
                int result = NativeMethods.GetDIBits(
                    hdc,
                    hBitmap,
                    0,
                    (uint)size,
                    pixels,
                    ref bmi,
                    NativeMethods.DIB_RGB_COLORS);

                if (result == 0) return null;
            }
            finally
            {
                NativeMethods.ReleaseDC(IntPtr.Zero, hdc);
            }

            var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, size, size, BitmapAlphaMode.Premultiplied);
            softwareBitmap.CopyFromBuffer(pixels.AsBuffer());
            return softwareBitmap;
        }
        finally
        {
            if (iconInfo.hbmColor != IntPtr.Zero) NativeMethods.DeleteObject(iconInfo.hbmColor);
            if (iconInfo.hbmMask != IntPtr.Zero) NativeMethods.DeleteObject(iconInfo.hbmMask);
        }
    }

    private static async Task<IRandomAccessStream?> ConvertToStream(SoftwareBitmap bitmap)
    {
        var stream = new InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
        encoder.SetSoftwareBitmap(bitmap);
        await encoder.FlushAsync();
        stream.Seek(0);
        return stream;
    }
}