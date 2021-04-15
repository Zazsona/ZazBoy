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

namespace ZazBoy
{
    public class EmulatorControl : UserControl
    {
        private GameBoy gameBoy;
        private Thread renderThread;
        private LCDUpdateHandler renderQueuer;
        private Action renderJob;

        private Avalonia.Controls.Image lcdDisplay;
        private Size displaySize;

        private Avalonia.Controls.Image pauseButton;
        private Avalonia.Media.Imaging.Bitmap resumeBitmap;
        private Avalonia.Media.Imaging.Bitmap pauseBitmap;
        private Avalonia.Media.Imaging.Bitmap powerBitmap;
        private Avalonia.Media.Imaging.Bitmap debugBitmap;

        public EmulatorControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            renderQueuer = delegate (LCD lcd, byte[,] colourMap) { renderJob = delegate () { RenderFrame(colourMap); }; };
            renderThread = new Thread(ExecuteRenderLoop);
            renderThread.Start();
            lcdDisplay = this.FindControl<Avalonia.Controls.Image>("LCD");
            displaySize = new Size(lcdDisplay.MinWidth, lcdDisplay.MinHeight);

            pauseButton = this.FindControl<Avalonia.Controls.Image>("PauseButton");
            Avalonia.Controls.Image powerButton = this.FindControl<Avalonia.Controls.Image>("PowerButton");
            Avalonia.Controls.Image debugButton = this.FindControl<Avalonia.Controls.Image>("DebugButton");
            Bitmap resumeResource = Properties.Resources.ResumeBtnImg;
            resumeBitmap = ConvertDrawingBitmapToUIBitmap(resumeResource);
            Bitmap pauseResource = Properties.Resources.PauseBtnImg;
            pauseBitmap = ConvertDrawingBitmapToUIBitmap(pauseResource);
            Bitmap powerResource = Properties.Resources.PowerBtnImg;
            powerBitmap = ConvertDrawingBitmapToUIBitmap(powerResource);
            Bitmap debugResource = Properties.Resources.DebugBtnImg;
            debugBitmap = ConvertDrawingBitmapToUIBitmap(debugResource);
            pauseButton.Source = pauseBitmap;
            powerButton.Source = powerBitmap;
            debugButton.Source = debugBitmap;
            pauseButton.PointerPressed += HandlePauseResume;
            powerButton.PointerPressed += HandlePower;

            RenderOptions.SetBitmapInterpolationMode(lcdDisplay, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.LowQuality);
            HookToGameBoy(GameBoy.Instance());

            //TODO: Separate button background and text (Set text property IsHitTestVisible to false to stop them consuming the click)
        }

        private void HookToGameBoy(GameBoy gameBoy)
        {
            if (this.gameBoy != null && this.gameBoy.LCD != null)
                this.gameBoy.LCD.onLCDUpdate -= renderQueuer;

            this.gameBoy = gameBoy;
            gameBoy.LCD.onLCDUpdate += renderQueuer;
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

        protected override void ArrangeCore(Rect finalRect)
        {
            base.ArrangeCore(finalRect);
            double width = Math.Max(LCD.ScreenPixelWidth, lcdDisplay.Bounds.Width);
            double height = Math.Max(LCD.ScreenPixelHeight, lcdDisplay.Bounds.Height);
            displaySize = new Size(width, height);
        }

        private Bitmap RenderFrame(byte[,] colourMap)
        {
            renderJob = null;
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
                        System.Drawing.Color colour = LCD.GetColourFromId(colourId);
                        int pos = (x * 3) + y * stride;
                        pixPtr[pos] = colour.B;
                        pixPtr[pos + 1] = colour.G;
                        pixPtr[pos + 2] = colour.R;
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


        private void HandlePauseResume(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (gameBoy.IsPoweredOn)
            {
                if (gameBoy.IsPaused)
                {
                    gameBoy.IsPaused = false;
                    pauseButton.Source = pauseBitmap;
                }
                else
                {
                    gameBoy.IsPaused = true;
                    pauseButton.Source = resumeBitmap;
                }
            }
        }

        private void HandlePower(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (gameBoy.IsPoweredOn)
            {
                gameBoy.SetPowerOn(false);
                //pauseResButton.Source = pauseBitmap;
            }
            else
            {
                gameBoy.SetPowerOn(true);
                pauseButton.Source = pauseBitmap; //Defaults to executing mode, so we need the button to reflect this.
                HookToGameBoy(gameBoy); //Have to rehook as the original LCD was lost.
                //pauseResButton.Source = resumeBitmap;
            }
        }
    }
}
