using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The grey door caps and the four shared cleared openings, and the block shape each draws from
/// <see cref="GreyDoorObjectDrawDefinitions"/> (#1862, #1994).
/// </summary>
internal static class GreyDoorCaps
{
    /// <summary>The block shape of each grey-cap draw: its four blocks down its column or along its row.</summary>
    private static readonly Dictionary<Draw, RoomBlockShape> Shapes = GreyDoorObjectDrawDefinitions.Collisions
        .ToDictionary(pair => pair.Key, pair => RoomBlockShapes.Line(pair.Key.LineAxis, pair.Value));

    /// <summary>One of the twenty grey door cap pictures: its facing and what it shows.</summary>
    /// <param name="Orientation">The cap's facing.</param>
    /// <param name="Look">The cleared opening every cap leaves (#1627), or the grey cap closed or sliding aside.</param>
    internal readonly record struct Draw(ColoredDoorOrientation Orientation, DoorCapLook Look)
    {
        /// <summary>The cleared opening facing <paramref name="orientation"/>, which draws no cap.</summary>
        internal static Draw ClearedOf(ColoredDoorOrientation orientation) => new(orientation, DoorCapLook.Cleared);

        /// <summary>Whether this is the cleared opening rather than the grey cap.</summary>
        internal bool IsCleared => Look == DoorCapLook.Cleared;

        /// <summary>The axis this cap's four blocks stand along: a column for a left or right cap's, a row otherwise (#2560).</summary>
        internal Axis LineAxis => DoorCapAxes.Of(Orientation);

        /// <summary>The cap's four blocks from its first and how each collides.</summary>
        internal RoomBlockShape Blocks => Shapes[this];
    }

    /// <summary>The grey caps and the cleared openings (#2034), pictured by the installed grey-door frames.</summary>
    internal static RoomObjectDrawFamily<Draw> Family { get; } =
        new RoomObjectDrawFamily<Draw>("grey-door", cap => cap.Blocks);
}
