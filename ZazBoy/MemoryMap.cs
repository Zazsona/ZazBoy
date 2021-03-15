using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy
{
    /// <summary>
    /// A class representing the memory mapping system, granting controlled access to various memory locations and hardware registers
    /// </summary>
    public static class MemoryMap
    {
        //Starting locations for map regions
        public const ushort CARTRIDGE_ADDRESS = 0; //#0000
        public const ushort VRAM_ADDRESS = 32768; //#8000
        public const ushort EXRAM_ADDRESS = 40960; //#A000
        public const ushort WRAM_ADDRESS = 49152; //#C000
        public const ushort PROHIBITED_ADDRESS = 57344; //#E000
        public const ushort OAM_ADDRESS = 65024; //#FE00
        public const ushort UNUSED_ADDRESS = 65184; //#FEA0
        public const ushort IO_ADDRESS = 65280; //#FF00
        public const ushort HRAM_ADDRESS = 65408; //#FF80
        public const ushort INTERRUPT_ENABLE_ADDRESS = 65535; //#FFFF

        //Memory arrays
        private static byte[] cartridge = new byte[32768];
        private static byte[] vram = new byte[8192];
        private static byte[] exram = new byte[8192];
        private static byte[] wram = new byte[8192];
        private static byte[] oam = new byte[160];
        private static byte[] io = new byte[128];
        private static byte[] hram = new byte[127];
        private static byte interruptEnable;

        /// <summary>
        /// Reads a byte stored at the specified memory address.<br></br>
        /// If no value has been set, this will return 0, but a return of 0 does not mean no value has been set, as 0 may be an intentional value.
        /// </summary>
        /// <param name="address">The memory address to get data from (0-65535)</param>
        /// <exception cref="IndexOutOfRangeException">Attempted to access address below 0, or above 65535</exception>
        /// <returns>The byte stored at the requested memory location.</returns>
        public static byte Read(ushort address)
        {
            if (address >= 0 && address < VRAM_ADDRESS)
            {
                return cartridge[address];
            }
            else if (address >= VRAM_ADDRESS && address < EXRAM_ADDRESS)
            {
                int index = (address - VRAM_ADDRESS);
                return vram[index];
            }
            else if (address >= EXRAM_ADDRESS && address < WRAM_ADDRESS)
            {
                int index = (address - EXRAM_ADDRESS);
                return exram[index];
            }
            else if (address >= WRAM_ADDRESS && address < PROHIBITED_ADDRESS)
            {
                int index = (address - WRAM_ADDRESS);
                return wram[index];
            }
            else if (address >= PROHIBITED_ADDRESS && address < OAM_ADDRESS)
            {
                int index = (address - PROHIBITED_ADDRESS);
                return wram[index]; //Intentional, for some reason inherent to the Game Boy, the prohibited addresses mirror WRAM.
            }
            else if (address >= OAM_ADDRESS && address < UNUSED_ADDRESS)
            {
                int index = (address - OAM_ADDRESS);
                return oam[index];
            }
            else if (address >= UNUSED_ADDRESS && address < IO_ADDRESS)
            {
                return byte.MaxValue; //No function is mapped to this location, so it always returns #FF
            }
            else if (address >= IO_ADDRESS && address < HRAM_ADDRESS)
            {
                int index = (address - IO_ADDRESS);
                return io[index];
            }
            else if (address >= HRAM_ADDRESS && address < INTERRUPT_ENABLE_ADDRESS)
            {
                int index = (address - HRAM_ADDRESS);
                return hram[index];
            }
            else if (address == INTERRUPT_ENABLE_ADDRESS)
            {
                return interruptEnable;
            }
            else
                throw new IndexOutOfRangeException("Attempted to access memory location outside of map: " + address); //Should never throw due to ushort limitations.
        }

        /// <summary>
        /// Writes a byte to the specified memory address, as long as it is accessible.
        /// </summary>
        /// <param name="address">The memory address to save data to (0-65535)</param>
        /// <param name="data">The data to write.</param>
        /// <exception cref="IndexOutOfRangeException">Attempted to access address below 0, or above 65535</exception>
        /// <returns></returns>
        public static void Write(ushort address, byte data)
        {
            if (address >= 0 && address < VRAM_ADDRESS)
            {
                //Do nothing; cartridge is read-only.
                //TODO: Use for memory bank flags later on.
            }
            else if (address >= VRAM_ADDRESS && address < EXRAM_ADDRESS)
            {
                int index = (address - VRAM_ADDRESS);
                vram[index] = data;
            }
            else if (address >= EXRAM_ADDRESS && address < WRAM_ADDRESS)
            {
                int index = (address - EXRAM_ADDRESS);
                exram[index] = data;
            }
            else if (address >= WRAM_ADDRESS && address < PROHIBITED_ADDRESS)
            {
                int index = (address - WRAM_ADDRESS);
                wram[index] = data;
            }
            else if (address >= PROHIBITED_ADDRESS && address < OAM_ADDRESS)
            {
                int index = (address - PROHIBITED_ADDRESS);
                wram[index] = data; //Intentional, for some reason inherent to the Game Boy, the prohibited addresses mirror WRAM.
            }
            else if (address >= OAM_ADDRESS && address < UNUSED_ADDRESS)
            {
                int index = (address - OAM_ADDRESS);
                oam[index] = data;
            }
            else if (address >= UNUSED_ADDRESS && address < IO_ADDRESS)
            {
                //Unused addresses, so do nothing.
            }
            else if (address >= IO_ADDRESS && address < HRAM_ADDRESS)
            {
                int index = (address - IO_ADDRESS);
                io[index] = data;
            }
            else if (address >= HRAM_ADDRESS && address < INTERRUPT_ENABLE_ADDRESS)
            {
                int index = (address - HRAM_ADDRESS);
                hram[index] = data;
            }
            else if (address == INTERRUPT_ENABLE_ADDRESS)
            {
                interruptEnable = data;
            }
            else
                throw new IndexOutOfRangeException("Attempted to access memory location outside of map: " + address); //Should never throw due to ushort limitations.
        }
    }
}
