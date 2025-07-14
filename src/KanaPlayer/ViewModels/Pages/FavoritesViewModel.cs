using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Hosts;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Helpers;
using KanaPlayer.Core.Models.Favorites;
using KanaPlayer.ViewModels.Pages.SubPages;
using KanaPlayer.Views.Pages.SubPages;

namespace KanaPlayer.ViewModels.Pages;

public partial class FavoritesViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    
    public FavoritesViewModel(INavigationService navigationService, IKanaToastManager kanaToastManager, IKanaDialogManager kanaDialogManager)
    {
        _navigationService = navigationService;
        
        InitializeFavoriteFoldersCommand.Execute(null);
    }
    
    [RelayCommand]
    private async Task InitializeFavoriteFoldersAsync()
    {
        Directory.CreateDirectory(AppHelper.ApplicationFavoritesFolderPath);
        
        foreach (var file in Directory.GetFiles(AppHelper.ApplicationFavoritesFolderPath, "*.json"))
        {
            try
            {
                await using var stream = File.OpenRead(file);
                var favoriteFolder = await JsonSerializer.DeserializeAsync<FavoriteFolderItem>(stream);
                if (favoriteFolder != null)
                    FavoriteFolders.Add(favoriteFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading favorite folder from {file}: {ex.Message}");
            }
        }
    }
    
    [ObservableProperty] public partial ObservableCollection<FavoriteFolderItem> FavoriteFolders { get; set; } = [];
    [ObservableProperty] public partial int SelectedFavoriteFolderIndex { get; set; } = -1;

    [RelayCommand]
    private void ImportFromBilibili()
    {
        
        _navigationService.Navigate(App.GetService<FavoritesBilibiliImportView>());
    }
    
    [RelayCommand]
    private void VisitSelectedFavoriteFolder()
    {
        // TODO: Implement the logic to visit the selected favorite folder.
    }
}