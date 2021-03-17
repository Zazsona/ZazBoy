﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    /// <summary>
    /// A "Command" class responsible for the execution of a CPU instruction
    /// </summary>
    public abstract class Instruction
    {
        /// <summary>
        /// The opcode for the instruction.
        /// </summary>
        public byte opcode { get; private set; }
        /// <summary>
        /// The prefix to signify use of a different optable (only used for bit operations - 0xCB)
        /// </summary>
        public byte opcodePrefix { get; private set; }
        /// <summary>
        /// The total number of clocks required to execute this instruction.<br></br>
        /// DO NOT CACHE THIS VALUE.<br></br>
        /// Some instructions, such as conditional jumps, may modify the required clocks depending on the outcome.
        /// </summary>
        public int totalClocks { get; private set; }
        /// <summary>
        /// The number of clocks executed against this instruction.
        /// </summary>
        public int executedClocks { get; private set; }

        /// <summary>
        /// Creates a new instruction command
        /// </summary>
        /// <param name="opcodePrefix">The prefix to signify use of a different optable (only used for bit operations - 0xCB)</param>
        /// <param name="opcode">The opcode for the instruction</param>
        /// <param name="clocks">The number of clocks required for the instruction. (This may change during execution due to conditional statements)</param>
        public Instruction(byte opcodePrefix, byte opcode, byte clocks)
        {
            this.opcode = opcodePrefix;
            this.opcode = opcode;
            this.totalClocks = clocks;
            this.executedClocks = 0;
        }

        /// <summary>
        /// Progresses the instruction by one clock.
        /// </summary>
        public virtual void Tick()
        {
            executedClocks++;
        }
    }
}
