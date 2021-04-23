using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using ZazBoy.Console;

namespace ZazBoy.UI.Controls.MemoryInspectorControls
{
    public abstract class InspectorCategoryControl : UserControl
    {
        public const int ITEMS_PER_PAGE = 32;
        protected GameBoy gameBoy;
        protected TextBlock categoryNameBlock;
        protected Button categoryExpandButton;
        protected Panel categoryContents;
        protected Grid memoryItemGrid;

        protected MemoryBlockItem[] memoryItems;
        protected Button backButton;
        protected Button nextButton;

        protected bool expanded;
        protected int length;
        protected int currentPage;

        public InspectorCategoryControl()
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
            HideContents();
        }

        private void HandleCategoryExpandToggle(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (expanded)
                HideContents();
            else
                ShowContents();
        }

        public virtual void Initialise(GameBoy gameBoy, string categoryName, int length)
        {
            this.gameBoy = gameBoy;
            this.categoryNameBlock.Text = categoryName;
            this.length = length;
        }

        public virtual void HideContents()
        {
            this.expanded = false;
            this.categoryExpandButton.Content = "+";
            this.categoryContents.IsVisible = false;
        }

        public virtual void ShowContents()
        {
            this.currentPage = 0;
            this.expanded = true;
            this.categoryExpandButton.Content = "-";
            this.categoryContents.IsVisible = true;
            if (length < ITEMS_PER_PAGE)
            {
                nextButton.IsEnabled = false;
                backButton.IsEnabled = false;
            }
            ShowPage(currentPage);
        }

        protected virtual void ShowPage(int pageNo)
        {

        }

        private void NextButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (((currentPage+1) * ITEMS_PER_PAGE) >= length)
                currentPage = 0;
            else
                currentPage++;
            ShowPage(currentPage);
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
            ShowPage(currentPage);
        }
    }
}
