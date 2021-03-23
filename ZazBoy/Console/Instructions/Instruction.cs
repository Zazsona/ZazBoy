using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZazBoy.Console.Operations;

namespace ZazBoy.Console.Instructions
{
    /// <summary>
    /// A "Command" class responsible for the execution of a CPU instruction
    /// </summary>
    public abstract class Instruction : Operation
    {
        /// <summary>
        /// The prefix for the bit operations set
        /// </summary>
        public const byte BitwiseInstructionPrefix = 0xCB;
        /// <summary>
        /// The opcode for the instruction.
        /// </summary>
        public byte opcode { get; private set; }
        /// <summary>
        /// The prefix to signify use of a different optable (only used for bit operations - 0xCB)
        /// </summary>
        public byte opcodePrefix { get; private set; }

        /// <summary>
        /// Creates a new instruction command
        /// </summary>
        /// <param name="opcodePrefix">The prefix to signify use of a different optable (only used for bit operations - 0xCB)</param>
        /// <param name="opcode">The opcode for the instruction</param>
        /// <param name="clocks">The number of clocks required for the instruction. (This may change during execution due to conditional statements)</param>
        public Instruction(byte opcodePrefix, byte opcode, int clocks) : base(clocks)
        {
            this.opcode = opcodePrefix;
            this.opcode = opcode;
        }

        /// <summary>
        /// Progresses the instruction by one clock.
        /// </summary>
        public override void Tick()
        {
            if (executedClocks == 0)
                System.Console.WriteLine("Opcode: " + opcodePrefix +" | "+ opcode);
            base.Tick();
        }
    }
}
