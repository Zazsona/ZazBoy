using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZazBoy.Console.Operations;

namespace ZazBoy.Console.Instructions
{
    public class HaltInstruction : Instruction
    {
        private HaltModeOperation haltModeOperation;

        public HaltInstruction() : base(0x00, 0x76, 4) //HALT takes at least 4 clocks
        {

        }

        public override void Tick()
        {
            if (haltModeOperation == null)
            {
                base.Tick();
                if (isComplete)
                {
                    if (GameBoy.Instance().CPU.delayedEIBugActive)
                    {
                        GameBoy.Instance().InterruptHandler.interruptMasterEnable = true;
                        GameBoy.Instance().CPU.delayedEIBugActive = false;
                    }

                    MemoryMap memMap = GameBoy.Instance().MemoryMap;
                    byte ieRegister = memMap.Read(InterruptHandler.InterruptEnableRegister);
                    byte ifRegister = memMap.Read(InterruptHandler.InterruptFlagRegister);
                    if ((ieRegister & ifRegister & 0x1F) == 0)
                    {
                        haltModeOperation = new HaltModeOperation();
                        isComplete = false;
                    }
                    else
                    {
                        isComplete = true;
                        GameBoy.Instance().CPU.haltRepeatBugActive = !GameBoy.Instance().InterruptHandler.interruptMasterEnable; //Repeat bug only occurs if IME is false
                    }
                }
            }
            else //Ensures this runs the next clock after the instruction's clocks.
            {
                haltModeOperation.Tick();
                isComplete = haltModeOperation.isComplete;
            }
        }

        protected override void Execute()
        {
            //Do nothing
        }
    }
}
