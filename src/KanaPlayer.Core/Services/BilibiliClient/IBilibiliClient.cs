using System.ComponentModel;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Models.Wrappers.User;

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

    #region Audio

    Task<AudioInfoModel> GetAudioInfoAsync(AudioUniqueId audioUniqueId, Dictionary<string, string> cookies);

    Task<string> GetAudioUrlAsync(AudioUniqueId audioUniqueId, Dictionary<string, string> cookies);

    Task<Stream> GetAudioStreamAsync(AudioUniqueId audioUniqueId, Dictionary<string, string> cookies);

    Task<BiliCollectionMediaListModel> GetCollectionAsync(ulong collectionId, Dictionary<string, string> cookies, bool fetchCompleteMediaList, IProgress<int>? fetchedCountProgress = null);

    #endregion

    #region Favorite

    Task<CreatedBiliFavoriteMediaListMetaModel> GetCreatedBiliFavoriteMediaListMetaAsync(ulong upMid, Dictionary<string, string> cookies);
    Task<List<CollectedBiliFavoriteMediaListMetaDataItemModel>> GetCollectedBiliFavoriteMediaListMetaAsync(ulong upMid, Dictionary<string, string> cookies);
    Task<BiliFavoriteMediaListInfoModel> GetBiliFavoriteMediaListInfoAsync(ulong folderId, Dictionary<string, string> cookies);
    Task<BiliFavoriteMediaListDetailModel> GetBiliFavoriteMediaListDetailAsync(ulong folderId, Dictionary<string, string> cookies, IProgress<int>? fetchedCountProgress = null);

    #endregion
}