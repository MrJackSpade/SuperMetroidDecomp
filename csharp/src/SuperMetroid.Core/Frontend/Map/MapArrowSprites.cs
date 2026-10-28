namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// Selects the map scroll arrow's sprite (<see cref="MapArrowDefinitions"/>) for each scroll direction.
/// </summary>
public static class MapArrowSprites
{
    /// <summary>$82:C1E4 base-ID variants resolve to menu shapes five/four/six/seven for these arrows.</summary>
    public static MapSpriteId SpriteBase(MapScrollDirection direction) =>
        Ensure.Listed(MapArrowDefinitions.SpriteBases, direction);
}
