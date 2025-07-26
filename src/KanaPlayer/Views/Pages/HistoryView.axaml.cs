using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels.Pages;
using Lucide.Avalonia;

namespace KanaPlayer.Views.Pages;

public partial class HistoryView : MainNavigablePageBase
{
    public HistoryView() : base("历史记录", LucideIconKind.History, NavigationPageCategory.Top, 2)
    {
        InitializeComponent();
        DataContext = App.GetService<HistoryViewModel>();
    }
}