using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels.Pages;

namespace KanaPlayer.Views.Pages;

public partial class SettingsView : NavigablePageBase
{
    public SettingsView() : base("设置")
    {
        InitializeComponent();
        DataContext = App.GetService<SettingsViewModel>();
    }
}