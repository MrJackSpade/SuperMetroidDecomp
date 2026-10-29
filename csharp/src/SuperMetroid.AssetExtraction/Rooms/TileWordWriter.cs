namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Something that writes tile words over a room's BG1 or BG2 after the room loads (#1392).</summary>
public enum TileWordWriter
{
    /// <summary>Kraid, whose body pages and head frames fill his 64x64 BG2 map.</summary>
    Kraid,

    /// <summary>Phantoon, drawing its body frames into the enemy BG2 image.</summary>
    Phantoon,

    /// <summary>Draygon, drawing its body frames into the enemy BG2 image.</summary>
    Draygon,

    /// <summary>Crocomire, drawing its body frames and melting images into the enemy BG2 image.</summary>
    Crocomire,

    /// <summary>Mother Brain, drawing her body frames into the enemy BG2 image from phase two.</summary>
    MotherBrain,

    /// <summary>The Zebes escape typewriter, typing its warning over Mother Brain's room's BG2.</summary>
    ZebesEscapeText,

    /// <summary>
    /// Ceres Ridley's getaway, writing its EMERGENCY title, the Japanese subtitle and its typed warning over the arena's BG1.
    /// </summary>
    CeresEscapeText,
}
