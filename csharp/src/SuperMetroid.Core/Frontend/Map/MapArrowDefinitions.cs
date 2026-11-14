namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Named bindings between map directions and their fixed menu arrow shapes.</summary>
public static class MapArrowDefinitions
{
    /// <summary>Four visual/control records, ordered left/right/up/down.</summary>
    public const int Count = 4;
    /// <summary>Each scroll direction's arrow picture.</summary>
    internal static readonly IReadOnlyDictionary<MapScrollDirection, MapSpriteId> SpriteBases =
        new Dictionary<MapScrollDirection, MapSpriteId>
        {
            [MapScrollDirection.Left] = MapSpriteId.ArrowLeft,
            [MapScrollDirection.Right] = MapSpriteId.ArrowRight,
            [MapScrollDirection.Up] = MapSpriteId.ArrowUp,
            [MapScrollDirection.Down] = MapSpriteId.ArrowDown,
        };
}
