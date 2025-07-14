using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace KanaPlayer.Controls.Navigation;

public static class NavigationServiceExtensions
{
    public static IServiceCollection AddMainPage<TView>(this IServiceCollection services) where TView : MainNavigablePageBase
    {
        services.AddSingleton<TView>();
        services.AddSingleton<MainNavigablePageBase>(x => x.GetRequiredService<TView>());
        return services;
    }
    
    public static IServiceCollection AddPage<TView>(this IServiceCollection services) where TView : NavigablePageBase
    {
        services.AddTransient<TView>();
        services.AddTransient<NavigablePageBase>(x => x.GetRequiredService<TView>());
        return services;
    }
}

public partial class NavigationService : ObservableObject, INavigationService
{
    private readonly Stack<NavigablePageBase> _backStack = new();
    private readonly Stack<NavigablePageBase> _forwardStack = new();

    internal IServiceProvider? PageProvider { get; private set; }

    public void Initialize(KanaNavigationView navigationView, IServiceProvider pageProvider)
    {
        PageProvider = pageProvider;
        navigationView.NavigationService = this;
    }

    public void Navigate(Type viewType)
    {
        if (PageProvider is null)
        {
            throw new InvalidOperationException("Navigation service is not initialized");
        }

        var page = PageProvider.GetRequiredService(viewType);
        if (page is NavigablePageBase navigablePage)
        {
            if (!_isHandlingBackAndForward)
            {
                if (CurrentPage != null)
                {
                    _backStack.Push(CurrentPage);
                }
                _forwardStack.Clear();
            }
            CurrentPage = navigablePage;
            if (CurrentPage.DataContext is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo();
            }
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoForward));
        }
        else
        {
            throw new InvalidOperationException("Invalid page type");
        }
    }
    
    public void Navigate(NavigablePageBase view)
    {
        if (PageProvider is null)
        {
            throw new InvalidOperationException("Navigation service is not initialized");
        }

        if (!_isHandlingBackAndForward)
        {
            if (CurrentPage != null)
            {
                _backStack.Push(CurrentPage);
            }
            _forwardStack.Clear();
        }
        CurrentPage = view;
        if (CurrentPage.DataContext is INavigationAware navigationAware)
        {
            navigationAware.OnNavigatedTo();
        }
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoForward));
    }

    [ObservableProperty] public partial NavigablePageBase? CurrentPage { get; private set; }
    private bool _isHandlingBackAndForward;

    public bool CanGoBack => _backStack.Count > 0;

    public bool CanGoForward => _forwardStack.Count > 0;

    public void GoBack()
    {
        if (!CanGoBack)
        {
            throw new InvalidOperationException("Cannot go back, no pages in the back stack");
        }

        var previousPage = _backStack.Pop();
        if (CurrentPage != null)
        {
            _forwardStack.Push(CurrentPage);
        }
        CurrentPage = previousPage;
        _isHandlingBackAndForward = true;
        _isHandlingBackAndForward = false;
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoForward));
    }

    public void GoForward()
    {
        if (!CanGoForward)
        {
            throw new InvalidOperationException("Cannot go forward, no pages in the forward stack");
        }

        var nextPage = _forwardStack.Pop();
        if (CurrentPage != null)
        {
            _backStack.Push(CurrentPage);
        }
        CurrentPage = nextPage;
        _isHandlingBackAndForward = true;
        _isHandlingBackAndForward = false;
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoForward));
    }
}