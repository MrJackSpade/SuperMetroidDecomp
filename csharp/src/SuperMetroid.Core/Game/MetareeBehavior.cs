using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Metaree species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class MetareeBehavior : IEnemySpecies
{

    /// <summary>Creates the debris burst during burrow and death reactions.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Uses the room's projectile engine for Metaree's four-piece metal debris.</summary>
    internal MetareeBehavior(EnemyProjectileEngine projectiles)
    {
        _projectiles = projectiles;
    }

    /// <summary>Publishes attack readiness at the end of the preparation list; main AI consumes it on the following enemy frame.</summary>
    private bool TryProcessMetareeInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Metaree ||
            !Enum.IsDefined((MetareeInstruction)word))
            return false;

        switch ((MetareeInstruction)word)
        {
            case MetareeInstruction.SetAttackReadyFlag:
                // The preparation list sleeps immediately after publishing this flag.
                // Main AI consumes it on the following enemy frame and installs the
                // launched list, exactly matching the native instruction/AI hand-off.
                RequireMetareeState(slot).AttackReady = true;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Metaree does not own instruction ${word:X4}.");
        }
    }

    /// <summary>Maximum strict horizontal separation for starting a dive, regardless of Samus's vertical side.</summary>
    private const ushort MetareeHorizontalActivationDistance = 0x48;
    /// <summary>Unsigned Y separation divisor used to choose NTSC dive speed.</summary>
    private const ushort MetareeDiveDivisorNtsc = 24;
    /// <summary>Whole-pixel downward speed added even for zero vertical separation.</summary>
    private const ushort MetareeMinimumYVelocity = 4;
    /// <summary>Whole-pixel X steering toward Samus, reevaluated every airborne frame.</summary>
    private const ushort MetareeHorizontalDiveSpeed = 2;
    /// <summary>Ground-impact countdown before the actor deletes itself.</summary>
    private const ushort MetareeBurrowLifetimeNtsc = 21;
    /// <summary>Burrow countdown value that spawns the particle burst.</summary>
    private const ushort MetareeParticleFrameNtsc = 8;

    /// <summary>AI phase and animation-list state indexed by native enemy slot.</summary>
    internal readonly MetareeEnemyState?[] _metareeStates =
        new MetareeEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Starts the idle list directly and initializes the slot's dive, burrow, and instruction-change state.</summary>
    internal void InitializeMetaree(RoomEnemySlot slot)
    {
        _metareeStates[slot.SlotIndex] = new MetareeEnemyState(slot)
        {
            BurrowTimer = NativeCountdown.Start(0),
            Function = MetareeEnemyFunction.Idling,
            RequestedInstructionListIndex = SkreeMetareeAnimationPhase.Idling,
            InstalledInstructionListIndex = SkreeMetareeAnimationPhase.Idling,
            AttackReady = false,
            YVelocity = 0,
        };

        // The initializer writes the idle pointer directly rather than going through the
        // change detector, because both requested and installed indexes intentionally start
        // at zero. InitializeEnemies clears the immediate map afterward; frame one restores
        // it from this real ROM list through the common instruction interpreter.
        slot.CurrentInstruction = OrdinaryEnemyInstructionLists.MetareeInitial;
    }

    /// <summary>Dispatches idle, attack preparation, airborne dive, and burrow phases with required Samus and room data.</summary>
    private void RunMetareeMain(
        RoomEnemySlot slot,
        MetareeEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Metaree AI requires the active Samus actor.");

        switch (state.Function)
        {
            case MetareeEnemyFunction.Idling:
                RunMetareeIdle(slot, state, samus);
                return;

            case MetareeEnemyFunction.PreparingAttack:
                RunMetareePreparation(slot, state);
                return;

            case MetareeEnemyFunction.LaunchedAttack:
                if (level is null)
                {
                    throw new InvalidOperationException(
                        "Metaree dive collision requires active room level data.");
                }
                RunMetareeDive(slot, state, samus, level);
                return;

            case MetareeEnemyFunction.Burrowing:
                RunMetareeBurrow(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Metaree function $A3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Starts preparation within 72 horizontal pixels and derives dive speed from the wrapped unsigned Y difference.</summary>
    internal static void RunMetareeIdle(
        RoomEnemySlot slot,
        MetareeEnemyState state,
        SamusState samus)
    {
        // `$A3:898A-$899A` computes a wrapped signed difference, takes its two's-complement
        // absolute value, and performs an unsigned compare. It notably does *not* check that
        // Samus is below the actor; preserving that retail bug matters to the velocity math.
        int horizontalDistance = Math.Abs(
            unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)));
        if (horizontalDistance >= MetareeHorizontalActivationDistance)
            return;

        // The SNES unsigned-divider input is the wrapped 16-bit Y difference. If Samus is
        // above Metaree, this deliberately creates the cartridge's enormous dive velocity
        // instead of sanitizing the old bug into modern signed physics.
        ushort verticalDifference = unchecked((ushort)(samus.Position.Y.Whole - slot.Position.Y.Whole));
        state.YVelocity = unchecked((ushort)(
            verticalDifference / MetareeDiveDivisorNtsc + MetareeMinimumYVelocity));
        state.RequestedInstructionListIndex = SkreeMetareeAnimationPhase.PreparingAttack;
        InstallRequestedMetareeInstruction(slot, state);
        state.Function = MetareeEnemyFunction.PreparingAttack;
    }

    /// <summary>Consumes the preparation list's ready flag, installs the dive list, and publishes sound $5B.</summary>
    internal void RunMetareePreparation(RoomEnemySlot slot, MetareeEnemyState state)
    {
        if (!state.AttackReady)
            return;

        state.AttackReady = false;
        state.RequestedInstructionListIndex = SkreeMetareeAnimationPhase.Diving;
        InstallRequestedMetareeInstruction(slot, state);
        state.Function = MetareeEnemyFunction.LaunchedAttack;

        // QueueSound_Lib2_Max6 receives $5B at $A3:89EB. Audio mixing remains an outer
        // subsystem, so expose the exact event word for the frontend/debugger seam.
        LastMetareeSoundEffect = 0x005b;
    }

    /// <summary>Probes solid terrain ahead before whole-pixel descent; ground contact begins burrowing while airborne frames steer toward Samus.</summary>
    private void RunMetareeDive(
        RoomEnemySlot slot,
        MetareeEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // Native AI reloads this every airborne frame. It becomes a countdown only after
        // the solid-bit probe reports the ground, which is why interrupted dives never age.
        state.BurrowTimer = NativeCountdown.Start(MetareeBurrowLifetimeNtsc);

        // Bits zero/one are still unnamed because their general engine semantics are not
        // proven. Metaree writes the literal mask `$0003`; retaining it raw avoids inventing
        // a flags-enum contract while preserving collision/processing behavior exactly.
        slot.Properties = unchecked((ushort)(slot.Properties | 0x0003));
        if (EnemyTerrain.EnemyHasSolidHighBitAhead(
                level,
                slot,
                SnesSixteenSixteen.FromWhole(state.YVelocity),
                movingDown: true))
        {
            slot.InstructionTimer = NativeCountdown.Start(1);
            slot.Timer = NativeCountdown.Start(0);
            state.Function = MetareeEnemyFunction.Burrowing;
            LastMetareeSoundEffect = 0x005c;
            return;
        }

        // Unlike common movement, Metaree applies whole-pixel words directly and leaves
        // both subpositions untouched. Horizontal steering re-evaluates Samus every frame.
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + state.YVelocity)));
        bool metareeIsLeftOfSamus =
            unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) < 0;
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole +
            (metareeIsLeftOfSamus
                ? MetareeHorizontalDiveSpeed
                : -MetareeHorizontalDiveSpeed))));
    }

    /// <summary>Moves down one pixel per burrow frame, emits debris at timer eight, then saves graphics indexes to the spawn record and deletes the actor.</summary>
    private void RunMetareeBurrow(RoomEnemySlot slot, MetareeEnemyState state)
    {
        state.BurrowTimer = state.BurrowTimer.Decrement().Remaining;
        if (state.BurrowTimer.IsZero)
        {
            // `$A3:8A92` writes the packed live palette/tile indexes back into the spawn
            // record before hiding/deleting the actor. Preserve that odd state mutation so
            // debugger-visible spawn data agrees even though current respawn logic is later.
            slot.Spawn = slot.Spawn with
            {
                VramTilesIndex = unchecked((ushort)(slot.PaletteIndex | slot.VramTilesIndex)),
            };
            slot.PaletteIndex = EnemyPaletteBits.Palette5;
            slot.VramTilesIndex = 0;
            slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
            return;
        }

        if (state.BurrowTimer.RawValue == MetareeParticleFrameNtsc)
            _projectiles.SpawnMetareeParticleBurst(slot);
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 1)));
    }

    /// <summary>Changes the instruction pointer only when the requested animation phase differs from the installed one.</summary>
    internal static void InstallRequestedMetareeInstruction(
        RoomEnemySlot slot,
        MetareeEnemyState state)
    {
        SkreeMetareeAnimationPhase requested = state.RequestedInstructionListIndex;
        if (requested == state.InstalledInstructionListIndex)
            return;

        state.InstalledInstructionListIndex = requested;
        slot.CurrentInstruction =
            SkreeMetareeAnimationDefinitions.MetareeInstructionList(requested);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Rejects main AI or private instruction dispatch before slot state has been initialized.</summary>
    private MetareeEnemyState RequireMetareeState(RoomEnemySlot slot) =>
        _metareeStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Metaree state.");

    /// <summary>Binds Metaree initialization only to the Metaree enemy-definition ID.</summary>
    internal void RegisterMetareeInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Metaree, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Metaree, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeMetaree(slot);
        });
    }

    /// <summary>Binds the phase dispatcher to Metaree's native main-AI routine.</summary>
    internal void RegisterMetareeMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Metaree, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Metaree, (slot, frame, nmiFrameCounter8) =>
        {
            RunMetareeMain(slot, RequireMetareeState(slot), frame.Samus, frame.Level);
        });
    }

    /// <summary>Claims the preparation-list ready instruction for Metaree slots and leaves other instructions to the shared interpreter.</summary>
    internal void RegisterMetareeOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessMetareeInstruction(slot, word, ref cursor);
        });
    }

    /// <summary>Last library-two Metaree dive ($5B) or floor-impact ($5C) sound this enemy pass, using the Max6 queue entry; cleared before the next pass.</summary>
    public ushort? LastMetareeSoundEffect { get; private set; }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastMetareeSoundEffect = null;
        Array.Clear(_metareeStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastMetareeSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastMetareeSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }


    /// <summary>Spawns four metal debris actors with the dying Metaree's graphics indexes before common shot death clears them.</summary>
    internal void RegisterMetareeShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Metaree &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.MetareeShot)
        {
            BeforeGenericDeath = c =>
            {
                _projectiles.SpawnMetareeParticleBurst(c.Enemy);
                c.Enemy.VramTilesIndex = 0;
                c.Enemy.PaletteIndex = 0;
            },
        });
    }

    /// <summary>After a lethal bomb, spawns debris while the body's graphics words are intact, then clears both indexes on the retained host record.</summary>
    internal void RegisterMetareeBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Metaree &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.MetareeShot)
        {
            AfterCommon = c =>
            {
                if (c.Enemy.Health != 0)
                    return;
                _projectiles.SpawnMetareeParticleBurst(c.Enemy);
                c.Enemy.VramTilesIndex = 0;
                c.Enemy.PaletteIndex = 0;
            },
        });
    }
}
