using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZazBoy.Console.Instructions.Prefixed;

namespace ZazBoy.Console.Instructions
{
    /// <summary>
    /// A factory for decoding opcodes
    /// </summary>
    public class InstructionFactory
    {
        private Dictionary<byte, Instruction> instructionsCache = new Dictionary<byte, Instruction>();
        private Dictionary<byte, Instruction> prefixedInstructionsCache = new Dictionary<byte, Instruction>();

        /// <summary>
        /// Decodes an opcode and returns the instruction it signifies
        /// </summary>
        /// <param name="opcode">The opcode to decode.</param>
        /// <returns>The instruction identified by the opcode.</returns>
        public Instruction GetInstruction(byte opcode)
        {
            if (instructionsCache.ContainsKey(opcode))
            {
                Instruction instruction = instructionsCache[opcode];
                instruction.Reset();
                return instruction;
            }
            else
            {
                Instruction instruction = CreateInstruction(opcode);
                instructionsCache.Add(opcode, instruction);
                return instruction;
            }
        }

        /// <summary>
        /// Decodes an opcode within the 0xCB prefix table and returns the instruction it signifies
        /// </summary>
        /// <param name="opcode">The opcode within the 0xCB prefix to decode.</param>
        /// <returns>The instruction identified by the opcode.</returns>
        public Instruction GetPrefixedInstruction(byte opcode)
        {
            if (prefixedInstructionsCache.ContainsKey(opcode))
            {
                Instruction instruction = prefixedInstructionsCache[opcode];
                instruction.Reset();
                return instruction;
            }
            else
            {
                Instruction instruction = CreatePrefixedInstruction(opcode);
                prefixedInstructionsCache.Add(opcode, instruction);
                return instruction;
            }
        }

