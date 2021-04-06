using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using ZazBoy.Console;
using Size = Avalonia.Size;

namespace ZazBoy
{
    public class MainWindow : Window
    {
        private Avalonia.Controls.Image lcd;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            lcd = this.FindControl<Avalonia.Controls.Image>("LCD");
            RenderOptions.SetBitmapInterpolationMode(lcd, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.LowQuality);
            if (GameBoy.Instance().LCD != null)
            {
                GameBoy.Instance().LCD.onLCDUpdate += delegate (Bitmap bitmap) { Dispatcher.UIThread.Post(() => DisplayBitmap(lcd, bitmap)); };
            }
        }

        private void DisplayBitmap(Avalonia.Controls.Image image, Bitmap bitmap)
        {
            Bitmap scaledBitmap = new Bitmap((int) image.Width, (int) image.Height);
            scaledBitmap.SetResolution(bitmap.Width, bitmap.Height);
            Rectangle scaledRect = new Rectangle(0, 0, (int)image.Width, (int)image.Height);
            Graphics gfx = Graphics.FromImage(scaledBitmap);
            gfx.CompositingMode = CompositingMode.SourceCopy;
            gfx.CompositingQuality = CompositingQuality.HighQuality;
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.SmoothingMode = SmoothingMode.HighQuality;
            gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var wrapMode = new ImageAttributes())
            {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                gfx.DrawImage(bitmap, scaledRect, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, wrapMode);
            }
            gfx.Dispose();

            MemoryStream stream = new MemoryStream();
            scaledBitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0;

            Avalonia.Media.Imaging.Bitmap displayImage = new Avalonia.Media.Imaging.Bitmap(stream);
            displayImage = displayImage.CreateScaledBitmap(new PixelSize(800, 720), Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.LowQuality);
            image.Source = displayImage;
            stream.Close();
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            AvaloniaProperty property = change.Property;
            if (property.PropertyType == typeof(Avalonia.Size) && lcd != null)
            {
                Size windowSize = change.NewValue.GetValueOrDefault<Size>();

                if (windowSize.Height <= windowSize.Width * 0.9f)
                {
                    //Height is the decider
                    lcd.Height = windowSize.Height;
                    lcd.Width = windowSize.Height * 1.11111111111f;
                }
                else
                {
                    //Width is the decider
                    lcd.Width = windowSize.Width;
                    lcd.Height = windowSize.Width * 0.9f;
                }
            }
        }
    }
}
