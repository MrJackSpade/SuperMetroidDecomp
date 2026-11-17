namespace SuperMetroid.Core.Frontend.Map;

/// <summary>The moments of the map highlight cycle its owner acts on (#2359).</summary>
internal enum MapHighlightCue
{
    /// <summary>The cycle passed its last step and starts again: the map plays its highlight-loop sound.</summary>
    Wrapped,
}
