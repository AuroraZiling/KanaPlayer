namespace KanaPlayer.Controls.Hosts;

public interface IKanaDialogManager
{
    event KanaDialogManagerEventHandler? OnDialogShown;
    event KanaDialogManagerEventHandler? OnDialogDismissed;
    
    bool TryShowDialog(IKanaDialog dialog);
    bool TryDismissDialog(IKanaDialog dialog);
    void DismissDialog();
}