using System;
using Avalonia.Controls;
using Avalonia.Input;
using KanaPlayer.ViewModels;

namespace KanaPlayer.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel mainWindowViewModel, MainView mainView)
    {
        InitializeComponent();
        DataContext = mainWindowViewModel;

        mainView.SetValue(Grid.RowProperty, 1);
        MainWindowGrid.Children.Add(mainView);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if(e.GetCurrentPoint(this).Position.Y <= 50)
            BeginMoveDrag(e);
    }
    
    private void KanaWindowTitleBar_OnMinimizeButtonClick(object? sender, EventArgs e)
        => WindowState = WindowState.Minimized;

    private void KanaWindowTitleBar_OnMaximizeButtonClick(object? sender, EventArgs e)
        => WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    private void KanaWindowTitleBar_OnCloseButtonClick(object? sender, EventArgs e)
        => Environment.Exit(0);  // TODO: Customized
}