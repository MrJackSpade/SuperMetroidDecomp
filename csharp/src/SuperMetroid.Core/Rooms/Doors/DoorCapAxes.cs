using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The axis a door cap's blocks stand along (#2560): a column for a cap on a left or right wall, a row for a cap on a ceiling or floor.</summary>
internal static class DoorCapAxes
{
    /// <summary>The axis a cap facing <paramref name="orientation"/> stands along.</summary>
    internal static Axis Of(ColoredDoorOrientation orientation) => orientation switch
    {
        ColoredDoorOrientation.Left or ColoredDoorOrientation.Right => Axis.Vertical,
        ColoredDoorOrientation.Up or ColoredDoorOrientation.Down => Axis.Horizontal,
        _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined door orientation."),
    };
}
