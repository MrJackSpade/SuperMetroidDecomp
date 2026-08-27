using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// ROM-backed room-enemy loader, scheduler, instruction interpreter, and draw queues for
/// the portions of bank $A0 currently exercised by normal Landing Site.
/// </summary>
/// <remarks>
/// This deliberately retains the cartridge's 32 fixed slots and its native slot offsets
/// ($0000, $0040, ... $07C0). Those offsets are data: enemy AI uses them for multi-part
/// actors, collision lists publish them, and the gunship bottom initializer even relies on
/// WRAM-array aliasing to copy fields from earlier slots. A compact host list would appear
/// cleaner while destroying behavior that the original code expects.
/// </remarks>
public sealed class RoomEnemySystem
{
    public const int MaximumEnemyCount = 32;
    public const int NativeSlotSize = 0x40;
    public const int MaximumGraphicsSetCount = 4;

    private const int EnemyDefinitionBank = 0xa00000;
    private const int EnemyPopulationBank = 0xa10000;
    private const int EnemyTilesetBank = 0xb40000;
    private const int EnemyVramByteBase = 0xd800;
    private const int OrdinaryEnemyStagingOffset = 0x0800;

    private readonly RoomEnemySlot[] _slots = new RoomEnemySlot[MaximumEnemyCount];
    private readonly List<ushort>[] _drawQueues =
        Enumerable.Range(0, 8).Select(_ => new List<ushort>()).ToArray();
    private readonly List<ushort> _activeEnemyIndexes = new();
    private readonly List<ushort> _interactiveEnemyIndexes = new();
    private readonly List<SolidEnemyCollisionBody> _interactiveCollisionBodies = new();
    private readonly List<RoomEnemyGraphicsSetEntry> _graphicsSet = new();
    private ISnesAddressSpace? _bus;

    public RoomEnemySystem()
    {
        for (int slotIndex = 0; slotIndex < _slots.Length; slotIndex++)
            _slots[slotIndex] = new RoomEnemySlot(slotIndex);
    }

    /// <summary>All 32 physical enemy slots, including unused zero-pointer slots.</summary>
    public IReadOnlyList<RoomEnemySlot> Slots => _slots;

    /// <summary>Native $40-byte slot offsets selected by the most recent activity scan.</summary>
    public IReadOnlyList<ushort> ActiveEnemyIndexes => _activeEnemyIndexes;

    /// <summary>Native slot offsets admitted to solid-enemy interaction.</summary>
    public IReadOnlyList<ushort> InteractiveEnemyIndexes => _interactiveEnemyIndexes;

    /// <summary>Collision words read from the current slots after their AI has run.</summary>
    public IReadOnlyList<SolidEnemyCollisionBody> InteractiveCollisionBodies =>
        _interactiveCollisionBodies;

    /// <summary>The room's terminated bank-$B4 graphics-set records.</summary>
    public IReadOnlyList<RoomEnemyGraphicsSetEntry> GraphicsSet => _graphicsSet;

    public ushort PopulationPointer { get; private set; }
    public ushort TilesetPointer { get; private set; }
    public ushort FirstFreeEnemyIndex { get; private set; }
    public ushort EnemyCount { get; private set; }
    public byte DeathQuota { get; private set; }
    public bool IsLoaded => _bus is not null;

    /// <summary>
    /// Ports the data-producing parts of <c>LoadEnemies</c>,
    /// <c>ProcessEnemyTilesets</c>, and <c>InitializeEnemies</c> at $A0:8A1E-$8C6C.
    /// </summary>
    public void Load(
        ISnesAddressSpace bus,
        ushort populationPointer,
        ushort tilesetPointer,
        SnesVram vram,
        SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);

