using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The yellow, green and red door caps: each colour's cap closed and sliding aside per facing, and the block shape each
/// draws from <see cref="ColoredDoorObjectDrawDefinitions"/> (#1862, #1994).
/// </summary>
internal static class ColoredDoorCaps
{
    /// <summary>The block shape of each coloured-cap draw: its four blocks down its column or along its row.</summary>
    private static readonly Dictionary<Draw, RoomBlockShape> Shapes = ColoredDoorObjectDrawDefinitions.Collisions
        .ToDictionary(pair => pair.Key, pair => RoomBlockShapes.Line(pair.Key.LineAxis, pair.Value));

    /// <summary>One coloured cap draw: its colour, its facing and what it shows.</summary>
    /// <param name="Color">The cap's colour.</param>
    /// <param name="Orientation">The cap's facing.</param>
    /// <param name="Look">The cap closed or sliding aside.</param>
    internal readonly record struct Draw(ColoredDoorColor Color, ColoredDoorOrientation Orientation, DoorCapLook Look)
    {
        /// <summary>The axis this cap's four blocks stand along: a column for a left or right cap's, a row otherwise (#2560).</summary>
        internal Axis LineAxis => DoorCapAxes.Of(Orientation);

        /// <summary>The cap's four blocks from its first and how each collides.</summary>
        internal RoomBlockShape Blocks => Shapes[this];
    }

    /// <summary>The coloured caps (#2034), pictured by the installed coloured-door frames.</summary>
    internal static RoomObjectDrawFamily<Draw> Family { get; } =
        new RoomObjectDrawFamily<Draw>("colored-door", cap => cap.Blocks);
}
