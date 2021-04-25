using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZazBoy.Console;

namespace ZazBoy.UI.Controls.MemoryInspectorControls
{
    public class MemoryRegionInspectorCategoryItem : InspectorCategoryItem
    {
        private ushort address;
        private byte data;

        public MemoryRegionInspectorCategoryItem() : base()
        {
            dataBox.KeyUp += HandleByteTypeEvent;
        }

        public void SetAddress(ushort address)
        {
            this.address = address;
            string addressText = "#" + address.ToString("X4");
            titleBlock.Text = addressText;
        }

        public ushort GetAddress()
        {
            return address;
        }

        public string GetAddressText()
        {
            return titleBlock.Text;
        }

        public void SetData(byte data)
        {
            string dataText = data.ToString("X2");
            dataBox.BorderBrush = new SolidColorBrush(UIUtil.validTextBoxBorderColor);
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
            if (UIUtil.IsHexByteValid(textBox.Text))
            {
                byte byteValue = UIUtil.ParseHexByte(textBox.Text);
                GameBoy.Instance().MemoryMap.WriteDirect(address, byteValue);
                textBox.BorderBrush = new SolidColorBrush(UIUtil.validTextBoxBorderColor);
            }
            else
                textBox.BorderBrush = new SolidColorBrush(UIUtil.invalidTextBoxBorderColor);
        }
    }
}
