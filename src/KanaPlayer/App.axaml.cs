using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading.Tasks;
using AsyncImageLoader;
using AsyncImageLoader.Loaders;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Helpers;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Database;
using KanaPlayer.Models;
using KanaPlayer.Services;
using KanaPlayer.Services.Theme;
using KanaPlayer.ViewModels;
using KanaPlayer.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;

namespace KanaPlayer;

// ReSharper disable once PartialTypeWithSinglePart
public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        this.AttachDeveloperTools();

        ImageLoader.AsyncImageLoader = new DiskCachedWebImageLoader(AppHelper.ApplicationImageCachesFolderPath);
    }

    public static event Action<IServiceCollection>? ConfigureServices;

    private static readonly Lazy<IServiceProvider> ServiceProvider = new(() =>
        new ServiceCollection()
            .RegisterViews()
            .RegisterViewModels()
            .RegisterServices()
            .With(x => ConfigureServices?.Invoke(x))
            .BuildServiceProvider());

    public static T GetService<T>() where T : notnull => ServiceProvider.Value.GetRequiredService<T>();

    public static object GetService(Type type) => ServiceProvider.Value.GetRequiredService(type);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            GetService<IThemeService>().SetThemeColor(new Color(255, 216, 196, 241), true);

            var splashWindow = GetService<SplashWindow>();
            desktop.MainWindow = splashWindow;
            splashWindow.RunAsync(async () =>
            {
                await Task.Run(() =>
                {
                    GetService<MainDbContext>().Database.EnsureCreated();
                });

                // First Authenticate Attempt
                await GetService<IBilibiliClient>().AuthenticateAsync();

                // Cache Cleanup
                await Task.Run(() =>
                {
                    var configuration = GetService<IConfigurationService<SettingsModel>>().Settings.CommonSettings;
                    CleanupCache(AppHelper.ApplicationAudioCachesFolderPath,
                        configuration.AudioCache.MaximumCacheSizeInMb);
                    CleanupCache(AppHelper.ApplicationImageCachesFolderPath,
                        configuration.ImageCache.MaximumCacheSizeInMb);
                });
            }).ContinueWith(_ =>
            {
                var mainWindow = GetService<MainWindow>();

                mainWindow.Show();
                mainWindow.Focus();

                desktop.MainWindow = mainWindow;
            }, TaskContinuationOptions.ExecuteSynchronously).Detach(); // TODO: Exception Handler
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            var mainView = GetService<MainView>();
            mainView.DataContext = GetService<MainViewModel>();
            singleViewPlatform.MainView = mainView;
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static void SafeShutdown()
    {
        // TODO: Implement a proper shutdown sequence
        Environment.Exit(0);
    }

    internal static void CleanupCache(string cacheFolderPath, int desiredCacheSizeInMb)
    {
        var dirInfo = new DirectoryInfo(cacheFolderPath);
        if (!dirInfo.Exists) return;

        var files = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                           .OrderByDescending(f => f.LastAccessTimeUtc)
                           .ToList();
        var currentCacheSizeInBytes = files.Sum(f => f.Length);
        var desiredCacheSizeInBytes = desiredCacheSizeInMb * 1024L * 1024L;

        while (currentCacheSizeInBytes > desiredCacheSizeInBytes && files.Count > 0)
        {
            var fileToDelete = files.Last();
            try
            {
                currentCacheSizeInBytes -= fileToDelete.Length;
                fileToDelete.Delete();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete cache file {fileToDelete.FullName}: {ex.Message}");
            }
            files.Remove(fileToDelete);
        }
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}