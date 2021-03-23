using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions.Prefixed
{
    public class RotateRightInstruction : Instruction
    {
        public RotateRightInstruction(byte opcode) : base(0xCB, opcode, 8)
        {
            switch (opcode)
            {
                case 0x08:
                case 0x09:
                case 0x0A:
                case 0x0B:
                case 0x0C:
                case 0x0D:
                case 0x0F:
                    totalClocks = 8;
                    break;
                case 0x0E:
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
                case 0x08:
                    cpu.registerB = RotateBitsRight(cpu, cpu.registerB);
                    break;
                case 0x09:
                    cpu.registerC = RotateBitsRight(cpu, cpu.registerC);
                    break;
                case 0x0A:
                    cpu.registerD = RotateBitsRight(cpu, cpu.registerD);
                    break;
                case 0x0B:
                    cpu.registerE = RotateBitsRight(cpu, cpu.registerE);
                    break;
                case 0x0C:
                    cpu.registerH = RotateBitsRight(cpu, cpu.registerH);
                    break;
                case 0x0D:
                    cpu.registerL = RotateBitsRight(cpu, cpu.registerL);
                    break;
                case 0x0F:
                    cpu.registerA = RotateBitsRight(cpu, cpu.registerA);
                    break;
                case 0x0E:
                    byte value = memMap.Read(cpu.registersHL);
                    memMap.Write(cpu.registersHL, RotateBitsRight(cpu, value));
                    break;
            }
        }

        private byte RotateBitsRight(CPU cpu, byte value)
        {
            byte droppedBitMask = (1 << 0);
            bool droppedBit = ((value & droppedBitMask) != 0);
            byte shiftedValue = ((byte)(value >> 1));
            shiftedValue = unchecked((byte)(shiftedValue | ((droppedBit) ? 0x80 : 0x00)));

            cpu.carryFlag = droppedBit;
            cpu.zeroFlag = shiftedValue == 0;
            cpu.halfCarryFlag = false;
            cpu.subtractionFlag = false;
            return shiftedValue;
        }
    }
}