        _bus = bus;
        PopulationPointer = populationPointer;
        TilesetPointer = tilesetPointer;
        FirstFreeEnemyIndex = 0;
        EnemyCount = 0;
        DeathQuota = 0;
        _activeEnemyIndexes.Clear();
        _interactiveEnemyIndexes.Clear();
        _interactiveCollisionBodies.Clear();
        _graphicsSet.Clear();
        foreach (List<ushort> queue in _drawQueues)
            queue.Clear();
        foreach (RoomEnemySlot slot in _slots)
            slot.Clear();

        LoadGraphicsSet(bus, tilesetPointer, vram, cgram);
        LoadPopulation(bus, populationPointer);
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
    public void QueueGraphicsUploads(VramWriteQueue queue)
    {
        ArgumentNullException.ThrowIfNull(queue);
        EnsureLoaded();
        foreach (RoomEnemyGraphicsSetEntry entry in _graphicsSet)
        {
            int destinationByteOffset = EnemyVramByteBase + entry.StagingOffset;
            if ((destinationByteOffset & 1) != 0 || entry.TileByteCount > ushort.MaxValue)
                throw new InvalidDataException("Enemy tile DMA is not word-aligned or exceeds one native transfer.");
            queue.Enqueue(
                unchecked((ushort)entry.TileByteCount),
                entry.Definition.TileDataAddress,
                unchecked((ushort)(destinationByteOffset / 2)));
        }
    }

    /// <summary>
    /// Rebuilds native active/interactive lists, executes selected enemy AI, advances
    /// instruction lists, and records the layer queues consumed by the later draw phase.
    /// </summary>
    public void StepFrame(
        ushort cameraX,
        ushort cameraY,
        bool timeIsFrozen,
        SamusState? samus = null,
        ushort newlyPressedControllerInput = 0)
    {
        EnsureLoaded();
        DetermineWhichEnemiesToProcess(cameraX, cameraY);
        foreach (List<ushort> queue in _drawQueues)
            queue.Clear();

        foreach (ushort nativeIndex in _activeEnemyIndexes)
        {
            RoomEnemySlot slot = SlotFromNativeIndex(nativeIndex);
            if (!timeIsFrozen)
            {
                RunMainAi(slot, samus, newlyPressedControllerInput);
                slot.FrameCounter = unchecked((ushort)(slot.FrameCounter + 1));
                if ((slot.Properties & 0x2000) != 0)
                    ProcessInstructions(slot);
            }

            // EnemyMain queues ordinary-sprite actors only after AI and instruction work.
            // Properties $0100/$0200 suppress drawing, while extra-property bit $0004 can
            // force an otherwise off-screen actor into the queue. Gunship uses the normal
            // radius-aware visibility test.
            bool visible = (slot.ExtraProperties & 0x0004) != 0 ||
                !EnemyWithNormalSpritesIsOffScreen(slot, cameraX, cameraY);
            if (visible && (slot.Properties & 0x0300) == 0)
                _drawQueues[slot.Layer & 7].Add(nativeIndex);
        }

        // DetermineWhichEnemiesToProcess freezes only the index list. Later bank-$94
        // collision routines dereference the live slot words, so publish bodies after AI
        // has had its chance to move multi-part enemies.
        _interactiveCollisionBodies.Clear();
        foreach (ushort nativeIndex in _interactiveEnemyIndexes)
        {
            RoomEnemySlot slot = SlotFromNativeIndex(nativeIndex);
            _interactiveCollisionBodies.Add(new SolidEnemyCollisionBody(
                nativeIndex,
                slot.XPosition,
                slot.YPosition,
                slot.XRadius,
                slot.YRadius,
                slot.FrozenTimer,
                slot.Properties));
        }
    }

