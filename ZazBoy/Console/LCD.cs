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
        private byte[,] colourMap;
        private byte[,] oldColourMap;
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
            colourMap = new byte[ScreenPixelWidth, ScreenPixelHeight];
            oldColourMap = new byte[ScreenPixelWidth, ScreenPixelHeight];
            FillScreen(lcdOff); 
        }

        public void DrawPixel(Pixel pixel, byte lineX, byte lineY)
        {
            colourMap[lineX, lineY] = pixel.paletteColour;
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
                            Color colour = GetColourFromId(colourMap[x, y]);
                            int pos = (x * 3) + y * stride;
                            pixPtr[pos] = colour.B;
                            pixPtr[pos + 1] = colour.G;
                            pixPtr[pos + 2] = colour.R;
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
            byte colourId = GetIdFromColour(colour);
            FillScreen(colourId);
        }

        /// <summary>
        /// Fills the ColourMap with the specified colour.
        /// </summary>
        /// <param name="colourId">The id of the colour to fill the map with.</param>
        private void FillScreen(byte colourId)
        {
            for (int x = 0; x < colourMap.GetLength(0); x++)
            {
                for (int y = 0; y < colourMap.GetLength(1); y++)
                {
                    oldColourMap[x, y] = colourMap[x, y];
                    colourMap[x, y] = colourId;
                }
            }
            WriteFrame();
        }

        /// <summary>
        /// Converts the id stored in ColourMap to the Color struct it represents.
        /// </summary>
        /// <param name="colourId">The Id to decode</param>
        /// <returns>The Color denoted by the id</returns>
        private Color GetColourFromId(byte colourId)
        {
            switch (colourId)
            {
                case 0:
                    return lcdWhite;
                case 1:
                    return lcdGrey;
                case 2:
                    return lcdDarkGrey;
                case 3:
                    return lcdBlack;
                case byte.MaxValue:
                default:
                    return lcdOff;
            }
        }

        /// <summary>
        /// Gets the id of the Color struct to store in the ColourMap
        /// </summary>
        /// <param name="colour">The colour to get an Id for</param>
        /// <returns>The id</returns>
        private byte GetIdFromColour(Color colour)
        {
            if (colour == lcdWhite)
                return 0;
            else if (colour == lcdGrey)
                return 1;
            else if (colour == lcdDarkGrey)
                return 2;
            else if (colour == lcdBlack)
                return 3;
            else //lcdOff
                return byte.MaxValue;
        }
    }
}
