using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Core.Services.Player;

namespace KanaPlayer.ViewModels;

public partial class MainViewModel(IPlayerService playerService) : ViewModelBase
{
    public IPlayerService PlayerService => playerService;
    
    [RelayCommand]
    private async Task TogglePlayAsync()
    {
        var client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true });
        var httpResponseMessage = await client.GetAsync("https://music.163.com/song/media/outer/url?id=447925558.mp3");
        var resultResponseMessage = httpResponseMessage;

        if (httpResponseMessage.StatusCode == HttpStatusCode.Redirect)
        {
            var location = httpResponseMessage.Headers.Location;
            if (location is not null)
            {
                var newResponseMessage = await client.GetAsync(location);
                resultResponseMessage = newResponseMessage;
            }
        }
        playerService.Load(await resultResponseMessage.Content.ReadAsStreamAsync());
        playerService.Play();
    }
}