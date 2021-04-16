using Avalonia;
using System;
using System.Threading;
using ZazBoy.Console;
using ZazBoy.UI;

namespace ZazBoy
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread emulatorThread = new Thread(StartEmulator);
            emulatorThread.Start();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        private static void StartEmulator()
        {
            GameBoy gameBoy = GameBoy.Instance();
            gameBoy.SetPowerOn(true);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
