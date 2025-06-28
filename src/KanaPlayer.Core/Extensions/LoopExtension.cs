namespace KanaPlayer.Core.Extensions;

public static class LoopExtension
{
	public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
	{
		foreach (var item in source)
		{
			action(item);
		}
	}

	public static void Reset<T>(this ICollection<T> source, IEnumerable<T> data)
	{
		source.Clear();

		foreach (var item in data)
		{
			source.Add(item);
		}
	}

	public static ICollection<T> AddRange<T>(this ICollection<T> source, IEnumerable<T> data)
	{
		foreach (var item in data)
		{
			source.Add(item);
		}

		return source;
	}

	public static void RemoveWhere<T>(this IList<T> source, Predicate<T> predicate)
	{
		for (var i = 0; i < source.Count; i++) 
		{
			if (predicate(source[i])) 
			{
				source.RemoveAt(i);
				i--;
			}
		}
	}

	public static T[] Fill<T>(this T[] array, Func<int,T> element, int count = 0, Action? later = null)
	{
		count = count == 0 ? array.Length : count;
		for (var i = 0; i < count; i++)
		{
			array[i] = element(i);
			later?.Invoke();
		}

		return array;
	}
	
	public static T[,] Fill<T>(this T[,] array, Func<T> element, Action? later = null)
	{
		for (var r = 0; r < array.GetLongLength(0); r++)
		{
			for (var c = 0; c < array.GetLongLength(1); c++)
			{
				array[r,c] = element();
				later?.Invoke();
			}
		}

		return array;
	}
}