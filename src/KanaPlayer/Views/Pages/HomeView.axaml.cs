using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels.Pages;
using Lucide.Avalonia;

namespace KanaPlayer.Views.Pages;

public partial class HomeView : NavigablePageBase
{
    public HomeView(HomeViewModel homeViewModel) : base("主页", LucideIconKind.House, NavigationPageCategory.Top, 0)
    {
        InitializeComponent();
        DataContext = homeViewModel;
    }
}