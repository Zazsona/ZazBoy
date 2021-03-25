using System;
using System.Collections.Generic;
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

        public LCD()
        {
            this.powered = true;
        }

        public void DrawPixel()
        {
            PPU ppu = GameBoy.Instance().PPU;
            if (!ppu.HasPPUDisabledThisFrame) //DMG disables drawing until the next frame if it's been disabled.
            {
                //TODO: Fill
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
