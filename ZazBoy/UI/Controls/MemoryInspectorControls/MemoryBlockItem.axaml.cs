using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Text.RegularExpressions;
using ZazBoy.Console;

namespace ZazBoy.UI.Controls.MemoryInspectorControls
{
    public class MemoryBlockItem : UserControl
    {
        private ushort address;
        private byte data;
        private TextBlock addressBlock;
        private TextBox dataBox;

        public MemoryBlockItem()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            addressBlock = this.FindControl<TextBlock>("AddressBlock");
            dataBox = this.FindControl<TextBox>("DataBox");
            dataBox.KeyUp += HandleByteTypeEvent;
        }

        public void SetAddress(ushort address)
        {
            this.address = address;
            string addressText = "#" + address.ToString("X4");
            addressBlock.Text = addressText;
        }

        public ushort GetAddress()
        {
            return address;
        }

        public string GetAddressText()
        {
            return addressBlock.Text;
        }

        public void SetData(byte data)
        {
            string dataText = data.ToString("X2");
            dataBox.Text = dataText;
        }

        public byte GetData()
        {
            return data;
        }

        public string GetByteText()
        {
            return dataBox.Text;
        }

        private void HandleByteTypeEvent(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (Regex.IsMatch(textBox.Text, "[0-9a-fA-F]+"))
            {
                int value = int.Parse(textBox.Text, System.Globalization.NumberStyles.HexNumber);
                if (value >= 0 && value <= byte.MaxValue)
                {
                    byte byteValue = (byte)value;
                    GameBoy.Instance().MemoryMap.WriteDirect(address, byteValue);
                }
            }
        }
    }
}
