using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class OrInstruction : Instruction
    {
        public OrInstruction(byte opcode) : base(0x00, opcode, 4)
        {
            switch (opcode)
            {
                case 0xB0:
                case 0xB1:
                case 0xB2:
                case 0xB3:
                case 0xB4:
                case 0xB5:
                case 0xB7:
                    totalClocks = 4;
                    break;
                case 0xB6:
                case 0xF6:
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
                case 0xB0:
                    ApplyOR(cpu, cpu.registerA, cpu.registerB);
                    break;
                case 0xB1:
                    ApplyOR(cpu, cpu.registerA, cpu.registerC);
                    break;
                case 0xB2:
                    ApplyOR(cpu, cpu.registerA, cpu.registerD);
                    break;
                case 0xB3:
                    ApplyOR(cpu, cpu.registerA, cpu.registerE);
                    break;
                case 0xB4:
                    ApplyOR(cpu, cpu.registerA, cpu.registerH);
                    break;
                case 0xB5:
                    ApplyOR(cpu, cpu.registerA, cpu.registerL);
                    break;
                case 0xB7:
                    ApplyOR(cpu, cpu.registerA, cpu.registerA);
                    break;
                case 0xB6:
                    ApplyOR(cpu, cpu.registerA, memMap.Read(cpu.registersHL));
                    break;
                case 0xF6:
                    ApplyOR(cpu, cpu.registerA, Get8BitImmediate());
                    break;
            }
        }

        private void ApplyOR(CPU cpu, byte firstOperand, byte secondOperand)
        {
            cpu.registerA = unchecked((byte)(firstOperand | secondOperand));

            cpu.zeroFlag = cpu.registerA == 0;
            cpu.subtractionFlag = false;
            cpu.halfCarryFlag = false;
            cpu.carryFlag = false;
        }
    }
}
