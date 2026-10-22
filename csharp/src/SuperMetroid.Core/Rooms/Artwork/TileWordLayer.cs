namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>The backgrounds a tile word sheet pictures words for (#1392).</summary>
public enum TileWordLayer
{
    /// <summary>BG1 and BG2: four-bit-per-pixel words over the room's characters, such as a boss's body or typed text.</summary>
    Bg12,

    /// <summary>BG3: two-bit-per-pixel words over the HUD's and room effects' characters, the HUD and an effect's page.</summary>
    Bg3,
}
