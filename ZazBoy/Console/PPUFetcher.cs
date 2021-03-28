using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZazBoy.Console
{
    /// <summary>
    /// Fetcher component of the PPU, responsible for parsing the tile map and grabbing tile data
    /// </summary>
    public class PPUFetcher
    {
        public FetcherState fetcherState { get; private set; }
        public Pixel[] pixelsToPush { get; private set; }

        private PPU ppu;
        private int cycleTicks;

        private ushort currentTileAddress;
        private byte currentLowByte;
        private byte currentHighByte;

        private byte currentFetcherX;

        public PPUFetcher(PPU ppu)
        {
            this.ppu = ppu;
            Reset();
        }

        public void Tick(byte lineX, byte lineY)
        {
            //TODO: Windows
            //TODO: Sprites
            //TODO: Properly advance the tile map
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            if (cycleTicks == 0)
            {
                fetcherState = FetcherState.GetTileNumber;
                ushort mapTileAddress = GetBackgroundMapTileAddress(memMap, lineY);
                byte tileNumber = memMap.ReadDirect(mapTileAddress);
                currentTileAddress = GetTileAddress(tileNumber, false); //TODO: Check if object.
            }
            else if (cycleTicks == 2)
            {
                fetcherState = FetcherState.GetTileLowByte;
                byte lowByteIndex = (byte)((lineY % 8) * 2);
                currentLowByte = GetTileByte(currentTileAddress, lowByteIndex);
            }
            else if (cycleTicks == 4)
            {
                fetcherState = FetcherState.GetTileHighByte;
                byte highByteIndex = (byte)(((lineY % 8) * 2)+1);
                currentHighByte = GetTileByte(currentTileAddress, highByteIndex);
                pixelsToPush = GetBackgroundPixels(currentLowByte, currentHighByte);
            }
            else if (cycleTicks == 6)
            {
                fetcherState = FetcherState.Idle; 
            }
            else if (cycleTicks >= 8 && true) //TODO: && if the queue has space.
            {
                fetcherState = FetcherState.Push;
                if (cycleTicks == 8)
                {
                    currentFetcherX++;
                    if (currentFetcherX > 31)
                        currentFetcherX = 0;
                }
            }
            cycleTicks++;
        }

        private ushort GetBackgroundMapTileAddress(MemoryMap memMap, byte lineY)
        {
            byte mapX = (byte)(((memMap.ReadDirect(PPU.ScrollXRegister) / 8) + currentFetcherX) & 0x1F);
            byte mapY = (byte)(lineY + memMap.ReadDirect(PPU.ScrollYRegister));
            ushort mapStart = (ushort)((ppu.IsBGTileMapStart9C00 && ppu.IsWindowTileMapStart9C00) ? 0x9C00 : 0x9800);
            return (ushort)(mapStart + mapX + mapY / 8 * 32);
        }

        private Pixel[] GetBackgroundPixels(byte lowByte, byte highByte)
        {
            Pixel[] pixels = new Pixel[8];
            for (int i = 7; i>-1; i--)
            {
                byte bitMask = ((byte)(1 << i));
                byte highBit = (byte)(((highByte & bitMask) == 0) ? 0x00 : 0x02); //0000 0000 or 0000 0010
                byte lowBit = (byte)(((lowByte & bitMask) == 0) ? 0x00 : 0x01); //0000 0000 or 0000 0001
                byte colourByte = (byte)(highBit | lowBit);
                byte paletteByte = GameBoy.Instance().MemoryMap.ReadDirect(PPU.BackgroundPaletteRegister);
                pixels[i] = new Pixel(colourByte, paletteByte, 0);
            }
            return pixels;
        }

        /// <summary>
        /// Gets the memory address of a tile from its index in the map, respecting the start position of tile data.
        /// </summary>
        /// <param name="tileIndex">The index of the tile (-128-255)</param>
        /// <param name="isObject">Whether this is a sprite fetch.</param>
        /// <returns>The memory address of the tile.</returns>
        private ushort GetTileAddress(byte tileIndex, bool isObject)
        {
            int tileIndexInt = (ppu.IsBGTileDataStart8000) ? tileIndex : (sbyte)tileIndex; //0x8000 start uses unsigned addresses, 0x8800 uses signed addressing.
            if (tileIndexInt > 255 || tileIndexInt < -128)
                throw new ArgumentOutOfRangeException("Tile Index out of range. (-128-255)");
            ushort dataStartAddress = (ushort)((ppu.IsBGTileDataStart8000 || isObject) ? 0x8000 : 0x8800);
            ushort tileAddress = (ushort)(dataStartAddress + (tileIndexInt*16));
            return tileAddress;
        }

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

        public void Reset()
        {
            fetcherState = FetcherState.GetTileNumber;
            cycleTicks = 0;
            pixelsToPush = null;
        }

        private void ProgressState()
        {
            switch (fetcherState)
            {
                case FetcherState.GetTileNumber:
                    fetcherState = FetcherState.GetTileLowByte;
                    break;
                case FetcherState.GetTileLowByte:
                    fetcherState = FetcherState.GetTileHighByte;
                    break;
                case FetcherState.GetTileHighByte:
                    fetcherState = FetcherState.Idle;
                    break;
                case FetcherState.Idle:
                    fetcherState = FetcherState.Push;
                    break;
                case FetcherState.Push:
                    fetcherState = FetcherState.GetTileNumber;
                    break;
            }
        }

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
