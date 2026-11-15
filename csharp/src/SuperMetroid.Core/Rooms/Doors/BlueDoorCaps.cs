using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The blue-door caps: the closed cap and the four opening frames of each facing, and the block shape each draws from
/// <see cref="BlueDoorObjectDrawDefinitions"/> (#1862).
/// </summary>
internal static class BlueDoorCaps
{
    /// <summary>The block shape of each blue-cap draw: its four blocks down its column or along its row.</summary>
    private static readonly Dictionary<Draw, RoomBlockShape> Shapes = BlueDoorObjectDrawDefinitions.Collisions
        .ToDictionary(pair => pair.Key, pair => RoomBlockShapes.Line(pair.Key.Vertical, pair.Value));

    /// <summary>One of the twenty blue-cap draws: four facings, each with the closed cap and four opening frames.</summary>
    /// <param name="Orientation">The cap's facing.</param>
    /// <param name="Frame">
    /// The opening frame, 0 through 3; none for the closed cap, which shows frame zero's picture with only its first block
    /// solid (#1627).
    /// </param>
    internal readonly record struct Draw(ColoredDoorOrientation Orientation, int? Frame)
    {
        /// <summary>The closed cap facing <paramref name="orientation"/>, which has no opening frame.</summary>
        internal static Draw ClosedFacing(ColoredDoorOrientation orientation) => new(orientation, null);

        /// <summary>Whether this is the closed cap rather than an opening frame.</summary>
        internal bool IsClosed => Frame is null;

        /// <summary>Whether this cap's four blocks stand in a column: a left or right cap's.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;

        /// <summary>The cap's four blocks from its first and how each collides.</summary>
        internal RoomBlockShape Blocks => Shapes[this];
    }

    /// <summary>The closed cap facing <paramref name="orientation"/>, which shows frame zero's artwork.</summary>
    internal static RoomObjectDraw Closed(ColoredDoorOrientation orientation) => RoomObjectDraw.Of(Draw.ClosedFacing(orientation));

    /// <summary>Accepts a blue-cap draw: the closed cap or one of the four opening frames.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw) =>
        RoomObjectDraw.TryFrame(request, static (Draw cap) => cap.Frame is null or (>= 0 and < 4), out draw);

    /// <summary>Maps the closed cap to the matching editable frame-zero artwork.</summary>
    internal static RoomObjectDraw VisualSource(RoomObjectDraw request) =>
        request is RoomObjectFrameDraw<Draw> { Frame: { IsClosed: true } closed }
            ? RoomObjectDraw.Of(closed with { Frame = BlueDoorObjectDrawDefinitions.ClosedPictureFrame })
            : request;
}
