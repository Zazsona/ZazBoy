using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    public class AddInstruction : Instruction
    {
        public AddInstruction(byte opcode) : base(0x00, opcode, 4)
        {
            switch (opcode)
            {
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x87:
                    totalClocks = 4;
                    break;
                case 0x09:
                case 0x19:
                case 0x29:
                case 0x39:
                case 0x86:
                case 0xC6:
                    totalClocks = 8;
                    break;
                case 0xE8:
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
                case 0x80:
                    ApplyAddition(cpu, cpu.registerA, cpu.registerB, false);
                    break;
                case 0x81:
                    ApplyAddition(cpu, cpu.registerA, cpu.registerC, false);
                    break;
                case 0x82:
                    ApplyAddition(cpu, cpu.registerA, cpu.registerD, false);
                    break;
                case 0x83:
                    ApplyAddition(cpu, cpu.registerA, cpu.registerE, false);
                    break;
                case 0x84:
                    ApplyAddition(cpu, cpu.registerA, cpu.registerH, false);
                    break;
                case 0x85:
                    ApplyAddition(cpu, cpu.registerA, cpu.registerL, false);
                    break;
                case 0x87:
                    ApplyAddition(cpu, cpu.registerA, cpu.registerA, false);
                    break;
                case 0x09:
                    ApplyHLAddition(cpu, cpu.registersHL, cpu.registersBC);
                    break;
                case 0x19:
                    ApplyHLAddition(cpu, cpu.registersHL, cpu.registersDE);
                    break;
                case 0x29:
                    ApplyHLAddition(cpu, cpu.registersHL, cpu.registersHL);
                    break;
                case 0x39:
                    ApplyHLAddition(cpu, cpu.registersHL, cpu.stackPointer);
                    break;
                case 0x86:
                    ApplyAddition(cpu, cpu.registerA, memMap.Read(cpu.registersHL), false);
                    break;
                case 0xC6:
                    ApplyAddition(cpu, cpu.registerA, Get8BitImmediate(), false);
                    break;
                case 0xE8:
                    ApplySPAddition(cpu, cpu.stackPointer, (sbyte)Get8BitImmediate());
                    break;
            }
        }

        private void ApplyAddition(CPU cpu, byte firstOperand, byte secondOperand, bool applyCarry)
        {
            byte carry = (byte)((cpu.carryFlag && applyCarry) ? 1 : 0);
            byte result = (byte)(firstOperand + secondOperand + carry);

            cpu.subtractionFlag = false;
            cpu.zeroFlag = result == 0;
            cpu.carryFlag = ((firstOperand & 0xFF) + (secondOperand & 0xFF) + carry) > 0xFF;
            cpu.halfCarryFlag = ((firstOperand & 0x0F) + (secondOperand & 0x0F) + carry) > 0x0F;
            cpu.registerA = (byte)result;
        }

        private void ApplyHLAddition(CPU cpu, ushort firstOperand, ushort secondOperand)
        {
            cpu.registersHL = (ushort)(firstOperand + secondOperand);
            cpu.subtractionFlag = false;
            cpu.halfCarryFlag = ((firstOperand & 0xFFF) + (secondOperand & 0xFFF)) > 0xFFF;
            cpu.carryFlag = ((uint)firstOperand + (uint)secondOperand) > 0xFFFF;
        }

        private void ApplySPAddition(CPU cpu, ushort firstOperand, sbyte secondOperand)
        {
            cpu.stackPointer = (ushort)(firstOperand + secondOperand);
            cpu.zeroFlag = false;
            cpu.subtractionFlag = false;
            cpu.halfCarryFlag = (firstOperand & 0x0F) + (secondOperand & 0x0F) > 0x0F;
            cpu.carryFlag = (firstOperand & 0xFF) + (secondOperand & 0xFF) > 0xFF;
        }
    }
}