    /// <summary>
    /// Writes queued enemies for an inclusive layer range using native slot order.
    /// </summary>
    public void DrawLayers(
        OamBuffer oam,
        ushort cameraX,
        ushort cameraY,
        int firstLayer,
        int lastLayer)
    {
        ArgumentNullException.ThrowIfNull(oam);
        EnsureLoaded();
        if ((uint)firstLayer > 7 || (uint)lastLayer > 7 || firstLayer > lastLayer)
            throw new ArgumentOutOfRangeException(nameof(firstLayer));

        foreach (int layer in Enumerable.Range(firstLayer, lastLayer - firstLayer + 1))
        {
            foreach (ushort nativeIndex in _drawQueues[layer])
            {
                RoomEnemySlot slot = SlotFromNativeIndex(nativeIndex);

                // Enemy spawn-point offsets are zero for ordinary room-population entries.
                // WriteEnemyOams nevertheless performs the additions before subtracting
                // layer-1 position, so preserve modular 16-bit arithmetic here.
                ushort originX = unchecked((ushort)(slot.SpawnXOffset + slot.XPosition - cameraX));
                ushort originY = unchecked((ushort)(slot.SpawnYOffset + slot.YPosition - cameraY));
                oam.AddEnemySpritemap(
                    _bus!,
                    slot.Definition.Bank,
                    slot.SpritemapPointer,
                    originX,
                    originY,
                    slot.PaletteIndex,
                    slot.VramTilesIndex);
            }
        }
    }

    private void LoadGraphicsSet(
        ISnesAddressSpace bus,
        ushort tilesetPointer,
        SnesVram vram,
        SnesCgram cgram)
    {
        int cursor = EnemyTilesetBank | tilesetPointer;
        int nextEnemyTileIndex = 0;
        int nextStagingOffset = OrdinaryEnemyStagingOffset;

        while (true)
        {
            ushort definitionPointer = ReadWord(bus, cursor);
            if (definitionPointer == 0xffff)
                return;
            if (_graphicsSet.Count == MaximumGraphicsSetCount)
            {
                throw new InvalidDataException(
                    $"Enemy graphics set $B4:{tilesetPointer:X4} exceeds the native four-entry arrays.");
            }

            ushort vramDestination = ReadWord(bus, AddWithinBank(cursor, 2));
            RoomEnemyDefinition definition = ReadDefinition(bus, definitionPointer);

            // ProcessEnemyTilesets copies one complete sixteen-color OBJ palette from the
            // enemy's selected code/data bank. The low nibble of vramDestination selects
            // OBJ palette zero through seven after the native +8 palette-row bias.
            int destinationColor = ((vramDestination & 0x000f) + 8) * 16;
            cgram.LoadFromBus(
                bus,
                (definition.Bank << 16) | definition.PalettePointer,
                colorCount: 16,
                destinationIndex: destinationColor);

            int byteCount = definition.TileDataSize & 0x7fff;
            int stagingOffset = (definition.TileDataSize & 0x8000) != 0
                ? (vramDestination & 0x3000) >> 3
                : nextStagingOffset;
            int vramByteOffset = EnemyVramByteBase + stagingOffset;
            if (vramByteOffset < 0 || vramByteOffset + byteCount > SnesVram.ByteCount)
            {
                throw new InvalidDataException(
                    $"Enemy ${definitionPointer:X4} tile DMA would leave VRAM: " +
                    $"offset ${vramByteOffset:X4}, size ${byteCount:X4}.");
            }

            var tileBytes = new byte[byteCount];
            for (int byteIndex = 0; byteIndex < tileBytes.Length; byteIndex++)
                tileBytes[byteIndex] = bus.ReadByte(AddWithinBank(definition.TileDataAddress, byteIndex));
            vram.LoadBytes(vramByteOffset, tileBytes);

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
            cursor = AddWithinBank(cursor, 4);
        }
    }

