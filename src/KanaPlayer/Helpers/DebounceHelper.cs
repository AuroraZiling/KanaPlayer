using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace KanaPlayer.Helpers;

/// <summary>
/// Efficient debounce helper
/// </summary>
/// <param name="time">Debounce delay time</param>
public sealed class DebounceHelper(TimeSpan time) : IDisposable
{
    private readonly Lock _lockObj = new();
    private volatile CancellationTokenSource? _cancellationTokenSource;
    private volatile bool _isDisposed;
    private volatile Task? _debounceTask;

    /// <summary>
    /// Attempts to execute an operation. If called again within the specified time, cancels the previous operation
    /// </summary>
    /// <param name="action">The operation to execute</param>
    /// <returns>Returns true if the operation is scheduled for execution, false if the object is disposed</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Execute(Action action)
    {
        if (_isDisposed) return;

        ArgumentNullException.ThrowIfNull(action);

        lock (_lockObj)
        {
            if (_isDisposed) return;

            // Cancel the previous operation
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            // Create a new cancellation token
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            // Use Task.Delay for efficient asynchronous delay
            _debounceTask = Task.Delay(time, token).ContinueWith(
                static (task, state) =>
                {
                    if (!task.IsCanceled && state is Action actionToExecute)
                    {
                        actionToExecute();
                    }
                },
                action,
                token,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Attempts to execute an operation. Returns false if currently executing or if less than <see cref="time"/> has passed since the last execution
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <returns>True if the action was scheduled, false otherwise</returns>
    public bool TryExecute(Action action)
    {
        if (_isDisposed) return false;

        ArgumentNullException.ThrowIfNull(action);

        lock (_lockObj)
        {
            if (_isDisposed) return false;

            // If currently executing or less than time has passed since the last execution, return false
            if (_debounceTask is not null && !_debounceTask.IsCompleted)
            {
                return false;
            }

            // Execute the operation
            Execute(action);
            return true;
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        lock (_lockObj)
        {
            if (_isDisposed) return;

            _isDisposed = true;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}