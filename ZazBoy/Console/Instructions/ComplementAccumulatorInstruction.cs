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
            cpu.registerA = unchecked((byte)~cpu.registerA); //TODO: This is checked as otherwise C# uses two's complement. As we're looking for unsigned bytes, that's a badness. Make sure such issues aren't present on other instructions.
        }
    }
}
