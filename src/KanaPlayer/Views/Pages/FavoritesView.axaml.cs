using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels.Pages;
using Lucide.Avalonia;

namespace KanaPlayer.Views.Pages;

public partial class FavoritesView : MainNavigablePageBase
{
    public FavoritesView() : base("收藏夹", LucideIconKind.Star, NavigationPageCategory.AccountFeatures, 0)
    {
        InitializeComponent();
        DataContext = App.GetService<FavoritesViewModel>();
    }
}