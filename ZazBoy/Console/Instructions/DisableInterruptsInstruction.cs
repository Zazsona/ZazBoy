using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class DisableInterruptsInstruction : Instruction
    {
        public DisableInterruptsInstruction() : base(0x00, 0xF3, 4)
        {

        }

        protected override void Execute()
        {
            GameBoy.Instance().InterruptHandler.interruptMasterEnable = false;
        }
    }
}
