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
        private InstructionEntry instruction;
        private ushort address;
        private bool isPrefixed;

        private Grid instructionSuggestionsGrid;
        private Border instructionDropdown;
        private TextBox instructionTextBox;
        private OperationBlock instructionDisplayBlock;

        public InstructionEditor()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void Initialise(GameBoy gameBoy, ushort address, bool isPrefixed)
        {
            this.gameBoy = gameBoy;
            this.address = address;
            this.isPrefixed = isPrefixed;
            this.instruction = UIUtil.GetInstructionEntry(gameBoy, address);

            instructionSuggestionsGrid = this.FindControl<Grid>("InstructionSuggestionsGrid");
            instructionDropdown = this.FindControl<Border>("InstructionDropdown");
            int instructionBytes = (isPrefixed) ? instruction.bytes-1 : instruction.bytes;
            instructionDropdown.IsEnabled = false;
            instructionDropdown.IsVisible = false;
            instructionTextBox = this.FindControl<TextBox>("InstructionTextBox");
            TextBox lowByteTextBox = this.FindControl<TextBox>("LowByteTextBox");
            TextBox highByteTextBox = this.FindControl<TextBox>("HighByteTextBox");
            instructionTextBox.KeyUp += HandleInstructionTypeEvent;
            instructionTextBox.Text = instruction.GetAssemblyLine();
            if (instructionBytes > 1)
                lowByteTextBox.Text = gameBoy.MemoryMap.ReadDirect((ushort)(address + 1)).ToString();
            else
                lowByteTextBox.IsEnabled = false;

            if (instructionBytes > 2)
                highByteTextBox.Text = gameBoy.MemoryMap.ReadDirect((ushort)(address + 2)).ToString();
            else
                highByteTextBox.IsEnabled = false;

            instructionDisplayBlock = this.FindControl<OperationBlock>("InstructionDisplayBlock");
            instructionDisplayBlock.SetMnemonic(instruction.GetAssemblyLine());
            instructionDisplayBlock.SetPosition("#" + address.ToString("X4"));
        }

        private void HandleInstructionTypeEvent(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            string instructionSnippet = instructionTextBox.Text.ToLower();
            instructionSuggestionsGrid.RowDefinitions.RemoveRange(0, instructionSuggestionsGrid.RowDefinitions.Count);
            instructionSuggestionsGrid.Children.RemoveRange(0, instructionSuggestionsGrid.Children.Count);
            InstructionDatabase idb = UIUtil.GetInstructionDatabase();

            string enteredOpcode = GetEnteredInstructionOpcode(instructionSnippet);
            if (enteredOpcode != null)
                SetModifiedInstruction(enteredOpcode);
            else
            {
                int index = 0;
                foreach (string opcode in idb.unprefixed.Keys)
                {
                    InstructionEntry instruction = idb.unprefixed[opcode];
                    if (instruction.GetAssemblyLine().ToLower().StartsWith(instructionSnippet) && instruction.bytes == this.instruction.bytes)
                    {
                        RowDefinition rowDefinition = new RowDefinition(1, GridUnitType.Auto);
                        instructionSuggestionsGrid.RowDefinitions.Add(rowDefinition);
                        TextBlock instructionBlock = new TextBlock();
                        instructionBlock.Text = instruction.GetAssemblyLine();
                        instructionBlock.PointerReleased += HandleInstructionSuggestionSelected;
                        Grid.SetRow(instructionBlock, index);
                        instructionSuggestionsGrid.Children.Add(instructionBlock);
                        index++;
                    }
                }
                instructionDropdown.IsEnabled = instructionSuggestionsGrid.Children.Count > 0;
                instructionDropdown.IsVisible = instructionSuggestionsGrid.Children.Count > 0;
            }
        }

        private void HandleInstructionSuggestionSelected(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            TextBlock instructionBlock = (TextBlock)sender;
            string assemblyLine = instructionBlock.Text;
            instructionTextBox.Text = assemblyLine;
            instructionDropdown.IsEnabled = false;
            instructionDropdown.IsVisible = false;

            string enteredOpcode = GetEnteredInstructionOpcode(assemblyLine);
            if (enteredOpcode != null)
                SetModifiedInstruction(enteredOpcode);
        }

        /// <summary>
        /// Gets the opcode for the instruction provided, if it matches a valid assembly line.
        /// </summary>
        /// <param name="instruction">The instruction to get an opcode for.</param>
        /// <returns>The opcode, or null if the instruction is invalid</returns>
        private string GetEnteredInstructionOpcode(string instruction)
        {
            string lowerInstruction = instruction.ToLower();
            InstructionDatabase idb = UIUtil.GetInstructionDatabase();
            foreach (string opcode in idb.unprefixed.Keys)
            {
                InstructionEntry instructionEntry = idb.unprefixed[opcode];
                if (instructionEntry.GetAssemblyLine().ToLower().Equals(lowerInstruction))
                {
                    return opcode;
                }
            }
            return null;
        }

        private void SetModifiedInstruction(string opcode)
        {
            InstructionEntry instructionEntry = UIUtil.GetInstructionDatabase().unprefixed[opcode];
            byte opcodeValue = byte.Parse(opcode.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
            gameBoy.MemoryMap.WriteDirect(address, opcodeValue);
            instructionDisplayBlock.SetMnemonic(instructionEntry.GetAssemblyLine());

        }
    }
}
