using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZazBoy.Console.Instructions;

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

        private InstructionFactory instructionFactory;
        private Instruction activeInstruction;

        /// <summary>
        /// Creates a new instance of the CPU, setting register values to match Game Boy boot defaults.
        /// </summary>
        public CPU()
        {
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
            activeInstruction = null;
        }

        /// <summary>
        /// Progresses the CPU by one clock
        /// </summary>
        public void Tick()
        {
            MemoryMap memoryMap = GameBoy.Instance().MemoryMap;
            if (activeInstruction == null)
            {
                byte opcode = memoryMap.Read(programCounter);
                activeInstruction = instructionFactory.GetInstruction(opcode);
                programCounter++;
                if (activeInstruction == null)
                    System.Console.WriteLine("Unrecognised opcode: " + opcode);
            }
            if (activeInstruction != null) //TODO: Remove once all opcodes are implemented. This is just to stop a crash due to activeInstruction being null.
            {
                activeInstruction.Tick();
                if (activeInstruction.isComplete)
                    activeInstruction = null;
            }
        }

        /// <summary>
        /// Pushes the specified bytes onto the stack.
        /// </summary>
        /// <param name="msb">Most Significant Byte</param>
        /// <param name="lsb">Least Significant Byte</param>
        public void PushToStack(byte msb, byte lsb)
        {
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
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
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            byte lsb = memMap.Read(stackPointer);
            stackPointer++;
            byte msb = memMap.Read(stackPointer);
            stackPointer++;
            ushort value = (ushort)(msb * 0x100);
            value += lsb;
            return value;
        }
    }
}
