using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions.Prefixed
{
    public class RotateLeftInstruction : Instruction
    {
        public RotateLeftInstruction(byte opcode) : base(0xCB, opcode, 8)
        {
            Reset();
        }

        public override void Reset()
        {
            base.Reset();
            switch (opcode)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x07:
                    totalClocks = 8;
                    break;
                case 0x06:
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
                case 0x00:
                    cpu.registerB = RotateBitsLeft(cpu, cpu.registerB);
                    break;
                case 0x01:
                    cpu.registerC = RotateBitsLeft(cpu, cpu.registerC);
                    break;
                case 0x02:
                    cpu.registerD = RotateBitsLeft(cpu, cpu.registerD);
                    break;
                case 0x03:
                    cpu.registerE = RotateBitsLeft(cpu, cpu.registerE);
                    break;
                case 0x04:
                    cpu.registerH = RotateBitsLeft(cpu, cpu.registerH);
                    break;
                case 0x05:
                    cpu.registerL = RotateBitsLeft(cpu, cpu.registerL);
                    break;
                case 0x07:
                    cpu.registerA = RotateBitsLeft(cpu, cpu.registerA);
                    break;
                case 0x06:
                    byte value = memMap.Read(cpu.registersHL);
                    memMap.Write(cpu.registersHL, RotateBitsLeft(cpu, value));
                    break;
            }
        }

        private byte RotateBitsLeft(CPU cpu, byte value)
        {
            byte droppedBitMask = (1 << 7);
            bool droppedBit = ((value & droppedBitMask) != 0);
            byte shiftedValue = ((byte)(value << 1));
            shiftedValue =  ((byte)(shiftedValue | ((droppedBit) ? 0x01 : 0x00)));

            cpu.carryFlag = droppedBit;
            cpu.zeroFlag = shiftedValue == 0;
            cpu.halfCarryFlag = false;
            cpu.subtractionFlag = false;
            return shiftedValue;
        }
    }
}
