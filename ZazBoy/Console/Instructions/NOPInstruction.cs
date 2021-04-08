using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class NOPInstruction : Instruction
    {
        public NOPInstruction() : base(0x00, 0x00, 4)
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
            //Do nothing.
        }
    }
}
