using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ZazBoy.Console;

namespace ZazBoy.UI.Controls.MemoryInspectorControls
{
    public class MemoryCategoryControl : UserControl
    {
        private GameBoy gameBoy;
        private TextBlock categoryNameBlock;
        private Button categoryExpandButton;
        private Panel categoryContents;
        private Grid memoryItemGrid;

        private bool expanded;
        private ushort startAddress;
        private int length;

        public MemoryCategoryControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            categoryNameBlock = this.FindControl<TextBlock>("CategoryNameBlock");
            categoryExpandButton = this.FindControl<Button>("CategoryExpandButton");
            categoryContents = this.FindControl<Panel>("CategoryContents");
            memoryItemGrid = this.FindControl<Grid>("MemoryItemGrid");
            categoryExpandButton.Click += HandleCategoryExpandToggle;
            UnloadAddresses();
        }

        private void HandleCategoryExpandToggle(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (expanded)
                UnloadAddresses();
            else
                LoadAddresses(startAddress, length);
        }

        public void Initialise(GameBoy gameBoy, string categoryName)
        {
            this.gameBoy = gameBoy;
            this.categoryNameBlock.Text = categoryName;
        }

        public void Initialise(GameBoy gameBoy, string categoryName, ushort startAddress, int length)
        {
            Initialise(gameBoy, categoryName);
            this.startAddress = startAddress;
            this.length = length;
        }

        public void UnloadAddresses()
        {
            this.expanded = false;
            this.categoryExpandButton.Content = "+";
            this.categoryContents.IsVisible = false;
            memoryItemGrid.Children.RemoveRange(0, memoryItemGrid.Children.Count);
        }

        public void LoadAddresses(ushort startAddress, int length)
        {
            this.startAddress = startAddress;
            this.length = length;
            this.expanded = true;
            this.categoryExpandButton.Content = "-";
            this.categoryContents.IsVisible = true;
            for (int i = 0; i<length; i++)
            {
                ushort address = (ushort)unchecked(startAddress + i); //Allow it to overflow so that length can be achieved.
                RowDefinition rowDefinition = new RowDefinition(1, GridUnitType.Auto);
                memoryItemGrid.RowDefinitions.Add(rowDefinition);

                MemoryBlockItem memoryItem = new MemoryBlockItem();
                memoryItem.SetAddress(address);
                memoryItem.SetData(gameBoy.MemoryMap.ReadDirect(address));
                Grid.SetRow(memoryItem, (memoryItemGrid.RowDefinitions.Count - 1));

                memoryItemGrid.Children.Add(memoryItem);
            }
        }
    }
}
