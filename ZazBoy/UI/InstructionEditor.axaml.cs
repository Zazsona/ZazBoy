using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ZazBoy.Console;
using ZazBoy.Database;
using ZazBoy.UI.Controls;

namespace ZazBoy.UI
{
    public class InstructionEditor : Window
    {
        private GameBoy gameBoy;
        private InstructionDatabase idb;
        private InstructionEntry instruction;
        private ushort address;
        private bool isPrefixed;

        public InstructionEditor()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void Initialise(GameBoy gameBoy, InstructionDatabase idb, InstructionEntry instruction, ushort address, bool isPrefixed)
        {
            this.gameBoy = gameBoy;
            this.idb = idb;
            this.instruction = instruction;
            this.address = address;
            this.isPrefixed = isPrefixed;

            int instructionBytes = (isPrefixed) ? instruction.bytes-1 : instruction.bytes;
            TextBox instructionTextBox = this.FindControl<TextBox>("InstructionTextBox");
            TextBox lowByteTextBox = this.FindControl<TextBox>("LowByteTextBox");
            TextBox highByteTextBox = this.FindControl<TextBox>("HighByteTextBox");
            instructionTextBox.Text = instruction.GetAssemblyLine();
            if (instructionBytes > 1)
                lowByteTextBox.Text = gameBoy.MemoryMap.ReadDirect((ushort)(address + 1)).ToString();
            else
                lowByteTextBox.IsEnabled = false;

            if (instructionBytes > 2)
                highByteTextBox.Text = gameBoy.MemoryMap.ReadDirect((ushort)(address + 2)).ToString();
            else
                highByteTextBox.IsEnabled = false;

            OperationBlock instructionBlock = this.FindControl<OperationBlock>("InstructionBlock");
            instructionBlock.SetMnemonic(instruction.GetAssemblyLine());
            instructionBlock.SetPosition("#" + address.ToString("X4"));
        }
    }
}
