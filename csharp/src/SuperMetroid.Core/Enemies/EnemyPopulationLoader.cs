using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Room enemy population loading: the bank-$B4 graphics set and its uploads, the bank-$A1 population and kill
/// quota, and resolution of enemy definitions, populations and graphics sets from the room state.
/// </summary>
internal sealed class EnemyPopulationLoader
{
    /// <summary>Supplies the selected enemy artwork and guards room-bound operations.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Receives the boss identity declared by an initialized population entry.</summary>
    private readonly EnemyRoomStatus _roomStatus;
    /// <summary>Holds the 32 native enemy slots and their published occupied range.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies enemy definitions and runs each slot's initialization AI.</summary>
    private readonly IEnemyDispatch _dispatch;

    /// <summary>Binds room artwork, status, slot storage, and AI dispatch for a population load.</summary>
    internal EnemyPopulationLoader(EnemyRoomBinding room, EnemyRoomStatus roomStatus, EnemySlotTable slotTable, IEnemyDispatch dispatch)
    {
        _room = room;
        _roomStatus = roomStatus;
        _slotTable = slotTable;
        _dispatch = dispatch;
    }
    /// <summary>Resolved graphics records in room-set order, used for DMA scheduling and per-enemy tile/palette indexes.</summary>
    internal readonly List<RoomEnemyGraphicsSetEntry> _graphicsSet = new();

