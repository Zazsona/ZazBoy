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

        }

        public override void Tick()
        {
            if (executedClocks == 0)
                System.Console.WriteLine("NOP");    
            base.Tick();
        }
    }
}
