using System.Runtime.CompilerServices;
#if !NET5_0_OR_GREATER
using TaskCompletionSource = System.Threading.Tasks.TaskCompletionSource<object?>;
#endif

namespace KanaPlayer.Core.Extensions;

public static class AsyncExtension
{
    public static Task AsTask(this CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource();
        cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken), useSynchronizationContext: false);
        return tcs.Task;
    }

    public static TaskAwaiter GetAwaiter(this CancellationToken cancellationToken)
    {
        return cancellationToken.AsTask().GetAwaiter();
    }

    public static Task AsTask(this WaitHandle handle)
    {
        var tcs = new TaskCompletionSource();
        ThreadPool.RegisterWaitForSingleObject(
            handle,
#if NET5_0_OR_GREATER
            (_, _) => tcs.TrySetResult(),
#else
            (_, _) => tcs.TrySetResult(null),
#endif
            null,
            Timeout.Infinite,
            executeOnlyOnce: true);

        return tcs.Task;
    }

    public static TaskAwaiter GetAwaiter(this WaitHandle handle)
    {
        return handle.AsTask().GetAwaiter();
    }
}