using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// Names the map sprite compositions (<see cref="MapSpriteDefinitions"/>) and each Zebes area's world-map label.
/// </summary>
public static class MapSpriteNames
{
    /// <summary>Resolves the exact case-sensitive presentation key for a map sprite, preserving semantic roles such as <c>Indicator.Frame0</c> and <c>World.Crateria</c>.</summary>
    /// <param name="id">The map sprite.</param>
    /// <returns>The stable key used to validate, serialize, and bind the installed map-sprite composition.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The identity has no named map-presentation role.</exception>
    public static string Name(MapSpriteId id) =>
        Ensure.Listed(MapSpriteDefinitions.Names, id);

    /// <summary>Selects the world-map label naming <paramref name="area"/>.</summary>
    public static MapSpriteId WorldLabel(AreaId area) =>
        Ensure.Listed(MapSpriteDefinitions.WorldLabels, area);

    /// <summary>Enumerates named roles in the established presentation-document order.</summary>
    public static IEnumerable<MapSpriteFrameName> Frames =>
        MapSpriteDefinitions.FrameOrder.Select(id => new MapSpriteFrameName(id, Name(id)));

    /// <summary>The map sprite sheet's frames, one per named role.</summary>
    public static IEnumerable<string> SheetFrames => Frames.Select(frame => frame.Name);
}
