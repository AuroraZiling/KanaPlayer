using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels.Pages;
using Lucide.Avalonia;

namespace KanaPlayer.Views.Pages;

public partial class FavoritesView : NavigablePageBase
{
    public FavoritesView(FavoritesViewModel favoritesViewModel) : base("收藏夹", LucideIconKind.Star, NavigationPageCategory.AccountFeatures, 0)
    {
        InitializeComponent();
        DataContext = favoritesViewModel;
    }
}