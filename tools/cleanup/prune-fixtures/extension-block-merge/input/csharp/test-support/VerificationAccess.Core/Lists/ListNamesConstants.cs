namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Layout names checks compare with.</summary>
internal static class ListNamesConstants
{
    /// <summary>The name checks expect of a layout.</summary>
    extension(ListLayout layout)
    {
        /// <summary>The length of the layout's name.</summary>
        internal int NameLength => layout.Name.Length;
    }
}
