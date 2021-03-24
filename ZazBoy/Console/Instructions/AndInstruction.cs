using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class AndInstruction : Instruction
    {
        public AndInstruction(byte opcode) : base(0x00, opcode, 4)
        {
            switch (opcode)
            {
                case 0xA0:
                case 0xA1:
                case 0xA2:
                case 0xA3:
                case 0xA4:
                case 0xA5:
                case 0xA7:
                    totalClocks = 4;
                    break;
                case 0xA6:
                case 0xE6:
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
                case 0xA0:
                    ApplyAND(cpu, cpu.registerA, cpu.registerB);
                    break;
                case 0xA1:
                    ApplyAND(cpu, cpu.registerA, cpu.registerC);
                    break;
                case 0xA2:
                    ApplyAND(cpu, cpu.registerA, cpu.registerD);
                    break;
                case 0xA3:
                    ApplyAND(cpu, cpu.registerA, cpu.registerE);
                    break;
                case 0xA4:
                    ApplyAND(cpu, cpu.registerA, cpu.registerH);
                    break;
                case 0xA5:
                    ApplyAND(cpu, cpu.registerA, cpu.registerL);
                    break;
                case 0xA7:
                    ApplyAND(cpu, cpu.registerA, cpu.registerA);
                    break;
                case 0xA6:
                    ApplyAND(cpu, cpu.registerA, memMap.Read(cpu.registersHL));
                    break;
                case 0xE6:
                    ApplyAND(cpu, cpu.registerA, Get8BitImmediate());
                    break;
            }
        }

        private void ApplyAND(CPU cpu, byte firstOperand, byte secondOperand)
        {
            cpu.registerA = (byte)(firstOperand & secondOperand);

            cpu.zeroFlag = cpu.registerA == 0;
            cpu.subtractionFlag = false;
            cpu.halfCarryFlag = true;
            cpu.carryFlag = false;
        }
    }
}
