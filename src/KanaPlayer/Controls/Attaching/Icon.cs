using Avalonia;
using Avalonia.Controls;
using Lucide.Avalonia;

namespace KanaPlayer.Controls.Attaching;

public class Icon : AvaloniaObject
{
    #region LucideIconKind
    
    public static LucideIconKind EmptyLucideIconKind => 0;
    public static readonly AttachedProperty<LucideIconKind> LucideIconKindProperty = 
        AvaloniaProperty.RegisterAttached<Icon, Control, LucideIconKind>("LucideIconKind");
    public static void SetLucideIconKind(Control control, LucideIconKind lucideIconKind) => control.SetValue(LucideIconKindProperty, lucideIconKind);
    public static LucideIconKind GetLucideIconKind(Control control) => control.GetValue(LucideIconKindProperty);
    
    #endregion

    #region Dock

    public static readonly AttachedProperty<Dock> DockProperty =
        AvaloniaProperty.RegisterAttached<Icon, Control, Dock>("Dock");
    public static void SetDock(Control control, Dock value) => control.SetValue(DockProperty, value);
    public static Dock GetDock(Control control) => control.GetValue(DockProperty);

    #endregion
}