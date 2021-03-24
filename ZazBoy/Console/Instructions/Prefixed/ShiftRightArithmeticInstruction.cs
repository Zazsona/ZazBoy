using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions.Prefixed
{
    public class ShiftRightArithmeticInstruction : Instruction
    {
        public ShiftRightArithmeticInstruction(byte opcode) : base(0xCB, opcode, 8)
        {
            switch (opcode)
            {
                case 0x28:
                case 0x29:
                case 0x2A:
                case 0x2B:
                case 0x2C:
                case 0x2D:
                case 0x2F:
                    totalClocks = 8;
                    break;
                case 0x2E:
                    totalClocks = 16;
                    break;
            }
        }

        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            MemoryMap memMap = GameBoy.Instance().MemoryMap;

            switch (opcode)
            {
                case 0x28:
                    cpu.registerB = ShiftBitsRight(cpu, cpu.registerB);
                    break;
                case 0x29:
                    cpu.registerC = ShiftBitsRight(cpu, cpu.registerC);
                    break;
                case 0x2A:
                    cpu.registerD = ShiftBitsRight(cpu, cpu.registerD);
                    break;
                case 0x2B:
                    cpu.registerE = ShiftBitsRight(cpu, cpu.registerE);
                    break;
                case 0x2C:
                    cpu.registerH = ShiftBitsRight(cpu, cpu.registerH);
                    break;
                case 0x2D:
                    cpu.registerL = ShiftBitsRight(cpu, cpu.registerL);
                    break;
                case 0x2F:
                    cpu.registerA = ShiftBitsRight(cpu, cpu.registerA);
                    break;
                case 0x2E:
                    byte value = memMap.Read(cpu.registersHL);
                    memMap.Write(cpu.registersHL, ShiftBitsRight(cpu, value));
                    break;
            }
        }

        private byte ShiftBitsRight(CPU cpu, byte value)
        {
            byte msbMask = (1 << 7);
            bool msb = ((value & msbMask) != 0);
            byte lsbMask = (1 << 0);
            bool lsb = ((value & lsbMask) != 0);
            byte shiftedValue = ((byte)(value >> 1));
            shiftedValue = ((byte)(shiftedValue | ((msb) ? 0x80 : 0x00)));

            cpu.carryFlag = lsb;
            cpu.zeroFlag = shiftedValue == 0;
            cpu.halfCarryFlag = false;
            cpu.subtractionFlag = false;
            return shiftedValue;
        }
    }
}
