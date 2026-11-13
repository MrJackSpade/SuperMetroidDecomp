namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>The marks of list layouts.</summary>
internal static class ListMarks
{
    /// <summary>The mark production prints for a layout.</summary>
    extension(ListLayout layout)
    {
        /// <summary>The layout's mark: its list count.</summary>
        internal int Mark => layout.Count + 1;
    }
}
