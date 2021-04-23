using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using ZazBoy.Console;

namespace ZazBoy.UI.Controls.MemoryInspectorControls
{
    public class MemoryCategoryControl : UserControl
    {
        private const int ITEMS_PER_PAGE = 32;
        private GameBoy gameBoy;
        private TextBlock categoryNameBlock;
        private Button categoryExpandButton;
        private Panel categoryContents;
        private Grid memoryItemGrid;

        private MemoryBlockItem[] memoryItems;
        private Button backButton;
        private Button nextButton;

        private bool expanded;
        private ushort startAddress;
        private int length;
        private int currentPage;

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
            backButton = this.FindControl<Button>("BackButton");
            nextButton = this.FindControl<Button>("NextButton");
            categoryExpandButton.Click += HandleCategoryExpandToggle;
            backButton.Click += BackButton_Click;
            nextButton.Click += NextButton_Click;

            memoryItems = new MemoryBlockItem[ITEMS_PER_PAGE];
            for (int i = 0; i < ITEMS_PER_PAGE; i++)
            {
                RowDefinition rowDefinition = new RowDefinition(1, GridUnitType.Auto);
                memoryItemGrid.RowDefinitions.Add(rowDefinition);
                MemoryBlockItem memoryItem = new MemoryBlockItem();
                Grid.SetRow(memoryItem, (memoryItemGrid.RowDefinitions.Count - 1));
                memoryItemGrid.Children.Add(memoryItem);
                memoryItems[i] = memoryItem;
            }
            HideAddresses();
        }

        private void HandleCategoryExpandToggle(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (expanded)
                HideAddresses();
            else
                DisplayAddresses(startAddress, length);
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

        public void HideAddresses()
        {
            this.expanded = false;
            this.categoryExpandButton.Content = "+";
            this.categoryContents.IsVisible = false;
        }

        public void DisplayAddresses(ushort startAddress, int length)
        {
            this.startAddress = startAddress;
            this.length = length;
            this.currentPage = 0;
            this.expanded = true;
            this.categoryExpandButton.Content = "-";
            this.categoryContents.IsVisible = true;
            if (length < ITEMS_PER_PAGE)
            {
                nextButton.IsEnabled = false;
                backButton.IsEnabled = false;
            }
            LoadAddressPage(currentPage);
        }

        private void LoadAddressPage(int pageNo)
        {
            ushort pageStartAddress = (ushort)(startAddress + (pageNo * ITEMS_PER_PAGE));
            int pageStartIndex = Math.Abs(pageStartAddress - startAddress);
            int itemsToDisplay = Math.Min(ITEMS_PER_PAGE, length-pageStartIndex);

            for (int i = 0; i < ITEMS_PER_PAGE; i++)
            {
                ushort address = (ushort)unchecked(pageStartAddress + i); //Allow it to overflow so that length can be achieved.

                MemoryBlockItem memoryItem = memoryItems[i];
                if (i < itemsToDisplay)
                {
                    memoryItem.IsVisible = true;
                    memoryItem.SetAddress(address);
                    memoryItem.SetData(gameBoy.MemoryMap.ReadDirect(address));
                }
                else
                    memoryItem.IsVisible = false;
            }
        }

        private void NextButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (((currentPage+1) * ITEMS_PER_PAGE) >= length)
                currentPage = 0;
            else
                currentPage++;
            LoadAddressPage(currentPage);
        }

        private void BackButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (currentPage == 0)
            {
                int topPage = (int) (Math.Ceiling(length / (ITEMS_PER_PAGE/1.0f))-1);
                currentPage = topPage;
            }
            else
            {
                currentPage--;
            }

            LoadAddressPage(currentPage);
        }
    }
}
