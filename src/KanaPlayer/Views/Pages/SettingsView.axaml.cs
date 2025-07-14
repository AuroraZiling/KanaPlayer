using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels.Pages;

namespace KanaPlayer.Views.Pages;

public partial class SettingsView : MainNavigablePageBase
{
    public SettingsView() : base("设置")
    {
        InitializeComponent();
        DataContext = App.GetService<SettingsViewModel>();
    }
}