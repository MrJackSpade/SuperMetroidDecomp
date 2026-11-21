namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Axis predicates over <see cref="DoorDirection"/>.</summary>
public static partial class DoorDirections
{
    /// <summary>
    /// True for down and up, as the door transition and room loader test it. Horizontal
    /// doors align camera Y and step X; vertical doors align camera X and step Y.
    /// </summary>
    public static bool IsVertical(this DoorDirection direction) => direction.TravelAxis() is Axis.Vertical;

    /// <summary>The axis a door of <paramref name="direction"/> is passed along (#2560): across for left and right, up or down for up and down.</summary>
    public static Axis TravelAxis(this DoorDirection direction) => direction switch
    {
        DoorDirection.Right or DoorDirection.Left => Axis.Horizontal,
        DoorDirection.Down or DoorDirection.Up => Axis.Vertical,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined door direction."),
    };
}
