using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels.Pages;
using Lucide.Avalonia;

namespace KanaPlayer.Views.Pages;

public partial class SettingsView : NavigablePageBase
{
    public SettingsView(SettingsViewModel settingsViewModel) : base("设置")
    {
        InitializeComponent();
        DataContext = settingsViewModel;
    }
}