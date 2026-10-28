using SuperMetroid.Core.Enemies.Drawing;
﻿using SuperMetroid.Core.Enemies.Artwork;
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
    internal EnemyGraphicsSet(EnemyRoomBinding room) => _room = room;

    /// <summary>Resolved graphics records in room-set order, used for DMA scheduling and per-enemy tile/palette indexes.</summary>
    internal readonly List<RoomEnemyGraphicsSetEntry> Entries = new();

    /// <summary>
    /// The graphics set whose tiles and palettes the room draws under, which selects its installed sheets: the one it
    /// loaded, or, when its population is empty and it loads none, the one it inherited.
    /// </summary>
    internal RoomEnemyGraphicsSetDefinition LoadedSet { get; private set; } = RoomEnemyGraphicsSetDefinitions.SessionStart;

    /// <summary>Draws under the set the outgoing room left loaded, until this room loads its own.</summary>
    /// <param name="graphicsSet">The outgoing room's loaded set.</param>
    internal void Inherit(RoomEnemyGraphicsSetDefinition graphicsSet) => LoadedSet = Ensure.NotNull(graphicsSet);


    /// <summary>
    /// Shows the set's enemy characters over the OBJ character table, after the standard sprite characters room setup
    /// selected (#1406): each entry's installed tiles from its place in the enemy tile area, in set order.
    /// </summary>
    /// <remarks>
    /// Standard sprite graphics cover the beginning of the enemy tile area, so the enemies' characters are shown after
    /// them, as LoadEnemyTileData's copies follow the standard room graphics; the cartridge queues them before any enemy
    /// AI's own transfer, which these records also precede.
    /// </remarks>
    /// <exception cref="InvalidDataException">An entry's tiles are not whole characters from a character's start.</exception>
    public void ShowGraphics()
    {
        _room.EnsureLoaded();
        foreach (RoomEnemyGraphicsSetEntry entry in Entries)
        {
            if (entry.StagingOffset % SpriteArtworkPlacements.CharacterByteCount != 0 || entry.TileByteCount % SpriteArtworkPlacements.CharacterByteCount != 0)
                throw new InvalidDataException($"{entry.DefinitionId}'s tiles are not whole sprite characters from a character's start.");
            _room.Session.SpriteCharacters.Show(new SpriteArtworkPlacement(new GraphicsSetTileCharacters(entry.DefinitionId),
                EnemyTileAreaFirstCharacter + entry.StagingOffset / SpriteArtworkPlacements.CharacterByteCount,
                entry.TileByteCount / SpriteArtworkPlacements.CharacterByteCount));
        }
    }

    /// <summary>The sprite character the enemy tile area begins at: the first after the standard sprites' area.</summary>
    internal const int EnemyTileAreaFirstCharacter = 192;

    /// <summary>The bytes of the enemy tile area: 320 characters, through the end of the OBJ character table.</summary>
    private const int EnemyTileAreaByteCount = 320 * SpriteArtworkPlacements.CharacterByteCount;

    /// <summary>
    /// Where the set's first member that follows the previous one shows its tiles: 64 characters into the enemy tile
    /// area, after the share the fixed sprite set keeps.
    /// </summary>
    private const int OrdinaryTilesStart = 64 * SpriteArtworkPlacements.CharacterByteCount;

    /// <summary>Stages selected enemy tiles and OBJ palettes in graphics-set record order.</summary>
    internal void LoadGraphicsSet(
        IRoomEnemyDefinitionSource definitions,
        RoomEnemyGraphicsSetDefinition graphicsSet,
        ISpriteColourSets sets)
    {
        LoadedSet = graphicsSet;
        foreach (RoomEnemyGraphicsSetEntry entry in LayOut(definitions, graphicsSet))
        {
            // ProcessEnemyTilesets copies one complete sixteen-color OBJ palette from the
            // enemy's selected code/data bank. Its assembly masks the entire low byte before
            // adding the native eight-row OBJ bias. Retail records use values zero through
            // seven, but retaining the wider mask makes corrupt data fail visibly.
            (_room.Artwork ?? throw new InvalidOperationException(
                "Enemy population requires installed palette artwork."))
                .LoadPaletteTo(entry.DefinitionId, sets.Set(entry.PaletteRow));
            Entries.Add(entry);
        }
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
                source.PaletteRow,
                nextEnemyTileIndex,
                stagingOffset,
                byteCount));

            followsFixedBlock = atFixedBlock;
            nextEnemyTileIndex += byteCount / SpriteArtworkPlacements.CharacterByteCount;
            nextStagingOffset += byteCount;
        }
        return entries;
    }

    /// <summary>
    /// The sheet characters and palette row <paramref name="enemy"/> draws with in this set: its record's, or the
    /// standard fallback for an enemy the set does not load, with the character index and palette its initialization
    /// copies from the enemy it follows, or fixes, in place of the set's where it does.
    /// </summary>
    internal SpriteGraphics DrawIndexes(EnemyDefinitionId enemy)
    {
        SpriteGraphics indexes = FindGraphicsIndexes(enemy);
        if (EnemyTileBaseDefinitions.DrawsAtTilesOf.TryGetValue(enemy, out EnemyDefinitionId tileOwner))
            indexes = indexes with { TileBase = FindGraphicsIndexes(tileOwner).TileBase };
        if (EnemyTileBaseDefinitions.DrawsInPaletteOf.TryGetValue(enemy, out EnemyDefinitionId paletteOwner))
            indexes = indexes with { PaletteRow = FindGraphicsIndexes(paletteOwner).PaletteRow };
        return indexes with { TileBase = EnemyTileBaseDefinitions.Fixed.GetValueOrDefault(enemy, indexes.TileBase) };
    }

    /// <summary>Finds the room-set characters and palette row, falling back to standard sprite character zero and palette row five.</summary>
    internal SpriteGraphics FindGraphicsIndexes(EnemyDefinitionId definitionId)
    {
        foreach (RoomEnemyGraphicsSetEntry entry in Entries)
        {
            if (entry.DefinitionId == definitionId)
                return SpriteGraphics.Of(entry.TileIndexBase, entry.PaletteRow & EnemyTileDataMasks.PaletteNibbleBits);
        }

        // LoadEnemyGfxIndexes uses standard sprite character zero and palette row five when an
        // actor has no room graphics-set record. That fallback supports invisible/control
        // enemies without synthesizing an asset association.
        return SpriteGraphics.InPalette(SpriteColourSetDefinitions.EnemyProjectiles);
    }
}
