using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// One record of a room's enemy graphics set, in the set's order: the enemy whose artwork the set loads, or artwork the set
/// loads for no enemy of the game (<see cref="UnplacedEnemyArtwork"/>, #1686), which only lands where the set's other
/// pictures draw from. Which of the two it loads is one closed value (#2560), so a record cannot name neither or both.
/// </summary>
/// <remarks>
/// The colours a member draws in are its installed sheet's (#1512) and its pictures are its sheet's frames (#1513): the
/// import composes and pre-colours each member's sheets as its graphics set loads it, so a member names nothing else.
/// </remarks>
internal sealed record RoomEnemyGraphicsSetMember
{
    /// <summary>A record loading <paramref name="definitionId"/>'s artwork.</summary>
    /// <param name="definitionId">The member enemy, which selects its installed sheets and its spawned objects' sheet.</param>
    internal RoomEnemyGraphicsSetMember(EnemyDefinitionId definitionId) => Source = new GraphicsSetEnemy(definitionId);

    /// <summary>A record loading <paramref name="artwork"/>, which no enemy of the game draws.</summary>
    internal RoomEnemyGraphicsSetMember(UnplacedEnemyArtwork artwork) => Source = new GraphicsSetUnplacedArtwork(artwork);

    /// <summary>What the record loads: an enemy's artwork, or artwork for no enemy (#2560).</summary>
    internal RoomEnemyGraphicsSetSource Source { get; }

    /// <summary>The member enemy; null for a record loading artwork for no enemy, which has none.</summary>
    internal EnemyDefinitionId? DefinitionId => Source is GraphicsSetEnemy enemy ? enemy.DefinitionId : null;

    /// <summary>The artwork the record loads for no enemy; null for an enemy's record, whose artwork is its enemy's.</summary>
    internal UnplacedEnemyArtwork? Unplaced => Source is GraphicsSetUnplacedArtwork unplaced ? unplaced.Artwork : null;

    /// <summary>The record's name in diagnostics: its enemy's, or its unplaced artwork's.</summary>
    internal string Name => Source.Name;

    /// <summary>The bytes of the tile set the record uploads.</summary>
    /// <exception cref="InvalidDataException">The import does not catalogue the record's tile set.</exception>
    internal int TileByteCount => Source.TileByteCount ??
        throw new InvalidDataException($"Graphics-set record {Name} names a tile set the import does not catalogue.");

    /// <summary>The fixed staging block, 0 through 3, the record uploads its tiles to; null when they follow the previous record's.</summary>
    internal int? FixedBlock => Source.FixedBlock;

    /// <summary>The installed tiles the record uploads, from <paramref name="artwork"/>.</summary>
    /// <exception cref="InvalidDataException">The artwork holds no tile set for the record.</exception>
    internal ReadOnlyMemory<byte> TilesIn(EnemyTileSourceArtwork artwork) => Source.TilesIn(Ensure.NotNull(artwork));

    /// <summary>The installed colours the record loads, from <paramref name="artwork"/>.</summary>
    /// <exception cref="InvalidDataException">The artwork holds no palette for the record.</exception>
    internal EnemyPaletteSheet PaletteIn(EnemyTileSourceArtwork artwork) => Source.PaletteIn(Ensure.NotNull(artwork));
}
