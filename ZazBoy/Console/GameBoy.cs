using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ZazBoy.Console.Operations;

namespace ZazBoy.Console
{
    /// <summary>
    /// Class representation of the Game Boy, holds all the internal components and systems.
    /// </summary>
    public class GameBoy
    {
        private static GameBoy instance;
        public bool DEBUG_MODE { get; set; }
        public bool IsStepping { get; set; }
        public HashSet<ushort> Breakpoints { get; private set; }
        public bool IsPaused
        {
            get
            {
                return paused;
            }
            set
            {
                if (value && !paused)
                {
                    this.paused = true;
                }
                else if (!value && paused)
                {
                    this.paused = false;
                    onEmulatorResumed?.Invoke();
                    clockTimer.Start();
                }
            }
        }
        private bool paused;
        private Dictionary<ushort, List<InstructionOverride>> instructionOverrides;
        public delegate void PauseHandler(ushort programCounter);
        public event PauseHandler onEmulatorPaused;
        public delegate void ResumeHandler();
        public event ResumeHandler onEmulatorResumed;
        public delegate void PowerStateChangeHandler(bool powered);
        public event PowerStateChangeHandler onEmulatorPowerStateChanged;

        public bool IsPoweredOn { get; private set; }
        private byte[] cartridge;
        public MemoryMap MemoryMap { get; private set; }
        public CPU CPU { get; private set; }
        public InterruptHandler InterruptHandler { get; private set; }
        public Timer Timer { get; private set; }
        public PPU PPU { get; private set; }
        public LCD LCD { get; private set; }
        public Joypad Joypad { get; private set; }
        public bool IsDMATransferActive { get => dmatOperation != null && !dmatOperation.isComplete; }
        private DMATransferOperation dmatOperation;

        private System.Timers.Timer clockTimer;
        //The time to wait between process loops. High intervals increase input latency, lower intervals have a greater performance cost.
        private int clockInterval;
        private bool tickActive;

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
            Breakpoints = new HashSet<ushort>();
            instructionOverrides = new Dictionary<ushort, List<InstructionOverride>>();
#if DEBUG
            //DEBUG_MODE = true;
#else
            DEBUG_MODE = false;
#endif
        }

        /// <summary>
        /// Defines the power on state. Setting to true will enable the display and initialise internals, false will clear the display and null attributes.
        /// </summary>
        /// <param name="enablePower">New power state</param>
        public void SetPowerOn(bool enablePower)
        {
            if (enablePower && !IsPoweredOn)
            {
                this.IsPoweredOn = true;
                LCD = new LCD();
                MemoryMap = new MemoryMap(this, cartridge);
                InterruptHandler = new InterruptHandler(MemoryMap);
                Joypad = new Joypad(InterruptHandler);
                CPU = new CPU(this);
                Timer = new Timer(MemoryMap, InterruptHandler);
                PPU = new PPU(MemoryMap, InterruptHandler, LCD);

                clockInterval = 16; //Target FPS is 59.7 (16.75ms per frame). 16ms ensures we can acknowledge inputs every frame.
                clockTimer = new System.Timers.Timer(clockInterval);
                clockTimer.AutoReset = true;
                clockTimer.Elapsed += Tick;
                clockTimer.Enabled = true;
                IsPaused = false;
                onEmulatorPowerStateChanged?.Invoke(enablePower);
            }
            else if (!enablePower && IsPoweredOn)
            {
                this.IsPoweredOn = false;
                IsPaused = true;
                clockTimer.Stop();
                clockTimer.Dispose();
                LCD.SetDisplayPowered(false);
                LCD = null;
                MemoryMap = null;
                InterruptHandler = null;
                Joypad = null;
                CPU = null;
                Timer = null;
                PPU = null;
                clockTimer.Stop();
                clockTimer.Dispose();
                onEmulatorPowerStateChanged?.Invoke(enablePower);
            }
        }

        private void Tick(Object source, ElapsedEventArgs e)
        {
            try
            {
                //We don't want two "ticking" processes simultaniously, as it can cause invalid timings, desynchronisation, and unexpected behaviour.
                if (tickActive) //TODO: Some sort of catch-up mechanic? 
                    return;
                tickActive = true;
                double scaleFactor = clockInterval / 1000.0f;
                int tickRate = (int)(4194304.0f * scaleFactor);
                for (int i = 0; i < tickRate; i++)
                {
                    if (IsDMATransferActive)
                        dmatOperation.Tick();
                    bool opComplete = CPU.Tick();
                    PPU.Tick();
                    Timer.Tick();
                    if (opComplete)
                    {
                        if (IsStepping)
                        {
                            IsStepping = false;
                            IsPaused = true;
                        }
                        if (Breakpoints.Contains(CPU.programCounter))
                        {
                            paused = true;
                        }
                        if (paused)
                        {
                            clockTimer.Stop();
                            onEmulatorPaused?.Invoke(CPU.programCounter);
                            break;
                        }
                    }
                }
                tickActive = false;
            }
            catch (NullReferenceException nullEx)
            {
                System.Console.WriteLine("Encountered null during tick: " + nullEx.Message);
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

        /// <summary>
        /// Sets the Game Boy to boot the file specified in path.
        /// </summary>
        /// <param name="path">The file to load into memory.</param>
        /// <returns>bool on successful load</returns>
        public bool LoadCartridge(string path)
        {
            if (File.Exists(path))
            {
                cartridge = File.ReadAllBytes(path);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds an InstructionOverride to execute instead of the data found on the cartridge. If any existing override is set for the address and opcode, it is replaced with the new one.
        /// </summary>
        /// <param name="instructionOverride">The override to add.</param>
        public void AddInstructionOverride(InstructionOverride instructionOverride)
        {
            ushort address = instructionOverride.address;
            if (!instructionOverrides.ContainsKey(address))
                instructionOverrides.Add(address, new List<InstructionOverride>());

            InstructionOverride[] overrides = GetInstructionOverrides(address);
            for (int i = 0; i < overrides.Length; i++)
            {
                if (overrides[i].isOverridingInstruction(instructionOverride.overriddenPrefixed, instructionOverride.overriddenOpcode, instructionOverride.overriddenLowByte, instructionOverride.overriddenHighByte))
                    RemoveInstructionOverride(overrides[i]);
            }

            instructionOverrides[address].Add(instructionOverride);
        }

        /// <summary>
        /// Removes the specified instruction override, reverting execution to data found on the loaded cartridge.
        /// </summary>
        /// <param name="instructionOverride">The override to remove.</param>
        public void RemoveInstructionOverride(InstructionOverride instructionOverride)
        {
            ushort address = instructionOverride.address;
            if (instructionOverrides.ContainsKey(address))
                instructionOverrides[address].Remove(instructionOverride);
        }

        /// <summary>
        /// Gets all the overrides for the specified address.
        /// </summary>
        /// <param name="address">The memory location exrcution occurs at. (For prefixed instructions, this includes the prefix)</param>
        /// <returns>The overrides for this address.</returns>
        public InstructionOverride[] GetInstructionOverrides(ushort address)
        {
            if (instructionOverrides.ContainsKey(address))
                return instructionOverrides[address].ToArray();
            return null;
        }
    }
}

