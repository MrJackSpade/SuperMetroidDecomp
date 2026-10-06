namespace SuperMetroid.Core.Game;

/// <summary>Mother Brain's staged enemy tilemap and initial visible transfer.</summary>
internal static class MotherBrainBg2Definitions
{
    /// <summary>EnemyBG2Tilemap at $7E:2000, cleared by InitAI_MotherBrainBody at $A9:8693.</summary>
    internal const int WorkAddress = 0x7e2000;
    /// <summary>$A9:868D loads empty tile $0338 before clearing the staging image.</summary>
    internal const ushort BlankTile = 0x0338;
    /// <summary>$A9:8690 starts at byte offset $0FFE, covering $800 words inclusively.</summary>
    internal const int ClearWordCount = 0x800;
    /// <summary>$A0:8A49 initializes EnemyBG2TilemapSize to $800 bytes; $A9:8D41 requests its first transfer.</summary>
    internal const ushort InitialTransferByteCount = 0x800;
}
