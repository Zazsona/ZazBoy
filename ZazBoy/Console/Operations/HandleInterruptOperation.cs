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
            bool ieOverwrittenWithMSB = ((ushort)(cpu.stackPointer - 1) == InterruptHandler.InterruptEnableRegister); //Pushing PC onto the stack can overwrite IE, leading to odd behaviour (I suspect similar occurs with IF, but couldn't find much documentation on it.)
            if (ieOverwrittenWithMSB)
                activeInterrupt = interruptHandler.GetActivePriorityInterrupt();
            cpu.PushToStack(cpu.programCounter);
            interruptHandler.interruptMasterEnable = false;
            if (activeInterrupt != InterruptType.None)
            {
                cpu.programCounter = interruptHandler.GetInterruptJumpAddress(activeInterrupt);
                interruptHandler.SetInterruptRequested(activeInterrupt, false);
            }
            else
                cpu.programCounter = 0;
        }
    }
}
