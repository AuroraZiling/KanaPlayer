using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels.Pages;

namespace KanaPlayer.Views.Pages;

public partial class AccountView : MainNavigablePageBase
{
    public AccountView() : base("账户")
    {
        InitializeComponent();
        DataContext = App.GetService<AccountViewModel>();
    }
}