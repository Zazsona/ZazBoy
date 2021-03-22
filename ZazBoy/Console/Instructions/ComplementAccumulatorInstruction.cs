using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class ComplementAccumulatorInstruction : Instruction
    {
        public ComplementAccumulatorInstruction(byte opcode) : base(0x00, opcode, 4)
        {

        }

        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            cpu.registerA = unchecked((byte)~cpu.registerA); //Unchecked as int -> byte cast uses two's complement at ints can be negative. We don't want that as our byte is unsigned.
            cpu.subtractionFlag = true;
            cpu.halfCarryFlag = true;
        }
    }
}
