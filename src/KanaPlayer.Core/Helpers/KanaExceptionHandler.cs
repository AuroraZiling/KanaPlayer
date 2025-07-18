using KanaPlayer.Core.Interfaces;
using NLog;

namespace KanaPlayer.Core.Helpers;

public class KanaExceptionHandler : IExceptionHandler
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(KanaExceptionHandler));
    public void HandleException(Exception exception, string? message = null, object? source = null)
        => ScopedLogger.Error(exception, "{Message}，源：{Source}", message ?? "发生了一个异常", source);
}