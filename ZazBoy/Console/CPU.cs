using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZazBoy.Console.Instructions;
using ZazBoy.Console.Operations;
using static ZazBoy.Console.InterruptHandler;

namespace ZazBoy.Console
{
    /// <summary>
    /// Sharp LR35902 CPU emulator
    /// </summary>
    public class CPU
    {
        public byte registerA { get; set; }
        public byte registerB { get; set; }
        public byte registerC { get; set; }
        public byte registerD { get; set; }
        public byte registerE { get; set; }
        public byte registerF { get; set; }
        public byte registerH { get; set; }
        public byte registerL { get; set; }
        public ushort programCounter { get; set; }
        public ushort stackPointer { get; set; }

        public ushort registersAF
        {
            get
            {
                ushort value = (ushort)(registerA * 0x100);
                value += registerF;
                return value;
            }
            set
            {
                registerA = (byte)(value / 0x100);
                registerF = ((byte)(value & 0x00FF));
            }
        }
        public ushort registersBC
        {
            get
            {
                ushort value = (ushort)(registerB * 0x100);
                value += registerC;
                return value;
            }
            set
            {
                registerB = (byte)(value / 0x100);
                registerC = ((byte)(value & 0x00FF));
            }
        }
        public ushort registersDE
        {
            get
            {
                ushort value = (ushort)(registerD * 0x100);
                value += registerE;
                return value;
            }
            set
            {
                registerD = (byte)(value / 0x100);
                registerE = ((byte)(value & 0x00FF));
            }
        }
        public ushort registersHL
        {
            get
            {
                ushort value = (ushort)(registerH * 0x100);
                value += registerL;
                return value;
            }
            set
            {
                registerH = (byte)(value / 0x100);
                registerL = ((byte)(value & 0x00FF));
            }
        }

        public bool zeroFlag
        {
            get
            {
                byte flagBit = (1 << 7);
                return ((registerF & flagBit) != 0); //Test bit is true
            }
            set
            {
                bool isFlagTrue = zeroFlag;
                if (isFlagTrue != value)
                {
                    byte flagBit = (1 << 7);
                    registerF = (byte)(registerF ^ flagBit); //XOR the bit.
                }
            }
        }
        public bool subtractionFlag
        {
            get
            {
                byte flagBit = (1 << 6);
                return ((registerF & flagBit) != 0); //Test bit is true
            }
            set
            {
                bool isFlagTrue = subtractionFlag;
                if (isFlagTrue != value)
                {
                    byte flagBit = (1 << 6);
                    registerF = (byte)(registerF ^ flagBit); //XOR the bit.
                }
            }
        }
        public bool halfCarryFlag
        {
            get
            {
                byte flagBit = (1 << 5);
                return ((registerF & flagBit) != 0); //Test bit is true
            }
            set
            {
                bool isFlagTrue = halfCarryFlag;
                if (isFlagTrue != value)
                {
                    byte flagBit = (1 << 5);
                    registerF = (byte)(registerF ^ flagBit); //XOR the bit.
                }
            }
        }
        public bool carryFlag
        {
            get
            {
                byte flagBit = (1 << 4);
                return ((registerF & flagBit) != 0); //Test bit is true
            }
            set
            {
                bool isFlagTrue = carryFlag;
                if (isFlagTrue != value)
                {
                    byte flagBit = (1 << 4);
                    registerF = (byte)(registerF ^ flagBit); //XOR the bit.
                }
            }
        }

        public bool haltRepeatBugActive { get; set; }
        public bool delayedEIBugActive { get; set; }
        private InstructionFactory instructionFactory;
        private Operation activeOperation;

        private GameBoy gameBoy;
        private MemoryMap memMap;
        private InterruptHandler interruptHandler;

        /// <summary>
        /// Creates a new instance of the CPU, setting register values to match Game Boy boot defaults.
        /// </summary>
        public CPU(GameBoy gameBoy)
        {
            this.gameBoy = gameBoy;
            this.memMap = gameBoy.MemoryMap;
            this.interruptHandler = gameBoy.InterruptHandler;

            registerA = 0x01;
            registerB = 0x00;
            registerC = 0x13;
            registerD = 0x00;
            registerE = 0xD8;
            registerF = 0xB0;
            registerH = 0x01;
            registerL = 0x4D;
            programCounter = 0x0100;
            stackPointer = 0xFFFE;

            instructionFactory = new InstructionFactory();
            activeOperation = null;
        }

