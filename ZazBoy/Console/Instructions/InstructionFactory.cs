using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    /// <summary>
    /// A factory for decoding opcodes
    /// </summary>
    public class InstructionFactory
    {
        /// <summary>
        /// Decodes an opcode and returns the instruction it signifies
        /// </summary>
        /// <param name="opcode">The opcode to decode.</param>
        /// <returns>The instruction identified by the opcode.</returns>
        public Instruction GetInstruction(byte opcode)
        {
            switch (opcode)
            {
                case 0x00:
                    return new NOPInstruction();
                default:
                    return null;
            }
        }
    }
}
