namespace SuperMetroid.Core.Game;

/// <summary>Scroll-zone writes made by MainAI_Crocomire_DeathSequence_0_NotStarted.</summary>
internal static class CrocomireCameraDefinitions
{
    /// <summary>$A4:8C75/8C84 write Scrolls+4, the screen immediately left of the melting viewport.</summary>
    internal const int BridgeLeftScreen = 4;
    /// <summary>The high byte of the same native word write keeps Scrolls+5 blue.</summary>
    internal const int BridgeScreen = 5;
    /// <summary>$A4:8C7C compares SamusXPosition against $0520 before selecting the red left boundary.</summary>
    internal const ushort BridgeVisibleSamusX = 0x0520;
}
