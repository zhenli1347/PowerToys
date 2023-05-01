// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.UI.Xaml.Media.Imaging;

namespace ArchivePreviewHandler.Helpers
{
    internal static class BitmapHelper
    {
        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);
                var bitmapImage = new BitmapImage();
                memoryStream.Seek(0, SeekOrigin.Begin);
                bitmapImage.SetSource(memoryStream.AsRandomAccessStream());
                return bitmapImage;
            }
        }
    }
}
