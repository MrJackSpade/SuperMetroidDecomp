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

    /// <summary>
    /// A Powamp's body or balloon, as the placement's settings name the part it is (#2163). The balloon shares the body's
    /// definition; only its placement tells them apart.
    /// </summary>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        occupant.SpawnSettings<PowampSpawnSettings>() is PowampBalloonSpawnSettings
            ? EnemyOccupant.WithoutMainAi(new PowampBalloonEntity(this, occupant, place, common))
            : EnemyOccupant.Of(new PowampBodyEntity(this, occupant, place, common));

    /// <summary>Clips body rise and sink motion against room collision.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Spawns the eight radial spikes emitted at death.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The room's enemy places, where each body finds its balloon when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The balloon each body found when it was placed, which its room lists right before it.</summary>
    private readonly EnemyPartLinks<RoomEnemyPlace> _balloons = new();
    /// <summary>Applies shared shot and touch reactions before paired-state propagation.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Starts ordinary death processing after the delayed spike burst.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Connects Powamp's paired body and balloon AI to terrain, projectile, and collision services.</summary>
    internal PowampBehavior(EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyPlaces places, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops)
    {
        _terrain = terrain;
        _projectiles = projectiles;
        _places = places;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>
    /// Readies either half of a Powamp to animate: the setup forces animation processing for both halves and clears
    /// the animation's clock before the half starts its own animation.
    /// </summary>
    private static void PrepareToAnimate(RoomEnemyPlace place)
    {
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Frame = EnemyFrameId.PointAtOrigin;
        place.AnimationClock.Restart();
    }

    /// <summary>Initializes a Powamp's balloon with its own state (#1845), deflated at its spawn.</summary>
    internal static void InitializePowampBalloon(RoomEnemyPlace place)
    {
        PrepareToAnimate(place);
        var balloon = new PowampBalloonState
        {
            Spawn = place.Position.Whole,
        };
        place.SetSpeciesState(balloon);
        StartPowampBalloonAnimation(place, balloon, PowampSegment.BalloonDeflated);
    }

    /// <summary>Initializes a Powamp's body with its own state (#1845), resting, joined to the balloon its room lists before it.</summary>
    internal void InitializePowampBody(RoomEnemyPlace place)
    {
        PrepareToAnimate(place);
        var state = new PowampBodyState(PowampAction.DeflatedResting);
        place.SetSpeciesState(state);
        // The body's balloon is placed before it and checked once as the body is placed.
        _balloons.Place(place, _places.Member(place, PowampPartListingDefinitions.Listing, PowampMember.Body, PowampMember.Balloon));
        CheckPowampBalloonPlaced(place);
        state.ActionTimer = UpdateCountdown.For(PowampMotionDefinitions.RestDuration);
        state.Playback = StartPowampAnimation(place, PowampSegment.BodySlow);
    }

    /// <summary>Runs every action of a Powamp's body; the balloon has none, its body moves it.</summary>
    internal void RunPowampMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, PowampBodyState state, RoomLevelData level, SamusState samus)
    {
        PowampBalloon balloon = BalloonOf(place);
        switch (state.Action)
        {
            case PowampAction.DeflatedResting:
                RunPowampDeflatedResting(place, state, balloon);
                return;
            case PowampAction.Inflating:
                RunPowampInflating(place, state, balloon);
                return;
            case PowampAction.InflatedRiseToTargetHeight:
                RunPowampRise(place, state, balloon, level, hold: PowampHold.Free);
                return;
            case PowampAction.InflatedFinishWiggle:
                RunPowampFinishWiggle(place, state, balloon, level, hold: PowampHold.Free);
                return;
            case PowampAction.GrappledRiseToTargetHeight:
                RunPowampRise(place, state, balloon, level, hold: PowampHold.Grappled);
                return;
            case PowampAction.GrappledFinishWiggle:
                RunPowampFinishWiggle(place, state, balloon, level, hold: PowampHold.Grappled);
                return;
            case PowampAction.GrappledResting:
                RunPowampGrappledResting(place, state, balloon);
                return;
            case PowampAction.Deflating:
                RunPowampDeflating(place, state, balloon);
                return;
            case PowampAction.DeflatedSinking:
                RunPowampSinking(place, state, balloon, level);
                return;
            case PowampAction.FatalDamage:
                BeginPowampDeathSequence(place, state, balloon);
                return;
            case PowampAction.DeathSequence:
                RunPowampDeathSequence(occupant, place, state, balloon, samus);
                return;
            default:
                throw new InvalidDataException(
                    $"Powamp body has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Counts down the deflated rest and starts balloon inflation.</summary>
    private static void RunPowampDeflatedResting(
        RoomEnemyPlace body,
        PowampBodyState state,
        PowampBalloon balloon)
    {
        if (TickPowampTimer(state))
        {
            StartPowampBalloonAnimation(balloon.Place, balloon.State, PowampSegment.BalloonInflatingFromDeflated);
            state.Action = PowampAction.Inflating;
            state.ActionTimer = UpdateCountdown.For(PowampMotionDefinitions.InflationDuration);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Finishes inflation and seeds the upward body velocity (<see cref="PowampMotionDefinitions.RiseVelocity"/>).</summary>
    private static void RunPowampInflating(
        RoomEnemyPlace body,
        PowampBodyState state,
        PowampBalloon balloon)
    {
        if (TickPowampTimer(state))
        {
            state.Action = PowampAction.InflatedRiseToTargetHeight;
            state.YVelocity = PowampMotionDefinitions.RiseVelocity;
            state.Playback = StartPowampAnimation(body, PowampSegment.BodyFast);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Rises to the grappling-dependent height or terrain, preserving wiggle and grapple handoff order.</summary>
    private void RunPowampRise(
        RoomEnemyPlace body,
        PowampBodyState state,
        PowampBalloon balloon,
        RoomLevelData level,
        PowampHold hold)
    {
        // The ordinary rise changes behaviour and runs the grappled behaviour immediately
        // on the frame the grapple beam takes hold.
        bool grappleAiActive = body.IsGrappled;
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
        bool collided = _terrain.MoveEnemyVertically(level, body, state.YVelocity);
        int travelDistance = hold == PowampHold.Grappled
            ? balloon.Occupant.SpawnSettings<PowampBalloonSpawnSettings>().GrappleTravelDistance
            : PowampMotionDefinitions.UngrappledRise;

        // The rise continues while the body is still below the target. A room collision completes the rise regardless
        // of its Y coordinate.
        bool reachedTarget = balloon.State.Spawn.Y - travelDistance >= body.Position.Y.Whole;
        if (collided || reachedTarget)
        {
            if (state.Wiggle.IsCentered)
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
        RoomEnemyPlace body,
        PowampBodyState state,
        PowampBalloon balloon,
        RoomLevelData level,
        PowampHold hold)
    {
        bool grappleAiActive = body.IsGrappled;
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
            _terrain.MoveEnemyVertically(level, body, state.YVelocity);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Holds the inflated pair while the grapple beam holds it, deflating on release.</summary>
    private static void RunPowampGrappledResting(
        RoomEnemyPlace body,
        PowampBodyState state,
        PowampBalloon balloon)
    {
        if (!body.IsGrappled)
            StartPowampDeflating(state, balloon);
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Counts the balloon deflation interval before body sinking begins.</summary>
    private static void RunPowampDeflating(
        RoomEnemyPlace body,
        PowampBodyState state,
        PowampBalloon balloon)
    {
        if (TickPowampTimer(state))
        {
            state.Action = PowampAction.DeflatedSinking;
            state.YVelocity = PowampMotionDefinitions.SinkVelocity;
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Sinks the body to its spawn Y and restarts the slow deflated cycle.</summary>
    private void RunPowampSinking(
        RoomEnemyPlace body,
        PowampBodyState state,
        PowampBalloon balloon,
        RoomLevelData level)
    {
        _terrain.MoveEnemyVertically(level, body, state.YVelocity);
        if (body.Position.Y.Whole >= balloon.State.Spawn.Y)
        {
            body.Position = body.Position.WithWholeY(balloon.State.Spawn.Y);
            state.Action = PowampAction.DeflatedResting;
            state.ActionTimer = UpdateCountdown.For(PowampMotionDefinitions.RestDuration);
            state.Playback = StartPowampAnimation(body, PowampSegment.BodySlow);
        }
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Selects a balloon inflation frame for fatal damage and primes the death delay (<see cref="PowampMotionDefinitions.DeathDelay"/>).</summary>
    private static void BeginPowampDeathSequence(
        RoomEnemyPlace body,
        PowampBodyState state,
        PowampBalloon balloon)
    {
        PowampBalloonInflation inflation = balloon.State.Inflation;
        if (inflation.Sequence == PowampBalloonSequence.Sinking)
        {
            // A balloon sunk halfway restarts inflating from halfway, and one sunk deflated restarts inflating from
            // deflated; one still full keeps sinking. A sinking balloon that has not shown its first pose is refused:
            // it has no pose to restart from.
            if (inflation.PosesShown == 0)
                throw new InvalidDataException("Powamp death saw a sinking balloon that had shown none of its poses.");
            if (PowampMotionDefinitions.SunkBalloonRestarts.TryGetValue(inflation.Pose, out PowampSegment restart))
                StartPowampBalloonAnimation(balloon.Place, balloon.State, restart);
        }

        state.Action = PowampAction.DeathSequence;
        state.ActionTimer = UpdateCountdown.For(PowampMotionDefinitions.DeathDelay);
        AlignPowampBalloonY(body, balloon);
    }

    /// <summary>Deletes the balloon after the death delay, emits spikes, then runs generic body death.</summary>
    private void RunPowampDeathSequence(
        RoomEnemyOccupant occupant, RoomEnemyPlace body,
        PowampBodyState state,
        PowampBalloon balloon,
        SamusState samus)
    {
        if (!TickPowampTimer(state))
        {
            AlignPowampBalloonY(body, balloon);
            return;
        }

        // The balloon is deleted, the spikes fire, and the body dies with the explosion the burst's last spike's place in
        // the projectile pool selects.
        balloon.Place.Health = 0;
        balloon.Place.Presence = balloon.Place.Presence.With(EnemyPresence.Deleted);
        SpawnPowampSpikeBurst(occupant, body, body.Shading);
        _deathsAndDrops.StartGenericEnemyDeath(occupant, ExplosionAfterBurst(), samus);
    }

    /// <summary>
    /// The explosion a Powamp shot dead dies in after its burst: the one the place its last spike took names, or the small
    /// one when its spikes found the pool full (<see cref="LeftoverNumberDeathExplosion{TExplosion}.LeftByLatestSpawn"/>).
    /// </summary>
    private EnemyDeathExplosion ExplosionAfterBurst() =>
        EnemyDeathExplosionDefinitions.LeftoverNumbered.LeftByLatestSpawn(_projectiles.Places);

    /// <summary>Finishes a touch on a Powamp's body: when it kills the Powamp, its balloon goes and its spikes burst out.</summary>
    private void ResolvePowampTouch(RoomEnemyOccupant occupant, RoomEnemyPlace body, SamusState samus, PseudoScrewCharge pseudoScrew)
    {
        // The spikes take the body's colours from before the touch, which may change them.
        PictureShading lookBeforeTouch = body.Shading;
        _collision.ResolveOrdinaryTouch(occupant, samus, pseudoScrew);
        if (body.Health != 0)
            return;

        PowampBalloon balloon = BalloonOf(body);
        balloon.Place.Presence = balloon.Place.Presence.With(EnemyPresence.Deleted);
        SpawnPowampSpikeBurst(occupant, body, lookBeforeTouch);
        body.Shading = EnemyShades.InProjectileLook;
    }

    /// <summary>Copies common shot/freeze presentation from the body to its balloon.</summary>
    private void ResolvePowampShotAfterCommon(RoomEnemyPlace body)
    {
        PowampBalloon balloon = BalloonOf(body);
        if (body.IsFrozen)
        {
            balloon.Place.Freeze.Restart(body.Freeze.UpdatesLeft);
            balloon.Place.MarkFrozen();
        }
        if (body.IsHurt)
        {
            balloon.Place.HurtFlash.Restart(body.HurtFlash.UpdatesLeft);
            balloon.Place.MarkHurt();
        }
        if (body.Health == 0)
        {
            // The fatal action refuses Samus's later touch and shots (PowampBodyState.IsDying).
            body.RequireSpeciesState<PowampBodyState>().Action = PowampAction.FatalDamage;
        }
    }

    /// <summary>Runs the paired state propagation after a power bomb hits a Powamp.</summary>
    private void ResolvePowampPowerBombAfterCommon(RoomEnemyPlace body)
    {
        PowampBalloon balloon = BalloonOf(body);
        if (body.Health == 0)
        {
            balloon.Place.Presence = balloon.Place.Presence.With(EnemyPresence.Deleted);
            return;
        }

        balloon.Place.Shake.Restart(body.Shake.UpdatesLeft);
        balloon.Place.Invincibility.Restart(body.Invincibility.UpdatesLeft);
        balloon.Place.HurtFlash.Restart(body.HurtFlash.UpdatesLeft);
        balloon.Place.Freeze.Restart(body.Freeze.UpdatesLeft);
        balloon.Place.AiStates = body.AiStates;
    }

    /// <summary>Advances both halves to the same authored X offset each wiggle step (<see cref="PowampMotionDefinitions.WiggleStepDuration"/>).</summary>
    private static bool AdvancePowampWiggle(
        RoomEnemyPlace body,
        PowampBodyState state,
        PowampBalloon balloon,
        PowampWiggleStop stop)
    {
        if (!state.WiggleTimer.StepEnds())
            return false;

        state.WiggleTimer = UpdateCountdown.For(PowampMotionDefinitions.WiggleStepDuration);
        int x = balloon.State.Spawn.X + state.Wiggle.Offset;
        balloon.Place.Position = balloon.Place.Position.WithWholeX(x);
        body.Position = body.Position.WithWholeX(x);
        if (stop == PowampWiggleStop.WhenCentered && state.Wiggle.IsCentered)
            return true;

        state.Wiggle = state.Wiggle.Next;
        return false;
    }

    /// <summary>
    /// Positions the balloon above the body at its pose's height: the latest pose it has shown, or before its first pose
    /// shows, its sequence's first pose.
    /// </summary>
    private static void AlignPowampBalloonY(RoomEnemyPlace body, PowampBalloon balloon) =>
        balloon.Place.Position = balloon.Place.Position.WithWholeY(
            body.Position.Y.Whole + PowampMotion.BalloonHeight(balloon.State.Inflation.Pose));

    /// <summary>Switches the balloon to its sinking art and begins the deflation timer.</summary>
    private static void StartPowampDeflating(PowampBodyState state, PowampBalloon balloon)
    {
        state.Action = PowampAction.Deflating;
        state.ActionTimer = UpdateCountdown.For(PowampMotionDefinitions.InflationDuration);
        StartPowampBalloonAnimation(balloon.Place, balloon.State, PowampSegment.BalloonStartSinking);
    }

    /// <summary>Counts one update of the body's action off its timer; true on the update it runs out.</summary>
    private static bool TickPowampTimer(PowampBodyState state)
    {
        return state.ActionTimer.StepEnds();
    }

    /// <summary>The playback of <paramref name="segment"/>'s animation, restarted on the next animation pass.</summary>
    private static AnimationPlayback<PowampSegment> StartPowampAnimation(RoomEnemyPlace place, PowampSegment segment) =>
        EnemyAnimationPlayback.Start(place, PowampAnimations.StartingWith(segment));

    /// <summary>
    /// Restarts the balloon's animation at <paramref name="segment"/> on the next animation pass, and its sequence where
    /// that animation starts it (#1845).
    /// </summary>
    private static void StartPowampBalloonAnimation(RoomEnemyPlace balloon, PowampBalloonState state, PowampSegment segment)
    {
        state.Playback = StartPowampAnimation(balloon, segment);
        state.Inflation = PowampMotionDefinitions.BalloonAnimationStarts.TryGetValue(segment, out PowampBalloonInflation start)
            ? start
            : throw new InvalidOperationException($"Powamp balloon started body animation {segment}.");
    }

    /// <summary>Checks, as the body is placed, that the enemy listed before it, placed before it, is its balloon half.</summary>
    /// <exception cref="InvalidDataException">The body is not listed right after a Powamp balloon.</exception>
    private void CheckPowampBalloonPlaced(RoomEnemyPlace body)
    {
        if (_balloons.Of(body).Entity is not PowampBalloonEntity)
        {
            throw new InvalidDataException(
                $"Powamp body place {body.Ordinal} is not preceded by its balloon half.");
        }
    }

    /// <summary>The balloon the body found, and checked, when it was placed.</summary>
    /// <exception cref="InvalidOperationException">The balloon has left its place.</exception>
    private PowampBalloon BalloonOf(RoomEnemyPlace body)
    {
        RoomEnemyPlace balloon = _balloons.Of(body);
        RoomEnemyOccupant occupant = balloon.Occupant ?? throw new InvalidOperationException(
            $"The Powamp body in place {body.Ordinal} reads its balloon in place {balloon.Ordinal}, which no enemy holds.");
        return new(balloon, balloon.RequireSpeciesState<PowampBalloonState>(), occupant);
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
    private StatefulSingleMotionProjectileBehavior<PowampSpikeState> Spike => field ??= new(
        context => FlyPowampSpike(context.Projectile, context.Level),
        shot: () => new PowampSpikeAnimation(PowampSpikeSegment.Delete));

    /// <summary>Advances the body's pulse.</summary>
    internal static void AdvancePowampBodyTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, PowampAnimations.Segments,
            static (PowampBodyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => RaisesNoCues(cue));

    /// <summary>Advances the balloon's inflate or sink animation and the poses it shows.</summary>
    internal static void AdvancePowampBalloonTypedAnimation(RoomEnemyPlace place)
    {
        if (!place.AnimationClock.Expire())
            return;
        PowampBalloonState balloon = place.RequireSpeciesState<PowampBalloonState>();
        AnimationPlayback<PowampSegment> playback = balloon.Playback;
        // Every frame of the balloon's animations is one of its poses, so each frame shown records the next pose.
        EnemyAnimationPlayback.Play(place, PowampAnimations.Segments, ref playback, pose =>
        {
            balloon.Inflation = balloon.Inflation.AfterShowingPose();
            return pose;
        }, static cue => RaisesNoCues(cue));
        balloon.Playback = playback;
    }

    /// <summary>Rejects a cue: no Powamp animation raises one.</summary>
    private static AnimationContinuation<PowampSegment>? RaisesNoCues(NoAnimationCue cue) =>
        throw new InvalidOperationException($"Powamp's cue-free animation raised {cue}.");

    /// <summary>Propagates power-bomb reaction state from the body to its balloon.</summary>
    internal EnemyPowerBombReaction PowampBodyPowerBombReaction => field ??= new()
    {
        AfterCommon = c => ResolvePowampPowerBombAfterCommon(c.Enemy),
    };

    /// <summary>The balloon's Power Bomb reaction: the blast's damage alone, its body carrying a hit on to it.</summary>
    internal EnemyPowerBombReaction PowampBalloonPowerBombReaction => field ??= new();

    /// <summary>Skips later shots once fatal damage begins and mirrors surviving body presentation to the balloon.</summary>
    /// <param name="body">The body the reaction is built for, whose placement names whether it refuses shots.</param>
    internal EnemyShotReaction PowampBodyShotReaction(RoomEnemyOccupant body) =>
        ShotReactionSkipping(place => BodyRefusesContact(body, place));

    /// <summary>The Powamp's shot reaction as the balloon takes it, skipped while the balloon refuses contact.</summary>
    /// <param name="balloon">The balloon the reaction is built for, whose placement names whether it refuses shots.</param>
    internal EnemyShotReaction PowampBalloonShotReaction(RoomEnemyOccupant balloon) =>
        ShotReactionSkipping(_ => PlacementRefusesContact(balloon));

    /// <summary>Whether a half's placement refuses Samus's touch and shots (#2474), as a grappled balloon's does.</summary>
    private static bool PlacementRefusesContact(RoomEnemyOccupant half) =>
        half.SpawnSettings<PowampSpawnSettings>().Contact == PowampContact.Refuses;

    /// <summary>Whether the body refuses Samus's touch and shots: its placement refuses them, or it is dying (#2474).</summary>
    private static bool BodyRefusesContact(RoomEnemyOccupant occupant, RoomEnemyPlace body) =>
        PlacementRefusesContact(occupant) || body.RequireSpeciesState<PowampBodyState>().IsDying;

    /// <summary>The Powamp's shot reaction for a half that refuses shots while <paramref name="refusesContact"/> says so.</summary>
    private EnemyShotReaction ShotReactionSkipping(Func<RoomEnemyPlace, bool> refusesContact) => new()
    {
        // A lethal shot starts the Powamp's death delay, during which later shots pass through it, neither spent
        // nor exploding, while the body visibly deflates; it dies in its own way, not in the usual burst.
        SkipsEnemy = refusesContact,
        Lethal = EnemyShotLethal.SpeciesOwned,
        // A shot damages a Powamp without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        AfterCommon = c => ResolvePowampShotAfterCommon(c.Enemy),
    };

    /// <summary>Applies touch behavior to the body before its fatal-shot state begins.</summary>
    internal void ReactToPowampBodyTouch(RoomEnemyOccupant occupant, EnemyTouchContext context)
    {
        if (!BodyRefusesContact(occupant, context.Place))
            ResolvePowampTouch(occupant, context.Place, context.Samus, context.PseudoScrew);
    }

    /// <summary>Applies the Powamp's touch to the balloon while it accepts contact.</summary>
    internal void ReactToPowampBalloonTouch(RoomEnemyOccupant occupant, EnemyTouchContext context)
    {
        // A balloon that refuses contact ignores Samus's touch.
        if (!PlacementRefusesContact(occupant))
            ResolvePowampTouch(occupant, context.Place, context.Samus, context.PseudoScrew);
    }
    /// <summary>
    /// Fires the Powamp's spikes in eight directions, drawn in <paramref name="look"/>, until the pool is full; the latest
    /// spawn's place selects a shot Powamp's explosion (<see cref="ExplosionAfterBurst"/>).
    /// </summary>
    /// <param name="occupant">The enemy holding <paramref name="body"/>.</param>
    /// <param name="body">The Powamp firing them.</param>
    /// <param name="look">The colours the spikes are drawn in.</param>
    private void SpawnPowampSpikeBurst(RoomEnemyOccupant occupant, RoomEnemyPlace body, PictureShading look)
    {
        // The spikes fire in their authored order, each taking the last free enemy-projectile place afresh.
        foreach (CompassDirection direction in PowampMotionDefinitions.SpikeFiringOrder)
        {
            EnemyProjectile? spike = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.PowampSpike, Spike, SpriteGraphics.SpawnedBy(occupant, body.Shading).In(look));
            if (spike is null)
                return;

            // The spike starts as its definition has it, with its animation, size and damage, still, at the Powamp.
            spike.StartAnimation(new PowampSpikeAnimation(PowampSpikeSegment.Flying));
            spike.Position = FixedPosition<RoomPixels>.FromWhole(body.Position.Whole);
            spike.Velocity = new(SignedEightEight.Zero, SignedEightEight.Zero);
            Spike.States.Start(spike, new PowampSpikeState(direction));
        }
    }

    /// <summary>Flies the Powamp spike for one update.</summary>
    internal void FlyPowampSpike(
        EnemyProjectile spike,
        RoomLevelData level)
    {
        CompassDirection direction = Spike.States.Of(spike).Direction;

        PixelVelocity<RoomPixels> acceleration = PowampMotion.SpikeAcceleration(direction);
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
    private static void BeginPowampSpikeDeletion(EnemyProjectile spike)
    {
        // The animation deletes the spike as its first step, which the animation pass carries out later in this same
        // update.
        spike.StartAnimation(new PowampSpikeAnimation(PowampSpikeSegment.Delete));
    }
}
