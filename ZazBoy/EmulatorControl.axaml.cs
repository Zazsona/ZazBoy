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
using Bitmap = System.Drawing.Bitmap;
using Size = Avalonia.Size;

namespace ZazBoy
{
    public class EmulatorControl : UserControl
    {
        private Thread renderThread;
        private Action renderJob;
        private byte[,] existingColourMap;

        private Avalonia.Controls.Image lcdDisplay;
        private Avalonia.Controls.StackPanel buttonStack;
        private Size displaySize;

        private Avalonia.Media.Imaging.Bitmap resumeBitmap;
        private Avalonia.Media.Imaging.Bitmap pauseBitmap;
        private Avalonia.Media.Imaging.Bitmap stopBitmap;
        private Avalonia.Media.Imaging.Bitmap debugBitmap;

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

            buttonStack = this.FindControl<Avalonia.Controls.StackPanel>("ButtonStack");
            Avalonia.Controls.Image resumeButton = this.FindControl<Avalonia.Controls.Image>("ResumeButton");
            Avalonia.Controls.Image stopButton = this.FindControl<Avalonia.Controls.Image>("StopButton");
            Avalonia.Controls.Image debugButton = this.FindControl<Avalonia.Controls.Image>("DebugButton");
            Bitmap resumeResource = Properties.Resources.ResumeBtnImg;
            resumeBitmap = ConvertDrawingBitmapToUIBitmap(resumeResource);
            Bitmap pauseResource = Properties.Resources.PauseBtnImg;
            pauseBitmap = ConvertDrawingBitmapToUIBitmap(pauseResource);
            Bitmap stopResource = Properties.Resources.StopBtnImg;
            stopBitmap = ConvertDrawingBitmapToUIBitmap(stopResource);
            Bitmap debugResource = Properties.Resources.DebugBtnImg;
            debugBitmap = ConvertDrawingBitmapToUIBitmap(debugResource);
            resumeButton.Source = pauseBitmap;
            stopButton.Source = stopBitmap;
            debugButton.Source = debugBitmap;

            existingColourMap = new byte[LCD.ScreenPixelWidth, LCD.ScreenPixelHeight];
            for (int x = 0; x < existingColourMap.GetLength(0); x++)
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
        }

        private Avalonia.Media.Imaging.Bitmap ConvertDrawingBitmapToUIBitmap(Bitmap drawingBitmap)
        {
            MemoryStream memoryStream = new MemoryStream();
            drawingBitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;
            Avalonia.Media.Imaging.Bitmap uiBitmap = new Avalonia.Media.Imaging.Bitmap(memoryStream);
            memoryStream.Close();
            return uiBitmap;
        }

        protected override Size MeasureCore(Size availableSize)
        {
            if (lcdDisplay.Bounds.Width != 0 && lcdDisplay.Bounds.Height != 0)
                buttonStack.Height = lcdDisplay.Bounds.Height / 20; //Do not put this in arrange core, as it changes the layout.
            return base.MeasureCore(availableSize);
        }

        protected override void ArrangeCore(Rect finalRect)
        {
            base.ArrangeCore(finalRect);
            if (lcdDisplay.Bounds.Width != 0 && lcdDisplay.Bounds.Height != 0)
                displaySize = new Size(lcdDisplay.Bounds.Width, lcdDisplay.Bounds.Height);
        }

        private Bitmap RenderFrame(LCD lcd, byte[,] colourMap)
        {
            Bitmap lcdBitmap = new Bitmap((int)displaySize.Width, (int)(displaySize.Width*0.9f));
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
            Avalonia.Media.Imaging.Bitmap uiFrame = ConvertDrawingBitmapToUIBitmap(lcdBitmap);
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
    }
}
