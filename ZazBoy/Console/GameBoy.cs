using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                while (true)
                {
                    CPU.Tick();
                    Thread.Sleep(50);
                }
            }
            else
            {
                MemoryMap = null;
                CPU = null;
            }
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
