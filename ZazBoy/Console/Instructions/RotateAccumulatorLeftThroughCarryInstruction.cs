using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class RotateAccumulatorLeftThroughCarryInstruction : Instruction
    {
        public RotateAccumulatorLeftThroughCarryInstruction(byte opcode) : base(0x00, opcode, 4)
        {

        }

        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            byte value = cpu.registerA;

            byte droppedBitMask = (1 << 7);
            bool droppedBit = ((value & droppedBitMask) != 0);
            byte shiftedValue = ((byte)(value << 1));
            shiftedValue = unchecked((byte)(shiftedValue | ((cpu.carryFlag) ? 0x01 : 0x00)));

            cpu.registerA = shiftedValue;
            cpu.carryFlag = droppedBit;
            cpu.zeroFlag = false;
            cpu.halfCarryFlag = false;
            cpu.subtractionFlag = false;
        }
    }
}
