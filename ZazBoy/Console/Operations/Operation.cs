using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Operations
{
    /// <summary>
    /// A "Command" class responsible for the execution of a CPU operation
    /// </summary>
    public abstract class Operation
    {
        /// <summary>
        /// The total number of clocks required to execute this operation.
        /// </summary>
        public int totalClocks { get; protected set; }
        /// <summary>
        /// The number of clocks executed against this operation.
        /// </summary>
        public int executedClocks { get; private set; }
        /// <summary>
        /// Signifies if this operation has completed execution
        /// </summary>
        public bool isComplete { get; protected set; }

        /// <summary>
        /// Creates a new operation command
        /// </summary>
        /// <param name="clocks">The number of clocks required for the operation. (This may change during execution)</param>
        public Operation(int clocks)
        {
            this.totalClocks = clocks;
            this.executedClocks = 0;
        }

        /// <summary>
        /// Progresses the operation by one clock.
        /// </summary>
        public virtual void Tick()
        {
            if (executedClocks == 0)
            {
                if (GameBoy.Instance().DEBUG_MODE)
                    System.Console.WriteLine(GameBoy.Instance().CPU.programCounter + ": Running " + this.GetType().Name);
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
            cpu.IncrementProgramCounter();
            return value;
        }

        protected ushort Get16BitImmediate()
        {
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            CPU cpu = GameBoy.Instance().CPU;
            byte lsb = memMap.Read(cpu.programCounter);
            cpu.IncrementProgramCounter();                       //Game Boy is little endian, so LSB then MSB
            byte msb = memMap.Read(cpu.programCounter);
            cpu.IncrementProgramCounter();

            ushort value = (ushort)(msb * 0x100);
            value += lsb;
            return value;
        }
    }
}
