using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ZazBoy.Console;
using ZazBoy.UI.Controls;
using Size = Avalonia.Size;

namespace ZazBoy.UI
{
    public class MainWindow : Window
    {
        private EmulatorDisabledNotice emulatorDisabledNotice;
        private Panel contentHolder;
        private Panel dialogShade;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            contentHolder = this.FindControl<Panel>("ContentHolder");
            dialogShade = this.FindControl<Panel>("DialogShade");
            emulatorDisabledNotice = this.FindControl<EmulatorDisabledNotice>("EmulatorDisabledNotice");
            ShowDialogShade(false);
            ShowEmulatorDisabledNotice(false);
            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;
        }

        public void SetActiveContent(Control control)
        {
            contentHolder.Children.RemoveRange(0, contentHolder.Children.Count);
            contentHolder.Children.Add(control);
        }

        private void HandleKeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            SetEmulatorButton(e.Key, false);
        }

        private void HandleKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            SetEmulatorButton(e.Key, true);
        }

        private void SetEmulatorButton(Avalonia.Input.Key key, bool state)
        {
            Joypad joypad = GameBoy.Instance().Joypad;
            switch (key)
            {
                case Avalonia.Input.Key.Down:
                    joypad.SetButton(Joypad.ButtonType.DPadDown, state);
                    break;
                case Avalonia.Input.Key.Up:
                    joypad.SetButton(Joypad.ButtonType.DPadUp, state);
                    break;
                case Avalonia.Input.Key.Left:
                    joypad.SetButton(Joypad.ButtonType.DPadLeft, state);
                    break;
                case Avalonia.Input.Key.Right:
                    joypad.SetButton(Joypad.ButtonType.DPadRight, state);
                    break;

                case Avalonia.Input.Key.Z:
                    joypad.SetButton(Joypad.ButtonType.BtnA, state);
                    break;
                case Avalonia.Input.Key.X:
                    joypad.SetButton(Joypad.ButtonType.BtnB, state);
                    break;
                case Avalonia.Input.Key.Enter:
                    joypad.SetButton(Joypad.ButtonType.BtnStart, state);
                    break;
                case Avalonia.Input.Key.Back:
                    joypad.SetButton(Joypad.ButtonType.BtnSelect, state);
                    break;
            }
        }

        public void ShowDialogShade(bool show)
        {
            dialogShade.IsVisible = show;
        }

        public void ShowEmulatorDisabledNotice(bool show)
        {
            emulatorDisabledNotice.IsVisible = show;
        }
    }
}
