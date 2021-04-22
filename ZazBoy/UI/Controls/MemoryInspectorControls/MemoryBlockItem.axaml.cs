using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
        }

        public void SetAddress(ushort address)
        {
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
    }
}
