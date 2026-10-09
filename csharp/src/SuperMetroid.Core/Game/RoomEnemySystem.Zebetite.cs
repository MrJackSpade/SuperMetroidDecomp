using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>The three native function words stored in Zebetite variable A.</summary>
public enum ZebetiteAiFunction : ushort
{
    /// <summary>$A6:FC41, Function_Zebetite_SpawnBottomZebetiteIfNeeded: primary setup allocates a linked half when the generation flag is negative, links native slot indices, then enters the door wait in the same update.</summary>
    SpawnLinkedHalf = 0xfc41,
    /// <summary>$A6:FC5B, Function_Zebetite_WaitForDoorTransitionToFinish: remains inert while shared WRAM $0795 is nonzero, then installs and executes the active function immediately.</summary>
    WaitForDoorTransition = 0xfc5b,
    /// <summary>$A6:FC67, Function_Zebetite_Active: updates palette/health artwork, regenerates one health per call up to 1000, or handles death with primary-owned generation progression.</summary>
    Active = 0xfc67,
}

/// <summary>
/// Debugger-facing view of the six common variables used by Tourian's Zebetite barrier.
/// Every native word remains in its physical <see cref="RoomEnemySlot"/> so a watch window
/// can be compared directly with <c>$0FA8-$0FB2</c> and the cartridge's linked-slot indices.
/// </summary>
public sealed class ZebetiteEnemyState
{
    /// <summary>Physical enemy slot containing the cartridge variables projected by this view.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a debugger-facing view over an initialized Zebetite slot.</summary>
    /// <param name="slot">Live enemy slot whose common words store the Zebetite state.</param>
    internal ZebetiteEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Native Zebetite.function in variable A ($0FA8 plus this half's slot offset): the bank-$A6 indirect main-AI entry, independent for each physical half.</summary>
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

    /// <summary>Nonzero native init0/parameter one identifies the linked bottom half; it skips primary palette cycling and dies without publishing the next generation's events or respawn.</summary>
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
    /// <summary>Bank-$A6 enemy definition pointer used by primary and linked Zebetite population records.</summary>
    internal const ushort ZebetiteDefinition = 0xe27f;

    /// <summary>Maximum health to which an active Zebetite regenerates.</summary>
    private const ushort ZebetiteMaximumHealth = 1000;

    /// <summary>Library-three sound effect requested by the native Zebetite shot callback.</summary>
    private const ushort ZebetiteShotSound = 9;

    /// <summary>Typed variable views indexed by physical enemy slot; cleared when a Zebetite dies.</summary>
    private readonly ZebetiteEnemyState?[] _zebetiteStates =
        new ZebetiteEnemyState?[MaximumEnemyCount];

    /// <summary>Most recent library-three sound requested by Zebetite shot AI.</summary>
    public ushort? LastZebetiteSoundEffect { get; private set; }

    /// <summary>
    /// Native <c>palette_change_num</c>. A nonzero scripted palette fade suppresses the
    /// Zebetite's private two-color cycle; normal gameplay leaves this at zero.
    /// </summary>
    public ushort PaletteChangeNumber { get; set; }

    /// <summary>
    /// <c>CameraDistanceIndex</c> ($0941), shared by every boss that retargets the camera.
    /// <c>Initialise_Special_Effects_for_New_Room</c> ($88:8347) clears it on each room load.
    /// </summary>
    public CameraDistanceMode CameraDistanceIndex { get; internal set; }

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

        ZebetiteGenerationDefinition definition = ZebetiteDefinitions.Generation(generation);
        state.GenerationFlags = definition.GenerationFlags;
        slot.YRadius = definition.YRadius;
        slot.CurrentInstruction = definition.InstructionList;
        slot.XPosition = definition.XPosition;
        slot.YPosition = definition.YPosition(state.IsSecondaryHalf);
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

    /// <summary>Allocates and links a secondary half when requested by the generation flags, then proceeds to door gating.</summary>
    /// <param name="slot">Primary barrier slot whose generation controls linked-half allocation.</param>
    /// <param name="state">Primary state receiving the spawned half's native slot index.</param>
    private void RunZebetiteSpawnLinkedHalf(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        if ((state.GenerationFlags & 0x8000) != 0)
        {
            RoomEnemySlot secondary = SpawnZebetite(linkedHalf: true);
            secondary.Parameter2 = slot.NativeIndex;
            state.LinkedNativeIndex = secondary.NativeIndex;
        }

        state.Function = ZebetiteAiFunction.WaitForDoorTransition;
        RunZebetiteWaitForDoorTransition(slot, state);
    }

    /// <summary>Holds a barrier inert during the shared door transition and activates it on the first clear update.</summary>
    /// <param name="slot">Barrier slot to activate after the transition.</param>
    /// <param name="state">State whose function changes from transition wait to active behavior.</param>
    private void RunZebetiteWaitForDoorTransition(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        // WRAM $0795 is shared with normal elevator transitions. Native holds the barrier
        // inert during door loading and enters its active body on the first clear frame.
        if (ElevatorDoorTransitionActive)
            return;
        state.Function = ZebetiteAiFunction.Active;
        RunZebetiteActive(slot, state);
    }

    /// <summary>Cycles active artwork, regenerates health, or advances generation events and death progression.</summary>
    /// <param name="slot">Barrier half whose health and lifetime are updated.</param>
    /// <param name="state">Generation and linked-half state governing the death path.</param>
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
            SpawnZebetite(linkedHalf: false);
    }

