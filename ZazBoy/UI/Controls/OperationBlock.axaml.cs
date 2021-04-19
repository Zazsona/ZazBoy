using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace ZazBoy.UI.Controls
{
    public class OperationBlock : UserControl
    {
        private bool selected;
        private Border operationBorder;
        private TextBlock mnemonicTag;
        private TextBlock positionTag;

        public OperationBlock()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            operationBorder = this.FindControl<Border>("OperationBorder");
            mnemonicTag = this.FindControl<TextBlock>("MnemonicTag");
            positionTag = this.FindControl<TextBlock>("PositionTag");

            this.PointerReleased += HandleClicked;
        }

        public void SetSelected(bool selected)
        {
            this.selected = selected;
            if (selected)
                this.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0), 0.5);
            else
                this.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0), 0.0);
        }

        private void HandleClicked(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            SetSelected(!selected);
        }

        public void SetMnemonic(string mnemonic)
        {
            mnemonicTag.Text = mnemonic;
        }

        public void SetPosition(string position)
        {
            positionTag.Text = position;
        }

        public string GetMnemonic()
        {
            return mnemonicTag.Text;
        }

        public string GetPosition()
        {
            return positionTag.Text;
        }
    }
}
