using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions.Prefixed
{
    public class ResetBitInstruction : Instruction
    {
        public ResetBitInstruction(byte opcode) : base(0xCB, opcode, 8)
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
            byte opcodeLSB = (byte)(opcode & 0x0F);
            int bitPosition = ((opcode - 0x80) / 8);
            if (opcodeLSB == 0x00 || opcodeLSB == 0x08)
                cpu.registerB = ResetBit(cpu.registerB, bitPosition);
            else if (opcodeLSB == 0x01 || opcodeLSB == 0x09)
                cpu.registerC = ResetBit(cpu.registerC, bitPosition);
            else if (opcodeLSB == 0x02 || opcodeLSB == 0x0A)
                cpu.registerD = ResetBit(cpu.registerD, bitPosition);
            else if (opcodeLSB == 0x03 || opcodeLSB == 0x0B)
                cpu.registerE = ResetBit(cpu.registerE, bitPosition);
            else if (opcodeLSB == 0x04 || opcodeLSB == 0x0C)
                cpu.registerH = ResetBit(cpu.registerH, bitPosition);
            else if (opcodeLSB == 0x05 || opcodeLSB == 0x0D)
                cpu.registerL = ResetBit(cpu.registerL, bitPosition);
            else if (opcodeLSB == 0x07 || opcodeLSB == 0x0F)
                cpu.registerA = ResetBit(cpu.registerA, bitPosition);
            else if (opcodeLSB == 0x06 || opcodeLSB == 0x0E)
            {
                byte value = memMap.Read(cpu.registersHL);
                memMap.Write(cpu.registersHL, ResetBit(value, bitPosition));
            }
        }

        private byte ResetBit(byte value, int bitPosition)
        {
            byte bitMask = ((byte)(1 << bitPosition));
            byte invertedBitMask = ((byte)~bitMask);
            value = ((byte)(value & invertedBitMask));
            return value;
        }
    }
}
