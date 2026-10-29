namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Something that writes tile words over a room's BG1 or BG2 after the room loads (#1392).</summary>
public enum TileWordWriter
{
    /// <summary>The Zebes escape typewriter, typing its warning over Mother Brain's room's BG2.</summary>
    ZebesEscapeText,

    /// <summary>
    /// Ceres Ridley's getaway, writing its EMERGENCY title, the Japanese subtitle and its typed warning over the arena's BG1.
    /// </summary>
    CeresEscapeText,
}
