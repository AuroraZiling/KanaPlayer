using System;
using Lucide.Avalonia;

namespace KanaPlayer.Controls.Navigation;

public sealed class NavigationSideBarItem
{
    public required LucideIconKind IconKind { get; set; }
    public required string ViewName { get; set; }
    public required Type ViewType { get; set; }
}