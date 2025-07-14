using Lucide.Avalonia;

namespace KanaPlayer.Controls.Navigation;

public class MainNavigablePageBase: NavigablePageBase
{
    public MainNavigablePageBase(string pageTitle, LucideIconKind iconKind, NavigationPageCategory category, int index) : base(pageTitle, iconKind, category, index)
    {
    }
    public MainNavigablePageBase(string pageTitle) : base(pageTitle)
    {
    }
}