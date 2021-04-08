using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions.Prefixed
{
    public class RotateLeftThroughCarryInstruction : Instruction
    {
        public RotateLeftThroughCarryInstruction(byte opcode) : base(0xCB, opcode, 8)
        {
            Reset();
        }

        public override void Reset()
        {
            base.Reset();
            switch (opcode)
            {
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x14:
                case 0x15:
                case 0x17:
                    totalClocks = 8;
                    break;
                case 0x16:
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
                case 0x10:
                    cpu.registerB = RotateBitsLeftThroughCarry(cpu, cpu.registerB);
                    break;
                case 0x11:
                    cpu.registerC = RotateBitsLeftThroughCarry(cpu, cpu.registerC);
                    break;
                case 0x12:
                    cpu.registerD = RotateBitsLeftThroughCarry(cpu, cpu.registerD);
                    break;
                case 0x13:
                    cpu.registerE = RotateBitsLeftThroughCarry(cpu, cpu.registerE);
                    break;
                case 0x14:
                    cpu.registerH = RotateBitsLeftThroughCarry(cpu, cpu.registerH);
                    break;
                case 0x15:
                    cpu.registerL = RotateBitsLeftThroughCarry(cpu, cpu.registerL);
                    break;
                case 0x17:
                    cpu.registerA = RotateBitsLeftThroughCarry(cpu, cpu.registerA);
                    break;
                case 0x16:
                    byte value = memMap.Read(cpu.registersHL);
                    memMap.Write(cpu.registersHL, RotateBitsLeftThroughCarry(cpu, value));
                    break;
            }
        }

        private byte RotateBitsLeftThroughCarry(CPU cpu, byte value)
        {
            byte droppedBitMask = (1 << 7);
            bool droppedBit = ((value & droppedBitMask) != 0);
            byte shiftedValue = ((byte)(value << 1));
            shiftedValue = ((byte)(shiftedValue | ((cpu.carryFlag) ? 0x01 : 0x00)));

            cpu.carryFlag = droppedBit;
            cpu.zeroFlag = shiftedValue == 0;
            cpu.halfCarryFlag = false;
            cpu.subtractionFlag = false;
            return shiftedValue;
        }
    }
}
