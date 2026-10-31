using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Powamp.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>The Powamp species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class PowampBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Powamp];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new PowampEntity(this, slot, common);

    /// <summary>Clips body rise and sink motion against room collision.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Spawns the eight radial spikes emitted at death.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Resolves each body's preceding balloon slot.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies shared shot and touch reactions before paired-state propagation.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Starts ordinary death processing after the delayed spike burst.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>
    /// The room's enemy presentation, whose current placement holds the body's colours, whose sharing with the common
    /// colours a full pool's death animation reads.
    /// </summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Connects Powamp's paired body and balloon AI to terrain, projectile, and collision services.</summary>
    internal PowampBehavior(EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemySlotTable slotTable, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops,
        EnemyPresentation presentation)
    {
        _presentation = presentation;
        _terrain = terrain;
        _projectiles = projectiles;
        _slotTable = slotTable;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>Ungrappled body rise measured from its balloon's spawn Y position.</summary>
    private const ushort PowampUngrappledTravelDistance = 64;
    /// <summary>Frames the underground body rests before reinflating its balloon.</summary>
    private const ushort PowampRestFrames = 60;
    /// <summary>Inflation or deflation transition duration.</summary>
    private const ushort PowampTransitionFrames = 10;
    /// <summary>Delay between fatal damage and deleting the balloon with a spike burst.</summary>
    private const ushort PowampDeathDelayFrames = 32;
    /// <summary>Frames between successive shared body and balloon horizontal wiggle offsets.</summary>
    private const ushort PowampWiggleFramesPerOffset = 5;

    /// <summary>Ports <c>InitAI_Powamp</c> at $A8:C1C9.</summary>
    internal void InitializePowamp(RoomEnemySlot slot)
    {
        var state = new PowampEnemyState
        {
            IsBalloon = slot.SpawnSettings<PowampSpawnSettings>().Part == PowampPart.Balloon,
            RefusesContact = slot.SpawnSettings<PowampSpawnSettings>().GrappleTravelDistance != 0,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // The initializer forces animation processing for both halves and clears the
        // instruction interpreter's loop counter before choosing the half-specific list.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.Frame = EnemyFrameId.Empty;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        if (!state.IsBalloon)
        {
            // Enemy[-1] is not a descriptive relationship in the original—it is a literal
            // $40-byte negative index. Reject malformed populations instead of accidentally
            // binding a body to an unrelated actor.
            RequirePowampBalloon(slot);
            state.FunctionTimer = WrappingCountdown.Start(PowampRestFrames);
            state.Function = PowampEnemyFunction.DeflatedResting;
            SetPowampProgram(slot, state, PowampProgram.BodySlow);
            return;
        }

        state.BalloonSpawn = slot.Position.Whole;
        state.Function = PowampEnemyFunction.BalloonNoOp;
        SetPowampProgram(slot, state, PowampProgram.BalloonDeflated);
        state.BalloonGrappleTravelDistance = slot.SpawnSettings<PowampSpawnSettings>().GrappleTravelDistance;
    }

    /// <summary>Ports <c>MainAI_Powamp</c> and every body function it dispatches.</summary>
    internal void RunPowampMain(RoomEnemySlot slot, PowampEnemyState state, RoomLevelData? level)
    {
        if (state.IsBalloon)
            return;

        PowampBalloon balloon = RequirePowampBalloon(slot);
        switch (state.Function)
        {
            case PowampEnemyFunction.DeflatedResting:
                RunPowampDeflatedResting(slot, state, balloon);
                return;
            case PowampEnemyFunction.Inflating:
                RunPowampInflating(slot, state, balloon);
                return;
            case PowampEnemyFunction.InflatedRiseToTargetHeight:
                RunPowampRise(slot, state, balloon, RequirePowampLevel(level), hold: PowampHold.Free);
                return;
            case PowampEnemyFunction.InflatedFinishWiggle:
                RunPowampFinishWiggle(slot, state, balloon, RequirePowampLevel(level), hold: PowampHold.Free);
                return;
            case PowampEnemyFunction.GrappledRiseToTargetHeight:
                RunPowampRise(slot, state, balloon, RequirePowampLevel(level), hold: PowampHold.Grappled);
                return;
            case PowampEnemyFunction.GrappledFinishWiggle:
                RunPowampFinishWiggle(slot, state, balloon, RequirePowampLevel(level), hold: PowampHold.Grappled);
                return;
            case PowampEnemyFunction.GrappledResting:
                RunPowampGrappledResting(slot, state, balloon);
                return;
            case PowampEnemyFunction.Deflating:
                RunPowampDeflating(slot, state, balloon);
                return;
            case PowampEnemyFunction.DeflatedSinking:
                RunPowampSinking(slot, state, balloon, RequirePowampLevel(level));
                return;
            case PowampEnemyFunction.FatalDamage:
                BeginPowampDeathSequence(slot, state, balloon);
                return;
            case PowampEnemyFunction.DeathSequence:
                RunPowampDeathSequence(slot, state, balloon);
                return;
            case PowampEnemyFunction.BalloonNoOp:
            default:
                throw new InvalidDataException(
                    $"Powamp body function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Counts down the deflated rest and starts balloon inflation.</summary>
    private static void RunPowampDeflatedResting(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon)
    {
        if (TickPowampTimer(state))
        {
            SetPowampProgram(balloon.Slot, balloon.State, PowampProgram.BalloonInflate0);
            state.Function = PowampEnemyFunction.Inflating;
            state.FunctionTimer = WrappingCountdown.Start(PowampTransitionFrames);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Finishes inflation and seeds the half-pixel upward body velocity.</summary>
    private static void RunPowampInflating(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon)
    {
        if (TickPowampTimer(state))
        {
            state.Function = PowampEnemyFunction.InflatedRiseToTargetHeight;
            // Exactly half a pixel per frame upward on the NTSC cartridge.
            state.YVelocity = SignedSixteenSixteen.FromRaw(-SixteenSixteen.FromWhole(1) / 2);
            SetPowampProgram(body, state, PowampProgram.BodyFast);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Rises to the grappling-dependent height or terrain, preserving wiggle and grapple handoff order.</summary>
    private void RunPowampRise(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon,
        RoomLevelData level,
        PowampHold hold)
    {
        // The ordinary rise changes dispatch and executes the grappled routine immediately
        // on the frame common grapple AI publishes bit one.
        bool grappleAiActive = (body.AiStates & EnemyAiStates.Grappled) != EnemyAiStates.None;
        if (hold != PowampHold.Grappled && grappleAiActive)
        {
            state.Function = PowampEnemyFunction.GrappledRiseToTargetHeight;
            RunPowampRise(body, state, balloon, level, hold: PowampHold.Grappled);
            return;
        }

        // Releasing a grapple during either grappled rising state consumes the frame after
        // changing the function. In particular it does not realign the balloon on this path.
        if (hold == PowampHold.Grappled && !grappleAiActive)
        {
            state.Function = PowampEnemyFunction.InflatedFinishWiggle;
            return;
        }

        AdvancePowampWiggle(body, state, balloon, stop: PowampWiggleStop.Never);
        bool collided = _terrain.MoveEnemyVertically(level, body, ComposePowampVerticalDisplacement(state));
        int travelDistance = hold == PowampHold.Grappled
            ? balloon.State.BalloonGrappleTravelDistance
            : PowampUngrappledTravelDistance;
        int targetY = unchecked((ushort)(balloon.State.BalloonSpawn.Y - travelDistance));

        // CMP target,body followed by BMI means "continue while body is still below the
        // target". A room collision completes the rise regardless of its Y coordinate.
        bool reachedTarget = !EnemyMath.IsNegative16(targetY - body.Position.Y.Whole);
        if (collided || reachedTarget)
        {
            if (IsPowampCentered(state.WiggleIndex))
            {
                if (hold == PowampHold.Grappled)
                    state.Function = PowampEnemyFunction.GrappledResting;
                else
                    StartPowampDeflating(state, balloon);
            }
            else
            {
                state.Function = hold == PowampHold.Grappled
                    ? PowampEnemyFunction.GrappledFinishWiggle
                    : PowampEnemyFunction.InflatedFinishWiggle;
            }
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Continues clipped vertical movement until horizontal wiggle reaches a centered offset.</summary>
    private void RunPowampFinishWiggle(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon,
        RoomLevelData level,
        PowampHold hold)
    {
        bool grappleAiActive = (body.AiStates & EnemyAiStates.Grappled) != EnemyAiStates.None;
        if (hold == PowampHold.Grappled && !grappleAiActive)
        {
            state.Function = PowampEnemyFunction.InflatedFinishWiggle;
            return;
        }

        bool centered = AdvancePowampWiggle(body, state, balloon, stop: PowampWiggleStop.WhenCentered);
        if (centered)
        {
            if (hold == PowampHold.Grappled)
                state.Function = PowampEnemyFunction.GrappledResting;
            else
                StartPowampDeflating(state, balloon);
        }
        else
        {
            // Native finish-wiggle ignores collision carry; terrain clips the displacement
            // but does not select another function until the horizontal wiggle is centered.
            _terrain.MoveEnemyVertically(level, body, ComposePowampVerticalDisplacement(state));
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Holds the inflated pair while grapple AI is active, deflating on release.</summary>
    private static void RunPowampGrappledResting(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon)
    {
        if ((body.AiStates & EnemyAiStates.Grappled) == EnemyAiStates.None)
            StartPowampDeflating(state, balloon);
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Counts the balloon deflation interval before body sinking begins.</summary>
    private static void RunPowampDeflating(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon)
    {
        if (TickPowampTimer(state))
        {
            state.Function = PowampEnemyFunction.DeflatedSinking;
            // Exactly +1 pixel/frame.
            state.YVelocity = SignedSixteenSixteen.FromParts(1, 0);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Sinks the body to its spawn Y and restarts the slow deflated cycle.</summary>
    private void RunPowampSinking(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon,
        RoomLevelData level)
    {
        _terrain.MoveEnemyVertically(level, body, ComposePowampVerticalDisplacement(state));
        int spawnY = balloon.State.BalloonSpawn.Y;
        if (!EnemyMath.IsNegative16(body.Position.Y.Whole - spawnY))
        {
            body.Position = body.Position.WithWholeY(balloon.State.BalloonSpawn.Y);
            state.Function = PowampEnemyFunction.DeflatedResting;
            state.FunctionTimer = WrappingCountdown.Start(PowampRestFrames);
            SetPowampProgram(body, state, PowampProgram.BodySlow);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>The program a fatally hit sinking balloon restarts, indexed by the poses it had shown less two.</summary>
    private static readonly PowampProgram[] PowampDeathBalloonPrograms = [PowampProgram.BalloonInflate1, PowampProgram.BalloonInflate0];

    /// <summary>Selects a balloon inflation frame for fatal damage and primes the 32-frame death delay.</summary>
    private static void BeginPowampDeathSequence(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon)
    {
        (bool sinking, int shownPoses) = PowampBalloonPhase(balloon.State.Program);
        if (sinking && shownPoses != 1)
        {
            // The native pointer table is indexed by the byte offset left in Y, twice the poses shown less one. Two
            // shown poses select inflate stage one and three restart stage zero; one pose (entry zero) is skipped.
            PowampProgram replacement = shownPoses - 2 is int entry && (uint)entry < (uint)PowampDeathBalloonPrograms.Length
                ? PowampDeathBalloonPrograms[entry]
                : throw new InvalidDataException($"Powamp death saw a sinking balloon with {shownPoses} poses shown.");
            SetPowampProgram(balloon.Slot, balloon.State, replacement);
        }

        state.Function = PowampEnemyFunction.DeathSequence;
        state.FunctionTimer = WrappingCountdown.Start(PowampDeathDelayFrames);
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Deletes the balloon after the death delay, emits spikes, then runs generic body death.</summary>
    private void RunPowampDeathSequence(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon)
    {
        if (!TickPowampTimer(state))
        {
            AlignPowampBalloonY(body, balloon);
            return;
        }

        // `$A8:C5A6-$C5B5`: the balloon is deleted, the spikes fire, and the body goes
        // through EnemyDeath with whatever the spike loop left in A as its animation.
        balloon.Slot.Health = 0;
        balloon.Slot.Properties = balloon.Slot.Properties.With(EnemyProperties.Deleted);
        int accumulator = SpawnPowampSpikeBurst(body);
        _deathsAndDrops.StartGenericEnemyDeath(body, EnemyDeathExplosions.ForLeftoverNumber(accumulator));
    }

    /// <summary>Ports the body-only private tail of <c>EnemyTouch_Powamp</c>.</summary>
    private void ResolvePowampTouch(RoomEnemySlot body, SamusState samus)
    {
        SpriteColouring originalColouring = body.Colouring;
        _collision.ResolveNormalEnemyTouch(body, samus);
        if (body.Health != 0)
            return;

        PowampBalloon balloon = RequirePowampBalloon(body);
        balloon.Slot.Properties = balloon.Slot.Properties.With(EnemyProperties.Deleted);
        body.Colouring = originalColouring;
        SpawnPowampSpikeBurst(body);
        body.Colouring = SharedSpriteColouring.EnemyProjectiles;
    }

    /// <summary>Copies common shot/freeze presentation from the body to its balloon.</summary>
    private void ResolvePowampShotAfterCommon(RoomEnemySlot body)
    {
        PowampBalloon balloon = RequirePowampBalloon(body);
        if ((body.AiStates & EnemyAiStates.Frozen) != EnemyAiStates.None)
        {
            balloon.Slot.FrozenTimer = body.FrozenTimer;
            balloon.Slot.AiStates = balloon.Slot.AiStates | EnemyAiStates.Frozen;
        }
        if ((body.AiStates & EnemyAiStates.Hurt) != EnemyAiStates.None)
        {
            balloon.Slot.FlashTimer = body.FlashTimer;
            balloon.Slot.AiStates = balloon.Slot.AiStates | EnemyAiStates.Hurt;
        }
        if (body.Health == 0)
        {
            PowampEnemyState bodyState = body.RequireSpeciesState<PowampEnemyState>();
            bodyState.Function = PowampEnemyFunction.FatalDamage;
            bodyState.RefusesContact = true;
        }
    }

    /// <summary>Ports the paired state propagation after <c>PowerBombReaction_Powamp</c>.</summary>
    private void ResolvePowampPowerBombAfterCommon(RoomEnemySlot body)
    {
        PowampBalloon balloon = RequirePowampBalloon(body);
        if (body.Health == 0)
        {
            balloon.Slot.Properties = balloon.Slot.Properties.With(EnemyProperties.Deleted);
            return;
        }

        balloon.Slot.ShakeTimer = body.ShakeTimer;
        balloon.Slot.InvincibilityTimer = body.InvincibilityTimer;
        balloon.Slot.FlashTimer = body.FlashTimer;
        balloon.Slot.FrozenTimer = body.FrozenTimer;
        balloon.Slot.AiStates = body.AiStates;
    }

    /// <summary>Advances both halves to the same authored X offset every five frames.</summary>
    private static bool AdvancePowampWiggle(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon,
        PowampWiggleStop stop)
    {
        state.WiggleTimer = state.WiggleTimer.Decrement().Remaining;
        if (!state.WiggleTimer.IsZeroOrNegative)
            return false;

        state.WiggleTimer = WrappingCountdown.Start(PowampWiggleFramesPerOffset);
        if (state.WiggleIndex >= PowampMotionDefinitions.WigglePhaseCount)
        {
            throw new InvalidDataException(
                $"Powamp wiggle index {state.WiggleIndex} exceeds the 12-word ROM table.");
        }

        int x = unchecked((ushort)(
            balloon.State.BalloonSpawn.X + PowampMotion.WiggleOffset(state.WiggleIndex)));
        balloon.Slot.Position = balloon.Slot.Position.WithWholeX(checked((ushort)x));
        body.Position = body.Position.WithWholeX(checked((ushort)x));
        if (stop == PowampWiggleStop.WhenCentered && IsPowampCentered(state.WiggleIndex))
            return true;

        state.WiggleIndex++;
        if (state.WiggleIndex >= PowampMotionDefinitions.WigglePhaseCount)
            state.WiggleIndex = 0;
        return false;
    }

    /// <summary>Positions the balloon above the body using its current inflate or sink animation phase.</summary>
    internal static void AlignPowampBalloonY(RoomEnemySlot body, PowampBalloon balloon)
    {
        (bool sinking, int shownPoses) = PowampBalloonPhase(balloon.State.Program);
        // The 65C816 calculation halves the pointer's byte offset past the first pose, (2 * shown - 2), and indexes
        // words with it. Before any pose has shown the offset underflows past five and falls back to the first entry.
        int index = shownPoses == 0 ? 0 : shownPoses - 1;
        balloon.Slot.Position = balloon.Slot.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + PowampMotion.BalloonOffset(index, sinking))));
    }

    /// <summary>
    /// Whether the balloon is in its sink sequence and how many of that sequence's poses it has shown. The native
    /// instruction pointer the AI reads is the sequence's first list plus four bytes per shown pose; the deflated and
    /// second inflate lists continue those sequences.
    /// </summary>
    internal static PowampBalloonState PowampBalloonPhase(AnimationProgramCursor<PowampProgram> cursor) => cursor.Program switch
    {
        PowampProgram.BalloonInflate0 => new PowampBalloonState(false, cursor.Position),
        PowampProgram.BalloonInflate1 => new PowampBalloonState(false, 1 + cursor.Position),
        PowampProgram.BalloonStartSinking => new PowampBalloonState(true, cursor.Position),
        PowampProgram.BalloonDeflated => new PowampBalloonState(true, 2 + cursor.Position),
        PowampProgram.BodyFast or
            PowampProgram.BodySlow => throw new InvalidOperationException($"Powamp balloon runs body animation {cursor.Program}."),
        _ => throw new InvalidOperationException($"Powamp balloon runs body animation {cursor.Program}."),
    };

    /// <summary>Switches the balloon to its sinking art and begins the deflation timer.</summary>
    private static void StartPowampDeflating(PowampEnemyState state, PowampBalloon balloon)
    {
        state.Function = PowampEnemyFunction.Deflating;
        state.FunctionTimer = WrappingCountdown.Start(PowampTransitionFrames);
        SetPowampProgram(balloon.Slot, balloon.State, PowampProgram.BalloonStartSinking);
    }

    /// <summary>Decrements the native timer, treating zero and signed underflow as elapsed.</summary>
    private static bool TickPowampTimer(PowampEnemyState state)
    {
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        return state.FunctionTimer.IsZeroOrNegative;
    }

    /// <summary>The body's vertical displacement for terrain movement.</summary>
    private static int ComposePowampVerticalDisplacement(PowampEnemyState state) => state.YVelocity.RawValue;

    /// <summary>Recognizes the two zero-offset points in the twelve-phase wiggle table.</summary>
    private static bool IsPowampCentered(int wiggleIndex) => wiggleIndex is 0 or 6;

    /// <summary>Restarts a body or balloon animation on the next interpreter pass.</summary>
    private static void SetPowampProgram(RoomEnemySlot slot, PowampEnemyState state, PowampProgram program)
    {
        state.Program = AnimationProgramCursor<PowampProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Rejects a rise or sink dispatch without room collision data.</summary>
    private static RoomLevelData RequirePowampLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException("Powamp movement requires room level data.");

    /// <summary>Requires the literal preceding enemy slot to be this body's initialized balloon half.</summary>
    private PowampBalloon RequirePowampBalloon(RoomEnemySlot body)
    {
        if (body.SlotIndex == 0)
            throw new InvalidDataException("A Powamp body has no preceding balloon slot.");
        RoomEnemySlot balloon = _slotTable.Slots[body.SlotIndex - 1];
        if (balloon.DefinitionId != EnemyDefinitionId.Powamp || balloon.SpawnSettings<PowampSpawnSettings>().Part != PowampPart.Balloon)
        {
            throw new InvalidDataException(
                $"Powamp body slot {body.SlotIndex} is not preceded by its balloon half.");
        }
        return new(balloon, balloon.RequireSpeciesState<PowampEnemyState>());
    }

    /// <summary>Routes bomb hits through common shot logic and mirrors damage state to the balloon.</summary>
    internal void AddPowampBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Powamp &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.PowampShot)
        {
            RunsGenericDeath = false,
            AfterCommon = c => ResolvePowampShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Connects radial spikes to their bank-$86 movement pre-instruction.</summary>
    internal static void AddPowampProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            RunPowampSpikePreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.PowampSpike);
        // A shot spike runs the same private delete program its definition names as its shot list.
        table.AddShotAnimation(RoomEnemyProjectileKind.PowampSpike, () => new PowampSpikeAnimation(PowampSpikeProgram.Delete));
    }

    /// <summary>Advances Powamp's typed animation.</summary>
    internal static void AdvancePowampTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, PowampAnimations.Programs, PowampFrameDefinitions.ShownFrames.Of,
            static (PowampEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor);

    /// <summary>Ports Powamp's main AI.</summary>
    internal void RunPowampMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunPowampMain(slot, slot.RequireSpeciesState<PowampEnemyState>(), frame.Level);
    }

    /// <summary>Propagates power-bomb reaction state from the body to its balloon.</summary>
    internal void AddPowampPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Powamp &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.PowampPowerBomb)
        {
            AfterCommon = (enemy, _) =>
            {
                if (enemy.SpawnSettings<PowampSpawnSettings>().Part != PowampPart.Balloon)
                    ResolvePowampPowerBombAfterCommon(enemy);
            },
        });
    }

    /// <summary>Skips later shots once fatal damage begins and mirrors surviving body presentation to the balloon.</summary>
    internal void AddPowampShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Powamp &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.PowampShot)
        {
            // Powamp refuses contact as soon as a lethal shot starts its private 32-frame death. Later
            // overlaps return before common shot AI, so they must not consume or explode the projectile while the
            // body is still visibly deflating.
            SkipsEnemy = enemy => enemy.RequireSpeciesState<PowampEnemyState>().RefusesContact,
            ExemptFromGenericDeath = true,
            AfterCommon = c => ResolvePowampShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Applies touch behavior only to the body before its fatal-shot state begins.</summary>
    internal void AddPowampTouch(EnemySharedReactions table)
    {
        // Only the body before fatal shot damage accepts contact. Balloons are normally absent from the
        // interactive list because population property $0400 excludes them, but retain the handler's literal guard.
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Powamp &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.PowampTouch,
            context =>
            {
                if (!context.Slot.RequireSpeciesState<PowampEnemyState>().RefusesContact)
                    ResolvePowampTouch(context.Slot, context.Samus);
            });
    }
    /// <summary>Ports <c>FirePowampSpikesIn8Directions</c> at $A8:C223.</summary>
    /// <returns>
    /// The death animation <c>FirePowampSpikesIn8Directions</c> leaves selected for EnemyDeath: the last spike's
    /// native projectile index, or, when the pool was full, <see cref="FullPoolDeathAnimation"/>.
    /// </returns>
    internal int SpawnPowampSpikeBurst(RoomEnemySlot body)
    {
        int graphics = FullPoolDeathAnimation(body);
        int accumulator = 0;
        // Native Y counts from seven down to zero. Allocation independently searches the
        // eighteen-slot bank-$86 pool from native index $22 downward on every iteration.
        for (int direction = 7; direction >= 0; direction--)
        {
            RoomEnemyProjectileSlot? spike = _projectiles.AllocateEnemyProjectile();
            if (spike is null)
                return graphics;

            // SpawnEnemyProjectileY_ParameterA_XGraphics copies definition $D298 before
            // initializer $D23A zeros velocities and captures the owner position. Reading
            // the record through the shared helper preserves its list, map sentinel, radii,
            // damage, and all property bits instead of maintaining a second hand copy here.
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                spike,
                RoomEnemyProjectileKind.PowampSpike,
                body.Graphics);
            spike.Position = FixedPosition<RoomPixels>.FromWhole(body.Position.Whole);
            spike.Velocity = new(new SignedEightEight(0), new SignedEightEight(0));
            spike.State = new PowampSpikeState(direction);
            accumulator = unchecked((ushort)(spike.SlotIndex * 2));
        }
        return accumulator;
    }

    /// <summary>
    /// The death animation a Powamp dies with when its spike burst found the projectile pool full. The cartridge leaves
    /// the body's graphics, read as one number, as the animation, and EnemyDeath treats any number past its animations
    /// as the default animation 0. That number names an animation only for a body whose colours are the common colours,
    /// held where the palette bits read as zero, so it is the body's tile base; any other colours put it past every animation.
    /// </summary>
    private int FullPoolDeathAnimation(RoomEnemySlot body) =>
        _presentation.Placement.SharesColours(body.Colouring, SharedSpriteColouring.Common) ? body.TileIndexBase : 0;

    /// <summary>Ports <c>PreInstruction_EnemyProjectile_PowampSpike</c> at $86:D263.</summary>
    internal static void RunPowampSpikePreInstruction(
        RoomEnemyProjectileSlot spike,
        RoomLevelData level)
    {
        int direction = spike.StateOf<PowampSpikeState>().Direction;

        spike.Velocity = spike.Velocity.WithX(new SignedEightEight(unchecked((ushort)(
            unchecked((short)spike.Velocity.X.RawValue) + PowampMotion.SpikeXAcceleration(direction)))));
        if (EnemyProjectileEngine.MoveProjectileAxis(spike, level, axis: Axis.Horizontal))
        {
            BeginPowampSpikeDeletion(spike);
            return;
        }

        spike.Velocity = spike.Velocity.WithY(new SignedEightEight(unchecked((ushort)(
            unchecked((short)spike.Velocity.Y.RawValue) + PowampMotion.SpikeYAcceleration(direction)))));
        if (EnemyProjectileEngine.MoveProjectileAxis(spike, level, axis: Axis.Vertical))
            BeginPowampSpikeDeletion(spike);
    }

    /// <summary>Marks a spike for its deletion list after its movement phase ends.</summary>
    internal static void BeginPowampSpikeDeletion(RoomEnemyProjectileSlot spike)
    {
        // The program deletes the spike as its first command; starting it at timer one lets the
        // instruction pass carry that out later in this same frame.
        spike.StartAnimation(new PowampSpikeAnimation(PowampSpikeProgram.Delete));
    }
}