    /// <summary>Advances the primary-owned two-color cycle unless a scripted fade or secondary half suppresses it.</summary>
    /// <param name="slot">Barrier slot whose parameters determine whether palette cycling is enabled.</param>
    private void CycleZebetitePalette(RoomEnemySlot slot)
    {
        if (PaletteChangeNumber != 0 || slot.Parameter1 != 0)
            return;

        // This is intentionally slot zero rather than the current actor. Later generations
        // occupy higher slots while retaining the original's vacated WRAM word as the shared
        // palette counter, exactly matching Get_Zebetites(0)->zebet_var_C.
        RoomEnemySlot firstPhysicalSlot = _slots[0];
        ushort paletteCycle = unchecked((ushort)((firstPhysicalSlot.VariableC + 1) &
            ZebetiteDefinitions.PaletteCycleMask));
        firstPhysicalSlot.VariableC = paletteCycle;
        (TileArtwork?.ZebetiteColors ?? throw new InvalidOperationException(
            "Zebetite requires installed palette-cycle colors."))
            .Apply(_cgram!, paletteCycle, ZebetiteDefinitions.PaletteDestinationColor);
    }

    /// <summary>Selects the compiled health-stage instruction list for this generation and current HP.</summary>
    /// <param name="slot">Barrier slot receiving the instruction pointer and fresh timers.</param>
    /// <param name="state">Generation flags used to select paired or single-barrier artwork.</param>
    private static void SelectZebetiteHealthAnimation(
        RoomEnemySlot slot,
        ZebetiteEnemyState state)
    {
        slot.CurrentInstruction = ZebetiteDefinitions.HealthInstruction(
            linkedPair: (state.GenerationFlags & 0x8000) != 0,
            slot.Health);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Allocates a free native enemy slot and initializes a primary or linked-half population record.</summary>
    /// <param name="linkedHalf">Whether to use the generation's secondary-half spawn record.</param>
    /// <returns>The initialized slot after its initialization AI has run.</returns>
    /// <exception cref="InvalidOperationException">The native enemy slot pool has no free slot.</exception>
    /// <exception cref="InvalidDataException">The compiled spawn record does not identify a Zebetite.</exception>
    private RoomEnemySlot SpawnZebetite(bool linkedHalf)
    {
        // A0:9275 scans physical slots from zero. In particular, a primary's
        // death vacates its slot before spawning the next generation; surviving
        // linked halves retain pointers to that reused physical slot.
        int slotIndex = Array.FindIndex(_slots, candidate => candidate.EnemyDefinitionPointer == 0);
        if (slotIndex < 0)
            throw new InvalidOperationException("Zebetite progression exhausted the 32-slot enemy pool.");

        RoomEnemyPopulationRecord population =
            ZebetiteDefinitions.SpawnPopulation(linkedHalf);
        if (population.DefinitionPointer != ZebetiteDefinition)
        {
            throw new InvalidDataException(
                $"Zebetite {(linkedHalf ? "linked" : "primary")} spawn record names enemy " +
                $"${population.DefinitionPointer:X4}.");
        }

        RoomEnemySlot spawned = _slots[slotIndex];
        RoomEnemyDefinition definition = ResolveRoomEnemyDefinition(_bus!, population.DefinitionPointer);
        InitializeSlotFromDefinition(spawned, population, definition);
        RunInitializationAi(spawned);
        EnemyCount = unchecked((ushort)Math.Max(EnemyCount, slotIndex + 1));
        FirstFreeEnemyIndex = unchecked((ushort)Math.Max(FirstFreeEnemyIndex, (slotIndex + 1) * NativeSlotSize));
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

    /// <summary>Creates the death effect when possible, clears the actor while preserving spawn provenance, and counts its death.</summary>
    /// <param name="slot">Defeated barrier slot to release.</param>
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
            projectile.InstructionPointer = EnemyDeathExplosionDefinitions
                .InstructionPointer((ushort)EnemyDeathAnimation.SmallExplosion);
            projectile.InstructionTimer = 1;
        }

        int slotIndex = slot.SlotIndex;
        slot.Clear();
        slot.Spawn = survivingSpawn;
        _zebetiteStates[slotIndex] = null;
        EnemiesKilled = unchecked((ushort)(EnemiesKilled + 1));
    }

