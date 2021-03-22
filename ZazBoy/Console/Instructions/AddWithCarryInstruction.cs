using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class AddWithCarryInstruction : Instruction
    {
        public AddWithCarryInstruction(byte opcode) : base(0x00, opcode, 4)
        {
            switch (opcode)
            {
                case 0x88:
                case 0x89:
                case 0x8A:
                case 0x8B:
                case 0x8C:
                case 0x8D:
                case 0x8F:
                    totalClocks = 4;
                    break;
                case 0x8E:
                case 0xCE:
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
                case 0x88:
                    ApplyCarryAddition(cpu, cpu.registerA, cpu.registerB);
                    break;
                case 0x89:
                    ApplyCarryAddition(cpu, cpu.registerA, cpu.registerC);
                    break;
                case 0x8A:
                    ApplyCarryAddition(cpu, cpu.registerA, cpu.registerD);
                    break;
                case 0x8B:
                    ApplyCarryAddition(cpu, cpu.registerA, cpu.registerE);
                    break;
                case 0x8C:
                    ApplyCarryAddition(cpu, cpu.registerA, cpu.registerH);
                    break;
                case 0x8D:
                    ApplyCarryAddition(cpu, cpu.registerA, cpu.registerL);
                    break;
                case 0x8F:
                    ApplyCarryAddition(cpu, cpu.registerA, cpu.registerA);
                    break;
                case 0x8E:
                    ApplyCarryAddition(cpu, cpu.registerA, memMap.Read(cpu.registersHL));
                    break;
                case 0xCE:
                    ApplyCarryAddition(cpu, cpu.registerA, Get8BitImmediate());
                    break;
            }
        }

        private void ApplyCarryAddition(CPU cpu, byte firstOperand, byte secondOperand)
        {
            byte carry = (byte)((cpu.carryFlag) ? 1 : 0);
            secondOperand += carry;
            byte result = (byte)(firstOperand + secondOperand);

            cpu.subtractionFlag = false;
            cpu.zeroFlag = result == 0;
            cpu.carryFlag = (firstOperand + secondOperand) > byte.MaxValue;
            cpu.halfCarryFlag = ((((firstOperand & 0x0F) + (secondOperand & 0x0F)) & 0x10) == 0x10);

            if (result > byte.MaxValue)
                result -= byte.MaxValue;
            cpu.registerA = (byte)result;
        }
    }
}
