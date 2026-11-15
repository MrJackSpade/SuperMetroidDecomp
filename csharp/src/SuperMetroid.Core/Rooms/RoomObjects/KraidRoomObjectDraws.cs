namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Kraid's room's ceiling and spike block draws (#2034): the block shape each draws, from
/// <see cref="KraidRoomObjectDrawDefinitions"/>, and the installed picture (#1557) each shows.
/// </summary>
internal static class KraidRoomObjectDraws
{
    /// <summary>The room's ten frames, pictured by the installed Kraid room frames.</summary>
    internal static RoomObjectDrawFamily<KraidRoomDraw> Family { get; } =
        RoomObjectDrawFamily<KraidRoomDraw>.Pictured<InstalledRoomObjectKraidVisuals>("Kraid room",
            frame => RoomObjectDrawFamily<KraidRoomDraw>.Defined(KraidRoomObjectDrawDefinitions.Shapes, frame),
            (visuals, draw) => visuals.Image(draw));
}
