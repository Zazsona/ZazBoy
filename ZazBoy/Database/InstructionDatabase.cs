using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Database
{
    public class InstructionDatabase
    {
        public Dictionary<string, InstructionEntry> unprefixed;
        public Dictionary<string, InstructionEntry> cbprefixed;
    }
}
