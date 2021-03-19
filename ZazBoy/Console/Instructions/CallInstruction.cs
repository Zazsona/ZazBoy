using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class CallInstruction : Instruction
    {
        public CallInstruction(byte opcode) : base(0x00, opcode, 12)
        {
            CPU cpu = GameBoy.Instance().CPU;
            if ((opcode == 0xCD) || (opcode == 0xC4 && !cpu.zeroFlag) || (opcode == 0xD4 && !cpu.carryFlag) || (opcode == 0xCC && cpu.zeroFlag) || (opcode == 0xDC && cpu.carryFlag))
                totalClocks = 24;
        }

        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            switch (opcode)
            {
                case 0xC4:
                    ApplyCall(cpu, !cpu.zeroFlag);
                    break;
                case 0xCD:
                    ApplyCall(cpu, true);
                    break;
                case 0xCC:
                    ApplyCall(cpu, cpu.zeroFlag);
                    break;
                case 0xD4:
                    ApplyCall(cpu, !cpu.carryFlag);
                    break;
                case 0xDC:
                    ApplyCall(cpu, cpu.carryFlag);
                    break;
            }
        }

        /// <summary>
        /// Enacts the effect of the call, setting PC & SP if condition is true, or incrementing PC if condition is false
        /// </summary>
        /// <param name="cpu">Context</param>
        /// <param name="condition">The flag to check</param>
        private void ApplyCall(CPU cpu, bool condition)
        {
            ushort address = Get16BitImmediate(); //Even if unused, this ensures PC is incremented
            if (condition)
            {
                cpu.PushToStack(cpu.programCounter);
                cpu.programCounter = address;
            }
        }
    }
}
