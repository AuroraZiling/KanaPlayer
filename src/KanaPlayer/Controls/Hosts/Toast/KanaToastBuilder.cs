using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using KanaPlayer.Services.Theme;

namespace KanaPlayer.Controls.Hosts;

public class KanaToastBuilder
{
    private IKanaToastManager Manager { get; }
    private IKanaToast Toast { get; }

    public KanaToastBuilder(IKanaToastManager manager)
    {
        Manager = manager;
        Toast = ToastPool.Get();
        Toast.Manager = Manager;
    }

    public IKanaToast Queue()
    {
        Manager.Queue(Toast);
        return Toast;
    }

    public void SetTitle(string title) => Toast.Title = title;

    public void SetContent(object? content) => Toast.Content = content;

    public void SetCanDismissByClicking(bool canDismiss) => Toast.CanDismissByClicking = canDismiss;
    public void SetLoadingState(bool loading) => Toast.LoadingState = loading;

    public void SetType(NotificationType type)
    {
        Toast.Background = type switch
        {
            NotificationType.Information => new SolidColorBrush(ThemeService.KanaNotificationInformationColor, opacity: 0.1),
            NotificationType.Success     => new SolidColorBrush(ThemeService.KanaNotificationSuccessColor, opacity: 0.1),
            NotificationType.Warning     => new SolidColorBrush(ThemeService.KanaNotificationWarningColor, opacity: 0.1),
            NotificationType.Error       => new SolidColorBrush(ThemeService.KanaNotificationErrorColor, opacity: 0.1),
            _                            => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    public void SetDismissAfter(TimeSpan delay, bool interruptWhileHover = true)
    {
        Toast.InterruptDismissTimerWhileHover = interruptWhileHover;
        Toast.CanDismissByTime = delay.TotalMilliseconds > 0;
        Toast.DismissTimeout = delay;
    }

    public void SetOnDismiss(Action<IKanaToast, KanaToastDismissSource> action) => Toast.OnDismissed = action;

    public void SetOnClicked(Action<IKanaToast> action) => Toast.OnClicked = action;

    public void AddActionButton(object buttonContent, Action<IKanaToast> action, bool dismissOnClick)
    {
        if (buttonContent is not Button btn)
        {
            btn = new Button
            {
                Content = buttonContent,
            };
        }

        btn.Tag = (action, dismissOnClick);
        Toast.ActionButtons.Add(btn);
    }

    public class DismissToast(KanaToastBuilder builder)
    {
        public KanaToastBuilder Builder { get; } = builder;
    }
}