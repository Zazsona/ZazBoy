using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ZazBoy.UI
{
    public class PipelineInspector : UserControl
    {
        public PipelineInspector()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
