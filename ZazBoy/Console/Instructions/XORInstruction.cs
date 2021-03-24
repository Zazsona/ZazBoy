using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class XORInstruction : Instruction
    {
        public XORInstruction(byte opcode) : base(0x00, opcode, 4)
        {
            switch (opcode)
            {
                case 0xA8:
                case 0xA9:
                case 0xAA:
                case 0xAB:
                case 0xAC:
                case 0xAD:
                case 0xAF:
                    totalClocks = 4;
                    break;
                case 0xAE:
                case 0xEE:
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
                case 0xA8:
                    ApplyXOR(cpu, cpu.registerA, cpu.registerB);
                    break;
                case 0xA9:
                    ApplyXOR(cpu, cpu.registerA, cpu.registerC);
                    break;
                case 0xAA:
                    ApplyXOR(cpu, cpu.registerA, cpu.registerD);
                    break;
                case 0xAB:
                    ApplyXOR(cpu, cpu.registerA, cpu.registerE);
                    break;
                case 0xAC:
                    ApplyXOR(cpu, cpu.registerA, cpu.registerH);
                    break;
                case 0xAD:
                    ApplyXOR(cpu, cpu.registerA, cpu.registerL);
                    break;
                case 0xAF:
                    ApplyXOR(cpu, cpu.registerA, cpu.registerA);
                    break;
                case 0xAE:
                    ApplyXOR(cpu, cpu.registerA, memMap.Read(cpu.registersHL));
                    break;
                case 0xEE:
                    ApplyXOR(cpu, cpu.registerA, Get8BitImmediate());
                    break;
            }
        }

        private void ApplyXOR(CPU cpu, byte firstOperand, byte secondOperand)
        {
            cpu.registerA = ((byte)(firstOperand ^ secondOperand));

            cpu.zeroFlag = cpu.registerA == 0;
            cpu.subtractionFlag = false;
            cpu.halfCarryFlag = false;
            cpu.carryFlag = false;
        }
    }
}
