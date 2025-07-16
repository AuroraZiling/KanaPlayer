using System;
using System.Threading.Tasks;

namespace KanaPlayer.Controls.Hosts;

public static class FluentKanaDialogBuilder
{
    #region Content

    public static KanaDialogBuilder CreateDialog(this IKanaDialogManager dialogManager)
        => new(dialogManager);
    
    public static KanaDialogBuilder WithTitle(this KanaDialogBuilder builder, string title)
    {
        builder.SetTitle(title);
        return builder;
    }

    public static KanaDialogBuilder WithContent(this KanaDialogBuilder builder, object content)
    {
        builder.SetContent(content);
        return builder;
    }

    public static KanaDialogBuilder ShowCardBackground(this KanaDialogBuilder builder, bool show)
    {
        builder.SetShowCardBackground(show);
        return builder;
    }
    
    public static KanaDialogBuilder WithView(this KanaDialogBuilder builder, object view)
    {
        builder.SetView(view);
        return builder;
    }

    public static KanaDialogBuilder WithViewModel(this KanaDialogBuilder builder, Func<IKanaDialog, object> viewModel)
    {
        builder.SetViewModel(viewModel);
        return builder;
    }

    #endregion Content

    #region Dismissing

    public static KanaDialogBuilder.DismissDialog Dismiss(this KanaDialogBuilder builder) => new(builder);

    public static KanaDialogBuilder ByClickingBackground(this KanaDialogBuilder.DismissDialog dismissBuilder)
    {
        dismissBuilder.Builder.SetCanDismissWithBackgroundClick(true);
        return dismissBuilder.Builder;
    }

    #endregion Dismissing

    #region Actions

    public static KanaDialogBuilder WithActionButton(this KanaDialogBuilder builder, object? content, Action<IKanaDialog> onClicked,
                                                     bool dismissOnClick = false, params string[] classes)
    {
        builder.AddActionButton(content, onClicked, dismissOnClick, classes);
        return builder;
    }

    public static KanaDialogBuilder OnDismissed(this KanaDialogBuilder builder, Action<IKanaDialog> onDismissed)
    {
        builder.SetOnDismissed(onDismissed);
        return builder;
    }

    public static KanaDialogBuilder WithYesNoResult(this KanaDialogBuilder builder, object? yesButtonContent, object? noButtonContent, params string[] classes)
    {
        builder.Completion = new TaskCompletionSource<bool>();

        builder.AddActionButton(yesButtonContent, _ => builder.Completion.SetResult(true), true, classes);
        builder.AddActionButton(noButtonContent, _ => builder.Completion.SetResult(false), true, classes);

        return builder;
    }

    public static KanaDialogBuilder WithOkResult(this KanaDialogBuilder builder, object? okButtonContent, params string[] classes)
    {
        builder.Completion = new TaskCompletionSource<bool>();
        builder.AddActionButton(okButtonContent, _ => builder.Completion.SetResult(true), true, classes);
        return builder;
    }

    #endregion Actions
}