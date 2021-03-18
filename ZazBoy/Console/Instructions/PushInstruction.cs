using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class PushInstruction : Instruction
    {
        public PushInstruction(byte opcode) : base(0x00, opcode, 16)
        {

        }

        /// <summary>
        /// Enacts the PUSH instruction
        /// </summary>
        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            switch (opcode)
            {
                case 0xC5:
                    PushWord(cpu, cpu.registerB, cpu.registerC);
                    break;
                case 0xD5:
                    PushWord(cpu, cpu.registerD, cpu.registerE);
                    break;
                case 0xE5:
                    PushWord(cpu, cpu.registerH, cpu.registerL);
                    break;
                case 0xF5:
                    PushWord(cpu, cpu.registerA, cpu.registerF);
                    break;
            }
        }

        /// <summary>
        /// Pushes the specified word onto the stack.
        /// </summary>
        /// <param name="cpu">The CPU context</param>
        /// <param name="msb">Most Significant Byte</param>
        /// <param name="lsb">Least Significant Byte</param>
        private void PushWord(CPU cpu, byte msb, byte lsb)
        {
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            cpu.stackPointer--;
            memMap.Write(cpu.stackPointer, msb);
            cpu.stackPointer--;
            memMap.Write(cpu.stackPointer, lsb);
        }
    }
}
