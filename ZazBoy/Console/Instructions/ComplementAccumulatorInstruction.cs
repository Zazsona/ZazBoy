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
            cpu.registerA = ((byte)~cpu.registerA);
            cpu.subtractionFlag = true;
            cpu.halfCarryFlag = true;
        }
    }
}
