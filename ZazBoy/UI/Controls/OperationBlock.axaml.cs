using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ZazBoy.UI.Controls
{
    public class OperationBlock : UserControl
    {
        public OperationBlock()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
