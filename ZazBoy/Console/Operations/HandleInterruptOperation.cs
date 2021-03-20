using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ZazBoy.Console.InterruptHandler;

namespace ZazBoy.Console.Operations
{
    public class HandleInterruptOperation : Operation
    {
        private InterruptType activeInterrupt; 

        public HandleInterruptOperation(InterruptType activeInterrupt) : base(20) //Apparently untested, but interrupts should take 20 T-cycles. (5 M-cycles)
        {
            this.activeInterrupt = activeInterrupt;
        }

        protected override void Execute()
        {
            InterruptHandler interruptHandler = GameBoy.Instance().InterruptHandler;
            CPU cpu = GameBoy.Instance().CPU;
            interruptHandler.SetInterruptRequested(activeInterrupt, false);
            interruptHandler.interruptMasterEnable = false;
            cpu.PushToStack(cpu.programCounter);
            cpu.programCounter = interruptHandler.GetInterruptJumpAddress(activeInterrupt);
            //interruptClocks = 20; 
        }
    }
}