        /// <summary>
        /// Decodes an opcode and creates the instruction it signifies
        /// </summary>
        /// <param name="opcode">The opcode to decode.</param>
        /// <returns>The instruction identified by the opcode.</returns>
        private Instruction CreateInstruction(byte opcode)
        {
            Instruction instruction = null;
            if (opcode == 0x00)
                instruction = new NOPInstruction();
            else if (IsLoadInstruction(opcode))
                instruction = new LoadInstruction(opcode);
            else if (opcode == 0xE0 || opcode == 0xF0)
                instruction = new LoadHalfInstruction(opcode);
            else if (opcode == 0xC5 || opcode == 0xD5 || opcode == 0xE5 || opcode == 0xF5)
                instruction = new PushInstruction(opcode);
            else if (opcode == 0xC1 || opcode == 0xD1 || opcode == 0xE1 || opcode == 0xF1)
                instruction = new PopInstruction(opcode);
            else if (opcode == 0xC2 || opcode == 0xC3 || opcode == 0xCA || opcode == 0xD2 || opcode == 0xDA || opcode == 0xE9)
                instruction = new JumpInstruction(opcode);
            else if (opcode == 0x18 || opcode == 0x20 || opcode == 0x28 || opcode == 0x30 || opcode == 0x38)
                instruction = new JumpRelativeInstruction(opcode);
            else if (opcode == 0xC4 || opcode == 0xCC || opcode == 0xCD || opcode == 0xD4 || opcode == 0xDC)
                instruction = new CallInstruction(opcode);
            else if (opcode == 0xC0 || opcode == 0xC8 || opcode == 0xC9 || opcode == 0xD0 || opcode == 0xD8)
                instruction = new ReturnInstruction(opcode);
            else if (opcode == 0xD9)
                instruction = new ReturnEnableInterruptsInstruction();
            else if (opcode == 0xC7 || opcode == 0xCF || opcode == 0xD7 || opcode == 0xDF || opcode == 0xE7 || opcode == 0xEF || opcode == 0xF7 || opcode == 0xFF)
                instruction = new RestartInstruction(opcode);
            else if (opcode == 0x03 || opcode == 0x04 || opcode == 0x0C || opcode == 0x13 || opcode == 0x14 || opcode == 0x1C || opcode == 0x23 || opcode == 0x24 || opcode == 0x2C || opcode == 0x33 || opcode == 0x34 || opcode == 0x3C)
                instruction = new IncrementInstruction(opcode);
            else if (opcode == 0x05 || opcode == 0x0B || opcode == 0x0D || opcode == 0x15 || opcode == 0x1B || opcode == 0x1D || opcode == 0x25 || opcode == 0x2B || opcode == 0x2D || opcode == 0x35 || opcode == 0x3B || opcode == 0x3D)
                instruction = new DecrementInstruction(opcode);
            else if (opcode == 0x80 || opcode == 0x81 || opcode == 0x82 || opcode == 0x83 || opcode == 0x84 || opcode == 0x85 || opcode == 0x86 || opcode == 0x87 || opcode == 0x09 || opcode == 0x19 || opcode == 0x29 || opcode == 0x39 || opcode == 0xC6 || opcode == 0xE8)
                instruction = new AddInstruction(opcode);
            else if (opcode == 0x88 || opcode == 0x89 || opcode == 0x8A || opcode == 0x8B || opcode == 0x8C || opcode == 0x8D || opcode == 0x8E || opcode == 0x8F || opcode == 0xCE)
                instruction = new AddWithCarryInstruction(opcode);
            else if (opcode == 0x90 || opcode == 0x91 || opcode == 0x92 || opcode == 0x93 || opcode == 0x94 || opcode == 0x95 || opcode == 0x96 || opcode == 0x97 || opcode == 0xD6)
                instruction = new SubInstruction(opcode);
            else if (opcode == 0x98 || opcode == 0x99 || opcode == 0x9A || opcode == 0x9B || opcode == 0x9C || opcode == 0x9D || opcode == 0x9E || opcode == 0x9F || opcode == 0xDE)
                instruction = new SubWithCarryInstruction(opcode);
            else if (opcode == 0xA0 || opcode == 0xA1 || opcode == 0xA2 || opcode == 0xA3 || opcode == 0xA4 || opcode == 0xA5 || opcode == 0xA6 || opcode == 0xA7 || opcode == 0xE6)
                instruction = new AndInstruction(opcode);
            else if (opcode == 0xA8 || opcode == 0xA9 || opcode == 0xAA || opcode == 0xAB || opcode == 0xAC || opcode == 0xAD || opcode == 0xAE || opcode == 0xAF || opcode == 0xEE)
                instruction = new XORInstruction(opcode);
            else if (opcode == 0xB0 || opcode == 0xB1 || opcode == 0xB2 || opcode == 0xB3 || opcode == 0xB4 || opcode == 0xB5 || opcode == 0xB6 || opcode == 0xB7 || opcode == 0xF6)
                instruction = new OrInstruction(opcode);
            else if (opcode == 0xB8 || opcode == 0xB9 || opcode == 0xBA || opcode == 0xBB || opcode == 0xBC || opcode == 0xBD || opcode == 0xBE || opcode == 0xBF || opcode == 0xFE)
                instruction = new CompareInstruction(opcode);
            else if (opcode == 0x3F)
                instruction = new ComplementCarryFlagInstruction();
            else if (opcode == 0x37)
                instruction = new SetCarryFlagInstruction();
            else if (opcode == 0x76)
                instruction = new HaltInstruction();
            else if (opcode == 0xFB)
                instruction = new EnableInterruptsInstruction();
            else if (opcode == 0xF3)
                instruction = new DisableInterruptsInstruction();
            else if (opcode == 0x07)
                instruction = new RotateAccumulatorLeftInstruction(opcode);
            else if (opcode == 0x17)
                instruction = new RotateAccumulatorLeftThroughCarryInstruction(opcode);
            else if (opcode == 0x0F)
                instruction = new RotateAccumulatorRightInstruction(opcode);
            else if (opcode == 0x1F)
                instruction = new RotateAccumulatorRightThroughCarryInstruction(opcode);
            else if (opcode == 0x2F)
                instruction = new ComplementAccumulatorInstruction(opcode);
            else if (opcode == 0x27)
                instruction = new DecimalAdjustAccumulatorInstruction();
            return instruction;
        }

