using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels.Pages;

namespace KanaPlayer.Views.Pages;

public partial class AccountView : NavigablePageBase
{
    public AccountView(AccountViewModel accountViewModel) : base("账户")
    {
        InitializeComponent();
        DataContext = accountViewModel;
    }
}