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
using static ZazBoy.Console.LCD;
using Bitmap = System.Drawing.Bitmap;
using Size = Avalonia.Size;

namespace ZazBoy.UI.Controls
{
    public class EmulatorControl : UserControl
    {
        private GameBoy gameBoy;
        private Thread renderThread;
        private LCDUpdateHandler renderQueuer;
        private Action renderJob;

        private DockPanel emulatorView;
        private Grid emulatorRoot;
        private Avalonia.Controls.Image lcdDisplay;

        private DebugControl debugControl;
        private ColumnDefinition debugColumn;
        private bool debugControlActive;

        private Avalonia.Controls.Image pauseButton;
        private Avalonia.Controls.Image pauseText;
        private Avalonia.Media.Imaging.Bitmap resumeTextBitmap;
        private Avalonia.Media.Imaging.Bitmap pauseTextBitmap;
        private Avalonia.Media.Imaging.Bitmap buttonDefaultBitmap;
        private Avalonia.Media.Imaging.Bitmap buttonPressedBitmap;

        public EmulatorControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            renderQueuer = delegate (LCD lcd, byte[,] colourMap)    //Yeah, this while thing fucking sucks. But it's typical UI threading awkwardness.
            {
                //Currently on the emulator thread...
                Dispatcher.UIThread.Post(() =>
                {
                    //We need to be on the UI thread to get Avalonia.Controls.Image width / height
                    //Trying to cache the Image size in thread-safe variables results in:
                    //  A). Capturing all events and having the thing constantly bloody jitter between 0.1 differences
                    //  B). Missing some events, and having the display size be desynchronised.
                    int width = (int)Math.Max(LCD.ScreenPixelWidth, lcdDisplay.Bounds.Width);
                    int height = (int)(width * 0.9f);
                    //Finally, we need to prep something for the render thread, because rendering on the emulator or UI thread is too expensive and causes them to hang.
                    renderJob = delegate () { RenderFrame(width, height, colourMap); };
                });

            };
            renderThread = new Thread(ExecuteRenderLoop);
            renderThread.Start();
            lcdDisplay = this.FindControl<Avalonia.Controls.Image>("LCD");
            debugControl = new DebugControl();
            debugColumn = new ColumnDefinition();
            debugColumn.Width = new GridLength(1, GridUnitType.Star);
            debugControlActive = false;

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

            RenderOptions.SetBitmapInterpolationMode(lcdDisplay, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.LowQuality);
            HookToGameBoy(GameBoy.Instance());
        }

        private void HookToGameBoy(GameBoy gameBoy)
        {
            if (this.gameBoy != null && this.gameBoy.LCD != null)
                this.gameBoy.LCD.onLCDUpdate -= renderQueuer;

            this.gameBoy = gameBoy;
            gameBoy.LCD.onLCDUpdate += renderQueuer;
        }

        private Bitmap RenderFrame(int width, int height, byte[,] colourMap)
        {
            renderJob = null;
            Bitmap lcdBitmap = new Bitmap(width, height);
            int lcdWidth = lcdBitmap.Width;
            int lcdHeight = lcdBitmap.Height;
            float scaleFactor = (LCD.ScreenPixelWidth / (lcdWidth / 1.0f));
            BitmapData data = lcdBitmap.LockBits(new Rectangle(0, 0, lcdWidth, lcdHeight), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = data.Stride;
            unsafe
            {
                byte* pixPtr = (byte*)data.Scan0;
                for (int x = 0; x < lcdWidth; x++)
                {
                    for (int y = 0; y < lcdHeight; y++)
                    {
                        int sourceX = (int)(x * scaleFactor);
                        int sourceY = (int)(y * scaleFactor);
                        byte colourId = colourMap[sourceX, sourceY];
                        System.Drawing.Color colour = LCD.GetColourFromId(colourId);
                        int pos = (x * 3) + y * stride;
                        pixPtr[pos] = colour.B;
                        pixPtr[pos + 1] = colour.G;
                        pixPtr[pos + 2] = colour.R;
                    }
                }
            }
            lcdBitmap.UnlockBits(data);
            Avalonia.Media.Imaging.Bitmap uiFrame = UIUtil.ConvertDrawingBitmapToUIBitmap(lcdBitmap);
            Dispatcher.UIThread.Post(() =>
            {
                lcdDisplay.Source = uiFrame;
            });
            return lcdBitmap;
        }

        private void ExecuteRenderLoop()
        {
            while (true)
            {
                if (renderJob != null)
                    renderJob.Invoke();
                Thread.Sleep(2);
            }
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
    }
}
