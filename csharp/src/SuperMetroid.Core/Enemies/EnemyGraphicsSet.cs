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

    /// <summary>The graphics set the room loaded, which selects its enemies' installed sheets.</summary>
    internal RoomEnemyGraphicsSetDefinition? LoadedSet { get; private set; }

    /// <summary>The loaded set's record for <paramref name="enemy"/>.</summary>
    /// <exception cref="InvalidDataException">The room's graphics set does not load that enemy.</exception>
    internal RoomEnemyGraphicsSetEntry Entry(EnemyDefinitionId enemy)
    {
        foreach (RoomEnemyGraphicsSetEntry entry in Entries)
        {
            if (entry.DefinitionId == enemy)
                return entry;
        }
        throw new InvalidDataException($"Enemy graphics set {LoadedSet?.Name ?? "(none loaded)"} does not load {enemy}.");
    }


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
        int nextEnemyTileIndex = 0;
        int nextStagingOffset = RoomEnemyRomLayout.OrdinaryStagingOffset;

        foreach (RoomEnemyGraphicsSetHeader source in graphicsSet.Records.Span)
        {
            EnemyDefinitionId definitionPointer = source.DefinitionId;
            if (Entries.Count == EnemyGraphicsSet.MaximumGraphicsSetCount)
            {
                throw new InvalidDataException(
                    $"Enemy graphics set {graphicsSet.Name} exceeds the native four-entry arrays.");
            }

            // Retail enemy headers are immutable gameplay definitions. Graphics uploads
            // use the selected extracted artwork, but
            // the header itself must not be re-read from the cartridge on room entry.
            RoomEnemyDefinition definition = definitions.ReadEnemyDefinition(definitionPointer);

            // ProcessEnemyTilesets copies one complete sixteen-color OBJ palette from the
            // enemy's selected code/data bank. Its assembly masks the entire low byte before
            // adding the native eight-row OBJ bias. Retail records use values zero through
            // seven, but retaining the wider mask makes corrupt data fail visibly.
            (_room.Artwork ?? throw new InvalidOperationException(
                "Enemy population requires installed palette artwork."))
                .LoadPaletteTo(definitionPointer, new CgramPalettes(cgram).ObjectRow(checked((byte)source.PaletteRow)));

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


            Entries.Add(new RoomEnemyGraphicsSetEntry(
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
