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
        private bool powered;

        private Bitmap bitmap;
        private Graphics gfx; //TODO: System.Drawing crap is all temp.

        public LCD()
        {
            this.powered = true;
            bitmap = new Bitmap(160, 144);
            gfx = Graphics.FromImage(bitmap);
            gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            gfx.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
            Pen pen = new Pen(Color.Red);
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
                        color = Color.White;
                        break;
                    case 1:
                        color = Color.LightGray;
                        break;
                    case 2:
                        color = Color.DarkGray;
                        break;
                    case 3:
                        color = Color.Black;
                        break;
                }
                Pen pen = new Pen(color);
                gfx.DrawLine(pen, lineX, lineY, lineX+1, lineY);
                if (DateTime.Now.Second == 30 || DateTime.Now.Second == 1)
                    bitmap.Save("H:\\frame.png");

            }
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
                //TODO: Fill
            }
        }
    }
}
