namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Axis predicates over <see cref="DoorDirection"/>.</summary>
public static class DoorDirections
{
    /// <summary>
    /// True for down and up: the bit-1 test of <c>$82:DE55</c> and <c>$80:AF89</c>. Horizontal
    /// doors align camera Y and step X; vertical doors align camera X and step Y.
    /// </summary>
    public static bool IsVertical(this DoorDirection direction) => direction switch
    {
        DoorDirection.Right or DoorDirection.Left => false,
        DoorDirection.Down or DoorDirection.Up => true,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined door direction."),
    };
}
