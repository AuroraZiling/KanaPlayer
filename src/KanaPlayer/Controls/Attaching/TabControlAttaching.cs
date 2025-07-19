using Avalonia;
using Avalonia.Controls;

namespace KanaPlayer.Controls.Attaching;

public class TabControlAttaching : AvaloniaObject
{
    public static string EmptyHeader => string.Empty;
    public static readonly AttachedProperty<string> HeaderProperty = 
        AvaloniaProperty.RegisterAttached<TabControlAttaching, TabControl, string>("Header");
    public static void SetHeader(TabControl control, string header) => control.SetValue(HeaderProperty, header);
    public static string GetHeader(TabControl control) => control.GetValue(HeaderProperty);
}