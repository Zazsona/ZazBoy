using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ZazBoy.Console;
using ZazBoy.Console.Instructions;
using ZazBoy.Database;

namespace ZazBoy.UI.Controls.Pipeline
{
    public class InstructionEditor : UserControl
    {
        public const string PlaceholderText = "---";

        private GameBoy gameBoy;
        private InstructionEntry instructionEntry;
        private InstructionOverride instructionOverride;

        private Grid instructionSuggestionsGrid;
        private Border instructionDropdown;
        private OperationBlock instructionDisplayBlock;

        private TextBox instructionTextBox;
        private TextBox lowByteTextBox;
        private TextBox highByteTextBox;

        public delegate void InstructionEditedHandler(ushort address, InstructionOverride instructionOverride);
        public event InstructionEditedHandler onInstructionEdited;

        public InstructionEditor()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            instructionDisplayBlock = this.FindControl<OperationBlock>("InstructionDisplayBlock");
            instructionSuggestionsGrid = this.FindControl<Grid>("InstructionSuggestionsGrid");
            instructionDropdown = this.FindControl<Border>("InstructionDropdown");
            instructionDropdown.IsEnabled = false;
            instructionDropdown.IsVisible = false;
            instructionTextBox = this.FindControl<TextBox>("InstructionTextBox");
            lowByteTextBox = this.FindControl<TextBox>("LowByteTextBox");
            highByteTextBox = this.FindControl<TextBox>("HighByteTextBox");
            lowByteTextBox.KeyUp += HandleByteTypeEvent;
            highByteTextBox.KeyUp += HandleByteTypeEvent;
            instructionTextBox.KeyUp += HandleInstructionTypeEvent;
            lowByteTextBox.IsEnabled = false;
            highByteTextBox.IsEnabled = false;
            instructionTextBox.IsEnabled = false;

            Button saveButton = this.FindControl<Button>("SaveButton");
            saveButton.Click += SaveButton_Click;
        }

