using System;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace KanaPlayer.Views;

public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
    }
    public async Task RunAsync(Func<Task> task) 
    {
        Show();
        await task();
        Hide();
    }
}