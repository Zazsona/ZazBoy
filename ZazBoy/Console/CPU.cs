using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console
{
    /// <summary>
    /// Sharp LR35902 CPU emulator
    /// </summary>
    public class CPU
    {
        private byte registerA;
        private byte registerB;
        private byte registerC;
        private byte registerD;
        private byte registerE;
        private byte registerF;
        private byte registerH;
        private byte registerL;
        private ushort programCounter;
        private ushort stackPointer;

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
        }

        /// <summary>
        /// Progresses the CPU by one clock
        /// </summary>
        public void Tick()
        {
            MemoryMap memoryMap = GameBoy.Instance().MemoryMap;
            byte opcode = memoryMap.Read(programCounter);
            programCounter++;
            DecodeInstruction(opcode);
        }

        private void DecodeInstruction(byte opcode)
        {
            switch (opcode)
            {
                case 0xC3:
                case 0xE9:
                    System.Console.WriteLine("Opcode: JP");
                    break;
                default:
                    System.Console.WriteLine("Unknown Opcode: " + opcode);
                    break;
            }
        }
    }
}
