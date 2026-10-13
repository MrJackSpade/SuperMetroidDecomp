using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>The Zebetite species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class ZebetiteBehavior : IEnemySpecies
{
    private readonly IEnemyDispatch _dispatch;
    private readonly EnemyRoomBinding _room;
    private readonly EnemyProjectileEngine _projectiles;
    private readonly RoomEarthquake _earthquake;
    private readonly EnemySlotTable _slotTable;
    private readonly EnemyPresentation _presentation;

    private readonly ElevatorBehavior _elevatorBehavior;
    private readonly EnemyRoomStatus _roomStatus;

    private readonly EnemyCollision _collision;

    internal ZebetiteBehavior(ZebetiteBehaviorDependencies dependencies)
    {
        _collision = dependencies.Collision;
        _elevatorBehavior = dependencies.ElevatorBehavior;
        _dispatch = dependencies.Dispatch;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        _roomStatus = dependencies.RoomStatus;
    }

    private const ushort ZebetiteMaximumHealth = 1000;
    private const ushort ZebetiteShotSound = 9;

    internal readonly ZebetiteEnemyState?[] _zebetiteStates =
        new ZebetiteEnemyState?[RoomEnemySystem.MaximumEnemyCount];

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
    internal void InitializeZebetite(RoomEnemySlot slot)
    {
        var state = new ZebetiteEnemyState(slot);
        _zebetiteStates[slot.SlotIndex] = state;

        // $8000 is still unnamed globally: its wider engine meaning is not proven. Keep it
        // raw while naming only the independently verified instruction-processing bit.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.Properties = slot.Properties.With(EnemyProperties.SolidToSamus);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
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
        slot.Radius = slot.Radius.WithY(definition.YRadius);
        slot.CurrentInstruction = definition.InstructionList;
        slot.Position = slot.Position.WithWholeX(definition.XPosition);
        slot.Position = slot.Position.WithWholeY(definition.YPosition(state.IsSecondaryHalf));
    }

    /// <summary>Ports the indirect main dispatcher at <c>$A6:FC33</c>.</summary>
    private void RunZebetiteMain(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        if (_earthquake.EarthquakeTimer.IsZero)
            slot.ShakeTimer = NativeCountdown.Start(0);

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
            RoomEnemySlot secondary = SpawnZebetite(linkedHalf: true);
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
        if (_elevatorBehavior.ElevatorDoorTransitionActive)
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
            SpawnZebetite(linkedHalf: false);
    }

    internal void CycleZebetitePalette(RoomEnemySlot slot)
    {
        if (PaletteChangeNumber != 0 || slot.Parameter1 != 0)
            return;

        // This is intentionally slot zero rather than the current actor. Later generations
        // occupy higher slots while retaining the original's vacated WRAM word as the shared
        // palette counter, exactly matching Get_Zebetites(0)->zebet_var_C.
        RoomEnemySlot firstPhysicalSlot = _slotTable._slots[0];
        ushort paletteCycle = unchecked((ushort)((firstPhysicalSlot.VariableC + 1) &
            ZebetiteDefinitions.PaletteCycleMask));
        firstPhysicalSlot.VariableC = paletteCycle;
        (_room.Artwork?.ZebetiteColors ?? throw new InvalidOperationException(
            "Zebetite requires installed palette-cycle colors."))
            .Apply(_presentation._cgram!, paletteCycle, ZebetiteDefinitions.PaletteDestinationColor);
    }

    internal static void SelectZebetiteHealthAnimation(
        RoomEnemySlot slot,
        ZebetiteEnemyState state)
    {
        slot.CurrentInstruction = ZebetiteDefinitions.HealthInstruction(
            linkedPair: (state.GenerationFlags & 0x8000) != 0,
            slot.Health);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    internal RoomEnemySlot SpawnZebetite(bool linkedHalf)
    {
        // A0:9275 scans physical slots from zero. In particular, a primary's
        // death vacates its slot before spawning the next generation; surviving
        // linked halves retain pointers to that reused physical slot.
        int slotIndex = Array.FindIndex(_slotTable._slots, candidate => candidate.EnemyDefinitionPointer == 0);
        if (slotIndex < 0)
            throw new InvalidOperationException("Zebetite progression exhausted the 32-slot enemy pool.");

        RoomEnemyPopulationRecord population =
            ZebetiteDefinitions.SpawnPopulation(linkedHalf);
        if (population.DefinitionPointer != EnemyDefinitionId.Zebetite)
        {
            throw new InvalidDataException(
                $"Zebetite {(linkedHalf ? "linked" : "primary")} spawn record names enemy " +
                $"${(int)population.DefinitionPointer:X4}.");
        }

        RoomEnemySlot spawned = _slotTable._slots[slotIndex];
        RoomEnemyDefinition definition = EnemyPopulationLoader.ResolveRoomEnemyDefinition(_room.Bus!, population.DefinitionPointer);
        _dispatch.InitializeSlotFromDefinition(spawned, population, definition);
        _dispatch.RunInitializationAi(spawned);
        _slotTable.EnemyCount = unchecked((ushort)Math.Max(_slotTable.EnemyCount, slotIndex + 1));
        _slotTable.FirstFreeEnemyIndex = unchecked((ushort)Math.Max(_slotTable.FirstFreeEnemyIndex, (slotIndex + 1) * RoomEnemySystem.NativeSlotSize));
        return spawned;
    }

    /// <summary>Ports the private tail of <c>Zebetites_Shot</c> at <c>$A6:FDAC</c>.</summary>
    private void ResolveZebetiteShotAfterCommon(RoomEnemySlot struck)
    {
        LastZebetiteSoundEffect = ZebetiteShotSound;
        ushort linkedIndex = struck.Parameter2;
        if (linkedIndex >= RoomEnemySystem.MaximumEnemyCount * RoomEnemySystem.NativeSlotSize ||
            linkedIndex % RoomEnemySystem.NativeSlotSize != 0)
        {
            throw new InvalidDataException(
                $"Zebetite slot {struck.SlotIndex} contains invalid linked index " +
                $"${linkedIndex:X4}.");
        }

        RoomEnemySlot linked = _slotTable.SlotFromNativeIndex(linkedIndex);
        linked.Health = struck.Health;
        linked.FlashTimer = struck.FlashTimer;
    }

    private void FinishZebetiteDeath(RoomEnemySlot slot)
    {
        RoomEnemySpawnSnapshot survivingSpawn = slot.Spawn;
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is not null)
        {
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                projectile,
                RoomEnemyProjectileKind.EnemyDeathExplosion,
                graphicsIndex: 0);
            projectile.Position = projectile.Position.WithWhole(slot.Position.Whole);
            projectile.EnemyHeaderPointer = slot.EnemyDefinitionPointer;
            projectile.KilledEnemyNativeIndex = slot.NativeIndex;
            projectile.InstructionPointer = EnemyDeathExplosionDefinitions
                .InstructionPointer((ushort)EnemyDeathAnimation.SmallExplosion);
            projectile.InstructionTimer = NativeCountdown.Start(1);
        }

        int slotIndex = slot.SlotIndex;
        slot.Clear();
        slot.Spawn = survivingSpawn;
        _zebetiteStates[slotIndex] = null;
        _roomStatus.EnemiesKilled = unchecked((ushort)(_roomStatus.EnemiesKilled + 1));
    }

    private bool HasZebetiteEvent(EventNumber eventNumber) => _room.Services.Events.Has(eventNumber);

    private void PublishZebetiteGenerationEvents(ushort generation)
    {
        PublishZebetiteEvent(EventNumber.ZebetiteDestroyedBit0, (generation & 1) != 0);
        PublishZebetiteEvent(EventNumber.ZebetiteDestroyedBit1, (generation & 2) != 0);
        PublishZebetiteEvent(EventNumber.ZebetiteDestroyedBit2, (generation & 4) != 0);
    }

    private void PublishZebetiteEvent(EventNumber eventNumber, bool set)
    {
        if (set)
            _room.Services.Events.Set(eventNumber);
        else
            _room.Services.Events.Clear(eventNumber);
    }

    private ZebetiteEnemyState RequireZebetiteState(RoomEnemySlot slot) =>
        _zebetiteStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Zebetite state.");

    internal void RegisterZebetiteBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Zebetite &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.ZebetiteShot)
        {
            RunsGenericDeath = false,
            AfterCommon = c => ResolveZebetiteShotAfterCommon(c.Enemy),
        });
    }

    internal void RegisterZebetiteInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Zebetite, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Zebetite, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeZebetite(slot);
        });
    }

    internal void RegisterZebetiteMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Zebetite, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Zebetite, (slot, frame, nmiFrameCounter8) =>
        {
            RunZebetiteMain(slot, RequireZebetiteState(slot));
        });
    }

    internal void RegisterZebetiteShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Zebetite &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.ZebetiteShot)
        {
            AlwaysUsesCollisionPrelude = true,
            ExemptFromGenericDeath = true,
            AfterCommon = (c, _) => ResolveZebetiteShotAfterCommon(c.Enemy),
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastZebetiteSoundEffect = null;
        PaletteChangeNumber = 0;
        CameraDistanceIndex = CameraDistanceMode.NormalTracking;
        Array.Clear(_zebetiteStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastZebetiteSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastZebetiteSoundEffect, library: SoundEffectLibrary.Library3, maximumQueued: 6);
    }


    internal void RegisterZebetiteTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Zebetite &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.ZebetiteTouch,
            context => _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, skipDeathAnimation: true));
    }
}
