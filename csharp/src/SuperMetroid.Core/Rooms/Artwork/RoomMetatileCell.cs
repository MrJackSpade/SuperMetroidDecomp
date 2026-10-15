namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>Editable components of one SNES BG tilemap word: a ten-bit character index, three-bit palette index, priority, and per-tile flips.</summary>
public sealed record RoomMetatileCell
{
    /// <summary>Zero-based column 0..31 in the logical 32-column character catalog; character index equals TileRow times 32 plus TileColumn.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Zero-based row 0..31 in the logical character catalog, selecting one of 1024 possible 8x8 BG characters with <see cref="TileColumn"/>.</summary>
    public required int TileRow { get; init; }
    /// <summary>Background palette index 0..7 encoded in tilemap bits 10..12; colors belong to the separate room palette catalog.</summary>
    public required int Palette { get; init; }
    /// <summary>Whether the BG tilemap priority bit 13 is set; final layer ordering remains the renderer's responsibility.</summary>
    public required bool Priority { get; init; }
    /// <summary>Whether this 8x8 character is horizontally mirrored, encoded in bit 14 independently of room-placement block flips.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Whether this 8x8 character is vertically mirrored, encoded in bit 15 independently of room-placement block flips.</summary>
    public required bool FlipY { get; init; }
}
