using System.ComponentModel;
using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Services;

public interface IBilibiliClient : INotifyPropertyChanged
{
    #region Login
    bool IsAuthenticated { get; }
    Task AuthenticateAsync();
    bool TryGetCookies(out Dictionary<string, string> cookies);
    Task<ApplyQrCodeModel> GetApplyQrCodeAsync();
    Task<LoginQrCodeModel> GetLoginQrCodeAsync(string qrCodeKey);

    #endregion

    #region Account

    Task<AccountNavInfoModel> GetAccountNavInfoAsync(Dictionary<string, string> cookies);

    #endregion

    #region Feed

    Task<AudioRegionFeedModel> GetAudioRegionFeedAsync(Dictionary<string, string> cookies, int requestCount = 15);

    #endregion

    #region Music

    Task<AudioInfoModel> GetAudioInfoAsync(string bvid, Dictionary<string, string> cookies);

    Task<string> GetAudioUrlAsync(string bvid, Dictionary<string, string> cookies);
    Task<Stream> GetAudioStreamAsync(string bvid,
        Dictionary<string, string> cookies);

    #endregion
}