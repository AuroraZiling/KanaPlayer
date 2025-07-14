using System.ComponentModel;

namespace KanaPlayer.Controls.Hosts;

public interface IKanaDialogManager: INotifyPropertyChanged
{
    event KanaDialogManagerEventHandler? OnDialogShown;
    event KanaDialogManagerEventHandler? OnDialogDismissed;
    
    bool TryShowDialog(IKanaDialog dialog);
    bool TryDismissDialog(IKanaDialog dialog);
    void DismissDialog();
}