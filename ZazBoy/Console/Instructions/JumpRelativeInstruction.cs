using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class JumpRelativeInstruction : Instruction
    {
        public JumpRelativeInstruction(byte opcode) : base(0x00, opcode, 8)
        {
            CPU cpu = GameBoy.Instance().CPU;
            if ((opcode == 0x18) || (opcode == 0x20 && !cpu.zeroFlag) || (opcode == 0x30 && !cpu.carryFlag) || (opcode == 0x28 && cpu.zeroFlag) || (opcode == 0x38 && cpu.carryFlag))
                totalClocks = 12;
        }

        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            switch (opcode)
            {
                case 0x18:
                    ApplyRelativeJump(cpu, true);
                    break;
                case 0x20:
                    ApplyRelativeJump(cpu, !cpu.zeroFlag);
                    break;
                case 0x28:
                    ApplyRelativeJump(cpu, cpu.zeroFlag);
                    break;
                case 0x30:
                    ApplyRelativeJump(cpu, !cpu.carryFlag);
                    break;
                case 0x38:
                    ApplyRelativeJump(cpu, cpu.carryFlag);
                    break;
            }
        }

        /// <summary>
        /// Enacts the effect of the relative jump, setting PC if condition is true, or incrementing it if condition is false
        /// </summary>
        /// <param name="cpu">Context</param>
        /// <param name="condition">The flag to check</param>
        private void ApplyRelativeJump(CPU cpu, bool condition)
        {
            sbyte signedByte = ((sbyte)Get8BitImmediate()); //Even if unused, this ensures PC is incremented
            if (condition)
            {
                ushort address = (ushort)(cpu.programCounter + signedByte);
                cpu.programCounter = address;
            }
        }
    }
}
