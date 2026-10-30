using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Decoded fields of one sixteen-bit SNES BG tilemap word, separating the character-sheet selector from its destination cell.</summary>
public sealed record RoomBackgroundTilemapCell
{
    /// <summary>Zero-based character-sheet column, 0-31; together with <see cref="TileRow"/> forms character index <c>TileRow * 32 + TileColumn</c>, not the destination map column.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Zero-based character-sheet row, 0-31, contributing to the ten-bit character index; not the destination map row.</summary>
    public required int TileRow { get; init; }
    /// <summary>Three-bit BG palette selector, 0-7, encoded in tilemap word bits 10-12; actual colors come from the consuming layer's palette.</summary>
    public required int Palette { get; init; }
    /// <summary>Whether native BG priority bit 13 is set; final layer ordering still depends on the selected PPU mode.</summary>
    public required bool Priority { get; init; }
    /// <summary>Whether native bit 14 mirrors the selected character horizontally when rendered.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Whether native bit 15 mirrors the selected character vertically when rendered.</summary>
    public required bool FlipY { get; init; }

    /// <summary>The tile this cell places, or null when its tile or palette lies outside the schema.</summary>
    internal BackgroundTile? Placement() =>
        (uint)TileColumn < RoomBackgroundTilemapFormat.TileColumns &&
        (uint)TileRow < RoomBackgroundTilemapFormat.TileRows &&
        (uint)Palette < RoomBackgroundTilemapFormat.PaletteCount
            ? new BackgroundTile(TileRow * RoomBackgroundTilemapFormat.TileColumns + TileColumn, Palette, FlipX, FlipY, Priority)
            : null;
}
