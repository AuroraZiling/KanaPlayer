using System.Diagnostics;

namespace KanaPlayer.Core.Extensions;

public static class DebugExtension
{
	[Conditional("DEBUG")]
	public static void Debug<T>(this T target, Action<T>? peek = null)
	{
		Debugger.Break();
		peek?.Invoke(target);
	}

	[Conditional("DEBUG")]
	public static void DebugWriteLineWithDateTime<T>(this T target)
	{
		System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {target}");
	}
}