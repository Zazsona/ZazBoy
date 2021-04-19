using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Globalization;
using System.Text.RegularExpressions;
using ZazBoy.Console;
using ZazBoy.UI.Controls;

namespace ZazBoy.UI
{
    public class BreakpointManager : Window
    {
        private GameBoy gameBoy;

        private Button addButton;
        private TextBox dataTextBox;
        private Button removeButton;
        private Grid breakpointsGrid;
        private OperationBlock selectedOperationBlock;

        public BreakpointManager()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            gameBoy = GameBoy.Instance();
            addButton = this.FindControl<Button>("AddButton");
            dataTextBox = this.FindControl<TextBox>("DataTextBox");
            removeButton = this.FindControl<Button>("RemoveButton");
            breakpointsGrid = this.FindControl<Grid>("BreakpointsGrid");

            addButton.Click += AddBreakpoint;
            removeButton.Click += RemoveBreakpoint;
            UpdateBreakpointsGrid();
        }

        private bool IsAddressValid(string address)
        {
            address = address.Replace("0x", "").Replace("#", "");
            if (address.Length <= 5 && Regex.IsMatch(address, "[0-9a-fA-F]+"))
            {
                int value = int.Parse(address, NumberStyles.HexNumber);
                if (value > 0 && value <= 65535)
                    return true;
            }
            return false;
        }

        private ushort ParseAddress(string address)
        {
            address = address.Replace("0x", "").Replace("#", "");
            ushort addressValue = ushort.Parse(address, NumberStyles.HexNumber);
            return addressValue;
        }

        private void UpdateBreakpointsGrid()
        {
            breakpointsGrid.RowDefinitions.RemoveRange(0, breakpointsGrid.RowDefinitions.Count);
            breakpointsGrid.Children.RemoveRange(0, breakpointsGrid.Children.Count);
            int index = 0;
            foreach (ushort address in gameBoy.Breakpoints)
            {
                RowDefinition rowDefinition = new RowDefinition(1, GridUnitType.Auto);
                breakpointsGrid.RowDefinitions.Add(rowDefinition);
                OperationBlock bpListing = new OperationBlock();
                bpListing.PointerReleased += HandleOperationBlockSelected;
                bpListing.SetMnemonic("#" + address.ToString("X4"));
                bpListing.SetPosition("Foo");
                Grid.SetRow(bpListing, index);
                breakpointsGrid.Children.Add(bpListing);
                index++;
            }
        }

        private void HandleOperationBlockSelected(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (selectedOperationBlock != null)
                selectedOperationBlock.SetSelected(false);


            OperationBlock operationBlock = (OperationBlock)sender;
            if (operationBlock == selectedOperationBlock)
                selectedOperationBlock = null;
            else
            {
                selectedOperationBlock = (OperationBlock)sender;
                dataTextBox.Text = selectedOperationBlock.GetMnemonic();
            }
        }

        private void AddBreakpoint(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (gameBoy.IsPaused)
            {
                string address = dataTextBox.Text;
                if (IsAddressValid(address))
                {
                    ushort addressValue = ParseAddress(address);
                    gameBoy.Breakpoints.Add(addressValue);
                    dataTextBox.Text = "";
                    UpdateBreakpointsGrid();
                }
            }
        }

        private void RemoveBreakpoint(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (gameBoy.IsPaused)
            {
                string address = dataTextBox.Text;
                if (IsAddressValid(address))
                {
                    ushort addressValue = ParseAddress(address);
                    gameBoy.Breakpoints.Remove(addressValue);
                    dataTextBox.Text = "";
                    UpdateBreakpointsGrid();
                }
            }
        }
    }
}
