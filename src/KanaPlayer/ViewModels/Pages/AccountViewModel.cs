using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Wrappers.User;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Models;
using NLog;
using QRCoder;

namespace KanaPlayer.ViewModels.Pages;


public partial class AccountViewModel(
    IBilibiliClient bilibiliClient,
    IConfigurationService<SettingsModel> configurationService) : ViewModelBase, INavigationAware
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(AccountViewModel));
    [ObservableProperty] public partial bool? IsLoggedIn { get; private set; } = null;
    [ObservableProperty] public partial string? AvatarUrl { get; set; } = null;
    [ObservableProperty] public partial string? UserName { get; set; } = null;
    [ObservableProperty] public partial ulong? Mid { get; set; } = null;
    [ObservableProperty] public partial string? VipLabelUrl { get; set; } = null;

    #region Login

    [ObservableProperty] public partial bool LoginAttempting { get; set; } = false;
    [ObservableProperty] public partial string LoginAttemptingStatus { get; set; } = "等待登录";
    [ObservableProperty] public partial Bitmap? LoginAttemptingQrCodeImage { get; set; }

    [RelayCommand]
    private async Task LoginAsync()
    {
        LoginAttempting = true;
        ScopedLogger.Info("开始登录流程");
        try
        {
            LoginAttemptingStatus = "正在获取二维码";
            var applyQrCodeModel = await bilibiliClient.GetApplyQrCodeAsync();
            if (applyQrCodeModel.Data is null)
            {
                LoginAttemptingStatus = "获取二维码失败";
                LoginAttempting = false;
                ScopedLogger.Error("获取二维码失败，错误信息：{ErrorMessage}", applyQrCodeModel.Message);
                return;
            }
            ScopedLogger.Info("获取二维码成功，开始生成二维码图片");
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(applyQrCodeModel.Data.Url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new BitmapByteQRCode(qrCodeData);
            using var stream = new MemoryStream(qrCode.GetGraphic(1, [251, 114, 153], [255, 255, 255]));
            LoginAttemptingQrCodeImage = new Bitmap(stream);
            
            ScopedLogger.Info("二维码图片生成成功，开始轮询登录状态");
            LoginQrCodeModel? succeedLoginQrCodeModel = null;
            while (succeedLoginQrCodeModel == null)
            {
                var loginQrCodeModel = await bilibiliClient.GetLoginQrCodeAsync(applyQrCodeModel.Data.QrCodeKey);
                switch (loginQrCodeModel.EnsureData().Code)
                {
                    case 86101: // 未扫码
                        LoginAttemptingStatus = "请扫码登录";
                        break;
                    case 86090: // 扫码未确认
                        LoginAttemptingStatus = "请在手机上确认登录";
                        break;
                    case 86038: // 已失效
                        LoginAttemptingStatus = "二维码已失效，请重新登录";
                        ScopedLogger.Warn("二维码已失效，重新登录");
                        return;
                    case 0 when loginQrCodeModel is { Cookies.Count: > 0 }: // 登录成功
                        LoginAttemptingStatus = "登录成功";
                        succeedLoginQrCodeModel = loginQrCodeModel;
                        await bilibiliClient.AuthenticateAsync();
                        LoadUserInfo(configurationService.Settings.CommonSettings.Account);
                        IsLoggedIn = true;
                        ScopedLogger.Info("登录成功");
                        break;
                    default:
                        LoginAttemptingStatus = "服务器错误";
                        ScopedLogger.Error("登录失败，错误信息：{ErrorMessage}", loginQrCodeModel.Message);
                        return;
                }
                await Task.Delay(1000);
            }

            configurationService.Settings.CommonSettings.Authentication = new CommonAuthenticationSettings
            {
                Timestamp = succeedLoginQrCodeModel.EnsureData().Timestamp,
                RefreshToken = succeedLoginQrCodeModel.EnsureData().RefreshToken,
                Cookies = succeedLoginQrCodeModel.Cookies!
            };
            configurationService.SaveImmediate();
            ScopedLogger.Info("登录信息保存成功");
        }
        catch (Exception e)
        {
            LoginAttemptingStatus = "登录失败，请重试";
            ScopedLogger.Error(e, "登录过程中发生异常");
        }
        finally
        {
            LoginAttemptingQrCodeImage = null;
            LoginAttempting = false;
        }
    }

    #endregion

    private void LoadUserInfo(CommonAccountSettings? accountInfo)
    {
        if (accountInfo is null)
        {
            ClearUserInfo();
            ScopedLogger.Warn("账户信息为空，无法加载用户信息");
            return;
        }
        AvatarUrl = accountInfo.AvatarImgUri;
        UserName = accountInfo.UserName;
        Mid = accountInfo.Mid;
        VipLabelUrl = accountInfo.VipLabelImgUri;
        ScopedLogger.Info("用户信息加载成功：{UserName} (MID: {Mid})", UserName, Mid);
    }

    [RelayCommand]
    private async Task Logout()
    {
        configurationService.Settings.CommonSettings.Authentication = null;
        configurationService.Settings.CommonSettings.Account = null;
        configurationService.SaveImmediate();
        await bilibiliClient.AuthenticateAsync();
        ClearUserInfo();
        IsLoggedIn = false;
        await LoginAsync();
        ScopedLogger.Info("用户已登出");
    }

    private void ClearUserInfo()
    {
        AvatarUrl = null;
        UserName = null;
        Mid = null;
        VipLabelUrl = null;
        IsLoggedIn = false;
    }

    public async void OnNavigatedTo()
    {
        try
        {
            await bilibiliClient.AuthenticateAsync();
            if (!bilibiliClient.IsAuthenticated)
            {
                IsLoggedIn = false;   
                ScopedLogger.Info("用户未登录，尝试登录");
                await LoginAsync();
            }
            else
            {
                LoadUserInfo(configurationService.Settings.CommonSettings.Account);
                IsLoggedIn = true;
                ScopedLogger.Info("用户已登录，加载用户信息");
            }
        }
        catch (Exception e)
        {
            IsLoggedIn = false;
            ClearUserInfo();
            ScopedLogger.Error(e, "加载用户信息时发生异常");
        }
    }
}