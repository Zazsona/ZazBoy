using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class ReturnEnableInterruptsInstruction : Instruction
    {
        public ReturnEnableInterruptsInstruction() : base(0x00, 0xD9, 16)
        {
            Reset();
        }

        public override void Reset()
        {
            base.Reset();
            this.totalClocks = 16;
        }

        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            ushort address = cpu.PopFromStack();
            cpu.programCounter = address;
            GameBoy.Instance().InterruptHandler.interruptMasterEnable = true;
        }
    }
}
