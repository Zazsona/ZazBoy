using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZazBoy.Console.Operations;

namespace ZazBoy.Console
{
    /// <summary>
    /// Class representation of the Game Boy, holds all the internal components and systems.
    /// </summary>
    public class GameBoy
    {
        private static GameBoy instance;
        public bool IsPoweredOn { get; private set; }
        public MemoryMap MemoryMap { get; private set; }
        public CPU CPU { get; private set; }
        public InterruptHandler InterruptHandler { get; private set; }
        public Timer Timer { get; private set; }
        public PPU PPU { get; private set; }
        public LCD LCD { get; private set; }
        public bool IsDMATransferActive { get => dmatOperation != null && !dmatOperation.isComplete; }
        private DMATransferOperation dmatOperation;

        /// <summary>
        /// Gets or creates the active Game Boy
        /// </summary>
        /// <returns>The Game Boy instance.</returns>
        public static GameBoy Instance()
        {
            if (instance == null)
                instance = new GameBoy();
            return instance;
        }

        /// <summary>
        /// Private constructor to force use of Instance()
        /// </summary>
        private GameBoy()
        {

        }

        /// <summary>
        /// Defines the power on state. Setting to true will enable the display and initialise internals, false will clear the display and null attributes.
        /// </summary>
        /// <param name="enablePower">New power state</param>
        public void SetPowerOn(bool enablePower)
        {
            this.IsPoweredOn = enablePower;
            if (enablePower)
            {
                byte[] cartridge = LoadCartridge();
                MemoryMap = new MemoryMap(cartridge);
                CPU = new CPU();
                InterruptHandler = new InterruptHandler();
                Timer = new Timer();
                PPU = new PPU();
                LCD = new LCD();
                while (true)
                {
                    if (IsDMATransferActive)
                        dmatOperation.Tick();
                    CPU.Tick();
                    Timer.Tick();
                    PPU.Tick();
                    Thread.Sleep(50);
                }
            }
            else
            {
                MemoryMap = null;
                CPU = null;
            }
        }

        /// <summary>
        /// Initiates and executes an OAM DMA Transfer, provided the device is powered on & no transfer is active.
        /// </summary>
        /// <param name="sourceAddressMSB">The most significant byte for the start address of the shadow OEM. Transfer will go from 0xXX00-0xXX9F</param>
        /// <exception cref="InvalidOperationException">Device not powered, or a DMAT is already active.</exception>
        public void InitiateDMATransfer(byte sourceAddressMSB)
        {
            if (!IsPoweredOn)
                throw new InvalidOperationException("You must power on first!");
            if (IsDMATransferActive)
                throw new InvalidOperationException("A DMA Transfer is already active!");
            ushort startAddress = (ushort)(sourceAddressMSB * 0x100);
            dmatOperation = new DMATransferOperation(startAddress);
        }

        private byte[] LoadCartridge()
        {
            string path = "C:\\cartridge.gb"; //TODO: Temp. Display a file select UI
            byte[] cartData;
            if (File.Exists(path))
                cartData = File.ReadAllBytes(path);
            else
                cartData = new byte[32768];
            return cartData;
        }
    }
}
