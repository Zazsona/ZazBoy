using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console
{
    /// <summary>
    /// A class for storing information about a user-modified instruction overwriting the one found in memory
    /// </summary>
    public class InstructionOverride
    {
        public ushort address { get; private set; }
        public byte opcode { get; private set; }
        public byte lowByte { get; private set; }
        public byte highByte { get; private set; }
        public bool isPrefixed { get; private set; }
        public byte overriddenOpcode { get; private set; } 
        public byte overriddenLowByte { get; private set; } 
        public byte overriddenHighByte { get; private set; } //We override instructions, not memory locations. So we need to recall exactly what we're overwriting in case it changes.
        public bool overriddenPrefixed { get; private set; }

        public InstructionOverride(ushort address, bool isPrefixed, byte opcode)
        {
            this.address = address;
            this.isPrefixed = isPrefixed;
            this.opcode = opcode;
            this.lowByte = 0;
            this.highByte = 0;
            ushort opcodeAddress = (ushort)((isPrefixed) ? address + 1 : address);
            this.overriddenOpcode = GameBoy.Instance().MemoryMap.ReadDirect(opcodeAddress);
        }

        public InstructionOverride(ushort address, bool isPrefixed, byte opcode, byte lowByte, byte highByte) : this(address, isPrefixed, opcode)
        {
            this.lowByte = lowByte;
            this.highByte = highByte;
        }

        public InstructionOverride(ushort address, bool isPrefixed, byte opcode, byte lowByte, byte highByte, bool overriddenPrefixed, byte overriddenOpcode, byte overriddenLowByte, byte overriddenHighByte) : this(address, isPrefixed, opcode, lowByte, highByte)
        {
            this.overriddenPrefixed = overriddenPrefixed;
            this.overriddenOpcode = overriddenOpcode;
            this.overriddenLowByte = overriddenLowByte;
            this.overriddenHighByte = overriddenHighByte;
        }

        public bool isOverridingInstruction(bool prefixed, byte opcode, byte lowByte, byte highByte)
        {
            bool prefixMatch = this.overriddenPrefixed == prefixed;
            bool opcodeMatch = this.overriddenOpcode == opcode;
            bool lowByteMatch = this.overriddenLowByte == lowByte;
            bool highByteMatch = this.overriddenHighByte == highByte;
            return (prefixMatch && opcodeMatch && lowByteMatch && highByteMatch);
        }

        public bool isOverridingInstruction(ushort address)
        {
            GameBoy gameBoy = GameBoy.Instance();
            ushort addressIndex = address;
            byte prefix = gameBoy.MemoryMap.ReadDirect(addressIndex);
            bool prefixed = prefix == 0xCB;
            if (prefixed)
                addressIndex++;

            byte opcode = gameBoy.MemoryMap.ReadDirect(addressIndex);
            addressIndex++;
            byte lowByte = gameBoy.MemoryMap.ReadDirect(addressIndex);
            addressIndex++;
            byte highByte = gameBoy.MemoryMap.ReadDirect(addressIndex);

            return isOverridingInstruction(prefixed, opcode, lowByte, highByte);
        }
    }
}
