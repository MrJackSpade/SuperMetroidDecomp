namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Named bindings between map directions and their fixed menu arrow shapes.</summary>
public static partial class MapArrowDefinitions
{
    /// <summary>Each scroll direction's arrow picture.</summary>
    internal static readonly IdentityTable<MapScrollDirection, MapSpriteId> ArrowPictures =
        new(new Dictionary<MapScrollDirection, MapSpriteId>
        {
            [MapScrollDirection.Left] = MapSpriteId.ArrowLeft,
            [MapScrollDirection.Right] = MapSpriteId.ArrowRight,
            [MapScrollDirection.Up] = MapSpriteId.ArrowUp,
            [MapScrollDirection.Down] = MapSpriteId.ArrowDown,
        });
}
