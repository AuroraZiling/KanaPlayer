using Avalonia;
using Avalonia.Controls;

namespace KanaPlayer.Controls;

public class KanaSettingsGroup : ItemsControl
{
    public static readonly StyledProperty<string> HeaderProperty = AvaloniaProperty.Register<KanaSettingsGroup, string>(
        nameof(Header));

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}