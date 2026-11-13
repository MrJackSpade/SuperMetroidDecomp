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

    /// <summary>The marks of a layout that only checks compare.</summary>
    extension(ListLayout layout)
    {
        /// <summary>Twice the layout's mark.</summary>
        internal int DoubleMark => layout.Mark * 2;

        /// <summary>Three times the layout's mark.</summary>
        internal int TripleMark => layout.Mark * 3;
    }
}
