using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    class SetCarryFlagInstruction : Instruction
    {
        public SetCarryFlagInstruction() : base(0x00, 0x37, 4)
        {
            Reset();
        }

        public override void Reset()
        {
            base.Reset();
            this.totalClocks = 4;
        }

        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            cpu.subtractionFlag = false;
            cpu.halfCarryFlag = false;
            cpu.carryFlag = true;
        }
    }
}
