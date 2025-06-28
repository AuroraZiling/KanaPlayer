using Avalonia.Threading;

namespace KanaPlayer.Core.Extensions;

public static class AvaloniaExtension
{
    public static void InvokeOnDemand(this Dispatcher dispatcher, Action action)
    {
        if (dispatcher.CheckAccess()) action();
        else dispatcher.Invoke(action);
    }

    public static void InvokeOnDemand(this Dispatcher dispatcher, Action action, in DispatcherPriority priority)
    {
        if (dispatcher.CheckAccess()) action();
        else dispatcher.Invoke(action, priority);
    }

    public static void InvokeOnDemand(
        this Dispatcher dispatcher,
        Action action,
        in DispatcherPriority priority,
        in CancellationToken cancellationToken)
    {
        if (dispatcher.CheckAccess()) action();
        else dispatcher.Invoke(action, priority, cancellationToken);
    }

    public static void InvokeOnDemand(
        this Dispatcher dispatcher,
        Action action,
        in DispatcherPriority priority,
        in CancellationToken cancellationToken,
        in TimeSpan timeout)
    {
        if (dispatcher.CheckAccess()) action();
        else dispatcher.Invoke(action, priority, cancellationToken, timeout);
    }

    public static T InvokeOnDemand<T>(this Dispatcher dispatcher, Func<T> func)
    {
        return dispatcher.CheckAccess() ? func() : dispatcher.Invoke(func);
    }

    public static T InvokeOnDemand<T>(
        this Dispatcher dispatcher,
        Func<T> func,
        in DispatcherPriority priority)
    {
        return dispatcher.CheckAccess() ? func() : dispatcher.Invoke(func, priority);
    }

    public static T InvokeOnDemand<T>(
        this Dispatcher dispatcher,
        Func<T> func,
        in DispatcherPriority priority,
        in CancellationToken cancellationToken)
    {
        return dispatcher.CheckAccess() ? func() : dispatcher.Invoke(func, priority, cancellationToken);
    }

    public static T InvokeOnDemand<T>(
        this Dispatcher dispatcher,
        Func<T> func,
        in DispatcherPriority priority,
        in CancellationToken cancellationToken,
        in TimeSpan timeout)
    {
        return dispatcher.CheckAccess() ? func() : dispatcher.Invoke(func, priority, cancellationToken, timeout);
    }

    public static Task InvokeOnDemandAsync(this Dispatcher dispatcher, Action action)
    {
        if (!dispatcher.CheckAccess()) return dispatcher.InvokeAsync(action).GetTask();
        action();
        return Task.CompletedTask;
    }

    public static Task InvokeOnDemandAsync(
        this Dispatcher dispatcher,
        Action action,
        in DispatcherPriority priority)
    {
        if (!dispatcher.CheckAccess()) return dispatcher.InvokeAsync(action, priority).GetTask();
        action();
        return Task.CompletedTask;
    }

    public static Task InvokeOnDemandAsync(
        this Dispatcher dispatcher,
        Action action,
        in DispatcherPriority priority,
        in CancellationToken cancellationToken)
    {
        if (!dispatcher.CheckAccess()) return dispatcher.InvokeAsync(action, priority, cancellationToken).GetTask();
        action();
        return Task.CompletedTask;
    }

    public static Task<T> InvokeOnDemandAsync<T>(this Dispatcher dispatcher, Func<T> func)
    {
        return dispatcher.CheckAccess() ? Task.FromResult(func()) : dispatcher.InvokeAsync(func).GetTask();
    }

    public static Task<T> InvokeOnDemandAsync<T>(
        this Dispatcher dispatcher,
        Func<T> func,
        in DispatcherPriority priority)
    {
        return dispatcher.CheckAccess() ? Task.FromResult(func()) : dispatcher.InvokeAsync(func, priority).GetTask();
    }

    public static Task<T> InvokeOnDemandAsync<T>(
        this Dispatcher dispatcher,
        Func<T> func,
        in DispatcherPriority priority,
        in CancellationToken cancellationToken)
    {
        return dispatcher.CheckAccess() ? Task.FromResult(func()) : dispatcher.InvokeAsync(func, priority, cancellationToken).GetTask();
    }
}