        /// <summary>
        /// Decodes an opcode within the 0xCB prefix table and creates the instruction it signifies
        /// </summary>
        /// <param name="opcode">The opcode within the 0xCB prefix to decode.</param>
        /// <returns>The instruction identified by the opcode.</returns>
        private Instruction CreatePrefixedInstruction(byte opcode)
        {
            Instruction instruction = null;
            if (opcode == 0x00 || opcode == 0x01 || opcode == 0x02 || opcode == 0x03 || opcode == 0x04 || opcode == 0x05 || opcode == 0x06 || opcode == 0x07)
                instruction = new RotateLeftInstruction(opcode);
            else if (opcode == 0x08 || opcode == 0x09 || opcode == 0x0A || opcode == 0x0B || opcode == 0x0C || opcode == 0x0D || opcode == 0x0E || opcode == 0x0F)
                instruction = new RotateRightInstruction(opcode);
            else if (opcode == 0x10 || opcode == 0x11 || opcode == 0x12 || opcode == 0x13 || opcode == 0x14 || opcode == 0x15 || opcode == 0x16 || opcode == 0x17)
                instruction = new RotateLeftThroughCarryInstruction(opcode);
            else if (opcode == 0x18 || opcode == 0x19 || opcode == 0x1A || opcode == 0x1B || opcode == 0x1C || opcode == 0x1D || opcode == 0x1E || opcode == 0x1F)
                instruction = new RotateRightThroughCarryInstruction(opcode);
            else if (opcode == 0x20 || opcode == 0x21 || opcode == 0x22 || opcode == 0x23 || opcode == 0x24 || opcode == 0x25 || opcode == 0x26 || opcode == 0x27)
                instruction = new ShiftLeftArithmeticInstruction(opcode);
            else if (opcode == 0x28 || opcode == 0x29 || opcode == 0x2A || opcode == 0x2B || opcode == 0x2C || opcode == 0x2D || opcode == 0x2E || opcode == 0x2F)
                instruction = new ShiftRightArithmeticInstruction(opcode);
            else if (opcode == 0x30 || opcode == 0x31 || opcode == 0x32 || opcode == 0x33 || opcode == 0x34 || opcode == 0x35 || opcode == 0x36 || opcode == 0x37)
                instruction = new SwapInstruction(opcode);
            else if (opcode == 0x38 || opcode == 0x39 || opcode == 0x3A || opcode == 0x3B || opcode == 0x3C || opcode == 0x3D || opcode == 0x3E || opcode == 0x3F)
                instruction = new ShiftRightLogicalInstruction(opcode);
            else if (opcode >= 0x40 && opcode <= 0x7F)
                instruction = new BitInstruction(opcode);
            else if (opcode >= 0x80 && opcode <= 0xBF)
                instruction = new ResetBitInstruction(opcode);
            else if (opcode >= 0xC0 && opcode <= 0xFF)
                instruction = new SetBitInstruction(opcode);
            return instruction;
        }

        /// <summary>
        /// Checks if the opcode represents a load instruction
        /// </summary>
        /// <param name="opcode">The opcode to decode</param>
        /// <returns>True on valid opcode</returns>
        private bool IsLoadInstruction(byte opcode)
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
                case 0x78:
                case 0x79:
                case 0x7A:
                case 0x7B:
                case 0x7C:
                case 0x7D:
                case 0x7F:
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
                case 0x01:
                case 0x11:
                case 0x21:
                case 0x31:
                case 0x36:
                case 0xF8:
                case 0xEA:
                case 0xFA:
                case 0x08:
                    return true;
                default:
                    return false;
            }
        }
    }
}
