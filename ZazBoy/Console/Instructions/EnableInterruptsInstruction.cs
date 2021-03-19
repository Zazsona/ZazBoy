using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class EnableInterruptsInstruction : Instruction
    {
        public EnableInterruptsInstruction() : base(0x00, 0xFB, 4)
        {

        }

        protected override void Execute()
        {
            GameBoy.Instance().InterruptHandler.interruptMasterEnable = true; //TODO: There's a 1 cycle gap for re-enabling interrupts to come into force. Might already be enabled? If so, DI and RETI are wrong.
        }
    }
}
