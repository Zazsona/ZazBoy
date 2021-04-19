using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Globalization;
using System.Text.RegularExpressions;
using ZazBoy.Console;

namespace ZazBoy.UI
{
    public class BreakpointManager : Window
    {
        private GameBoy gameBoy;

        private Button addButton;
        private TextBox dataTextBox;
        private Button removeButton;

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

            addButton.Click += AddBreakpoint;
            removeButton.Click += RemoveBreakpoint;
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

        private void AddBreakpoint(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (gameBoy.IsPaused)
            {
                string address = dataTextBox.Text;
                if (IsAddressValid(address))
                {
                    address = address.Replace("0x", "").Replace("#", "");
                    ushort addressValue = ushort.Parse(address, NumberStyles.HexNumber);
                    gameBoy.Breakpoints.Add(addressValue);
                    dataTextBox.Text = "";
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
                    address = address.Replace("0x", "").Replace("#", "");
                    ushort addressValue = ushort.Parse(address, NumberStyles.HexNumber);
                    gameBoy.Breakpoints.Remove(addressValue);
                    dataTextBox.Text = "";
                }
            }
        }
    }
}
