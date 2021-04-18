using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Database
{
    public class InstructionEntry
    {
        public string mnemonic;
        public int bytes;
        public int[] cycles;
        public Operand[] operands;
        public bool immediate;
        public Flags flags;
    }
}
