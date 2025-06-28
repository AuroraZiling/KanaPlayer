using System;
using Avalonia.Controls;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels;

namespace KanaPlayer.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }
    
    public MainView(MainViewModel mainViewModel, INavigationService navigationService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        DataContext = mainViewModel;
        navigationService.Initialize(MainNavigationView, serviceProvider);
    }
}