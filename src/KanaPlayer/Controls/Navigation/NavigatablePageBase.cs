using System;
using Avalonia.Controls;
using Avalonia.Layout;
using Lucide.Avalonia;

namespace KanaPlayer.Controls.Navigation;

public abstract class NavigablePageBase : UserControl
{
    protected NavigablePageBase(string pageTitle, LucideIconKind iconKind, NavigationPageCategory category, int index)
    {
        IconKind = iconKind;
        PageTitle = pageTitle;
        Category = category;
        Index = index;
        OnSideBar = true;
    }

    protected NavigablePageBase(string pageTitle)
    {
        PageTitle = pageTitle;
        OnSideBar = false;
    }
    internal string PageTitle { get; }
    internal LucideIconKind IconKind { get; }
    internal NavigationPageCategory Category { get; }
    internal int Index { get; }
    internal bool OnSideBar { get; }
}