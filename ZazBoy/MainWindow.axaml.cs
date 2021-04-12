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
        private Thread renderThread;
        private Action renderJob;
        private byte[,] existingColourMap;

        private Avalonia.Controls.Image lcdDisplay;
        private Size displaySize;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            renderThread = new Thread(ExecuteRenderLoop);
            renderThread.Start();
            lcdDisplay = this.FindControl<Avalonia.Controls.Image>("LCD");
            displaySize = new Size(lcdDisplay.Width, lcdDisplay.Height);
            existingColourMap = new byte[LCD.ScreenPixelWidth, LCD.ScreenPixelHeight];
            for (int x = 0; x<existingColourMap.GetLength(0); x++)
            {
                for (int y = 0; y < existingColourMap.GetLength(1); y++)
                {
                    existingColourMap[x, y] = 0xFF;
                }
            }

            RenderOptions.SetBitmapInterpolationMode(lcdDisplay, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.LowQuality);
            if (GameBoy.Instance().LCD != null)
                GameBoy.Instance().LCD.onLCDUpdate += delegate (LCD lcd, byte[,] colourMap) 
                {
                    renderJob = delegate () { RenderFrame(lcd, colourMap); };
                };

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

        private Bitmap RenderFrame(LCD lcd, byte[,] colourMap)
        {
            Bitmap lcdBitmap = new Bitmap((int) displaySize.Width, (int) displaySize.Height);
            int lcdWidth = lcdBitmap.Width;
            int lcdHeight = lcdBitmap.Height;
            float scaleFactor = (LCD.ScreenPixelWidth / (lcdWidth/1.0f));
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
                        if (existingColourMap[sourceX, sourceY] != colourId)
                        {
                            System.Drawing.Color colour = lcd.GetColourFromId(colourId);
                            int pos = (x * 3) + y * stride;
                            pixPtr[pos] = colour.B;
                            pixPtr[pos + 1] = colour.G;
                            pixPtr[pos + 2] = colour.R;
                        }
                        else if (scaleFactor < 1.0f)
                        {
                            y += (int)(1.0f / scaleFactor);
                        }
                    }
                }
            }
            lcdBitmap.UnlockBits(data);
            MemoryStream stream = new MemoryStream();
            lcdBitmap.Save(stream, ImageFormat.Png);
            Dispatcher.UIThread.Post(() =>
            {
                DisplayBitmap(stream);
            });
            return lcdBitmap;
        }

        private void DisplayBitmap(MemoryStream renderStream)
        {
            renderStream.Position = 0;
            Avalonia.Media.Imaging.Bitmap displayImage = new Avalonia.Media.Imaging.Bitmap(renderStream);
            lcdDisplay.Source = displayImage;
            renderStream.Close();
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            AvaloniaProperty property = change.Property;
            if (property.PropertyType == typeof(Avalonia.Size) && lcdDisplay != null)
            {
                Size windowSize = change.NewValue.GetValueOrDefault<Size>();

                if (windowSize.Height <= windowSize.Width * 0.9f)
                {
                    //Height is the decider
                    lcdDisplay.Height = windowSize.Height;
                    lcdDisplay.Width = windowSize.Height * 1.11111111111f;
                }
                else
                {
                    //Width is the decider
                    lcdDisplay.Width = windowSize.Width;
                    lcdDisplay.Height = windowSize.Width * 0.9f;
                }
                displaySize = new Size(lcdDisplay.Width, lcdDisplay.Height);
            }
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
    }
}
