using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Hosts;

namespace KanaPlayer.ViewModels.Pages;

public partial class FavoritesViewModel(IKanaToastManager kanaToastManager, IKanaDialogManager kanaDialogManager): ViewModelBase
{
    [RelayCommand]
    private void Test()
    {
        kanaToastManager.CreateSimpleInfoToast()
                        .WithTitle("A Simple Toast")
                        .WithContent("This is the content of an info toast.")
                        .Queue();
    }
}