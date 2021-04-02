using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        public bool IsPPUEnabled
        {
            get
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 7));
                return stateBit != 0;
            }

            set
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 7));
                if (value)
                {
                    GameBoy.Instance().LCD.SetDisplayPowered(true);
                    lcdControl = (byte)(lcdControl | stateBit);
                }
                else
                {
                    lcdControl = (byte)(lcdControl & ~stateBit);
                    HasPPUDisabledThisFrame = true;
                    currentState = PPUState.HBlank; //PPU reports itself as mode 00
                    GameBoy.Instance().LCD.SetDisplayPowered(false);
                }

                memMap.Write(LCDControlRegister, lcdControl);
            }
        }
        public bool IsWindowTileMapStart9C00
        {
            get
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 6));
                return stateBit != 0;
            }

            set
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 6));
                if (value)
                    lcdControl = (byte)(lcdControl | stateBit);
                else
                    lcdControl = (byte)(lcdControl & ~stateBit);
                memMap.Write(LCDControlRegister, lcdControl);
            }
        }
        public bool IsWindowEnabled
        {
            get
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 5));
                return stateBit != 0;
            }

            set
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 5));
                if (value)
                    lcdControl = (byte)(lcdControl | stateBit);
                else
                    lcdControl = (byte)(lcdControl & ~stateBit);
                memMap.Write(LCDControlRegister, lcdControl);
            }
        }
        public bool IsBGTileDataStart8000
        {
            get
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 4));
                return stateBit != 0;
            }

            set
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 4));
                if (value)
                    lcdControl = (byte)(lcdControl | stateBit);
                else
                    lcdControl = (byte)(lcdControl & ~stateBit);
                memMap.Write(LCDControlRegister, lcdControl);
            }
        }
        public bool IsBGTileMapStart9C00
        {
            get
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 3));
                return stateBit != 0;
            }

            set
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 3));
                if (value)
                    lcdControl = (byte)(lcdControl | stateBit);
                else
                    lcdControl = (byte)(lcdControl & ~stateBit);
                memMap.Write(LCDControlRegister, lcdControl);
            }
        }
        public bool IsOBJDoubleHeight
        {
            get
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 2));
                return stateBit != 0;
            }

            set
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 2));
                if (value)
                    lcdControl = (byte)(lcdControl | stateBit);
                else
                    lcdControl = (byte)(lcdControl & ~stateBit);
                memMap.Write(LCDControlRegister, lcdControl);
            }
        }
        public bool IsOBJEnabled
        {
            get
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 1));
                return stateBit != 0;
            }

            set
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 1));
                if (value)
                    lcdControl = (byte)(lcdControl | stateBit);
                else
                    lcdControl = (byte)(lcdControl & ~stateBit);
                memMap.Write(LCDControlRegister, lcdControl);
            }
        }
        public bool IsBGAndWindowEnabled
        {
            get
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 0));
                return stateBit != 0;
            }

            set
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 0));
                if (value)
                    lcdControl = (byte)(lcdControl | stateBit);
                else
                    lcdControl = (byte)(lcdControl & ~stateBit);
                memMap.Write(LCDControlRegister, lcdControl);

                if (!value)
                    IsWindowEnabled = false; //This flag overrides the window flag when reset.
            }
        }
        public bool HasPPUDisabledThisFrame { get; private set; }

        public bool IsLineYCompareEnabled
        {
            get
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBit = (byte)(lcdStatus & (1 << 6));
                return stateBit != 0;
            }

            set
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBit = (byte)(lcdStatus & (1 << 6));
                if (value)
                    lcdStatus = (byte)(lcdStatus | stateBit);
                else
                    lcdStatus = (byte)(lcdStatus & ~stateBit);
                memMap.Write(LCDControlStatusRegister, lcdStatus);
            }
        }
        public bool IsOAMCheckEnabled
        {
            get
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBit = (byte)(lcdStatus & (1 << 5));
                return stateBit != 0;
            }

            set
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBit = (byte)(lcdStatus & (1 << 5));
                if (value)
                    lcdStatus = (byte)(lcdStatus | stateBit);
                else
                    lcdStatus = (byte)(lcdStatus & ~stateBit);
                memMap.Write(LCDControlStatusRegister, lcdStatus);
            }
        }
        public bool IsVBlankCheckEnabled
        {
            get
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBit = (byte)(lcdStatus & (1 << 4));
                return stateBit != 0;
            }

            set
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBit = (byte)(lcdStatus & (1 << 4));
                if (value)
                    lcdStatus = (byte)(lcdStatus | stateBit);
                else
                    lcdStatus = (byte)(lcdStatus & ~stateBit);
                memMap.Write(LCDControlStatusRegister, lcdStatus);
            }
        }
        public bool IsHBlankEnabled
        {
            get
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBit = (byte)(lcdStatus & (1 << 3));
                return stateBit != 0;
            }

            set
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBit = (byte)(lcdStatus & (1 << 3));
                if (value)
                    lcdStatus = (byte)(lcdStatus | stateBit);
                else
                    lcdStatus = (byte)(lcdStatus & ~stateBit);
                memMap.Write(LCDControlStatusRegister, lcdStatus);
            }
        }
        public PPUState currentState
        {
            get
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBits = (byte)(lcdStatus & 0x03);
                switch (stateBits)
                {
                    case 0:
                        return PPUState.HBlank;
                    case 1:
                        return PPUState.VBlank;
                    case 2:
                        return PPUState.OAMSearch;
                    case 3:
                        return PPUState.PixelTransfer;
                }
                throw new InvalidOperationException("PPU State is invalid: " + stateBits);
            }

            private set
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBits = (byte)(lcdStatus & 0xFC); //Unset last two bits
                switch (value)
                {
                    case PPUState.HBlank:
                        stateBits = (byte)(stateBits | 0x00);
                        break;
                    case PPUState.VBlank:
                        stateBits = (byte)(stateBits | 0x01);
                        break;
                    case PPUState.OAMSearch:
                        stateBits = (byte)(stateBits | 0x02);
                        break;
                    case PPUState.PixelTransfer:
                        stateBits = (byte)(stateBits | 0x03);
                        break;
                }
                memMap.WriteDirect(LCDControlStatusRegister, stateBits);
            }
        }

        /// <summary>
        /// The # of clocks that have occurred for this horizontal line
        /// </summary>
        private int horizontalClocks;
        /// <summary>
        /// Records if the LYC has already been performed for this line.
        /// </summary>
        private bool compareYCheckPerformedForLine;
        /// <summary>
        /// Records if the OAM process has already been started for this line.
        /// </summary>
        private bool initialOAMLineTick = true;
        /// <summary>
        /// Records if the Pixel Transfer process has already been started for this line.
        /// </summary>
        private bool initialPixelTransferLineTick = true;
        /// <summary>
        /// Records if the HBlank process has already been started for this line.
        /// </summary>
        private bool initialHBlankLineTick = true;
        /// <summary>
        /// Records if the VBlank process has already been started for this line.
        /// </summary>
        private bool initialVBlankLineTick = true;
        /// <summary>
        /// The indexes of the objects in OAM that are present on the current horizontal line.
        /// </summary>
        private List<int> objectIdsForLine;

        private PPUFetcher fetcher;

        private byte lineX;

        private Queue<Pixel> backgroundQueue;

        public PPU()
        {
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            memMap.WriteDirect(LCDControlRegister, 0x91); //1001 0001
            memMap.WriteDirect(BackgroundPaletteRegister, 0xFC); //1111 1100 (Two-tone palette?)
            memMap.WriteDirect(ObjectPalette0Register, 0xFF); //1111 1100 (One-tone palette?)
            memMap.WriteDirect(ObjectPalette1Register, 0xFF); //1111 1100 (One-tone palette?)

            this.fetcher = new PPUFetcher(this);
        }

        private int frame = 0;
        public void Tick()
        {
            if (!IsPPUEnabled)
                return;


            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            byte lineY = memMap.Read(LineYCoordinateRegister);
            if (!compareYCheckPerformedForLine)
                ExecuteLineYCompare(memMap, lineY);

            if (lineY < LCD.ScreenPixelHeight)
            {
                if (horizontalClocks < MaxOAMSearchClocks)
                    TickOAMSearch(memMap, lineY);
                else if (horizontalClocks >= MaxOAMSearchClocks && (horizontalClocks < 252 || lineX < 160))
                    TickPixelTransfer(memMap, lineY);
                else if (horizontalClocks < MaxHorizontalClocks)
                    TickHBlank(memMap);

                horizontalClocks++;
            }
            else if (lineY >= LCD.ScreenPixelHeight && lineY < (LCD.ScreenPixelHeight + VBlankLines))
            {
                TickVBlank(memMap, lineY);
                horizontalClocks++;
            }

            if (horizontalClocks >= MaxHorizontalClocks)
            {
                horizontalClocks = 0;
                lineY++;
                lineX = 0;
                compareYCheckPerformedForLine = false;
                initialOAMLineTick = true;
                initialPixelTransferLineTick = true;
                initialHBlankLineTick = true;
                memMap.Write(LineYCoordinateRegister, lineY);
            }
            if (lineY >= (LCD.ScreenPixelHeight + VBlankLines))
            {
                if ((frame == 10 || frame % 30 == 0))
                {
                    GameBoy.Instance().LCD.SaveToFile();
                    System.Console.WriteLine("Frame: " + frame);
                }
                frame++;
                memMap.Write(LineYCoordinateRegister, 0);
                HasPPUDisabledThisFrame = false;
                initialVBlankLineTick = true;
            }
        }

        private void TickOAMSearch(MemoryMap memMap, byte lineY)
        {
            currentState = PPUState.OAMSearch;
            if (initialOAMLineTick)
            {
                if (IsOAMCheckEnabled)
                    GameBoy.Instance().InterruptHandler.SetInterruptRequested(InterruptHandler.InterruptType.LCDStatus, true);

                int spriteHeight = (IsOBJDoubleHeight) ? 16 : 8; //TODO: This is a bit icky as it doesn't read direct, and while VRAM isn't blocked yet, it may cause issues later on.
                objectIdsForLine = new List<int>();
                for (int spriteIndex = 0; spriteIndex < 40; spriteIndex++)
                {
                    ushort yPosAddress = (ushort)(MemoryMap.OAM_ADDRESS + (spriteIndex * 4));
                    ushort xPosAddress = (ushort)(yPosAddress + 1);

                    byte yPos = memMap.Read(yPosAddress);
                    byte xPos = memMap.Read(xPosAddress);

                    if (xPos != 0 && (lineY + 16) >= yPos && (lineY + 16) < yPos + spriteHeight) //Objects are mapped per-pixel, and can have a height of 16. As YPos 0 == LineY -16, (YPos 16 == LineY 0) we have to account for that.
                    {
                        objectIdsForLine.Add(spriteIndex);
                        if (objectIdsForLine.Count == 10) //Only 10 sprites per line
                            break;
                    }
                }
                initialOAMLineTick = false;
            }
        }

        private void TickPixelTransfer(MemoryMap memMap, byte lineY)
        {
            currentState = PPUState.PixelTransfer;
            if (initialPixelTransferLineTick)
            {
                backgroundQueue = new Queue<Pixel>();
                fetcher.Reset();
                initialPixelTransferLineTick = false;
            }

            fetcher.Tick(lineX, lineY);
            if (backgroundQueue.Count > 0 && lineX < 160)
            {
                GameBoy.Instance().LCD.DrawPixel(backgroundQueue.Dequeue(), lineX, lineY);
                lineX++;
            }
            if (fetcher.fetcherState == PPUFetcher.FetcherState.Push && fetcher.pixelsToPush != null && backgroundQueue.Count == 0)
            {
                Pixel[] pixels = fetcher.pixelsToPush;
                for (int i = 7; i > -1; i--)
                {
                    backgroundQueue.Enqueue(pixels[i]);
                }
                fetcher.ProgressCycle();
                fetcher.Tick(lineX, lineY);
            }
        }

        private void TickHBlank(MemoryMap memMap)
        {
            currentState = PPUState.HBlank;
            if (initialHBlankLineTick && IsHBlankEnabled)
            {
                GameBoy.Instance().InterruptHandler.SetInterruptRequested(InterruptHandler.InterruptType.LCDStatus, true);
                initialHBlankLineTick = false;
            }
            //HBlank purposely does pretty much nothing. It's blanking after all! We've just got to burn the ticks.
        }

        private void TickVBlank(MemoryMap memMap, byte lineY)
        {
            currentState = PPUState.VBlank;
            if (lineY == LCD.ScreenPixelHeight)
                GameBoy.Instance().InterruptHandler.SetInterruptRequested(InterruptHandler.InterruptType.VBlank, true); //It seems odd that there is both a dedicated VBlank interrupt, and an LCD status one, so there may be something wrong here.
            if (initialVBlankLineTick && (IsOAMCheckEnabled || IsVBlankCheckEnabled)) //Yup, it'll take VBlank or OAM being enabled apparently... This needs more research.
            {
                GameBoy.Instance().InterruptHandler.SetInterruptRequested(InterruptHandler.InterruptType.LCDStatus, true);
                initialVBlankLineTick = false;
            }
            //For the same reasons as HBlank, this does basically nothing but burn ticks.
        }

        /// <summary>
        /// Updates the LCD Status register to reflect the state of whether LY == LYC, and requests the interrupt if enabled.
        /// </summary>
        /// <param name="memMap">The current memory map</param>
        /// <param name="lineY">The value of Line Y to check.</param>
        private void ExecuteLineYCompare(MemoryMap memMap, byte lineY)
        {
            byte lineYCompare = memMap.Read(LineYCompareRegister);
            if (lineY == lineYCompare)
            {
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBit = (byte)(lcdStatus & (1 << 2));
                lcdStatus = (byte)(lcdStatus | stateBit);
                memMap.Write(LCDControlStatusRegister, lcdStatus);
                if (IsLineYCompareEnabled)
                    GameBoy.Instance().InterruptHandler.SetInterruptRequested(InterruptHandler.InterruptType.LCDStatus, true);
            }
            else
            {
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBit = (byte)(lcdStatus & (1 << 2));
                lcdStatus = (byte)(lcdStatus & ~stateBit);
                memMap.Write(LCDControlStatusRegister, lcdStatus);
            }
            compareYCheckPerformedForLine = true;
        }

        /// <summary>
        /// Enum representing the PPU states.
        /// </summary>
        public enum PPUState
        {
            OAMSearch,
            PixelTransfer,
            HBlank,
            VBlank
        }
    }
}