        private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ApplyInstructionOverride();
        }

        private void ApplyInstructionOverride()
        {
            if (instructionOverride != null)
            {
                SetInstruction(gameBoy, instructionOverride);
                gameBoy.AddInstructionOverride(instructionOverride);
                onInstructionEdited?.Invoke(instructionOverride.address, instructionOverride);
            }
        }

        public void SetInstruction(GameBoy gameBoy, ushort address)
        {
            SetInstruction(gameBoy, GetInstructionOverride(gameBoy, address));
        }

        public void SetInstruction(GameBoy gameBoy, InstructionOverride instructionOverride)
        {
            this.gameBoy = gameBoy;
            this.instructionOverride = instructionOverride;
            InstructionDatabase idb = UIUtil.GetInstructionDatabase();
            string opcodeKey = "0x" + instructionOverride.opcode.ToString("X2");
            this.instructionEntry = (instructionOverride.isPrefixed) ? idb.cbprefixed[opcodeKey] : idb.unprefixed[opcodeKey];

            instructionDisplayBlock.SetMnemonic(instructionEntry.GetAssemblyLine());
            instructionDisplayBlock.SetPosition("#" + instructionOverride.address.ToString("X4"));
            instructionDropdown.IsEnabled = false;
            instructionDropdown.IsVisible = false;
            instructionTextBox.Text = instructionEntry.GetAssemblyLine();
            instructionTextBox.IsEnabled = true;
            instructionTextBox.BorderBrush = new SolidColorBrush(UIUtil.validTextBoxBorderColor);
            lowByteTextBox.BorderBrush = new SolidColorBrush(UIUtil.validTextBoxBorderColor);
            highByteTextBox.BorderBrush = new SolidColorBrush(UIUtil.validTextBoxBorderColor);

            int instructionParameters = (instructionOverride.isPrefixed) ? instructionEntry.bytes - 2 : instructionEntry.bytes - 1;
            if (instructionParameters > 0)
            {
                lowByteTextBox.Text = instructionOverride.lowByte.ToString("X2");
                lowByteTextBox.IsEnabled = true;
            }
            else
            {
                lowByteTextBox.IsEnabled = false;
                lowByteTextBox.Text = PlaceholderText;
            }
            if (instructionParameters > 1)
            {
                highByteTextBox.Text = instructionOverride.highByte.ToString("X2");
                highByteTextBox.IsEnabled = true;
            }
            else
            {
                highByteTextBox.IsEnabled = false;
                highByteTextBox.Text = PlaceholderText;
            }
        }

        private InstructionOverride GetInstructionOverride(GameBoy gameBoy, ushort address)
        {
            InstructionOverride[] instructionOverrides = gameBoy.GetInstructionOverrides(address);
            if (instructionOverrides != null)
            {
                foreach (InstructionOverride addressOverride in instructionOverrides)
                {
                    if (addressOverride.isOverridingInstruction(address))
                    {
                        return addressOverride;
                    }
                }
            }

            ushort prefixAddress = address;
            bool basePrefixed = gameBoy.MemoryMap.ReadDirect(prefixAddress) == Instruction.BitwiseInstructionPrefix;
            ushort opcodeAddress = (ushort)((basePrefixed) ? prefixAddress + 1 : prefixAddress); //If it isn't prefixed, re-use the same address.
            byte baseOpcode = gameBoy.MemoryMap.ReadDirect(opcodeAddress);
            ushort lowByteAddress = (ushort)(opcodeAddress + 1);
            byte baseLowByte = gameBoy.MemoryMap.ReadDirect(lowByteAddress);
            ushort highByteAddress = (ushort)(lowByteAddress + 1);
            byte baseHighByte = gameBoy.MemoryMap.ReadDirect(highByteAddress);
            InstructionOverride instructionOverride = new InstructionOverride(address, basePrefixed, baseOpcode, baseLowByte, baseHighByte, basePrefixed, baseOpcode, baseLowByte, baseHighByte);
            return instructionOverride;
        }

        public void ResetInstruction()
        {
            this.gameBoy = null;
            this.instructionEntry = null;
            this.instructionOverride = null;
            instructionDropdown.IsEnabled = false;
            instructionDropdown.IsVisible = false;
            instructionTextBox.IsEnabled = false;
            lowByteTextBox.IsEnabled = false;
            highByteTextBox.IsEnabled = false;
            instructionTextBox.BorderBrush = new SolidColorBrush(UIUtil.validTextBoxBorderColor);
            lowByteTextBox.BorderBrush = new SolidColorBrush(UIUtil.validTextBoxBorderColor);
            highByteTextBox.BorderBrush = new SolidColorBrush(UIUtil.validTextBoxBorderColor);
            instructionDisplayBlock.SetMnemonic(PlaceholderText);
            instructionDisplayBlock.SetPosition(PlaceholderText);
            instructionTextBox.Text = PlaceholderText;
            lowByteTextBox.Text = PlaceholderText;
            highByteTextBox.Text = PlaceholderText;
        }

        private void HandleInstructionTypeEvent(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            string instructionSnippet = instructionTextBox.Text.ToLower();
            instructionSuggestionsGrid.RowDefinitions.RemoveRange(0, instructionSuggestionsGrid.RowDefinitions.Count);
            instructionSuggestionsGrid.Children.RemoveRange(0, instructionSuggestionsGrid.Children.Count);
            InstructionDatabase idb = UIUtil.GetInstructionDatabase();

            string enteredOpcode = ConvertInstructionToOpcode(instructionSnippet);
            if (enteredOpcode != null)
            {
                instructionOverride.opcode = UIUtil.ParseHexByte(enteredOpcode);
                SetInstruction(gameBoy, instructionOverride);
            }
            else
            {
                int index = 0;
                System.Collections.Generic.Dictionary<string, InstructionEntry>.KeyCollection unprefixedKeys = idb.unprefixed.Keys;
                foreach (string opcode in unprefixedKeys)
                {
                    InstructionEntry instruction = idb.unprefixed[opcode];
                    if (instruction.GetAssemblyLine().ToLower().StartsWith(instructionSnippet))
                    {
                        AddSuggestedInstruction(instruction, index);
                        index++;
                    }
                }

                System.Collections.Generic.Dictionary<string, InstructionEntry>.KeyCollection prefixedKeys = idb.cbprefixed.Keys;
                foreach (string opcode in prefixedKeys)
                {
                    InstructionEntry instruction = idb.cbprefixed[opcode];
                    if (instruction.GetAssemblyLine().ToLower().StartsWith(instructionSnippet))
                    {
                        AddSuggestedInstruction(instruction, index);
                        index++;
                    }
                }
                instructionDropdown.IsEnabled = instructionSuggestionsGrid.Children.Count > 0;
                instructionDropdown.IsVisible = instructionSuggestionsGrid.Children.Count > 0;
            }
        }

        private void AddSuggestedInstruction(InstructionEntry instructionEntry, int rowIndex)
        {
            RowDefinition rowDefinition = new RowDefinition(1, GridUnitType.Auto);
            instructionSuggestionsGrid.RowDefinitions.Add(rowDefinition);
            TextBlock instructionBlock = new TextBlock();
            instructionBlock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)); //Allows for non-text areas to capture clicks.
            instructionBlock.Text = instructionEntry.GetAssemblyLine();
            instructionBlock.PointerReleased += HandleInstructionSuggestionSelected;
            Grid.SetRow(instructionBlock, rowIndex);
            instructionSuggestionsGrid.Children.Add(instructionBlock);
        }

        private void HandleInstructionSuggestionSelected(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            TextBlock instructionBlock = (TextBlock)sender;
            string assemblyLine = instructionBlock.Text;
            instructionTextBox.Text = assemblyLine;
            instructionDropdown.IsEnabled = false;
            instructionDropdown.IsVisible = false;

            string enteredOpcode = ConvertInstructionToOpcode(assemblyLine);
            if (enteredOpcode != null)
            {
                instructionOverride.opcode = UIUtil.ParseHexByte(enteredOpcode);
                SetInstruction(gameBoy, instructionOverride);
            }
        }

        /// <summary>
        /// Gets the opcode for the instruction provided, if it matches a valid assembly line.
        /// </summary>
        /// <param name="instruction">The instruction to get an opcode for.</param>
        /// <returns>The opcode, or null if the instruction is invalid</returns>
        private string ConvertInstructionToOpcode(string instruction)
        {
            instruction = instruction.ToLower();
            InstructionDatabase idb = UIUtil.GetInstructionDatabase();
            System.Collections.Generic.Dictionary<string, InstructionEntry>.KeyCollection unprefixedKeys = idb.unprefixed.Keys;
            foreach (string opcode in unprefixedKeys)
            {
                InstructionEntry instructionEntry = idb.unprefixed[opcode];
                if (instructionEntry.GetAssemblyLine().ToLower().Equals(instruction))
                    return opcode;
            }
            System.Collections.Generic.Dictionary<string, InstructionEntry>.KeyCollection prefixedKeys = idb.cbprefixed.Keys;
            foreach (string opcode in prefixedKeys)
            {
                InstructionEntry instructionEntry = idb.cbprefixed[opcode];
                if (instructionEntry.GetAssemblyLine().ToLower().Equals(instruction))
                    return opcode;
            }
            return null;
        }

        private void HandleByteTypeEvent(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            bool isHighByte = (textBox == highByteTextBox);
            if (UIUtil.IsHexByteValid(textBox.Text))
            {
                byte byteValue = UIUtil.ParseHexByte(textBox.Text);
                if (isHighByte)
                    instructionOverride.highByte = byteValue;
                else
                    instructionOverride.lowByte = byteValue;
                textBox.BorderBrush = new SolidColorBrush(UIUtil.validTextBoxBorderColor);
            }
            else
                textBox.BorderBrush = new SolidColorBrush(UIUtil.invalidTextBoxBorderColor);
        }
    }
}
