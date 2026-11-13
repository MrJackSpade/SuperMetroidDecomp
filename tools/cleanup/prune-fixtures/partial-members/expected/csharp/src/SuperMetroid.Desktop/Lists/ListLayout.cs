namespace SuperMetroid.Desktop.Lists;

/// <summary>A run of lists, each with its entry.</summary>
/// <param name="entries">Each list's entry.</param>
internal sealed class ListLayout(int[] entries)
{
    /// <summary>The number of lists.</summary>
    internal int Count => entries.Length;
}
