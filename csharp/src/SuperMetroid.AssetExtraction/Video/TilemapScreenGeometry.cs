namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// The cartridge's tilemap screen: the 32-by-32-cell page its background tilemaps are laid out in, which the import reads
/// tilemaps through and verification lays native memory out with. The game's display model knows no such page.
/// </summary>
public static class TilemapScreenGeometry
{
    /// <summary>Tiles on either axis of one tilemap screen.</summary>
    public const int WidthInTiles = 32;

    /// <summary>Cells in one 32-by-32 tilemap screen.</summary>
    public const int CellCount = WidthInTiles * WidthInTiles;
}
