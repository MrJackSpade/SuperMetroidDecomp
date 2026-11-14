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
internal readonly record struct RoomEnemyGraphicsSetMember
{
    /// <summary>A record loading <paramref name="definitionId"/>'s artwork.</summary>
    /// <param name="definitionId">The member enemy, which selects its installed sheets and its spawned objects' sheet.</param>
    internal RoomEnemyGraphicsSetMember(EnemyDefinitionId definitionId) => DefinitionId = definitionId;

    /// <summary>A record loading <paramref name="artwork"/>, which no enemy of the game draws.</summary>
    internal RoomEnemyGraphicsSetMember(UnplacedEnemyArtwork artwork) => Unplaced = artwork;

    /// <summary>The member enemy; null for a record loading <see cref="Unplaced"/> artwork.</summary>
    internal EnemyDefinitionId? DefinitionId { get; }

    /// <summary>The artwork the record loads for no enemy; null for an enemy's record.</summary>
    internal UnplacedEnemyArtwork? Unplaced { get; }

    /// <summary>The record's name in diagnostics: its enemy's, or its unplaced artwork's.</summary>
    internal string Name => DefinitionId is { } enemy ? enemy.ToString() : UnplacedArtwork.ToString();

    /// <summary>The bytes of the tile set the record uploads.</summary>
    /// <exception cref="InvalidDataException">The import does not catalogue the record's tile set.</exception>
    internal int TileByteCount
    {
        get
        {
            int bytes;
            bool catalogued = DefinitionId is { } enemy
                ? EnemyTileSetDefinitions.ByteCounts.TryGetValue(enemy, out bytes)
                : EnemyTileSetDefinitions.UnplacedByteCounts.TryGetValue(UnplacedArtwork, out bytes);
            return catalogued ? bytes
                : throw new InvalidDataException($"Graphics-set record {Name} names a tile set the import does not catalogue.");
        }
    }

    /// <summary>The fixed staging block, 0 through 3, the record uploads its tiles to; null when they follow the previous record's.</summary>
    internal int? FixedBlock
    {
        get
        {
            int block;
            bool fixedBlock = DefinitionId is { } enemy
                ? EnemyTileSetDefinitions.FixedBlocks.TryGetValue(enemy, out block)
                : EnemyTileSetDefinitions.UnplacedFixedBlocks.TryGetValue(UnplacedArtwork, out block);
            return fixedBlock ? block : null;
        }
    }

    /// <summary>The installed tiles the record uploads, from <paramref name="artwork"/>.</summary>
    /// <exception cref="InvalidDataException">The artwork holds no tile set for the record.</exception>
    internal ReadOnlyMemory<byte> TilesIn(EnemyTileSourceArtwork artwork) =>
        DefinitionId is { } enemy ? Ensure.NotNull(artwork).TilesOf(enemy) : Ensure.NotNull(artwork).UnplacedTilesOf(UnplacedArtwork);

    /// <summary>The installed colours the record loads, from <paramref name="artwork"/>.</summary>
    /// <exception cref="InvalidDataException">The artwork holds no palette for the record.</exception>
    internal EnemyPaletteSheet PaletteIn(EnemyTileSourceArtwork artwork) =>
        DefinitionId is { } enemy ? Ensure.NotNull(artwork).PaletteOf(enemy) : Ensure.NotNull(artwork).UnplacedPaletteOf(UnplacedArtwork);

    /// <summary>The unplaced artwork of a record that names no enemy.</summary>
    /// <exception cref="InvalidOperationException">The record names neither, as a default value does.</exception>
    private UnplacedEnemyArtwork UnplacedArtwork =>
        Unplaced ?? throw new InvalidOperationException("A graphics-set record names neither an enemy nor unplaced artwork.");
}
