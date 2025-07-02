using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using KanaPlayer.Core.Extensions;

namespace KanaPlayer.Behaviors;

public enum ScrollViewerReachEndOrientation
{
    Vertical,
    Horizontal,
    Both
}

public class ScrollViewerReachEndBehavior : Behavior<ScrollViewer>
{
    public static readonly StyledProperty<ICommand?> CommandProperty = 
        AvaloniaProperty.Register<ScrollViewerReachEndBehavior, ICommand?>(nameof(Command));

    /// <summary>
    /// Called when the ScrollViewer reaches the end of its content.
    /// </summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    
    public static readonly StyledProperty<ScrollViewerReachEndOrientation> OrientationProperty = 
        AvaloniaProperty.Register<ScrollViewerReachEndBehavior, ScrollViewerReachEndOrientation>(nameof(Orientation));
    
    /// <summary>
    /// The orientation to check for reaching the end.
    /// </summary>
    public ScrollViewerReachEndOrientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    
    private TimeSpan _interval;

    public static readonly DirectProperty<ScrollViewerReachEndBehavior, TimeSpan> IntervalProperty = 
        AvaloniaProperty.RegisterDirect<ScrollViewerReachEndBehavior, TimeSpan>(
        nameof(Interval), o => o.Interval, (o, v) => o.Interval = v);

    public TimeSpan Interval
    {
        get => _interval;
        set
        {
            if (!SetAndRaise(IntervalProperty, ref _interval, value)) return;
            _executionTimer.Interval = value;
        }
    }

    public static readonly StyledProperty<double> BiasProperty = 
        AvaloniaProperty.Register<ScrollViewerReachEndBehavior, double>(nameof(Bias));

    public double Bias
    {
        get => GetValue(BiasProperty);
        set => SetValue(BiasProperty, value);
    }
    
    private readonly DispatcherTimer _executionTimer = new();
    
    private DateTimeOffset _previousExecutionTime = DateTimeOffset.MinValue;
    
    protected override void OnAttached()
    {
        base.OnAttached();
        
        if (AssociatedObject is null) return;
        AssociatedObject.ScrollChanged += OnScrollChanged;
        _executionTimer.Tick += delegate
        {
            _executionTimer.Stop();
            ExecuteCommand();
        };
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        var scrollViewer = sender.NotNull<ScrollViewer>();
        var viewport = scrollViewer.Viewport;
        var offset = scrollViewer.Offset;
        var extent = scrollViewer.Extent;
        var isAtEnd = Orientation switch
        {
            ScrollViewerReachEndOrientation.Vertical => offset.Y + viewport.Height >= extent.Height - Bias,
            ScrollViewerReachEndOrientation.Horizontal => offset.X + viewport.Width >= extent.Width - Bias,
            ScrollViewerReachEndOrientation.Both => 
                offset.Y + viewport.Height >= extent.Height - Bias && offset.X + viewport.Width >= extent.Width - Bias,
            _ => false
        };
        
        if (!isAtEnd) return;
        
        if (DateTimeOffset.UtcNow - _previousExecutionTime >= Interval)
            ExecuteCommand();
        else
            _executionTimer.Start();

        _previousExecutionTime = DateTimeOffset.UtcNow;
    }
    
    private void ExecuteCommand()
    {
        if (Command is null || !Command.CanExecute(null)) return;
        Command.Execute(null);
    }
}