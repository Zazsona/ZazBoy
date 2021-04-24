using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace ZazBoy.UI.Controls
{
    public class NavBar : UserControl
    {
        private NavBarItem selectedNavBarItem;

        public NavBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Image hideButton = this.FindControl<Image>("HideButton");
            hideButton.Source = UIUtil.ConvertDrawingBitmapToUIBitmap(Properties.Resources.HideNavBar);

            NavBarItem emulatorTabButton = this.FindControl<NavBarItem>("EmulatorTabButton");
            NavBarItem pipelineTabButton = this.FindControl<NavBarItem>("PipelineTabButton");
            NavBarItem breakpointsTabButton = this.FindControl<NavBarItem>("BreakpointsTabButton");
            NavBarItem inspectorTabButton = this.FindControl<NavBarItem>("InspectorTabButton");
            emulatorTabButton.SetName("Emulator");
            pipelineTabButton.SetName("Pipeline");
            breakpointsTabButton.SetName("Breakpoints");
            inspectorTabButton.SetName("Inspector");
            emulatorTabButton.PointerPressed += HandleItemSelected;
            pipelineTabButton.PointerPressed += HandleItemSelected;
            breakpointsTabButton.PointerPressed += HandleItemSelected;
            inspectorTabButton.PointerPressed += HandleItemSelected;
            SelectNavBarItem(emulatorTabButton);
        }

        private void HandleItemSelected(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            NavBarItem navBarItem = (NavBarItem)sender;
            SelectNavBarItem(navBarItem);
        }

        private void SelectNavBarItem(NavBarItem navBarItem)
        {
            if (navBarItem != selectedNavBarItem)
            {
                DeselectNavBarItem();
                navBarItem.Background = new SolidColorBrush(Color.FromRgb(37, 37, 37));
                selectedNavBarItem = navBarItem;
            }
        }

        private void DeselectNavBarItem()
        {
            if (selectedNavBarItem != null)
            {
                selectedNavBarItem.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                selectedNavBarItem = null;
            }
        }
    }
}
