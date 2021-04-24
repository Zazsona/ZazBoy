using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace ZazBoy.UI.Controls.NavBarControls
{
    public class NavBarItem : UserControl
    {
        private Panel itemPanel;
        private TextBlock itemText;

        public NavBarItem()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.itemPanel = this.FindControl<Panel>("NavBarItemPanel");
            this.itemText = this.FindControl<TextBlock>("NavBarItemText");
        }

        public void SetName(string name)
        {
            this.itemText.Text = name;
        }

        public void SetSelected(bool selected)
        {
            if (selected)
            {
                this.Background = new SolidColorBrush(Color.FromRgb(37, 37, 37));
            }
            else
            {
                this.Background = new SolidColorBrush(Color.FromRgb(0,0,0), 0.0f);
            }
        }
    }
}
