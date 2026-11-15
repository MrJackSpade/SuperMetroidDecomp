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
        .ToDictionary(pair => pair.Key, pair => RoomBlockShapes.Line(pair.Key.Vertical, pair.Value));

    /// <summary>One of the twenty grey door cap pictures: its facing and what it shows.</summary>
    /// <param name="Orientation">The cap's facing.</param>
    /// <param name="Look">The cleared opening every cap leaves (#1627), or the grey cap closed or sliding aside.</param>
    internal readonly record struct Draw(ColoredDoorOrientation Orientation, DoorCapLook Look)
    {
        /// <summary>The cleared opening facing <paramref name="orientation"/>, which draws no cap.</summary>
        internal static Draw ClearedOf(ColoredDoorOrientation orientation) => new(orientation, DoorCapLook.Cleared);

        /// <summary>Whether this is the cleared opening rather than the grey cap.</summary>
        internal bool IsCleared => Look == DoorCapLook.Cleared;

        /// <summary>Whether this cap's four blocks stand in a column: a left or right cap's.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;

        /// <summary>The cap's four blocks from its first and how each collides.</summary>
        internal RoomBlockShape Blocks => Shapes[this];
    }

    /// <summary>Describes a grey door cap's draw: its orientation and the cleared opening or the cap closed or sliding aside.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw) =>
        RoomObjectDraw.TryFrame(request, static (Draw cap) => cap.IsCleared || DoorCaps.IsOpeningLook(cap.Look), out draw);
}
