using System.ComponentModel;
using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Services;

public interface IBilibiliClient : INotifyPropertyChanged
{
    #region Login
    bool IsAuthenticated { get; }
    Task AuthenticateAsync();
    Task<ApplyQrCodeModel> GetApplyQrCodeAsync();
    Task<LoginQrCodeModel> GetLoginQrCodeAsync(string qrCodeKey);

    #endregion

    #region Account

    Task<AccountNavInfoModel> GetAccountNavInfoAsync(string[] cookies);

    #endregion
}