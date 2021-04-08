using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class SubInstruction : Instruction
    {
        public SubInstruction(byte opcode) : base(0x00, opcode, 4)
        {
            Reset();
        }

        public override void Reset()
        {
            base.Reset();
            switch (opcode)
            {
                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x97:
                    totalClocks = 4;
                    break;
                case 0x96:
                case 0xD6:
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
                case 0x90:
                    ApplySubtraction(cpu, cpu.registerA, cpu.registerB);
                    break;
                case 0x91:
                    ApplySubtraction(cpu, cpu.registerA, cpu.registerC);
                    break;
                case 0x92:
                    ApplySubtraction(cpu, cpu.registerA, cpu.registerD);
                    break;
                case 0x93:
                    ApplySubtraction(cpu, cpu.registerA, cpu.registerE);
                    break;
                case 0x94:
                    ApplySubtraction(cpu, cpu.registerA, cpu.registerH);
                    break;
                case 0x95:
                    ApplySubtraction(cpu, cpu.registerA, cpu.registerL);
                    break;
                case 0x97:
                    ApplySubtraction(cpu, cpu.registerA, cpu.registerA);
                    break;
                case 0x96:
                    ApplySubtraction(cpu, cpu.registerA, memMap.Read(cpu.registersHL));
                    break;
                case 0xD6:
                    ApplySubtraction(cpu, cpu.registerA, Get8BitImmediate());
                    break;
            }
        }

        private void ApplySubtraction(CPU cpu, byte firstOperand, byte secondOperand)
        {
            byte result = ((byte)(firstOperand - secondOperand));

            cpu.subtractionFlag = true;
            cpu.zeroFlag = result == 0;
            cpu.carryFlag = secondOperand > firstOperand;
            cpu.halfCarryFlag = (secondOperand & 0x0F) > (firstOperand & 0x0F);

            cpu.registerA = result;
        }
    }
}