    private void LoadPopulation(ISnesAddressSpace bus, ushort populationPointer)
    {
        int cursor = EnemyPopulationBank | populationPointer;
        int slotIndex = 0;
        while (true)
        {
            ushort definitionPointer = ReadWord(bus, cursor);
            if (definitionPointer == 0xffff)
            {
                DeathQuota = bus.ReadByte(AddWithinBank(cursor, 2));
                EnemyCount = unchecked((ushort)slotIndex);
                FirstFreeEnemyIndex = unchecked((ushort)(slotIndex * NativeSlotSize));
                return;
            }
            if (slotIndex == MaximumEnemyCount)
            {
                throw new InvalidDataException(
                    $"Enemy population $A1:{populationPointer:X4} has more than 32 records.");
            }

            RoomEnemyPopulationRecord population = new(
                definitionPointer,
                ReadWord(bus, AddWithinBank(cursor, 2)),
                ReadWord(bus, AddWithinBank(cursor, 4)),
                ReadWord(bus, AddWithinBank(cursor, 6)),
                ReadWord(bus, AddWithinBank(cursor, 8)),
                ReadWord(bus, AddWithinBank(cursor, 10)),
                ReadWord(bus, AddWithinBank(cursor, 12)),
                ReadWord(bus, AddWithinBank(cursor, 14)));
            RoomEnemyDefinition definition = ReadDefinition(bus, definitionPointer);
            RoomEnemySlot slot = _slots[slotIndex];
            InitializeSlotFromDefinition(slot, population, definition);
            RunInitializationAi(slot);

            // InitializeEnemies deliberately clears the init routine's immediate map.
            // Disable-Samus-collision actors receive the canonical empty map until their
            // first instruction-list tick replaces it. Gunship properties contain $2000.
            slot.SpritemapPointer = (slot.Properties & 0x2000) != 0 ? (ushort)0x804d : (ushort)0;

            slotIndex++;
            cursor = AddWithinBank(cursor, 16);
        }
    }

    private void InitializeSlotFromDefinition(
        RoomEnemySlot slot,
        RoomEnemyPopulationRecord population,
        RoomEnemyDefinition definition)
    {
        (ushort tileIndex, ushort paletteIndex) = FindGraphicsIndexes(population.DefinitionPointer);

        slot.EnemyDefinitionPointer = population.DefinitionPointer;
        slot.Definition = definition;
        slot.XRadius = definition.XRadius;
        slot.YRadius = definition.YRadius;
        slot.Health = definition.Health;
        slot.Layer = definition.Layer;
        slot.XPosition = population.XPosition;
        slot.YPosition = population.YPosition;
        slot.CurrentInstruction = population.InitializationParameter;
        slot.Properties = population.Properties;
        slot.ExtraProperties = population.ExtraProperties;
        slot.Parameter1 = population.Parameter1;
        slot.Parameter2 = population.Parameter2;
        slot.InstructionTimer = 1;
        slot.PaletteIndex = paletteIndex;
        slot.VramTilesIndex = tileIndex;
        slot.Spawn = new RoomEnemySpawnSnapshot(
            population,
            definition.XRadius,
            definition.YRadius,
            definition.Health,
            definition.Layer);
    }

