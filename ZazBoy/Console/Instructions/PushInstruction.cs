using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class PushInstruction : Instruction
    {
        public PushInstruction(byte opcode) : base(0x00, opcode, 16)
        {
            Reset();
        }

        public override void Reset()
        {
            base.Reset();
            this.totalClocks = 16;
        }

        /// <summary>
        /// Enacts the PUSH instruction
        /// </summary>
        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            switch (opcode)
            {
                case 0xC5:
                    cpu.PushToStack(cpu.registersBC);
                    break;
                case 0xD5:
                    cpu.PushToStack(cpu.registersDE);
                    break;
                case 0xE5:
                    cpu.PushToStack(cpu.registersHL);
                    break;
                case 0xF5:
                    cpu.PushToStack(cpu.registersAF);
                    break;
            }
        }
    }
}
