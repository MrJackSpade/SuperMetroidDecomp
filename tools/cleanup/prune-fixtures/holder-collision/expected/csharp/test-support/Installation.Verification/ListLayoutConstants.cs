namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>The layout the checks read: an instance class, which static members cannot join.</summary>
internal sealed class ListLayoutConstants
{
    /// <summary>Two lists, each with the word just before it.</summary>
    internal ListLayout Layout { get; } = new(ListEntriesConstants.Entries, [2, 10]);
}
