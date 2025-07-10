using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace KanaPlayer.Controls;

public class InlineHyperlink : InlineUIContainer
{
    private Button Button { get; set; }

    public InlineCollection Inlines { get; set; } = new();

    private readonly TextDecoration _underlineDecoration = new()
    {
        Location = TextDecorationLocation.Underline,
        StrokeThickness = 0
    };

    public InlineHyperlink()
    {
        Button = new Button
        {
            Name = "InlineHyperlinkButton",
            Padding = new Thickness(2, 0),
            Margin = new Thickness(0),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = new Cursor(StandardCursorType.Hand),
            Content = new TextBlock
            {
                Foreground = Brushes.White,
                Inlines = Inlines
            }
        };
        Child = Button;
        // Button.Click += (_, _) => IsVisited = true;
        // Button.PointerEntered += (_, _) => (Button.Content as TextBlock)!.TextDecorations = [_underlineDecoration];
        // Button.PointerExited += (_, _) => (Button.Content as TextBlock)!.TextDecorations = [];
        Button.Bind(Button.CommandProperty, new Binding(nameof(Command)) { Source = this });
        Button.Bind(Button.CommandParameterProperty, new Binding(nameof(CommandParameter)) { Source = this });
    }
    
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => Button.Click += value;
        remove => Button.Click -= value;
    }

    public static readonly DirectProperty<InlineHyperlink, string?> TextProperty = AvaloniaProperty.RegisterDirect<InlineHyperlink, string?>(
        nameof(Text), o => o.Text, (o, v) => o.Text = v);

    public string? Text
    {
        get => Inlines.Text;
        set
        {
            Inlines.Clear();
            Inlines.Add(new Run(value));
            RaisePropertyChanged(TextProperty, value, value);
        }
    }

    public static readonly StyledProperty<bool> IsVisitedProperty = AvaloniaProperty.Register<InlineHyperlink, bool>(
        nameof(IsVisited));

    public bool IsVisited
    {
        get => GetValue(IsVisitedProperty);
        set
        {
            if (value)
            {
                (Button.Content as TextBlock)!.Foreground = Brushes.MediumPurple;
                _underlineDecoration.Stroke = Brushes.MediumPurple;
            }
            else
            {
                (Button.Content as TextBlock)!.Foreground = Brushes.DodgerBlue;
                _underlineDecoration.Stroke = Brushes.DodgerBlue;
            }
            SetValue(IsVisitedProperty, value);
        }
    }
    
    public static readonly StyledProperty<ICommand?> CommandProperty = 
        AvaloniaProperty.Register<InlineHyperlink, ICommand?>(nameof(Command), enableDataValidation: true);

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<InlineHyperlink, object?>(nameof(CommandParameter));

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
}