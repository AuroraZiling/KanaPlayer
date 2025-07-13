using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace KanaPlayer.Controls.Hosts
{
    public class KanaDialog : TemplatedControl, IKanaDialog
    {
        public static readonly StyledProperty<object?> ViewModelProperty = AvaloniaProperty.Register<KanaDialog, object?>(nameof(ViewModel));

        public object? ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly StyledProperty<string?> TitleProperty = AvaloniaProperty.Register<KanaDialog, string?>(nameof(Title));

        public string? Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<KanaDialog, object?>(nameof(Content));

        public object? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly StyledProperty<bool> IsViewModelOnlyProperty = AvaloniaProperty.Register<KanaDialog, bool>(nameof(IsViewModelOnly));

        public bool IsViewModelOnly
        {
            get => GetValue(IsViewModelOnlyProperty);
            set => SetValue(IsViewModelOnlyProperty, value);
        }

        public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<KanaDialog, object?>(nameof(Icon));

        public object? Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly StyledProperty<bool> ShowCardBackgroundProperty = AvaloniaProperty.Register<KanaDialog, bool>(nameof(ShowCardBackground), defaultValue: true);

        public bool ShowCardBackground
        {
            get => GetValue(ShowCardBackgroundProperty);
            set => SetValue(ShowCardBackgroundProperty, value);
        }
        
        public static readonly StyledProperty<IBrush?> IconColorProperty = AvaloniaProperty.Register<KanaDialog, IBrush?>(nameof(IconColor));

        public IBrush? IconColor
        {
            get => GetValue(IconColorProperty);
            set => SetValue(IconColorProperty, value);
        }

        public static readonly StyledProperty<ObservableCollection<object>> ActionButtonsProperty = AvaloniaProperty.Register<KanaDialog, ObservableCollection<object>>(nameof(ActionButtons));

        public ObservableCollection<object> ActionButtons
        {
            get => GetValue(ActionButtonsProperty);
            set => SetValue(ActionButtonsProperty, value);
        }
        
        public IKanaDialogManager? Manager { get; set; }
        
        public Action<IKanaDialog>? OnDismissed { get; set; }

        public bool CanDismissWithBackgroundClick { get; set; }
        
        public KanaDialog()
        {
            ActionButtons = [];
        }
        
        public void Dismiss() => Manager?.TryDismissDialog(this);

        public void ResetToDefault()
        {
            ActionButtons.Clear();
            ViewModel = null;
            Title = null;
            Content = null;
            IsViewModelOnly = false;
        }
    }
}