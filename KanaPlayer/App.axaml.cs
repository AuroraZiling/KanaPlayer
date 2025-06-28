using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using AsyncImageLoader;
using AsyncImageLoader.Loaders;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using KanaPlayer.Core.Helpers;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Models;
using KanaPlayer.Services;
using KanaPlayer.ViewModels;
using KanaPlayer.Views;
using Microsoft.Extensions.DependencyInjection;

namespace KanaPlayer;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        ImageLoader.AsyncImageLoader = new DiskCachedWebImageLoader(AppHelper.ApplicationCachesFolderPath);
    }

    private static readonly IServiceProvider? ServiceProvider =
        new ServiceCollection()
            .RegisterViews()
            .RegisterViewModels()
            .RegisterServices()
            .BuildServiceProvider();

    public static T GetService<T>() where T : notnull =>
        (ServiceProvider ?? throw new InvalidOperationException("Services not Configured")).GetRequiredService<T>();

    public static object GetService(Type type) =>
        (ServiceProvider ?? throw new InvalidOperationException("Services not Configured")).GetRequiredService(type);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            var splashWindow = new SplashWindow(() =>
                {
                    var mainWindow = GetService<MainWindow>();

                    mainWindow.Show();
                    mainWindow.Focus();

                    desktop.MainWindow = mainWindow;
                },
                () => GetService<IBilibiliClient>().AuthenticateAsync());
            desktop.MainWindow = splashWindow;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            var mainView = GetService<MainView>();
            mainView.DataContext = GetService<MainViewModel>();
            singleViewPlatform.MainView = mainView;
        }

        base.OnFrameworkInitializationCompleted();
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