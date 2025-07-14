using System;
using Avalonia.Controls.Notifications;

namespace KanaPlayer.Controls.Hosts;

public static class FluentKanaToastBuilder
{
    #region Content
    
    public static KanaToastBuilder CreateToast(this IKanaToastManager manager) => new(manager);

    public static KanaToastBuilder CreateSimpleInfoToast(this IKanaToastManager manager)
    {
        return new KanaToastBuilder(manager)
            .OfType(NotificationType.Information)
            .Dismiss().After(TimeSpan.FromSeconds(3))
            .Dismiss().ByClicking();
    }

    public static KanaToastBuilder WithTitle(this KanaToastBuilder builder, string title)
    {
        builder.SetTitle(title);
        return builder;
    }

    public static KanaToastBuilder WithLoadingState(this KanaToastBuilder builder, bool state)
    {
        builder.SetLoadingState(state);
        return builder;
    }

    public static KanaToastBuilder WithContent(this KanaToastBuilder builder, object? content)
    {
        builder.SetContent(content);
        return builder;
    }

    public static KanaToastBuilder OfType(this KanaToastBuilder builder, NotificationType type)
    {
        builder.SetType(type);
        return builder;
    }

    #endregion

    #region Dismissing

    public static KanaToastBuilder.DismissToast Dismiss(this KanaToastBuilder builder) => new(builder);

    public static KanaToastBuilder After(this KanaToastBuilder.DismissToast dismiss, TimeSpan after, bool interruptWhileHover = true)
    {
        dismiss.Builder.SetDismissAfter(after, interruptWhileHover);
        return dismiss.Builder;
    }
    
    public static KanaToastBuilder ByClicking(this KanaToastBuilder.DismissToast dismiss)
    {
        dismiss.Builder.SetCanDismissByClicking(true);
        return dismiss.Builder;
    }

    #endregion

    #region Actions

    public static KanaToastBuilder OnClicked(this KanaToastBuilder builder, Action<IKanaToast> action)
    {
        builder.SetOnClicked(action);
        return builder;
    }

    public static KanaToastBuilder OnDismissed(this KanaToastBuilder builder, Action<IKanaToast, KanaToastDismissSource> onDismissAction)
    {
        builder.SetOnDismiss(onDismissAction);
        return builder;
    }

    public static KanaToastBuilder WithActionButton(this KanaToastBuilder builder, object buttonContent, Action<IKanaToast> onClicked, bool dismissOnClick = false)
    {
        builder.AddActionButton(buttonContent, onClicked, dismissOnClick);
        return builder;
    }

    #endregion
}