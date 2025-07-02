using Avalonia;
using Avalonia.Controls;

namespace KanaPlayer.Controls;

public class KanaSettingsCard : ContentControl
{
    public static readonly StyledProperty<string> HeaderProperty = AvaloniaProperty.Register<KanaSettingsCard, string>(
        nameof(Header));

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly StyledProperty<string> DescriptionProperty = AvaloniaProperty.Register<KanaSettingsCard, string>(
        nameof(Description));

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
}