    /// <summary>Reads one persistent event bit used to reconstruct the barrier generation.</summary>
    /// <param name="eventNumber">Event bit identifying a destroyed Zebetite generation.</param>
    /// <returns><see langword="true"/> when the persistent event is set.</returns>
    private bool HasZebetiteEvent(EventNumber eventNumber) => RequireEvent(eventNumber);

    /// <summary>Writes all three persistent destruction bits from the encoded next-generation value.</summary>
    /// <param name="generation">Generation number whose low bits map to the three destruction events.</param>
    private void PublishZebetiteGenerationEvents(ushort generation)
    {
        PublishZebetiteEvent(EventNumber.ZebetiteDestroyedBit0, (generation & 1) != 0);
        PublishZebetiteEvent(EventNumber.ZebetiteDestroyedBit1, (generation & 2) != 0);
        PublishZebetiteEvent(EventNumber.ZebetiteDestroyedBit2, (generation & 4) != 0);
    }

    /// <summary>Sets or clears one generation event to match its corresponding bit.</summary>
    /// <param name="eventNumber">Persistent event bit to update.</param>
    /// <param name="set">Whether the event must be set; <see langword="false"/> clears it.</param>
    private void PublishZebetiteEvent(EventNumber eventNumber, bool set)
    {
        if (set)
            RequireSetEvent(eventNumber);
        else
            RequireClearEvent(eventNumber);
    }

    /// <summary>Gets the typed state installed for a Zebetite slot or reports an initialization-order violation.</summary>
    /// <param name="slot">Zebetite slot whose state view is required.</param>
    /// <returns>The state created by the Zebetite initializer.</returns>
    /// <exception cref="InvalidOperationException">No Zebetite state has been initialized for the slot.</exception>
    private ZebetiteEnemyState RequireZebetiteState(RoomEnemySlot slot) =>
        _zebetiteStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Zebetite state.");
}
