﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZazBoy.Console
{
    /// <summary>
    /// A class representing the memory mapping system, granting controlled access to various memory locations and hardware registers
    /// </summary>
    public class MemoryMap
    {
        //Starting locations for map regions
        public const ushort CARTRIDGE_ADDRESS = 0x0000;
        public const ushort VRAM_ADDRESS = 0x8000;
        public const ushort EXRAM_ADDRESS = 0xA000;
        public const ushort WRAM_ADDRESS = 0xC000;
        public const ushort PROHIBITED_ADDRESS = 0xE000;
        public const ushort OAM_ADDRESS = 0xFE00;
        public const ushort UNUSED_ADDRESS = 0xFEA0;
        public const ushort IO_ADDRESS = 0xFF00;
        public const ushort HRAM_ADDRESS = 0xFF80;
        public const ushort INTERRUPT_ENABLE_ADDRESS = 0xFFFF;

        public const ushort DMA_ADDRESS = 0xFF46;

        //Memory arrays
        private byte[] cartridge;
        private byte[] vram;
        private byte[] exram;
        private byte[] wram;
        private byte[] oam;
        private byte[] io;
        private byte[] hram;
        private byte interruptEnable;

        /// <summary>
        /// Initialises the memory banks and returns a new MemoryMap.
        /// </summary>
        public MemoryMap(byte[] cartridge)
        {
            if (cartridge.Length != 32768)
                throw new ArgumentException("Cartridge has invalid length! (Is it valid Game Boy software?");
            this.cartridge = cartridge;
            vram = new byte[8192];
            exram = new byte[8192];
            wram = new byte[8192];
            oam = new byte[160];
            io = new byte[128];
            hram = new byte[127];
            interruptEnable = 0;
        }

        /// <summary>
        /// Reads a byte stored at the specified memory address.<br></br>
        /// If no value has been set, this will return 0, but a return of 0 does not mean no value has been set, as 0 may be an intentional value.
        /// </summary>
        /// <param name="address">The memory address to get data from (0-65535)</param>
        /// <exception cref="IndexOutOfRangeException">Attempted to access address below 0, or above 65535</exception>
        /// <returns>The byte stored at the requested memory location.</returns>
        public byte Read(ushort address)
        {
            if (GameBoy.Instance().IsDMATransferActive && address >= CARTRIDGE_ADDRESS && address < UNUSED_ADDRESS) //DMAT blocks Cart, VRAM, ExRAM, WRAM and OAM while active.
                return 0xFF;

            if (address >= 0 && address < VRAM_ADDRESS)
            {
                return cartridge[address];
            }
            else if (address >= VRAM_ADDRESS && address < EXRAM_ADDRESS)
            {
                PPU ppu = GameBoy.Instance().PPU;
                if (ppu.currentState == PPU.PPUState.PixelTransfer)
                    return 0xFF;
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
                PPU ppu = GameBoy.Instance().PPU;
                if (ppu.currentState == PPU.PPUState.OAMSearch || ppu.currentState == PPU.PPUState.PixelTransfer)
                    return 0xFF;
                int index = (address - OAM_ADDRESS);
                return oam[index];
            }
            else if (address >= UNUSED_ADDRESS && address < IO_ADDRESS)
            {
                return byte.MaxValue; //No function is mapped to this location, so it always returns #FF
            }
            else if (address >= IO_ADDRESS && address < HRAM_ADDRESS)
            {
                if (address == 0xFF00)
                    return GameBoy.Instance().Joypad.GetControlsByte();
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
        public void Write(ushort address, byte data)
        {
            if (GameBoy.Instance().IsDMATransferActive && address >= CARTRIDGE_ADDRESS && address < UNUSED_ADDRESS) //DMAT blocks Cart, VRAM, ExRAM, WRAM and OAM while active.
                return;

            if (address >= 0 && address < VRAM_ADDRESS)
            {
                //Do nothing; cartridge is read-only.
                //TODO: Use for memory bank flags later on.
            }
            else if (address >= VRAM_ADDRESS && address < EXRAM_ADDRESS)
            {
                PPU ppu = GameBoy.Instance().PPU;
                if (ppu.currentState == PPU.PPUState.PixelTransfer)
                    return;
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
                PPU ppu = GameBoy.Instance().PPU;
                if (ppu.currentState == PPU.PPUState.OAMSearch || ppu.currentState == PPU.PPUState.PixelTransfer)
                    return;
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
                if (address == Timer.DividerRegister)
                {
                    io[index] = 0;
                    GameBoy.Instance().Timer.ResetDivider();
                }
                else if (address == Timer.TimerControl)
                {
                    Timer timer = GameBoy.Instance().Timer;
                    bool dividerBitSet = timer.IsDividerTimerFrequencyBitSet();
                    io[index] = data;
                    if (dividerBitSet && !timer.IsDividerTimerFrequencyBitSet())
                        timer.IncrementTimerCounter();
                }
                else if (address == Timer.TimerCounter)
                {
                    Timer timer = GameBoy.Instance().Timer;
                    if (timer.isOverflowCycle)
                        timer.isTIMAModifiedDuringOverflow = true;
                    io[index] = data;
                }
                else if (address == DMA_ADDRESS)
                {
                    if (!GameBoy.Instance().IsDMATransferActive)
                        GameBoy.Instance().InitiateDMATransfer(data);
                    io[index] = data;
                }
                else if (address == PPU.LCDControlStatusRegister)
                {
                    byte writableBits = (byte)(data & 0xF8); //1111 1000
                    byte readonlyBits = (byte)(io[index] & 0x07); //0000 0111
                    byte finalData = (byte)(writableBits | readonlyBits);
                    io[index] = finalData;
                }
                else if (address == InterruptHandler.InterruptFlagRegister)
                {
                    data = (byte)(data & 0x1F);
                    io[index] = data;
                }
                else if (address == Joypad.JoypadRegister)
                {
                    Joypad joypad = GameBoy.Instance().Joypad;
                    bool actionButtonsSet = (data & (1 << 5)) != 0;
                    bool dPadSet = (data & (1 << 4)) != 0;
                    joypad.ActionButtonsSelected = !actionButtonsSet;   //It's 0 to select, 1 to deselect
                    joypad.DirectionalPadSelected = !dPadSet;
                }
                else
                    io[index] = data;
            }
            else if (address >= HRAM_ADDRESS && address < INTERRUPT_ENABLE_ADDRESS)
            {
                int index = (address - HRAM_ADDRESS);
                hram[index] = data;
            }
            else if (address == INTERRUPT_ENABLE_ADDRESS)
            {
                data = (byte)(data & 0x1F);
                interruptEnable = data;
            }
            else
                throw new IndexOutOfRangeException("Attempted to access memory location outside of map: " + address); //Should never throw due to ushort limitations.
        }

        /// <summary>
        /// Forcibly reads a byte stored at the specified memory address, with no regard to locks or access restrictions.<br></br>
        /// If no value has been set, this will return 0, but a return of 0 does not mean no value has been set, as 0 may be an intentional value.
        /// </summary>
        /// <param name="address">The memory address to get data from (0-65535)</param>
        /// <exception cref="IndexOutOfRangeException">Attempted to access address below 0, or above 65535</exception>
        /// <returns>The byte stored at the requested memory location.</returns>
        public byte ReadDirect(ushort address)
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
                if (address == 0xFF00)
                    return GameBoy.Instance().Joypad.GetControlsByte();
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
        /// Writes a byte to the specified memory address, with no regard to locks or access restrictions. Also allows for writing to cart.
        /// </summary>
        /// <param name="address">The memory address to save data to (0-65535)</param>
        /// <param name="data">The data to write.</param>
        /// <exception cref="IndexOutOfRangeException">Attempted to access address below 0, or above 65535</exception>
        /// <returns></returns>
        public void WriteDirect(ushort address, byte data)
        {
            if (address >= 0 && address < VRAM_ADDRESS)
            {
                int index = (address - CARTRIDGE_ADDRESS);
                cartridge[index] = data;
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
                if (address == Joypad.JoypadRegister)
                {
                    Joypad joypad = GameBoy.Instance().Joypad;
                    bool actionButtonsSet = (data & (1 << 5)) != 0;
                    bool dPadSet = (data & (1 << 4)) != 0;
                    joypad.ActionButtonsSelected = !actionButtonsSet;   //It's 0 to select, 1 to deselect
                    joypad.DirectionalPadSelected = !dPadSet;
                }
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
