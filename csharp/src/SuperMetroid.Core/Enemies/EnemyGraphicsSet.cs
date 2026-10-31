using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies;

/// <summary>The room's bank-$B4 enemy graphics set: the staged tiles and palettes, their uploads, and each enemy's tile and palette indexes.</summary>
internal sealed class EnemyGraphicsSet
{
    /// <summary>Maximum four enemy graphics-set entries accepted by the native room tileset arrays.</summary>
    public const int MaximumGraphicsSetCount = 4;

    /// <summary>Supplies the selected enemy artwork and guards room-bound operations.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Binds the room whose artwork the graphics set stages.</summary>
    internal EnemyGraphicsSet(EnemyRoomBinding room)
    {
        _room = room;
        Placement = GraphicsSetColourPlacement.Of(LoadedSet, Entries);
    }

    /// <summary>Resolved graphics records in room-set order, used for DMA scheduling and per-enemy tile/palette indexes.</summary>
    internal readonly List<RoomEnemyGraphicsSetEntry> Entries = new();

    /// <summary>
    /// The graphics set whose tiles and palettes the room draws under, which selects its installed sheets: the one it
    /// loaded, or, when its population is empty and it loads none, the standard sprites alone
    /// (<see cref="RoomEnemyGraphicsSetDefinitions.SessionStart"/>).
    /// </summary>
    internal RoomEnemyGraphicsSetDefinition LoadedSet { get; private set; } = RoomEnemyGraphicsSetDefinitions.SessionStart;

    /// <summary>
    /// Where the set's latest load placed the sprite colourings drawn under it (#1464): a record taken when the set loads
    /// or is inherited, which draws carry as they are published, so no draw reaches the set's live entries.
    /// </summary>
    internal GraphicsSetColourPlacement Placement { get; private set; }


    /// <summary>
    /// Shows the set's members' looks, after the standard sprites room setup selected (#1406, #1463): each entry's look as
    /// the room loaded it, in set order.
    /// </summary>
    /// <remarks>
    /// The members' looks are shown after the standard sprites, as LoadEnemyTileData's copies follow the standard room
    /// graphics; the cartridge queues them before any enemy AI's own transfer, which these showings also precede.
    /// </remarks>
    public void ShowGraphics()
    {
        _room.EnsureLoaded();
        foreach (RoomEnemyGraphicsSetEntry entry in Entries)
            _room.Session.SpriteLooks.Show(new GraphicsSetTileCharacters(entry));
    }

    /// <summary>The bytes of the enemy tile area, through the end of the sprites' tile memory.</summary>
    private const int EnemyTileAreaByteCount = EnemyTileDataMasks.EnemyTileAreaByteCount;

    /// <summary>
    /// Where the set's first member that follows the previous one shows its tiles: into the enemy tile area, after the
    /// share the fixed sprite set keeps.
    /// </summary>
    private const int OrdinaryTilesStart = EnemyTileDataMasks.OrdinaryTilesStart;

    /// <summary>Stages selected enemy tiles and OBJ palettes in graphics-set record order.</summary>
    internal void LoadGraphicsSet(
        IRoomEnemyDefinitionSource definitions,
        RoomEnemyGraphicsSetDefinition graphicsSet,
        IColourSets sets)
    {
        LoadedSet = graphicsSet;
        foreach (RoomEnemyGraphicsSetEntry entry in LayOut(definitions, graphicsSet))
        {
            // ProcessEnemyTilesets copies one complete sixteen-colour palette from the enemy's selected code/data bank
            // into the shared store the record names.
            (_room.Artwork ?? throw new InvalidOperationException(
                "Enemy population requires installed palette artwork."))
                .LoadPaletteTo(entry.DefinitionId, entry.ColourStore.In(sets));
            Entries.Add(entry);
        }
        Placement = GraphicsSetColourPlacement.Of(LoadedSet, Entries);
    }

    /// <summary>
    /// The entries <paramref name="graphicsSet"/> loads, in record order: each enemy's tiles, where in the enemy tile area
    /// they upload to and the OBJ character index they start at.
    /// </summary>
    /// <remarks>
    /// A member shown at a fixed block ends every retail set. The cartridge adds the header's fixed-block flag into the
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
                    $"Enemy graphics set {graphicsSet.Name} exceeds the native four-entry arrays.");
            }
            if (followsFixedBlock)
            {
                throw new InvalidDataException(
                    $"Enemy graphics set {graphicsSet.Name} places {enemy} after a member shown at a fixed block.");
            }

            // Retail enemy headers are immutable gameplay definitions. Graphics uploads
            // use the selected extracted artwork, but
            // the header itself must not be re-read from the cartridge on room entry.
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
                source.ColourStore,
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
    /// The sheet characters and colours <paramref name="enemy"/> draws with in this set: its record's, or the standard
    /// fallback for an enemy the set does not load, with the character index and colours its initialization copies
    /// from the enemy it follows, or fixes, in place of the set's where it does.
    /// </summary>
    internal SpriteGraphics DrawIndexes(EnemyDefinitionId enemy)
    {
        SpriteGraphics indexes = FindGraphicsIndexes(enemy);
        if (EnemyTileBaseDefinitions.DrawsAtTilesOf.TryGetValue(enemy, out EnemyDefinitionId tileOwner))
            indexes = indexes with { TileBase = FindGraphicsIndexes(tileOwner).TileBase };
        if (EnemyTileBaseDefinitions.DrawsInPaletteOf.TryGetValue(enemy, out EnemyDefinitionId paletteOwner))
            indexes = indexes.In(FindGraphicsIndexes(paletteOwner).Colouring);
        return indexes with { TileBase = EnemyTileBaseDefinitions.Fixed.GetValueOrDefault(enemy, indexes.TileBase) };
    }

    /// <summary>
    /// Finds the room-set characters and the colours the set loads for the enemy, falling back to standard sprite
    /// character zero and the enemy projectiles' colours.
    /// </summary>
    internal SpriteGraphics FindGraphicsIndexes(EnemyDefinitionId definitionId)
    {
        foreach (RoomEnemyGraphicsSetEntry entry in Entries)
        {
            if (entry.DefinitionId == definitionId)
                return new SpriteGraphics(entry.TileIndexBase, new GraphicsSetMemberColouring(entry.DefinitionId));
        }

        // LoadEnemyGfxIndexes uses standard sprite character zero and palette row five, the enemy projectiles'
        // colours, when an actor has no room graphics-set record. That fallback supports invisible/control
        // enemies without synthesizing an asset association.
        return SpriteGraphics.CharacterZeroIn(SharedSpriteColouring.EnemyProjectiles);
    }
}
