using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console
{
    /// <summary>
    /// A class emulating the functionality of the Sharp LR35902's SoC GPU
    /// </summary>
    public class PPU
    {
        public const ushort LCDControlRegister = 0xFF40;
        public const ushort LCDControlStatusRegister = 0xFF41;
        public const ushort ScrollYRegister = 0xFF42;
        public const ushort ScrollXRegister = 0xFF43;
        public const ushort LineYCoordinateRegister = 0xFF44;
        public const ushort LineYCompareRegister = 0xFF45;
        public const ushort BackgroundPaletteRegister = 0xFF47;
        public const ushort ObjectPalette0Register = 0xFF48;
        public const ushort ObjectPalette1Register = 0xFF49;
        public const ushort WindowXRegister = 0xFF4A;
        public const ushort WindowYRegister = 0xFF4B;

        private const int MaxOAMSearchClocks = 80;
        private const int MaxHorizontalClocks = 456;
        private const int VBlankLines = 10;

        public PPUState currentState { get; private set; }

        /// <summary>
        /// The # of clocks that have occurred for this horizontal line
        /// </summary>
        private int horizontalClocks;

        public void Tick()
        {
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            byte lineY = memMap.Read(LineYCoordinateRegister);
            if (lineY < LCD.ScreenPixelHeight)
            {
                if (horizontalClocks >= 0 && horizontalClocks < MaxOAMSearchClocks)
                    TickOAMSearch();
                else if (horizontalClocks >= MaxOAMSearchClocks && true) //TODO: Add a check for when pixel transfer is complete
                    TickPixelTransfer();
                else if (true && horizontalClocks < MaxHorizontalClocks) //TODO: Ensure Pixel Transfer is complete before starting
                    TickHBlank();
                horizontalClocks++;
            }
            else if (lineY >= LCD.ScreenPixelHeight && lineY < (LCD.ScreenPixelHeight+VBlankLines))
            {
                TickVBlank();
                horizontalClocks++;
            }

            if (horizontalClocks >= MaxHorizontalClocks)
            {
                horizontalClocks = 0;
                lineY++;
                memMap.Write(LineYCoordinateRegister, lineY);
            }
            if (lineY >= (LCD.ScreenPixelHeight + VBlankLines))
                memMap.Write(LineYCoordinateRegister, 0);
        }

        private void TickOAMSearch()
        {
            currentState = PPUState.OAMSearch;
        }

        private void TickPixelTransfer()
        {
            currentState = PPUState.PixelTransfer;
        }

        private void TickHBlank()
        {
            currentState = PPUState.HBlank;
        }

        private void TickVBlank()
        {
            currentState = PPUState.VBlank;
        }

        public enum PPUState
        {
            OAMSearch,
            PixelTransfer,
            HBlank,
            VBlank
        }
    }
}
