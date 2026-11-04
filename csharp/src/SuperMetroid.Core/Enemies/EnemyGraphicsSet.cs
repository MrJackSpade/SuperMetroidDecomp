using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The room's enemy graphics set: the staged tiles, their uploads, and each enemy's tile index. The colours each member
/// draws in are its installed sheets' (#1512), which the import pre-colours as the set loads them.
/// </summary>
internal sealed class EnemyGraphicsSet
{
    /// <summary>Maximum four enemy graphics-set entries accepted by the room tileset arrays.</summary>
    public const int MaximumGraphicsSetCount = 4;

    /// <summary>Supplies the selected enemy artwork and guards room-bound operations.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Binds the room whose artwork the graphics set stages.</summary>
    internal EnemyGraphicsSet(EnemyRoomBinding room) => _room = room;

    /// <summary>Resolved graphics records in room-set order, used for tile-upload scheduling and per-enemy tile indexes.</summary>
    internal readonly List<RoomEnemyGraphicsSetEntry> Entries = new();

    /// <summary>
    /// The graphics set whose tiles and palettes the room draws under, which selects its installed sheets: the one it
    /// loaded, or, when its population is empty and it loads none, the standard sprites alone
    /// (<see cref="RoomEnemyGraphicsSetDefinitions.SessionStart"/>).
    /// </summary>
    internal RoomEnemyGraphicsSetDefinition LoadedSet { get; private set; } = RoomEnemyGraphicsSetDefinitions.SessionStart;

    /// <summary>
    /// Shows the set's members as the room loaded them (#1406, #1463, #1513): every frame shows the picture its room's load
    /// gives it until one of the room's encounters plays its artwork sequence, so none of the new room's has started.
    /// </summary>
    public void ShowGraphics()
    {
        _room.EnsureLoaded();
        _room.Session.ArtworkProgress.Restart();
    }

    /// <summary>The bytes of the enemy tile area, through the end of the sprites' tile memory.</summary>
    private const int EnemyTileAreaByteCount = EnemyTileDataMasks.EnemyTileAreaByteCount;

    /// <summary>
    /// Where the set's first member that follows the previous one shows its tiles: into the enemy tile area, after the
    /// share the fixed sprite set keeps.
    /// </summary>
    private const int OrdinaryTilesStart = EnemyTileDataMasks.OrdinaryTilesStart;

    /// <summary>
    /// Stages selected enemy tiles in graphics-set record order. The colours the set loads for each member are its
    /// installed sheets' own (#1512); loading copies none.
    /// </summary>
    internal void LoadGraphicsSet(IRoomEnemyDefinitionSource definitions, RoomEnemyGraphicsSetDefinition graphicsSet)
    {
        LoadedSet = graphicsSet;
        Entries.AddRange(LayOut(definitions, graphicsSet));
    }

    /// <summary>
    /// The entries <paramref name="graphicsSet"/> loads, in record order: each enemy's tiles, where in the enemy tile area
    /// they upload to and the OBJ character index they start at.
    /// </summary>
    /// <remarks>
    /// A member shown at a fixed block ends every retail set. The game adds the header's fixed-block flag into the
    /// counters the following members would start from; no retail set has a following member, so that sum is never
    /// observed, and a set that would observe it is rejected rather than reproduced (#1412).
    /// </remarks>
    /// <exception cref="InvalidDataException">
    /// The set has more than four entries, places a member after one shown at a fixed block, or an entry's tiles leave the
    /// enemy tile area.
    /// </exception>
    internal static IReadOnlyList<RoomEnemyGraphicsSetEntry> LayOut(IRoomEnemyDefinitionSource definitions,
        RoomEnemyGraphicsSetDefinition graphicsSet)
    {
        Ensure.NotNull(definitions);
        Ensure.NotNull(graphicsSet);
        var entries = new List<RoomEnemyGraphicsSetEntry>();
        int nextEnemyTileIndex = 0;
        int nextStagingOffset = OrdinaryTilesStart;
        bool followsFixedBlock = false;

        foreach (RoomEnemyGraphicsSetHeader source in graphicsSet.Records.Span)
        {
            EnemyDefinitionId enemy = source.DefinitionId;
            if (entries.Count == EnemyGraphicsSet.MaximumGraphicsSetCount)
            {
                throw new InvalidDataException(
                    $"Enemy graphics set {graphicsSet.Name} has more than the four entries a room's enemy graphics can hold.");
            }
            if (followsFixedBlock)
            {
                throw new InvalidDataException(
                    $"Enemy graphics set {graphicsSet.Name} places {enemy} after a member shown at a fixed block.");
            }

            // Retail enemy headers are immutable gameplay definitions. Graphics uploads
            // use the selected extracted artwork, but
            // the header itself must not be re-read from the game on room entry.
            RoomEnemyDefinition definition = definitions.ReadEnemyDefinition(enemy);

            int byteCount = definition.TileByteCount;
            bool atFixedBlock = definition.TilePlacement == EnemyTilePlacement.AtFixedBlock;
            int stagingOffset = atFixedBlock
                ? source.StagingBlock * EnemyTileDataMasks.StagingBlockByteCount
                : nextStagingOffset;
            if (stagingOffset + byteCount > EnemyTileAreaByteCount)
            {
                throw new InvalidDataException(
                    $"Enemy {enemy}'s tiles would leave the enemy tile area: offset {stagingOffset}, size {byteCount}.");
            }

            entries.Add(new RoomEnemyGraphicsSetEntry(
                enemy,
                nextEnemyTileIndex,
                stagingOffset,
                byteCount));

            followsFixedBlock = atFixedBlock;
            nextEnemyTileIndex += byteCount / EnemyTileDataMasks.TileByteCount;
            nextStagingOffset += byteCount;
        }
        return entries;
    }

    /// <summary>
    /// The sheet characters <paramref name="enemy"/> draws with in this set, in its sheet's standard colours: its
    /// record's, or standard character zero for an enemy the set does not load, with the character index its
    /// initialization copies from the enemy it follows, or fixes, in place of the set's where it does.
    /// </summary>
    internal SpriteGraphics DrawIndexes(EnemyDefinitionId enemy)
    {
        SpriteGraphics indexes = FindGraphicsIndexes(enemy);
        if (EnemyTileBaseDefinitions.DrawsAtTilesOf.TryGetValue(enemy, out EnemyDefinitionId tileOwner))
            indexes = indexes with { TileBase = FindGraphicsIndexes(tileOwner).TileBase };
        return indexes with { TileBase = EnemyTileBaseDefinitions.Fixed.GetValueOrDefault(enemy, indexes.TileBase) };
    }

    /// <summary>
    /// Finds the room-set characters the set loads for the enemy, falling back to standard sprite character zero, in
    /// the standard colours of the sheet the import composed for the enemy under this set (#1512).
    /// </summary>
    internal SpriteGraphics FindGraphicsIndexes(EnemyDefinitionId definitionId)
    {
        foreach (RoomEnemyGraphicsSetEntry entry in Entries)
        {
            if (entry.DefinitionId == definitionId)
                return new SpriteGraphics(entry.TileIndexBase, PictureShading.Standard);
        }

        // LoadEnemyGfxIndexes uses standard sprite character zero when an actor has no room graphics-set record, which
        // supports invisible/control enemies without synthesizing an asset association; the import colours such an
        // enemy's sheet as the game's fallback does.
        return SpriteGraphics.Emptied;
    }
}
