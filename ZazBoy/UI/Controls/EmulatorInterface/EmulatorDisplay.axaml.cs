using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using ZazBoy.Console;
using static ZazBoy.Console.LCD;
using Color = Avalonia.Media.Color;
using Image = Avalonia.Controls.Image;

namespace ZazBoy.UI.Controls.EmulatorInterface
{
    public class EmulatorDisplay : Image
    {
        private Avalonia.Controls.Image display;

        private static bool rendererInitiated;
        private static GameBoy gameBoy;
        private static Thread renderThread;
        private static LCDUpdateHandler renderQueuer;
        private static Action renderJob;
        private static List<Avalonia.Controls.Image> activeDisplays = new List<Avalonia.Controls.Image>();
        private delegate void FrameRenderedHandler(Avalonia.Media.Imaging.Bitmap frame);
        private static event FrameRenderedHandler onFrameRendered;

        public EmulatorDisplay()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            display = this.FindControl<Image>("Display");
            activeDisplays.Add(display);
            InitialiseDisplay();
            onFrameRendered += (Avalonia.Media.Imaging.Bitmap frame) => { Dispatcher.UIThread.Post(() => display.Source = frame); };
            if (!rendererInitiated)
                CreateRenderer();
        }

        private void InitialiseDisplay()
        {
            RenderOptions.SetBitmapInterpolationMode(display, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.LowQuality);
            Bitmap defaultOutput = new Bitmap(LCD.ScreenPixelWidth, LCD.ScreenPixelHeight);
            Graphics gfx = Graphics.FromImage(defaultOutput);
            System.Drawing.Pen pen = new System.Drawing.Pen(LCD.lcdOff);    //Not the most efficient set-up in the world, admittedly.
            gfx.FillRectangle(pen.Brush, 0, 0, defaultOutput.Width, defaultOutput.Height);
            gfx.Dispose();
            display.Source = UIUtil.ConvertDrawingBitmapToUIBitmap(defaultOutput);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            activeDisplays.Add(display);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            activeDisplays.Remove(display);
        }


        /*
         * Below are the static methods relating to the renderer. These are executed when the first instance of the display is launched, and all following displays hook onto the renderer.
         * Basically, no need to touch this stuff with every instance that gets created.
         */

        private static void CreateRenderer()
        {
            renderQueuer = delegate (LCD lcd, byte[,] colourMap)    //Yeah, this while thing fucking sucks. But it's typical UI threading awkwardness.
            {
                //Currently on the emulator thread...
                Dispatcher.UIThread.Post(() =>
                {
                    //We need to be on the UI thread to get Avalonia.Controls.Image width / height
                    //Trying to cache the Image size in thread-safe variables results in:
                    //  A). Capturing all events and having the thing constantly bloody jitter between 0.1 differences
                    //  B). Missing some events, and having the display size be desynchronised.
                    Avalonia.Controls.Image largestDisplay = GetLargestDisplay();
                    int width = (int)Math.Max(LCD.ScreenPixelWidth, largestDisplay.Bounds.Width);
                    int height = (int)(width * 0.9f);
                    //Finally, we need to prep something for the render thread, because rendering on the emulator or UI thread is too expensive and causes them to hang.
                    renderJob = delegate () { RenderFrame(width, height, colourMap); };
                });

            };
            renderThread = new Thread(ExecuteRenderLoop);
            renderThread.Start();
            gameBoy = GameBoy.Instance();
            gameBoy.onEmulatorPowerStateChanged += HandleGameBoyPowerCycle;
            rendererInitiated = true;
        }

        private static Avalonia.Controls.Image GetLargestDisplay()
        {
            Avalonia.Controls.Image largestDisplay = null;
            foreach (Avalonia.Controls.Image display in activeDisplays)
            {
                if (largestDisplay == null || display.Bounds.Width > largestDisplay.Bounds.Width)
                    largestDisplay = display;
            }
            return largestDisplay;
        }

        private static void HandleGameBoyPowerCycle(bool powered)
        {
            if (powered)
                HookToGameBoy(gameBoy);
        }

        private static void HookToGameBoy(GameBoy newGameBoy)
        {
            if (gameBoy != null && gameBoy.LCD != null)
            {
                gameBoy.LCD.onLCDUpdate -= renderQueuer;
            }

            gameBoy = newGameBoy;
            gameBoy.LCD.onLCDUpdate += renderQueuer;
        }

        private static Bitmap RenderFrame(int width, int height, byte[,] colourMap)
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
            onFrameRendered?.Invoke(uiFrame);
            return lcdBitmap;
        }

        private static void ExecuteRenderLoop()
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
