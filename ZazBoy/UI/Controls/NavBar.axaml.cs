using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Collections.Generic;

namespace ZazBoy.UI.Controls
{
    public class NavBar : UserControl
    {
        private NavBarItem selectedNavBarItem;

        private MainWindow mainWindow;
        private EmulatorControl emulatorControl;
        private BreakpointManager breakpointsControl;
        private Panel pipelineControl;
        private MemoryInspector inspectorControl;

        private Dictionary<NavBarItem, Control> itemControls;

        public NavBar()
        {
            itemControls = new Dictionary<NavBarItem, Control>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Image hideButton = this.FindControl<Image>("HideButton");
            hideButton.Source = UIUtil.ConvertDrawingBitmapToUIBitmap(Properties.Resources.HideNavBar);

            NavBarItem emulatorTabButton = this.FindControl<NavBarItem>("EmulatorTabButton");
            NavBarItem breakpointsTabButton = this.FindControl<NavBarItem>("BreakpointsTabButton");
            NavBarItem pipelineTabButton = this.FindControl<NavBarItem>("PipelineTabButton");
            NavBarItem inspectorTabButton = this.FindControl<NavBarItem>("InspectorTabButton");
            emulatorTabButton.SetName("Emulator");
            breakpointsTabButton.SetName("Breakpoints");
            pipelineTabButton.SetName("Pipeline");
            inspectorTabButton.SetName("Inspector");
            emulatorTabButton.PointerPressed += HandleItemSelected;
            breakpointsTabButton.PointerPressed += HandleItemSelected;
            pipelineTabButton.PointerPressed += HandleItemSelected;
            inspectorTabButton.PointerPressed += HandleItemSelected;
            emulatorControl = new EmulatorControl();
            breakpointsControl = new BreakpointManager();
            pipelineControl = new Panel();
            inspectorControl = new MemoryInspector();
            itemControls.Add(emulatorTabButton, emulatorControl);
            itemControls.Add(breakpointsTabButton, breakpointsControl);
            itemControls.Add(pipelineTabButton, pipelineControl);
            itemControls.Add(inspectorTabButton, inspectorControl);

            pipelineControl.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            mainWindow = (MainWindow)this.VisualRoot;
            SelectNavBarItem(this.FindControl<NavBarItem>("EmulatorTabButton"));
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
                mainWindow.SetActiveContent(itemControls[navBarItem]);
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
