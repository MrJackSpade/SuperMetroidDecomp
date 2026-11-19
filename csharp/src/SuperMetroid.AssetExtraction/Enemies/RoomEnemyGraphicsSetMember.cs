using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// One record of a room's enemy graphics set, in the set's order: the enemy whose artwork the set loads, or artwork the set
/// loads for no enemy of the game (<see cref="UnplacedEnemyArtwork"/>, #1686), which only lands where the set's other
/// pictures draw from.
/// </summary>
/// <remarks>
/// The colours a member draws in are its installed sheet's (#1512) and its pictures are its sheet's frames (#1513): the
/// import composes and pre-colours each member's sheets as its graphics set loads it, so a member names nothing else.
/// </remarks>
internal sealed record RoomEnemyGraphicsSetMember
{
    /// <summary>What the record loads: an enemy's artwork or artwork no enemy draws, exactly one of them (#2554).</summary>
    private readonly Source source;

    /// <summary>A record loading <paramref name="definitionId"/>'s artwork.</summary>
    /// <param name="definitionId">The member enemy, which selects its installed sheets and its spawned objects' sheet.</param>
    internal RoomEnemyGraphicsSetMember(EnemyDefinitionId definitionId) => source = new EnemySource(definitionId);

    /// <summary>A record loading <paramref name="artwork"/>, which no enemy of the game draws.</summary>
    internal RoomEnemyGraphicsSetMember(UnplacedEnemyArtwork artwork) => source = new UnplacedSource(artwork);

    /// <summary>The member enemy; null for a record loading <see cref="Unplaced"/> artwork.</summary>
    internal EnemyDefinitionId? DefinitionId => source is EnemySource enemy ? enemy.Enemy : null;

    /// <summary>The artwork the record loads for no enemy; null for an enemy's record.</summary>
    internal UnplacedEnemyArtwork? Unplaced => source is UnplacedSource unplaced ? unplaced.Artwork : null;

    /// <summary>The record's name in diagnostics: its enemy's, or its unplaced artwork's.</summary>
    internal string Name => source.Match(enemy => enemy.ToString(), artwork => artwork.ToString());

    /// <summary>The bytes of the tile set the record uploads.</summary>
    /// <exception cref="InvalidDataException">The import does not catalogue the record's tile set.</exception>
    internal int TileByteCount
    {
        get
        {
            int bytes = 0;
            bool catalogued = source.Match(
                enemy => EnemyTileSetDefinitions.ByteCounts.TryGetValue(enemy, out bytes),
                artwork => EnemyTileSetDefinitions.UnplacedByteCounts.TryGetValue(artwork, out bytes));
            return catalogued ? bytes
                : throw new InvalidDataException($"Graphics-set record {Name} names a tile set the import does not catalogue.");
        }
    }

    /// <summary>The fixed staging block, 0 through 3, the record uploads its tiles to; null when they follow the previous record's.</summary>
    internal int? FixedBlock => source.Match<int?>(
        enemy => EnemyTileSetDefinitions.FixedBlocks.TryGetValue(enemy, out int block) ? block : null,
        artwork => EnemyTileSetDefinitions.UnplacedFixedBlocks.TryGetValue(artwork, out int block) ? block : null);

    /// <summary>The installed tiles the record uploads, from <paramref name="artwork"/>.</summary>
    /// <exception cref="InvalidDataException">The artwork holds no tile set for the record.</exception>
    internal ReadOnlyMemory<byte> TilesIn(EnemyTileSourceArtwork artwork)
    {
        Ensure.NotNull(artwork);
        return source.Match(artwork.TilesOf, artwork.UnplacedTilesOf);
    }

    /// <summary>The installed colours the record loads, from <paramref name="artwork"/>.</summary>
    /// <exception cref="InvalidDataException">The artwork holds no palette for the record.</exception>
    internal EnemyPaletteSheet PaletteIn(EnemyTileSourceArtwork artwork)
    {
        Ensure.NotNull(artwork);
        return source.Match(artwork.PaletteOf, artwork.UnplacedPaletteOf);
    }

    /// <summary>What a record loads.</summary>
    private abstract record Source
    {
        /// <summary>The result of <paramref name="ofEnemy"/> for an enemy's artwork, or of <paramref name="ofUnplaced"/> for artwork no enemy draws.</summary>
        internal abstract T Match<T>(Func<EnemyDefinitionId, T> ofEnemy, Func<UnplacedEnemyArtwork, T> ofUnplaced);
    }

    /// <summary>An enemy's artwork.</summary>
    private sealed record EnemySource(EnemyDefinitionId Enemy) : Source
    {
        /// <inheritdoc/>
        internal override T Match<T>(Func<EnemyDefinitionId, T> ofEnemy, Func<UnplacedEnemyArtwork, T> ofUnplaced) => ofEnemy(Enemy);
    }

    /// <summary>Artwork a graphics set loads for no enemy.</summary>
    private sealed record UnplacedSource(UnplacedEnemyArtwork Artwork) : Source
    {
        /// <inheritdoc/>
        internal override T Match<T>(Func<EnemyDefinitionId, T> ofEnemy, Func<UnplacedEnemyArtwork, T> ofUnplaced) => ofUnplaced(Artwork);
    }
}
