using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Services.Player;
using NLog;

namespace KanaPlayer.ViewModels.Pages;

public partial class PlayListViewModel : ViewModelBase
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(PlayListViewModel));
    
    [ObservableProperty] public partial PlayListItem? SelectedPlayListItem { get; set; }

    [RelayCommand]
    private async Task PlaySelectedItemAsync()
    {
        if (SelectedPlayListItem is null)
        {
            ScopedLogger.Warn("尝试播放空的选中项");
            return;
        }
        await PlayerManager.LoadAndPlayAsync(SelectedPlayListItem);
    }

    [RelayCommand]
    private void Clear()
    {
        PlayerManager.Clear();
        ScopedLogger.Info("播放列表已清空");
    }

    public IPlayerManager PlayerManager { get; }
    public PlayListViewModel(IPlayerManager playerManager)
    {
        PlayerManager = playerManager;
        
        playerManager.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IPlayerManager.CurrentPlayListItem))
                SelectedPlayListItem = playerManager.CurrentPlayListItem;
        };
        SelectedPlayListItem = playerManager.CurrentPlayListItem;
    }
}