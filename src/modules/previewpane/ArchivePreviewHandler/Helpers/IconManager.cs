// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using ManagedCommon;
using Microsoft.UI.Xaml.Media;

namespace ArchivePreviewHandler.Helpers
{
    public class IconManager : IIconManager
    {
        private readonly Dictionary<string, ImageSource> _cache;

        private ImageSource _directoryIconCache;

        public IconManager()
        {
            _cache = new Dictionary<string, ImageSource>();
        }

        public ImageSource GetFileIcon(string fileName)
        {
            var extension = Path.GetExtension(fileName);

            if (_cache.TryGetValue(extension, out var cachedIcon))
            {
                return cachedIcon;
            }

            try
            {
                var shFileInfo = default(SHFILEINFO);
                _ = NativeMethods.SHGetFileInfo(fileName, NativeMethods.FILE_ATTRIBUTE_NORMAL, ref shFileInfo, (uint)Marshal.SizeOf(shFileInfo), NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_SMALLICON | NativeMethods.SHGFI_USEFILEATTRIBUTES);
                var icon = (Icon)Icon.FromHandle(shFileInfo.hIcon).Clone();
                var bitmap = icon.ToBitmap();
                var imageSource = BitmapHelper.BitmapToImageSource(bitmap);
                _ = NativeMethods.DestroyIcon(shFileInfo.hIcon);

                _cache.Add(extension, imageSource);
                return imageSource;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to retrieve icon for file extension {extension}", ex);
            }

            return null;
        }

        public ImageSource GetDirectoryIcon()
        {
            if (_directoryIconCache != null)
            {
                return _directoryIconCache;
            }

            try
            {
                var shinfo = default(SHFILEINFO);
                _ = NativeMethods.SHGetFileInfo("directory", NativeMethods.FILE_ATTRIBUTE_DIRECTORY, ref shinfo, (uint)Marshal.SizeOf(shinfo), NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_SMALLICON | NativeMethods.SHGFI_USEFILEATTRIBUTES);
                var icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
                var bmp = icon.ToBitmap();
                var imageSource = BitmapHelper.BitmapToImageSource(bmp);
                _ = NativeMethods.DestroyIcon(shinfo.hIcon);

                _directoryIconCache = imageSource;
                return imageSource;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to retrieve icon for directory", ex);
            }

            return null;
        }
    }
}
