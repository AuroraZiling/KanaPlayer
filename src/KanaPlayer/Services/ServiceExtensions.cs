using System.Net;
using System.Net.Http;
using Avalonia.Platform.Storage;
using KanaPlayer.Controls.Hosts;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Favorites;
using KanaPlayer.Core.Services.Player;
using KanaPlayer.Database;
using KanaPlayer.Models;
using KanaPlayer.Services.Theme;
using KanaPlayer.Services.TrayMenu;
using KanaPlayer.ViewModels;
using KanaPlayer.ViewModels.Pages;
using KanaPlayer.ViewModels.Pages.SubPages;
using KanaPlayer.Views;
using KanaPlayer.Views.Pages;
using KanaPlayer.Views.Pages.SubPages;
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
        services.AddMainPage<HomeView>();
        services.AddMainPage<PlayListView>();

        // Navigation - Account Features
        services.AddMainPage<FavoritesView>();

        // Navigation - Tools
        services.AddMainPage<DownloadQueueView>();

        // Navigation - TitleBar
        services.AddMainPage<AccountView>();
        services.AddMainPage<SettingsView>();
        
        // SubPages
        services.AddPage<FavoritesBilibiliImportView>();

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
        
        // SubPages
        services.AddTransient<FavoritesBilibiliImportViewModel>();

        return services;
    }

    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<IKanaToastManager, KanaToastManager>();
        services.AddSingleton<IKanaDialogManager, KanaDialogManager>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ITrayMenuService, TrayMenuService>();
        services.AddSingleton<IConfigurationService<SettingsModel>, ConfigurationService<SettingsModel>>();
        services.AddSingleton<IPlayerManager, PlayerManager<SettingsModel>>();
        services.AddDbContext<MainDbContext>(ServiceLifetime.Singleton);
        
        services.AddSingleton<IFavoritesManager>(x => x.GetRequiredService<MainDbContext>());
        
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