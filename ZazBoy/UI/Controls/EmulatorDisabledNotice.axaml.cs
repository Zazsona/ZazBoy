using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ZazBoy.UI.Controls
{
    public class EmulatorDisabledNotice : UserControl
    {
        public EmulatorDisabledNotice()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
