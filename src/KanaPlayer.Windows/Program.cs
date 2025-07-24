using Avalonia;
using KanaPlayer.Core.Interfaces;
using KanaPlayer.Windows.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KanaPlayer.Windows;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.ConfigureServices += x =>
        {
            x.AddSingleton<IAudioPlayer, MediaFoundationAudioPlayer>();
        };
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}