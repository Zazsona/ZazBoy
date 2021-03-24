using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions.Prefixed
{
    public class SwapInstruction : Instruction
    {
        public SwapInstruction(byte opcode) : base(0xCB, opcode, 8)
        {
            switch (opcode)
            {
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x34:
                case 0x35:
                case 0x37:
                    totalClocks = 8;
                    break;
                case 0x36:
                    totalClocks = 16;
                    break;
            }
        }

        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            MemoryMap memMap = GameBoy.Instance().MemoryMap;

            switch (opcode)
            {
                case 0x30:
                    cpu.registerB = SwapNibbles(cpu, cpu.registerB);
                    break;
                case 0x31:
                    cpu.registerC = SwapNibbles(cpu, cpu.registerC);
                    break;
                case 0x32:
                    cpu.registerD = SwapNibbles(cpu, cpu.registerD);
                    break;
                case 0x33:
                    cpu.registerE = SwapNibbles(cpu, cpu.registerE);
                    break;
                case 0x34:
                    cpu.registerH = SwapNibbles(cpu, cpu.registerH);
                    break;
                case 0x35:
                    cpu.registerL = SwapNibbles(cpu, cpu.registerL);
                    break;
                case 0x37:
                    cpu.registerA = SwapNibbles(cpu, cpu.registerA);
                    break;
                case 0x36:
                    byte value = memMap.Read(cpu.registersHL);
                    memMap.Write(cpu.registersHL, SwapNibbles(cpu, value));
                    break;
            }
        }

        private byte SwapNibbles(CPU cpu, byte value)
        {
            byte lsb = (byte)(value & 0x0F);
            byte msb = (byte)(value / 0x10);
            byte newMsb = (byte)(lsb * 0x10);
            byte newLsb = msb;
            byte swappedByte = ((byte)(newMsb | newLsb));

            cpu.zeroFlag = swappedByte == 0;
            cpu.carryFlag = false;
            cpu.halfCarryFlag = false;
            cpu.subtractionFlag = false;
            return swappedByte;
        }
    }
}
