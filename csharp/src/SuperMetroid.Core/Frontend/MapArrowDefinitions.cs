namespace SuperMetroid.Core.Frontend;

/// <summary>Named bindings between map directions and their fixed menu arrow shapes.</summary>
public static class MapArrowDefinitions
{
    /// <summary>$81:AF32 four visual/control records, ordered left/right/up/down.</summary>
    public const int Count = 4;
    /// <summary>$82:C1E4 base-ID variants resolve to menu shapes five/four/six/seven for these arrows.</summary>
    public static ushort SpriteBase(MapScrollDirection direction) => direction switch
    {
        MapScrollDirection.Left => 5, MapScrollDirection.Right => 4,
        MapScrollDirection.Up => 6, MapScrollDirection.Down => 7,
        _ => throw new ArgumentOutOfRangeException(nameof(direction))
    };
}
