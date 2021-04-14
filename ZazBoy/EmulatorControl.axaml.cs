using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using ZazBoy.Console;
using Size = Avalonia.Size;

namespace ZazBoy
{
    public class EmulatorControl : UserControl
    {
        private Thread renderThread;
        private Action renderJob;
        private byte[,] existingColourMap;

        private Avalonia.Controls.Image lcdDisplay;
        private Size displaySize;

        public EmulatorControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            renderThread = new Thread(ExecuteRenderLoop);
            renderThread.Start();
            lcdDisplay = this.FindControl<Avalonia.Controls.Image>("LCD");
            displaySize = new Size(lcdDisplay.MinWidth, lcdDisplay.MinHeight);
            existingColourMap = new byte[LCD.ScreenPixelWidth, LCD.ScreenPixelHeight];
            for (int x = 0; x < existingColourMap.GetLength(0); x++)
            {
                for (int y = 0; y < existingColourMap.GetLength(1); y++)
                {
                    existingColourMap[x, y] = 0xFF;
                }
            }

            lcdDisplay.PropertyChanged += HandleLCDResize;

            RenderOptions.SetBitmapInterpolationMode(lcdDisplay, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.LowQuality);
            if (GameBoy.Instance().LCD != null)
                GameBoy.Instance().LCD.onLCDUpdate += delegate (LCD lcd, byte[,] colourMap)
                {
                    renderJob = delegate () { RenderFrame(lcd, colourMap); };
                };
        }

        private void HandleLCDResize(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.PropertyType == typeof(TransformedBounds) || e.Property.PropertyType == typeof(Avalonia.Rect))
            {
                if (lcdDisplay.Bounds.Width != 0 && lcdDisplay.Bounds.Height != 0)
                    displaySize = new Size(lcdDisplay.Bounds.Width, lcdDisplay.Bounds.Width*0.9f); //Doesn't perfectly map to image, but matches GB aspect ratio.
            }
        }

        private Bitmap RenderFrame(LCD lcd, byte[,] colourMap)
        {
            Bitmap lcdBitmap = new Bitmap((int)displaySize.Width, (int)displaySize.Height);
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
                        if (existingColourMap[sourceX, sourceY] != colourId)
                        {
                            System.Drawing.Color colour = lcd.GetColourFromId(colourId);
                            int pos = (x * 3) + y * stride;
                            pixPtr[pos] = colour.B;
                            pixPtr[pos + 1] = colour.G;
                            pixPtr[pos + 2] = colour.R;
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
