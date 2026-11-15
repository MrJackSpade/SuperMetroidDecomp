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
        .ToDictionary(pair => pair.Key, pair => RoomBlockShapes.Line(pair.Key.Vertical, pair.Value));

    /// <summary>One coloured cap draw: its colour, its facing and what it shows.</summary>
    /// <param name="Color">The cap's colour.</param>
    /// <param name="Orientation">The cap's facing.</param>
    /// <param name="Look">The cap closed or sliding aside.</param>
    internal readonly record struct Draw(ColoredDoorColor Color, ColoredDoorOrientation Orientation, DoorCapLook Look)
    {
        /// <summary>Whether this cap's four blocks stand in a column: a left or right cap's.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;

        /// <summary>The cap's four blocks from its first and how each collides.</summary>
        internal RoomBlockShape Blocks => Shapes[this];
    }

    /// <summary>Describes a coloured door cap's draw: its colour, orientation and the cap closed or sliding aside.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw) =>
        RoomObjectDraw.TryFrame(request, static (Draw cap) => DoorCaps.IsOpeningLook(cap.Look), out draw);
}
