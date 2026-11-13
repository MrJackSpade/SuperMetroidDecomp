using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The blue-door caps: the closed alias and the four opening frames of each facing, and how their cells collide from
/// <see cref="BlueDoorObjectDrawDefinitions"/>.
/// </summary>
internal static class BlueDoorCaps
{
    /// <summary>One of the twenty blue-cap draws: four facings, each with the alias and four opening frames.</summary>
    /// <param name="Orientation">The cap's facing.</param>
    /// <param name="Frame">
    /// The opening frame, 0 through 3; none for the physical closed-cap alias, which shows frame zero's picture with only
    /// its first cell solid (#1627).
    /// </param>
    internal readonly record struct Draw(ColoredDoorOrientation Orientation, int? Frame)
    {
        /// <summary>The physical closed-cap alias facing <paramref name="orientation"/>, which has no opening frame.</summary>
        internal static Draw AliasOf(ColoredDoorOrientation orientation) => new(orientation, null);

        /// <summary>Whether this is the physical closed-cap alias rather than an opening frame.</summary>
        internal bool IsAlias => Frame is null;

        /// <summary>Whether this cap's four blocks are laid out as a vertical run.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;

        /// <summary>How <paramref name="cell"/>, one of the cap's four cells in run order, collides.</summary>
        internal RoomCollisionType CollisionAt(int cell)
        {
            if ((uint)cell >= 4) throw new ArgumentOutOfRangeException(nameof(cell));
            return BlueDoorObjectDrawDefinitions.Collisions[this][cell];
        }
    }

    /// <summary>The closed cap facing <paramref name="orientation"/>, the alias that shows frame zero's artwork.</summary>
    internal static RoomObjectDraw Alias(ColoredDoorOrientation orientation) => RoomObjectDraw.Of(Draw.AliasOf(orientation));

    /// <summary>Accepts a blue-cap draw: the alias or one of the four opening frames.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw) =>
        RoomObjectDraw.TryFrame(request, static (Draw cap) => cap.Frame is null or (>= 0 and < 4), out draw);

    /// <summary>Maps a physical closed-cap alias to the matching editable frame-zero artwork.</summary>
    internal static RoomObjectDraw VisualSource(RoomObjectDraw request) =>
        request is RoomObjectFrameDraw<Draw> { Frame: { IsAlias: true } alias }
            ? RoomObjectDraw.Of(alias with { Frame = BlueDoorObjectDrawDefinitions.AliasPictureFrame })
            : request;
}
