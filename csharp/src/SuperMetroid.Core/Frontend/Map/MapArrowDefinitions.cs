namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Named bindings between map directions and their fixed menu arrow shapes.</summary>
public static class MapArrowDefinitions
{
    /// <summary>$81:AF32 four visual/control records, ordered left/right/up/down.</summary>
    public const int Count = 4;
    /// <summary>$82:C1E4 base-ID variants resolve to menu shapes five/four/six/seven for these arrows.</summary>
    internal static readonly IReadOnlyDictionary<MapScrollDirection, MapSpriteId> SpriteBases =
        new Dictionary<MapScrollDirection, MapSpriteId>
        {
            [MapScrollDirection.Left] = MapSpriteId.ArrowLeft,
            [MapScrollDirection.Right] = MapSpriteId.ArrowRight,
            [MapScrollDirection.Up] = MapSpriteId.ArrowUp,
            [MapScrollDirection.Down] = MapSpriteId.ArrowDown,
        };
}
