using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Collections.Generic;
using ZazBoy.UI.Controls.NavBarControls;

namespace ZazBoy.UI
{
    public class NavBar : UserControl
    {
        private NavBarItem selectedNavBarItem;

        private MainWindow mainWindow;
        private EmulatorInterface emulatorControl;
        private BreakpointManager breakpointsControl;
        private PipelineInspector pipelineControl;
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
            emulatorControl = new EmulatorInterface();
            breakpointsControl = new BreakpointManager();
            pipelineControl = new PipelineInspector();
            inspectorControl = new MemoryInspector();
            itemControls.Add(emulatorTabButton, emulatorControl);
            itemControls.Add(breakpointsTabButton, breakpointsControl);
            itemControls.Add(pipelineTabButton, pipelineControl);
            itemControls.Add(inspectorTabButton, inspectorControl);
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
