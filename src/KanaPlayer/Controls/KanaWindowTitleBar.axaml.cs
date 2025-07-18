using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using KanaPlayer.Core.Helpers;

namespace KanaPlayer.Controls;

[TemplatePart(Name = "PART_KanaWindowTitleBarMinimizeButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_KanaWindowTitleBarMaximizeButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_KanaWindowTitleBarCloseButton", Type = typeof(Button))]
public class KanaWindowTitleBar : TemplatedControl
{
    public static readonly StyledProperty<IImage> IconProperty = AvaloniaProperty.Register<KanaWindowTitleBar, IImage>(
        nameof(Icon));

    public IImage Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<KanaWindowTitleBar, string>(
        nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<bool> IsDebuggingProperty = AvaloniaProperty.Register<KanaWindowTitleBar, bool>(
        nameof(IsDebugging), AppHelper.IsDebug);

    public bool IsDebugging
    {
        get => GetValue(IsDebuggingProperty);
        set => SetValue(IsDebuggingProperty, value);
    }
    
    private void EnableWindowsSnapLayout(Button maximize)
    {
        if (TopLevel.GetTopLevel(this) is not Window window) return;
        var pointerOnMaxButton = false;
        var setter = typeof(Button).GetProperty("IsPointerOver");
        var proc = (IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
        {
            switch (msg)
            {
                case 533:
                    if (!pointerOnMaxButton) break;
                    window.WindowState = window.WindowState == WindowState.Maximized
                        ? WindowState.Normal
                        : WindowState.Maximized;
                    break;
                case 0x0084:
                    var point = new PixelPoint(
                        (short)(ToInt32(lParam) & 0xffff),
                        (short)(ToInt32(lParam) >> 16));
                    var size = maximize.Bounds;
                    var buttonLeftTop = maximize.PointToScreen(FlowDirection == FlowDirection.LeftToRight
                        ? new Point(size.Width, 0)
                        : new Point(0, 0));
                    var x = (buttonLeftTop.X - point.X) / window.RenderScaling;
                    var y = (point.Y - buttonLeftTop.Y) / window.RenderScaling;
                    if (new Rect(0, 0,
                            size.Width,
                            size.Height)
                        .Contains(new Point(x, y)))
                    {
                        setter?.SetValue(maximize, true);
                        pointerOnMaxButton = true;
                        handled = true;
                        return 9;
                    }

                    pointerOnMaxButton = false;
                    setter?.SetValue(maximize, false);
                    break;
            }

            return IntPtr.Zero;

            static int ToInt32(IntPtr ptr)
            {
                return IntPtr.Size == 4
                    ? ptr.ToInt32()
                    : (int)(ptr.ToInt64() & 0xffffffff);
            }
        };

        Win32Properties.AddWndProcHookCallback(window, new Win32Properties.CustomWndProcHookCallback(proc));
    }
    
    
    public static readonly StyledProperty<Control> RightControlProperty = 
        AvaloniaProperty.Register<KanaWindowTitleBar, Control>(nameof(RightControl));

    public Control RightControl
    {
        get => GetValue(RightControlProperty);
        set => SetValue(RightControlProperty, value);
    }

    public event EventHandler? MinimizeButtonClick;
    private IDisposable? _minimizeButtonClickSubscription;

    public event EventHandler? MaximizeButtonClick;
    private IDisposable? _maximizeButtonClickSubscription;

    public event EventHandler? CloseButtonClick;
    private IDisposable? _closeButtonClickSubscription;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _minimizeButtonClickSubscription?.Dispose();
        _maximizeButtonClickSubscription?.Dispose();
        _closeButtonClickSubscription?.Dispose();
        
        var minimizeButton = e.NameScope.Find<Button>("PART_KanaWindowTitleBarMinimizeButton");
        _minimizeButtonClickSubscription = minimizeButton?.AddDisposableHandler(
            Button.ClickEvent, (s, args) =>
            {
                args.Handled = true;
                MinimizeButtonClick?.Invoke(s, EventArgs.Empty);
            });

        var maximizeButton = e.NameScope.Find<Button>("PART_KanaWindowTitleBarMaximizeButton");
        if (maximizeButton != null)
        {
            EnableWindowsSnapLayout(maximizeButton);
            _maximizeButtonClickSubscription = maximizeButton.AddDisposableHandler(
                Button.ClickEvent, (s, args) =>
                {
                    args.Handled = true;
                    MaximizeButtonClick?.Invoke(s, EventArgs.Empty);
                });
        }
        
        var closeButton = e.NameScope.Find<Button>("PART_KanaWindowTitleBarCloseButton");
        _closeButtonClickSubscription = closeButton?.AddDisposableHandler(
            Button.ClickEvent, (s, args) =>
            {
                args.Handled = true;
                CloseButtonClick?.Invoke(s, EventArgs.Empty);
            });
        
        base.OnApplyTemplate(e);
    }
}