namespace SuperMetroid.Core.Assets;

/// <summary>Native-size artwork references, not tile numbers, addresses or gameplay commands.</summary>
public sealed record PauseBackdropCell
{
    /// <summary>Case-sensitive artwork sheet key, Map or Interface, selecting the first or second 256-character page of the loaded pause graphics.</summary>
    public required string Atlas { get; init; }
    /// <summary>Zero-based column 0..31 of an eight-pixel tile within the selected atlas, not its destination tilemap column.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Zero-based row 0..7 of an eight-pixel tile within the selected atlas, not its destination tilemap row.</summary>
    public required int TileRow { get; init; }
    /// <summary>BG palette selector 0..7, encoded in tilemap attribute bits 10 through 12.</summary>
    public required int Palette { get; init; }
    /// <summary>Whether the tilemap's BG priority bit 13 is set; this is not an OBJ priority tier.</summary>
    public required bool Priority { get; init; }
    /// <summary>Whether the selected tile's pixels are reflected horizontally via attribute bit 14.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Whether the selected tile's pixels are reflected vertically via attribute bit 15.</summary>
    public required bool FlipY { get; init; }
}
