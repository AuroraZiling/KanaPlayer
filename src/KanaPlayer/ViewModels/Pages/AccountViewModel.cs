using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Models;
using QRCoder;

namespace KanaPlayer.ViewModels.Pages;


public partial class AccountViewModel(
    IBilibiliClient bilibiliClient,
    IConfigurationService<SettingsModel> configurationService) : ViewModelBase, INavigationAware
{
    [ObservableProperty] public partial bool? IsLoggedIn { get; private set; } = null;
    [ObservableProperty] public partial string? AvatarUrl { get; set; } = null;
    [ObservableProperty] public partial string? UserName { get; set; } = null;
    [ObservableProperty] public partial ulong? Mid { get; set; } = null;
    [ObservableProperty] public partial string? VipLabelUrl { get; set; } = null;

    #region Login Process

    [ObservableProperty] public partial bool LoginAttempting { get; set; } = false;
    [ObservableProperty] public partial string LoginAttemptingStatus { get; set; } = "等待登录";
    [ObservableProperty] public partial Bitmap? LoginAttemptingQrCodeImage { get; set; }

    [RelayCommand]
    private async Task LoginAsync()
    {
        LoginAttempting = true;
        try
        {
            LoginAttemptingStatus = "正在获取二维码";
            var applyQrCodeModel = await bilibiliClient.GetApplyQrCodeAsync();
            if (applyQrCodeModel.Data is null)
            {
                LoginAttemptingStatus = "获取二维码失败";
                LoginAttempting = false;
                return;
            }
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(applyQrCodeModel.Data.Url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new BitmapByteQRCode(qrCodeData);
            using var stream = new MemoryStream(qrCode.GetGraphic(1, [251, 114, 153], [255, 255, 255]));
            LoginAttemptingQrCodeImage = new Bitmap(stream);

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
                        LoginAttemptingQrCodeImage = null;
                        LoginAttempting = false;
                        return;
                    case 0 when loginQrCodeModel is { Cookies.Count: > 0 }: // 登录成功
                        LoginAttemptingStatus = "登录成功";
                        LoginAttemptingQrCodeImage = null;
                        succeedLoginQrCodeModel = loginQrCodeModel;
                        break;
                    default:
                        LoginAttemptingStatus = "服务器错误";
                        LoginAttemptingQrCodeImage = null;
                        LoginAttempting = false;
                        return;
                }
                await Task.Delay(1000);
            }


            configurationService.Settings.CommonSettings.Authentication = new CommonAuthenticationSettings()
            {
                Timestamp = succeedLoginQrCodeModel.EnsureData().Timestamp,
                RefreshToken = succeedLoginQrCodeModel.EnsureData().RefreshToken, 
                Cookies = succeedLoginQrCodeModel.Cookies!
            };
            configurationService.Save();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        await bilibiliClient.AuthenticateAsync();
        LoadUserInfo(configurationService.Settings.CommonSettings.Account);
        
        LoginAttempting = false;
        IsLoggedIn = true;
    }

    #endregion

    private void LoadUserInfo(CommonAccountSettings? accountInfo)
    {
        if (accountInfo is null)
        {
            ClearUserInfo();
            return;
        }
        AvatarUrl = accountInfo.AvatarImgUri;
        UserName = accountInfo.UserName;
        Mid = accountInfo.Mid;
        VipLabelUrl = accountInfo.VipLabelImgUri;
    }

    [RelayCommand]
    private async Task Logout()
    {
        configurationService.Settings.CommonSettings.Authentication = null;
        configurationService.Settings.CommonSettings.Account = null;
        configurationService.Save();
        await bilibiliClient.AuthenticateAsync();
        ClearUserInfo();
        IsLoggedIn = false;
        await LoginAsync();
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
                await LoginAsync();
            }
            else
            {
                LoadUserInfo(configurationService.Settings.CommonSettings.Account);
                IsLoggedIn = true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            IsLoggedIn = false;
            ClearUserInfo();
        }
    }
}