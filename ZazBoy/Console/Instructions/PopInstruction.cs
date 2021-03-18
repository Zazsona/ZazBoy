using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class PopInstruction : Instruction
    {
        public PopInstruction(byte opcode) : base(0x00, opcode, 12)
        {

        }

        /// <summary>
        /// Enacts the POP instruction
        /// </summary>
        protected override void Execute()
        {
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            CPU cpu = GameBoy.Instance().CPU;
            byte lsb = memMap.Read(cpu.stackPointer);
            cpu.stackPointer++;
            byte msb = memMap.Read(cpu.stackPointer);
            cpu.stackPointer++;
            ushort value = (ushort)(msb * 0x100);
            value += lsb;

            switch (opcode)
            {
                case 0xC1:
                    cpu.registersBC = value;
                    break;
                case 0xD1:
                    cpu.registersDE = value;
                    break;
                case 0xE1:
                    cpu.registersHL = value;
                    break;
                case 0xF1:
                    cpu.registersAF = value;
                    break;
            }
        }
    }
}
