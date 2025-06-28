using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels.Pages;
using Lucide.Avalonia;

namespace KanaPlayer.Views.Pages;

public partial class AccountView : NavigablePageBase
{
    public AccountView(AccountViewModel accountViewModel) : base("账户")
    {
        InitializeComponent();
        DataContext = accountViewModel;
    }
}