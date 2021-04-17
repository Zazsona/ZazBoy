using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading;
using ZazBoy.Console;

namespace ZazBoy.UI.Controls
{
    public class DebugControl : UserControl
    {
        private Grid operationsList;

        public DebugControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            this.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
            operationsList = this.FindControl<Grid>("OperationsList");
            Image cpuIcon = this.FindControl<Image>("CPUIcon");
            cpuIcon.Source = UIUtil.ConvertDrawingBitmapToUIBitmap(Properties.Resources.CPUIcon);

            Button stepButton = this.FindControl<Button>("StepButton");
            //stepButton.PointerReleased += HandleStepOperation;
            stepButton.Click += StepButton_Click;
        }

        private void StepButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            GameBoy.Instance().IsStepping = true;
            GameBoy.Instance().IsPaused = false;
        }
    }
}
