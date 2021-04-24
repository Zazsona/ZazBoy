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
        private Panel pipelineControl;
        private BreakpointManager breakpointsControl;
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
            emulatorControl = new EmulatorControl();
            pipelineControl = new Panel();
            breakpointsControl = new BreakpointManager();
            inspectorControl = new MemoryInspector();
            itemControls.Add(emulatorTabButton, emulatorControl);
            itemControls.Add(pipelineTabButton, pipelineControl);
            itemControls.Add(breakpointsTabButton, breakpointsControl);
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
