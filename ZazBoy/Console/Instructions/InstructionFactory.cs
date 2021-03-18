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
            Instruction instruction = null;
            if (opcode == 0x00)
                instruction = new NOPInstruction();
            else if (GetLoadInstruction(opcode) != null)
                instruction = GetLoadInstruction(opcode);
            return instruction;
        }

        /// <summary>
        /// Parses the opcode for the associated load instruction type.
        /// </summary>
        /// <param name="opcode">The opcode to decode</param>
        /// <returns>The LoadInstruction</returns>
        private LoadInstruction GetLoadInstruction(byte opcode)
        {
            switch (opcode)
            {
                case 0x40:
                case 0x41:
                case 0x42:
                case 0x43:
                case 0x44:
                case 0x45:
                case 0x47:
                case 0x48:
                case 0x49:
                case 0x4A:
                case 0x4B:
                case 0x4C:
                case 0x4D:
                case 0x4F:
                case 0x50:
                case 0x51:
                case 0x52:
                case 0x53:
                case 0x54:
                case 0x55:
                case 0x57:
                case 0x58:
                case 0x59:
                case 0x5A:
                case 0x5B:
                case 0x5C:
                case 0x5D:
                case 0x5F:
                case 0x60:
                case 0x61:
                case 0x62:
                case 0x63:
                case 0x64:
                case 0x65:
                case 0x67:
                case 0x68:
                case 0x69:
                case 0x6A:
                case 0x6B:
                case 0x6C:
                case 0x6D:
                case 0x6F:
                    return new LoadInstruction(opcode, 4);
                case 0x02:
                case 0x06:
                case 0x0A:
                case 0x0E:
                case 0x12:
                case 0x16:
                case 0x1A:
                case 0x1E:
                case 0x22:
                case 0x26:
                case 0x2A:
                case 0x2E:
                case 0x32:
                case 0x3A:
                case 0x3E:
                case 0x46:
                case 0x4E:
                case 0x56:
                case 0x5E:
                case 0x66:
                case 0x6E:
                case 0x70:
                case 0x71:
                case 0x72:
                case 0x73:
                case 0x74:
                case 0x75:
                case 0x77:
                case 0x7E:
                case 0xE2:
                case 0xF2:
                case 0xF9:
                    return new LoadInstruction(opcode, 8);
                case 0x01:
                case 0x11:
                case 0x21:
                case 0x31:
                case 0x36:
                case 0xF8:
                    return new LoadInstruction(opcode, 12);
                case 0xEA:
                case 0xFA:
                    return new LoadInstruction(opcode, 16);
                case 0x08:
                    return new LoadInstruction(opcode, 20);
                default:
                    return null;
            }
        }
    }
}
