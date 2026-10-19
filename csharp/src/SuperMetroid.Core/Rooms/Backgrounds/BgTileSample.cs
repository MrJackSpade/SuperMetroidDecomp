namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>The tile a background shows at a pixel, and the pixel's position within that tile.</summary>
/// <param name="Tile">The tilemap word shown.</param>
/// <param name="Pixel">The pixel within the tile, before the tile's own flips.</param>
public readonly record struct BgTileSample(SnesBgTilemapWord Tile, Position16<CharacterPixels> Pixel)
{
    /// <summary>The sample at a plane position: the tile there and the position's pixel within it.</summary>
    /// <param name="tile">The tilemap word shown.</param>
    /// <param name="position">The plane position the tile covers.</param>
    public static BgTileSample At(SnesBgTilemapWord tile, Position16<BgPlanePixels> position) =>
        new(tile, new Position16<CharacterPixels>(
            (ushort)(position.X % SnesPpuLayout.BackgroundTileSizePixels), (ushort)(position.Y % SnesPpuLayout.BackgroundTileSizePixels)));
}
