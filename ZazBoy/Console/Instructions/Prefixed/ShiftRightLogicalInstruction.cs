using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions.Prefixed
{
    public class ShiftRightLogicalInstruction : Instruction
    {
        public ShiftRightLogicalInstruction(byte opcode) : base(0xCB, opcode, 8)
        {
            Reset();
        }

        public override void Reset()
        {
            base.Reset();
            switch (opcode)
            {
                case 0x38:
                case 0x39:
                case 0x3A:
                case 0x3B:
                case 0x3C:
                case 0x3D:
                case 0x3F:
                    totalClocks = 8;
                    break;
                case 0x3E:
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
                case 0x38:
                    cpu.registerB = ShiftBitsRight(cpu, cpu.registerB);
                    break;
                case 0x39:
                    cpu.registerC = ShiftBitsRight(cpu, cpu.registerC);
                    break;
                case 0x3A:
                    cpu.registerD = ShiftBitsRight(cpu, cpu.registerD);
                    break;
                case 0x3B:
                    cpu.registerE = ShiftBitsRight(cpu, cpu.registerE);
                    break;
                case 0x3C:
                    cpu.registerH = ShiftBitsRight(cpu, cpu.registerH);
                    break;
                case 0x3D:
                    cpu.registerL = ShiftBitsRight(cpu, cpu.registerL);
                    break;
                case 0x3F:
                    cpu.registerA = ShiftBitsRight(cpu, cpu.registerA);
                    break;
                case 0x3E:
                    byte value = memMap.Read(cpu.registersHL);
                    memMap.Write(cpu.registersHL, ShiftBitsRight(cpu, value));
                    break;
            }
        }

        private byte ShiftBitsRight(CPU cpu, byte value)
        {
            byte lsbMask = (1 << 0);
            bool lsb = ((value & lsbMask) != 0);
            byte shiftedValue = ((byte)(value >> 1));

            cpu.carryFlag = lsb;
            cpu.zeroFlag = shiftedValue == 0;
            cpu.halfCarryFlag = false;
            cpu.subtractionFlag = false;
            return shiftedValue;
        }
    }
}
