namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>The names of list layouts.</summary>
internal static class ListNames
{
    /// <summary>The title production prints for <paramref name="layout"/>.</summary>
    internal static string Title(ListLayout layout) => $"{layout.Count} lists";

    /// <summary>The name of a <see cref="ListLayout"/> that only checks print.</summary>
    extension(ListLayout layout)
    {
        /// <summary>The layout's title with its first entry.</summary>
        internal string Name => $"{Title(layout)} from {layout.Entry(0)}";
    }
}
