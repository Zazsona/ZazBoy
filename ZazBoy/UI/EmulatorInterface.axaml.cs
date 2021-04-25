using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using ZazBoy.Console;
using ZazBoy.UI.Controls;
using ZazBoy.UI.Controls.EmulatorInterface;
using static ZazBoy.Console.GameBoy;
using static ZazBoy.Console.LCD;
using Bitmap = System.Drawing.Bitmap;
using Size = Avalonia.Size;

namespace ZazBoy.UI
{
    public class EmulatorInterface : UserControl
    {
        private GameBoy gameBoy;

        private Panel cartridgeSelectPanel;
        private Button cartridgeButton;

        private DockPanel emulatorView;
        private Grid emulatorRoot;
        private EmulatorDisplay display;

        private DebugControl debugControl;
        private ColumnDefinition debugColumn;
        private bool debugControlActive;

        private PauseHandler pauseHandler;
        private ResumeHandler resumeHandler;
        private Avalonia.Controls.Image pauseButton;
        private Avalonia.Controls.Image pauseText;
        private Avalonia.Media.Imaging.Bitmap resumeTextBitmap;
        private Avalonia.Media.Imaging.Bitmap pauseTextBitmap;
        private Avalonia.Media.Imaging.Bitmap buttonDefaultBitmap;
        private Avalonia.Media.Imaging.Bitmap buttonPressedBitmap;

        public EmulatorInterface()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            display = this.FindControl<EmulatorDisplay>("Display");
            debugControl = new DebugControl();
            debugColumn = new ColumnDefinition();
            debugColumn.Width = new GridLength(1, GridUnitType.Star);
            debugControlActive = false;

            cartridgeSelectPanel = this.FindControl<Panel>("CartridgeSelectPanel");
            cartridgeButton = this.FindControl<Button>("CartridgeButton");
            cartridgeButton.Click += HandleCartridgeButtonClick;

            emulatorRoot = this.FindControl<Grid>("EmulatorRoot");
            emulatorView = this.FindControl<DockPanel>("EmulatorView");

            pauseButton = this.FindControl<Avalonia.Controls.Image>("PauseButton");
            pauseText = this.FindControl<Avalonia.Controls.Image>("PauseText");
            Avalonia.Controls.Image powerButton = this.FindControl<Avalonia.Controls.Image>("PowerButton");
            Avalonia.Controls.Image powerText = this.FindControl<Avalonia.Controls.Image>("PowerText");
            Avalonia.Controls.Image debugButton = this.FindControl<Avalonia.Controls.Image>("DebugButton");
            Avalonia.Controls.Image debugText = this.FindControl<Avalonia.Controls.Image>("DebugText");

            Avalonia.Media.Imaging.Bitmap powerTextBitmap;
            Avalonia.Media.Imaging.Bitmap debugTextBitmap;
            buttonDefaultBitmap = UIUtil.ConvertDrawingBitmapToUIBitmap(Properties.Resources.ButtonBackground);
            buttonPressedBitmap = UIUtil.ConvertDrawingBitmapToUIBitmap(Properties.Resources.ButtonPressedBackground);
            resumeTextBitmap = UIUtil.ConvertDrawingBitmapToUIBitmap(Properties.Resources.ResumeText);
            pauseTextBitmap = UIUtil.ConvertDrawingBitmapToUIBitmap(Properties.Resources.PauseText);
            powerTextBitmap = UIUtil.ConvertDrawingBitmapToUIBitmap(Properties.Resources.PowerText);
            debugTextBitmap = UIUtil.ConvertDrawingBitmapToUIBitmap(Properties.Resources.DebugText);

            pauseHandler = (ushort programCounter) => { Dispatcher.UIThread.Post(() => pauseText.Source = resumeTextBitmap); };
            resumeHandler = () => { Dispatcher.UIThread.Post(() => pauseText.Source = pauseTextBitmap); };
            pauseButton.Source = buttonDefaultBitmap;
            pauseText.Source = pauseTextBitmap;
            pauseButton.PointerPressed += HandleButtonPressed;
            pauseButton.PointerReleased += HandleButtonReleased;
            powerButton.Source = buttonDefaultBitmap;
            powerText.Source = powerTextBitmap;
            powerButton.PointerPressed += HandleButtonPressed;
            powerButton.PointerReleased += HandleButtonReleased;
            debugButton.Source = buttonDefaultBitmap;
            debugText.Source = debugTextBitmap;
            debugButton.PointerPressed += HandleButtonPressed;
            debugButton.PointerReleased += HandleButtonReleased;

