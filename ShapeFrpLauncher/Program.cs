using Avalonia;
using AvaloniaApplication1.Services;
using System;

namespace AvaloniaApplication1
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ReleaseSourceConfig.Load();
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
