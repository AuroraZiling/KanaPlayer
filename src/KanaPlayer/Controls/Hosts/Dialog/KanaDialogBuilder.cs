using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using KanaPlayer.Services.Theme;
using Lucide.Avalonia;

namespace KanaPlayer.Controls.Hosts;

public class KanaDialogBuilder
{
    private IKanaDialogManager Manager { get; }
    private IKanaDialog Dialog { get; }
    public TaskCompletionSource<bool>? Completion { get; set; }

    public KanaDialogBuilder(IKanaDialogManager manager)
    {
        Manager = manager;
        Dialog = DialogPool.Get();
        Dialog.Manager = Manager;
    }

    public bool TryShow()
        => Manager.TryShowDialog(Dialog);

    public async Task<bool> TryShowAsync(CancellationToken cancellationToken = default)
    {
        var completion = Completion;
        if (completion is null)
        {
            throw new InvalidOperationException($"{nameof(KanaDialogBuilder)} is not configured correctly. Its missing a valid value for {nameof(Completion)}.");
        }

        cancellationToken.Register(CancellationRequested);
        Dialog.OnDismissed += DialogCancellationRequested;

        var result = Manager.TryShowDialog(Dialog);
        if (!result)
        {
            Dialog.OnDismissed -= DialogCancellationRequested;
            throw new InvalidOperationException("Opening a new dialog failed. Looks like there is already one open.");
        }

        return await completion.Task;

        void CancellationRequested()
        {
            Manager.TryDismissDialog(Dialog);
            completion.TrySetResult(false);
        }

        void DialogCancellationRequested(IKanaDialog dialog)
        {
            dialog.OnDismissed -= DialogCancellationRequested;
            completion.TrySetResult(false);
        }
    }

    public void SetTitle(string title)
        => Dialog.Title = title;

    public void SetShowCardBackground(bool show)
        => Dialog.ShowCardBackground = show;

    public void SetContent(object content)
        => Dialog.Content = content;

    public void SetView(object view)
    {
        Dialog.Content = view;
    }

    public void SetViewModel(Func<IKanaDialog, object> viewModel)
    {
        Dialog.ViewModel = viewModel(Dialog);
        
        if(Dialog.Content is Control control)
            control.DataContext = Dialog.ViewModel;
    }

    public void SetCanDismissWithBackgroundClick(bool canDismissWithBackgroundClick)
        => Dialog.CanDismissWithBackgroundClick = canDismissWithBackgroundClick;

    public void SetOnDismissed(Action<IKanaDialog> onDismissed)
        => Dialog.OnDismissed = onDismissed;

    public void AddActionButton(object? buttonContent, Action<IKanaDialog> onClicked, bool dismissOnClick, string[] classes)
    {
        if (classes.Length == 0)
            classes = ["Flat"];

        var btn = new Button { Content = buttonContent };
        foreach (var @class in classes)
            btn.Classes.Add(@class);

        btn.Click += (_, _) =>
        {
            onClicked(Dialog);
            if (!dismissOnClick)
                return;
            Manager.TryDismissDialog(Dialog);
        };

        Dialog.ActionButtons.Add(btn);
    }

    public class DismissDialog(KanaDialogBuilder builder)
    {
        public KanaDialogBuilder Builder { get; } = builder;
    }
}