using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class DecrementInstruction : Instruction
    {
        public DecrementInstruction(byte opcode) : base(0x00, opcode, 0)
        {
            switch (opcode)
            {
                case 0x0B:
                case 0x1B:
                case 0x2B:
                case 0x3B:
                    totalClocks = 8;
                    break;
                case 0x05:
                case 0x15:
                case 0x25:
                case 0x0D:
                case 0x1D:
                case 0x2D:
                case 0x3D:
                    totalClocks = 4;
                    break;
                case 0x35:
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
                case 0x0B:
                    cpu.registersBC--;
                    break;
                case 0x1B:
                    cpu.registersDE--;
                    break;
                case 0x2B:
                    cpu.registersHL--;
                    break;
                case 0x3B:
                    cpu.stackPointer--;
                    break;
                case 0x05:
                    cpu.registerB--;
                    SetFlags(cpu, cpu.registerB);
                    break;
                case 0x15:
                    cpu.registerD--;
                    SetFlags(cpu, cpu.registerD);
                    break;
                case 0x25:
                    cpu.registerH--;
                    SetFlags(cpu, cpu.registerH);
                    break;
                case 0x0D:
                    cpu.registerC--;
                    SetFlags(cpu, cpu.registerC);
                    break;
                case 0x1D:
                    cpu.registerE--;
                    SetFlags(cpu, cpu.registerE);
                    break;
                case 0x2D:
                    cpu.registerL--;
                    SetFlags(cpu, cpu.registerL);
                    break;
                case 0x3D:
                    cpu.registerA--;
                    SetFlags(cpu, cpu.registerA);
                    break;
                case 0x35:
                    byte value = memMap.Read(cpu.registersHL);
                    value--;
                    SetFlags(cpu, value);
                    memMap.Write(cpu.registersHL, value);
                    break;
            }
        }

        private void SetFlags(CPU cpu, byte value)
        {
            cpu.subtractionFlag = true;
            if (value == 0)
                cpu.zeroFlag = true;
            if (((value - 1) & 0x0F) < (1 & 0x0F)) //If base value is less than decrement, half carry is needed
                cpu.halfCarryFlag = true;
        }
    }
}
