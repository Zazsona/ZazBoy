using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Operations
{
    public class HaltModeOperation : Operation
    {
        private bool interruptSet = false;
        private int exitHaltClocks = 4;

        public HaltModeOperation() : base(-1)
        {

        }

        public override void Tick()
        {
            if (GameBoy.Instance().DEBUG_MODE)
                System.Console.WriteLine("Halted...");
            Execute();
        }

        protected override void Execute()
        {
            if (!interruptSet)
            {
                MemoryMap memMap = GameBoy.Instance().MemoryMap;
                byte ieRegister = memMap.Read(InterruptHandler.InterruptEnableRegister);
                byte ifRegister = memMap.Read(InterruptHandler.InterruptFlagRegister);
                for (int i = 0; i < 5; i++)
                {
                    byte flagBit = ((byte)(1 << i));
                    if ((ieRegister & flagBit) != 0 && (ifRegister & flagBit) != 0) //Halt explicitly ignores IME
                    {
                        interruptSet = true;
                        break;
                    }
                }
            }

            if (interruptSet) //Intentionally, this will run on the same clock interrupt set is discovered. Whether that's correct? More research needed! But if you're in here looking for bugs, might be it.
            {
                exitHaltClocks--;
                if (exitHaltClocks == 0)
                    isComplete = true;
            }
        }
    }
}
