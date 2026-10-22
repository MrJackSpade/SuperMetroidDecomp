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
    /// Appends enemy tile DMAs after the standard OBJ transfer queued by room setup.
    /// </summary>
    /// <remarks>
    /// Standard sprite graphics occupy VRAM bytes $C000-$EDFF and therefore overlap the
    /// beginning of the enemy region at $E000. ProcessEnemyTilesets stages enemy art first,
    /// but LoadEnemyTileData schedules its final VRAM copies after standard room graphics.
    /// A direct host load alone would be overwritten by the next accepted NMI.
    /// </remarks>
    /// <param name="queue">Ordered NMI VRAM-write queue to which selected enemy character transfers are appended.</param>
    public void QueueGraphicsUploads(VramWriteQueue queue)
    {
        Ensure.NotNull(queue);
        _room.EnsureLoaded();
        foreach (RoomEnemyGraphicsSetEntry entry in Entries)
        {
            if ((entry.StagingOffset & 1) != 0 || entry.TileByteCount > ushort.MaxValue)
                throw new InvalidDataException("Enemy tile DMA is not word-aligned or exceeds one native transfer.");
            queue.Enqueue(
                unchecked((ushort)entry.TileByteCount),
                entry.Definition.TileDataAddress,
                EnemyVram.EnemyTilesDma(entry.StagingOffset / 2));
        }
    }

    /// <summary>Stages selected enemy tiles and OBJ palettes in bank-$B4 record order while retaining native size arithmetic.</summary>
    internal void LoadGraphicsSet(
        IRoomEnemyDefinitionSource definitions,
        RoomEnemyGraphicsSetDefinition graphicsSet,
        SnesVram vram,
        IPaletteColors cgram)
    {
        LoadedSet = graphicsSet;
        foreach (RoomEnemyGraphicsSetEntry entry in LayOut(definitions, graphicsSet, vram))
        {
            // ProcessEnemyTilesets copies one complete sixteen-color OBJ palette from the
            // enemy's selected code/data bank. Its assembly masks the entire low byte before
            // adding the native eight-row OBJ bias. Retail records use values zero through
            // seven, but retaining the wider mask makes corrupt data fail visibly.
            (_room.Artwork ?? throw new InvalidOperationException(
                "Enemy population requires installed palette artwork."))
                .LoadPaletteTo(entry.DefinitionId, new CgramPalettes(cgram).ObjectRow(checked((byte)entry.PaletteRow)));
            Entries.Add(entry);
        }
    }

    /// <summary>
    /// The entries <paramref name="graphicsSet"/> loads, in bank-$B4 record order: each enemy's tiles, where in the enemy
    /// tile area they upload to and the OBJ character index they start at, with the native size arithmetic retained.
    /// </summary>
    /// <exception cref="InvalidDataException">The set has more than four entries, or an entry's tiles leave the enemy tile area.</exception>
    internal static IReadOnlyList<RoomEnemyGraphicsSetEntry> LayOut(IRoomEnemyDefinitionSource definitions,
        RoomEnemyGraphicsSetDefinition graphicsSet, SnesVram vram)
    {
        Ensure.NotNull(definitions);
        Ensure.NotNull(graphicsSet);
        var entries = new List<RoomEnemyGraphicsSetEntry>();
        int nextEnemyTileIndex = 0;
        int nextStagingOffset = RoomEnemyRomLayout.OrdinaryStagingOffset;

        foreach (RoomEnemyGraphicsSetHeader source in graphicsSet.Records.Span)
        {
            EnemyDefinitionId definitionPointer = source.DefinitionId;
            if (entries.Count == EnemyGraphicsSet.MaximumGraphicsSetCount)
            {
                throw new InvalidDataException(
                    $"Enemy graphics set {graphicsSet.Name} exceeds the native four-entry arrays.");
            }

            // Retail enemy headers are immutable gameplay definitions. Graphics uploads
            // use the selected extracted artwork, but
            // the header itself must not be re-read from the cartridge on room entry.
            RoomEnemyDefinition definition = definitions.ReadEnemyDefinition(definitionPointer);

            int byteCount = definition.TileDataSize & EnemyTileDataMasks.ByteCountBits;
            int stagingOffset = (definition.TileDataSize & EnemyTileDataMasks.FixedStagingBit) != 0
                ? source.StagingBlock * EnemyTileDataMasks.StagingBlockByteCount
                : nextStagingOffset;
            int vramByteOffset = RoomEnemyRomLayout.VramByteBase + stagingOffset;
            if (vramByteOffset < 0 || stagingOffset + byteCount > new EnemyVram(vram).EnemyTiles.ByteCount)
            {
                throw new InvalidDataException(
                    $"Enemy ${(int)definitionPointer:X4} tile DMA would leave VRAM: " +
                    $"offset ${vramByteOffset:X4}, size ${byteCount:X4}.");
            }


            entries.Add(new RoomEnemyGraphicsSetEntry(
                definitionPointer,
                source.PaletteRow,
                nextEnemyTileIndex,
                definition,
                stagingOffset,
                byteCount));

            // The original adds the unmasked size word for both counters. Retail ordinary
            // entries, including Landing Site, have bit 15 clear; retaining that arithmetic
            // makes malformed/high-bit records visible instead of silently normalized.
            nextEnemyTileIndex = unchecked((ushort)(nextEnemyTileIndex + (definition.TileDataSize >> 5)));
            nextStagingOffset = unchecked((ushort)(nextStagingOffset + definition.TileDataSize));
        }
        return entries;
    }

    /// <summary>
    /// The character index and OBJ palette bits <paramref name="enemy"/> draws with in this set: its record's, or the
    /// standard fallback for an enemy the set does not load, with the character index and palette its initialization
    /// copies from the enemy it follows, or fixes, in place of the set's where it does.
    /// </summary>
    internal EnemyGraphicsIndexes DrawIndexes(EnemyDefinitionId enemy)
    {
        EnemyGraphicsIndexes indexes = FindGraphicsIndexes(enemy);
        if (EnemyTileBaseDefinitions.DrawsAtTilesOf.TryGetValue(enemy, out EnemyDefinitionId tileOwner))
            indexes = indexes with { TileIndex = FindGraphicsIndexes(tileOwner).TileIndex };
        if (EnemyTileBaseDefinitions.DrawsInPaletteOf.TryGetValue(enemy, out EnemyDefinitionId paletteOwner))
            indexes = indexes with { PaletteIndex = FindGraphicsIndexes(paletteOwner).PaletteIndex };
        return indexes with { TileIndex = EnemyTileBaseDefinitions.Fixed.GetValueOrDefault(enemy, indexes.TileIndex) };
    }

    /// <summary>Finds the room-set tile and OBJ palette indexes, falling back to standard sprite tile zero and palette five.</summary>
    internal EnemyGraphicsIndexes FindGraphicsIndexes(EnemyDefinitionId definitionId)
    {
        foreach (RoomEnemyGraphicsSetEntry entry in Entries)
        {
            if (entry.DefinitionId == definitionId)
            {
                return new EnemyGraphicsIndexes(                    checked((ushort)entry.VramTilesIndex),
                    unchecked((ushort)((entry.PaletteRow & EnemyTileDataMasks.PaletteNibbleBits) << 9)));
            }
        }

        // LoadEnemyGfxIndexes uses standard sprite tile zero and OBJ palette five when an
        // actor has no room graphics-set record. That fallback supports invisible/control
        // enemies without synthesizing an asset association.
        return new EnemyGraphicsIndexes(0, 2560);
    }
}
