using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions.Prefixed
{
    public class BitInstruction : Instruction
    {
        public BitInstruction(byte opcode) : base(0xCB, opcode, 8)
        {
            byte opcodeLSB = (byte)(opcode & 0x0F);
            if (opcodeLSB == 0x06 || opcodeLSB == 0x0E)
                totalClocks = 12;
            else
                totalClocks = 8;
        }

        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            int bitPosition = ((opcode - 0x40) / 8);
            byte opcodeLSB = (byte)(opcode & 0x0F);
            if (opcodeLSB == 0x00 || opcodeLSB == 0x08)
                FlagBit(cpu, cpu.registerB, bitPosition);
            else if (opcodeLSB == 0x01 || opcodeLSB == 0x09)
                FlagBit(cpu, cpu.registerC, bitPosition);
            else if (opcodeLSB == 0x02 || opcodeLSB == 0x0A)
                FlagBit(cpu, cpu.registerD, bitPosition);
            else if (opcodeLSB == 0x03 || opcodeLSB == 0x0B)
                FlagBit(cpu, cpu.registerE, bitPosition);
            else if (opcodeLSB == 0x04 || opcodeLSB == 0x0C)
                FlagBit(cpu, cpu.registerH, bitPosition);
            else if (opcodeLSB == 0x05 || opcodeLSB == 0x0D)
                FlagBit(cpu, cpu.registerL, bitPosition);
            else if (opcodeLSB == 0x07 || opcodeLSB == 0x0F)
                FlagBit(cpu, cpu.registerA, bitPosition);
            else if (opcodeLSB == 0x06 || opcodeLSB == 0x0E)
            {
                byte value = memMap.Read(cpu.registersHL);
                FlagBit(cpu, value, bitPosition);
            }
        }

        private void FlagBit(CPU cpu, byte value, int bitPosition)
        {
            byte bitMask = ((byte)(1 << bitPosition));
            bool bit = ((value & bitMask) != 0);

            cpu.zeroFlag = !bit;
            cpu.subtractionFlag = false;
            cpu.halfCarryFlag = true;
        }
    }
}
