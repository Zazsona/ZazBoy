using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.UI
{
    /// <summary>
    /// Common utilities for UI.
    /// </summary>
    public static class UIUtil
    {
        /// <summary>
        /// Converts a System.Drawing Bitmap to a Avalonia.Media.Imaging Bitmap
        /// </summary>
        /// <param name="drawingBitmap">The System.Drawing Bitmap to convert.</param>
        /// <returns>The Avalonia.Media.Imaging Bitmap</returns>
        public static Avalonia.Media.Imaging.Bitmap ConvertDrawingBitmapToUIBitmap(Bitmap drawingBitmap)
        {
            MemoryStream memoryStream = new MemoryStream();
            drawingBitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;
            Avalonia.Media.Imaging.Bitmap uiBitmap = new Avalonia.Media.Imaging.Bitmap(memoryStream);
            memoryStream.Close();
            return uiBitmap;
        }
    }
}
