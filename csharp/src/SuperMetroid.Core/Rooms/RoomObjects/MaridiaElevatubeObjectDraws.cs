namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Maridia elevatube's draw (#2034): its one solid block and its installed picture (#1557).</summary>
internal static class MaridiaElevatubeObjectDraws
{
    /// <summary>The elevatube's block, pictured by its installed frame.</summary>
    internal static RoomObjectDrawFamily<MaridiaElevatubeObjectDefinitions.Draw> Family { get; } =
        RoomObjectDrawFamily<MaridiaElevatubeObjectDefinitions.Draw>.Pictured<InstalledRoomObjectMaridiaElevatubeVisuals>("Maridia elevatube",
            _ => MaridiaElevatubeObjectDefinitions.Shape, (visuals, draw) => visuals.Image(draw));
}
