using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;

namespace ZazBoy.UI.Controls.Pipeline
{
    public class OperationQueue : UserControl
    {
        private OperationBlock[] operationBlocks;
        private OperationBlock selectedOperationBlock;

        public OperationQueue()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Grid operationsList = this.FindControl<Grid>("OperationsList");
            operationBlocks = new OperationBlock[operationsList.RowDefinitions.Count];
            for (int i = 0; i < operationBlocks.Length; i++)
            {
                OperationBlock operationBlock = this.FindControl<OperationBlock>("OperationBlock" + i);
                operationBlocks[i] = operationBlock;
                operationBlock.PointerReleased += HandleOperationBlockSelected;
            }
            Image cpuIcon = this.FindControl<Image>("CPUIcon");
            cpuIcon.Source = UIUtil.ConvertDrawingBitmapToUIBitmap(Properties.Resources.CPUIcon);
        }

        private void HandleOperationBlockSelected(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (selectedOperationBlock != null)
                selectedOperationBlock.SetSelected(false);

            OperationBlock operationBlock = (OperationBlock)sender;
            if (operationBlock == selectedOperationBlock)
                selectedOperationBlock = null;
            else
                selectedOperationBlock = (OperationBlock)sender;
        }

        public OperationBlock[] GetOperationBlocks()
        {
            return operationBlocks;
        }

        public OperationBlock GetSelectedBlock()
        {
            return selectedOperationBlock;
        }
    }
}