            debugButton.PointerReleased += HandleDebugDisplay;
            pauseButton.PointerReleased += HandlePauseResume;
            powerButton.PointerReleased += HandlePower;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            MainWindow mainWindow = (MainWindow)e.Root;
            mainWindow.KeyDown += HandleKeyDown;
            mainWindow.KeyUp += HandleKeyUp;
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            MainWindow mainWindow = (MainWindow)e.Root;
            mainWindow.KeyDown -= HandleKeyDown;
            mainWindow.KeyUp -= HandleKeyUp;
            if (gameBoy != null)
                gameBoy.IsPaused = true;
        }

        private async void ShowFileDialog()
        {
            MainWindow window = (MainWindow)this.VisualRoot;
            window.ShowDialogShade(true);
            OpenFileDialog romSelectDialog = new OpenFileDialog();
            romSelectDialog.Title = "Select ROM...";
            FileDialogFilter allFilter = new FileDialogFilter() { Name = "All Files", Extensions = { "*" } };
            FileDialogFilter gbFilter = new FileDialogFilter() { Name = "GB (Game Boy)", Extensions = { "gb" } };
            FileDialogFilter romFilter = new FileDialogFilter() { Name = "ROM", Extensions = { "rom" } };
            romSelectDialog.Filters.Add(gbFilter);
            romSelectDialog.Filters.Add(romFilter);
            romSelectDialog.Filters.Add(allFilter);
            string[] selectedFiles = await romSelectDialog.ShowAsync(window);
            window.ShowDialogShade(false);
            if (selectedFiles.Length > 0)
            {
                string selectedFile = selectedFiles[0];
                StartEmulator(selectedFile);
                cartridgeButton.Click -= HandleCartridgeButtonClick;
                cartridgeButton.Focusable = false;
                cartridgeSelectPanel.Focusable = false;
                cartridgeSelectPanel.IsVisible = false;
                cartridgeSelectPanel.IsEnabled = false;
                window.Focus();
                //No filetype check here because, hey, if people want to see what happens when they load a .png or whatever, I'm not going to stop it.
            }
        }

        private void StartEmulator(string cartridgePath)
        {
            GameBoy gameBoy = GameBoy.Instance();
            gameBoy.LoadCartridge(cartridgePath);
            gameBoy.SetPowerOn(true);
            HookToGameBoy(gameBoy);
        }

        private void HookToGameBoy(GameBoy gameBoy)
        {
            if (this.gameBoy != null && this.gameBoy.LCD != null)
            {
                this.gameBoy.onEmulatorPaused -= pauseHandler;
                this.gameBoy.onEmulatorResumed -= resumeHandler;
            }

            this.gameBoy = gameBoy;
            gameBoy.onEmulatorPaused += pauseHandler;
            gameBoy.onEmulatorResumed += resumeHandler;
            //debugControl.HookToGameBoy(gameBoy);
        }

        private void HandleButtonPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Avalonia.Controls.Image button = (Avalonia.Controls.Image)sender;
            button.Source = buttonPressedBitmap;
        }

        private void HandleButtonReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            Avalonia.Controls.Image button = (Avalonia.Controls.Image)sender;
            button.Source = buttonDefaultBitmap;
        }

        private void HandleDebugDisplay(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (!debugControlActive)
            {
                emulatorRoot.ColumnDefinitions.Add(debugColumn);
                Grid.SetRow(debugControl, 0);
                Grid.SetColumn(debugControl, 1);
                emulatorRoot.Children.Add(debugControl);
                debugControlActive = true;
            }
            else
            {
                emulatorRoot.Children.Remove(debugControl);
                emulatorRoot.ColumnDefinitions.Remove(debugColumn);
                debugControlActive = false;
            }
        }

        private void HandlePauseResume(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (gameBoy.IsPoweredOn)
            {
                if (gameBoy.IsPaused)
                {
                    gameBoy.IsPaused = false;
                    pauseText.Source = pauseTextBitmap;
                }
                else
                {
                    gameBoy.IsPaused = true;
                    pauseText.Source = resumeTextBitmap;
                }
            }
        }

        private void HandlePower(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (gameBoy.IsPoweredOn)
            {
                gameBoy.SetPowerOn(false);
            }
            else
            {
                gameBoy.SetPowerOn(true);
                pauseText.Source = pauseTextBitmap; //Defaults to executing mode, so we need the button to reflect this.
                HookToGameBoy(gameBoy); //Have to rehook as the original LCD was lost.
            }
        }

        private void HandleCartridgeButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ShowFileDialog();
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
            if (gameBoy != null && gameBoy.IsPoweredOn)
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
}
