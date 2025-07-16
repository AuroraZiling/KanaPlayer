using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using KanaPlayer.Helpers.Animations;
using KanaPlayer.Services.Theme;

namespace KanaPlayer.Controls.Hosts;

[TemplatePart("PART_ToastCard", typeof(Border))]
[TemplatePart("PART_DismissProgressBar", typeof(ProgressBar))]
public class KanaToast : ContentControl, IKanaToast
{
    private bool _wasDismissTimerInterrupted;
    private Border? _toastCard;

    public IKanaToastManager? Manager { get; set; }
    public Action<IKanaToast, KanaToastDismissSource>? OnDismissed { get; set; }
    public Action<IKanaToast>? OnClicked { get; set; }

    public static readonly DirectProperty<KanaToast, double> DismissStartTimestampProperty =
        AvaloniaProperty.RegisterDirect<KanaToast, double>(nameof(DismissStartTimestamp), o => o.DismissStartTimestamp,
            (o, value) => o.DismissStartTimestamp = value);

    public double DismissStartTimestamp
    {
        get;
        set => SetAndRaise(DismissStartTimestampProperty, ref field, value);
    }

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<KanaToast, string>(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<bool> LoadingStateProperty = AvaloniaProperty.Register<KanaToast, bool>(nameof(LoadingState));

    public bool LoadingState
    {
        get => GetValue(LoadingStateProperty);
        set => SetValue(LoadingStateProperty, value);
    }

    public static readonly StyledProperty<bool> CanDismissByClickingProperty = AvaloniaProperty.Register<KanaToast, bool>(nameof(CanDismissByClicking));

    public bool CanDismissByClicking
    {
        get => GetValue(CanDismissByClickingProperty);
        set => SetValue(CanDismissByClickingProperty, value);
    }

    public static readonly StyledProperty<bool> CanDismissByTimeProperty = AvaloniaProperty.Register<KanaToast, bool>(nameof(CanDismissByTime));

    public bool CanDismissByTime
    {
        get => GetValue(CanDismissByTimeProperty);
        set => SetValue(CanDismissByTimeProperty, value);
    }

    public static readonly DirectProperty<KanaToast, TimeSpan> DismissTimeoutProperty =
        AvaloniaProperty.RegisterDirect<KanaToast, TimeSpan>(nameof(DismissTimeout), o => o.DismissTimeout, (o, value) => o.DismissTimeout = value);

    private TimeSpan _dismissTimeout = TimeSpan.FromSeconds(5);
    public TimeSpan DismissTimeout
    {
        get => _dismissTimeout;
        set => SetAndRaise(DismissTimeoutProperty, ref _dismissTimeout, value);
    }

    public static readonly StyledProperty<bool> InterruptDismissTimerWhileHoverProperty =
        AvaloniaProperty.Register<KanaToast, bool>(nameof(InterruptDismissTimerWhileHover), true);

    public bool InterruptDismissTimerWhileHover
    {
        get => GetValue(InterruptDismissTimerWhileHoverProperty);
        set => SetValue(InterruptDismissTimerWhileHoverProperty, value);
    }

    public static readonly DirectProperty<KanaToast, ObservableCollection<object>> ActionButtonsProperty =
        AvaloniaProperty.RegisterDirect<KanaToast, ObservableCollection<object>>(nameof(ActionButtons), o => o.ActionButtons,
            (o, value) => o.ActionButtons = value);

    public ObservableCollection<object> ActionButtons
    {
        get;
        set => SetAndRaise(ActionButtonsProperty, ref field, value);
    } = [];

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_toastCard is not null)
        {
            _toastCard.PointerPressed -= ToastCardClickedHandler;
        }

        _toastCard = e.NameScope.Get<Border>("PART_ToastCard");
        _toastCard.PointerPressed += ToastCardClickedHandler;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        foreach (var actionButton in ActionButtons)
        {
            if (actionButton is not Button button) continue;
            if (button.Tag is not ValueTuple<Action<IKanaToast>, bool>) continue;
            button.Click += OnActionButtonClick;
        }

        DismissStartTimestamp = Stopwatch.GetTimestamp() * 1000d / Stopwatch.Frequency;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        DismissStartTimestamp = 0;

        foreach (var actionButton in ActionButtons)
        {
            if (actionButton is not Button button) continue;
            if (button.Tag is not ValueTuple<Action<IKanaToast>, bool>) continue;
            button.Click -= OnActionButtonClick;
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (InterruptDismissTimerWhileHover)
        {
            _wasDismissTimerInterrupted = true;
            DismissStartTimestamp = 0;
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        if (_wasDismissTimerInterrupted)
        {
            _wasDismissTimerInterrupted = false;
            if (IsLoaded && CanDismissByTime)  // Need to check for IsLoaded as this method will still trigger after Unloaded!
                DismissStartTimestamp = Stopwatch.GetTimestamp() * 1000d / Stopwatch.Frequency;
        }
    }

    private void ToastCardClickedHandler(object? o, PointerPressedEventArgs pointerPressedEventArgs)
    {
        OnClicked?.Invoke(this);
        if (!CanDismissByClicking) return;
        Dismiss(KanaToastDismissSource.Click);
    }

    private void OnActionButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.Tag is not ValueTuple<Action<IKanaToast>, bool> tuple) return;
        tuple.Item1(this);
        if (tuple.Item2) // Is dismiss on click
        {
            Dismiss(KanaToastDismissSource.ActionButton);
        }
    }

    public void Dismiss(KanaToastDismissSource dismiss = KanaToastDismissSource.Code)
        => Manager?.Dismiss(this, dismiss);

    public void AnimateShow()
    {
        this.Animate(OpacityProperty, 0d, 1d, TimeSpan.FromMilliseconds(500));
        this.Animate<double>(MaxHeightProperty, 0, 500, TimeSpan.FromMilliseconds(500));
        this.Animate(MarginProperty, new Thickness(0, 10, 0, -10), new Thickness(), TimeSpan.FromMilliseconds(500));
    }

    public void AnimateDismiss()
    {
        this.Animate(OpacityProperty, 1d, 0d, TimeSpan.FromMilliseconds(300));
        this.Animate(MarginProperty, new Thickness(), new Thickness(0, 0, 0, 10), TimeSpan.FromMilliseconds(300));
    }

    public IKanaToast ResetToDefault()
    {
        _wasDismissTimerInterrupted = false;
        DismissStartTimestamp = 0;
        
        Title = string.Empty;
        Content = string.Empty;
        Foreground = new SolidColorBrush(ThemeService.KanaNotificationInformationColor);
        CanDismissByClicking = false;
        CanDismissByTime = false;
        DismissTimeout = TimeSpan.FromSeconds(5);
        InterruptDismissTimerWhileHover = true;

        ActionButtons.Clear();
        OnDismissed = null;
        OnClicked = null;
        LoadingState = false;
        DockPanel.SetDock(this, Dock.Bottom);
        return this;
    }
}

public class KanaToastDismissedEventArgs(IKanaToast toast, KanaToastDismissSource dismissSource) : EventArgs
{
    public IKanaToast Toast { get; init; } = toast;
    public KanaToastDismissSource DismissSource { get; init; } = dismissSource;
}

public delegate void KanaToastDismissedEventHandler(object sender, KanaToastDismissedEventArgs args);

public class KanaToastQueuedEventArgs(IKanaToast toast) : EventArgs
{
    public IKanaToast Toast { get; set; } = toast;
}

public delegate void KanaToastQueuedEventHandler(object sender, KanaToastQueuedEventArgs args);