namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Counts of list layouts and the signals that follow them.</summary>
internal static partial class ListPings
{
    /// <summary>The number of lists production lays out.</summary>
    internal static int Count(ListLayout layout) => layout.Count;
}

/// <summary>A meter over a list.</summary>
internal sealed partial class ListMeter
{
    /// <summary>The meter's level.</summary>
    internal int Level => 3;
}
