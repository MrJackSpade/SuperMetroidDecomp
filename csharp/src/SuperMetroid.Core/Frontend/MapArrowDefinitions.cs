namespace SuperMetroid.Core.Frontend;

/// <summary>Named bindings between map directions and their fixed menu arrow shapes.</summary>
public static class MapArrowDefinitions
{
    /// <summary>$81:AF32 four visual/control records, ordered left/right/up/down.</summary>
    public const int Count = 4;
    /// <summary>$82:C1E4 base-ID variants resolve to menu shapes five/four/six/seven for these arrows.</summary>
    public static MapSpriteId SpriteBase(MapScrollDirection direction) => direction switch
    {
        MapScrollDirection.Left => MapSpriteId.ArrowLeft, MapScrollDirection.Right => MapSpriteId.ArrowRight,
        MapScrollDirection.Up => MapSpriteId.ArrowUp, MapScrollDirection.Down => MapSpriteId.ArrowDown,
        _ => throw new ArgumentOutOfRangeException(nameof(direction))
    };
}
