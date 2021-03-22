using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class CompareInstruction : Instruction
    {
        public CompareInstruction(byte opcode) : base(0x00, opcode, 4)
        {
            switch (opcode)
            {
                case 0xB8:
                case 0xB9:
                case 0xBA:
                case 0xBB:
                case 0xBC:
                case 0xBD:
                case 0xBF:
                    totalClocks = 4;
                    break;
                case 0xBE:
                case 0xFE:
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
                case 0xB8:
                    ApplyCompare(cpu, cpu.registerA, cpu.registerB);
                    break;
                case 0xB9:
                    ApplyCompare(cpu, cpu.registerA, cpu.registerC);
                    break;
                case 0xBA:
                    ApplyCompare(cpu, cpu.registerA, cpu.registerD);
                    break;
                case 0xBB:
                    ApplyCompare(cpu, cpu.registerA, cpu.registerE);
                    break;
                case 0xBC:
                    ApplyCompare(cpu, cpu.registerA, cpu.registerH);
                    break;
                case 0xBD:
                    ApplyCompare(cpu, cpu.registerA, cpu.registerL);
                    break;
                case 0xBF:
                    ApplyCompare(cpu, cpu.registerA, cpu.registerA);
                    break;
                case 0xBE:
                    ApplyCompare(cpu, cpu.registerA, memMap.Read(cpu.registersHL));
                    break;
                case 0xFE:
                    ApplyCompare(cpu, cpu.registerA, Get8BitImmediate());
                    break;
            }
        }

        private void ApplyCompare(CPU cpu, byte firstOperand, byte secondOperand)
        {
            cpu.zeroFlag = firstOperand == secondOperand;
            cpu.subtractionFlag = true;
            cpu.halfCarryFlag = ((firstOperand & 0x0F) < (secondOperand & 0x0F));
            cpu.carryFlag = firstOperand < secondOperand;
        }
    }
}
