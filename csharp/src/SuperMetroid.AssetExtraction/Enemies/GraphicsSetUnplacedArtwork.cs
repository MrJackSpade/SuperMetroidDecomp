using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>A graphics-set record loading artwork no enemy of the game draws (#1686, #2560).</summary>
/// <param name="Artwork">The artwork the record loads.</param>
internal sealed record GraphicsSetUnplacedArtwork(UnplacedEnemyArtwork Artwork) : RoomEnemyGraphicsSetSource
{
    /// <inheritdoc/>
    internal override string Name => Artwork.ToString();

    /// <inheritdoc/>
    internal override int? TileByteCount =>
        EnemyTileSetDefinitions.UnplacedByteCounts.TryGetValue(Artwork, out int bytes) ? bytes : null;

    /// <inheritdoc/>
    internal override int? FixedBlock =>
        EnemyTileSetDefinitions.UnplacedFixedBlocks.TryGetValue(Artwork, out int block) ? block : null;

    /// <inheritdoc/>
    internal override ReadOnlyMemory<byte> TilesIn(EnemyTileSourceArtwork artwork) => artwork.UnplacedTilesOf(Artwork);

    /// <inheritdoc/>
    internal override EnemyPaletteSheet PaletteIn(EnemyTileSourceArtwork artwork) => artwork.UnplacedPaletteOf(Artwork);
}
