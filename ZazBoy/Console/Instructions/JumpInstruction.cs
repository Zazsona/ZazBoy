using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class JumpInstruction : Instruction
    {
        public JumpInstruction(byte opcode) : base(0x00, opcode, 12)
        {
            CPU cpu = GameBoy.Instance().CPU;
            if ((opcode == 0xC3) || (opcode == 0xC2 && !cpu.zeroFlag) || (opcode == 0xD2 && !cpu.carryFlag) || (opcode == 0xCA && cpu.zeroFlag) || (opcode == 0xDA && cpu.carryFlag))
                totalClocks = 16;
            else if (opcode == 0xE9)
                totalClocks = 4;
        }

        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            switch (opcode)
            {
                case 0xC2:
                    ApplyJump(cpu, !cpu.zeroFlag);
                    break;
                case 0xC3:
                    ApplyJump(cpu, true);
                    break;
                case 0xCA:
                    ApplyJump(cpu, cpu.zeroFlag);
                    break;
                case 0xD2:
                    ApplyJump(cpu, !cpu.carryFlag);
                    break;
                case 0xDA:
                    ApplyJump(cpu, cpu.carryFlag);
                    break;
                case 0xE9:
                    cpu.programCounter = cpu.registersHL;
                    break;
            }
        }

        /// <summary>
        /// Enacts the effect of the jump, setting PC if condition is true, or incrementing it if condition is false
        /// </summary>
        /// <param name="cpu">Context</param>
        /// <param name="condition">The flag to check</param>
        private void ApplyJump(CPU cpu, bool condition)
        {
            ushort address = Get16BitImmediate(); //Even if unused, this ensures PC is incremented
            if (condition)
                cpu.programCounter = address;
        }
    }
}
