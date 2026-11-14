namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// Selects the map scroll arrow's sprite (<see cref="MapArrowDefinitions"/>) for each scroll direction.
/// </summary>
public static class MapArrowSprites
{
    /// <summary>The arrow picture shown for <paramref name="direction"/>.</summary>
    public static MapSpriteId SpriteBase(MapScrollDirection direction) =>
        Ensure.Listed(MapArrowDefinitions.SpriteBases, direction);
}
