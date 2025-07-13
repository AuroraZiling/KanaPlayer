using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace KanaPlayer.Controls.Hosts
{
    public class KanaToastHost : ItemsControl
    {
        public static readonly StyledProperty<IKanaToastManager> ManagerProperty =
            AvaloniaProperty.Register<KanaToastHost, IKanaToastManager>(nameof(Manager));

        public IKanaToastManager Manager
        {
            get => GetValue(ManagerProperty);
            set => SetValue(ManagerProperty, value);
        }

        public static readonly StyledProperty<byte> MaxToastsProperty = AvaloniaProperty.Register<KanaToastHost, byte>(nameof(MaxToasts), defaultValue: 5);

        public byte MaxToasts
        {
            get => GetValue(MaxToastsProperty);
            set => SetValue(MaxToastsProperty, value);
        }

        public static readonly StyledProperty<ToastLocation> PositionProperty = AvaloniaProperty.Register<KanaToastHost, ToastLocation>(nameof(Position), defaultValue: ToastLocation.BottomRight);

        public ToastLocation Position
        {
            get => GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            OnPositionChanged(Position);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == PositionProperty && change.NewValue is ToastLocation loc)
                OnPositionChanged(loc);
        }

        private void OnPositionChanged(ToastLocation newLoc)
        {
            HorizontalAlignment = newLoc switch
            {
                ToastLocation.BottomRight => HorizontalAlignment.Right,
                ToastLocation.BottomLeft => HorizontalAlignment.Left,
                ToastLocation.TopRight => HorizontalAlignment.Right,
                ToastLocation.TopLeft => HorizontalAlignment.Left,
                _ => throw new ArgumentOutOfRangeException()
            };
            VerticalAlignment = newLoc switch
            {
                ToastLocation.BottomRight => VerticalAlignment.Bottom,
                ToastLocation.BottomLeft => VerticalAlignment.Bottom,
                ToastLocation.TopRight => VerticalAlignment.Top,
                ToastLocation.TopLeft => VerticalAlignment.Top,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static void OnManagerPropertyChanged(AvaloniaObject sender,
            AvaloniaPropertyChangedEventArgs propChanged)
        {
            if (sender is not KanaToastHost host)
                throw new NullReferenceException("Dependency object is not of valid type " + nameof(KanaToastHost));
            if (propChanged.OldValue is IKanaToastManager oldManager)
                host.DetachManagerEvents(oldManager);
            if (propChanged.NewValue is IKanaToastManager newManager)
                host.AttachManagerEvents(newManager);
        }

        private void AttachManagerEvents(IKanaToastManager newManager)
        {
            newManager.OnToastQueued += ManagerOnToastQueued;
            newManager.OnToastDismissed += ManagerOnToastDismissed;
            newManager.OnAllToastsDismissed += ManagerOnAllToastsDismissed;
        }

        private void DetachManagerEvents(IKanaToastManager oldManager)
        {
            oldManager.OnToastQueued -= ManagerOnToastQueued;
            oldManager.OnToastDismissed -= ManagerOnToastDismissed;
            oldManager.OnAllToastsDismissed -= ManagerOnAllToastsDismissed;
        }

        private void ManagerOnToastDismissed(object sender, KanaToastDismissedEventArgs args) =>
            ClearToast(args.Toast);

        private void ManagerOnAllToastsDismissed(object sender, EventArgs e)
        {
            foreach(var toast in Items)
                ClearToast((IKanaToast)toast!);
        }

        private void ManagerOnToastQueued(object sender, KanaToastQueuedEventArgs args)
        {
            if (MaxToasts <= 0) return;
            var toast = args.Toast;
            Items.Add(args.Toast);
            Manager.EnsureMaximum(MaxToasts);
            toast.AnimateShow();
        }

        private void ClearToast(IKanaToast toast)
        {
            if (Manager.IsDismissed(toast)) return;
            toast.AnimateDismiss();
            Task.Delay(300).ContinueWith(_ =>
            {
                Items.Remove(toast);
                ToastPool.Return((KanaToast)toast);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        static KanaToastHost()
        {
            ManagerProperty.Changed.Subscribe(
                new Avalonia.Reactive.AnonymousObserver<AvaloniaPropertyChangedEventArgs<IKanaToastManager>>(x =>
                    OnManagerPropertyChanged(x.Sender, x)));
        }
    }
}