using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class LoadInstruction : Instruction
    {
        public LoadInstruction(byte opcode, int clocks) : base(0x00, opcode, clocks)
        {
        }

        /// <summary>
        /// Enacts the load operation
        /// </summary>
        protected override void Execute()
        {
            MemoryMap memMap = GameBoy.Instance().MemoryMap;
            CPU cpu = GameBoy.Instance().CPU;
            switch (opcode)
            {
                case 0x01:
                    cpu.registersBC = Get16BitImmediate();
                    break;
                case 0x02:
                    memMap.Write(cpu.registersBC, cpu.registerA);
                    break;
                case 0x06:
                    cpu.registerB = Get8BitImmediate();
                    break;
                case 0x08:
                    ushort memoryAddress = Get16BitImmediate();
                    memMap.Write(memoryAddress, ((byte)(cpu.stackPointer & 0x00FF)));
                    memoryAddress++;
                    memMap.Write(memoryAddress, (byte)(cpu.stackPointer / 0x100));
                    break;
                case 0x0A:
                    cpu.registerA = memMap.Read(cpu.registersBC);
                    break;
                case 0x0E:
                    cpu.registerC = Get8BitImmediate();
                    break;
                case 0x11:
                    cpu.registersDE = Get16BitImmediate();
                    break;
                case 0x12:
                    memMap.Write(cpu.registersDE, cpu.registerA);
                    break;
                case 0x16:
                    cpu.registerD = Get8BitImmediate();
                    break;
                case 0x1A:
                    cpu.registerA = memMap.Read(cpu.registersDE);
                    break;
                case 0x1E:
                    cpu.registerE = Get8BitImmediate();
                    break;
                case 0x21:
                    cpu.registersHL = Get16BitImmediate();
                    break;
                case 0x22:
                    memMap.Write(cpu.registersHL, cpu.registerA);
                    cpu.registersHL++;
                    break;
                case 0x26:
                    cpu.registerH = Get8BitImmediate();
                    break;
                case 0x2A:
                    cpu.registerA = memMap.Read(cpu.registersHL);
                    cpu.registersHL++;
                    break;
                case 0x2E:
                    cpu.registerL = Get8BitImmediate();
                    break;
                case 0x31:
                    cpu.stackPointer = Get16BitImmediate();
                    break;
                case 0x32:
                    memMap.Write(cpu.registersHL, cpu.registerA);
                    cpu.registersHL--;
                    break;
                case 0x36:
                    memMap.Write(cpu.registersHL, Get8BitImmediate());
                    break;
                case 0x3A:
                    cpu.registerA = memMap.Read(cpu.registersHL);
                    cpu.registersHL--;
                    break;
                case 0x3E:
                    cpu.registerA = Get8BitImmediate();
                    break;
                case 0x40:
                    cpu.registerB = cpu.registerB; //Yes, this is an actual opcode. In effect, it's identical to NOP.
                    break;
                case 0x41:
                    cpu.registerB = cpu.registerC;
                    break;
                case 0x42:
                    cpu.registerB = cpu.registerD;
                    break;
                case 0x43:
                    cpu.registerB = cpu.registerE;
                    break;
                case 0x44:
                    cpu.registerB = cpu.registerH;
                    break;
                case 0x45:
                    cpu.registerB = cpu.registerL;
                    break;
                case 0x46:
                    cpu.registerB = memMap.Read(cpu.registersHL);
                    break;
                case 0x47:
                    cpu.registerB = cpu.registerA;
                    break;
                case 0x48:
                    cpu.registerC = cpu.registerB;
                    break;
                case 0x49:
                    cpu.registerC = cpu.registerC;
                    break;
                case 0x4A:
                    cpu.registerC = cpu.registerD;
                    break;
                case 0x4B:
                    cpu.registerC = cpu.registerE;
                    break;
                case 0x4C:
                    cpu.registerC = cpu.registerH;
                    break;
                case 0x4D:
                    cpu.registerC = cpu.registerL;
                    break;
                case 0x4E:
                    cpu.registerC = memMap.Read(cpu.registersHL);
                    break;
                case 0x4F:
                    cpu.registerC = cpu.registerA;
                    break;
                case 0x50:
                    cpu.registerD = cpu.registerB;
                    break;
                case 0x51:
                    cpu.registerD = cpu.registerC;
                    break;
                case 0x52:
                    cpu.registerD = cpu.registerD;
                    break;
                case 0x53:
                    cpu.registerD = cpu.registerE;
                    break;
                case 0x54:
                    cpu.registerD = cpu.registerH;
                    break;
                case 0x55:
                    cpu.registerD = cpu.registerL;
                    break;
                case 0x56:
                    cpu.registerD = memMap.Read(cpu.registersHL);
                    break;
                case 0x57:
                    cpu.registerD = cpu.registerA;
                    break;
                case 0x58:
                    cpu.registerE = cpu.registerB;
                    break;
                case 0x59:
                    cpu.registerE = cpu.registerC;
                    break;
                case 0x5A:
                    cpu.registerE = cpu.registerD;
                    break;
                case 0x5B:
                    cpu.registerE = cpu.registerE;
                    break;
                case 0x5C:
                    cpu.registerE = cpu.registerH;
                    break;
                case 0x5D:
                    cpu.registerE = cpu.registerL;
                    break;
                case 0x5E:
                    cpu.registerE = memMap.Read(cpu.registersHL);
                    break;
                case 0x5F:
                    cpu.registerE = cpu.registerA;
                    break;
                case 0x60:
                    cpu.registerH = cpu.registerB;
                    break;
                case 0x61:
                    cpu.registerH = cpu.registerC;
                    break;
                case 0x62:
                    cpu.registerH = cpu.registerD;
                    break;
                case 0x63:
                    cpu.registerH = cpu.registerE;
                    break;
                case 0x64:
                    cpu.registerH = cpu.registerH;
                    break;
                case 0x65:
                    cpu.registerH = cpu.registerL;
                    break;
                case 0x66:
                    cpu.registerH = memMap.Read(cpu.registersHL);
                    break;
                case 0x67:
                    cpu.registerH = cpu.registerA;
                    break;
                case 0x68:
                    cpu.registerL = cpu.registerB;
                    break;
                case 0x69:
                    cpu.registerL = cpu.registerC;
                    break;
                case 0x6A:
                    cpu.registerL = cpu.registerD;
                    break;
                case 0x6B:
                    cpu.registerL = cpu.registerE;
                    break;
                case 0x6C:
                    cpu.registerL = cpu.registerH;
                    break;
                case 0x6D:
                    cpu.registerL = cpu.registerL;
                    break;
                case 0x6E:
                    cpu.registerL = memMap.Read(cpu.registersHL);
                    break;
                case 0x6F:
                    cpu.registerL = cpu.registerA;
                    break;
                case 0x70:
                    memMap.Write(cpu.registersHL, cpu.registerB);
                    break;
                case 0x71:
                    memMap.Write(cpu.registersHL, cpu.registerC);
                    break;
                case 0x72:
                    memMap.Write(cpu.registersHL, cpu.registerD);
                    break;
                case 0x73:
                    memMap.Write(cpu.registersHL, cpu.registerE);
                    break;
                case 0x74:
                    memMap.Write(cpu.registersHL, cpu.registerH);
                    break;
                case 0x75:
                    memMap.Write(cpu.registersHL, cpu.registerL);
                    break;
                case 0x77:
                    memMap.Write(cpu.registersHL, cpu.registerA);
                    break;
                case 0x78:
                    cpu.registerA = cpu.registerB;
                    break;
                case 0x79:
                    cpu.registerA = cpu.registerC;
                    break;
                case 0x7A:
                    cpu.registerA = cpu.registerD;
                    break;
                case 0x7B:
                    cpu.registerA = cpu.registerE;
                    break;
                case 0x7C:
                    cpu.registerA = cpu.registerH;
                    break;
                case 0x7D:
                    cpu.registerA = cpu.registerL;
                    break;
                case 0x7E:
                    cpu.registerA = memMap.Read(cpu.registersHL);
                    break;
                case 0x7F:
                    cpu.registerA = cpu.registerA;
                    break;
                case 0xE2:
                    memMap.Write((ushort)(0xFF00 + cpu.registerC), cpu.registerA);
                    break;
                case 0xEA:
                    memMap.Write(Get16BitImmediate(), cpu.registerA);
                    break;
                case 0xF2:
                    cpu.registerA = memMap.Read((ushort)(0xFF00 + cpu.registerC));
                    break;
                case 0xF8:
                    sbyte signedByte = unchecked((sbyte)Get8BitImmediate());
                    cpu.registersHL = (ushort)(cpu.stackPointer + signedByte);
                    break;
                case 0xF9:
                    cpu.stackPointer = cpu.registersHL;
                    break;
                case 0xFA:
                    cpu.registerA = memMap.Read(Get16BitImmediate());
                    break;
            }
        }
    }
}
