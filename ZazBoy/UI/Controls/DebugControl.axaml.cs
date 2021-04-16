using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ZazBoy.UI.Controls
{
    public class DebugControl : UserControl
    {
        public DebugControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            this.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        }
    }
}
