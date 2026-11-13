using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>The Powamp species owner: its per-room state, behaviour and the interactions it registers.</summary>
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
    /// <summary>The room's enemy places, where each body finds its balloon when it is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>The balloon each body found when it was placed, which its room lists right before it.</summary>
    private readonly EnemyPartLinks<RoomEnemySlot> _balloons = new();
    /// <summary>Applies shared shot and touch reactions before paired-state propagation.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Starts ordinary death processing after the delayed spike burst.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Connects Powamp's paired body and balloon AI to terrain, projectile, and collision services.</summary>
    internal PowampBehavior(EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemySlotTable slotTable, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops)
    {
        _terrain = terrain;
        _projectiles = projectiles;
        _slotTable = slotTable;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>Ungrappled body rise measured from its balloon's spawn Y position.</summary>
    private const int PowampUngrappledTravelDistance = 64;
    /// <summary>Frames the underground body rests before reinflating its balloon.</summary>
    private const int PowampRestFrames = 60;
    /// <summary>Inflation or deflation transition duration.</summary>
    private const int PowampTransitionFrames = 10;
    /// <summary>Delay between fatal damage and deleting the balloon with a spike burst.</summary>
    private const int PowampDeathDelayFrames = 32;
    /// <summary>Frames between successive shared body and balloon horizontal wiggle offsets.</summary>
    private const int PowampWiggleFramesPerOffset = 5;

    /// <summary>Initializes the Powamp.</summary>
    internal void InitializePowamp(RoomEnemySlot slot)
    {
        var state = new PowampEnemyState
        {
            IsBalloon = slot.SpawnSettings<PowampSpawnSettings>().Part == PowampPart.Balloon,
            RefusesContact = slot.SpawnSettings<PowampSpawnSettings>().GrappleTravelDistance != 0,
        };
        slot.SetSpeciesState(state);

        // The initializer forces animation processing for both halves and clears the
        // animation's loop counter before choosing the half-specific list.
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        slot.Frame = EnemyFrameId.Empty;
        slot.AnimationClock.Restart();

        if (!state.IsBalloon)
        {
            // The body's balloon is the enemy its room lists before it, whatever occupies that place.
            // Reject malformed populations instead of accidentally
            // binding a body to an unrelated actor.
            _balloons.Place(slot, _slotTable.ListedBefore(slot, 1));
            RequirePowampBalloon(slot);
            state.FunctionTimer = PowampRestFrames;
            state.Function = PowampEnemyFunction.DeflatedResting;
            SetPowampAnimation(slot, state, PowampSegment.BodySlow);
            return;
        }

        state.BalloonSpawn = slot.Position.Whole;
        state.Function = PowampEnemyFunction.BalloonNoOp;
        SetPowampAnimation(slot, state, PowampSegment.BalloonDeflated);
        state.BalloonGrappleTravelDistance = slot.SpawnSettings<PowampSpawnSettings>().GrappleTravelDistance;
    }

    /// <summary>Runs a Powamp's per-frame behaviour and every body function it dispatches.</summary>
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
            SetPowampAnimation(balloon.Slot, balloon.State, PowampSegment.BalloonInflate0);
            state.Function = PowampEnemyFunction.Inflating;
            state.FunctionTimer = PowampTransitionFrames;
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
            // Exactly half a pixel per frame upward on NTSC.
            state.YVelocity = SignedSixteenSixteen.FromSixteenthsOfPixel(-8);
            SetPowampAnimation(body, state, PowampSegment.BodyFast);
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
        // The ordinary rise changes dispatch and runs the grappled behaviour immediately
        // on the frame common grapple AI publishes the grappled state.
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

        // Comparing the target with the body by the sign of their difference means "continue while body is still
        // below the target". A room collision completes the rise regardless of its Y coordinate.
        bool reachedTarget = !CoordinateAxis.Precedes(balloon.State.BalloonSpawn.Y - travelDistance, body.Position.Y.Whole);
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
            // Finish-wiggle ignores whether the move hit terrain; terrain clips the displacement
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
        if (!CoordinateAxis.Precedes(body.Position.Y.Whole, balloon.State.BalloonSpawn.Y))
        {
            body.Position = body.Position.WithWholeY(balloon.State.BalloonSpawn.Y);
            state.Function = PowampEnemyFunction.DeflatedResting;
            state.FunctionTimer = PowampRestFrames;
            SetPowampAnimation(body, state, PowampSegment.BodySlow);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Selects a balloon inflation frame for fatal damage and primes the 32-frame death delay.</summary>
    private static void BeginPowampDeathSequence(
        RoomEnemySlot body,
        PowampEnemyState state,
        PowampBalloon balloon)
    {
        (bool sinking, int shownPoses) = PowampBalloonPhase(balloon.State.Playback);
        if (sinking && shownPoses != 1)
        {
            // A balloon that had sunk through two poses restarts inflate stage one, and one that had sunk through three
            // restarts stage zero; one that had shown a single sinking pose keeps sinking.
            PowampSegment replacement = PowampMotionDefinitions.SunkBalloonRestarts.TryGetValue(shownPoses, out PowampSegment restart)
                ? restart
                : throw new InvalidDataException($"Powamp death saw a sinking balloon with {shownPoses} poses shown.");
            SetPowampAnimation(balloon.Slot, balloon.State, replacement);
        }

        state.Function = PowampEnemyFunction.DeathSequence;
        state.FunctionTimer = PowampDeathDelayFrames;
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

        // The balloon is deleted, the spikes fire, and the body dies with the explosion the burst's last spike's place in
        // the projectile pool selects.
        balloon.Slot.Health = 0;
        balloon.Slot.Presence = balloon.Slot.Presence.With(EnemyPresence.Deleted);
        int? lastSpikePlace = SpawnPowampSpikeBurst(body, body.Shading);
        _deathsAndDrops.StartGenericEnemyDeath(body,
            PowampBurstExplosion.AfterBurst(lastSpikePlace, EnemyDeathExplosionDefinitions.AuthoredOrder));
    }

    /// <summary>Resolves the body-only private tail of a Powamp's touch.</summary>
    private void ResolvePowampTouch(RoomEnemySlot body, SamusState samus)
    {
        // The spikes take the body's colours from before the touch, which may change them.
        PictureShading lookBeforeTouch = body.Shading;
        _collision.ResolveNormalEnemyTouch(body, samus);
        if (body.Health != 0)
            return;

        PowampBalloon balloon = RequirePowampBalloon(body);
        balloon.Slot.Presence = balloon.Slot.Presence.With(EnemyPresence.Deleted);
        SpawnPowampSpikeBurst(body, lookBeforeTouch);
        body.Shading = EnemyShades.InProjectileLook;
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

    /// <summary>Runs the paired state propagation after a power bomb hits a Powamp.</summary>
    private void ResolvePowampPowerBombAfterCommon(RoomEnemySlot body)
    {
        PowampBalloon balloon = RequirePowampBalloon(body);
        if (body.Health == 0)
        {
            balloon.Slot.Presence = balloon.Slot.Presence.With(EnemyPresence.Deleted);
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
        if (--state.WiggleTimer > 0)
            return false;

        state.WiggleTimer = PowampWiggleFramesPerOffset;
        if (state.WiggleIndex >= PowampMotionDefinitions.WigglePhaseCount)
        {
            throw new InvalidDataException(
                $"Powamp wiggle index {state.WiggleIndex} exceeds the twelve authored wiggle offsets.");
        }

        int x = CoordinateAxis.Wrap(
            balloon.State.BalloonSpawn.X + PowampMotion.WiggleOffset(state.WiggleIndex));
        balloon.Slot.Position = balloon.Slot.Position.WithWholeX(x);
        body.Position = body.Position.WithWholeX(x);
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
        (bool sinking, int shownPoses) = PowampBalloonPhase(balloon.State.Playback);
        // The balloon sits at the height of the latest pose it has shown; before its first pose shows, it sits at the
        // first pose's height.
        int pose = shownPoses == 0 ? 0 : shownPoses - 1;
        balloon.Slot.Position = balloon.Slot.Position.WithWrappedWholeY(body.Position.Y.Whole + PowampMotion.BalloonOffset(pose, sinking));
    }

    /// <summary>
    /// Whether the balloon is in its sink sequence and how many of that sequence's poses it has shown. The AI reads the
    /// segment the balloon's animation stands in and how many of its poses it has passed; the deflated and second inflate
    /// segments continue those sequences, so their poses count after the sequence's earlier ones.
    /// </summary>
    internal static PowampBalloonState PowampBalloonPhase(AnimationPlayback<PowampSegment> playback)
    {
        AnimationPhase<PowampSegment> phase = playback.Animation.Phases[playback.Place.Phase];
        // A phase the play has not entered yet has shown none of its poses, and a held one all of them.
        int passed = phase.FirstBeat + Math.Clamp(playback.Place.Beat, 0, phase.BeatCount);
        return phase.Segment switch
        {
            PowampSegment.BalloonInflate0 => new PowampBalloonState(false, passed),
            PowampSegment.BalloonInflate1 => new PowampBalloonState(false, 1 + passed),
            PowampSegment.BalloonStartSinking => new PowampBalloonState(true, passed),
            PowampSegment.BalloonDeflated => new PowampBalloonState(true, 2 + passed),
            PowampSegment.BodyFast or
                PowampSegment.BodySlow => throw new InvalidOperationException($"Powamp balloon runs body animation {phase.Segment}."),
            _ => throw new InvalidOperationException($"Powamp balloon runs body animation {phase.Segment}."),
        };
    }

    /// <summary>Switches the balloon to its sinking art and begins the deflation timer.</summary>
    private static void StartPowampDeflating(PowampEnemyState state, PowampBalloon balloon)
    {
        state.Function = PowampEnemyFunction.Deflating;
        state.FunctionTimer = PowampTransitionFrames;
        SetPowampAnimation(balloon.Slot, balloon.State, PowampSegment.BalloonStartSinking);
    }

    /// <summary>Decrements the timer, treating zero and below as elapsed.</summary>
    private static bool TickPowampTimer(PowampEnemyState state)
    {
        return --state.FunctionTimer <= 0;
    }

    /// <summary>The body's vertical displacement for terrain movement.</summary>
    private static SignedSixteenSixteen ComposePowampVerticalDisplacement(PowampEnemyState state) => state.YVelocity;

    /// <summary>Recognizes the two zero-offset points in the twelve-phase wiggle table.</summary>
    private static bool IsPowampCentered(int wiggleIndex) => wiggleIndex is 0 or 6;

    /// <summary>Restarts a body or balloon animation on the next animation pass.</summary>
    private static void SetPowampAnimation(RoomEnemySlot slot, PowampEnemyState state, PowampSegment program)
    {
        state.Playback = EnemyAnimationPlayback.Start(slot, PowampAnimations.StartingWith(program));
    }

    /// <summary>Rejects a rise or sink dispatch without room collision data.</summary>
    private static RoomLevelData RequirePowampLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException("Powamp movement requires room level data.");

    /// <summary>Requires the balloon the body found when it was placed to be its initialized balloon half.</summary>
    private PowampBalloon RequirePowampBalloon(RoomEnemySlot body)
    {
        RoomEnemySlot balloon = _balloons.Of(body);
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
                                                       c.SelectedShotAi == EnemyReaction.PowampShot)
        {
            RunsGenericDeath = false,
            AfterCommon = c => ResolvePowampShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Connects radial spikes to their movement frame behaviour.</summary>
    internal static void AddPowampProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            RunPowampSpikeFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.PowampSpikeFlight);
        // A shot spike runs the same private delete program its definition names as its shot list.
        table.AddShotAnimation(RoomEnemyProjectileKind.PowampSpike, () => new PowampSpikeAnimation(PowampSpikeSegment.Delete));
    }

    /// <summary>Advances Powamp's typed animation.</summary>
    internal static void AdvancePowampTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, PowampAnimations.Segments,
            static (PowampEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"Powamp's cue-free animation raised {cue}."));

    /// <summary>Runs Powamp's main AI.</summary>
    internal void RunPowampMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunPowampMain(slot, slot.RequireSpeciesState<PowampEnemyState>(), frame.Level);
    }

    /// <summary>Propagates power-bomb reaction state from the body to its balloon.</summary>
    internal void AddPowampPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Powamp &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyReaction.PowampPowerBomb)
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
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.PowampShot)
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
        // interactive list because their intangibility property excludes them, but retain the handler's literal guard.
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Powamp &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.PowampTouch,
            context =>
            {
                if (!context.Slot.RequireSpeciesState<PowampEnemyState>().RefusesContact)
                    ResolvePowampTouch(context.Slot, context.Samus);
            });
    }
    /// <summary>Fires the Powamp's spikes in eight directions, drawn in <paramref name="look"/>.</summary>
    /// <param name="body">The Powamp firing them.</param>
    /// <param name="look">The colours the spikes are drawn in.</param>
    /// <returns>
    /// The projectile pool place of the last spike the burst fired, which selects a shot Powamp's explosion
    /// (<see cref="PowampBurstExplosion"/>), or null when the pool was full before all eight had fired.
    /// </returns>
    internal int? SpawnPowampSpikeBurst(RoomEnemySlot body, PictureShading look)
    {
        int? lastSpikePlace = null;
        // The spike loop counts from seven down to zero. Allocation independently searches the
        // eighteen-slot pool from the last slot downward on every iteration.
        for (int direction = 7; direction >= 0; direction--)
        {
            EnemyProjectile? spike = _projectiles.AllocateEnemyProjectile();
            if (spike is null)
                return null;

            // Spawning copies the spike's definition before
            // its initializer zeros velocities and captures the owner position. Reading
            // the record through the shared helper preserves its list, map sentinel, radii,
            // damage, and all property bits instead of maintaining a second hand copy here.
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                spike,
                RoomEnemyProjectileKind.PowampSpike,
                SpriteGraphics.SpawnedBy(body).In(look));
            spike.Position = FixedPosition<RoomPixels>.FromWhole(body.Position.Whole);
            spike.Velocity = new(SignedEightEight.Zero, SignedEightEight.Zero);
            spike.State = new PowampSpikeState(direction);
            lastSpikePlace = spike.Place;
        }
        return lastSpikePlace;
    }

    /// <summary>Runs the Powamp spike's per-frame behaviour.</summary>
    internal static void RunPowampSpikeFrameBehavior(
        EnemyProjectile spike,
        RoomLevelData level)
    {
        int direction = spike.StateOf<PowampSpikeState>().Direction;

        EightEightVector<RoomPixels> acceleration = PowampMotion.SpikeAcceleration(direction);
        spike.Velocity = spike.Velocity.WithX(spike.Velocity.X + acceleration.X);
        if (EnemyProjectileEngine.MoveProjectileAxis(spike, level, axis: Axis.Horizontal))
        {
            BeginPowampSpikeDeletion(spike);
            return;
        }

        spike.Velocity = spike.Velocity.WithY(spike.Velocity.Y + acceleration.Y);
        if (EnemyProjectileEngine.MoveProjectileAxis(spike, level, axis: Axis.Vertical))
            BeginPowampSpikeDeletion(spike);
    }

    /// <summary>Marks a spike for its deletion list after its movement phase ends.</summary>
    internal static void BeginPowampSpikeDeletion(EnemyProjectile spike)
    {
        // The program deletes the spike as its first command; starting it at timer one lets the
        // animation pass carry that out later in this same frame.
        spike.StartAnimation(new PowampSpikeAnimation(PowampSpikeSegment.Delete));
    }
}
