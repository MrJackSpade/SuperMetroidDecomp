using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The grey door caps and the four shared cleared openings, and how their cells collide from
/// <see cref="GreyDoorObjectDrawDefinitions"/>.
/// </summary>
internal static class GreyDoorCaps
{
    /// <summary>One of twenty twelve-byte records.</summary>
    internal readonly record struct Draw(ColoredDoorOrientation Orientation, [property: IntegerQuantity("cap animation frame index")] int Frame)
    {
        /// <summary>Left and right caps use a vertical four-cell run.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;

        /// <summary>How <paramref name="cell"/>, one of the cap's four cells in run order, collides.</summary>
        internal RoomCollisionType CollisionAt(int cell)
        {
            if ((uint)cell >= 4) throw new ArgumentOutOfRangeException(nameof(cell));
            return GreyDoorObjectDrawDefinitions.Collisions[this][cell];
        }
    }

    /// <summary>Describes a grey door cap's draw: its orientation and its clear or animated frame.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw) =>
        RoomObjectDraw.TryFrame(request, static (Draw cap) => cap.Frame is >= GreyDoorObjectDrawDefinitions.ClearFrame and < 4, out draw);
}
