namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The timing of the gate in Mother Brain's escape room (#1554).</summary>
internal static class MotherBrainEscapeGateDefinitions
{
    /// <summary>The gate the room places: drawn closed on its first update, it leaves the room six updates later.</summary>
    internal static TerrainChange Shut { get; } = new(1,
    [
        new TerrainChangeFrame(RoomObjectDraw.Of(MotherBrainEscapeGateDraw.Closed), 6),
    ]);

    /// <summary>
    /// The gate closing behind Samus: open, half closed and closed, two updates each, from its first update, after which it
    /// leaves the room.
    /// </summary>
    internal static TerrainChange Closing { get; } = new(1,
    [
        new TerrainChangeFrame(RoomObjectDraw.Of(MotherBrainEscapeGateDraw.Open), 2),
        new TerrainChangeFrame(RoomObjectDraw.Of(MotherBrainEscapeGateDraw.HalfClosed), 2),
        new TerrainChangeFrame(RoomObjectDraw.Of(MotherBrainEscapeGateDraw.Closed), 2),
    ]);
}
