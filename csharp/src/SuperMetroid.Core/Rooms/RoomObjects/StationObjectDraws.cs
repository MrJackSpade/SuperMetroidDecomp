namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The stations' draws (#2034): the block shape each draws (<see cref="StationObjectDrawDefinitions"/>, #1862), and the
/// installed picture (#1557) each shows.
/// </summary>
internal static class StationObjectDraws
{
    /// <summary>The stations' frames, pictured by the installed station frames.</summary>
    internal static RoomObjectDrawFamily<StationDraw> Family { get; } =
        RoomObjectDrawFamily<StationDraw>.Pictured<InstalledRoomObjectStationVisuals>("station",
            frame => RoomObjectDrawFamily<StationDraw>.Defined(StationObjectDrawDefinitions.Shapes, frame),
            (visuals, draw) => visuals.Image(draw));
}
