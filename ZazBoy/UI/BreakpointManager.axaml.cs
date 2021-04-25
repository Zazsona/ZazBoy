using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ZazBoy.Console;
using ZazBoy.UI.Controls;

namespace ZazBoy.UI
{
    public class BreakpointManager : UserControl
    {
        private GameBoy gameBoy;

        private Button addButton;
        private TextBox dataTextBox;
        private Button removeButton;
        private Grid breakpointsGrid;
        private OperationBlock selectedOperationBlock;

        public BreakpointManager()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            gameBoy = GameBoy.Instance();
            addButton = this.FindControl<Button>("AddButton");
            dataTextBox = this.FindControl<TextBox>("DataTextBox");
            removeButton = this.FindControl<Button>("RemoveButton");
            breakpointsGrid = this.FindControl<Grid>("BreakpointsGrid");

            dataTextBox.KeyUp += HandleAddressModified;
            addButton.Click += AddBreakpoint;
            removeButton.Click += RemoveBreakpoint;
            UpdateBreakpointsGrid();
        }

        private void HandleAddressModified(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            TextBox dataBox = (TextBox)sender;
            bool isAddressValid = UIUtil.IsHexUShortValid(dataBox.Text);
            if (isAddressValid)
                dataBox.BorderBrush = new SolidColorBrush(UIUtil.validTextBoxBorderColor);
            else
                dataBox.BorderBrush = new SolidColorBrush(UIUtil.invalidTextBoxBorderColor);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            UpdateBreakpointsGrid();
        }

        private void UpdateBreakpointsGrid()
        {
            breakpointsGrid.RowDefinitions.RemoveRange(0, breakpointsGrid.RowDefinitions.Count);
            breakpointsGrid.Children.RemoveRange(0, breakpointsGrid.Children.Count);
            int index = 0;
            foreach (ushort address in gameBoy.Breakpoints)
            {
                RowDefinition rowDefinition = new RowDefinition(1, GridUnitType.Auto);
                breakpointsGrid.RowDefinitions.Add(rowDefinition);
                OperationBlock bpListing = new OperationBlock();
                bpListing.PointerReleased += HandleOperationBlockSelected;
                bpListing.SetMnemonic("#" + address.ToString("X4"));
                bpListing.SetPosition(MemoryMap.GetAddressLocationName(address));
                Grid.SetRow(bpListing, index);
                breakpointsGrid.Children.Add(bpListing);
                index++;
            }
        }

        private void HandleOperationBlockSelected(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (selectedOperationBlock != null)
                selectedOperationBlock.SetSelected(false);


            OperationBlock operationBlock = (OperationBlock)sender;
            if (operationBlock == selectedOperationBlock)
                selectedOperationBlock = null;
            else
            {
                selectedOperationBlock = (OperationBlock)sender;
                dataTextBox.Text = selectedOperationBlock.GetMnemonic();
            }
        }

        private void AddBreakpoint(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (gameBoy.IsPaused || !gameBoy.IsPoweredOn)
            {
                string address = dataTextBox.Text;
                if (UIUtil.IsHexUShortValid(address))
                {
                    ushort addressValue = UIUtil.ParseHexUShort(address);
                    gameBoy.Breakpoints.Add(addressValue);
                    dataTextBox.Text = "";
                    UpdateBreakpointsGrid();
                }
            }
        }

        private void RemoveBreakpoint(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (gameBoy.IsPaused || !gameBoy.IsPoweredOn)
            {
                string address = dataTextBox.Text;
                if (UIUtil.IsHexUShortValid(address))
                {
                    ushort addressValue = UIUtil.ParseHexUShort(address);
                    gameBoy.Breakpoints.Remove(addressValue);
                    dataTextBox.Text = "";
                    UpdateBreakpointsGrid();
                }
            }
        }
    }
}
