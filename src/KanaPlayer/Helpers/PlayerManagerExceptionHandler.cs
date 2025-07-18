using System;
using Avalonia.Controls.Notifications;
using KanaPlayer.Controls.Hosts;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Interfaces;
using NLog;

namespace KanaPlayer.Helpers;

public class PlayerManagerExceptionHandler(IKanaToastManager kanaToastManager): IExceptionHandler
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(PlayerManagerExceptionHandler));
    public void HandleException(Exception exception, string? message = null, object? source = null)
    {
        kanaToastManager.CreateToast()
                        .WithTitle("播放器错误")
                        .WithContent(exception.GetFriendlyMessage())
                        .WithType(NotificationType.Error)
                        .Queue();
        ScopedLogger.Error(exception, "{Message}，源：{Source}", message ?? "发生了一个异常", source);
    }
}