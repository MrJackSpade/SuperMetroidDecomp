namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Cartridge values and helpers of <see cref="ListMarks"/> that only verification reads.</summary>
internal static class ListMarksConstants
{
    /// <summary>The marks of a layout that only checks compare.</summary>
    extension(ListLayout layout)
    {
        /// <summary>Twice the layout's mark.</summary>
        internal int DoubleMark => layout.Mark * 2;

        /// <summary>Three times the layout's mark.</summary>
        internal int TripleMark => layout.Mark * 3;
    }
}
