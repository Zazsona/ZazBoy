using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class IncrementInstruction : Instruction
    {
        public IncrementInstruction(byte opcode) : base(0x00, opcode, 0)
        {
            switch (opcode)
            {
                case 0x03:
                case 0x13:
                case 0x23:
                case 0x33:
                    totalClocks = 8;
                    break;
                case 0x04:
                case 0x14:
                case 0x24:
                case 0x0C:
                case 0x1C:
                case 0x2C:
                case 0x3C:
                    totalClocks = 4;
                    break;
                case 0x34:
                    totalClocks = 12;
                    break;
            }
        }

        protected override void Execute()
        {
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            CPU cpu = GameBoy.Instance().CPU;
            switch (opcode)
            {
                case 0x03:
                    cpu.registersBC++;
                    break;
                case 0x13:
                    cpu.registersDE++;
                    break;
                case 0x23:
                    cpu.registersHL++;
                    break;
                case 0x33:
                    cpu.stackPointer++;
                    break;
                case 0x04:
                    cpu.registerB++;
                    SetFlags(cpu, cpu.registerB);
                    break;
                case 0x14:
                    cpu.registerD++;
                    SetFlags(cpu, cpu.registerD);
                    break;
                case 0x24:
                    cpu.registerH++;
                    SetFlags(cpu, cpu.registerH);
                    break;
                case 0x0C:
                    cpu.registerC++;
                    SetFlags(cpu, cpu.registerC);
                    break;
                case 0x1C:
                    cpu.registerE++;
                    SetFlags(cpu, cpu.registerE);
                    break;
                case 0x2C:
                    cpu.registerL++;
                    SetFlags(cpu, cpu.registerL);
                    break;
                case 0x3C:
                    cpu.registerA++;
                    SetFlags(cpu, cpu.registerA);
                    break;
                case 0x34:
                    byte value = memMap.Read(cpu.registersHL);
                    value++;
                    SetFlags(cpu, value);
                    memMap.Write(cpu.registersHL, value);
                    break;
            }
        }

        private void SetFlags(CPU cpu, byte value)
        {
            cpu.subtractionFlag = false;
            if (value == 0)
                cpu.zeroFlag = true;
            if (((((value - 1) & 0x0F) + (1 & 0x0F)) & 0x10) == 0x10) //(Nibble of Base + Nibble of Increment) AND 10000 == 10000
                cpu.halfCarryFlag = true;
        }
    }
}
