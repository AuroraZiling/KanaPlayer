using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Rendering.Composition;
using KanaPlayer.Helpers;

namespace KanaPlayer.Controls.Hosts;

public class KanaDialogHost : TemplatedControl
{
    public static readonly StyledProperty<IKanaDialogManager> ManagerProperty = AvaloniaProperty.Register<KanaDialogHost, IKanaDialogManager>(nameof(Manager));

    public IKanaDialogManager Manager
    {
        get => GetValue(ManagerProperty);
        set => SetValue(ManagerProperty, value);
    }
        
    public static readonly StyledProperty<object?> DialogProperty = AvaloniaProperty.Register<KanaDialogHost, object?>(nameof(Dialog));

    internal object? Dialog
    {
        get => GetValue(DialogProperty);
        set => SetValue(DialogProperty, value);
    }

    public static readonly StyledProperty<bool> IsDialogOpenProperty = AvaloniaProperty.Register<KanaDialogHost, bool>(nameof(IsDialogOpen));

    internal bool IsDialogOpen
    {
        get => GetValue(IsDialogOpenProperty);
        set => SetValue(IsDialogOpenProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (e.NameScope.Find<Border>("PART_DialogBackground") is { } dialogBackground)
        {
            dialogBackground.PointerPressed += (_, _) => BackgroundRequestClose();
            dialogBackground.Loaded += (_, _) =>
            {
                var v = ElementComposition.GetElementVisual(dialogBackground);
                CompositionAnimationHelper.MakeOpacityAnimated(v, 400);
            }; 
        }
    }

    private void BackgroundRequestClose()
    {
        if (Dialog is not IKanaDialog { CanDismissWithBackgroundClick: true } KanaDialog) return;
        if (!KanaDialog.CanDismissWithBackgroundClick) return;
        Manager.TryDismissDialog(KanaDialog);
    }

    private static void OnManagerPropertyChanged(AvaloniaObject sender,
                                                 AvaloniaPropertyChangedEventArgs propChanged)
    {
        if (sender is not KanaDialogHost host)
            throw new NullReferenceException("Dependency object is not of valid type " + nameof(KanaDialogHost));
        if (propChanged.OldValue is IKanaDialogManager oldManager)
            host.DetachManagerEvents(oldManager);
        if (propChanged.NewValue is IKanaDialogManager newManager)
            host.AttachManagerEvents(newManager);
    }

    private void AttachManagerEvents(IKanaDialogManager newManager)
    {
        newManager.OnDialogShown += ManagerOnDialogShown;
        newManager.OnDialogDismissed += ManagerOnDialogDismissed;
    }
        
    private void DetachManagerEvents(IKanaDialogManager oldManager)
    {
        oldManager.OnDialogShown -= ManagerOnDialogShown;
        oldManager.OnDialogDismissed -= ManagerOnDialogDismissed;
    }

    private void ManagerOnDialogShown(object sender, KanaDialogManagerEventArgs args)
    {
        Dialog = args.Dialog;
        IsDialogOpen = true;
    }
        
    private void ManagerOnDialogDismissed(object sender, KanaDialogManagerEventArgs args)
    {
        IsDialogOpen = false;
        Task.Delay(500).ContinueWith(_ =>
        {
            if (Dialog != args.Dialog) return;
            Dialog = null;
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    static KanaDialogHost()
    {
        ManagerProperty.Changed.Subscribe(
            new Avalonia.Reactive.AnonymousObserver<AvaloniaPropertyChangedEventArgs<IKanaDialogManager>>(x =>
                OnManagerPropertyChanged(x.Sender, x)));
    }
}

public class KanaDialogManagerEventArgs : EventArgs
{
    public IKanaDialog Dialog { get; set; }

    public KanaDialogManagerEventArgs(IKanaDialog dialog)
    {
        Dialog = dialog;
    }
}
    
public delegate void KanaDialogManagerEventHandler(object sender, KanaDialogManagerEventArgs args);