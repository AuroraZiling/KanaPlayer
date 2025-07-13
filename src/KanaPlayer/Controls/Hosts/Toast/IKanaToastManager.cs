using System;

namespace KanaPlayer.Controls.Hosts;

public interface IKanaToastManager
{
    event KanaToastQueuedEventHandler OnToastQueued;
    event KanaToastDismissedEventHandler OnToastDismissed;
    event EventHandler OnAllToastsDismissed;
    void Queue(IKanaToast toast);
    void Dismiss(IKanaToast toast, KanaToastDismissSource dismissSource = KanaToastDismissSource.Code);
    public void Dismiss(int index, KanaToastDismissSource dismissSource = KanaToastDismissSource.Code);
    void DismissRange(int startIndex, int count);
    void EnsureMaximum(int maxAllowed);
    void DismissAll();
    bool IsDismissed(IKanaToast toast);
    public void SetDismissTimerPollingInterval(int milliseconds);
    public void SetDismissTimerPollingInterval(TimeSpan timeSpan);
}