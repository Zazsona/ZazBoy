using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions.Prefixed
{
    public class RotateRightThroughCarryInstruction : Instruction
    {
        public RotateRightThroughCarryInstruction(byte opcode) : base(0xCB, opcode, 8)
        {
            switch (opcode)
            {
                case 0x18:
                case 0x19:
                case 0x1A:
                case 0x1B:
                case 0x1C:
                case 0x1D:
                case 0x1F:
                    totalClocks = 8;
                    break;
                case 0x1E:
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
                case 0x18:
                    cpu.registerB = RotateBitsRightThroughCarry(cpu, cpu.registerB);
                    break;
                case 0x19:
                    cpu.registerC = RotateBitsRightThroughCarry(cpu, cpu.registerC);
                    break;
                case 0x1A:
                    cpu.registerD = RotateBitsRightThroughCarry(cpu, cpu.registerD);
                    break;
                case 0x1B:
                    cpu.registerE = RotateBitsRightThroughCarry(cpu, cpu.registerE);
                    break;
                case 0x1C:
                    cpu.registerH = RotateBitsRightThroughCarry(cpu, cpu.registerH);
                    break;
                case 0x1D:
                    cpu.registerL = RotateBitsRightThroughCarry(cpu, cpu.registerL);
                    break;
                case 0x1F:
                    cpu.registerA = RotateBitsRightThroughCarry(cpu, cpu.registerA);
                    break;
                case 0x1E:
                    byte value = memMap.Read(cpu.registersHL);
                    memMap.Write(cpu.registersHL, RotateBitsRightThroughCarry(cpu, value));
                    break;
            }
        }

        private byte RotateBitsRightThroughCarry(CPU cpu, byte value)
        {
            byte droppedBitMask = (1 << 0);
            bool droppedBit = ((value & droppedBitMask) != 0);
            byte shiftedValue = ((byte)(value >> 1));
            shiftedValue = unchecked((byte)(shiftedValue | ((cpu.carryFlag) ? 0x80 : 0x00)));

            cpu.carryFlag = droppedBit;
            cpu.zeroFlag = shiftedValue == 0;
            cpu.halfCarryFlag = false;
            cpu.subtractionFlag = false;
            return shiftedValue;
        }
    }
}