    /// <summary>Bank-$A1 population identity supplied by the current room state, matching native EnemyPopulationPointer ($07CF).</summary>
    public ushort PopulationPointer { get; internal set; }
    /// <summary>Bank-$B4 enemy graphics-set identity supplied by the current room state, matching native EnemySetPointer ($07D1); an empty population does not load that set.</summary>
    public ushort TilesetPointer { get; internal set; }
    /// <summary>Required room kill count from the nonempty population's terminator for enemy-clear triggers; native empty-population initialization retains the previous quota.</summary>
    public byte DeathQuota { get; private set; }

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
        foreach (RoomEnemyGraphicsSetEntry entry in _graphicsSet)
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
        ISnesAddressSpace bus,
        ushort tilesetPointer,
        SnesVram vram,
        SnesCgram cgram)
    {
        RoomEnemyGraphicsSetDefinition graphicsSet =
            ResolveRoomEnemyGraphicsSet(bus, tilesetPointer);
        int nextEnemyTileIndex = 0;
        int nextStagingOffset = RoomEnemyRomLayout.OrdinaryStagingOffset;

        foreach (RoomEnemyGraphicsSetHeader source in graphicsSet.Records.Span)
        {
            EnemyDefinitionId definitionPointer = source.DefinitionPointer;
            if (_graphicsSet.Count == RoomEnemySystem.MaximumGraphicsSetCount)
            {
                throw new InvalidDataException(
                    $"Enemy graphics set $B4:{tilesetPointer:X4} exceeds the native four-entry arrays.");
            }

            ushort vramDestination = source.VramDestination;
            // Retail enemy headers are immutable gameplay definitions. Graphics uploads
            // use the selected extracted artwork, but
            // the header itself must not be re-read from the cartridge on room entry.
            RoomEnemyDefinition definition = ResolveRoomEnemyDefinition(bus, definitionPointer);

            // ProcessEnemyTilesets copies one complete sixteen-color OBJ palette from the
            // enemy's selected code/data bank. Its assembly masks the entire low byte before
            // adding the native eight-row OBJ bias. Retail records use values zero through
            // seven, but retaining the wider mask makes corrupt data fail visibly.
            (_room.Artwork ?? throw new InvalidOperationException(
                "Enemy population requires installed palette artwork."))
                .LoadPaletteTo(definitionPointer, new CgramPalettes(cgram).ObjectRow(unchecked((byte)vramDestination)));

            int byteCount = definition.TileDataSize & 0x7fff;
            int stagingOffset = (definition.TileDataSize & 0x8000) != 0
                ? (vramDestination & 0x3000) >> 3
                : nextStagingOffset;
            int vramByteOffset = RoomEnemyRomLayout.VramByteBase + stagingOffset;
            if (vramByteOffset < 0 || stagingOffset + byteCount > new EnemyVram(vram).EnemyTiles.ByteCount)
            {
                throw new InvalidDataException(
                    $"Enemy ${(int)definitionPointer:X4} tile DMA would leave VRAM: " +
                    $"offset ${vramByteOffset:X4}, size ${byteCount:X4}.");
            }

            _room.Artwork.LoadTo(definitionPointer, byteCount, new EnemyVram(vram).EnemyTiles, stagingOffset);

            _graphicsSet.Add(new RoomEnemyGraphicsSetEntry(
                definitionPointer,
                vramDestination,
                unchecked((ushort)nextEnemyTileIndex),
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

    /// <summary>Initializes each bank-$A1 population slot, runs its AI, and publishes count and kill quota for nonempty populations.</summary>
    internal void LoadPopulation(
        ISnesAddressSpace bus,
        RoomEnemyPopulationDefinition populationDefinition,
        RoomLevelData? level,
        SamusState? samus,
        ushort controllerInput,
        ushort cameraX)
    {
        ReadOnlySpan<RoomEnemyPopulationRecord> records = populationDefinition.Records.Span;
        for (int slotIndex = 0; slotIndex < records.Length; slotIndex++)
        {
            RoomEnemyPopulationRecord population = records[slotIndex];
            EnemyDefinitionId definitionPointer = population.DefinitionPointer;
            if (slotIndex == RoomEnemySystem.MaximumEnemyCount)
            {
                throw new InvalidDataException(
                    $"Enemy population $A1:{populationDefinition.Pointer:X4} has more than 32 records.");
            }

            RoomEnemyDefinition definition = ResolveRoomEnemyDefinition(bus, definitionPointer);
            RoomEnemySlot slot = _slotTable._slots[slotIndex];
            _dispatch.InitializeSlotFromDefinition(slot, population, definition);
            if (definition.BossId != 0)
                _roomStatus.BossId = definition.BossId;
            _dispatch.RunInitializationAi(slot, level, samus, controllerInput, cameraX);

            // InitializeEnemies deliberately clears the init routine's immediate map.
            // Disable-Samus-collision actors receive the canonical empty map until their
            // first instruction-list tick replaces it. Gunship properties contain $2000.
            slot.SpritemapPointer = slot.Properties.HasAny(EnemyProperties.ProcessInstructions)
                ? slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap)
                    ? (ushort)32847
                    : CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap
                : (ushort)0;

        }

        // InitializeEnemies returns early for an initially empty population. It has
        // already zeroed both enemy counters, but does not rewrite the previous
        // first-free index or death quota. Nonempty populations publish these after
        // all initialization callbacks, exactly as the native terminator path does.
        if (records.IsEmpty)
            return;
        DeathQuota = populationDefinition.DeathQuota;
        _slotTable.EnemyCount = unchecked((ushort)records.Length);
        _slotTable.FirstFreeEnemyIndex = unchecked((ushort)(records.Length * RoomEnemySystem.NativeSlotSize));
    }

    /// <summary>Finds the room-set tile and OBJ palette indexes, falling back to standard sprite tile zero and palette five.</summary>
    internal (ushort TileIndex, ushort PaletteIndex) FindGraphicsIndexes(EnemyDefinitionId definitionPointer)
    {
        foreach (RoomEnemyGraphicsSetEntry entry in _graphicsSet)
        {
            if (entry.DefinitionPointer == definitionPointer)
            {
                return (
                    entry.VramTilesIndex,
                    unchecked((ushort)((entry.VramDestination & 0x000f) << 9)));
            }
        }

        // LoadEnemyGfxIndexes uses standard sprite tile zero and OBJ palette five when an
        // actor has no room graphics-set record. That fallback supports invisible/control
        // enemies without synthesizing an asset association.
        return (0, 2560);
    }

    /// <summary>Uses compiled retail definitions unless a constructed test bus explicitly supplies fixtures.</summary>
    internal static RoomEnemyDefinition ResolveRoomEnemyDefinition(
        ISnesAddressSpace bus, EnemyDefinitionId pointer)
    {
        if (bus is IRoomEnemyFixtureSource fixture)
            return fixture.ReadEnemyDefinition(pointer);
        return RoomEnemyAuxiliaryDefinitionCatalog.TryGet(pointer, out RoomEnemyDefinition auxiliary)
            ? auxiliary
            : RoomEnemyDefinitionCatalog.Get(pointer);
    }

    /// <summary>Uses constructed bus fixtures when present, otherwise the compiled bank-$A1 population catalog.</summary>
    internal static RoomEnemyPopulationDefinition ResolveRoomEnemyPopulation(
        ISnesAddressSpace bus, ushort pointer) =>
        bus is IRoomEnemyFixtureSource fixture
            ? fixture.ReadEnemyPopulation(pointer)
            : RoomEnemyPopulationDefinitions.Get(pointer);

    /// <summary>Uses a constructed graphics-set fixture when present, otherwise the compiled bank-$B4 room set.</summary>
    private static RoomEnemyGraphicsSetDefinition ResolveRoomEnemyGraphicsSet(
        ISnesAddressSpace bus, ushort pointer) =>
        bus is IRoomEnemyFixtureSource fixture
            ? fixture.ReadEnemyGraphicsSet(pointer)
            : RoomEnemyGraphicsSetDefinitions.Get(pointer);
}
