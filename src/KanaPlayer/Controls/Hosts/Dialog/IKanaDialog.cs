using System;
using System.Collections.ObjectModel;

namespace KanaPlayer.Controls.Hosts;

public interface IKanaDialog
{
    IKanaDialogManager? Manager { get; set; }
    object? ViewModel { get; set; }
    string? Title { get; set; }
    object? Content { get; set; }
    ObservableCollection<object> ActionButtons { get; }
    Action<IKanaDialog>? OnDismissed { get; set; }
    bool CanDismissWithBackgroundClick { get; set; }
    bool ShowCardBackground { get; set; }
    void Dismiss();
    void ResetToDefault();
}