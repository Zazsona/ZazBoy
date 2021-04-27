using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZazBoy.Console.Operations;
using ZazBoy.Database;

namespace ZazBoy.Console.Instructions
{
    /// <summary>
    /// A "Command" class responsible for the execution of a CPU instruction
    /// </summary>
    public abstract class Instruction : Operation
    {
        private static InstructionDatabase instructionDatabase;
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
        /// The database entry that relates to this instruction.
        /// </summary>
        public InstructionEntry instructionDatabaseEntry { get; private set; }
        /// <summary>
        /// The override bytes. Set to null for normal execution
        /// </summary>
        public InstructionOverride overrideContext { get; set; }
        /// <summary>
        /// Whether this instruction is an override or not.
        /// </summary>
        public bool isOverride { get => overrideContext != null; }

        /// <summary>
        /// Creates a new instruction command
        /// </summary>
        /// <param name="opcodePrefix">The prefix to signify use of a different optable (only used for bit operations - 0xCB)</param>
        /// <param name="opcode">The opcode for the instruction</param>
        /// <param name="clocks">The number of clocks required for the instruction. (This may change during execution due to conditional statements)</param>
        public Instruction(byte opcodePrefix, byte opcode, int clocks) : base(clocks)
        {
            if (instructionDatabase == null)
                instructionDatabase = JsonConvert.DeserializeObject<InstructionDatabase>(Properties.Resources.InstructionDatabase);

            bool isPrefixed = opcodePrefix == 0;
            string opcodeHex = "0x" + opcode.ToString("X2");
            this.instructionDatabaseEntry = (isPrefixed) ? instructionDatabase.cbprefixed[opcodeHex] : instructionDatabase.unprefixed[opcodeHex];
            this.opcode = opcodePrefix;
            this.opcode = opcode;
        }

        /// <summary>
        /// Progresses the instruction by one clock.
        /// </summary>
        public override void Tick()
        {
            if (GameBoy.Instance().DEBUG_MODE && executedClocks == 0)
                System.Console.WriteLine("Opcode: " + opcodePrefix +" | "+ opcode);
            base.Tick();
        }

        public override void Reset()
        {
            base.Reset();
            overrideContext = null;
        }

        protected override ushort Get16BitImmediate()
        {
            if (overrideContext == null)
                return base.Get16BitImmediate();
            else
            {
                CPU cpu = GameBoy.Instance().CPU;
                byte lsb = overrideContext.lowByte;
                cpu.IncrementProgramCounter();
                byte msb = overrideContext.highByte;
                cpu.IncrementProgramCounter();

                ushort value = (ushort)(msb * 0x100);
                value += lsb;
                return value;
            }
        }

        protected override byte Get8BitImmediate()
        {
            if (overrideContext == null)
                return base.Get8BitImmediate();
            else
            {
                CPU cpu = GameBoy.Instance().CPU;
                byte lsb = overrideContext.lowByte;
                cpu.IncrementProgramCounter();
                return lsb;
            }
        }
    }
}
