namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>The names of list layouts.</summary>
internal static class ListNames
{
    /// <summary>The title production prints for <paramref name="layout"/>.</summary>
    internal static string Title(ListLayout layout) => $"{layout.Count} lists";
}
