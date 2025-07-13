using System;
using Avalonia.Controls;
using Avalonia.Input;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels;

namespace KanaPlayer.Views;

public partial class MainView : UserControl
{
    private readonly MainViewModel _mainViewModel;

    public MainView()
    {
        InitializeComponent();
        DataContext = _mainViewModel = App.GetService<MainViewModel>();
        App.GetService<INavigationService>().Initialize(MainNavigationView, App.GetService<IServiceProvider>());
    }

    private void AudioSliderThumb_OnDragCompleted(object? sender, VectorEventArgs e)
        => _mainViewModel.IsSeeking = false;

    private void AudioSliderThumb_OnDragStarted(object? sender, VectorEventArgs e)
        => _mainViewModel.IsSeeking = true;
}