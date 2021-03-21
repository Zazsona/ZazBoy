using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class RotateRightAccumulatorInstruction : Instruction
    {
        public RotateRightAccumulatorInstruction(byte opcode) : base(0x00, opcode, 4)
        {

        }

        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            byte value = cpu.registerA;

            byte droppedBitMask = (1 << 0);
            bool droppedBit = ((value & droppedBitMask) != 0);
            byte shiftedValue = ((byte)(value >> 1));
            if (droppedBit)
                shiftedValue = (byte)(shiftedValue | 0x80); //1000 0000

            cpu.registerA = shiftedValue;
            cpu.carryFlag = droppedBit;
            cpu.zeroFlag = false;
            cpu.halfCarryFlag = false;
            cpu.subtractionFlag = false;
        }
    }
}
