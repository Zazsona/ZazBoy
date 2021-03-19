using System;
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
        /// Some instructions, such as conditional jumps, may modify the required clocks in the constructor depending on the outcome.
        /// </summary>
        public int totalClocks { get; protected set; }
        /// <summary>
        /// The number of clocks executed against this instruction.
        /// </summary>
        public int executedClocks { get; private set; }
        /// <summary>
        /// Signifies if this instruction has completed execution
        /// </summary>
        public bool isComplete { get; protected set; }

        /// <summary>
        /// Creates a new instruction command
        /// </summary>
        /// <param name="opcodePrefix">The prefix to signify use of a different optable (only used for bit operations - 0xCB)</param>
        /// <param name="opcode">The opcode for the instruction</param>
        /// <param name="clocks">The number of clocks required for the instruction. (This may change during execution due to conditional statements)</param>
        public Instruction(byte opcodePrefix, byte opcode, int clocks)
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
            if (executedClocks == 0)
            {
                System.Console.WriteLine(GameBoy.Instance().CPU.programCounter+": Running "+this.GetType().Name+" (" + opcode + ")");
                Execute();
            }
            executedClocks++;
            if (executedClocks == totalClocks)
                isComplete = true;
        }

        protected abstract void Execute();

        protected byte Get8BitImmediate()
        {
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            CPU cpu = GameBoy.Instance().CPU;
            byte value = memMap.Read(cpu.programCounter);
            cpu.programCounter++;
            return value;
        }

        protected ushort Get16BitImmediate()
        {
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            CPU cpu = GameBoy.Instance().CPU;
            byte lsb = memMap.Read(cpu.programCounter);
            cpu.programCounter++;                       //Game Boy is little endian, so LSB then MSB
            byte msb = memMap.Read(cpu.programCounter);
            cpu.programCounter++;

            ushort value = (ushort)(msb * 0x100);
            value += lsb;
            return value;
        }
    }
}
