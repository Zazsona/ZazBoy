using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class ReturnInstruction : Instruction
    {
        public ReturnInstruction(byte opcode) : base(0x00, opcode, 8)
        {
            CPU cpu = GameBoy.Instance().CPU;
            if ((opcode == 0xC0 && !cpu.zeroFlag) || (opcode == 0xD0 && !cpu.carryFlag) || (opcode == 0xC8 && cpu.zeroFlag) || (opcode == 0xD8 && cpu.carryFlag))
                totalClocks = 20;
            else if (opcode == 0xC9)
                totalClocks = 16;
        }

        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            switch (opcode)
            {
                case 0xC0:
                    ApplyReturn(cpu, !cpu.zeroFlag);
                    break;
                case 0xC9:
                    ApplyReturn(cpu, true);
                    break;
                case 0xC8:
                    ApplyReturn(cpu, cpu.zeroFlag);
                    break;
                case 0xD0:
                    ApplyReturn(cpu, !cpu.carryFlag);
                    break;
                case 0xD8:
                    ApplyReturn(cpu, cpu.carryFlag);
                    break;
            }
        }

        /// <summary>
        /// Enacts the effect of the return, setting PC & SP if condition is true
        /// </summary>
        /// <param name="cpu">Context</param>
        /// <param name="condition">The flag to check</param>
        private void ApplyReturn(CPU cpu, bool condition)
        {
            if (condition)
            {
                ushort address = cpu.PopFromStack();
                cpu.programCounter = address;
            }
        }
    }
}
