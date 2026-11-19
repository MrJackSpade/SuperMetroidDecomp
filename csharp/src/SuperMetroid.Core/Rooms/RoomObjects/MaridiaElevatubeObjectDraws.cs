namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Maridia elevatube's draw (#2034): its one solid block and its installed picture (#1557).</summary>
internal static class MaridiaElevatubeObjectDraws
{
    /// <summary>The elevatube's block, pictured by its installed frame.</summary>
    internal static RoomObjectDrawFamily<MaridiaElevatubeObjectDefinitions.Draw> Family { get; } =
        new RoomObjectDrawFamily<MaridiaElevatubeObjectDefinitions.Draw>("Maridia elevatube",
            _ => MaridiaElevatubeObjectDefinitions.Shape);
}
