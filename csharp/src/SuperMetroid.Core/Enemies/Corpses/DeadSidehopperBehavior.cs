using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead sidehopper species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DeadSidehopperBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.CorpseSidehopper];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new DeadSidehopperEntity(this, slot, common);

    /// <summary>Provides installed artwork and the room random source for corpse decomposition.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Handles level collision after the scripted opening movement reaches the terrain boundary.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>The room's enemy places, whose first, the Shitroid, suppresses the victim when it starts hidden.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Supplies the enemy counter used to pace dust sound effects.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Spawns dust from each completed corpse row.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Applies normal touch damage before the corpse finishes transforming.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Raises each fallen row's dust and its sound request.</summary>
    private readonly CorpseRowDust _rowDust;

    /// <summary>Connects the corpse actor to room state, terrain, presentation, and interaction services.</summary>
    internal DeadSidehopperBehavior(DeadSidehopperBehaviorDependencies dependencies)
    {
        _rowDust = dependencies.RowDust;
        _room = dependencies.Room;
        _terrain = dependencies.Terrain;
        _slotTable = dependencies.SlotTable;
        _frameContext = dependencies.FrameContext;
        _projectiles = dependencies.Projectiles;
        _collision = dependencies.Collision;
    }

    /// <summary>Carries out the dead Sidehopper's animation cue.</summary>
    private static DeadTourianProgram? ApplyDeadSidehopperCue(RoomEnemySlot slot, DeadTourianCue cue)
    {
        switch (cue.Kind)
        {
            case DeadTourianCueKind.EndHop:
                // The end-hop cue ends the landing animation and hands ownership back to main AI.
                SelectDeadSidehopperPostAnimationState(slot);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DeadTourianCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Remembers level data for the private power-bomb callback, which has no level argument.</summary>
    private RoomLevelData? _deadSidehopperLevel;
    /// <summary>Remembers the camera position for the private power-bomb AI dispatch.</summary>
    private Position16<RoomPixels> _deadSidehopperCamera;

    /// <summary>The living victim activates once the camera's left edge is left of this room pixel column.</summary>
    private static readonly Position16<RoomPixels> VictimActivationCamera = new(513, 0);

    /// <summary>Initializes the dead Sidehopper.</summary>
    internal void InitializeDeadSidehopper(RoomEnemySlot slot)
    {
        DeadSidehopperLayout layout = slot.SpawnSettings<DeadSidehopperSpawnSettings>().Layout;
        switch (layout)
        {
            case DeadSidehopperLayout.ShitroidVictim:
                InitializeShitroidVictimSidehopper(slot);
                break;
            case DeadSidehopperLayout.AlreadyDead:
                InitializeAlternateDeadSidehopper(slot);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DeadSidehopperLayout)} {layout}.");
        }
    }

    /// <summary>Runs the Shitroid victim layout.</summary>
    private void InitializeShitroidVictimSidehopper(RoomEnemySlot slot)
    {
        // The mask clears the solid bit and any inherited process-offscreen bit,
        // then explicitly reinstalls only process-offscreen. Do not replace this with an
        // enum union: preserving the unrelated population-property bits is observable.
        slot.Contact = slot.Contact.Without(EnemyContact.SolidToSamus);
        slot.Processing = slot.Processing.With(EnemyProcessing.OffScreen);
        // The victim's room lists the Shitroid first; a Shitroid that starts hidden has already fed, so the victim is gone.
        if (_slotTable.ListedFirst().Presence.HasAny(EnemyPresence.Hidden))
            slot.Presence = slot.Presence.With(EnemyPresence.Deleted);

        slot.Position = slot.Position.WithWhole(new(488, 184));
        slot.Radius = slot.Radius.WithY(21);

        DeadSidehopperEnemyState state = InitializeDeadSidehopperCorpseState(
            slot,
            graphicsVariant: 0);
        SetDeadSidehopperProgram(slot, state, DeadTourianProgram.SidehopperIdle);
        state.Function = DeadSidehopperAiFunction.AliveWaitForCamera;
        // The victim shows the colours the Shitroid's entry gives it, the target the room's colours fade to (#1512).
        state.DrainStage = 0;
        slot.Shading = state.Shading;
        state.Velocity = new EightEightVector<RoomPixels>(SignedEightEight.FromParts(0, 96), SignedEightEight.FromParts(1, 0));
    }

    /// <summary>Runs the already-dead Tourian layout.</summary>
    private static void InitializeAlternateDeadSidehopper(RoomEnemySlot slot)
    {
        DeadSidehopperEnemyState state = InitializeDeadSidehopperCorpseState(
            slot,
            graphicsVariant: 2);
        SetDeadSidehopperProgram(slot, state, DeadTourianProgram.SidehopperInitiallyDead);
        state.DrainStage = null;
        slot.Shading = state.Shading;
        state.Function = DeadSidehopperAiFunction.WaitForSamusCollision;
    }

    /// <summary>Allocates slot state with the corpse's own rot schedule and its own rows.</summary>
    private static DeadSidehopperEnemyState InitializeDeadSidehopperCorpseState(
        RoomEnemySlot slot,
        int graphicsVariant)
    {
        var state = new DeadSidehopperEnemyState(slot, graphicsVariant, new CorpseRotSchedule(DeadSidehopperCorpseDefinitions.RowCount));
        slot.SetSpeciesState(state);
        return state;
    }

    /// <summary>Runs the dead Sidehopper's main AI.</summary>
    private void RunDeadSidehopperMain(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level,
        Position16<RoomPixels> camera)
    {
        DeadSidehopperEnemyState state = RequireDeadSidehopperState(slot);
        _deadSidehopperLevel = level;
        _deadSidehopperCamera = camera;
        DispatchDeadSidehopperState(slot, state, samus, level, camera);
    }

    /// <summary>Advances the authored hop, post-landing delay, drain, or collision-triggered decay state.</summary>
    internal void DispatchDeadSidehopperState(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        Position16<RoomPixels> camera)
    {
        switch (state.Function)
        {
            case DeadSidehopperAiFunction.AliveWaitForCamera:
                if ((camera - VictimActivationCamera).X < 0)
                {
                    state.Function = DeadSidehopperAiFunction.ActivatedMovement;
                    goto case DeadSidehopperAiFunction.ActivatedMovement;
                }
                return;

            case DeadSidehopperAiFunction.ActivatedMovement:
                if (MoveDeadSidehopper(slot, state, level))
                {
                    state.JumpPhase = (state.JumpPhase + 1) % DeadSidehopperLaunchDefinitions.PhaseCount;
                    SetDeadSidehopperProgram(slot, state, DeadTourianProgram.SidehopperHopping);
                    state.Function = DeadSidehopperAiFunction.NoOperation;
                }
                return;

            case DeadSidehopperAiFunction.NoOperation:
                return;

            case DeadSidehopperAiFunction.BeginPostLandingDelay:
                state.Function = DeadSidehopperAiFunction.PostLandingDelay;
                state.StateTimer = 64;
                return;

            case DeadSidehopperAiFunction.PostLandingDelay:
                if (--state.StateTimer >= 0)
                    return;

                if (state.HasBegunDraining)
                {
                    state.Function = DeadSidehopperAiFunction.Draining;
                    return;
                }

                state.Function = DeadSidehopperAiFunction.ActivatedMovement;
                SetDeadSidehopperProgram(slot, state, DeadTourianProgram.SidehopperIdle);
                state.Velocity = DeadSidehopperLaunchDefinitions.Velocity(state.JumpPhase);
                return;

            case DeadSidehopperAiFunction.Draining:
                StepDeadSidehopperDrain(slot, state);
                return;

            case DeadSidehopperAiFunction.WaitForSamusCollision:
                if (samus?.Kinematics.DidCollideWithSolidEnemy(slot.SlotIndex) == true)
                    state.Function = DeadSidehopperAiFunction.PreRotDelay;
                return;

            case DeadSidehopperAiFunction.PreRotDelay:
                state.CountPreRotDelayCall();
                if (state.PreRotDelayCounter >= 16)
                {
                    state.Function = DeadSidehopperAiFunction.Rotting;
                    slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
                }
                return;

            case DeadSidehopperAiFunction.Rotting:
                RunDeadSidehopperRotting(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Dead sidehopper function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>
    /// Moves a dead monster through its staged set. The scripted
    /// Shitroid-room half moves directly through the staged set until X reaches 544; the
    /// general room half then switches to the shared terrain collision primitives.
    /// </summary>
    private bool MoveDeadSidehopper(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state,
        RoomLevelData? level)
    {
        bool beforeTerrainBoundary = CoordinateAxis.Difference(slot.Position.X.Whole, 544) < 0;
        SignedSixteenSixteen horizontalDisplacement = state.Velocity.X.ToSixteenSixteenDelta();
        if (beforeTerrainBoundary)
        {
            MoveDeadSidehopperAxisDirect(
                slot,
                axis: Axis.Horizontal,
                state.Velocity.X);
        }
        else
        {
            _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                RequireDeadSidehopperLevel(level),
                slot,
                horizontalDisplacement);
        }

        // Upward motion slows by an eighth of a pixel per update each update; falling motion speeds up by half a
        // pixel. The sign test is performed before the addition in the game.
        SignedEightEight acceleration = state.Velocity.Y.IsNegative ? SignedEightEight.FromParts(0, 32) : SignedEightEight.FromParts(0, 128);
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + acceleration);
        if (beforeTerrainBoundary)
        {
            MoveDeadSidehopperAxisDirect(
                slot,
                axis: Axis.Vertical,
                state.Velocity.Y);
            return CoordinateAxis.Difference(slot.Position.Y.Whole, 184) >= 0;
        }

        SignedSixteenSixteen verticalDisplacement = state.Velocity.Y.ToSixteenSixteenDelta();
        return _terrain.MoveEnemyVertically(
            RequireDeadSidehopperLevel(level),
            slot,
            verticalDisplacement);
    }

    /// <summary>Applies signed 8.8 scripted motion to one 16.16 axis before terrain collision takes over.</summary>
    private static void MoveDeadSidehopperAxisDirect(
        RoomEnemySlot slot,
        Axis axis,
        SignedEightEight velocity)
    {
        FixedCoordinate current = axis == Axis.Horizontal ? slot.Position.X : slot.Position.Y;
        FixedCoordinate moved = current.MovedBy(velocity);
        if (axis == Axis.Horizontal)
        {
            slot.Position = slot.Position.WithX(moved);
        }
        else
        {
            slot.Position = slot.Position.WithY(moved);
        }
    }

    /// <summary>Rejects terrain-driven movement when no room level was supplied.</summary>
    private static RoomLevelData RequireDeadSidehopperLevel(RoomLevelData? level) =>
        level ?? throw new InvalidDataException(
            "Dead sidehopper crossed into terrain-driven movement without room level data.");

    /// <summary>Counts one draining call and, on every eighth, steps the drain, which only the victim shows (#1512).</summary>
    private static void StepDeadSidehopperDrain(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state)
    {
        state.DrainStepCalls++;
        if (state.DrainStepCalls < 8)
            return;

        state.DrainStepCalls = 0;

        int stage = state.DrainStage ??
            throw new InvalidOperationException("A dead Sidehopper placed already dead has no drain to step.");
        state.DrainStage = stage + 1;
        slot.Shading = state.Shading;
        if (!state.IsDrained)
            return;

        SetDeadSidehopperProgram(slot, state, DeadTourianProgram.SidehopperCorpse);
        state.Function = DeadSidehopperAiFunction.WaitForSamusCollision;
        slot.Contact = slot.Contact.With(EnemyContact.SolidToSamus);
        slot.Radius = slot.Radius.WithY(12);
    }

    /// <summary>
    /// The end-hop cue selects the post-landing state from whether Shitroid has begun draining the victim. It is
    /// deliberately instruction-driven: replacing it with a main-AI timer would make animation length and movement
    /// cadence diverge.
    /// </summary>
    private static void SelectDeadSidehopperPostAnimationState(RoomEnemySlot slot)
    {
        DeadSidehopperEnemyState state = RequireDeadSidehopperState(slot);
        state.Function = state.HasBegunDraining
            ? DeadSidehopperAiFunction.Draining
            : DeadSidehopperAiFunction.BeginPostLandingDelay;
    }

    /// <summary>Begins Shitroid's drain of its victim, when the Shitroid latches on to feed.</summary>
    internal static void ActivateDeadSidehopperVictim(RoomEnemySlot victim) =>
        RequireDeadSidehopperState(victim).DrainStage = 1;

    /// <summary>Resolves a shot hitting the dead Sidehopper.</summary>
    private static void ResolveDeadSidehopperShot(RoomEnemySlot slot)
    {
        DeadSidehopperEnemyState state = RequireDeadSidehopperState(slot);
        if (slot.Contact.HasAny(EnemyContact.IgnoresSamus) ||
            !state.IsDrained)
        {
            return;
        }
        TriggerDeadSidehopperRotting(slot, state);
    }

    /// <summary>Resolves Samus touching the dead Sidehopper.</summary>
    private void ResolveDeadSidehopperTouch(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state,
        SamusState samus)
    {
        if (!state.IsDrained)
        {
            _collision.ResolveNormalEnemyTouch(slot, samus);
            return;
        }
        TriggerDeadSidehopperRotting(slot, state);
    }

    /// <summary>Resolves a power bomb reaching the dead Sidehopper.</summary>
    private void ResolveDeadSidehopperPowerBomb(RoomEnemySlot slot, SamusState? samus)
    {
        DeadSidehopperEnemyState state = RequireDeadSidehopperState(slot);
        if (state.IsDrained)
        {
            ResolveDeadSidehopperShot(slot);
            return;
        }

        // Before the corpse is ready, the private power-bomb callback literally invokes
        // the actor's main dispatcher once. Use the last scheduled room context because the
        // outer power-bomb pass does not carry level/camera arguments.
        DispatchDeadSidehopperState(
            slot,
            state,
            samus,
            _deadSidehopperLevel,
            _deadSidehopperCamera);
    }

    /// <summary>Starts decay and keeps the corpse updating offscreen without further Samus collision.</summary>
    internal static void TriggerDeadSidehopperRotting(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state)
    {
        state.Function = DeadSidehopperAiFunction.Rotting;
        slot.Processing = slot.Processing.With(EnemyProcessing.OffScreen);
        slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Runs one rot call and its dust, moving the corpse's own rows as the call moves them (#1539).</summary>
    private void RunDeadSidehopperRotting(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state)
    {
        state.ProcessCallCount++;
        bool stillRotting = state.Rot.Step(
            state.Rows.MoveRow,
            _ => FinishDeadSidehopperCorpseRow(slot, state));
        if (!stillRotting)
            state.Function = DeadSidehopperAiFunction.WaitForSamusCollision;
    }

    /// <summary>The rows <paramref name="slot"/>'s frames show: its corpse's own, as its rot calls have left them (#1539).</summary>
    internal static CorpseRotRows ShownDeadSidehopperRows(RoomEnemySlot slot) => RequireDeadSidehopperState(slot).Rows.Shown;

    /// <summary>Records a completed row, spawns dust at its randomized X offset, and conditionally requests sound.</summary>
    private void FinishDeadSidehopperCorpseRow(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state)
    {
        state.FinishedEntryCount++;
        _rowDust.Spawn(slot, _room.Random.Read(), _projectiles, _frameContext.RandomEnemyCounter);
        state.DustSpawnCount++;
    }



    /// <summary>Starts a new dead Sidehopper animation, resetting both instruction clocks before its first step runs.</summary>
    internal static void SetDeadSidehopperProgram(RoomEnemySlot slot, DeadSidehopperEnemyState state, DeadTourianProgram program) =>
        state.Playback = StartDeadTourianProgram(slot, program);

    /// <summary>Resets <paramref name="slot"/>'s instruction clocks and returns the playback of <paramref name="program"/>'s animation from its start.</summary>
    internal static AnimationPlayback<DeadTourianProgram> StartDeadTourianProgram(RoomEnemySlot slot, DeadTourianProgram program)
    {
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        return new AnimationPlayback<DeadTourianProgram>(DeadTourianAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Rejects missing or mismatched private state before dispatching the slot's corpse AI.</summary>
    internal static DeadSidehopperEnemyState RequireDeadSidehopperState(RoomEnemySlot slot)
    {
        DeadSidehopperEnemyState state = slot.SpeciesState<DeadSidehopperEnemyState>() ??
            throw new InvalidDataException(
                $"Enemy slot {slot.SlotIndex} has no initialized dead-sidehopper state.");
        if (!ReferenceEquals(state.Slot, slot))
            throw new InvalidDataException("Dead-sidehopper state belongs to a different slot.");
        return state;
    }

    /// <summary>Routes bomb-special hits on the corpse shot callback to its decay check.</summary>
    internal static void AddDeadSidehopperBombReactions(EnemySharedReactions table)
    {
        table.AddBombSpecialAfterNoOp(c => c.Enemy.DefinitionId == EnemyDefinitionId.CorpseSidehopper &&
                                           c.SelectedShotAi == EnemyReaction.DeadSidehopperShot,
            c => ResolveDeadSidehopperShot(c.Enemy));
    }

    /// <summary>Runs the dead Sidehopper's main AI.</summary>
    internal void RunDeadSidehopperMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunDeadSidehopperMain(slot, frame.Samus, frame.Level, frame.Camera);
    }

    /// <summary>Advances the dead Sidehopper's typed animation.</summary>
    internal static void AdvanceDeadSidehopperTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        DeadSidehopperEnemyState state = RequireDeadSidehopperState(slot);
        AnimationPlayback<DeadTourianProgram> playback = state.Playback;
        EnemyProgramAnimation.Play(slot, DeadTourianAnimations.Segments, ref playback,
            cue => ApplyDeadSidehopperCue(slot, cue) is { } target
                ? new AnimationContinuation<DeadTourianProgram>(DeadTourianAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Uses the private power-bomb callback in place of common enemy damage handling.</summary>
    internal void AddDeadSidehopperPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.CorpseSidehopper &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyReaction.DeadSidehopperPowerBomb)
        {
            ReplacesCommon = ResolveDeadSidehopperPowerBomb,
        });
    }

    /// <summary>Marks projectile collision while withholding damage until the transformed corpse can decay.</summary>
    internal static void AddDeadSidehopperShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.CorpseSidehopper &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.DeadSidehopperShot)
        {
            // The common projectile prelude always marks the shot, but the private callback ignores it until the
            // victim is fully drained. Even then it starts rotting without vulnerability damage or impact.
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);
                ResolveDeadSidehopperShot(c.Enemy);
                return true;
            },
        });
    }

    /// <summary>Uses ordinary touch damage during transformation and starts decomposition afterward.</summary>
    internal void AddDeadSidehopperTouch(EnemySharedReactions table)
    {
        // Before Shitroid finishes feeding, the touch reaction uses normal contact damage without a death animation. At
        // the end of the drain it instead starts the corpse decomposition and never enters common touch damage.
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.CorpseSidehopper &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.DeadSidehopperTouch,
            context => ResolveDeadSidehopperTouch(context.Slot, RequireDeadSidehopperState(context.Slot), context.Samus));
    }
}
