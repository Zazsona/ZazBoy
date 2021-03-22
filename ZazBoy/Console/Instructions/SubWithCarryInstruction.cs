using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class SubWithCarryInstruction : Instruction
    {
        public SubWithCarryInstruction(byte opcode) : base(0x00, opcode, 4)
        {
            switch (opcode)
            {
                case 0x98:
                case 0x99:
                case 0x9A:
                case 0x9B:
                case 0x9C:
                case 0x9D:
                case 0x9F:
                    totalClocks = 4;
                    break;
                case 0x9E:
                case 0xDE:
                    totalClocks = 8;
                    break;
            }
        }

        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            switch (opcode)
            {
                case 0x98:
                    ApplyCarrySubtraction(cpu, cpu.registerA, cpu.registerB);
                    break;
                case 0x99:
                    ApplyCarrySubtraction(cpu, cpu.registerA, cpu.registerC);
                    break;
                case 0x9A:
                    ApplyCarrySubtraction(cpu, cpu.registerA, cpu.registerD);
                    break;
                case 0x9B:
                    ApplyCarrySubtraction(cpu, cpu.registerA, cpu.registerE);
                    break;
                case 0x9C:
                    ApplyCarrySubtraction(cpu, cpu.registerA, cpu.registerH);
                    break;
                case 0x9D:
                    ApplyCarrySubtraction(cpu, cpu.registerA, cpu.registerL);
                    break;
                case 0x9F:
                    ApplyCarrySubtraction(cpu, cpu.registerA, cpu.registerA);
                    break;
                case 0x9E:
                    ApplyCarrySubtraction(cpu, cpu.registerA, memMap.Read(cpu.registersHL));
                    break;
                case 0xDE:
                    ApplyCarrySubtraction(cpu, cpu.registerA, Get8BitImmediate());
                    break;
            }
        }

        private void ApplyCarrySubtraction(CPU cpu, byte firstOperand, byte secondOperand)
        {
            byte carry = (byte)((cpu.carryFlag) ? 1 : 0);
            secondOperand += carry;
            byte result = ((byte)(firstOperand - secondOperand));

            cpu.subtractionFlag = true;
            cpu.zeroFlag = result == 0;
            cpu.carryFlag = secondOperand > firstOperand;
            cpu.halfCarryFlag = (secondOperand & 0x0F) > (firstOperand & 0x0F);

            cpu.registerA = result;
        }
    }
}