    private (ushort TileIndex, ushort PaletteIndex) FindGraphicsIndexes(ushort definitionPointer)
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
        return (0, 0x0a00);
    }

    private void RunInitializationAi(RoomEnemySlot slot)
    {
        int address = (slot.Definition.Bank << 16) | slot.Definition.InitializationAiPointer;
        switch (address)
        {
            case 0xa2a644:
                InitializeGunshipTop(slot);
                return;
            case 0xa2a6d2:
                InitializeGunshipBottom(slot);
                return;
            case 0xa2804c:
                return;
            default:
                throw new NotSupportedException(
                    $"Enemy ${slot.EnemyDefinitionPointer:X4} initialization AI ${address:X6} is not translated.");
        }
    }

    private static void InitializeGunshipTop(RoomEnemySlot slot)
    {
        // Normal gameplay takes $A2:A67C: the cutscene game-state/loading-state alternatives
        // are different room-entry scenarios and therefore cannot be inferred here.
        slot.Properties |= 0x2400;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = 0xa616;
        slot.PaletteIndex = 0x0e00;
        slot.YPosition = unchecked((ushort)(slot.YPosition - 25));
        slot.VariableE = slot.YPosition;
        slot.VariableF = 0xa9bd;
        slot.VariableD = 1;
        slot.VariableC = 0;
    }

    private void InitializeGunshipBottom(RoomEnemySlot slot)
    {
        slot.Properties |= 0x2400;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = slot.Parameter2 != 0 ? (ushort)0xa60e : (ushort)0xa61c;

        // $A2:A6F1 reads enemy_drawing_queue[(cur_enemy_index >> 1) + 106].
        // For the two Landing Site bottom slots those WRAM addresses alias the preceding
        // slot's +$20 vram_tiles_index word. Express the alias semantically, but retain the
        // dependency on physical slot order.
        if (slot.SlotIndex == 0)
            throw new InvalidDataException("Gunship bottom cannot occupy enemy slot zero.");
        slot.VramTilesIndex = _slots[slot.SlotIndex - 1].VramTilesIndex;
        slot.PaletteIndex = 0x0e00;

        if (slot.Parameter2 != 0)
        {
            // For slot two, the equally strange +61 indexed read aliases slot zero's +$06
            // Y-position word. The opening pad sits one pixel above the top hull origin.
            if (slot.SlotIndex < 2)
                throw new InvalidDataException("Gunship entrance pad requires two preceding enemy slots.");
            slot.YPosition = unchecked((ushort)(_slots[slot.SlotIndex - 2].YPosition - 1));
        }
        else
        {
            slot.YPosition = unchecked((ushort)(slot.YPosition + 15));
            slot.VariableD = 71;
        }
        slot.VariableF = 0x804c;
    }

    private void RunMainAi(RoomEnemySlot slot, SamusState? samus, ushort newlyPressedControllerInput)
    {
        int address = (slot.Definition.Bank << 16) | slot.Definition.MainAiPointer;
        switch (address)
        {
            case 0xa2a759:
                RunGunshipTopMain(slot, samus, newlyPressedControllerInput);
                return;
            case 0xa2804c:
                return;
            default:
                throw new NotSupportedException(
                    $"Enemy ${slot.EnemyDefinitionPointer:X4} main AI ${address:X6} is not translated.");
        }
    }

    private void RunGunshipTopMain(
        RoomEnemySlot top,
        SamusState? samus,
        ushort newlyPressedControllerInput)
    {
        if (top.SlotIndex + 2 >= EnemyCount)
            throw new InvalidDataException("Gunship top is missing its two following component slots.");

        RoomEnemySlot bottom = _slots[top.SlotIndex + 1];

        // $A2:A75C decrements the solid bottom's sound timer and reloads 70 on one/underflow.
        // Audio queue two is not yet represented by the runtime, but the actor-owned timer
        // is observable state and must still advance at the original point.
        ushort oldBottomTimer = bottom.VariableD;
        bottom.VariableD = unchecked((ushort)(bottom.VariableD - 1));
        if (oldBottomTimer == 1 || (short)bottom.VariableD < 0)
            bottom.VariableD = 70;

        // The native address-range test admits functions $A942-$AC1A. Idle function $A9BD
        // is inside that interval, so the landed ship continuously performs its four-phase
        // bob before dispatching the function itself.
        if (!IsNegative16(top.VariableF + 0x56be) && IsNegative16(top.VariableF + 0x53e5))
            StepGunshipBob(top);

        switch (top.VariableF)
        {
            case 0xa9bd:
                HandleIdleGunshipEntrance(top, samus, newlyPressedControllerInput);
                return;
            default:
                throw new NotSupportedException(
                    $"Gunship function $A2:{top.VariableF:X4} is not translated.");
        }
    }

    private void StepGunshipBob(RoomEnemySlot top)
    {
        ushort oldTimer = top.VariableD;
        top.VariableD = unchecked((ushort)(top.VariableD - 1));
        if (oldTimer != 1 && (short)top.VariableD >= 0)
            return;

        int tableAddress = 0xa2a7cf + (top.VariableC & 3) * 2;
        top.VariableD = _bus!.ReadByte(tableAddress);
        sbyte yDelta = unchecked((sbyte)_bus.ReadByte(tableAddress + 1));
        for (int component = 0; component < 3; component++)
        {
            RoomEnemySlot slot = _slots[top.SlotIndex + component];
            slot.YPosition = unchecked((ushort)(slot.YPosition + yDelta));
        }
        top.VariableC = unchecked((ushort)((top.VariableC + 1) & 3));
    }

    private void HandleIdleGunshipEntrance(
        RoomEnemySlot top,
        SamusState? samus,
        ushort newlyPressedControllerInput)
    {
        if (samus is null || (newlyPressedControllerInput & 0x0400) == 0)
            return;

        bool insideEntrance =
            (short)unchecked((ushort)(top.XPosition - 8 - samus.XPosition)) < 0 &&
            (short)unchecked((ushort)(top.XPosition + 8 - samus.XPosition)) >= 0 &&
            (short)unchecked((ushort)(top.YPosition - 64 - samus.YPosition)) < 0 &&
            (short)unchecked((ushort)(top.YPosition - samus.YPosition)) >= 0;
        if (insideEntrance && samus.ReadMovementType(_bus!) == 0)
        {
            // The predicate above is a literal port of $A2:A9BD. Entering the ship changes
            // Samus frame handlers, pose, elevator state, save-station flags, sound, and the
            // third component's opening instruction list. Until that complete transaction
            // exists, failing at the exact trigger is safer than applying a half-transition.
            throw new NotSupportedException(
                "Gunship entry was triggered; the $A2:AA09 multi-system transition is not translated yet.");
        }
    }

    private void ProcessInstructions(RoomEnemySlot slot)
    {
        ushort oldTimer = slot.InstructionTimer;
        slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
        if (oldTimer != 1)
        {
            slot.ExtraProperties &= 0x7fff;
            return;
        }

        ushort cursor = slot.CurrentInstruction;
        for (int commandCount = 0; commandCount < 64; commandCount++)
        {
            ushort word = ReadWord(_bus!, (slot.Definition.Bank << 16) | cursor);
            if ((word & 0x8000) == 0)
            {
                slot.InstructionTimer = word;
                slot.SpritemapPointer = ReadWord(
                    _bus!,
                    (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                slot.CurrentInstruction = unchecked((ushort)(cursor + 4));
                slot.ExtraProperties |= 0x8000;
                return;
            }

            switch (word)
            {
                case 0x80ed: // EnemyInstr_Goto: next word is a same-bank instruction pointer.
                    cursor = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    break;
                case 0x812f: // EnemyInstr_Sleep: pin the PC on this command and stop forever.
                    slot.CurrentInstruction = cursor;
                    return;
                default:
                    throw new NotSupportedException(
                        $"Enemy ${slot.EnemyDefinitionPointer:X4} instruction " +
                        $"${slot.Definition.Bank:X2}:{cursor:X4} opcode ${word:X4} is not translated.");
            }
        }

        throw new InvalidDataException(
            $"Enemy ${slot.EnemyDefinitionPointer:X4} instruction list exceeded 64 commands without a frame.");
    }

    private void DetermineWhichEnemiesToProcess(ushort cameraX, ushort cameraY)
    {
        _activeEnemyIndexes.Clear();
        _interactiveEnemyIndexes.Clear();
        foreach (RoomEnemySlot slot in _slots)
        {
            if (slot.EnemyDefinitionPointer is 0 or 0xdaff)
                continue;
            if ((slot.Properties & 0x0200) != 0)
            {
                slot.EnemyDefinitionPointer = 0;
                continue;
            }

            bool active = (slot.Properties & 0x0800) != 0 ||
                EnemyIsWithinProcessingWindow(slot, cameraX, cameraY);
            if (!active)
                continue;

            _activeEnemyIndexes.Add(slot.NativeIndex);
            if ((slot.Properties & 0x0400) == 0)
                _interactiveEnemyIndexes.Add(slot.NativeIndex);
        }
    }

    private static bool EnemyIsWithinProcessingWindow(
        RoomEnemySlot slot,
        ushort cameraX,
        ushort cameraY) =>
        !IsNegative16(slot.XRadius + slot.XPosition - cameraX) &&
        !IsNegative16(slot.XRadius + cameraX + 256 - slot.XPosition) &&
        !IsNegative16(slot.YPosition + 8 - cameraY) &&
        !IsNegative16(cameraY + 248 - slot.YPosition);

    private static bool EnemyWithNormalSpritesIsOffScreen(
        RoomEnemySlot slot,
        ushort cameraX,
        ushort cameraY) =>
        IsNegative16(slot.XRadius + slot.XPosition - cameraX) ||
        IsNegative16(slot.XRadius + cameraX + 256 - slot.XPosition) ||
        IsNegative16(slot.YPosition + 8 - cameraY) ||
        IsNegative16(cameraY + 248 - slot.YPosition);

    private RoomEnemySlot SlotFromNativeIndex(ushort nativeIndex)
    {
        if ((nativeIndex & (NativeSlotSize - 1)) != 0 || nativeIndex >= MaximumEnemyCount * NativeSlotSize)
            throw new ArgumentOutOfRangeException(nameof(nativeIndex));
        return _slots[nativeIndex / NativeSlotSize];
    }

    private void EnsureLoaded()
    {
        if (_bus is null)
            throw new InvalidOperationException("A room enemy population must be loaded first.");
    }

    private static RoomEnemyDefinition ReadDefinition(ISnesAddressSpace bus, ushort pointer)
    {
        int address = EnemyDefinitionBank | pointer;
        return new RoomEnemyDefinition(
            TileDataSize: ReadWord(bus, address),
            PalettePointer: ReadWord(bus, AddWithinBank(address, 2)),
            Health: ReadWord(bus, AddWithinBank(address, 4)),
            Damage: ReadWord(bus, AddWithinBank(address, 6)),
            XRadius: ReadWord(bus, AddWithinBank(address, 8)),
            YRadius: ReadWord(bus, AddWithinBank(address, 10)),
            Bank: bus.ReadByte(AddWithinBank(address, 12)),
            HurtAiTime: bus.ReadByte(AddWithinBank(address, 13)),
            BossId: ReadWord(bus, AddWithinBank(address, 16)),
            InitializationAiPointer: ReadWord(bus, AddWithinBank(address, 18)),
            MainAiPointer: ReadWord(bus, AddWithinBank(address, 24)),
            TileDataAddress: ReadLong(bus, AddWithinBank(address, 54)),
            Layer: bus.ReadByte(AddWithinBank(address, 57)));
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | (bus.ReadByte(AddWithinBank(address, 1)) << 8));

    private static int ReadLong(ISnesAddressSpace bus, int address) =>
        bus.ReadByte(address) |
        (bus.ReadByte(AddWithinBank(address, 1)) << 8) |
        (bus.ReadByte(AddWithinBank(address, 2)) << 16);

    private static int AddWithinBank(int address, int byteCount) =>
        (address & 0xff0000) | ((address + byteCount) & 0xffff);

    private static bool IsNegative16(int value) => (short)unchecked((ushort)value) < 0;
}

/// <summary>Parsed 64-byte bank-$A0 enemy definition fields used by the translated core.</summary>
public readonly record struct RoomEnemyDefinition(
    ushort TileDataSize,
    ushort PalettePointer,
    ushort Health,
    ushort Damage,
    ushort XRadius,
    ushort YRadius,
    byte Bank,
    byte HurtAiTime,
    ushort BossId,
    ushort InitializationAiPointer,
    ushort MainAiPointer,
    int TileDataAddress,
    byte Layer);

/// <summary>One literal 16-byte bank-$A1 room-population record.</summary>
public readonly record struct RoomEnemyPopulationRecord(
    ushort DefinitionPointer,
    ushort XPosition,
    ushort YPosition,
    ushort InitializationParameter,
    ushort Properties,
    ushort ExtraProperties,
    ushort Parameter1,
    ushort Parameter2);

/// <summary>One literal four-byte bank-$B4 room graphics-set record plus resolved data.</summary>
public readonly record struct RoomEnemyGraphicsSetEntry(
    ushort DefinitionPointer,
    ushort VramDestination,
    ushort VramTilesIndex,
    RoomEnemyDefinition Definition,
    int StagingOffset,
    int TileByteCount);

/// <summary>Immutable spawn words retained beside the mutable native enemy slot.</summary>
public readonly record struct RoomEnemySpawnSnapshot(
    RoomEnemyPopulationRecord Population,
    ushort XRadius,
    ushort YRadius,
    ushort Health,
    byte Layer);

/// <summary>
/// Mutable projection of the 64-byte WRAM <c>EnemyData</c> record. Fields not yet consumed
/// by translated code remain absent instead of receiving invented behavior.
/// </summary>
public sealed class RoomEnemySlot
{
    internal RoomEnemySlot(int slotIndex)
    {
        SlotIndex = slotIndex;
        NativeIndex = checked((ushort)(slotIndex * RoomEnemySystem.NativeSlotSize));
    }

    public int SlotIndex { get; }
    public ushort NativeIndex { get; }
    public ushort EnemyDefinitionPointer { get; internal set; }
    public RoomEnemyDefinition Definition { get; internal set; }
    public RoomEnemySpawnSnapshot Spawn { get; internal set; }
    public ushort XPosition { get; internal set; }
    public ushort XSubposition { get; internal set; }
    public ushort YPosition { get; internal set; }
    public ushort YSubposition { get; internal set; }
    public ushort XRadius { get; internal set; }
    public ushort YRadius { get; internal set; }
    public ushort Properties { get; internal set; }
    public ushort ExtraProperties { get; internal set; }
    public ushort Health { get; internal set; }
    public ushort SpritemapPointer { get; internal set; }
    public ushort Timer { get; internal set; }
    public ushort CurrentInstruction { get; internal set; }
    public ushort InstructionTimer { get; internal set; }
    public ushort PaletteIndex { get; internal set; }
    public ushort VramTilesIndex { get; internal set; }
    public byte Layer { get; internal set; }
    public ushort FrozenTimer { get; internal set; }
    public ushort FrameCounter { get; internal set; }
    public ushort Parameter1 { get; internal set; }
    public ushort Parameter2 { get; internal set; }
    public ushort VariableC { get; internal set; }
    public ushort VariableD { get; internal set; }
    public ushort VariableE { get; internal set; }
    public ushort VariableF { get; internal set; }
    public ushort SpawnXOffset { get; internal set; }
    public ushort SpawnYOffset { get; internal set; }

    internal void Clear()
    {
        EnemyDefinitionPointer = 0;
        Definition = default;
        Spawn = default;
        XPosition = XSubposition = YPosition = YSubposition = 0;
        XRadius = YRadius = Properties = ExtraProperties = Health = 0;
        SpritemapPointer = Timer = CurrentInstruction = InstructionTimer = 0;
        PaletteIndex = VramTilesIndex = FrozenTimer = FrameCounter = 0;
        Layer = 0;
        Parameter1 = Parameter2 = 0;
        VariableC = VariableD = VariableE = VariableF = 0;
        SpawnXOffset = SpawnYOffset = 0;
    }
}
