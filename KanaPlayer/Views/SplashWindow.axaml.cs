using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace KanaPlayer.Views;

public partial class SplashWindow : Window
{
    private readonly Action _mainAction;
    private readonly Func<Task> _task;
    
    public SplashWindow(Action mainAction, Func<Task> task)
    {
        InitializeComponent();
        _mainAction = mainAction;
        _task = task;
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        try
        {
            await _task();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
        finally
        {
            _mainAction.Invoke();
            Close();
        }
    }
}