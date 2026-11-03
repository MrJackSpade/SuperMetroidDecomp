namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// The colours a map sprite's palette-inheriting parts draw in on the pause and file-select maps (#1512): what the map
/// shows each sprite as. Which of the screen's colours each one is, the import alone knows.
/// </summary>
public enum MapSpriteColouring
{
    /// <summary>An area, elevator or world title label the screen shows at full brightness.</summary>
    Labels,

    /// <summary>An area label of an area the file-select world map does not have selected.</summary>
    DimLabels,

    /// <summary>A map marker: a boss, a gunship, a station or the pause map's position indicator.</summary>
    Markers,

    /// <summary>The boss marker drawn under a defeated boss's cross.</summary>
    DefeatedBossMarker,

    /// <summary>A highlighted sprite, the map's scroll arrows, in the screen's own colours before the highlight cycle's first frame.</summary>
    Highlighted,
}
