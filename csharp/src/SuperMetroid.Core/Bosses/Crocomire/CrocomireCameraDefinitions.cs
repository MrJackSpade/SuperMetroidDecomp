namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Scroll-zone writes made as Crocomire's death sequence starts.</summary>
internal static class CrocomireCameraDefinitions
{
    /// <summary>The bridge's scroll setup writes the screen immediately left of the melting viewport.</summary>
    internal const int BridgeLeftScreen = 4;
    /// <summary>The high byte of the same word write keeps Scrolls+5 blue.</summary>
    internal const int BridgeScreen = 5;
    /// <summary>The bridge's scroll setup compares Samus's X against 1312 before selecting the red left boundary.</summary>
    internal const int BridgeVisibleSamusX = 1312;
}
