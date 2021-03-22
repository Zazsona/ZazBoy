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
                    ApplySPAddition(cpu, cpu.stackPointer, unchecked((sbyte)Get8BitImmediate()));
                    break;
            }
        }

        private void ApplyAddition(CPU cpu, int firstOperand, int secondOperand, bool applyCarry)
        {
            int result = (firstOperand + secondOperand);
            int carry = (applyCarry) ? 1 : 0;
            result += carry;

            cpu.subtractionFlag = false;
            cpu.zeroFlag = result == 0;
            cpu.carryFlag = result > byte.MaxValue;
            cpu.halfCarryFlag = ((((firstOperand & 0x0F) + (secondOperand & 0x0F) + (carry & 0x0F)) & 0x10) == 0x10);

            if (result > byte.MaxValue)
                result -= byte.MaxValue;
            cpu.registerA = (byte)result;
        }

        private void ApplyHLAddition(CPU cpu, ushort firstOperand, ushort secondOperand)
        {
            bool zeroFlag = cpu.zeroFlag;
            byte firstOperandLSB = (byte)(firstOperand & 0xFF);
            byte secondOperandLSB = (byte)(secondOperand & 0xFF);
            ApplyAddition(cpu, firstOperandLSB, secondOperandLSB, false);
            byte lsbResult = cpu.registerA;

            byte firstOperandMSB = (byte)(firstOperand / 0x100);
            byte secondOperandMSB = (byte)(secondOperand / 0x100);
            ApplyAddition(cpu, firstOperandMSB, secondOperandMSB, cpu.carryFlag);    //This overrides the flags. That's just how the GB does it due to an 8-bit ALU
            byte msbResult = cpu.registerA;

            ushort value = (ushort)(msbResult * 0x100);
            value += lsbResult;
            cpu.registersHL = value;
            cpu.zeroFlag = zeroFlag;
        }

        private void ApplySPAddition(CPU cpu, ushort firstOperand, sbyte secondOperand)
        {
            byte firstOperandLSB = (byte)(firstOperand & 0xFF);
            ApplyAddition(cpu, firstOperandLSB, secondOperand, false);
            byte byteResult = cpu.registerA;

            ushort result = (ushort)(firstOperand & 0xFF00);
            result += byteResult;
            if (cpu.carryFlag)
                result += byte.MaxValue;

            cpu.zeroFlag = false;
            cpu.stackPointer = result;
        }
    }
}
