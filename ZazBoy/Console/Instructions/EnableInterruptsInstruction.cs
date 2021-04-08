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
            Reset();
        }

        public override void Reset()
        {
            base.Reset();
            this.totalClocks = 4;
        }

        protected override void Execute()
        {
            GameBoy.Instance().InterruptHandler.interruptMasterEnable = false; //EI actually disables interrupts for the delay period
            GameBoy.Instance().CPU.delayedEIBugActive = true;
        }
    }
}
