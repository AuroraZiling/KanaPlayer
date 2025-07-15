using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Services.Favorites;
using KanaPlayer.Views.Pages.SubPages;

namespace KanaPlayer.ViewModels.Pages;

public partial class FavoritesViewModel(INavigationService navigationService, IFavoritesManager favoritesManager) : ViewModelBase, INavigationAware
{
    [RelayCommand]
    private void RefreshFavoriteFolders()
    {
        FavoriteFolders = new ObservableCollection<LocalFavoriteFolderItem>(favoritesManager.GetLocalFavoriteFolders());
    }
    
    [ObservableProperty] public partial ObservableCollection<LocalFavoriteFolderItem> FavoriteFolders { get; set; } = [];
    [ObservableProperty] public partial int SelectedFavoriteFolderIndex { get; set; } = -1;

    [RelayCommand]
    private void ImportFromBilibili()
    {
        navigationService.Navigate(App.GetService<FavoritesBilibiliImportView>());
    }
    
    [RelayCommand]
    private void VisitSelectedFavoriteFolder()
    {
        // TODO: Implement the logic to visit the selected favorite folder.
    }
    
    public void OnNavigatedTo()
    {
        RefreshFavoriteFoldersCommand.Execute(null);
    }
}