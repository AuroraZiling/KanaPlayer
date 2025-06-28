using System.Runtime.CompilerServices;

namespace KanaPlayer.Core.Interfaces;

public interface IExceptionHandler
{
    void HandleException(Exception exception, string? message = null, [CallerMemberName] object? source = null);

    public static IExceptionHandler DangerouslyIgnoreAllException { get; } = new AnonymousExceptionHandler(static (_, _, _) => { });
}

public class AnonymousExceptionHandler(Action<Exception, string?, object?> handler) : IExceptionHandler
{
    public void HandleException(Exception exception, string? message = null, object? source = null)
    {
        handler.Invoke(exception, message, source);
    }
}