using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console
{
    /// <summary>
    /// Representation of a pixel gathered during the Pixel Transfer process. As this represents data already gathered from VRAM, it is read-only.
    /// </summary>
    public class Pixel
    {
        /// <summary>
        /// The colour value of the pixel (0-3)
        /// </summary>
        public byte colour { get; private set; }
        /// <summary>
        /// The palette selection. For BG/Window this is always 0.
        /// </summary>
        public byte palette { get; private set; }
        /// <summary>
        /// The OBJ-to-BG priority (0-1)
        /// </summary>
        public byte backgroundPriority { get; private set; }
        /// <summary>
        /// Gets the value stored in palette and pointed to by colour.<br></br>
        /// 0 == White<br></br>
        /// 1 == Light Grey<br></br>
        /// 2 == Dark Grey<br></br>
        /// 3 == Black
        /// </summary>
        public byte paletteColour
        {
            get
            {
                switch (colour)
                {
                    case 0:
                        return (byte)(palette & 0x03); //0000 0011;
                    case 1:
                        byte index1Bits = (byte)(palette & 0x0C); //0000 1100
                        return ((byte)(index1Bits >> 2));
                    case 2:
                        byte index2Bits = (byte)(palette & 0x30); //0011 0000
                        return ((byte)(index2Bits >> 4));
                    case 3:
                        byte index3Bits = (byte)(palette & 0xC0); //1100 0000
                        return ((byte)(index3Bits >> 6));
                    default:
                        throw new InvalidOperationException("Invalid Colour Index: " + colour);
                }
            }
        }

        public Pixel(byte colour, byte palette, byte backgroundPriority)
        {
            this.colour = colour;
            this.palette = palette;
            this.backgroundPriority = backgroundPriority;
        }
    }
}
