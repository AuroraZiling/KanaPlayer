using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels.Pages;
using Lucide.Avalonia;

namespace KanaPlayer.Views.Pages;

public partial class PlayListView : MainNavigablePageBase
{
    public PlayListView() : base("播放列表", LucideIconKind.ListMusic, NavigationPageCategory.Top, 1)
    {
        InitializeComponent();
        DataContext = App.GetService<PlayListViewModel>();
    }
}