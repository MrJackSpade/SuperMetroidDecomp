namespace SuperMetroid.Core.Game;

/// <summary>The three native function words stored in Zebetite variable A.</summary>
public enum ZebetiteAiFunction : ushort
{
    SpawnLinkedHalf = 0xfc41,
    WaitForDoorTransition = 0xfc5b,
    Active = 0xfc67,
}

/// <summary>
/// Debugger-facing view of the six common variables used by Tourian's Zebetite barrier.
/// Every native word remains in its physical <see cref="RoomEnemySlot"/> so a watch window
/// can be compared directly with <c>$0FA8-$0FB2</c> and the cartridge's linked-slot indices.
/// </summary>
public sealed class ZebetiteEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal ZebetiteEnemyState(RoomEnemySlot slot) => _slot = slot;

    public ZebetiteAiFunction Function
    {
        get => (ZebetiteAiFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Variable C. Native code deliberately stores the palette-cycle counter in physical
    /// slot zero even after the first barrier generation has vacated that slot.
    /// </summary>
    public ushort PaletteCycle
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Variable D, the binary value formed from persistent events five/four/three.</summary>
    public ushort Generation
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Variable F, including the sign bit that requests a linked second half.</summary>
    public ushort GenerationFlags
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Parameter two contains the linked half's native <c>$40</c>-byte slot index.</summary>
    public ushort LinkedNativeIndex
    {
        get => _slot.Parameter2;
        internal set => _slot.Parameter2 = value;
    }

    public bool IsSecondaryHalf => _slot.Parameter1 != 0;
}

/// <summary>
/// Literal translation of Zebetites <c>$E27F</c> at <c>$A6:FB72-$FDCA</c>. The actor is a
/// four-generation persistent barrier: event bits choose the generation on room entry,
/// sign-tagged generations allocate a linked half through the real enemy-slot pool, damage
/// is mirrored by the private shot callback, and killing a primary updates events before
/// spawning the next cartridge-authored enemy record.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort ZebetiteDefinition = 0xe27f;

    private const ushort ZebetiteGenerationFlagsTable = 0xfc03;
    private const ushort ZebetiteYRadiusTable = 0xfc0b;
    private const ushort ZebetiteInstructionTable = 0xfc13;
    private const ushort ZebetiteXPositionTable = 0xfc1b;
    private const ushort ZebetiteUpperYPositionTable = 0xfc23;
    private const ushort ZebetiteLowerYPositionTable = 0xfc2b;
    private const ushort ZebetitePrimarySpawnRecord = 0xfce1;
    private const ushort ZebetiteSecondarySpawnRecord = 0xfcf9;
    private const ushort ZebetiteUpperHealthInstructionTable = 0xfd4a;
    private const ushort ZebetiteLowerHealthInstructionTable = 0xfd54;
    private const ushort ZebetiteMaximumHealth = 1000;
    private const ushort ZebetiteShotSound = 9;
    private const int ZebetitePaletteDestination = 0x0158 / 2;

    private readonly ZebetiteEnemyState?[] _zebetiteStates =
        new ZebetiteEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for each live or previously allocated physical enemy slot.</summary>
    public IReadOnlyList<ZebetiteEnemyState?> ZebetiteStates => _zebetiteStates;

    /// <summary>Most recent library-three sound requested by Zebetite shot AI.</summary>
    public ushort? LastZebetiteSoundEffect { get; private set; }

    /// <summary>
    /// Native <c>palette_change_num</c>. A nonzero scripted palette fade suppresses the
    /// Zebetite's private two-color cycle; normal gameplay leaves this at zero.
    /// </summary>
    public ushort PaletteChangeNumber { get; set; }

    /// <summary>Ports <c>Zebetites_Init</c> at <c>$A6:FB72</c>.</summary>
    private void InitializeZebetite(RoomEnemySlot slot)
    {
        var state = new ZebetiteEnemyState(slot);
        _zebetiteStates[slot.SlotIndex] = state;

        // $8000 is still unnamed globally: its wider engine meaning is not proven. Keep it
        // raw while naming only the independently verified instruction-processing bit.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.Properties = slot.Properties.With(EnemyProperties.SolidToSamus);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.PaletteIndex = EnemyPaletteBits.Palette2;
        slot.VramTilesIndex = 0x0080;
        state.PaletteCycle = 0;
        state.Function = state.IsSecondaryHalf
            ? ZebetiteAiFunction.WaitForDoorTransition
            : ZebetiteAiFunction.SpawnLinkedHalf;

        ushort generation = 0;
        generation = unchecked((ushort)((generation << 1) | (HasZebetiteEvent(EventNumber.ZebetiteDestroyedBit2) ? 1 : 0)));
        generation = unchecked((ushort)((generation << 1) | (HasZebetiteEvent(EventNumber.ZebetiteDestroyedBit1) ? 1 : 0)));
        generation = unchecked((ushort)((generation << 1) | (HasZebetiteEvent(EventNumber.ZebetiteDestroyedBit0) ? 1 : 0)));
        state.Generation = generation;
        if (generation >= 4)
        {
            slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
            return;
        }

        int tableOffset = generation * 2;
        state.GenerationFlags = ReadWord(
            _bus!,
            0xa60000 | unchecked((ushort)(ZebetiteGenerationFlagsTable + tableOffset)));
        slot.YRadius = ReadWord(
            _bus!,
            0xa60000 | unchecked((ushort)(ZebetiteYRadiusTable + tableOffset)));
        slot.CurrentInstruction = ReadWord(
            _bus!,
            0xa60000 | unchecked((ushort)(ZebetiteInstructionTable + tableOffset)));
        slot.XPosition = ReadWord(
            _bus!,
            0xa60000 | unchecked((ushort)(ZebetiteXPositionTable + tableOffset)));
        ushort yTable = state.IsSecondaryHalf
            ? ZebetiteLowerYPositionTable
            : ZebetiteUpperYPositionTable;
        slot.YPosition = ReadWord(
            _bus!,
            0xa60000 | unchecked((ushort)(yTable + tableOffset)));
    }

    /// <summary>Ports the indirect main dispatcher at <c>$A6:FC33</c>.</summary>
    private void RunZebetiteMain(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        if (EarthquakeTimer == 0)
            slot.ShakeTimer = 0;

        switch (state.Function)
        {
            case ZebetiteAiFunction.SpawnLinkedHalf:
                RunZebetiteSpawnLinkedHalf(slot, state);
                break;
            case ZebetiteAiFunction.WaitForDoorTransition:
                RunZebetiteWaitForDoorTransition(slot, state);
                break;
            case ZebetiteAiFunction.Active:
                RunZebetiteActive(slot, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Zebetite function $A6:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void RunZebetiteSpawnLinkedHalf(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        if ((state.GenerationFlags & 0x8000) != 0)
        {
            RoomEnemySlot secondary = SpawnZebetiteFromRecord(ZebetiteSecondarySpawnRecord);
            secondary.Parameter2 = slot.NativeIndex;
            state.LinkedNativeIndex = secondary.NativeIndex;
        }

        state.Function = ZebetiteAiFunction.WaitForDoorTransition;
        RunZebetiteWaitForDoorTransition(slot, state);
    }

    private void RunZebetiteWaitForDoorTransition(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        // WRAM $0795 is shared with normal elevator transitions. Native holds the barrier
        // inert during door loading and enters its active body on the first clear frame.
        if (ElevatorDoorTransitionActive)
            return;
        state.Function = ZebetiteAiFunction.Active;
        RunZebetiteActive(slot, state);
    }

    private void RunZebetiteActive(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        CycleZebetitePalette(slot);
        SelectZebetiteHealthAnimation(slot, state);

        if (slot.Health != 0)
        {
            slot.Health = unchecked((ushort)Math.Min(
                ZebetiteMaximumHealth,
                slot.Health + 1));
            return;
        }

        if (state.IsSecondaryHalf)
        {
            FinishZebetiteDeath(slot);
            return;
        }

        ushort nextGeneration = unchecked((ushort)(state.Generation + 1));
        state.Generation = nextGeneration;
        PublishZebetiteGenerationEvents(nextGeneration);
        FinishZebetiteDeath(slot);
        if (nextGeneration < 4)
            SpawnZebetiteFromRecord(ZebetitePrimarySpawnRecord);
    }

    private void CycleZebetitePalette(RoomEnemySlot slot)
    {
        if (PaletteChangeNumber != 0 || slot.Parameter1 != 0)
            return;

        // This is intentionally slot zero rather than the current actor. Later generations
        // occupy higher slots while retaining the original's vacated WRAM word as the shared
        // palette counter, exactly matching Get_Zebetites(0)->zebet_var_C.
        RoomEnemySlot firstPhysicalSlot = _slots[0];
        ushort paletteCycle = unchecked((ushort)((firstPhysicalSlot.VariableC + 1) & 7));
        firstPhysicalSlot.VariableC = paletteCycle;
        ushort sourcePointer = unchecked((ushort)(4 * paletteCycle - 0x0279));
        _cgram!.LoadFromBus(
            _bus!,
            0xa60000 | sourcePointer,
            colorCount: 2,
            destinationIndex: ZebetitePaletteDestination);
    }

    private void SelectZebetiteHealthAnimation(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        int tier = slot.Health < 200 ? 4 :
            slot.Health < 400 ? 3 :
            slot.Health < 600 ? 2 :
            slot.Health < 800 ? 1 : 0;
        ushort table = (state.GenerationFlags & 0x8000) != 0
            ? ZebetiteLowerHealthInstructionTable
            : ZebetiteUpperHealthInstructionTable;
        slot.CurrentInstruction = ReadWord(
            _bus!,
            0xa60000 | unchecked((ushort)(table + tier * 2)));
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private RoomEnemySlot SpawnZebetiteFromRecord(ushort recordPointer)
    {
        int slotIndex = FirstFreeEnemyIndex / NativeSlotSize;
        if ((uint)slotIndex >= MaximumEnemyCount)
            throw new InvalidOperationException("Zebetite progression exhausted the 32-slot enemy pool.");

        int record = 0xa60000 | recordPointer;
        RoomEnemyPopulationRecord population = new(
            ReadWord(_bus!, record),
            ReadWord(_bus!, record + 2),
            ReadWord(_bus!, record + 4),
            ReadWord(_bus!, record + 6),
            ReadWord(_bus!, record + 8),
            ReadWord(_bus!, record + 10),
            ReadWord(_bus!, record + 12),
            ReadWord(_bus!, record + 14));
        if (population.DefinitionPointer != ZebetiteDefinition)
        {
            throw new InvalidDataException(
                $"Zebetite spawn record $A6:{recordPointer:X4} names enemy " +
                $"${population.DefinitionPointer:X4}.");
        }

        RoomEnemySlot spawned = _slots[slotIndex];
        RoomEnemyDefinition definition = ReadDefinition(_bus!, population.DefinitionPointer);
        InitializeSlotFromDefinition(spawned, population, definition);
        RunInitializationAi(spawned);
        EnemyCount = unchecked((ushort)Math.Max(EnemyCount, slotIndex + 1));
        FirstFreeEnemyIndex = unchecked((ushort)((slotIndex + 1) * NativeSlotSize));
        return spawned;
    }

    /// <summary>Ports the private tail of <c>Zebetites_Shot</c> at <c>$A6:FDAC</c>.</summary>
    private void ResolveZebetiteShotAfterCommon(RoomEnemySlot struck)
    {
        LastZebetiteSoundEffect = ZebetiteShotSound;
        ushort linkedIndex = struck.Parameter2;
        if (linkedIndex >= MaximumEnemyCount * NativeSlotSize ||
            linkedIndex % NativeSlotSize != 0)
        {
            throw new InvalidDataException(
                $"Zebetite slot {struck.SlotIndex} contains invalid linked index " +
                $"${linkedIndex:X4}.");
        }

        RoomEnemySlot linked = SlotFromNativeIndex(linkedIndex);
        linked.Health = struck.Health;
        linked.FlashTimer = struck.FlashTimer;
    }

    private void FinishZebetiteDeath(RoomEnemySlot slot)
    {
        RoomEnemySpawnSnapshot survivingSpawn = slot.Spawn;
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is not null)
        {
            InitializeEnemyProjectileFromDefinition(
                projectile,
                RoomEnemyProjectileKind.EnemyDeathExplosion,
                graphicsIndex: 0);
            projectile.XPosition = slot.XPosition;
            projectile.YPosition = slot.YPosition;
            projectile.EnemyHeaderPointer = slot.EnemyDefinitionPointer;
            projectile.KilledEnemyNativeIndex = slot.NativeIndex;
            projectile.InstructionPointer = ReadWord(
                _bus!,
                0x860000 | EnemyDeathInstructionPointerTable);
            projectile.InstructionTimer = 1;
        }

        int slotIndex = slot.SlotIndex;
        slot.Clear();
        slot.Spawn = survivingSpawn;
        _zebetiteStates[slotIndex] = null;
        EnemiesKilled = unchecked((ushort)(EnemiesKilled + 1));
    }

    private bool HasZebetiteEvent(EventNumber eventNumber) => RequireEvent(eventNumber);

    private void PublishZebetiteGenerationEvents(ushort generation)
    {
        PublishZebetiteEvent(EventNumber.ZebetiteDestroyedBit0, (generation & 1) != 0);
        PublishZebetiteEvent(EventNumber.ZebetiteDestroyedBit1, (generation & 2) != 0);
        PublishZebetiteEvent(EventNumber.ZebetiteDestroyedBit2, (generation & 4) != 0);
    }

    private void PublishZebetiteEvent(EventNumber eventNumber, bool set)
    {
        if (set)
            RequireSetEvent(eventNumber);
        else
            RequireClearEvent(eventNumber);
    }

    private ZebetiteEnemyState RequireZebetiteState(RoomEnemySlot slot) =>
        _zebetiteStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Zebetite state.");
}