        /// <summary>
        /// Progresses the CPU by one clock
        /// </summary>
        /// <returns>True if the current operation was an instruction and completed this tick.</returns>
        public bool Tick()
        {
            if (activeOperation == null)
            {
                bool interrupted = CheckInterrupts();
                if (!interrupted)
                {
                    byte opcode = memMap.Read(programCounter);
                    InstructionOverride instructionOverride = GetOverridingInstruction(programCounter);
                    if (opcode == Instruction.BitwiseInstructionPrefix)
                    {
                        IncrementProgramCounter();
                        opcode = (instructionOverride == null) ? memMap.Read(programCounter) : instructionOverride.opcode;
                        activeOperation = instructionFactory.GetPrefixedInstruction(opcode);
                    }
                    else
                    {
                        opcode = (instructionOverride == null) ? opcode : instructionOverride.opcode;
                        activeOperation = instructionFactory.GetInstruction(opcode);
                    }
                    if (activeOperation is Instruction)
                        ((Instruction)activeOperation).overrideContext = instructionOverride;
                        
                    if (activeOperation == null)
                        throw new InvalidOperationException("Value \"" + opcode + "\" is not a valid opcode.");
                    IncrementProgramCounter();
                }
            }
            activeOperation.Tick();
            if (activeOperation.isComplete)
            {
                if (delayedEIBugActive && activeOperation.GetType() != typeof(EnableInterruptsInstruction))
                {
                    interruptHandler.interruptMasterEnable = true;
                    delayedEIBugActive = false;
                }


                bool isInstruction = (activeOperation is Instruction);
                if (isInstruction)
                {
                    Instruction completedInstruction = (Instruction)activeOperation;
                    if (completedInstruction.isOverride)
                    {
                        InstructionOverride overrideContext = completedInstruction.overrideContext;
                        Instruction originalInstruction = (overrideContext.overriddenPrefixed) ? instructionFactory.GetPrefixedInstruction(overrideContext.overriddenOpcode) : instructionFactory.GetInstruction(overrideContext.overriddenOpcode);
                        int pcIncrements = completedInstruction.instructionDatabaseEntry.bytes;
                        int expectedIncrements = originalInstruction.instructionDatabaseEntry.bytes;
                        int incrementsOffset = expectedIncrements - pcIncrements;
                        programCounter = (ushort)unchecked(programCounter + incrementsOffset);
                    }
                }
                activeOperation = null;
                return isInstruction;
            }
            return false;
        }

        /// <summary>
        /// Checks for interrupts, and launches interrupt handler operations
        /// </summary>
        /// <returns>True/false on interrupt.</returns>
        private bool CheckInterrupts()
        {
            InterruptType activeInterrupt = interruptHandler.GetActivePriorityInterrupt();
            if (interruptHandler.interruptMasterEnable && activeInterrupt != InterruptType.None)
            {
                activeOperation = new HandleInterruptOperation(activeInterrupt); 
                return true;
            }
            return false;
        }

        private InstructionOverride GetOverridingInstruction(ushort address)
        {
            InstructionOverride[] instructionOverrides = gameBoy.GetInstructionOverrides(address);
            if (instructionOverrides != null)
            {
                foreach (InstructionOverride instructionOverride in instructionOverrides)
                {
                    if (instructionOverride.isOverridingInstruction(address))
                        return instructionOverride;
                }
            }
            return null;
        }

        /// <summary>
        /// Increments the program counter by one, respecting the halt repeat bug.
        /// </summary>
        public void IncrementProgramCounter()
        {
            if (haltRepeatBugActive)
                haltRepeatBugActive = false;    //The halt repeat bug causes the PC to not increment after a halt instruction ended without halt mode being started & IME being false.
            else
                programCounter++;
        }

        /// <summary>
        /// Pushes the specified bytes onto the stack.
        /// </summary>
        /// <param name="msb">Most Significant Byte</param>
        /// <param name="lsb">Least Significant Byte</param>
        public void PushToStack(byte msb, byte lsb)
        {
            stackPointer--;
            memMap.Write(stackPointer, msb);
            stackPointer--;
            memMap.Write(stackPointer, lsb);
        }

        /// <summary>
        /// Pushes the word onto the stack
        /// </summary>
        /// <param name="value">The word to push</param>
        public void PushToStack(ushort value)
        {
            byte msb = (byte)(value / 0x100);
            byte lsb = ((byte)(value & 0x00FF));
            PushToStack(msb, lsb);
        }

        /// <summary>
        /// Pops the head value from the stack.
        /// </summary>
        /// <returns>The popped value.</returns>
        public ushort PopFromStack()
        {
            byte lsb = memMap.Read(stackPointer);
            stackPointer++;
            byte msb = memMap.Read(stackPointer);
            stackPointer++;
            ushort value = (ushort)(msb * 0x100);
            value += lsb;
            return value;
        }

        public enum RegisterType
        {
            A,
            B,
            C,
            D,
            E,
            F,
            H,
            L,
        }

        public enum RegisterPairType
        {
            AF,
            BC,
            DE,
            HL,
            PC,     //While technically not pairs, they follow the same data format.
            SP
        }
    }
}
