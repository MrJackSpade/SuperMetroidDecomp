using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The yellow, green and red door caps: each colour's four frames per facing and the block shape each draws from
/// <see cref="ColoredDoorObjectDrawDefinitions"/> (#1862).
/// </summary>
internal static class ColoredDoorCaps
{
    /// <summary>The block shape of each coloured-cap draw: its four blocks down its column or along its row.</summary>
    private static readonly Dictionary<Draw, RoomBlockShape> Shapes = ColoredDoorObjectDrawDefinitions.Collisions
        .ToDictionary(pair => pair.Key, pair => RoomBlockShapes.Line(pair.Key.Vertical, pair.Value));

    /// <summary>Color, direction, and animation phase of one cap draw.</summary>
    internal readonly record struct Draw(ColoredDoorColor Color, ColoredDoorOrientation Orientation, [property: IntegerQuantity("cap animation frame index")] int Frame)
    {
        /// <summary>Whether this cap's four blocks stand in a column: a left or right cap's.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;

        /// <summary>The cap's four blocks from its first and how each collides.</summary>
        internal RoomBlockShape Blocks => Shapes[this];
    }

    /// <summary>Describes a coloured door cap's draw: its colour, orientation and one of its four frames.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw)
    {
        if (request is RoomObjectFrameDraw<Draw> { Frame: { Frame: >= 0 and < 4 } cap })
        {
            draw = cap;
            return true;
        }
        draw = default;
        return false;
    }
}
