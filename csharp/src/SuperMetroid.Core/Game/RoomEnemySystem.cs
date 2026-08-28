using SuperMetroid.Core.Hardware;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Rooms;

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
public sealed partial class RoomEnemySystem
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
    private Func<ushort>? _nextRandom;
    private Action<ushort>? _setRandomNumber;
    private ushort _randomEnemyCounter;
    private SnesVram? _vram;
    private SnesCgram? _cgram;
    private CeresRidleyState? _ceresRidley;

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
    public ushort EnemiesKilled { get; private set; }
    public byte DeathQuota { get; private set; }
    public ushort BossId { get; private set; }
    public bool IsLoaded => _bus is not null;
    public GunshipFrameEvent LastGunshipEvent { get; private set; }
    public bool GunshipSavePromptPending { get; private set; }
    public bool GunshipSaveRequested { get; private set; }
    public ushort? LastMochtroidSoundEffect { get; private set; }
    public ushort? LastHopperSoundEffect { get; private set; }
    public ushort? LastYardSoundEffect { get; private set; }
    public ushort? LastMetareeSoundEffect { get; private set; }
    public ushort FirefleaDarknessLevel { get; private set; }
    public ushort EarthquakeTimer { get; set; }
    public ushort EarthquakeType { get; set; }

    /// <summary>
    /// Ridley's bank-$A6 state extension while enemy $E13F owns slot zero. The native actor
    /// extends far beyond the common $40-byte enemy record, so exposing a deliberately named
    /// object is both more accurate and considerably easier to inspect than aliasing dozens
    /// of unrelated generic slot words.
    /// </summary>
    public CeresRidleyState? CeresRidley => _ceresRidley;

    /// <summary>
    /// Native <c>ceres_status</c> word consumed by the Ceres door actor. Fresh station load
    /// begins at zero; Ridley's escape sequence is the later producer of values one/two.
    /// </summary>
    public ushort CeresStatus { get; set; }

    /// <summary>
    /// Ports the data-producing parts of <c>LoadEnemies</c>,
    /// <c>ProcessEnemyTilesets</c>, and <c>InitializeEnemies</c> at $A0:8A1E-$8C6C.
    /// </summary>
    public void Load(
        ISnesAddressSpace bus,
        ushort populationPointer,
        ushort tilesetPointer,
        SnesVram vram,
        SnesCgram cgram,
        Func<ushort> nextRandom,
        Action<ushort>? setRandomNumber = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        ArgumentNullException.ThrowIfNull(nextRandom);

        _bus = bus;
        _nextRandom = nextRandom;
        _setRandomNumber = setRandomNumber;
        _vram = vram;
        _cgram = cgram;
        PopulationPointer = populationPointer;
        TilesetPointer = tilesetPointer;
        EnemyCount = 0;
        EnemiesKilled = 0;
        BossId = 0;
        LastGunshipEvent = GunshipFrameEvent.None;
        GunshipSavePromptPending = false;
        GunshipSaveRequested = false;
        LastMochtroidSoundEffect = null;
        LastHopperSoundEffect = null;
        LastYardSoundEffect = null;
        LastMetareeSoundEffect = null;
        FirefleaDarknessLevel = 0;
        EarthquakeTimer = 0;
        EarthquakeType = 0;
        _ceresRidley = null;
        Array.Clear(_crawlerStates);
        Array.Clear(_skreeStates);
        Array.Clear(_flyStates);
        Array.Clear(_sbugStates);
        Array.Clear(_mochtroidStates);
        Array.Clear(_hopperStates);
        Array.Clear(_zoaStates);
        Array.Clear(_yardStates);
        Array.Clear(_waverStates);
        Array.Clear(_metareeStates);
        Array.Clear(_firefleaStates);
        // Enemy projectiles live in a separate native bank-$86 pool, but room loading
        // destroys them just as decisively as it clears bank-$A0 enemy slots. Without
        // this reset, leaving Ridley's room could carry a fireball (and its stale room
        // collision coordinates) into the destination room.
        foreach (RoomEnemyProjectileSlot projectile in _enemyProjectiles)
            projectile.Clear();
        _activeEnemyIndexes.Clear();
        _interactiveEnemyIndexes.Clear();
        _interactiveCollisionBodies.Clear();
        _graphicsSet.Clear();
        foreach (List<ushort> queue in _drawQueues)
            queue.Clear();
        foreach (RoomEnemySlot slot in _slots)
            slot.Clear();

        // $A0:8A6D does not even inspect the room's enemy set when the first population
        // word is the terminator. This is observable: an empty room must not overwrite
        // CGRAM or VRAM merely because its state happens to retain a non-empty set pointer.
        if (ReadWord(bus, EnemyPopulationBank | populationPointer) != 0xffff)
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
        ushort newlyPressedControllerInput = 0,
        RoomLevelData? level = null)
    {
        EnsureLoaded();
        LastGunshipEvent = GunshipFrameEvent.None;
        LastMochtroidSoundEffect = null;
        LastHopperSoundEffect = null;
        LastYardSoundEffect = null;
        LastMetareeSoundEffect = null;
        DetermineWhichEnemiesToProcess(cameraX, cameraY);
        foreach (List<ushort> queue in _drawQueues)
            queue.Clear();

        foreach (ushort nativeIndex in _activeEnemyIndexes)
        {
            RoomEnemySlot slot = SlotFromNativeIndex(nativeIndex);
            if (!timeIsFrozen)
            {
                if (slot.FrozenTimer != 0)
                {
                    // Common_NormalEnemyFrozenAI owns the actor while its freeze clock is
                    // nonzero. Movement and instruction animation do not run underneath it.
                    slot.FrozenTimer = unchecked((ushort)(slot.FrozenTimer - 1));
                    if (slot.FrozenTimer == 0)
                        slot.AiHandlerBits = unchecked((ushort)(slot.AiHandlerBits & ~0x0004));
                }
                else
                {
                    RunMainAi(
                        slot,
                        samus,
                        newlyPressedControllerInput,
                        level,
                        cameraX,
                        cameraY);
                    slot.FrameCounter = unchecked((ushort)(slot.FrameCounter + 1));
                    if (slot.Properties.HasAny(EnemyProperties.ProcessInstructions))
                        ProcessInstructions(slot, samus);
                }
            }

            // EnemyMain queues ordinary-sprite actors only after AI and instruction work.
            // Properties $0100/$0200 suppress drawing, while extra-property bit $0004 can
            // force an otherwise off-screen actor into the queue. Gunship uses the normal
            // radius-aware visibility test.
            bool visible = slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap) ||
                !EnemyWithNormalSpritesIsOffScreen(slot, cameraX, cameraY);
            if (visible && !slot.Properties.HasAny(EnemyProperties.Invisible | EnemyProperties.Deleted))
                _drawQueues[slot.Layer & 7].Add(nativeIndex);

            // $A0:9128 performs this after the enemy has been admitted to its draw queue.
            // Keeping it here also makes the boss's latched draw palettes describe the
            // frame just processed rather than the already-decremented following frame.
            if (!timeIsFrozen && slot.FlashTimer != 0)
                slot.FlashTimer = unchecked((ushort)(slot.FlashTimer - 1));
            if (!timeIsFrozen && slot.InvincibilityTimer != 0)
                slot.InvincibilityTimer = unchecked((ushort)(slot.InvincibilityTimer - 1));
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
        _randomEnemyCounter = unchecked((ushort)(_randomEnemyCounter + 1));
    }

    /// <summary>
    /// Supplies the choice normally returned by message box $1C after restoration. SRAM
    /// persistence remains an outer-runtime seam; the actor publishes a Yes choice while
    /// continuing the cartridge's identical exit animation for either answer.
    /// </summary>
    public void AnswerGunshipSavePrompt(bool save)
    {
        EnsureLoaded();
        if (!GunshipSavePromptPending)
            throw new InvalidOperationException("The gunship save prompt is not awaiting a response.");

        RoomEnemySlot top = _slots[0];
        RoomEnemySlot pad = _slots[2];
        GunshipSavePromptPending = false;
        GunshipSaveRequested = save;
        top.VariableF = 0xab60;
        pad.InstructionTimer = 1;
        pad.CurrentInstruction = 0xa5be;
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.SavePromptAnswered;
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
                ushort drawPaletteIndex = slot.EnemyDefinitionPointer == CeresRidleyDefinition &&
                    _ceresRidley is not null
                        ? _ceresRidley.CommonDrawPaletteIndex
                        : slot.PaletteIndex;
                if (slot.EnemyDefinitionPointer == CeresRidleyDefinition)
                {
                    // CeresRidley_Main calls DrawRidleyTail/DrawRidleyWings before the
                    // common WriteEnemyOams pass emits the extended body. Appending these
                    // here preserves both that OAM order and the enemy's normal layer queue.
                    DrawCeresRidleySupplementalSprites(oam, slot, cameraX, cameraY);
                }
                if (!slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap))
                {
                    oam.AddEnemySpritemap(
                        _bus!,
                        slot.Definition.Bank,
                        slot.SpritemapPointer,
                        originX,
                        originY,
                        drawPaletteIndex,
                        slot.VramTilesIndex);
                    continue;
                }

                // Extended spritemaps begin with a low-byte component count followed by
                // eight-byte {X,Y,spritemap,hitbox} records. Steam uses one component, but
                // retaining the native list format is necessary for bosses and composite
                // enemies that share this bank-$A0 draw path.
                int extendedAddress = (slot.Definition.Bank << 16) | slot.SpritemapPointer;
                int componentCount = _bus!.ReadByte(extendedAddress);
                ushort componentPointer = unchecked((ushort)(slot.SpritemapPointer + 2));
                for (int component = 0; component < componentCount; component++)
                {
                    int componentAddress = (slot.Definition.Bank << 16) | componentPointer;
                    ushort componentX = unchecked((ushort)(originX + ReadWord(_bus, componentAddress)));
                    ushort componentY = unchecked((ushort)(originY + ReadWord(_bus, AddWithinBank(componentAddress, 2))));
                    ushort ordinarySpritemap = ReadWord(_bus, AddWithinBank(componentAddress, 4));

                    // $FFFE names a BG2 tilemap command stream, not OBJ art. Its writer is
                    // a separate modeled-BG seam; invisible steam frames never use it.
                    if (ReadWord(
                            _bus,
                            (slot.Definition.Bank << 16) | ordinarySpritemap) != 0xfffe &&
                        ((componentX + 128) & 0xfe00) == 0 &&
                        ((componentY + 128) & 0xfe00) == 0)
                    {
                        oam.AddEnemySpritemap(
                            _bus,
                            slot.Definition.Bank,
                            ordinarySpritemap,
                            componentX,
                            componentY,
                            drawPaletteIndex,
                            slot.VramTilesIndex,
                            clipVerticalWrap: true,
                            originYIsOnScreen: (componentY >> 8) == 0);
                    }

                    componentPointer = unchecked((ushort)(componentPointer + 8));
                }
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
            // enemy's selected code/data bank. Its assembly masks the entire low byte before
            // adding the native eight-row OBJ bias. Retail records use values zero through
            // seven, but retaining the wider mask makes corrupt data fail visibly.
            int destinationColor = ((vramDestination & 0x00ff) + 8) * 16;
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
                // InitializeEnemies returns early for an initially empty population. It has
                // already zeroed both enemy counters, but it does not rewrite the previous
                // first-free index or death quota. Preserve that retail quirk on reloads.
                if (slotIndex == 0)
                    return;

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
            if (definition.BossId != 0)
                BossId = definition.BossId;
            RunInitializationAi(slot);

            // InitializeEnemies deliberately clears the init routine's immediate map.
            // Disable-Samus-collision actors receive the canonical empty map until their
            // first instruction-list tick replaces it. Gunship properties contain $2000.
            slot.SpritemapPointer = slot.Properties.HasAny(EnemyProperties.ProcessInstructions)
                ? slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap)
                    ? (ushort)0x804f
                    : (ushort)0x804d
                : (ushort)0;

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
        slot.AiHandlerBits = 0;
        slot.Parameter1 = population.Parameter1;
        slot.Parameter2 = population.Parameter2;
        slot.Timer = 0;
        slot.InstructionTimer = 1;
        slot.FrameCounter = 0;
        slot.AiBank = definition.Bank;
        slot.HurtAiTime = definition.HurtAiTime;
        slot.PaletteIndex = paletteIndex;
        slot.VramTilesIndex = tileIndex;
        slot.Spawn = new RoomEnemySpawnSnapshot(
            population,
            definition.XRadius,
            definition.YRadius,
            definition.Health,
            definition.Layer,
            tileIndex,
            paletteIndex,
            ReadSpawnNameWords(definition));
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
            case 0xa6efb1:
                InitializeCeresSteam(slot);
                return;
            case 0xa6f6c5:
                InitializeCeresDoor(slot);
                return;
            case 0xa6a0f5 when slot.EnemyDefinitionPointer == 0xe13f:
                InitializeCeresRidley(slot);
                return;
            case 0xa2e49f when slot.EnemyDefinitionPointer == RipperDefinition:
                InitializeRipper(slot);
                return;
            case 0xa396e3 when slot.EnemyDefinitionPointer == SciserDefinition:
                InitializeCrawler(slot, SciserInitialInstructionTable, speciesInstructionOffset: 8);
                return;
            case 0xa3993b when slot.EnemyDefinitionPointer == ZeroDefinition:
                InitializeCrawler(slot, ZeroInitialInstructionTable, speciesInstructionOffset: 10);
                return;
            case 0xa3b66f when slot.EnemyDefinitionPointer == ViolaDefinition:
                InitializeCrawler(slot, ViolaInitialInstructionTable, speciesInstructionOffset: 6);
                return;
            case 0xa3e2d4 when slot.EnemyDefinitionPointer == ZeelaDefinition:
            case 0xa3e59c when slot.EnemyDefinitionPointer == SovaDefinition:
            case 0xa3e669 when slot.EnemyDefinitionPointer is ZoomerDefinition or StoneZoomerDefinition:
                InitializeCrawler(slot, SharedCrawlerInitialInstructionTable);
                return;
            case 0xa3e043 when slot.EnemyDefinitionPointer == HZoomerDefinition:
                InitializeHZoomer(slot);
                return;
            case 0xa3c6ae when slot.EnemyDefinitionPointer == SkreeDefinition:
                InitializeSkree(slot);
                return;
            case 0xa2b06b when slot.EnemyDefinitionPointer is
                MellowDefinition or MellaDefinition or MemuDefinition:
                InitializeFly(slot);
                return;
            case 0xa3a14d when slot.EnemyDefinitionPointer is SbugDefinition or Sbug2Definition:
                InitializeSbug(slot);
                return;
            case 0xa3a77d when slot.EnemyDefinitionPointer == MochtroidDefinition:
                InitializeMochtroid(slot);
                return;
            case 0xa3ab09 when IsHopperDefinition(slot.EnemyDefinitionPointer):
                InitializeHopper(slot);
                return;
            case 0xa3b44a when slot.EnemyDefinitionPointer == ZoaDefinition:
                InitializeZoa(slot);
                return;
            case 0xa3cde2 when slot.EnemyDefinitionPointer == YardDefinition:
                InitializeYard(slot);
                return;
            case 0xa386ed when slot.EnemyDefinitionPointer == WaverDefinition:
                InitializeWaver(slot);
                return;
            case 0xa38960 when slot.EnemyDefinitionPointer == MetareeDefinition:
                InitializeMetaree(slot);
                return;
            case 0xa38d2d when slot.EnemyDefinitionPointer == FirefleaDefinition:
                InitializeFireflea(slot);
                return;
            case 0xa2804c:
                return;
            default:
                throw new NotSupportedException(
                    $"Enemy ${slot.EnemyDefinitionPointer:X4} initialization AI ${address:X6} is not translated.");
        }
    }

    private void InitializeCeresSteam(RoomEnemySlot slot)
    {
        if (slot.Parameter1 >= 6)
        {
            throw new InvalidDataException(
                $"Ceres steam parameter one ${slot.Parameter1:X4} exceeds its six-entry tables.");
        }

        slot.VramTilesIndex = 0;
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.UsesExtendedSpritemap);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.PaletteIndex = 0x0a00;
        slot.VariableD = unchecked((ushort)((_nextRandom!() & 0x001f) + 1));
        int tableIndex = slot.Parameter1 * 2;
        slot.CurrentInstruction = ReadWord(_bus!, 0xa6eff5 + tableIndex);
        slot.VariableA = ReadWord(_bus!, 0xa6f001 + tableIndex);
    }

    /// <summary>Ports <c>CeresDoor_Init</c> at $A6:F6C5 for the live Ceres room path.</summary>
    private void InitializeCeresDoor(RoomEnemySlot slot)
    {
        // Both ROM tables contain one word per population parameter. Variants five and six
        // are the left/right OBJ walls spawned at $A6:A9A5 for Ridley's Mode-7 departure;
        // the old four-entry bound made the native spawned records impossible to create.
        if (slot.Parameter1 >= 7)
        {
            throw new InvalidDataException(
                $"Ceres door parameter one ${slot.Parameter1:X4} exceeds its seven variants.");
        }

        slot.SpritemapPointer = 0xfac7;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.VramTilesIndex = 0;
        slot.PaletteIndex = 0x0400;
        int tableOffset = slot.Parameter1 * 2;
        slot.VariableA = ReadWord(_bus!, 0xa6f72b + tableOffset);
        slot.CurrentInstruction = ReadWord(_bus!, 0xa6f52c + tableOffset);
        slot.VariableB = 0;

        // CeresDoor_Func_1 performs this extra direct transfer only for variant two. The
        // source/destination are the literal reconstructed DMA record at $A6:F739.
        if (slot.Parameter1 == 2)
        {
            // The source register used bank $B0 with a 16-bit address that increments
            // independently of the bank byte. Materialize that exact DMA source slice,
            // then use SnesVram's range-checked consecutive transfer primitive.
            byte[] tileBytes = new byte[0x0400];
            for (int byteIndex = 0; byteIndex < tileBytes.Length; byteIndex++)
                tileBytes[byteIndex] = _bus!.ReadByte(0xb00000 | ((0xc400 + byteIndex) & 0xffff));
            _vram!.LoadBytes(0xe000, tileBytes);
        }

        if (CeresStatus == 0 && slot.Parameter1 == 3)
        {
            // The native destination $142 is a byte offset into target_palettes: colors
            // 161..175. This runtime exposes the final fade target directly in CGRAM.
            _cgram!.LoadFromBus(_bus!, 0xa6f4ee, colorCount: 15, destinationIndex: 0x142 / 2);
            return;
        }

        slot.PaletteIndex = 0x0e00;
        int source = CeresStatus != 0 ? 0xa6f50e : 0xa6f4ee;
        _cgram!.LoadFromBus(_bus!, source, colorCount: 15, destinationIndex: 0x1e2 / 2);
    }

    private static void InitializeGunshipTop(RoomEnemySlot slot)
    {
        // Normal gameplay takes $A2:A67C: the cutscene game-state/loading-state alternatives
        // are different room-entry scenarios and therefore cannot be inferred here.
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessInstructions | EnemyProperties.IgnoreSamusCollision);
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
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessInstructions | EnemyProperties.IgnoreSamusCollision);
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

    private void RunMainAi(
        RoomEnemySlot slot,
        SamusState? samus,
        ushort newlyPressedControllerInput,
        RoomLevelData? level,
        ushort cameraX,
        ushort cameraY)
    {
        int address = (slot.Definition.Bank << 16) | slot.Definition.MainAiPointer;
        switch (address)
        {
            case 0xa2a759:
                RunGunshipTopMain(slot, samus, newlyPressedControllerInput);
                return;
            case 0xa2804c:
                return;
            case 0xa6f00d:
                RunCeresSteamMain(slot);
                return;
            case 0xa6f765:
                RunCeresDoorMain(slot);
                return;
            case 0xa6a288 when slot.EnemyDefinitionPointer == 0xe13f:
                RunCeresRidleyMain(slot, samus);
                return;
            case 0xa2e4da when slot.EnemyDefinitionPointer == RipperDefinition:
                RunRipperMain(slot, level);
                return;
            case 0xa3e6c2 when IsSharedCrawlerDefinition(slot.EnemyDefinitionPointer):
                RunCrawlerMain(slot, level);
                return;
            case 0xa3e08b when slot.EnemyDefinitionPointer == HZoomerDefinition:
                RunHZoomerMain(slot, samus, level);
                return;
            case 0xa3c6c7 when slot.EnemyDefinitionPointer == SkreeDefinition:
                RunSkreeMain(slot, samus, level);
                return;
            case 0xa2b11f when slot.EnemyDefinitionPointer is
                MellowDefinition or MellaDefinition or MemuDefinition:
                RunFlyMain(slot, samus);
                return;
            case 0xa3a2d0 when slot.EnemyDefinitionPointer is SbugDefinition or Sbug2Definition:
                RunSbugMain(slot, samus, level);
                return;
            case 0xa3a790 when slot.EnemyDefinitionPointer == MochtroidDefinition:
                RunMochtroidMain(slot, samus, level);
                return;
            case 0xa3abcf when IsHopperDefinition(slot.EnemyDefinitionPointer):
                RunHopperMain(slot, samus, level);
                return;
            case 0xa3b47c when slot.EnemyDefinitionPointer == ZoaDefinition:
                RunZoaMain(slot, RequireZoaState(slot), samus, cameraX, cameraY);
                return;
            case 0xa3ce64 when slot.EnemyDefinitionPointer == YardDefinition:
                RunYardMain(slot, samus, level);
                return;
            case 0xa3874c when slot.EnemyDefinitionPointer == WaverDefinition:
                RunWaverMain(slot, RequireWaverState(slot), level);
                return;
            case 0xa38979 when slot.EnemyDefinitionPointer == MetareeDefinition:
                RunMetareeMain(slot, RequireMetareeState(slot), samus, level);
                return;
            case 0xa38dee when slot.EnemyDefinitionPointer == FirefleaDefinition:
                RunFirefleaMain(slot, RequireFirefleaState(slot));
                return;
            default:
                throw new NotSupportedException(
                    $"Enemy ${slot.EnemyDefinitionPointer:X4} main AI ${address:X6} is not translated.");
        }
    }

    private static void RunCeresSteamMain(RoomEnemySlot slot)
    {
        slot.Health = 0x7fff;
        if (slot.VariableA == 0xeff4)
            return;

        // Parameters four/five install $A6:F019 through table words at $F009/$F00B; its
        // graphical offsets depend on the Ceres elevator's Mode-7 matrix. They are retained
        // as a named unsupported AI boundary instead of silently drawing the untransformed
        // actor at its base point.
        throw new NotSupportedException(
            $"Ceres steam Mode-7 function $A6:{slot.VariableA:X4} is not translated.");
    }

    private void RunCeresDoorMain(RoomEnemySlot slot)
    {
        switch (slot.VariableA)
        {
            // Functions two/three only produce escape earthquake state when status >= 2.
            // Earthquake rendering is independent of the door's own initial presentation.
            case 0xf76b:
            case 0xf770:
                return;

            case 0xf7a5:
                slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
                if ((CeresStatus & 1) != 0)
                {
                    slot.PaletteIndex = 0x0e00;
                    slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                }
                return;

            case 0xf7bd:
                RunCeresDoorPaletteAnimation();
                if (CeresStatus >= 2)
                {
                    // $A6:F7BD begins a 48-frame destruction sequence. Retaining this as
                    // an explicit later boundary avoids pretending the escape state exists.
                    throw new NotSupportedException("Ceres door destruction sequence $A6:F7DC is not translated.");
                }
                return;

            case 0xf850:
                RunCeresDoorPaletteAnimation();
                return;

            default:
                throw new NotSupportedException(
                    $"Ceres door main function $A6:{slot.VariableA:X4} is not translated.");
        }
    }

    private void RunCeresDoorPaletteAnimation()
    {
        // $A6:F850 selects six colors by NMI counter bits 3..5. Enemy FrameCounter advances
        // at the same accepted-frame cadence in this runtime, so slot zero is the shared
        // timebase for the room-owned palette cycle.
        ushort frame = _slots[0].FrameCounter;

        // AnimateCeresElevatorPlatform at $A6:F8F1 does not belong to either arrival
        // projectile. It survives their touchdown deletion because the rotating-room door
        // actor keeps alternating these four Mode-7 tilemap bytes forever. Omitting this
        // queue made the moving OBJ pad flash correctly, then left the landed tile platform
        // frozen on whichever frame happened to be present at deletion.
        ushort transferPointer = ReadWord(_bus!, 0xa6f900 + (frame & 2));
        ApplyMode7TransferList(transferPointer);

        ushort sourcePointer = unchecked((ushort)(2 * (frame & 0x0038) - 0x078f));
        _cgram!.LoadFromBus(_bus!, 0xa60000 | sourcePointer, colorCount: 6, destinationIndex: 0x52 / 2);
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
            case 0xaa4f:
                if (TickGunshipFunctionTimer(top))
                    top.VariableF = 0xaa5d;
                return;
            case 0xaa5d:
                LowerSamusIntoGunship(top, samus);
                return;
            case 0xaa94:
                if (TickGunshipFunctionTimer(top))
                    top.VariableF = 0xaaa2;
                return;
            case 0xaaa2:
                RestoreSamusInGunship(top, samus);
                return;
            case 0xab1f:
                GunshipSavePromptPending = true;
                return;
            case 0xab60:
                if (TickGunshipFunctionTimer(top))
                    top.VariableF = 0xab6e;
                return;
            case 0xab6e:
                RaiseSamusOutOfGunship(top, samus);
                return;
            case 0xaba5:
                if (TickGunshipFunctionTimer(top))
                {
                    top.VariableF = 0xa9bd;
                    if (samus is not null)
                        samus.InputLocked = false;
                    LastGunshipEvent = GunshipFrameEvent.ExitCompleted;
                }
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
        if (insideEntrance && samus.ReadMovementKind(_bus!) == SamusMovementType.Standing)
        {
            RoomEnemySlot pad = _slots[top.SlotIndex + 2];
            top.VariableF = 0xaa4f;
            if (samus.XPosition != 0x0480)
                samus.XPosition = top.XPosition;
            samus.ApplyForwardFacingPoseSetup(_bus!);
            samus.InputLocked = true;
            samus.PrimeGraphics(_bus!);
            pad.YPosition = unchecked((ushort)(top.YPosition - 1));
            pad.InstructionTimer = 1;
            pad.CurrentInstruction = 0xa5be;
            top.VariableA = 144;
            LastGunshipEvent = GunshipFrameEvent.EntryStarted;
        }
    }

    private static bool TickGunshipFunctionTimer(RoomEnemySlot top)
    {
        ushort oldTimer = top.VariableA;
        top.VariableA = unchecked((ushort)(top.VariableA - 1));
        return oldTimer == 1 || (short)top.VariableA < 0;
    }

    private void LowerSamusIntoGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship entry lost its Samus actor.");
        samus.YPosition = unchecked((ushort)(samus.YPosition + 2));
        if (IsNegative16(samus.YPosition - unchecked((ushort)(top.VariableE + 18))))
            return;

        RoomEnemySlot pad = _slots[top.SlotIndex + 2];
        top.VariableF = 0xaa94;
        pad.InstructionTimer = 1;
        pad.CurrentInstruction = 0xa5ee;
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.EntryPadClosing;
    }

    private void RestoreSamusInGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship restoration lost its Samus actor.");
        samus.Health = RestoreTwo(samus.Health, samus.MaxHealth);
        samus.Missiles = RestoreTwo(samus.Missiles, samus.MaxMissiles);
        samus.SuperMissiles = RestoreTwo(samus.SuperMissiles, samus.MaxSuperMissiles);
        samus.PowerBombs = RestoreTwo(samus.PowerBombs, samus.MaxPowerBombs);
        if ((short)(samus.ReserveEnergy - samus.MaxReserveEnergy) < 0 ||
            (short)(samus.Health - samus.MaxHealth) < 0 ||
            (short)(samus.Missiles - samus.MaxMissiles) < 0 ||
            (short)(samus.SuperMissiles - samus.MaxSuperMissiles) < 0 ||
            (short)(samus.PowerBombs - samus.MaxPowerBombs) < 0)
            return;

        top.VariableF = 0xab1f;
        GunshipSavePromptPending = true;
        LastGunshipEvent = GunshipFrameEvent.SavePromptRequested;
    }

    private void RaiseSamusOutOfGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship exit lost its Samus actor.");
        samus.YPosition = unchecked((ushort)(samus.YPosition - 2));
        if (!IsNegative16(samus.YPosition - unchecked((ushort)(top.VariableE - 30))))
            return;

        RoomEnemySlot pad = _slots[top.SlotIndex + 2];
        top.VariableF = 0xaba5;
        pad.InstructionTimer = 1;
        pad.CurrentInstruction = 0xa5ee;
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.ExitPadClosing;
    }

    private static ushort RestoreTwo(ushort current, ushort maximum)
    {
        if ((short)(current - maximum) >= 0)
            return current;
        return unchecked((ushort)Math.Min(current + 2, maximum));
    }

    private void ProcessInstructions(RoomEnemySlot slot, SamusState? samus)
    {
        ushort oldTimer = slot.InstructionTimer;
        slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
        if (oldTimer != 1)
        {
            slot.ExtraProperties = slot.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
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
                slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
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
                case 0x8173: // EnemyInstr_EnableOffScreenProcessing.
                    slot.Properties = slot.Properties.With(EnemyProperties.ProcessOffScreen);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x817d: // EnemyInstr_DisableOffScreenProcessing.
                    slot.Properties = slot.Properties.Without(EnemyProperties.ProcessOffScreen);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xcc36 when slot.EnemyDefinitionPointer == YardDefinition:
                    // Yard animation bytecode owns movement dispatch. The word after the
                    // opcode is a same-bank function pointer, not a branch destination.
                    RequireYardState(slot).MovementFunction = (YardMovementFunction)ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xcc3f when slot.EnemyDefinitionPointer == YardDefinition:
                    RequireYardState(slot).HidingInstructionList = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xcc48 when slot.EnemyDefinitionPointer == YardDefinition:
                {
                    YardEnemyState yard = RequireYardState(slot);
                    yard.Direction = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    if (yard.Direction >= 8)
                    {
                        throw new InvalidDataException(
                            $"Yard instruction selected invalid direction {yard.Direction}.");
                    }
                    yard.AirborneFacingDirection = ReadWord(
                        _bus!,
                        YardDirectionData + yard.Direction * 8 + 6);
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                }
                case 0xcc5f when slot.EnemyDefinitionPointer == YardDefinition:
                    slot.XPosition = unchecked((ushort)(slot.XPosition + ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)))));
                    slot.YPosition = unchecked((ushort)(slot.YPosition + ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 4)))));
                    cursor = unchecked((ushort)(cursor + 6));
                    break;
                case 0xcc78 when slot.EnemyDefinitionPointer == YardDefinition:
                    // The native instruction receives Y already advanced past the opcode.
                    // Subtracting six therefore resumes four bytes before the opcode.
                    cursor = RequireYardState(slot).Behavior == 2 || (_nextRandom!() & 1) != 0
                        ? unchecked((ushort)(cursor - 4))
                        : unchecked((ushort)(cursor + 2));
                    break;
                case 0xaa68 when IsHopperDefinition(slot.EnemyDefinitionPointer):
                    // Sidehopper's list passes a library-two sound operand, then the native
                    // instruction returns the cursor after that operand. Audio playback is
                    // an outer concern; publishing the exact word keeps the event observable.
                    LastHopperSoundEffect = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xaafe when IsHopperDefinition(slot.EnemyDefinitionPointer):
                    RequireHopperState(slot).ReadyToHop = true;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xb429 when slot.EnemyDefinitionPointer == ZoaDefinition:
                    RequireZoaState(slot).XSpeedTableIndex = 4;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xb434 when slot.EnemyDefinitionPointer == ZoaDefinition:
                    RequireZoaState(slot).XSpeedTableIndex = 8;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xb43f when slot.EnemyDefinitionPointer == ZoaDefinition:
                    RequireZoaState(slot).XSpeedTableIndex = 12;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe660: // Shared crawler: install the function pointer operand.
                    RequireCrawlerState(slot).Function = (CrawlerEnemyFunction)ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xdfc2 when slot.EnemyDefinitionPointer == HZoomerDefinition:
                    RequireCrawlerState(slot).Function = (CrawlerEnemyFunction)ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xc6a4: // Skree: the preparation animation releases the dive AI.
                    RequireSkreeState(slot).AttackReady = true;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x86e3 when slot.EnemyDefinitionPointer == WaverDefinition:
                    // The four-frame spin list hands its completion back to main AI rather
                    // than branching directly to steady art.
                    RequireWaverState(slot).SpinFinished = true;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x8956 when slot.EnemyDefinitionPointer == MetareeDefinition:
                    // The preparation list sleeps immediately after publishing this flag.
                    // Main AI consumes it on the following enemy frame and installs the
                    // launched list, exactly matching the native instruction/AI hand-off.
                    RequireMetareeState(slot).AttackReady = true;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe4be: // Ridley: begin roar; audio playback is outside this subsystem.
                    RequireCeresRidley(slot).Roaring = true;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe4ca: // Ridley: close mouth / clear the roaring presentation flag.
                    RequireCeresRidley(slot).Roaring = false;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe4d2: // Ceres: low-energy branch embedded in the fireball animation.
                    if (samus is null)
                    {
                        throw new InvalidOperationException(
                            "Ceres Ridley fireball branch requires the active Samus actor.");
                    }
                    cursor = unchecked((short)(samus.Health - 30)) < 0
                        ? ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)))
                        : unchecked((ushort)(cursor + 4));
                    break;
                case 0xe501: // Ceres Ridley: select feet-distance animation index operand.
                    RequireCeresRidley(slot).FeetDistanceIndex = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xe517: // Ridley: branch to operand when he is not facing left.
                    CeresRidleyState ridley = RequireCeresRidley(slot);
                    cursor = ridley.FacingDirection != 0
                        ? ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)))
                        : unchecked((ushort)(cursor + 4));
                    break;
                case 0xe51f: // Ridley: add the two signed pixel operands to his origin.
                    slot.XPosition = unchecked((ushort)(
                        slot.XPosition +
                        ReadWord(_bus!, (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)))));
                    slot.YPosition = unchecked((ushort)(
                        slot.YPosition +
                        ReadWord(_bus!, (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 4)))));
                    cursor = unchecked((ushort)(cursor + 6));
                    break;
                case 0xe84d: // Ridley: aim the next fireball from the facing-dependent mouth.
                    if (samus is null)
                    {
                        throw new InvalidOperationException(
                            "Ceres Ridley fireball aim requires the active Samus actor.");
                    }
                    CalculateCeresRidleyFireballVelocity(slot, samus);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe904: // Ridley: spawn the leading fireball with a wall afterburn.
                    SpawnCeresRidleyFireball(slot, spawnAfterburn: true);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe909: // Ridley: spawn a following fireball without an afterburn.
                    SpawnCeresRidleyFireball(slot, spawnAfterburn: false);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe969: // Ceres Ridley: animation hands control to accelerating liftoff.
                    CeresRidleyState liftoff = RequireCeresRidley(slot);
                    liftoff.Function = CeresRidleyAiFunction.LiftoffAccelerating;
                    liftoff.VerticalVelocity = unchecked((ushort)-352);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf11d: // Ceres steam: hide and exclude from interaction.
                    slot.Properties = slot.Properties.With(
                        EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf127: // Ceres steam: randomized dormant-loop branch.
                    slot.VariableD = unchecked((ushort)(slot.VariableD - 1));
                    if (slot.VariableD != 0)
                    {
                        cursor = ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    }
                    else
                    {
                        cursor = ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 4)));
                        slot.Properties = slot.Properties.Without(
                            EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
                    }
                    break;
                case 0xf135: // Ceres steam: show and admit interaction.
                    slot.Properties = slot.Properties.Without(
                        EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf63e: // Ceres door: loop until Samus is within a 48x48-pixel box.
                    if (samus is null)
                    {
                        throw new InvalidOperationException(
                            "Ceres door proximity instruction requires the active Samus actor.");
                    }

                    // `$A6:F63E` subtracts the two unsigned position words, interprets the
                    // wrapped result as signed, then takes its absolute value independently
                    // on each axis. If either distance is at least $30, the operand is a
                    // same-bank loop target; otherwise execution skips that operand.
                    int xDistance = Math.Abs(unchecked((short)(slot.XPosition - samus.XPosition)));
                    int yDistance = Math.Abs(unchecked((short)(slot.YPosition - samus.YPosition)));
                    cursor = xDistance >= 0x30 || yDistance >= 0x30
                        ? ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)))
                        : unchecked((ushort)(cursor + 4));
                    break;
                case 0xf66a: // Ceres door: branch while area boss bit one is clear.
                    // Fresh Ceres begins with the boss bit clear. CeresStatus becomes the
                    // translated owner of that event later; until then, follow the native
                    // false branch to the same-bank pointer in the next word.
                    cursor = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    break;
                case 0xf678: // Ceres door: branch while ceres_status is zero.
                    cursor = CeresStatus != 0
                        ? unchecked((ushort)(cursor + 4))
                        : ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    break;
                case 0xf68b: // Ceres steam/door: set native property bit $0400.
                    slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf695: // Ceres door: clear native property bit $0400.
                    slot.Properties = slot.Properties.Without(EnemyProperties.IgnoreSamusCollision);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf69f: // Ceres door: publish animation-state word B=1.
                    slot.VariableB = 1;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf6a6: // Ceres door/steam: hide the actor.
                    slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf6b0: // Ceres door: state B=0, then show the actor.
                    slot.VariableB = 0;
                    slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf6b3: // Ceres door: show the actor.
                    slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf6bd: // Ceres door sound command; audio queue is not yet modeled.
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
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
            if (slot.Properties.HasAny(EnemyProperties.Deleted))
            {
                slot.EnemyDefinitionPointer = 0;
                continue;
            }

            bool active = slot.Properties.HasAny(EnemyProperties.ProcessOffScreen) ||
                EnemyIsWithinProcessingWindow(slot, cameraX, cameraY);
            if (!active)
                continue;

            _activeEnemyIndexes.Add(slot.NativeIndex);
            if (!slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision))
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

    /// <summary>
    /// Parses one complete 64-byte enemy header from the fixed bank-$A0 definition table.
    /// Keeping this reader public lets debugger tooling inspect unsupported actors without
    /// pretending their initialization or main AI has already been translated.
    /// </summary>
    public static RoomEnemyDefinition ReadDefinition(ISnesAddressSpace bus, ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
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
            HurtSoundEffect: ReadWord(bus, AddWithinBank(address, 14)),
            BossId: ReadWord(bus, AddWithinBank(address, 16)),
            InitializationAiPointer: ReadWord(bus, AddWithinBank(address, 18)),
            PartCount: ReadWord(bus, AddWithinBank(address, 20)),
            Unused16: ReadWord(bus, AddWithinBank(address, 22)),
            MainAiPointer: ReadWord(bus, AddWithinBank(address, 24)),
            GrappleAiPointer: ReadWord(bus, AddWithinBank(address, 26)),
            HurtAiPointer: ReadWord(bus, AddWithinBank(address, 28)),
            FrozenAiPointer: ReadWord(bus, AddWithinBank(address, 30)),
            TimeFrozenAiPointer: ReadWord(bus, AddWithinBank(address, 32)),
            DeathAnimation: ReadWord(bus, AddWithinBank(address, 34)),
            Unused24: ReadWord(bus, AddWithinBank(address, 36)),
            Unused26: ReadWord(bus, AddWithinBank(address, 38)),
            PowerBombReactionPointer: ReadWord(bus, AddWithinBank(address, 40)),
            VariantIndex: ReadWord(bus, AddWithinBank(address, 42)),
            Unused2C: ReadWord(bus, AddWithinBank(address, 44)),
            Unused2E: ReadWord(bus, AddWithinBank(address, 46)),
            TouchAiPointer: ReadWord(bus, AddWithinBank(address, 48)),
            ShotAiPointer: ReadWord(bus, AddWithinBank(address, 50)),
            InitialSpritemapPointer: ReadWord(bus, AddWithinBank(address, 52)),
            TileDataAddress: ReadLong(bus, AddWithinBank(address, 54)),
            Layer: bus.ReadByte(AddWithinBank(address, 57)),
            ItemDropChancesPointer: ReadWord(bus, AddWithinBank(address, 58)),
            VulnerabilityPointer: ReadWord(bus, AddWithinBank(address, 60)),
            NamePointer: ReadWord(bus, AddWithinBank(address, 62)));
    }

    private RoomEnemySpawnNameWords ReadSpawnNameWords(RoomEnemyDefinition definition)
    {
        if (definition.NamePointer == 0)
            return default;

        int address = 0xb40000 | definition.NamePointer;
        return new RoomEnemySpawnNameWords(
            ReadWord(_bus!, address),
            ReadWord(_bus!, AddWithinBank(address, 2)),
            ReadWord(_bus!, AddWithinBank(address, 4)),
            ReadWord(_bus!, AddWithinBank(address, 6)),
            ReadWord(_bus!, AddWithinBank(address, 8)),
            ReadWord(_bus!, AddWithinBank(address, 12)));
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | (bus.ReadByte(AddWithinBank(address, 1)) << 8));

    private static int ReadLong(ISnesAddressSpace bus, int address) =>
        bus.ReadByte(address) |
        (bus.ReadByte(AddWithinBank(address, 1)) << 8) |
        (bus.ReadByte(AddWithinBank(address, 2)) << 16);

    private static bool IsNegative16(int value) => (short)unchecked((ushort)value) < 0;
}
