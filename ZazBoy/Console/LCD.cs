using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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
        private Bitmap bitmap;
        private Graphics gfx;

        private Color lcdOff = Color.FromArgb(202, 220, 159);
        private Color lcdWhite = Color.FromArgb(155, 188, 15);
        private Color lcdGrey = Color.FromArgb(139, 172, 15);
        private Color lcdDarkGrey = Color.FromArgb(48, 98, 48);
        private Color lcdBlack = Color.FromArgb(15, 56, 15);

        public LCD()
        {
            this.powered = true;
            bitmap = new Bitmap(160, 144);
            gfx = Graphics.FromImage(bitmap);
            gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            gfx.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
            Pen pen = new Pen(lcdOff);
            gfx.FillRectangle(pen.Brush, 0, 0, 160, 144);
        }

        public void DrawPixel(Pixel pixel, byte lineX, byte lineY)
        {
            PPU ppu = GameBoy.Instance().PPU;
            if (!ppu.HasPPUDisabledThisFrame) //DMG disables drawing until the next frame if it's been disabled.
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
                Pen pen = new Pen(color);
                gfx.DrawLine(pen, lineX, lineY, lineX+1, lineY);
            }
        }

        public void WriteFrame()
        {
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
                {
                    Pen pen = new Pen(lcdOff);
                    gfx.FillRectangle(pen.Brush, 0, 0, bitmap.Width, bitmap.Height);
                }
            }
        }
    }
}
