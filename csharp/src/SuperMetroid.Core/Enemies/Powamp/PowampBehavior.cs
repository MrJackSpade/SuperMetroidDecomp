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
internal sealed class PowampBehavior : IEnemySpecies, IEnemySpawnFactory<PowampSpawnSettings>
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Powamp];

    /// <summary>
    /// A Powamp's body or balloon, as the placement's settings name the part it is (#2163). The balloon shares the body's
    /// definition; only its placement tells them apart.
    /// </summary>
    /// <exception cref="InvalidOperationException">The placement names no part of a Powamp.</exception>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, PowampSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        settings switch
        {
            PowampBalloonSpawnSettings balloon => EnemyOccupant.PlacedOnly(new PowampBalloonEntity(this, occupant, place, common, balloon)),
            PowampBodySpawnSettings body => EnemyOccupant.Running(new PowampBodyEntity(this, occupant, place, common, body)),
            var part => throw new InvalidOperationException($"Undefined Powamp part {part}."),
        };

    /// <summary>Clips body rise and sink motion against room collision.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Spawns the eight radial spikes emitted at death.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The room's enemy places, where each body finds its balloon as it is built.</summary>
    private readonly EnemyPlaces _places;
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
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static PowampBalloonState InitializePowampBalloon(RoomEnemyPlace place, PowampBalloonSpawnSettings settings)
    {
        PrepareToAnimate(place);
        var balloon = new PowampBalloonState
        {
            Spawn = place.Position.Whole,
            RefusesContact = Ensure.NotNull(settings).Contact == PowampContact.Refuses,
        };
        StartPowampBalloonAnimation(place, balloon, PowampSegment.BalloonDeflated);
        return balloon;
    }

    /// <summary>Initializes a Powamp's body with its own state (#1845), resting, joined to the balloon its room lists before it.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static PowampBodyState InitializePowampBody(RoomEnemyPlace place)
    {
        PrepareToAnimate(place);
        var state = new PowampBodyState(PowampAction.DeflatedResting);
        state.ActionTimer = UpdateCountdown.For(PowampMotionDefinitions.RestDuration);
        state.Playback = StartPowampAnimation(place, PowampSegment.BodySlow);
        return state;
    }

    /// <summary>Runs every action of a Powamp's body; the balloon has none, its body moves it.</summary>
    internal void RunPowampMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, PowampBodyState state, PowampBalloonEntity balloonEntity,
        RoomLevelData level, SamusState samus)
    {
        PowampBalloon balloon = BalloonOf(balloonEntity);
        switch (state.Action)
        {
            case PowampAction.DeflatedResting:
                RunPowampDeflatedResting(place, state, balloon);
                return;
            case PowampAction.Inflating:
                RunPowampInflating(place, state, balloon);
                return;
            case PowampAction.InflatedRiseToTargetHeight:
                RunPowampRise(place, state, balloon, balloonEntity.Settings, level, hold: PowampHold.Free);
                return;
            case PowampAction.InflatedFinishWiggle:
                RunPowampFinishWiggle(place, state, balloon, level, hold: PowampHold.Free);
                return;
            case PowampAction.GrappledRiseToTargetHeight:
                RunPowampRise(place, state, balloon, balloonEntity.Settings, level, hold: PowampHold.Grappled);
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
        PowampBalloon balloon, PowampBalloonSpawnSettings powampBalloonSpawnSettings,
        RoomLevelData level,
        PowampHold hold)
    {
        // The ordinary rise changes behaviour and runs the grappled behaviour immediately
        // on the frame the grapple beam takes hold.
        bool grappleAiActive = body.IsGrappled;
        if (hold != PowampHold.Grappled && grappleAiActive)
        {
            state.Action = PowampAction.GrappledRiseToTargetHeight;
            RunPowampRise(body, state, balloon, powampBalloonSpawnSettings, level, hold: PowampHold.Grappled);
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
            ? powampBalloonSpawnSettings.GrappleTravelDistance
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

    /// <summary>
    /// Finishes a touch on a Powamp half: when it kills the half, the enemy the room lists before it (a body's balloon)
    /// goes and its spikes burst out.
    /// </summary>
    private void ResolvePowampTouch(RoomEnemyOccupant occupant, RoomEnemyPlace body, RoomEnemyPlace before, SamusState samus, PseudoScrewCharge pseudoScrew)
    {
        // The spikes take the body's colours from before the touch, which may change them.
        PictureShading lookBeforeTouch = body.Shading;
        _collision.ResolveOrdinaryTouch(occupant, samus, pseudoScrew);
        if (body.Health != 0)
            return;

        before.Presence = before.Presence.With(EnemyPresence.Deleted);
        SpawnPowampSpikeBurst(occupant, body, lookBeforeTouch);
        body.Shading = EnemyShades.InProjectileLook;
    }

    /// <summary>Copies common shot/freeze presentation from the body to its balloon.</summary>
    private static void ResolvePowampShotAfterCommon(RoomEnemyPlace body, PowampBodyState powampBodyState, RoomEnemyPlace balloon)
    {
        if (body.IsFrozen)
        {
            balloon.Freeze.Restart(body.Freeze.UpdatesLeft);
            balloon.MarkFrozen();
        }
        if (body.IsHurt)
        {
            balloon.HurtFlash.Restart(body.HurtFlash.UpdatesLeft);
            balloon.MarkHurt();
        }
        if (body.Health == 0)
        {
            // The fatal action refuses Samus's later touch and shots (PowampBodyState.IsDying).
            powampBodyState.Action = PowampAction.FatalDamage;
        }
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

    /// <summary>
    /// The balloon half the room lists, and so places, right before <paramref name="body"/>, which the body receives as it
    /// is built and checks once there (#2554).
    /// </summary>
    /// <exception cref="InvalidDataException">The body is not listed right after a Powamp balloon.</exception>
    internal PowampBalloonEntity PlacedBalloon(RoomEnemyPlace body) =>
        _places.Member(body, PowampPartListingDefinitions.Listing, PowampMember.Body, PowampMember.Balloon).Entity is PowampBalloonEntity balloon
            ? balloon
            : throw new InvalidDataException($"Powamp body place {body.Ordinal} is not preceded by its balloon half.");

    /// <summary>The balloon a body holds: its place, the state it holds and its occupant.</summary>
    private static PowampBalloon BalloonOf(PowampBalloonEntity balloon) => new(balloon.Place, balloon.State, balloon.Occupant);

    /// <summary>Routes bomb hits on a body through common shot logic and mirrors damage state to its balloon.</summary>
    /// <param name="bodyState">The state of the body the reaction is built for.</param>
    /// <param name="balloon">The balloon the body holds.</param>
    internal static EnemyBombReaction PowampBodyBombReaction(PowampBodyState bodyState, RoomEnemyPlace balloon) => new()
    {
        RunsGenericDeath = false,
        // Powamp's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = c => ResolvePowampShotAfterCommon(c.Enemy, bodyState, balloon),
    };

    /// <summary>
    /// Routes bomb hits on a balloon through common shot logic and the Powamp's reaction to a shot as the balloon runs it
    /// (<see cref="ResolvePowampBalloonShotAfterCommon"/>).
    /// </summary>
    /// <param name="balloonState">The state of the balloon the reaction is built for.</param>
    internal EnemyBombReaction PowampBalloonBombReaction(PowampBalloonState balloonState) => new()
    {
        RunsGenericDeath = false,
        // Powamp's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = c => ResolvePowampBalloonShotAfterCommon(c.Enemy, balloonState),
    };

    /// <summary>
    /// The Powamp's reaction to a shot after its common damage, as a balloon that accepted the hit runs it (#2554): it
    /// carries the struck half's freeze and hurt flash on to the enemy the room lists before it, which for a balloon is the
    /// enemy before the Powamp, and marks a half the hit left without health as refusing contact. Its fatal-damage action
    /// is the body's AI's, which a balloon has none of. Every room's balloons refuse contact from their setup, so their own
    /// reaction to a shot ends before its damage.
    /// </summary>
    private void ResolvePowampBalloonShotAfterCommon(RoomEnemyPlace balloon, PowampBalloonState balloonState)
    {
        RoomEnemyPlace before = _places.PlacesBefore(balloon)(1);
        if ((balloon.AiStates & EnemyAiStates.Frozen) != EnemyAiStates.None)
        {
            before.Freeze.Restart(balloon.Freeze.UpdatesLeft);
            before.AiStates = before.AiStates | EnemyAiStates.Frozen;
        }
        if ((balloon.AiStates & EnemyAiStates.Hurt) != EnemyAiStates.None)
        {
            before.HurtFlash.Restart(balloon.HurtFlash.UpdatesLeft);
            before.AiStates = before.AiStates | EnemyAiStates.Hurt;
        }
        if (balloon.Health == 0)
            balloonState.RefusesContact = true;
    }

    /// <summary>
    /// A Powamp spike (#1760): it flies outward, accelerating, with room collision. A shot spike runs the same private
    /// delete animation its definition names as its shot animation.
    /// </summary>
    private StatefulSingleMotionProjectileBehavior<PowampSpikeState> Spike => field ??= new(
        context => FlyPowampSpike(context.Projectile, context.Level),
        shot: () => new PowampSpikeAnimation(PowampSpikeSegment.Delete));

    /// <summary>Advances the body's pulse.</summary>
    internal static void AdvancePowampBodyTypedAnimation(RoomEnemyPlace place, PowampBodyState powampBodyState) =>
        EnemyAnimationPlayback.Advance(place, powampBodyState, PowampAnimations.Segments,
            static (PowampBodyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => RaisesNoCues(cue));

    /// <summary>Advances the balloon's inflate or sink animation and the poses it shows.</summary>
    internal static void AdvancePowampBalloonTypedAnimation(RoomEnemyPlace place, PowampBalloonState balloon)
    {
        if (!place.AnimationClock.Expire())
            return;
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
    /// <param name="balloon">The balloon the body holds.</param>
    internal static EnemyPowerBombReaction PowampBodyPowerBombReaction(RoomEnemyPlace balloon) => new()
    {
        AfterCommon = c => EnemyPartnerHits.CarryOnto(c.Enemy, balloon),
    };

    /// <summary>The balloon's Power Bomb reaction: the blast's damage alone, its body carrying a hit on to it.</summary>
    internal EnemyPowerBombReaction PowampBalloonPowerBombReaction => field ??= new();

    /// <summary>Skips later shots once fatal damage begins and mirrors surviving body presentation to the balloon.</summary>
    /// <param name="powampSpawnSettings">What the enemy's population entry tells its species.</param>
    /// <param name="bodyState">The state the body holds.</param>
    /// <param name="balloon">The balloon the body holds.</param>
    internal static EnemyShotReaction PowampBodyShotReaction(PowampSpawnSettings powampSpawnSettings, PowampBodyState bodyState,
        RoomEnemyPlace balloon) =>
        ShotReactionSkipping(place => BodyRefusesContact(powampSpawnSettings, bodyState),
            c => ResolvePowampShotAfterCommon(c.Enemy, bodyState, balloon));

    /// <summary>The Powamp's shot reaction as the balloon takes it, skipped while the balloon refuses contact.</summary>
    /// <param name="balloonState">The state of the balloon that was shot.</param>
    internal EnemyShotReaction PowampBalloonShotReaction(PowampBalloonState balloonState) =>
        ShotReactionSkipping(_ => balloonState.RefusesContact, c => ResolvePowampBalloonShotAfterCommon(c.Enemy, balloonState));

    /// <summary>Whether a half's placement refuses Samus's touch and shots (#2474), as a grappled balloon's does.</summary>
    private static bool PlacementRefusesContact(PowampSpawnSettings powampSpawnSettings) =>
        powampSpawnSettings.Contact == PowampContact.Refuses;

    /// <summary>Whether the body refuses Samus's touch and shots: its placement refuses them, or it is dying (#2474).</summary>
    private static bool BodyRefusesContact(PowampSpawnSettings powampSpawnSettings, PowampBodyState powampBodyState) =>
        PlacementRefusesContact(powampSpawnSettings) || powampBodyState.IsDying;

    /// <summary>
    /// The Powamp's shot reaction for a half that refuses shots while <paramref name="refusesContact"/> says so, a body
    /// carrying the hit on to its balloon (<paramref name="afterCommon"/>).
    /// </summary>
    private static EnemyShotReaction ShotReactionSkipping(Func<RoomEnemyPlace, bool> refusesContact, Action<EnemyShotContext> afterCommon) => new()
    {
        // A lethal shot starts the Powamp's death delay, during which later shots pass through it, neither spent
        // nor exploding, while the body visibly deflates; it dies in its own way, not in the usual burst.
        SkipsEnemy = refusesContact,
        Lethal = EnemyShotLethal.SpeciesOwned,
        // A shot damages a Powamp without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        AfterCommon = afterCommon,
    };

    /// <summary>Applies touch behavior to the body before its fatal-shot state begins.</summary>
    internal void ReactToPowampBodyTouch(RoomEnemyOccupant occupant, PowampSpawnSettings powampSpawnSettings, EnemyTouchContext context, PowampBodyState powampBodyState,
        RoomEnemyPlace balloon)
    {
        if (!BodyRefusesContact(powampSpawnSettings, powampBodyState))
            ResolvePowampTouch(occupant, context.Place, balloon, context.Samus, context.PseudoScrew);
    }

    /// <summary>Applies the Powamp's touch to the balloon while it accepts contact.</summary>
    internal void ReactToPowampBalloonTouch(RoomEnemyOccupant occupant, PowampBalloonState balloonState, EnemyTouchContext context)
    {
        // A balloon that refuses contact ignores Samus's touch.
        // A touch that kills it deletes the enemy listed before it, as the Powamp's touch does for either half.
        if (!balloonState.RefusesContact)
            ResolvePowampTouch(occupant, context.Place, _places.PlacesBefore(context.Place)(1), context.Samus, context.PseudoScrew);
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
