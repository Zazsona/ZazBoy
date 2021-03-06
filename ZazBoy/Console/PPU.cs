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
    public partial class PPU
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
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 7));
                return stateBit != 0;
            }

            set
            {
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 7));
                if (value)
                {
                    lcd.SetDisplayPowered(true);
                    lcdControl = (byte)(lcdControl | stateBit);
                }
                else
                {
                    lcdControl = (byte)(lcdControl & ~stateBit);
                    HasPPUDisabledThisFrame = true;
                    currentState = PPUState.HBlank; //PPU reports itself as mode 00
                    lcd.SetDisplayPowered(false);
                }

                memMap.Write(LCDControlRegister, lcdControl);
            }
        }
        public bool IsWindowTileMapStart9C00
        {
            get
            {
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 6));
                return stateBit != 0;
            }

            set
            {
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
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 5));
                return stateBit != 0;
            }

            set
            {
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
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 4));
                return stateBit != 0;
            }

            set
            {
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
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 3));
                return stateBit != 0;
            }

            set
            {
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
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 2));
                return stateBit != 0;
            }

            set
            {
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
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 1));
                return stateBit != 0;
            }

            set
            {
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
                byte lcdControl = memMap.Read(LCDControlRegister);
                byte stateBit = (byte)(lcdControl & (1 << 0));
                return stateBit != 0;
            }

            set
            {
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
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBit = (byte)(lcdStatus & (1 << 6));
                return stateBit != 0;
            }

            set
            {
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
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBit = (byte)(lcdStatus & (1 << 5));
                return stateBit != 0;
            }

            set
            {
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
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBit = (byte)(lcdStatus & (1 << 4));
                return stateBit != 0;
            }

            set
            {
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
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBit = (byte)(lcdStatus & (1 << 3));
                return stateBit != 0;
            }

            set
            {
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
        /// <summary>
        /// Fetcher responsible for deciphering tiles and gathering Pixel data
        /// </summary>
        private PPUFetcher fetcher;
        /// <summary>
        /// The queue for pixels on the background or window
        /// </summary>
        private Queue<Pixel> backgroundQueue;
        /// <summary>
        /// The queue for pixels from objects (sprites)
        /// </summary>
        private Queue<Pixel> objectQueue;
        /// <summary>
        /// The X and Y positions for the LCD (These do not always match the BG/window position due to SCX/SCY and WX/WY)
        /// </summary>
        private byte lcdX, lcdY = 0;
        /// <summary>
        /// Checks if the sprite fetch has started for the current lcd X coordinate
        /// </summary>
        private bool spriteFetchStarted;
        /// <summary>
        /// Checks if the sprite fetch is complete for the current lcd X coordinate
        /// </summary>
        private bool spriteFetchComplete;
        /// <summary>
        /// Maintains the ticks for the active sprite fetch
        /// </summary>
        private int spriteTicks = 0;
        /// <summary>
        /// The remaining clocks needing to be applied for an object at lcd 0
        /// </summary>
        private int objectHorizontalPositionPenaltyClocks;
        /// <summary>
        /// Bool for penalty complete this line
        /// </summary>
        private bool objectHorizontalPositionPenaltyComplete;
        /// <summary>
        /// Flag for if the sprite fetch abortion is currently being executed.
        /// </summary>
        private bool abortingSpriteFetch;

        /// <summary>
        /// The Memory context
        /// </summary>
        private MemoryMap memMap;
        /// <summary>
        /// The interrupt handler for this GB
        /// </summary>
        private InterruptHandler interruptHandler;
        /// <summary>
        /// The LCD to draw on
        /// </summary>
        private LCD lcd;

        public PPU(MemoryMap memMap, InterruptHandler interruptHandler, LCD lcd)
        {
            this.memMap = memMap;
            this.interruptHandler = interruptHandler;
            this.lcd = lcd;
            memMap.WriteDirect(LCDControlRegister, 0x91); //1001 0001
            memMap.WriteDirect(BackgroundPaletteRegister, 0xFC); //1111 1100 (Two-tone palette?)
            memMap.WriteDirect(ObjectPalette0Register, 0xFF); //1111 1100 (One-tone palette?)
            memMap.WriteDirect(ObjectPalette1Register, 0xFF); //1111 1100 (One-tone palette?)
            this.fetcher = new PPUFetcher(memMap, this);
        }

        public void Tick()
        {
            if (!IsPPUEnabled)
                return;

            if (!compareYCheckPerformedForLine)
                ExecuteLineYCompare(memMap);

            if (lcdY < LCD.ScreenPixelHeight)
            {
                if (horizontalClocks < MaxOAMSearchClocks)
                    TickOAMSearch(memMap);
                else if (horizontalClocks >= MaxOAMSearchClocks && (horizontalClocks < 252 || lcdX < 160))
                    TickPixelTransfer(memMap);
                else if (horizontalClocks < MaxHorizontalClocks)
                    TickHBlank(memMap);
                horizontalClocks++;
            }
            else if (lcdY >= LCD.ScreenPixelHeight && lcdY < (LCD.ScreenPixelHeight + VBlankLines))
            {
                TickVBlank(memMap);
                horizontalClocks++;
            }

            if (horizontalClocks >= MaxHorizontalClocks)
            {
                horizontalClocks = 0;
                compareYCheckPerformedForLine = false;
                initialOAMLineTick = true;
                initialPixelTransferLineTick = true;
                initialHBlankLineTick = true;
                lcdX = 0;
                lcdY++;
                memMap.Write(LineYCoordinateRegister, lcdY);
            }
            if (lcdY >= (LCD.ScreenPixelHeight + VBlankLines))
            {
                lcd.WriteFrame();
                lcdY = 0;
                memMap.Write(LineYCoordinateRegister, 0);
                HasPPUDisabledThisFrame = false;
                initialVBlankLineTick = true;
            }
        }

        private void TickOAMSearch(MemoryMap memMap)
        {
            if (currentState != PPUState.OAMSearch)
                currentState = PPUState.OAMSearch;
            if (initialOAMLineTick)
            {
                if (IsOAMCheckEnabled)
                    interruptHandler.SetInterruptRequested(InterruptHandler.InterruptType.LCDStatus, true);

                int spriteHeight = (IsOBJDoubleHeight) ? 16 : 8; //This is a bit icky as it doesn't read direct, and while VRAM isn't blocked yet, it may cause issues later on.
                objectIdsForLine = new List<int>();
                for (int spriteIndex = 0; spriteIndex < 40; spriteIndex++)
                {
                    ushort yPosAddress = (ushort)(MemoryMap.OAM_ADDRESS + (spriteIndex * 4));
                    ushort xPosAddress = (ushort)(yPosAddress + 1);

                    byte yPos = memMap.ReadDirect(yPosAddress);
                    byte xPos = memMap.ReadDirect(xPosAddress);

                    if (xPos != 0 && (lcdY + 16) >= yPos && (lcdY + 16) < yPos + spriteHeight) //Objects are mapped per-pixel, and can have a height of 16. As YPos 0 == LcdY -16, (YPos 16 == LcdY 0) we have to account for that.
                    {
                        objectIdsForLine.Add(spriteIndex);
                        if (objectIdsForLine.Count == 10) //Only 10 sprites per line
                            break;
                    }
                }
                initialOAMLineTick = false;
            }
        }

        private void TickPixelTransfer(MemoryMap memMap)
        {
            if (currentState != PPUState.PixelTransfer)
                currentState = PPUState.PixelTransfer;
            if (initialPixelTransferLineTick)
            {
                backgroundQueue = new Queue<Pixel>();
                objectQueue = new Queue<Pixel>();
                fetcher.Reset();
                spriteFetchStarted = false;
                spriteFetchComplete = false;
                spriteTicks = 0;
                objectHorizontalPositionPenaltyClocks = 0;
                objectHorizontalPositionPenaltyComplete = false;
                initialPixelTransferLineTick = false;
            }

            if (!spriteFetchComplete)
            {
                bool tickConsumed = TickSpriteFetch(memMap);
                if (tickConsumed)
                    return;
            }
            fetcher.Tick(lcdX, lcdY);
            AttemptPixelRender();
        }

        public bool PushPixelsToBackgroundQueue(Pixel[] pixels)
        {
            if (backgroundQueue.Count == 0)
            {
                for (int i = 7; i > -1; i--)
                {
                    backgroundQueue.Enqueue(pixels[i]);
                }
                return true;
            }
            return false;
        }

        private void AttemptPixelRender()
        {
            if (backgroundQueue.Count > 0 && lcdX < 160)
            {
                Pixel pixelToRender;
                Pixel bgPixel = backgroundQueue.Dequeue();
                Pixel? spritePixel = (objectQueue.Count > 0) ? objectQueue.Dequeue() : null;
                if (spritePixel != null && spritePixel.colour != 0x00 && (!spritePixel.backgroundPriority || spritePixel.backgroundPriority && bgPixel.colour == 0x00))
                    pixelToRender = spritePixel;
                else
                    pixelToRender = bgPixel;

                if (!HasPPUDisabledThisFrame) //PPU doesn't draw for the rest of the frame if it's been disabled at any point.
                    lcd.DrawPixel(pixelToRender, lcdX, lcdY);
                lcdX++;
                spriteFetchStarted = false;
                spriteFetchComplete = false; //Increment X, so we may need to fetch another sprite.
                spriteTicks = 0;
                objectHorizontalPositionPenaltyClocks = 0;
                objectHorizontalPositionPenaltyComplete = false;
            }
        }

        private bool TickSpriteFetch(MemoryMap memMap) //TODO: Sprite overlapping
        {
            if (abortingSpriteFetch)
            {
                AbortSpriteFetch();
                return true;
            }
            if ((spriteFetchStarted || IsBGAndWindowEnabled) && IsSpriteAtCurrentLCDPosition(memMap))
            {
                spriteFetchStarted = true;
                if (backgroundQueue.Count == 0 || fetcher.fetcherState < PPUFetcher.FetcherState.Push)
                {
                    fetcher.Tick(lcdX, lcdY);
                    if (fetcher.fetcherState != PPUFetcher.FetcherState.Push)
                        fetcher.Tick(lcdX, lcdY); //Apparently only takes one clock to advance a "step" in this mode. As fetcher takes two clocks to advance normally, just call it twice!
                    if (!IsBGAndWindowEnabled)
                        AbortSpriteFetch();
                    return true;
                }

                byte scx = memMap.ReadDirect(ScrollXRegister);
                if (lcdX == 0 && (scx & 0x07) > 0 && objectHorizontalPositionPenaltyClocks == 0 && !objectHorizontalPositionPenaltyComplete) //We have already checked there's a sprite at the current X/Y pos, so if lcdX == 0, there's a sprite at 0, so we need to handle the pixel binning delay.
                    objectHorizontalPositionPenaltyClocks = scx & 0x07;
                
                if (objectHorizontalPositionPenaltyClocks > 0)
                {
                    objectHorizontalPositionPenaltyClocks--;
                    if (objectHorizontalPositionPenaltyClocks == 0)
                    {
                        objectHorizontalPositionPenaltyComplete = true;
                        if (!IsBGAndWindowEnabled)
                            AbortSpriteFetch();
                    }
                    return true;
                }

                if (spriteTicks == 0)
                {
                    fetcher.Tick(lcdX, lcdY);
                    spriteTicks++;
                    if (!IsBGAndWindowEnabled)
                        AbortSpriteFetch();
                    return true;
                }

                if (spriteTicks < 4)
                {
                    spriteTicks++;
                    if (spriteTicks == 4)
                    {
                        fetcher.Tick(lcdX, lcdY);
                        if (!IsBGAndWindowEnabled)
                            AbortSpriteFetch();
                    }
                    return true;
                }

                if (spriteTicks < 5)
                {
                    //DMG would normally grab the tile address during this clock.
                    spriteTicks++;
                    if (!IsBGAndWindowEnabled)
                        AbortSpriteFetch();
                    return true;
                }

                if (spriteTicks < 6)
                {
                    //DMG would normally exit tile fetch here
                    spriteTicks++;
                    return true;
                }

                ushort spriteAddress = GetSpriteAddressAtCurrentLCDPosition(memMap);

                ushort flagAddress = (ushort)(spriteAddress + 3);
                byte flagByte = memMap.ReadDirect(flagAddress);
                bool prioritySet = (flagByte & (1 << 7)) != 0;
                bool yFlipSet = (flagByte & (1 << 6)) != 0;
                bool xFlipSet = (flagByte & (1 << 5)) != 0;
                bool altPalette = (flagByte & (1 << 4)) != 0;

                int spriteHeight = (IsOBJDoubleHeight) ? 16 : 8;
                int tileIndex = memMap.ReadDirect((ushort)(spriteAddress + 2));
                byte objYPosition = (byte)(memMap.ReadDirect(spriteAddress) - 16); //YPos of a sprite is the first byte.
                byte pixelLowByteIndex = (yFlipSet) ? ((byte) (((spriteHeight-1)-(lcdY-objYPosition)) * 2)) : ((byte)((lcdY - objYPosition) * 2));
                byte pixelHighByteIndex = (byte)(pixelLowByteIndex + 1);
                ushort tileAddress = ((ushort)(0x8000 + (tileIndex * 16)));
                byte lowByte = GetTileByte(tileAddress, pixelLowByteIndex);
                byte highByte = GetTileByte(tileAddress, pixelHighByteIndex);

                byte paletteByte = (altPalette) ? memMap.ReadDirect(ObjectPalette1Register) : memMap.ReadDirect(ObjectPalette0Register);    //TODO: Sprite overlapping
                Pixel[] pixels = GetPixels(lowByte, highByte, paletteByte, prioritySet);
                if (!xFlipSet)
                {
                    for (int i = 7; i > -1; i--)
                        objectQueue.Enqueue(pixels[i]);
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                        objectQueue.Enqueue(pixels[i]);
                }

            }
            spriteFetchComplete = true;
            return false;
        }

        private void AbortSpriteFetch()
        {
            AttemptPixelRender();
            fetcher.Tick(lcdX, lcdY);
            if (lcdX < 160 && !abortingSpriteFetch)
            {
                abortingSpriteFetch = true;
            }
            else
            {
                abortingSpriteFetch = false;
                spriteFetchComplete = true;
            }
        }

        /// <summary>
        /// Convenience method that calls GetSpriteIdAtCurrentLCDPosition, and returns if there is a sprite or not.
        /// </summary>
        /// <param name="memMap">The memory map</param>
        /// <returns>True on sprite at lcdX/lcdY position.</returns>
        private bool IsSpriteAtCurrentLCDPosition(MemoryMap memMap)
        {
            return GetSpriteAddressAtCurrentLCDPosition(memMap) != 0;
        }

        /// <summary>
        /// Gets the id of any sprite at the current lcdX and lcdY positions.
        /// </summary>
        /// <param name="memMap">The memory map</param>
        /// <returns>The index of the sprite in OAM, or -1 if no valid sprite.</returns>
        private ushort GetSpriteAddressAtCurrentLCDPosition(MemoryMap memMap)
        {
            for (int i = 0; i < objectIdsForLine.Count; i++)
            {
                ushort xPosAddress = (ushort)((MemoryMap.OAM_ADDRESS + (objectIdsForLine[i] * 4)) + 1);
                byte xStart = memMap.ReadDirect(xPosAddress);
                byte lcdXStart = (byte)(xStart - 8);
                if (lcdX == lcdXStart)
                    return (ushort)(MemoryMap.OAM_ADDRESS + (objectIdsForLine[i] * 4));
            }
            return 0;
        }

        private void TickHBlank(MemoryMap memMap)
        {
            if (currentState != PPUState.HBlank)
                currentState = PPUState.HBlank;
            if (initialHBlankLineTick && IsHBlankEnabled)
            {
                interruptHandler.SetInterruptRequested(InterruptHandler.InterruptType.LCDStatus, true);
                initialHBlankLineTick = false;
            }
            //HBlank purposely does pretty much nothing. It's blanking after all! We've just got to burn the ticks.
        }

        private void TickVBlank(MemoryMap memMap)
        {
            if (currentState != PPUState.VBlank)
                currentState = PPUState.VBlank;
            if (initialVBlankLineTick)
            {
                interruptHandler.SetInterruptRequested(InterruptHandler.InterruptType.VBlank, true); //It seems odd that there is both a dedicated VBlank interrupt, and an LCD status one. Possibly releated to STAT being repeatedly set.
                initialVBlankLineTick = false;
            }
            if (IsOAMCheckEnabled || IsVBlankCheckEnabled) //Fires every tick, not just once per line. Also, yup, it'll take VBlank or OAM being enabled apparently... This needs more research.
                interruptHandler.SetInterruptRequested(InterruptHandler.InterruptType.LCDStatus, true);
            //For the same reasons as HBlank, this does basically nothing but burn ticks.
        }

        /// <summary>
        /// Updates the LCD Status register to reflect the state of whether LY == LYC, and requests the interrupt if enabled.
        /// </summary>
        /// <param name="memMap">The current memory map</param>
        /// <param name="lineY">The value of Line Y to check.</param>
        private void ExecuteLineYCompare(MemoryMap memMap)
        {
            byte lineYCompare = memMap.Read(LineYCompareRegister);
            if (lcdY == lineYCompare)
            {
                byte lcdStatus = memMap.Read(LCDControlStatusRegister);
                byte stateBit = (byte)(lcdStatus & (1 << 2));
                lcdStatus = (byte)(lcdStatus | stateBit);
                memMap.Write(LCDControlStatusRegister, lcdStatus);
                if (IsLineYCompareEnabled)
                    interruptHandler.SetInterruptRequested(InterruptHandler.InterruptType.LCDStatus, true);
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
