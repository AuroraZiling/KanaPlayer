using System.Net;
using System.Net.Http;
using Avalonia.Platform.Storage;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Player;
using KanaPlayer.Models;
using KanaPlayer.Services.Theme;
using KanaPlayer.ViewModels;
using KanaPlayer.ViewModels.Pages;
using KanaPlayer.Views;
using KanaPlayer.Views.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace KanaPlayer.Services;

public static class ServiceExtensions
{
    public static IServiceCollection RegisterViews(this IServiceCollection services)
    {
        services.AddSingleton<SplashWindow>();
        services.AddTransient<IStorageProvider>(x => x.GetRequiredService<SplashWindow>().StorageProvider);
        services.AddTransient<ILauncher>(x => x.GetRequiredService<SplashWindow>().Launcher);

        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainView>();

        // Navigation - Top
        services.AddPage<HomeView>();
        services.AddPage<PlayListView>();

        // Navigation - Account Features
        services.AddPage<FavoritesView>();

        // Navigation - Tools
        services.AddPage<DownloadQueueView>();

        // Navigation - TitleBar
        services.AddPage<AccountView>();
        services.AddPage<SettingsView>();

        return services;
    }

    public static IServiceCollection RegisterViewModels(this IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainViewModel>();

        // Navigation - Top
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<PlayListViewModel>();

        // Navigation - Account Features
        services.AddSingleton<FavoritesViewModel>();

        // Navigation - Tools
        services.AddSingleton<DownloadQueueViewModel>();

        // Navigation - TitleBar
        services.AddSingleton<AccountViewModel>();
        services.AddSingleton<SettingsViewModel>();

        return services;
    }

    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IConfigurationService<SettingsModel>, ConfigurationService<SettingsModel>>();
        services.AddSingleton<IPlayerManager, PlayerManager<SettingsModel>>();
        services.AddSingleton<IBilibiliClient, BilibiliClient<SettingsModel>>();
        services.AddKeyedSingleton<HttpClient, HttpClient>("bilibili", (_, _) =>
            new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            })
            {
                DefaultRequestHeaders =
                {
                    {
                        "User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3"
                    },
                    {
                        "Referer",
                        "https://www.bilibili.com"
                    }
                }
            });

        services.AddSingleton<IThemeService, ThemeService>();

        return services;
    }
}