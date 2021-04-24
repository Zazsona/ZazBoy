using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using ZazBoy.Console;
using static ZazBoy.Console.LCD;
using Image = Avalonia.Controls.Image;

namespace ZazBoy.UI.Controls.EmulatorInterface
{
    public class EmulatorDisplay : Image
    {
        private GameBoy gameBoy;
        private Avalonia.Controls.Image display;
        private Thread renderThread;
        private LCDUpdateHandler renderQueuer;
        private Action renderJob;


        public EmulatorDisplay()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            display = this.FindControl<Image>("Display");
            InitialiseDisplay();
            renderQueuer = delegate (LCD lcd, byte[,] colourMap)    //Yeah, this while thing fucking sucks. But it's typical UI threading awkwardness.
            {
                //Currently on the emulator thread...
                Dispatcher.UIThread.Post(() =>
                {
                    //We need to be on the UI thread to get Avalonia.Controls.Image width / height
                    //Trying to cache the Image size in thread-safe variables results in:
                    //  A). Capturing all events and having the thing constantly bloody jitter between 0.1 differences
                    //  B). Missing some events, and having the display size be desynchronised.
                    int width = (int)Math.Max(LCD.ScreenPixelWidth, display.Bounds.Width);
                    int height = (int)(width * 0.9f);
                    //Finally, we need to prep something for the render thread, because rendering on the emulator or UI thread is too expensive and causes them to hang.
                    renderJob = delegate () { RenderFrame(width, height, colourMap); };
                });

            };
            renderThread = new Thread(ExecuteRenderLoop);
            renderThread.Start();
        }

        public void HookToGameBoy(GameBoy gameBoy)
        {
            if (this.gameBoy != null && this.gameBoy.LCD != null)
            {
                this.gameBoy.LCD.onLCDUpdate -= renderQueuer;
            }

            this.gameBoy = gameBoy;
            gameBoy.LCD.onLCDUpdate += renderQueuer;
        }

        private void InitialiseDisplay()
        {
            RenderOptions.SetBitmapInterpolationMode(display, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.LowQuality);
            byte[,] initialColourMap = new byte[LCD.ScreenPixelWidth, LCD.ScreenPixelHeight];
            for (int x = 0; x < initialColourMap.GetLength(0); x++)
            {
                for (int y = 0; y < initialColourMap.GetLength(1); y++)
                {
                    initialColourMap[x, y] = byte.MaxValue;
                }
            }
            RenderFrame(LCD.ScreenPixelWidth, LCD.ScreenPixelHeight, initialColourMap);
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
                display.Source = uiFrame;
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
