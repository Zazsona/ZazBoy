using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions.Prefixed
{
    public class ShiftLeftArithmeticInstruction : Instruction
    {
        public ShiftLeftArithmeticInstruction(byte opcode) : base(0xCB, opcode, 8)
        {
            switch (opcode)
            {
                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x27:
                    totalClocks = 8;
                    break;
                case 0x26:
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
                case 0x20:
                    cpu.registerB = ShiftBitsLeft(cpu, cpu.registerB);
                    break;
                case 0x21:
                    cpu.registerC = ShiftBitsLeft(cpu, cpu.registerC);
                    break;
                case 0x22:
                    cpu.registerD = ShiftBitsLeft(cpu, cpu.registerD);
                    break;
                case 0x23:
                    cpu.registerE = ShiftBitsLeft(cpu, cpu.registerE);
                    break;
                case 0x24:
                    cpu.registerH = ShiftBitsLeft(cpu, cpu.registerH);
                    break;
                case 0x25:
                    cpu.registerL = ShiftBitsLeft(cpu, cpu.registerL);
                    break;
                case 0x27:
                    cpu.registerA = ShiftBitsLeft(cpu, cpu.registerA);
                    break;
                case 0x26:
                    byte value = memMap.Read(cpu.registersHL);
                    memMap.Write(cpu.registersHL, ShiftBitsLeft(cpu, value));
                    break;
            }
        }

        private byte ShiftBitsLeft(CPU cpu, byte value)
        {
            byte droppedBitMask = (1 << 7);
            bool droppedBit = ((value & droppedBitMask) != 0);
            byte shiftedValue = ((byte)(value << 1));

            cpu.carryFlag = droppedBit;
            cpu.zeroFlag = shiftedValue == 0;
            cpu.halfCarryFlag = false;
            cpu.subtractionFlag = false;
            return shiftedValue;
        }
    }
}
