using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels.Pages;
using Lucide.Avalonia;

namespace KanaPlayer.Views.Pages;

public partial class PlayListView : NavigablePageBase
{
    public PlayListView(PlayListViewModel playListViewModel) : base("播放列表", LucideIconKind.ListMusic, NavigationPageCategory.Top, 1)
    {
        InitializeComponent();
        DataContext = playListViewModel;
    }
}