namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>Serializable components of one SNES BG tilemap word.</summary>
public sealed record GameplayHudCell
{
    /// <summary>Gets the zero-based character column in the 32-by-32 character page.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Gets the zero-based character row in the 32-by-32 character page.</summary>
    public required int TileRow { get; init; }
    /// <summary>Gets the three-bit BG palette index.</summary>
    public required int Palette { get; init; }
    /// <summary>Gets whether the BG tile uses high priority.</summary>
    public required bool Priority { get; init; }
    /// <summary>Gets whether the character is flipped horizontally.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Gets whether the character is flipped vertically.</summary>
    public required bool FlipY { get; init; }
}
