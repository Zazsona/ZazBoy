using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZazBoy.Console
{
    public partial class PPU
    {
        /// <summary>
        /// Gets a byte of data from a tile.
        /// </summary>
        /// <param name="tileAddress">The memory address of the tile to get a byte from</param>
        /// <param name="byteIndex">The index of the byte within the tile (0-15)</param>
        /// <returns>The requested byte</returns>
        private byte GetTileByte(ushort tileAddress, byte byteIndex)
        {
            if (byteIndex > 15 || byteIndex < 0)
                throw new ArgumentOutOfRangeException("Byte Index out of range. (0-15)");
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            ushort tileByteAddress = (ushort)(tileAddress + byteIndex);
            return memMap.ReadDirect(tileByteAddress);
        }

        /// <summary>
        /// Merges pixel low and high data to produced a Pixel object.
        /// </summary>
        /// <param name="lowByte">The lower bits of the pixel</param>
        /// <param name="highByte">The higher bits of the pixel</param>
        /// <param name="palette">The colour palette to associate with the pixel</param>
        /// <param name="prioritySet">Whether OBJ-To-BG is set.</param>
        /// <returns>A Pixel</returns>
        private Pixel[] GetPixels(byte lowByte, byte highByte, byte palette, bool prioritySet)
        {
            Pixel[] pixels = new Pixel[8];
            for (int i = 7; i > -1; i--)
            {
                byte bitMask = ((byte)(1 << i));
                byte highBit = (byte)(((highByte & bitMask) == 0) ? 0x00 : 0x02); //0000 0000 or 0000 0010
                byte lowBit = (byte)(((lowByte & bitMask) == 0) ? 0x00 : 0x01); //0000 0000 or 0000 0001
                byte colourByte = (byte)(highBit | lowBit);
                pixels[i] = new Pixel(colourByte, palette, prioritySet);
            }
            return pixels;
        }

        /// <summary>
        /// Fetcher component of the PPU, responsible for parsing the tile map and grabbing tile data
        /// </summary>
        private class PPUFetcher
        {
            public FetcherState fetcherState { get; private set; }

            private PPU ppu;
            private int cycleTicks;
            private Pixel[] pixelsToPush;
            private ushort currentTileAddress;
            private byte currentLowByte;
            private byte currentHighByte;
            private byte currentFetcherX = 0;

            public PPUFetcher(PPU ppu)
            {
                this.ppu = ppu;
                ResetCycle();
            }

            public void Tick(byte lcdX, byte lcdY)
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                if (cycleTicks == 0)
                {
                    fetcherState = FetcherState.GetTileNumber;
                    ushort mapTileAddress = GetBackgroundMapTileAddress(memMap, lcdX, lcdY);
                    byte tileNumber = memMap.ReadDirect(mapTileAddress);
                    currentTileAddress = GetBackgroundTileAddress(tileNumber, false);
                }
                else if (cycleTicks == 2)
                {
                    fetcherState = FetcherState.GetTileLowByte;
                    byte lowByteIndex = (byte)((lcdY % 8) * 2);
                    currentLowByte = ppu.GetTileByte(currentTileAddress, lowByteIndex);
                }
                else if (cycleTicks == 4)
                {
                    fetcherState = FetcherState.GetTileHighByte;
                    byte highByteIndex = (byte)(((lcdY % 8) * 2) + 1);
                    currentHighByte = ppu.GetTileByte(currentTileAddress, highByteIndex);
                }
                else if (cycleTicks == 6)
                {
                    fetcherState = FetcherState.Idle;
                }
                else if (cycleTicks >= 8)
                {
                    fetcherState = FetcherState.Push;
                    if (cycleTicks == 8)
                    {
                        byte palette = memMap.ReadDirect(BackgroundPaletteRegister);
                        pixelsToPush = ppu.GetPixels(currentLowByte, currentHighByte, palette, false);

                        currentFetcherX++;
                        currentFetcherX = (byte)(currentFetcherX & 0x1F);
                    }
                    bool pixelsPushed = ppu.PushPixelsToBackgroundQueue(pixelsToPush);
                    if (pixelsPushed)
                    {
                        ResetCycle();
                        return;
                    }
                }
                cycleTicks++;
            }

            /// <summary>
            /// Gets the memory address the points to the co-ordinate map, holding the index for the tile at the current X/Y position.
            /// </summary>
            /// <param name="memMap">Context</param>
            /// <param name="lcdX">X Position</param>
            /// <param name="lcdY">Y Position</param>
            /// <returns>Memory address pointing to a location in tile maps, holding the index of the tile for the specified position</returns>
            private ushort GetBackgroundMapTileAddress(MemoryMap memMap, byte lcdX, byte lcdY)
            {
                byte mapX = 0;
                byte mapY = 0;
                ushort mapStart = 0x9800;
                if (IsWindowOverride(memMap, lcdX, lcdY))
                {
                    mapX = (byte)(lcdX - memMap.ReadDirect(PPU.WindowXRegister));
                    mapY = (byte)(lcdY - memMap.ReadDirect(PPU.WindowYRegister));
                    if (ppu.IsWindowTileMapStart9C00)
                        mapStart = 0x9C00;
                }
                else
                {
                    mapX = (byte)(((memMap.ReadDirect(PPU.ScrollXRegister) / 8) + currentFetcherX) & 0x1F);
                    mapY = (byte)((lcdY + memMap.ReadDirect(PPU.ScrollYRegister)) & 0xFF);
                    if (ppu.IsBGTileMapStart9C00)
                        mapStart = 0x9C00;
                }
                return (ushort)(mapStart + mapX + mapY / 8 * 32);
            }

            /// <summary>
            /// Gets the memory address of a tile from its index in the map, respecting the start position of tile data.
            /// </summary>
            /// <param name="tileIndex">The index of the tile (-128-255)</param>
            /// <param name="isObject">Whether this is a sprite fetch.</param>
            /// <returns>The memory address of the tile.</returns>
            private ushort GetBackgroundTileAddress(byte tileIndex, bool isObject)
            {
                int tileIndexInt = (ppu.IsBGTileDataStart8000) ? tileIndex : (sbyte)tileIndex; //0x8000 start uses unsigned addresses, 0x8800 uses signed addressing.
                if (tileIndexInt > 255 || tileIndexInt < -128)
                    throw new ArgumentOutOfRangeException("Tile Index out of range. (-128-255)");
                ushort dataStartAddress = (ushort)((ppu.IsBGTileDataStart8000 || isObject) ? 0x8000 : 0x8800);
                ushort tileAddress = (ushort)(dataStartAddress + (tileIndexInt * 16));
                return tileAddress;
            }

            /// <summary>
            /// Checks if the Window is active at the specified position.
            /// </summary>
            /// <param name="memMap">Context</param>
            /// <param name="lcdX">X Position</param>
            /// <param name="lcdY">Y Position</param>
            /// <returns>True if Window is enabled at the current position.</returns>
            private bool IsWindowOverride(MemoryMap memMap, byte lcdX, byte lcdY)
            {
                byte windowX = memMap.ReadDirect(PPU.WindowXRegister);
                byte windowY = memMap.ReadDirect(PPU.WindowYRegister);
                return (windowX <= lcdX && windowY <= lcdY && ppu.IsWindowEnabled);
            }

            /// <summary>
            /// Resets all line-relevant data for the fetcher (Use when starting a new line)
            /// </summary>
            public void ResetCycle()
            {
                fetcherState = FetcherState.GetTileNumber;
                cycleTicks = 0;
                pixelsToPush = null;
            }

            /// <summary>
            /// Resets all data for the fetcher.
            /// </summary>
            public void Reset()
            {
                ResetCycle();
                currentFetcherX = 0;
                currentHighByte = 0;
                currentLowByte = 0;
                currentTileAddress = 0;
            }

            /// <summary>
            /// The states the fetcher can be in
            /// </summary>
            public enum FetcherState
            {
                GetTileNumber,
                GetTileLowByte,
                GetTileHighByte,
                Idle,
                Push
            }
        }
    }
}
