using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Player;
using KanaPlayer.Models;

namespace KanaPlayer.ViewModels.Pages;

public partial class PlayListViewModel : ViewModelBase
{
    [ObservableProperty] public partial PlayListItem? SelectedPlayListItem { get; set; }

    [RelayCommand]
    private async Task PlaySelectedItemAsync()
    {
        if (SelectedPlayListItem is null)
            return;
        await PlayerManager.LoadAsync(SelectedPlayListItem);
        PlayerManager.Play();
    }

    [RelayCommand]
    private void Clear()
        => PlayerManager.Clear();

    public IPlayerManager PlayerManager { get; }
    private readonly IConfigurationService<SettingsModel> _configurationService;
    public PlayListViewModel(IPlayerManager playerManager, IConfigurationService<SettingsModel> configurationService)
    {
        PlayerManager = playerManager;
        _configurationService = configurationService;
        
        playerManager.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IPlayerManager.CurrentPlayListItem))
            {
                SelectedPlayListItem = playerManager.CurrentPlayListItem;
            }
        };
        SelectedPlayListItem = playerManager.CurrentPlayListItem;
    }
}