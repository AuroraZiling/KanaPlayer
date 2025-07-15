using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Hosts;
using KanaPlayer.Core.Models.Favorites;

namespace KanaPlayer.ViewModels.Dialogs;

public partial class FavoritesBilibiliImportDialogViewModel(IKanaDialog kanaDialog, FavoriteFolderImportItem importItem): ViewModelBase
{
    [ObservableProperty]
    public partial FavoriteFolderImportItem ImportItem { get; set; } = importItem;
    
    [ObservableProperty] public partial int ImportedMediaCount { get; set; } = 0;
    [ObservableProperty] public partial string ImportingMediaTitle { get; set; } = string.Empty;

    [RelayCommand]
    private void Close()
    {
        kanaDialog.Dismiss();
    }
}