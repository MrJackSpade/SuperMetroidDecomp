using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The grey door caps and the four shared cleared openings, and how their cells collide from
/// <see cref="GreyDoorObjectDrawDefinitions"/>.
/// </summary>
internal static class GreyDoorCaps
{
    /// <summary>One of the twenty grey door cap pictures: its facing and its animation frame.</summary>
    /// <param name="Orientation">The cap's facing.</param>
    /// <param name="Frame">The cap's animation frame, 0 through 3; none for the cleared opening, which draws no cap (#1627).</param>
    internal readonly record struct Draw(ColoredDoorOrientation Orientation, [property: IntegerQuantity("cap animation frame index")] int? Frame)
    {
        /// <summary>The cleared opening facing <paramref name="orientation"/>, which draws no cap and so has no frame.</summary>
        internal static Draw ClearedOf(ColoredDoorOrientation orientation) => new(orientation, null);

        /// <summary>Whether this is the cleared opening rather than one of the cap's frames.</summary>
        internal bool IsCleared => Frame is null;

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
        RoomObjectDraw.TryFrame(request, static (Draw cap) => cap.Frame is null or (>= 0 and < 4), out draw);
}
