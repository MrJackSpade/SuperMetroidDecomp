using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>A graphics-set record loading an enemy's artwork (#2560).</summary>
/// <param name="DefinitionId">The member enemy, which selects its installed sheets and its spawned objects' sheet.</param>
internal sealed record GraphicsSetEnemy(EnemyDefinitionId DefinitionId) : RoomEnemyGraphicsSetSource
{
    /// <inheritdoc/>
    internal override string Name => DefinitionId.ToString();

    /// <inheritdoc/>
    internal override int? TileByteCount =>
        EnemyTileSetDefinitions.ByteCounts.TryGetValue(DefinitionId, out int bytes) ? bytes : null;

    /// <inheritdoc/>
    internal override int? FixedBlock =>
        EnemyTileSetDefinitions.FixedBlocks.TryGetValue(DefinitionId, out int block) ? block : null;

    /// <inheritdoc/>
    internal override ReadOnlyMemory<byte> TilesIn(EnemyTileSourceArtwork artwork) => artwork.TilesOf(DefinitionId);

    /// <inheritdoc/>
    internal override EnemyPaletteSheet PaletteIn(EnemyTileSourceArtwork artwork) => artwork.PaletteOf(DefinitionId);
}
