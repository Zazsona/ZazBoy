using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ZazBoy.UI.Controls
{
    public class NavBar : UserControl
    {
        public NavBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Image hideButton = this.FindControl<Image>("HideButton");
            hideButton.Source = UIUtil.ConvertDrawingBitmapToUIBitmap(Properties.Resources.HideNavBar);
        }
    }
}
