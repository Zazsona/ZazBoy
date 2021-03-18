using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    class LoadHalfInstruction : Instruction
    {
        public LoadHalfInstruction(byte opcode) : base(0x00, opcode, 12)
        {
        }

        public override void Tick()
        {
            if (executedClocks == 0)
            {
                System.Console.WriteLine("Running Half Load (" + opcode + ")");
                Execute();
            }
            base.Tick();
        }

        /// <summary>
        /// Enacts the load half operation
        /// </summary>
        private void Execute()
        {
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            CPU cpu = GameBoy.Instance().CPU;
            byte offset = Get8BitImmediate();
            ushort address = (ushort)(0xFF00 + offset);
            switch (opcode)
            {
                case 0xE0:
                    memMap.Write(address, cpu.registerA);
                    break;
                case 0xF0:
                    cpu.registerA = memMap.Read(address);
                    break;
            }
        }
    }
}
