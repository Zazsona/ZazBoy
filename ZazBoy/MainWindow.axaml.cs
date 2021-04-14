using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using ZazBoy.Console;
using Size = Avalonia.Size;

namespace ZazBoy
{
    public class MainWindow : Window
    {
        private EmulatorControl emulatorControls;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            emulatorControls = this.FindControl<EmulatorControl>("EmuCtrl");
            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;
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

    }
}
