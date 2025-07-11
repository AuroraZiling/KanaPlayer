using System;
using Avalonia.Controls;
using Avalonia.Input;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels;

namespace KanaPlayer.Views;

public partial class MainView : UserControl
{
    private readonly MainViewModel _mainViewModel;

    public MainView(MainViewModel mainViewModel, INavigationService navigationService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        DataContext = _mainViewModel = mainViewModel;
        navigationService.Initialize(MainNavigationView, serviceProvider);
    }

    private void Thumb_OnDragCompleted(object? sender, VectorEventArgs e)
    {
        _mainViewModel.IsSeeking = false;
    }

    private void Thumb_OnDragStarted(object? sender, VectorEventArgs e)
    {
        _mainViewModel.IsSeeking = true;
    }
}