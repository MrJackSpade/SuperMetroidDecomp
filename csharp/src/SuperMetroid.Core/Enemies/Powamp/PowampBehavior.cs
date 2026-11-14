using SuperMetroid.Core.Animation;
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

    /// <summary>Initializes the Powamp's balloon or body, each with its own state (#1845).</summary>
    internal void InitializePowamp(RoomEnemySlot slot)
    {
        // The initializer forces animation processing for both halves and clears the
        // animation's loop counter before choosing the half-specific animation.
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        slot.Frame = EnemyFrameId.Empty;
        slot.AnimationClock.Restart();

        PowampSpawnSettings settings = slot.SpawnSettings<PowampSpawnSettings>();
        if (settings.Part == PowampPart.Balloon)
        {
            var balloon = new PowampBalloonState
            {
                RefusesContact = settings.GrappleTravelDistance != 0,
                Spawn = slot.Position.Whole,
                GrappleTravelDistance = settings.GrappleTravelDistance,
            };
            slot.SetSpeciesState(balloon);
            StartPowampBalloonAnimation(slot, balloon, PowampSegment.BalloonDeflated);
            return;
        }

        var state = new PowampBodyState { RefusesContact = settings.GrappleTravelDistance != 0 };
        slot.SetSpeciesState(state);
        // The body's balloon is the enemy its room lists before it, whatever occupies that place.
        // Reject malformed populations instead of accidentally
        // binding a body to an unrelated actor.
        _balloons.Place(slot, _slotTable.ListedBefore(slot, 1));
        RequirePowampBalloon(slot);
        state.ActionTimer = PowampRestFrames;
        state.Action = PowampAction.DeflatedResting;
        state.Playback = StartPowampAnimation(slot, PowampSegment.BodySlow);
    }

    /// <summary>Runs every action of a Powamp's body; the balloon has none, its body moves it.</summary>
    internal void RunPowampMain(RoomEnemySlot slot, PowampBodyState state, RoomLevelData? level)
    {
        PowampBalloon balloon = RequirePowampBalloon(slot);
        switch (state.Action)
        {
            case PowampAction.DeflatedResting:
                RunPowampDeflatedResting(slot, state, balloon);
                return;
            case PowampAction.Inflating:
                RunPowampInflating(slot, state, balloon);
                return;
            case PowampAction.InflatedRiseToTargetHeight:
                RunPowampRise(slot, state, balloon, RequirePowampLevel(level), hold: PowampHold.Free);
                return;
            case PowampAction.InflatedFinishWiggle:
                RunPowampFinishWiggle(slot, state, balloon, RequirePowampLevel(level), hold: PowampHold.Free);
                return;
            case PowampAction.GrappledRiseToTargetHeight:
                RunPowampRise(slot, state, balloon, RequirePowampLevel(level), hold: PowampHold.Grappled);
                return;
            case PowampAction.GrappledFinishWiggle:
                RunPowampFinishWiggle(slot, state, balloon, RequirePowampLevel(level), hold: PowampHold.Grappled);
                return;
            case PowampAction.GrappledResting:
                RunPowampGrappledResting(slot, state, balloon);
                return;
            case PowampAction.Deflating:
                RunPowampDeflating(slot, state, balloon);
                return;
            case PowampAction.DeflatedSinking:
                RunPowampSinking(slot, state, balloon, RequirePowampLevel(level));
                return;
            case PowampAction.FatalDamage:
                BeginPowampDeathSequence(slot, state, balloon);
                return;
            case PowampAction.DeathSequence:
                RunPowampDeathSequence(slot, state, balloon);
                return;
            default:
                throw new InvalidDataException(
                    $"Powamp body has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Counts down the deflated rest and starts balloon inflation.</summary>
    private static void RunPowampDeflatedResting(
        RoomEnemySlot body,
        PowampBodyState state,
        PowampBalloon balloon)
    {
        if (TickPowampTimer(state))
        {
            StartPowampBalloonAnimation(balloon.Slot, balloon.State, PowampSegment.BalloonInflatingFromDeflated);
            state.Action = PowampAction.Inflating;
            state.ActionTimer = PowampTransitionFrames;
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Finishes inflation and seeds the half-pixel upward body velocity.</summary>
    private static void RunPowampInflating(
        RoomEnemySlot body,
        PowampBodyState state,
        PowampBalloon balloon)
    {
        if (TickPowampTimer(state))
        {
            state.Action = PowampAction.InflatedRiseToTargetHeight;
            // Exactly half a pixel per frame upward.
            state.YVelocity = SignedSixteenSixteen.FromSixteenthsOfPixel(-8);
            state.Playback = StartPowampAnimation(body, PowampSegment.BodyFast);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Rises to the grappling-dependent height or terrain, preserving wiggle and grapple handoff order.</summary>
    private void RunPowampRise(
        RoomEnemySlot body,
        PowampBodyState state,
        PowampBalloon balloon,
        RoomLevelData level,
        PowampHold hold)
    {
        // The ordinary rise changes dispatch and runs the grappled behaviour immediately
        // on the frame common grapple AI publishes the grappled state.
        bool grappleAiActive = (body.AiStates & EnemyAiStates.Grappled) != EnemyAiStates.None;
        if (hold != PowampHold.Grappled && grappleAiActive)
        {
            state.Action = PowampAction.GrappledRiseToTargetHeight;
            RunPowampRise(body, state, balloon, level, hold: PowampHold.Grappled);
            return;
        }

        // Releasing a grapple during either grappled rising state consumes the frame after
        // changing the action. In particular it does not realign the balloon on this path.
        if (hold == PowampHold.Grappled && !grappleAiActive)
        {
            state.Action = PowampAction.InflatedFinishWiggle;
            return;
        }

        AdvancePowampWiggle(body, state, balloon, stop: PowampWiggleStop.Never);
        bool collided = _terrain.MoveEnemyVertically(level, body, ComposePowampVerticalDisplacement(state));
        int travelDistance = hold == PowampHold.Grappled
            ? balloon.State.GrappleTravelDistance
            : PowampUngrappledTravelDistance;

        // Comparing the target with the body by whether their difference is negative means "continue while body is still
        // below the target". A room collision completes the rise regardless of its Y coordinate.
        bool reachedTarget = !CoordinateAxis.Precedes(balloon.State.Spawn.Y - travelDistance, body.Position.Y.Whole);
        if (collided || reachedTarget)
        {
            if (IsPowampCentered(state.WiggleIndex))
            {
                if (hold == PowampHold.Grappled)
                    state.Action = PowampAction.GrappledResting;
                else
                    StartPowampDeflating(state, balloon);
            }
            else
            {
                state.Action = hold == PowampHold.Grappled
                    ? PowampAction.GrappledFinishWiggle
                    : PowampAction.InflatedFinishWiggle;
            }
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Continues clipped vertical movement until horizontal wiggle reaches a centered offset.</summary>
    private void RunPowampFinishWiggle(
        RoomEnemySlot body,
        PowampBodyState state,
        PowampBalloon balloon,
        RoomLevelData level,
        PowampHold hold)
    {
        bool grappleAiActive = (body.AiStates & EnemyAiStates.Grappled) != EnemyAiStates.None;
        if (hold == PowampHold.Grappled && !grappleAiActive)
        {
            state.Action = PowampAction.InflatedFinishWiggle;
            return;
        }

        bool centered = AdvancePowampWiggle(body, state, balloon, stop: PowampWiggleStop.WhenCentered);
        if (centered)
        {
            if (hold == PowampHold.Grappled)
                state.Action = PowampAction.GrappledResting;
            else
                StartPowampDeflating(state, balloon);
        }
        else
        {
            // Finish-wiggle ignores whether the move hit terrain; terrain clips the displacement
            // but does not select another action until the horizontal wiggle is centered.
            _terrain.MoveEnemyVertically(level, body, ComposePowampVerticalDisplacement(state));
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Holds the inflated pair while grapple AI is active, deflating on release.</summary>
    private static void RunPowampGrappledResting(
        RoomEnemySlot body,
        PowampBodyState state,
        PowampBalloon balloon)
    {
        if ((body.AiStates & EnemyAiStates.Grappled) == EnemyAiStates.None)
            StartPowampDeflating(state, balloon);
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Counts the balloon deflation interval before body sinking begins.</summary>
    private static void RunPowampDeflating(
        RoomEnemySlot body,
        PowampBodyState state,
        PowampBalloon balloon)
    {
        if (TickPowampTimer(state))
        {
            state.Action = PowampAction.DeflatedSinking;
            // Exactly +1 pixel/frame.
            state.YVelocity = SignedSixteenSixteen.FromParts(1, 0);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Sinks the body to its spawn Y and restarts the slow deflated cycle.</summary>
    private void RunPowampSinking(
        RoomEnemySlot body,
        PowampBodyState state,
        PowampBalloon balloon,
        RoomLevelData level)
    {
        _terrain.MoveEnemyVertically(level, body, ComposePowampVerticalDisplacement(state));
        if (!CoordinateAxis.Precedes(body.Position.Y.Whole, balloon.State.Spawn.Y))
        {
            body.Position = body.Position.WithWholeY(balloon.State.Spawn.Y);
            state.Action = PowampAction.DeflatedResting;
            state.ActionTimer = PowampRestFrames;
            state.Playback = StartPowampAnimation(body, PowampSegment.BodySlow);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Selects a balloon inflation frame for fatal damage and primes the 32-frame death delay.</summary>
    private static void BeginPowampDeathSequence(
        RoomEnemySlot body,
        PowampBodyState state,
        PowampBalloon balloon)
    {
        PowampBalloonInflation inflation = balloon.State.Inflation;
        if (inflation.Sequence == PowampBalloonSequence.Sinking)
        {
            // A balloon sunk halfway restarts inflating from halfway, and one sunk deflated restarts inflating from
            // deflated; one still full keeps sinking. A sinking balloon that has not shown its first pose is refused, as
            // the game would read past its restart table.
            if (inflation.PosesShown == 0)
                throw new InvalidDataException("Powamp death saw a sinking balloon that had shown none of its poses.");
            if (PowampMotionDefinitions.SunkBalloonRestarts.TryGetValue(inflation.Pose, out PowampSegment restart))
                StartPowampBalloonAnimation(balloon.Slot, balloon.State, restart);
        }

        state.Action = PowampAction.DeathSequence;
        state.ActionTimer = PowampDeathDelayFrames;
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Deletes the balloon after the death delay, emits spikes, then runs generic body death.</summary>
    private void RunPowampDeathSequence(
        RoomEnemySlot body,
        PowampBodyState state,
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
            PowampBodyState bodyState = body.RequireSpeciesState<PowampBodyState>();
            bodyState.Action = PowampAction.FatalDamage;
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
        PowampBodyState state,
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
            balloon.State.Spawn.X + PowampMotion.WiggleOffset(state.WiggleIndex));
        balloon.Slot.Position = balloon.Slot.Position.WithWholeX(x);
        body.Position = body.Position.WithWholeX(x);
        if (stop == PowampWiggleStop.WhenCentered && IsPowampCentered(state.WiggleIndex))
            return true;

        state.WiggleIndex++;
        if (state.WiggleIndex >= PowampMotionDefinitions.WigglePhaseCount)
            state.WiggleIndex = 0;
        return false;
    }

    /// <summary>
    /// Positions the balloon above the body at its pose's height: the latest pose it has shown, or before its first pose
    /// shows, its sequence's first pose.
    /// </summary>
    internal static void AlignPowampBalloonY(RoomEnemySlot body, PowampBalloon balloon) =>
        balloon.Slot.Position = balloon.Slot.Position.WithWrappedWholeY(
            body.Position.Y.Whole + PowampMotion.BalloonHeight(balloon.State.Inflation.Pose));

    /// <summary>Switches the balloon to its sinking art and begins the deflation timer.</summary>
    private static void StartPowampDeflating(PowampBodyState state, PowampBalloon balloon)
    {
        state.Action = PowampAction.Deflating;
        state.ActionTimer = PowampTransitionFrames;
        StartPowampBalloonAnimation(balloon.Slot, balloon.State, PowampSegment.BalloonStartSinking);
    }

    /// <summary>Decrements the timer, treating zero and below as elapsed.</summary>
    private static bool TickPowampTimer(PowampBodyState state)
    {
        return --state.ActionTimer <= 0;
    }

    /// <summary>The body's vertical displacement for terrain movement.</summary>
    private static SignedSixteenSixteen ComposePowampVerticalDisplacement(PowampBodyState state) => state.YVelocity;

    /// <summary>Whether the wiggle stands at its centre, where its offset is zero.</summary>
    private static bool IsPowampCentered(int wiggleIndex) => PowampMotion.WiggleOffset(wiggleIndex) == 0;

    /// <summary>The playback of <paramref name="segment"/>'s animation, restarted on the next animation pass.</summary>
    private static AnimationPlayback<PowampSegment> StartPowampAnimation(RoomEnemySlot slot, PowampSegment segment) =>
        EnemyAnimationPlayback.Start(slot, PowampAnimations.StartingWith(segment));

    /// <summary>
    /// Restarts the balloon's animation at <paramref name="segment"/> on the next animation pass, and its sequence where
    /// that animation starts it (#1845).
    /// </summary>
    internal static void StartPowampBalloonAnimation(RoomEnemySlot balloon, PowampBalloonState state, PowampSegment segment)
    {
        state.Playback = StartPowampAnimation(balloon, segment);
        state.Inflation = PowampMotionDefinitions.BalloonAnimationStarts.TryGetValue(segment, out PowampBalloonInflation start)
            ? start
            : throw new InvalidOperationException($"Powamp balloon started body animation {segment}.");
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
        return new(balloon, balloon.RequireSpeciesState<PowampBalloonState>());
    }

    /// <summary>Routes bomb hits through common shot logic and mirrors damage state to the balloon.</summary>
    internal EnemyBombReaction PowampBombReaction => field ??= new()
    {
        RunsGenericDeath = false,
        // Powamp's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = c => ResolvePowampShotAfterCommon(c.Enemy),
    };

    /// <summary>
    /// A Powamp spike (#1760): it flies outward, accelerating, with room collision. A shot spike runs the same private
    /// delete animation its definition names as its shot animation.
    /// </summary>
    internal static readonly EnemyProjectileBehavior Spike = new SingleMotionProjectileBehavior(
        EnemyProjectileFrameBehavior.PowampSpikeFlight,
        context => RunPowampSpikeFrameBehavior(context.Projectile, context.Level),
        shot: () => new PowampSpikeAnimation(PowampSpikeSegment.Delete));

    /// <summary>Advances the body's pulse, or the balloon's inflate or sink animation and the poses it shows.</summary>
    internal static void AdvancePowampTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        if (slot.SpawnSettings<PowampSpawnSettings>().Part != PowampPart.Balloon)
        {
            EnemyAnimationPlayback.Advance(slot, PowampAnimations.Segments,
                static (PowampBodyState state) => state.Playback, static (state, playback) => state.Playback = playback,
                static (_, cue) => RaisesNoCues(cue));
            return;
        }

        if (!slot.AnimationClock.Expire())
            return;
        PowampBalloonState balloon = slot.RequireSpeciesState<PowampBalloonState>();
        AnimationPlayback<PowampSegment> playback = balloon.Playback;
        // Every frame of the balloon's animations is one of its poses, so each frame shown records the next pose.
        EnemyAnimationPlayback.Play(slot, PowampAnimations.Segments, ref playback, pose =>
        {
            balloon.Inflation = balloon.Inflation.AfterShowingPose();
            return pose;
        }, static cue => RaisesNoCues(cue));
        balloon.Playback = playback;
    }

    /// <summary>Rejects a cue: no Powamp animation raises one.</summary>
    private static AnimationContinuation<PowampSegment>? RaisesNoCues(NoAnimationCue cue) =>
        throw new InvalidOperationException($"Powamp's cue-free animation raised {cue}.");

    /// <summary>Runs the body's main AI; the balloon has none, its body moves it.</summary>
    internal void RunPowampMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        if (slot.SpawnSettings<PowampSpawnSettings>().Part == PowampPart.Balloon)
            return;
        RunPowampMain(slot, slot.RequireSpeciesState<PowampBodyState>(), frame.Level);
    }

    /// <summary>Whether the Powamp half in <paramref name="slot"/> refuses touch and shot AI.</summary>
    private static bool RefusesContact(RoomEnemySlot slot) =>
        slot.SpawnSettings<PowampSpawnSettings>().Part == PowampPart.Balloon
            ? slot.RequireSpeciesState<PowampBalloonState>().RefusesContact
            : slot.RequireSpeciesState<PowampBodyState>().RefusesContact;

    /// <summary>Propagates power-bomb reaction state from the body to its balloon.</summary>
    internal EnemyPowerBombReaction PowampPowerBombReaction => field ??= new()
    {
        AfterCommon = c =>
        {
            if (c.Enemy.SpawnSettings<PowampSpawnSettings>().Part != PowampPart.Balloon)
                ResolvePowampPowerBombAfterCommon(c.Enemy);
        },
    };

    /// <summary>Skips later shots once fatal damage begins and mirrors surviving body presentation to the balloon.</summary>
    internal EnemyShotReaction PowampShotReaction => field ??= new()
    {
        // Powamp refuses contact as soon as a lethal shot starts its private 32-frame death. Later
        // overlaps return before common shot AI, so they must not consume or explode the projectile while the
        // body is still visibly deflating.
        SkipsEnemy = RefusesContact,
        ExemptFromGenericDeath = true,
        // Powamp's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        AfterCommon = c => ResolvePowampShotAfterCommon(c.Enemy),
    };

    /// <summary>Applies touch behavior only to the body before its fatal-shot state begins.</summary>
    internal void ReactToPowampTouch(EnemyTouchContext context)
    {
        // Only the body before fatal shot damage accepts contact. Balloons are normally absent from the
        // interactive list because their intangibility property excludes them, but retain the handler's literal guard.
        if (!RefusesContact(context.Slot))
            ResolvePowampTouch(context.Slot, context.Samus);
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
        // The spikes fire in their authored order. Allocation independently searches the
        // eighteen-slot pool from the last slot downward on every iteration.
        foreach (CompassDirection direction in PowampMotionDefinitions.SpikeFiringOrder)
        {
            EnemyProjectile? spike = _projectiles.AllocateEnemyProjectile();
            if (spike is null)
                return null;

            // Spawning copies the spike's definition before
            // its initializer zeros velocities and captures the owner position. Reading
            // the definition through the shared helper keeps its animation, frame, radii, damage and
            // properties instead of a second hand copy here.
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                spike,
                RoomEnemyProjectileKind.PowampSpike,
                Spike,
                SpriteGraphics.SpawnedBy(body).In(look));
            spike.StartAnimation(new PowampSpikeAnimation(PowampSpikeSegment.Initial));
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
        CompassDirection direction = spike.StateOf<PowampSpikeState>().Direction;

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

    /// <summary>Marks a spike for its deletion animation after its movement phase ends.</summary>
    internal static void BeginPowampSpikeDeletion(EnemyProjectile spike)
    {
        // The animation deletes the spike as its first step; starting it at timer one lets the
        // animation pass carry that out later in this same frame.
        spike.StartAnimation(new PowampSpikeAnimation(PowampSpikeSegment.Delete));
    }
}
