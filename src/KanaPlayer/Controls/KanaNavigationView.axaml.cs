using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using KanaPlayer.Controls.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace KanaPlayer.Controls;

public class KanaNavigationView : TemplatedControl
{
    public required NavigationService NavigationService
    {
        get;
        set
        {
            var pageProvider = value.PageProvider;
            if (pageProvider is null) throw new InvalidOperationException("Navigation service is not initialized");
            
            var topNavigationSideBarItems = new List<NavigationSideBarItem>();
            var accountFeaturesNavigationSideBarItems = new List<NavigationSideBarItem>();
            var toolsNavigationSideBarItems = new List<NavigationSideBarItem>();
            foreach (var page in pageProvider.GetServices<MainNavigablePageBase>())
            {
                if (!page.OnSideBar) continue;
                switch (page.Category)
                {
                    case NavigationPageCategory.Top:
                        topNavigationSideBarItems.Add(new NavigationSideBarItem
                        {
                            ViewType = page.GetType(),
                            ViewName = page.PageTitle,
                            IconKind = page.IconKind
                        });
                        break;
                    case NavigationPageCategory.AccountFeatures:
                        accountFeaturesNavigationSideBarItems.Add(new NavigationSideBarItem
                        {
                            ViewType = page.GetType(),
                            ViewName = page.PageTitle,
                            IconKind = page.IconKind
                        });
                        break;
                    case NavigationPageCategory.Tools:
                        toolsNavigationSideBarItems.Add(new NavigationSideBarItem
                        {
                            ViewType = page.GetType(),
                            ViewName = page.PageTitle,
                            IconKind = page.IconKind
                        });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(page.Category), page.Category, 
                            "Unknown navigation page category");
                }
            }
            TopNavigationSideBarItems = new AvaloniaList<NavigationSideBarItem>(topNavigationSideBarItems);
            AccountFeaturesNavigationSideBarItems = new AvaloniaList<NavigationSideBarItem>(accountFeaturesNavigationSideBarItems);
            ToolsNavigationSideBarItems = new AvaloniaList<NavigationSideBarItem>(toolsNavigationSideBarItems);
            field = value;
            field.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(NavigationService.CurrentPage))
                {
                    if (field.CurrentPage is not null)
                    {
                        SelectedNavigationSideBarItem = TopNavigationSideBarItems
                            .FirstOrDefault(x => x.ViewType == field.CurrentPage.GetType()) ??
                            ToolsNavigationSideBarItems.FirstOrDefault(x => x.ViewType == field.CurrentPage.GetType());
                    }
                }
            };

            SelectedNavigationSideBarItem =
                TopNavigationSideBarItems.FirstOrDefault() ??
                ToolsNavigationSideBarItems.FirstOrDefault();
        }
    }
    
    private bool _isNavigating;

    public static readonly DirectProperty<KanaNavigationView, NavigationSideBarItem?> SelectedNavigationSideBarItemProperty = 
        AvaloniaProperty.RegisterDirect<KanaNavigationView, NavigationSideBarItem?>(
        nameof(SelectedNavigationSideBarItem), 
        o => o.SelectedNavigationSideBarItem, 
        (o, v) => o.SelectedNavigationSideBarItem = v);

    public NavigationSideBarItem? SelectedNavigationSideBarItem
    {
        get;
        set
        {
            if (_isNavigating) return;
            _isNavigating = true;
            try
            {
                if (!SetAndRaise(SelectedNavigationSideBarItemProperty, ref field, value)) return;
                if (field is not null) NavigationService.Navigate(field.ViewType);
            }
            finally
            {
                _isNavigating = false;
            }
        }
    }

    internal static readonly StyledProperty<bool> IsSideBarEnabledProperty = AvaloniaProperty.Register<KanaNavigationView, bool>(
        nameof(IsSideBarEnabled), true);

    internal bool IsSideBarEnabled
    {
        get => GetValue(IsSideBarEnabledProperty);
        set => SetValue(IsSideBarEnabledProperty, value);
    }


    internal static readonly StyledProperty<AvaloniaList<NavigationSideBarItem>> TopNavigationSideBarItemsProperty = 
        AvaloniaProperty.Register<KanaNavigationView, AvaloniaList<NavigationSideBarItem>>(nameof(TopNavigationSideBarItems));

    internal AvaloniaList<NavigationSideBarItem> TopNavigationSideBarItems
    {
        get => GetValue(TopNavigationSideBarItemsProperty);
        set => SetValue(TopNavigationSideBarItemsProperty, value);
    }
    
    
    internal static readonly StyledProperty<AvaloniaList<NavigationSideBarItem>> AccountFeaturesNavigationSideBarItemsProperty = 
        AvaloniaProperty.Register<KanaNavigationView, AvaloniaList<NavigationSideBarItem>>(nameof(AccountFeaturesNavigationSideBarItems));

    internal AvaloniaList<NavigationSideBarItem> AccountFeaturesNavigationSideBarItems
    {
        get => GetValue(AccountFeaturesNavigationSideBarItemsProperty);
        set => SetValue(AccountFeaturesNavigationSideBarItemsProperty, value);
    }
    
    
    internal static readonly StyledProperty<AvaloniaList<NavigationSideBarItem>> ToolsNavigationSideBarItemsProperty = 
        AvaloniaProperty.Register<KanaNavigationView, AvaloniaList<NavigationSideBarItem>>(nameof(ToolsNavigationSideBarItems));

    internal AvaloniaList<NavigationSideBarItem> ToolsNavigationSideBarItems
    {
        get => GetValue(ToolsNavigationSideBarItemsProperty);
        set => SetValue(ToolsNavigationSideBarItemsProperty, value);
    }

    internal static readonly StyledProperty<AvaloniaList<NavigationSideBarItem>> BottomNavigationSideBarItemsProperty = 
        AvaloniaProperty.Register<KanaNavigationView, AvaloniaList<NavigationSideBarItem>>(nameof(BottomNavigationSideBarItems));

    internal AvaloniaList<NavigationSideBarItem> BottomNavigationSideBarItems
    {
        get => GetValue(BottomNavigationSideBarItemsProperty);
        set => SetValue(BottomNavigationSideBarItemsProperty, value);
    }
}