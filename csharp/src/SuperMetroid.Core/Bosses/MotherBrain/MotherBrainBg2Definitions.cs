namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's staged enemy tilemap and initial visible transfer.</summary>
internal static class MotherBrainBg2Definitions
{
    /// <summary>$A9:868D loads empty tile $0338 before clearing the staging image.</summary>
    internal const ushort BlankTile = 0x0338;
    /// <summary>$A9:8690 starts at byte offset $0FFE, covering $800 words inclusively.</summary>
    internal const int ClearWordCount = 0x800;
    /// <summary>$A0:8A49 initializes EnemyBG2TilemapSize to $800 bytes, the whole working tilemap; $A9:8D41 first shows it.</summary>
    internal const ushort InitialTilemapByteCount = 0x800;
}
