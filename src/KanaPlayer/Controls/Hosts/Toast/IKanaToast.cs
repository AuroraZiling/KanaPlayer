using System;
using System.Collections.ObjectModel;
using Avalonia.Media;

namespace KanaPlayer.Controls.Hosts;

public interface IKanaToast
{
    IKanaToastManager? Manager { get; set; }
    double DismissStartTimestamp { get; set; }
    string Title { get; set; }
    object? Content { get; set; }
    IBrush? Background { get; set; }
    bool CanDismissByClicking { get; set; }
    bool CanDismissByTime { get; set; }
    bool InterruptDismissTimerWhileHover { get; set; }
    TimeSpan DismissTimeout { get; set; }
    bool LoadingState { get; set; }
    Action<IKanaToast, KanaToastDismissSource>? OnDismissed { get; set; }
    Action<IKanaToast>? OnClicked { get; set; }
    ObservableCollection<object> ActionButtons { get; }
    void AnimateShow();
    void AnimateDismiss();
    IKanaToast ResetToDefault();

}