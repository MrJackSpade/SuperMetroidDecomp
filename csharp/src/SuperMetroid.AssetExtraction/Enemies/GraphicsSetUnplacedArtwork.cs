using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>A graphics-set record loading artwork no enemy of the game draws (#1686, #2560).</summary>
/// <param name="Artwork">The artwork the record loads.</param>
internal sealed partial record GraphicsSetUnplacedArtwork(UnplacedEnemyArtwork Artwork) : RoomEnemyGraphicsSetSource
{
    /// <inheritdoc/>
    internal override string Name => Artwork.ToString();

    /// <inheritdoc/>
    internal override int TileByteCount => Ensure.Listed(EnemyTileSetDefinitions.UnplacedByteCounts, Artwork);

    /// <inheritdoc/>
    internal override int? FixedBlock =>
        EnemyTileSetDefinitions.UnplacedFixedBlocks.TryGetValue(Artwork, out int block) ? block : null;

    /// <inheritdoc/>
    internal override ReadOnlyMemory<byte> TilesIn(EnemyTileSourceArtwork artwork) => artwork.UnplacedTilesOf(Artwork);

    /// <inheritdoc/>
    internal override EnemyPaletteSheet PaletteIn(EnemyTileSourceArtwork artwork) => artwork.UnplacedPaletteOf(Artwork);
}
