using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Services.Player;
using NLog;

namespace KanaPlayer.ViewModels.Pages;

public partial class HistoryViewModel(IPlayerManager playerManager): ViewModelBase, INavigationAware
{
    
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(HistoryViewModel));
    public IPlayerManager PlayerManager { get; } = playerManager;
    
    [ObservableProperty] public partial PlayListItem? SelectedHistoryPlayListItem { get; set; }

    [RelayCommand]
    private async Task PlaySelectedItemAsync()
    {
        if (SelectedHistoryPlayListItem is null)
        {
            ScopedLogger.Warn("尝试播放空的选中项");
            return;
        }
        await PlayerManager.AppendAsync(SelectedHistoryPlayListItem, detectFirstPlay: false);
        await PlayerManager.LoadAndPlayAsync(SelectedHistoryPlayListItem);
    }
    
    [RelayCommand]
    private void ClearHistory()
    {
        PlayerManager.ClearHistory();
    }
    public void OnNavigatedTo()
    {
    }
}