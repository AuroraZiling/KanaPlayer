using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.ViewModels.Pages;
using Lucide.Avalonia;

namespace KanaPlayer.Views.Pages;

public partial class DownloadQueueView : NavigablePageBase
{
    public DownloadQueueView(DownloadQueueViewModel downloadQueueViewModel) : base("下载队列", LucideIconKind.Download, NavigationPageCategory.Tools, 0)
    {
        InitializeComponent();
        DataContext = downloadQueueViewModel;
    }
}