using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ZazBoy.UI.Controls
{
    public class MessageBox : Window
    {
        private TextBlock messageBlock;
        private Button okButton;

        public MessageBox()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.messageBlock = this.FindControl<TextBlock>("MessageBlock");
            this.okButton = this.FindControl<Button>("OkButton");
            okButton.Click += OkButton_Click;
        }

        private void OkButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }

        public void SetMessage(string message)
        {
            this.messageBlock.Text = message;
        }


    }
}
