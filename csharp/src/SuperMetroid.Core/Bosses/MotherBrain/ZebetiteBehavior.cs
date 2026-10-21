using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Zebetite species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ZebetiteBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Zebetite];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new ZebetiteEntity(this, slot, common);

    /// <summary>Initializes newly spawned Zebetite slots through the common enemy dispatch path.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Supplies room artwork, cartridge data, and event services used by Zebetites.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates the explosion projectile when a Zebetite half dies.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Clears actor shaking when the room earthquake ends.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Owns physical enemy slots, including slots reused across Zebetite generations.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Receives the Zebetite's private palette-cycle colors.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Exposes the door transition that keeps newly initialized Zebetites inert.</summary>
    private readonly ElevatorBehavior _elevatorBehavior;
    /// <summary>Tracks the room kill count as Zebetite halves are removed.</summary>
    private readonly EnemyRoomStatus _roomStatus;

    /// <summary>Resolves touch reactions through the shared enemy collision rules.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Captures the shared systems used by Zebetite initialization, AI, and reactions.</summary>
    internal ZebetiteBehavior(ZebetiteBehaviorDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
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

    /// <summary>Health ceiling applied while an active Zebetite regenerates each update.</summary>
    private const ushort ZebetiteMaximumHealth = 1000;
    /// <summary>Library-3 sound $09, selected by the Zebetite shot reaction.</summary>
    private static readonly SoundEffectId ZebetiteShotSound = SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 9);

    /// <summary>Most recent library-three sound requested by Zebetite shot AI.</summary>
    public SoundEffectId? LastZebetiteSoundEffect { get; private set; }

    /// <summary>
    /// Native <c>palette_change_num</c>, the count of scripted palette fades in progress. A
    /// nonzero count suppresses the Zebetite's private two-color cycle; normal gameplay leaves
    /// this at zero.
    /// </summary>
    public int PaletteChangeNumber { get; set; }

    /// <summary>
    /// <c>CameraDistanceIndex</c> ($0941), shared by every boss that retargets the camera.
    /// <c>Initialise_Special_Effects_for_New_Room</c> ($88:8347) clears it on each room load.
    /// </summary>
    public CameraDistanceMode CameraDistanceIndex { get; internal set; }

    /// <summary>
    /// The Zebetites' shared palette-cycle counter: physical slot zero's record word C
    /// (<c>Get_Zebetites(0)->zebet_var_C</c>) whichever enemy holds that slot, advanced by every Zebetite in the room.
    /// It restarts with slot zero's record: at room load, when a Zebetite initializes into slot zero, and when a kill
    /// empties a slot-zero Zebetite. Tourian's Zebetite room puts Mother Brain's body in slot zero, and the body never
    /// uses that word.
    /// </summary>
    public int SharedPaletteCycle
    {
        get;
        internal set => field = checked((ushort)value);
    }

    /// <summary>Ports <c>Zebetites_Init</c> at <c>$A6:FB72</c>.</summary>
    internal void InitializeZebetite(RoomEnemySlot slot)
    {
        var state = new ZebetiteEnemyState(slot.Spawn.Population);
        slot.SetSpeciesState(state, () =>
        {
            state.ClearWithRecord();
            if (slot.SlotIndex == 0)
                SharedPaletteCycle = 0;
        });

        // $8000 is still unnamed globally: its wider engine meaning is not proven. Keep it
        // raw while naming only the independently verified instruction-processing bit.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.Properties = slot.Properties.With(EnemyProperties.SolidToSamus);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        slot.PaletteIndex = EnemyPaletteBits.Palette2;
        // Tile 128: the Zebetite artwork follows the room's first 128 enemy tiles.
        slot.VramTilesIndex = 128;
        if (slot.SlotIndex == 0)
            SharedPaletteCycle = 0;
        state.Function = state.IsSecondaryHalf
            ? ZebetiteAiFunction.WaitForDoorTransition
            : ZebetiteAiFunction.SpawnLinkedHalf;

        int generation = 0;
        generation = (generation << 1) | (HasZebetiteEvent(EventNumber.ZebetiteDestroyedBit2) ? 1 : 0);
        generation = (generation << 1) | (HasZebetiteEvent(EventNumber.ZebetiteDestroyedBit1) ? 1 : 0);
        generation = (generation << 1) | (HasZebetiteEvent(EventNumber.ZebetiteDestroyedBit0) ? 1 : 0);
        state.Generation = generation;
        if (generation >= 4)
        {
            slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
            return;
        }

        ZebetiteGenerationDefinition definition = ZebetiteDefinitions.Generation(generation);
        state.GenerationFlags = definition.GenerationFlags;
        slot.Radius = slot.Radius.WithY(definition.YRadius);
        state.Cursor = AnimationProgramCursor<ZebetiteProgram>.Start(definition.Program);
        slot.Position = slot.Position.WithWholeX(definition.XPosition);
        slot.Position = slot.Position.WithWholeY(definition.YPosition(state.IsSecondaryHalf));
    }

    /// <summary>Ports the indirect main dispatcher at <c>$A6:FC33</c>.</summary>
    private void RunZebetiteMain(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        if (_earthquake.EarthquakeTimer.IsZero)
            slot.ShakeTimer = WrappingCountdown.Start(0);

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
                    $"Zebetite function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Creates a linked half when this generation requires one, then enters the door-transition wait.</summary>
    private void RunZebetiteSpawnLinkedHalf(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        if ((state.GenerationFlags & ZebetiteGenerationFlags.LinkedPair) != 0)
        {
            RoomEnemySlot secondary = SpawnZebetite(ZebetiteHalf.Linked);
            secondary.RequireSpeciesState<ZebetiteEnemyState>().LinkedNativeIndex = slot.NativeIndex;
            state.LinkedNativeIndex = secondary.NativeIndex;
        }

        state.Function = ZebetiteAiFunction.WaitForDoorTransition;
        RunZebetiteWaitForDoorTransition(slot, state);
    }

    /// <summary>Starts active AI on the first update after the shared elevator door transition clears.</summary>
    private void RunZebetiteWaitForDoorTransition(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        // WRAM $0795 is shared with normal elevator transitions. Native holds the barrier
        // inert during door loading and enters its active body on the first clear frame.
        if (_elevatorBehavior.ElevatorDoorTransitionActive)
            return;
        state.Function = ZebetiteAiFunction.Active;
        RunZebetiteActive(slot, state);
    }

    /// <summary>Regenerates living halves and advances the primary generation when its health reaches zero.</summary>
    private void RunZebetiteActive(RoomEnemySlot slot, ZebetiteEnemyState state)
    {
        CycleZebetitePalette(slot);
        SelectZebetiteHealthAnimation(slot, state);

        if (slot.Health != 0)
        {
            slot.Health = Math.Min(ZebetiteMaximumHealth, slot.Health + 1);
            return;
        }

        if (state.IsSecondaryHalf)
        {
            FinishZebetiteDeath(slot);
            return;
        }

        int nextGeneration = state.Generation + 1;
        state.Generation = nextGeneration;
        PublishZebetiteGenerationEvents(nextGeneration);
        FinishZebetiteDeath(slot);
        if (nextGeneration < 4)
            SpawnZebetite(ZebetiteHalf.Primary);
    }

    /// <summary>Applies the shared two-color cycle using physical slot zero's native counter when no fade suppresses it.</summary>
    internal void CycleZebetitePalette(RoomEnemySlot slot)
    {
        if (PaletteChangeNumber != 0 || slot.RequireSpeciesState<ZebetiteEnemyState>().IsSecondaryHalf)
            return;

        int paletteCycle = (SharedPaletteCycle + 1) & ZebetiteDefinitions.PaletteCycleMask;
        SharedPaletteCycle = paletteCycle;
        (_bossArtwork.Artwork?.ZebetiteColors ?? throw new InvalidOperationException(
            "Zebetite requires installed palette-cycle colors."))
            .Apply(_presentation.Cgram, paletteCycle, ZebetiteDefinitions.PaletteDestinationColor);
    }

    /// <summary>Restarts the instruction list chosen from the half's pairing and current health.</summary>
    internal static void SelectZebetiteHealthAnimation(
        RoomEnemySlot slot,
        ZebetiteEnemyState state)
    {
        state.Cursor = AnimationProgramCursor<ZebetiteProgram>.Start(ZebetiteAnimations.HealthProgram(
            linkedPair: (state.GenerationFlags & ZebetiteGenerationFlags.LinkedPair) != 0,
            slot.Health));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Finds a free physical slot and initializes a primary or linked half from its population record.</summary>
    internal RoomEnemySlot SpawnZebetite(ZebetiteHalf half)
    {
        // A0:9275 scans physical slots from zero. In particular, a primary's
        // death vacates its slot before spawning the next generation; surviving
        // linked halves retain pointers to that reused physical slot.
        int slotIndex = _slotTable.FindFreeSlotIndex();
        if (slotIndex < 0)
            throw new InvalidOperationException("Zebetite progression exhausted the 32-slot enemy pool.");

        RoomEnemyPopulationRecord population =
            ZebetiteDefinitions.SpawnPopulation(half);
        if (population.DefinitionPointer != EnemyDefinitionId.Zebetite)
        {
            throw new InvalidDataException(
                $"Zebetite {half} spawn record names enemy " +
                $"${(int)population.DefinitionPointer:X4}.");
        }

        RoomEnemySlot spawned = _slotTable.Slots[slotIndex];
        RoomEnemyDefinition definition = _room.LoadedDefinitions.ReadEnemyDefinition(population.DefinitionPointer);
        _dispatch.InitializeSlotFromDefinition(spawned, population, definition);
        _dispatch.RunInitializationAi(spawned, EnemySpawnSurroundings.None);
        _slotTable.EnemyCount = unchecked((ushort)Math.Max(_slotTable.EnemyCount, slotIndex + 1));
        _slotTable.FirstFreeEnemyIndex = unchecked((ushort)Math.Max(_slotTable.FirstFreeEnemyIndex, (slotIndex + 1) * EnemySlotTable.NativeSlotSize));
        return spawned;
    }

    /// <summary>Ports the private tail of <c>Zebetites_Shot</c> at <c>$A6:FDAC</c>.</summary>
    private void ResolveZebetiteShotAfterCommon(RoomEnemySlot struck)
    {
        LastZebetiteSoundEffect = ZebetiteShotSound;
        ushort linkedIndex = checked((ushort)struck.RequireSpeciesState<ZebetiteEnemyState>().LinkedNativeIndex);
        if (linkedIndex >= EnemySlotTable.MaximumEnemyCount * EnemySlotTable.NativeSlotSize ||
            linkedIndex % EnemySlotTable.NativeSlotSize != 0)
        {
            throw new InvalidDataException(
                $"Zebetite slot {struck.SlotIndex} contains invalid linked index " +
                $"${linkedIndex:X4}.");
        }

        RoomEnemySlot linked = _slotTable.SlotFromNativeIndex(linkedIndex);
        linked.Health = struck.Health;
        linked.FlashTimer = struck.FlashTimer;
    }

    /// <summary>Starts the death explosion, frees the half's slot, and records its room kill.</summary>
    private void FinishZebetiteDeath(RoomEnemySlot slot)
    {
        RoomEnemySpawnSnapshot survivingSpawn = slot.Spawn;
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is not null)
        {
            MotherBrainProjectileStarts.Initialize(
                projectile,
                RoomEnemyProjectileKind.EnemyDeathExplosion,
                graphicsIndex: 0);
            projectile.Position = projectile.Position.WithWhole(slot.Position.Whole);
            projectile.EnemyHeaderPointer = slot.EnemyDefinitionPointer;
            projectile.KilledEnemyNativeIndex = slot.NativeIndex;
            projectile.StartAnimation(new EnemyDeathProjectileAnimation(EnemyDeathProjectileProgram.SmallExplosion));
        }

        int slotIndex = slot.SlotIndex;
        slot.Clear();
        slot.Spawn = survivingSpawn;
        _slotTable.Slots[slotIndex].SetSpeciesState<ZebetiteEnemyState>(null);
        _roomStatus.RecordKill();
    }

    /// <summary>Reads one persistent bit used to reconstruct the current Zebetite generation.</summary>
    private bool HasZebetiteEvent(EventNumber eventNumber) => _room.Services.Events.Has(eventNumber);

    /// <summary>Stores the next generation as three persistent Zebetite-destroyed event bits.</summary>
    private void PublishZebetiteGenerationEvents(int generation)
    {
        PublishZebetiteEvent(EventNumber.ZebetiteDestroyedBit0, (generation & 1) != 0);
        PublishZebetiteEvent(EventNumber.ZebetiteDestroyedBit1, (generation & 2) != 0);
        PublishZebetiteEvent(EventNumber.ZebetiteDestroyedBit2, (generation & 4) != 0);
    }

    /// <summary>Sets or clears one persistent Zebetite generation event.</summary>
    private void PublishZebetiteEvent(EventNumber eventNumber, bool set)
    {
        if (set)
            _room.Services.Events.Set(eventNumber);
        else
            _room.Services.Events.Clear(eventNumber);
    }

    /// <summary>
    /// Runs a half's health program where the enemy instruction interpreter would: its pose shows on the update the
    /// native timer expires, raising the new-frame flag, and the native sleep then keeps it.
    /// </summary>
    internal static void AdvanceZebetiteAnimation(RoomEnemySlot slot)
    {
        ZebetiteEnemyState state = slot.RequireSpeciesState<ZebetiteEnemyState>();
        ushort oldTimer = slot.InstructionTimer.RawValue;
        slot.InstructionTimer = WrappingCountdown.FromRaw(unchecked((ushort)(oldTimer - 1)));
        if (oldTimer != 1)
        {
            slot.ExtraProperties = slot.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        AnimationProgramCursor<ZebetiteProgram> cursor = state.Cursor;
        WrappingCountdown counter = slot.Timer;
        AnimationProgramAdvance<ZebetiteFrame> advance = ZebetiteAnimations.Programs.Advance(ref cursor, ref counter,
            (cue, continuation) => throw new InvalidOperationException($"Zebetite programs publish no cue {cue}."));
        slot.Timer = counter;
        state.Cursor = cursor;
        switch (advance)
        {
            case AnimationProgramAdvance<ZebetiteFrame>.Show shown:
                slot.InstructionTimer = WrappingCountdown.FromRaw((ushort)shown.Duration);
                slot.Frame = ZebetiteVisualDefinitions.ShownFrames.Of(shown.Frame);
                slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
                return;
            case AnimationProgramAdvance<ZebetiteFrame>.Hold:
                // The native sleep: the timer wraps from zero and the pose stays.
                return;
            default:
                throw new InvalidOperationException($"Zebetite programs have no {advance} step.");
        }
    }

    /// <summary>Registers bombs to share the Zebetite shot tail without generic death handling.</summary>
    internal void RegisterZebetiteBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Zebetite &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.ZebetiteShot)
        {
            RunsGenericDeath = false,
            AfterCommon = c => ResolveZebetiteShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Ports Zebetite's main AI.</summary>
    internal void RunZebetiteMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunZebetiteMain(slot, slot.RequireSpeciesState<ZebetiteEnemyState>());
    }

    /// <summary>Registers shot damage with the Zebetite-specific linked-health tail and no generic death.</summary>
    internal void RegisterZebetiteShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Zebetite &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.ZebetiteShot)
        {
            AlwaysUsesCollisionPrelude = true,
            ExemptFromGenericDeath = true,
            AfterCommon = c => ResolveZebetiteShotAfterCommon(c.Enemy),
        });
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastZebetiteSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastZebetiteSoundEffect is SoundEffectId sound)
            audio.QueueEnemySound(sound, maximumQueued: 6);
    }

    /// <summary>Routes Zebetite contact through normal touch collision while skipping generic death animation.</summary>
    internal void RegisterZebetiteTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Zebetite &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.ZebetiteTouch,
            context => _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip));
    }
}
