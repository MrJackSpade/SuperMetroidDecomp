namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>One cell of a Crocomire melt layout as the cartridge packs it, import data only the import composes pictures from (#1638).</summary>
public sealed partial record CrocomireMeltingTilemapCell
{
    /// <summary>BG character selector from 0 through 1023, encoded in the low ten bits; blank glyphs and repeated references remain independent presentation choices.</summary>
    public required int TileIndex { get; init; }
    /// <summary>SNES BG palette index from 0 through 7, encoded in bits 10–12; stock illustrated fragments use palette 7.</summary>
    public required int Palette { get; init; }
    /// <summary>Whether to set packed BG priority bit $2000; this changes visual layering, not dissolve timing or collision.</summary>
    public required bool Priority { get; init; }
    /// <summary>Whether to mirror the referenced character horizontally, setting packed BG word bit $4000.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Whether to mirror the referenced character vertically, setting packed BG word bit $8000.</summary>
    public required bool FlipY { get; init; }
}
