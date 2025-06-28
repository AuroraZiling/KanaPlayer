﻿using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;

namespace KanaPlayer.Controls;

public class FitSquarelyWithinAspectRatioConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var bounds = (Rect)value!;
        return Math.Min(bounds.Width, bounds.Height);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// A control used to indicate the progress of an operation.
/// </summary>
[PseudoClasses(":preserveaspect", ":indeterminate")]
public class ProgressRing : RangeBase
{
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        ProgressBar.IsIndeterminateProperty.AddOwner<ProgressRing>();

    public static readonly StyledProperty<bool> PreserveAspectProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(nameof(PreserveAspect), true);

    public static readonly StyledProperty<double> ValueAngleProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(ValueAngle));

    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(StartAngle));

    public static readonly StyledProperty<double> EndAngleProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(EndAngle), 360);

    static ProgressRing()
    {
        MinimumProperty.Changed.AddClassHandler<ProgressRing>(OnMinimumPropertyChanged);
        MaximumProperty.Changed.AddClassHandler<ProgressRing>(OnMaximumPropertyChanged);
        ValueProperty.Changed.AddClassHandler<ProgressRing>(OnValuePropertyChanged);
        MaximumProperty.Changed.AddClassHandler<ProgressRing>(OnStartAnglePropertyChanged);
        MaximumProperty.Changed.AddClassHandler<ProgressRing>(OnEndAnglePropertyChanged);
    }

    public ProgressRing()
    {
        UpdatePseudoClasses(IsIndeterminate, PreserveAspect);
    }

    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public bool PreserveAspect
    {
        get => GetValue(PreserveAspectProperty);
        set => SetValue(PreserveAspectProperty, value);
    }

    public double ValueAngle
    {
        get => GetValue(ValueAngleProperty);
        private set => SetValue(ValueAngleProperty, value);
    }

    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    public double EndAngle
    {
        get => GetValue(EndAngleProperty);
        set => SetValue(EndAngleProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        var e = change as AvaloniaPropertyChangedEventArgs<bool>;
        if (e is null) return;

        if (e.Property == IsIndeterminateProperty)
        {
            UpdatePseudoClasses(e.NewValue.GetValueOrDefault(), null);
        }
        else if (e.Property == PreserveAspectProperty)
        {
            UpdatePseudoClasses(null, e.NewValue.GetValueOrDefault());
        }
    }

    private void UpdatePseudoClasses(
        bool? isIndeterminate,
        bool? preserveAspect)
    {
        if (isIndeterminate.HasValue)
        {
            PseudoClasses.Set(":indeterminate", isIndeterminate.Value);
        }

        if (preserveAspect.HasValue)
        {
            PseudoClasses.Set(":preserveaspect", preserveAspect.Value);
        }
    }

    static void OnMinimumPropertyChanged(ProgressRing sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.Minimum = (double)e.NewValue!;
    }

    static void OnMaximumPropertyChanged(ProgressRing sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.Maximum = (double)e.NewValue!;
    }

    static void OnValuePropertyChanged(ProgressRing sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.ValueAngle = ((double)e.NewValue! - sender.Minimum) * (sender.EndAngle - sender.StartAngle) / (sender.Maximum - sender.Minimum);
    }

    static void OnStartAnglePropertyChanged(ProgressRing sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.StartAngle = (double)e.NewValue!;
    }

    static void OnEndAnglePropertyChanged(ProgressRing sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.EndAngle = (double)e.NewValue!;
    }
}