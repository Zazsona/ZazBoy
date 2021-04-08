using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class RestartInstruction : Instruction
    {
        public RestartInstruction(byte opcode) : base(0x00, opcode, 16)
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
            cpu.PushToStack(cpu.programCounter);
            switch (opcode)
            {
                case 0xC7:
                    cpu.programCounter = 0x00;
                    break;
                case 0xCF:
                    cpu.programCounter = 0x08;
                    break;
                case 0xD7:
                    cpu.programCounter = 0x10;
                    break;
                case 0xDF:
                    cpu.programCounter = 0x18;
                    break;
                case 0xE7:
                    cpu.programCounter = 0x20;
                    break;
                case 0xEF:
                    cpu.programCounter = 0x28;
                    break;
                case 0xF7:
                    cpu.programCounter = 0x30;
                    break;
                case 0xFF:
                    cpu.programCounter = 0x38;
                    break;
            }
        }
    }
}
