using System.Collections.Concurrent;
using System.Collections.Generic;

namespace KanaPlayer.Controls.Hosts;

internal static class ToastPool
{
    private static readonly ConcurrentBag<IKanaToast> Pool = new();

    internal static IKanaToast Get()
    {
        var toast = Pool.TryTake(out var item) ? item : new KanaToast();
        return toast.ResetToDefault();
    }

    internal static void Return(IKanaToast toast) => Pool.Add(toast);

    internal static void Return(IEnumerable<IKanaToast> toasts)
    {
        foreach (var toast in toasts)
            Pool.Add(toast);
    }
}