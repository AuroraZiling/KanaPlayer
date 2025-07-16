using CommunityToolkit.Mvvm.ComponentModel;

namespace KanaPlayer.Controls.Hosts;

public class KanaDialogManager : ObservableObject, IKanaDialogManager
{
    public event KanaDialogManagerEventHandler? OnDialogShown;
    public event KanaDialogManagerEventHandler? OnDialogDismissed;

    private IKanaDialog? _activeDialog;
        
    public bool TryShowDialog(IKanaDialog dialog)
    {
        if (_activeDialog != null) return false;
        _activeDialog = dialog;
        OnDialogShown?.Invoke(this, new KanaDialogManagerEventArgs(_activeDialog));
        return true;
    }

    public bool TryDismissDialog(IKanaDialog dialog)
    {
        if (_activeDialog == null || _activeDialog != dialog) 
            return false;
            
        OnDialogDismissed?.Invoke(this, new KanaDialogManagerEventArgs(_activeDialog));
        _activeDialog.OnDismissed?.Invoke(_activeDialog);
        _activeDialog = null;
        
        return true;
    }

    public void DismissDialog()
    {
        if (_activeDialog == null) 
            return;
        
        OnDialogDismissed?.Invoke(this, new KanaDialogManagerEventArgs(_activeDialog));
        _activeDialog.OnDismissed?.Invoke(_activeDialog);
        _activeDialog = null;
    }
}