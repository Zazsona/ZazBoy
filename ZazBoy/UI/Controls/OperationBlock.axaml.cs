using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ZazBoy.UI.Controls
{
    public class OperationBlock : UserControl
    {
        private TextBlock mnemonicTag;
        private TextBlock positionTag;

        public OperationBlock()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            mnemonicTag = this.FindControl<TextBlock>("MnemonicTag");
            positionTag = this.FindControl<TextBlock>("PositionTag");
        }

        public void SetMnemonic(string mnemonic)
        {
            mnemonicTag.Text = mnemonic;
        }

        public void SetPosition(string position)
        {
            positionTag.Text = position;
        }
    }
}
