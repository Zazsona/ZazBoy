using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Text.RegularExpressions;
using ZazBoy.Console;

namespace ZazBoy.UI.Controls.MemoryInspectorControls
{
    public class InspectorCategoryItem : UserControl
    {
        protected TextBlock titleBlock;
        protected TextBox dataBox;

        public InspectorCategoryItem()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            titleBlock = this.FindControl<TextBlock>("TitleBlock");
            dataBox = this.FindControl<TextBox>("DataBox");
        }
    }
}
