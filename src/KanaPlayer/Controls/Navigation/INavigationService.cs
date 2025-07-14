using System;
using System.ComponentModel;

namespace KanaPlayer.Controls.Navigation;

public interface INavigationService : INotifyPropertyChanged
{
    void Initialize(KanaNavigationView navigationView, IServiceProvider pageProvider);
    void Navigate(Type viewType);
    void Navigate(NavigablePageBase view);
    NavigablePageBase? CurrentPage { get; }
    bool CanGoBack { get; }
    bool CanGoForward { get; }
    void GoBack();
    void GoForward();
}