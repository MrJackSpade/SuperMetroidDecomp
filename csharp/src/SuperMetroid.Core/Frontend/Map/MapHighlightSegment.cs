namespace SuperMetroid.Core.Frontend.Map;

/// <summary>The segments the map highlight cycle plays (#2359).</summary>
internal enum MapHighlightSegment
{
    /// <summary>The installed cycle's steps, each shown for its installed hold.</summary>
    Steps,
    /// <summary>The cycle starting again from its first step, which requests the map's highlight-loop sound.</summary>
    Wrap,
}
