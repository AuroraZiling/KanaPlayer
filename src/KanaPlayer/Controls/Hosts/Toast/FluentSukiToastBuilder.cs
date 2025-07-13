using System;
using Avalonia.Controls.Notifications;

namespace KanaPlayer.Controls.Hosts;

public static class FluentKanaToastBuilder
{
    #region Content

    /// <summary>
    /// Creates a <see cref="KanaToastBuilder"/> instance that can be used to construct a <see cref="IKanaToast"/>
    /// </summary>
    public static KanaToastBuilder CreateToast(this IKanaToastManager manager) => new(manager);

    /// <summary>
    /// Creates a simple informational toast that can be dismissed by clicking and is otherwise dismissed after 3 seconds.
    /// This doesn't set the title/content and you should use <see cref="WithTitle"/> and <see cref="WithContent"/>
    /// </summary>
    public static KanaToastBuilder CreateSimpleInfoToast(this IKanaToastManager manager)
    {
        return new KanaToastBuilder(manager)
            .OfType(NotificationType.Information)
            .Dismiss().After(TimeSpan.FromSeconds(3))
            .Dismiss().ByClicking();
    }

    /// <summary>
    /// Gives the toast a title.
    /// </summary>
    public static KanaToastBuilder WithTitle(this KanaToastBuilder builder, string title)
    {
        builder.SetTitle(title);
        return builder;
    }


    /// <summary>
    /// Show a loading Toast.
    /// </summary>
    public static KanaToastBuilder WithLoadingState(this KanaToastBuilder builder, bool state)
    {
        builder.SetLoadingState(state);
        return builder;
    }

    /// <summary>
    /// Gives the toast some content. This can be a ViewModel if desired - View will be located via the default location strategy.
    /// </summary>
    public static KanaToastBuilder WithContent(this KanaToastBuilder builder, object? content)
    {
        builder.SetContent(content);
        return builder;
    }

    /// <summary>
    /// Sets the notification type - By default it is <see cref="NotificationType.Information"/>
    /// </summary>
    public static KanaToastBuilder OfType(this KanaToastBuilder builder, NotificationType type)
    {
        builder.SetType(type);
        return builder;
    }

    #endregion

    #region Dismissing

    /// <summary>
    /// Begins a dismiss statement for the toast - Follow this with something like <see cref="After"/>.
    /// </summary>
    public static KanaToastBuilder.DismissToast Dismiss(this KanaToastBuilder builder) => new(builder);

    /// <summary>
    /// Automatically dismisses the toast after the given amount of time.
    /// </summary>
    public static KanaToastBuilder After(this KanaToastBuilder.DismissToast dismiss, TimeSpan after, bool interruptWhileHover = true)
    {
        dismiss.Builder.SetDismissAfter(after, interruptWhileHover);
        return dismiss.Builder;
    }

    /// <summary>
    /// Allows the toast to be dismissed by clicking anywhere on the toast.
    /// </summary>
    public static KanaToastBuilder ByClicking(this KanaToastBuilder.DismissToast dismiss)
    {
        dismiss.Builder.SetCanDismissByClicking(true);
        return dismiss.Builder;
    }

    #endregion

    #region Actions

    /// <summary>
    /// The action provided will be called if the body of the toast is clicked.
    /// </summary>
    public static KanaToastBuilder OnClicked(this KanaToastBuilder builder, Action<IKanaToast> action)
    {
        builder.SetOnClicked(action);
        return builder;
    }

    /// <summary>
    /// The action provided will be called when the toast is dismissed for any reason, including clicking.
    /// </summary>
    public static KanaToastBuilder OnDismissed(this KanaToastBuilder builder, Action<IKanaToast, KanaToastDismissSource> onDismissAction)
    {
        builder.SetOnDismiss(onDismissAction);
        return builder;
    }

    /// <summary>
    /// Adds an action button to the toast which will call the provided callback on click. Any number of buttons can be added to a toast.
    /// </summary>
    public static KanaToastBuilder WithActionButton(this KanaToastBuilder builder, object buttonContent, Action<IKanaToast> onClicked, bool dismissOnClick = false)
    {
        builder.AddActionButton(buttonContent, onClicked, dismissOnClick);
        return builder;
    }

    #endregion
}