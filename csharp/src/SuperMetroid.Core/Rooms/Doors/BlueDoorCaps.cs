using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The blue-door caps: each facing's cap closed and sliding aside, and its closed cap drawn solid, the block shape each
/// draws from <see cref="BlueDoorObjectDrawDefinitions"/> (#1862, #1994) and the installed picture each shows (#2034).
/// </summary>
internal static class BlueDoorCaps
{
    /// <summary>The block shape of each blue-cap draw: its four blocks down its column or along its row.</summary>
    private static readonly Dictionary<Draw, RoomBlockShape> Shapes = BlueDoorObjectDrawDefinitions.Collisions
        .ToDictionary(pair => pair.Key, pair => RoomBlockShapes.Line(pair.Key.Vertical, pair.Value));

    /// <summary>One of the twenty blue-cap draws: four facings, each with the cap closed, sliding aside, or closed and solid.</summary>
    /// <param name="Orientation">The cap's facing.</param>
    /// <param name="Look">
    /// The cap closed or sliding aside, or <see cref="DoorCapLook.ClosedSolid"/>: the closed cap's picture with only its
    /// first block solid (#1627).
    /// </param>
    internal readonly record struct Draw(ColoredDoorOrientation Orientation, DoorCapLook Look)
    {
        /// <summary>Whether this cap's four blocks stand in a column: a left or right cap's.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;

        /// <summary>The cap's four blocks from its first and how each collides.</summary>
        internal RoomBlockShape Blocks => Shapes[this];
    }

    /// <summary>The blue caps (#2034), pictured by the installed blue-door frames; the closed cap drawn solid shows the closed cap's.</summary>
    internal static RoomObjectDrawFamily<Draw> Family { get; } =
        RoomObjectDrawFamily<Draw>.Pictured<InstalledRoomObjectBlueDoorVisuals>("blue-door", cap => cap.Blocks,
            (visuals, draw) => visuals.Image(draw));

    /// <summary>The closed cap drawn solid facing <paramref name="orientation"/>, which shows the closed cap's picture.</summary>
    internal static RoomObjectDraw ClosedSolid(ColoredDoorOrientation orientation) =>
        Family.Of(new Draw(orientation, DoorCapLook.ClosedSolid));

    /// <summary>The draw whose installed picture <paramref name="request"/> shows: the closed cap drawn solid shows the closed cap's.</summary>
    internal static RoomObjectDraw VisualSource(RoomObjectDraw request) =>
        request is RoomObjectFrameDraw<Draw> { Frame: { Look: DoorCapLook.ClosedSolid } solid }
            ? Family.Of(solid with { Look = DoorCapLook.Closed })
            : request;
}
