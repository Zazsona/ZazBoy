using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZazBoy.Console
{
    /// <summary>
    /// A class representing the LCD as a 'canvas'
    /// </summary>
    public class LCD
    {
        public const int ScreenPixelWidth = 160;
        public const int ScreenPixelHeight = 144;

        public delegate void LCDUpdateHandler(Bitmap bitmap);
        public event LCDUpdateHandler onLCDUpdate;

        private bool powered;
        private Color[,] colourMap; //TODO: Consider changing to numbers (Color == 12 bytes, byte == 1 byte!)
        private Color[,] oldColourMap;
        private Bitmap bitmap;

        private Color lcdOff = Color.FromArgb(202, 220, 159);
        private Color lcdWhite = Color.FromArgb(155, 188, 15);
        private Color lcdGrey = Color.FromArgb(139, 172, 15);
        private Color lcdDarkGrey = Color.FromArgb(48, 98, 48);
        private Color lcdBlack = Color.FromArgb(15, 56, 15);

        public LCD()
        {
            this.powered = true;
            bitmap = new Bitmap(ScreenPixelWidth, ScreenPixelHeight);
            colourMap = new Color[ScreenPixelWidth, ScreenPixelHeight];
            oldColourMap = new Color[ScreenPixelWidth, ScreenPixelHeight];
            FillScreen(lcdOff);
        }

        public void DrawPixel(Pixel pixel, byte lineX, byte lineY)
        {
            Color color = Color.Red;
            switch (pixel.paletteColour)
            {
                case 0:
                    color = lcdWhite;
                    break;
                case 1:
                    color = lcdGrey;
                    break;
                case 2:
                    color = lcdDarkGrey;
                    break;
                case 3:
                    color = lcdBlack;
                    break;
            }
            colourMap[lineX, lineY] = color;
        }

        public void WriteFrame()
        {
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = data.Stride;
            unsafe
            {
                byte* pixPtr = (byte*)data.Scan0;
                for (int x = 0; x < LCD.ScreenPixelWidth; x++)
                {
                    for (int y = 0; y < LCD.ScreenPixelHeight; y++)
                    {
                        if (colourMap[x, y] != oldColourMap[x, y])
                        {
                            int pos = (x * 3) + y * stride;
                            pixPtr[pos] = colourMap[x, y].B;
                            pixPtr[pos + 1] = colourMap[x, y].G;
                            pixPtr[pos + 2] = colourMap[x, y].R;
                            oldColourMap[x, y] = colourMap[x, y];
                        }
                    }
                }
            }
            bitmap.UnlockBits(data);
            onLCDUpdate?.Invoke(bitmap);
        }

        /// <summary>
        /// Sets if the display has power. If disabled, the display will be wiped and cannot be drawn to. When reenabled, BG Palette 0x00 will fill the display.
        /// </summary>
        /// <param name="newPoweredState">The new powered state</param>
        public void SetDisplayPowered(bool newPoweredState)
        {
            if (powered != newPoweredState)
            {
                this.powered = newPoweredState;
                if (!powered)
                    FillScreen(lcdOff);
                else
                    FillScreen(lcdWhite);
            }
        }

        /// <summary>
        /// Fills the ColourMap with the specified colour.
        /// </summary>
        /// <param name="colour">The colour to fill the map with.</param>
        private void FillScreen(Color colour)
        {
            for (int x = 0; x < colourMap.GetLength(0); x++)
            {
                for (int y = 0; y < colourMap.GetLength(1); y++)
                {
                    oldColourMap[x, y] = colourMap[x, y];
                    colourMap[x, y] = colour;
                }
            }
            WriteFrame();
        }
    }
}
