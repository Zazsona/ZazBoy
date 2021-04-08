using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class PopInstruction : Instruction
    {
        public PopInstruction(byte opcode) : base(0x00, opcode, 12)
        {
            Reset();
        }

        public override void Reset()
        {
            base.Reset();
            this.totalClocks = 12;
        }

        /// <summary>
        /// Enacts the POP instruction
        /// </summary>
        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            ushort poppedValue = cpu.PopFromStack();
            switch (opcode)
            {
                case 0xC1:
                    cpu.registersBC = poppedValue;
                    break;
                case 0xD1:
                    cpu.registersDE = poppedValue;
                    break;
                case 0xE1:
                    cpu.registersHL = poppedValue;
                    break;
                case 0xF1:
                    cpu.registersAF = poppedValue;
                    cpu.registersAF &= 0xFFF0; //Last four bits aren't supported in flag register
                    break;
            }
        }
    }
}
