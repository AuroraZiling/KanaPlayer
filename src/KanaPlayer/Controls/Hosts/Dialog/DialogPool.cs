using System.Collections.Concurrent;
using System.Collections.Generic;

namespace KanaPlayer.Controls.Hosts;

internal static class DialogPool
{
    private static readonly ConcurrentStack<IKanaDialog> Pool = new();

    static DialogPool()
    {
        Pool.Push(new KanaDialog());
        Pool.Push(new KanaDialog());
    }

    internal static IKanaDialog Get()
    {
        var dialog = Pool.TryPop(out var item) ? item : new KanaDialog();
        dialog.ResetToDefault();
        return dialog;
    }

    internal static void Return(IKanaDialog dialog)
        => Pool.Push(dialog);

    internal static void Return(IEnumerable<IKanaDialog> dialogs)
    {
        foreach (var dialog in dialogs)
            Pool.Push(dialog);
    }
}