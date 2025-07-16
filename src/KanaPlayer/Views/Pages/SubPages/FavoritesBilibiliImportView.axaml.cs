using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels.Pages.SubPages;

namespace KanaPlayer.Views.Pages.SubPages;

public partial class FavoritesBilibiliImportView : NavigablePageBase
{
    public FavoritesBilibiliImportView(): base("从 Bilibili 导入收藏夹")
    {
        InitializeComponent();
        DataContext = App.GetService<FavoritesBilibiliImportViewModel>();
    }
}