using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The yellow, green and red door caps: each colour's four frames per facing and how their cells collide from
/// <see cref="ColoredDoorObjectDrawDefinitions"/>.
/// </summary>
internal static class ColoredDoorCaps
{
    /// <summary>Color, direction, and animation phase of one cap draw.</summary>
    internal readonly record struct Draw(ColoredDoorColor Color, ColoredDoorOrientation Orientation, [property: IntegerQuantity("cap animation frame index")] int Frame)
    {
        /// <summary>Left and right caps draw a vertical four-cell run in authored list orientation.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;

        /// <summary>How <paramref name="cell"/>, one of the cap's four cells in run order, collides.</summary>
        internal RoomCollisionType CollisionAt(int cell)
        {
            if ((uint)cell >= 4) throw new ArgumentOutOfRangeException(nameof(cell));
            return ColoredDoorObjectDrawDefinitions.Collisions[this][cell];
        }
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
