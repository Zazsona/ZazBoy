using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Text.RegularExpressions;
using ZazBoy.Console;
using static ZazBoy.Console.CPU;

namespace ZazBoy.UI.Controls.MemoryInspectorControls
{
    public class RegistersInspectorCategoryItem : InspectorCategoryItem
    {
        private RegisterType register;
        private RegisterPairType registerPair;
        private bool isPairedRegister;

        public void SetRegister(RegisterType register)
        {
            this.register = register;
            isPairedRegister = false;
            titleBlock.Text = GetRegisterPrettyName(register);
            dataBox.Text = GetRegisterValue(register).ToString("X2");
            dataBox.BorderBrush = new SolidColorBrush(UIUtil.validTextBoxBorderColor);
            dataBox.KeyUp -= HandleUShortTypeEvent;
            dataBox.KeyUp -= HandleByteTypeEvent;
            dataBox.KeyUp += HandleByteTypeEvent;
        }

        public void SetRegister(RegisterPairType register)
        {
            this.registerPair = register;
            isPairedRegister = true;
            titleBlock.Text = GetRegisterPrettyName(register);
            dataBox.Text = GetRegisterValue(registerPair).ToString("X4");
            dataBox.BorderBrush = new SolidColorBrush(UIUtil.validTextBoxBorderColor);
            dataBox.KeyUp -= HandleByteTypeEvent;
            dataBox.KeyUp -= HandleUShortTypeEvent;
            dataBox.KeyUp += HandleUShortTypeEvent;
        }

        public string GetDataText()
        {
            return dataBox.Text;
        }

        private string GetRegisterPrettyName(RegisterType registerType)
        {
            switch (register)
            {
                case RegisterType.A:
                    return "Accumulator (Register A)";
                case RegisterType.B:
                    return "Register B";
                case RegisterType.C:
                    return "Register C";
                case RegisterType.D:
                    return "Register D";
                case RegisterType.E:
                    return "Register E";
                case RegisterType.F:
                    return "Flags (Register F)";
                case RegisterType.H:
                    return "Register H";
                case RegisterType.L:
                    return "Register L";
                default:
                    return "Unknown Register";
            }
        }

        private string GetRegisterPrettyName(RegisterPairType registerType)
        {
            switch (registerPair)
            {
                case RegisterPairType.AF:
                    return "Register Pair AF";
                case RegisterPairType.BC:
                    return "Register Pair BC";
                case RegisterPairType.DE:
                    return "Register Pair DE";
                case RegisterPairType.HL:
                    return "Register Pair HL";
                case RegisterPairType.PC:
                    return "Program Counter";
                case RegisterPairType.SP:
                    return "Stack Pointer";
                default:
                    return "Unknown Register Pair";
            }
        }

        private byte GetRegisterValue(RegisterType register)
        {
            GameBoy gameBoy = GameBoy.Instance();
            switch (register)
            {
                case RegisterType.A:
                    return gameBoy.CPU.registerA;
                case RegisterType.B:
                    return gameBoy.CPU.registerB;
                case RegisterType.C:
                    return gameBoy.CPU.registerC;
                case RegisterType.D:
                    return gameBoy.CPU.registerD;
                case RegisterType.E:
                    return gameBoy.CPU.registerE;
                case RegisterType.F:
                    return gameBoy.CPU.registerF;
                case RegisterType.H:
                    return gameBoy.CPU.registerH;
                case RegisterType.L:
                    return gameBoy.CPU.registerL;
                default:
                    return 0;
            }
        }

        private void SetRegisterValue(RegisterType register, byte value)
        {
            GameBoy gameBoy = GameBoy.Instance();
            switch (register)
            {
                case RegisterType.A:
                    gameBoy.CPU.registerA = value;
                    return;
                case RegisterType.B:
                    gameBoy.CPU.registerB = value;
                    return;
                case RegisterType.C:
                    gameBoy.CPU.registerC = value;
                    return;
                case RegisterType.D:
                    gameBoy.CPU.registerD = value;
                    return;
                case RegisterType.E:
                    gameBoy.CPU.registerE = value;
                    return;
                case RegisterType.F:
                    gameBoy.CPU.registerF = value;
                    return;
                case RegisterType.H:
                    gameBoy.CPU.registerH = value;
                    return;
                case RegisterType.L:
                    gameBoy.CPU.registerL = value;
                    return;
            }
        }

        private ushort GetRegisterValue(RegisterPairType registerPair)
        {
            GameBoy gameBoy = GameBoy.Instance();
            switch (registerPair)
            {
                case RegisterPairType.AF:
                    return gameBoy.CPU.registersAF;
                case RegisterPairType.BC:
                    return gameBoy.CPU.registersBC;
                case RegisterPairType.DE:
                    return gameBoy.CPU.registersDE;
                case RegisterPairType.HL:
                    return gameBoy.CPU.registersHL;
                case RegisterPairType.PC:
                    return gameBoy.CPU.programCounter;
                case RegisterPairType.SP:
                    return gameBoy.CPU.stackPointer;
                default:
                    return 0;
            }
        }

        private void SetRegisterValue(RegisterPairType registerPair, ushort value)
        {
            GameBoy gameBoy = GameBoy.Instance();
            switch (registerPair)
            {
                case RegisterPairType.AF:
                    gameBoy.CPU.registersAF = value;
                    return;
                case RegisterPairType.BC:
                    gameBoy.CPU.registersBC = value;
                    return;
                case RegisterPairType.DE:
                    gameBoy.CPU.registersDE = value;
                    return;
                case RegisterPairType.HL:
                    gameBoy.CPU.registersHL = value;
                    return;
                case RegisterPairType.PC:
                    gameBoy.CPU.programCounter = value;
                    return;
                case RegisterPairType.SP:
                    gameBoy.CPU.stackPointer = value;
                    return;
            }
        }

        private void HandleByteTypeEvent(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (UIUtil.IsHexByteValid(textBox.Text))
            {
                byte byteValue = UIUtil.ParseHexByte(textBox.Text);
                SetRegisterValue(register, byteValue);
                textBox.BorderBrush = new SolidColorBrush(UIUtil.validTextBoxBorderColor);
            }
            else
                textBox.BorderBrush = new SolidColorBrush(UIUtil.invalidTextBoxBorderColor);
        }

        private void HandleUShortTypeEvent(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (UIUtil.IsHexUShortValid(textBox.Text))
            {
                ushort ushortValue = UIUtil.ParseHexUShort(textBox.Text);
                SetRegisterValue(registerPair, ushortValue);
                textBox.BorderBrush = new SolidColorBrush(UIUtil.validTextBoxBorderColor);
            }
            else
                textBox.BorderBrush = new SolidColorBrush(UIUtil.invalidTextBoxBorderColor);
        }
    }
}
