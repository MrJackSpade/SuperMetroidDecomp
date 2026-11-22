using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>The Yard species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class YardEnemyBehavior : IEnemySpecies, IEnemySpawnFactory<YardSpawnSettings>
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Yard];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, YardSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new YardEntity(this, occupant, place, common, settings));

    /// <summary>Provides active room context for Yard initialization and main AI.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Runs surface probes and movement while Yard crawls or falls.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Lets Yard react to the active room quake during its main AI.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Applies the common touch and vulnerability paths after Yard-specific reactions.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>The camera's last measure of Samus's sideways movement, which a kicked Yard flies off with.</summary>
    private readonly YardKickTrackingDistance _kickDistance;

    /// <summary>Binds Yard's room, terrain, quake, and collision owners and the kick's camera measure for its registered AI behaviours.</summary>
    internal YardEnemyBehavior(EnemyRoomBinding room, EnemyTerrain terrain, RoomEarthquake earthquake, EnemyCollision collision, EnemyAudioQueue audio,
        YardKickTrackingDistance kickDistance)
    {
        _audio = audio;
        _kickDistance = Ensure.NotNull(kickDistance);
        _room = room;
        _terrain = terrain;
        _earthquake = earthquake;
        _collision = collision;
    }

    /// <summary>
    /// Performs one cue of the Yard's animations. The end of an update of its closed shell may hold the shell closed for
    /// another (#2180); every other cue plays on.
    /// </summary>
    /// <param name="place">The Yard.</param>
    /// <param name="state">The Yard's state.</param>
    /// <param name="cue">The cue to perform.</param>
    /// <param name="playing">Where the play stands just past the cue.</param>
    private AnimationContinuation<YardSegment>? ApplyYardCue(
        RoomEnemyPlace place, YardEnemyState state, YardCue cue, AnimationPlayback<YardSegment> playing) =>
        cue.Match(ApplyYardCueCases, new ApplyYardCueContext(place, state, playing, this));

    /// <summary>What <see cref="ApplyYardCue"/> does on each <see cref="YardCue"/>, built once (#2553).</summary>
    private static YardCueCases<ApplyYardCueContext, AnimationContinuation<YardSegment>?> ApplyYardCueCases { get; } = new(
            MoveBy: static (moveBy, context) =>
            {
                context.Place.Position += moveBy.Offset;
                return (AnimationContinuation<YardSegment>?)null;
            },
            SetDirection: static (setDirection, context) =>
            {
                context.State.Direction = setDirection.Direction;
                context.State.AirborneFacingDirection = YardDirections.ForDirection(context.State.Direction).AirborneFacingDirection;
                return null;
            },
            SetHidingAnimation: static (setHidingAnimation, context) =>
            {
                context.State.HidingAnimation = setHidingAnimation.OptionalTarget;
                return null;
            },
            BeginCrawling: static (beginCrawling, context) =>
            {
                context.State.Motion = YardMotion.Crawling;
                context.State.Crawl = beginCrawling.Crawl;
                return null;
            },
            BeginFlight: static (beginFlight, context) =>
            {
                context.State.Motion = new YardMotion.Airborne(beginFlight.Landing);
                return null;
            },
            Signal: static (signal, context) => context.Owner.YardSignals[signal.Kind](context.State, context.Playing));

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Place">The Yard.</param>
    /// <param name="State">The Yard's state.</param>
    /// <param name="Playing">Where the play stands just past the cue.</param>
    /// <param name="Owner">The <see cref="YardEnemyBehavior"/> reacting.</param>
    private readonly partial record struct ApplyYardCueContext(RoomEnemyPlace Place, YardEnemyState State, AnimationPlayback<YardSegment> Playing, YardEnemyBehavior Owner);

    /// <summary>What each of the Yard's signals does, and the animation it continues at, or null to run on.</summary>
    [field: NonSerialized]
    private IdentityTable<YardCueKind, Func<YardEnemyState, AnimationPlayback<YardSegment>, AnimationContinuation<YardSegment>?>> YardSignals => field ??=
        new(new Dictionary<YardCueKind, Func<YardEnemyState, AnimationPlayback<YardSegment>, AnimationContinuation<YardSegment>?>>
        {
            // The Yard holds still while it turns the corner; the turn's animation begins its next crawl.
            [YardCueKind.BeginTurn] = static (state, _) =>
            {
                state.Motion = YardMotion.HoldingStill;
                return null;
            },
            [YardCueKind.BeginHiding] = static (state, _) =>
            {
                state.Motion = YardMotion.Hiding;
                return null;
            },
            [YardCueKind.ShellClosedUpdateEnds] = (state, playing) =>
                state.Behavior != YardBehavior.Hiding && !YardDecisions.HoldsShellClosedAgain.IsTakenBy(_room.Random.Generate())
                    ? null
                    : HoldShellClosedAgain(playing),
        });

    /// <summary>
    /// Holds the Yard's shell closed for another update (#2180): the phase its closed shell and the cue ending that update
    /// make up, which <paramref name="playing"/> has just played, plays again from its start.
    /// </summary>
    private static AnimationContinuation<YardSegment> HoldShellClosedAgain(AnimationPlayback<YardSegment> playing) =>
        new(playing.Animation, AnimationPlace.StartOf(playing.Place.Phase));

    /// <summary>Initializes the Yard.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static YardEnemyState InitializeYard(YardSpawnSettings settings, RoomEnemyPlace place)
    {
        YardDirection direction = settings.Direction;

        var state = new YardEnemyState
        {
            Direction = YardDirectionDefinitions.BeforeFirstDirectionCue,
            Behavior = YardBehavior.NormalCrawling,
            CrawlSpeed = settings.CrawlSpeed,
        };

        place.Frame = EnemyFrameId.PointAtOrigin;
        place.AnimationClock.StepOnNextUpdate();

        YardDirectionDefinition definition = YardDirections.ForDirection(direction);
        state.Playback = new(YardAnimations.StartingWith(definition.CrawlingAnimation), AnimationPlace.StartOfAnimation);
        state.BlockSide = definition.BlockSide;
        state.HidingAnimation = definition.HidingAnimation;
        state.AirborneFacingDirection = definition.AirborneFacingDirection;
        SetYardCrawlingVelocities(state, direction);
        return state;
    }

    /// <summary>
    /// Sets the Yard's crawl velocity for <paramref name="direction"/>: each axis takes its current crawl speed, or that
    /// speed reversed.
    /// </summary>
    internal static void SetYardCrawlingVelocities(YardEnemyState state, YardDirection direction) =>
        state.CrawlingVelocity = YardCrawlVelocity.Apply(state.CrawlSpeed, direction);

    /// <summary>Runs the Yard's main AI.</summary>
    private void RunYardMain(RoomEnemyPlace place, YardEnemyState state, SamusState samus, RoomLevelData level)
    {

        // A super-missile earthquake drops an attached/hiding Yard, but cannot restart one
        // that is already falling, kicked, or shot through the air.
        if (!YardBehaviorDefinitions.Behaviors[state.Behavior].Airborne &&
            _earthquake.IsShakingWith(SuperMissileDefinitions.WallImpactEarthquake, SuperMissileDefinitions.EarthquakeDuration))
        {
            DropYard(place, state);
        }

        HandleYardHiding(place, state, samus);
        UpdateYardSolidProperty(place, state, samus);

        switch (state.Motion)
        {
            case { Movement: YardMovement.HoldingStill }:
                return;
            case { Movement: YardMovement.Hiding }:
                RunYardHidingMovement(place, state, level);
                return;
            case YardMotion.Airborne flight:
                RunYardAirborneMovement(place, state, samus, level, flight);
                return;
            // Direction-changing animation can publish the new direction one frame before it starts the matching
            // crawl. The game crawls the way its crawl began, not by the briefly newer direction.
            case { Movement: YardMovement.Crawling }:
                RunYardCrawlingMovement(place, state, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Yard has no behaviour for movement {state.Motion}.");
        }
    }

    /// <summary>Runs the look-at/hide test.</summary>
    private static void HandleYardHiding(
        RoomEnemyPlace place,
        YardEnemyState state,
        SamusState samus)
    {
        if (!YardBehaviorDefinitions.Behaviors[state.Behavior].WatchesSamus)
            return;

        bool withinVerticalBand = place.Position.Y.Whole - samus.Position.Y.Whole >= -YardMotionDefinitions.HidingReachAbove;
        SamusFacingDirection samusFacing = SamusPoseMetadata.ReadFacingDirection(samus);
        bool samusLookingAtYard = place.Position.X.Whole < samus.Position.X.Whole
            ? samusFacing == SamusFacingDirection.Left
            : samusFacing == SamusFacingDirection.Right;
        if (withinVerticalBand && samusLookingAtYard)
        {
            if (state.Behavior == YardBehavior.Hiding)
                return;
            if (state.HidingAnimation is { } hiding)
            {
                state.Playback = new(YardAnimations.StartingWith(hiding), AnimationPlace.StartOfAnimation);
                place.Frame = EnemyFrameId.PointAtOrigin;
                place.AnimationClock.Restart();
                state.Behavior = YardBehavior.Hiding;
                return;
            }
        }

        state.Behavior = YardBehavior.NormalCrawling;
    }

    /// <summary>Runs Yard's dynamic solid-hitbox decision.</summary>
    private static void UpdateYardSolidProperty(
        RoomEnemyPlace place,
        YardEnemyState state,
        SamusState samus)
    {
        bool solid;
        if (state.Movement == YardMovement.Airborne)
        {
            solid = false;
        }
        else if (state.Behavior == YardBehavior.AggressiveCrawling)
        {
            solid = !samus.HorizontalSpeed.HasRunningMomentum &&
                state.Movement == YardMovement.HoldingStill;
        }
        else if (samus.HorizontalSpeed.HasRunningMomentum)
        {
            solid = false;
        }
        else
        {
            solid = YardBehaviorDefinitions.Behaviors[state.Behavior].Solid;
        }

        place.Contact = solid
            ? place.Contact.With(EnemyContact.SolidToSamus)
            : place.Contact.Without(EnemyContact.SolidToSamus);
    }

    /// <summary>Runs the hidden-animation movement owner.</summary>
    private static void RunYardHidingMovement(
        RoomEnemyPlace place,
        YardEnemyState state,
        RoomLevelData level)
    {
        bool stillAttached = YardDirections.IsOnWall(state.Direction)
            ? EnemyTerrain.EnemyMeetsAttachmentBlockHorizontallyAhead(
                level,
                place,
                CrawlersBehavior.WithProbeMargin(state.CrawlingVelocity.X, YardMotionDefinitions.HidingProbeMargin))
            : EnemyTerrain.EnemyMeetsAttachmentBlockVerticallyAhead(
                level,
                place,
                CrawlersBehavior.WithProbeMargin(state.CrawlingVelocity.Y, YardMotionDefinitions.HidingProbeMargin));
        if (!stillAttached)
            DropYard(place, state);
    }

    /// <summary>Moves Yard along its wall or floor, probes the turn lookahead, and detaches if the supporting surface ends.</summary>
    internal void RunYardCrawlingMovement(
        RoomEnemyPlace place,
        YardEnemyState state,
        RoomLevelData level)
    {
        // A crawl along a wall moves vertically, one along a floor or ceiling horizontally.
        bool movesVertically = YardDirections.IsOnWall(state.Crawl);
        YardCrawlStep turn = YardTurns.ForCrawl(
            state.Crawl,
            state.TurnsSuppressed);

        // The probe stands the lookahead ahead and moves toward the surface; the Yard keeps that move (ProbeMovementKept).
        var probe = new EnemyTerrainProbe(place, turn.Lookahead);
        bool surfaceCollision = movesVertically
            ? _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                probe,
                CrawlersBehavior.WithProbeMargin(state.CrawlingVelocity.X, YardMotionDefinitions.CrawlProbeMargin))
            : _terrain.MoveEnemyVertically(
                level,
                probe,
                CrawlersBehavior.WithProbeMargin(state.CrawlingVelocity.Y, YardMotionDefinitions.CrawlProbeMargin));
        place.Position = ProbeMovementKept.AfterLookaheadProbe(probe.Position, turn.Lookahead);

        if (!surfaceCollision)
        {
            if (state.MissedSurfaceTooOften())
            {
                DropYard(place, state);
                return;
            }

            if (movesVertically)
                state.CrawlingVelocity = state.CrawlingVelocity.WithY(-state.CrawlingVelocity.Y);
            else
                state.CrawlingVelocity = state.CrawlingVelocity.WithX(-state.CrawlingVelocity.X);
            SetYardAnimationAndDisableTurn(
                place,
                state,
                turn.WhenSurfaceEnds);
            return;
        }

        state.ResetMissedProbes();
        if (movesVertically)
        {
            bool slopeAdjusted = EnemyTerrain.AlignEnemyYWithNonSquareSlopeAndReportAdjustment(level, place);
            CountYardTurnSuppressionPass(state, slopeAdjusted);
            SignedSixteenSixteen tangent = state.CrawlingVelocity.Y.ToSixteenSixteenDelta();
            if (_terrain.MoveEnemyVertically(level, place, tangent))
            {
                state.CrawlingVelocity = state.CrawlingVelocity.WithX(-state.CrawlingVelocity.X);
                SetYardAnimationAndDisableTurn(
                    place,
                    state,
                    turn.WhenSurfaceMet);
            }
            return;
        }

        SignedSixteenSixteen horizontalTangent = CrawlersBehavior.GetSurfaceSlopeAdjustedHorizontalDisplacement(
            place,
            state.CrawlingVelocity,
            level);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, horizontalTangent))
        {
            state.CrawlingVelocity = state.CrawlingVelocity.WithY(-state.CrawlingVelocity.Y);
            SetYardAnimationAndDisableTurn(
                place,
                state,
                turn.WhenSurfaceMet);
            return;
        }

        bool adjusted = EnemyTerrain.AlignEnemyYWithNonSquareSlopeAndReportAdjustment(level, place);
        CountYardTurnSuppressionPass(state, adjusted);
    }

    /// <summary>Suppresses turns again on a slope adjustment, and otherwise counts one slope-free pass toward turning again.</summary>
    private static void CountYardTurnSuppressionPass(
        YardEnemyState state,
        bool slopeAdjusted)
    {
        if (slopeAdjusted)
        {
            state.SuppressTurns();
            return;
        }

        state.TurnSuppressionPassesLeft.Step();
    }

    /// <summary>Starts a turn animation immediately and resets the temporary turn suppression counter.</summary>
    private static void SetYardAnimationAndDisableTurn(
        RoomEnemyPlace place,
        YardEnemyState state,
        YardSegment segment)
    {
        state.Playback = new(YardAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
        state.SuppressTurns();
    }

    /// <summary>Runs the common detach path.</summary>
    internal static void DropYard(RoomEnemyPlace place, YardEnemyState state)
    {
        if (state.Behavior == YardBehavior.Dropped)
            return;
        state.Behavior = YardBehavior.Dropped;
        state.Motion = new YardMotion.Airborne(SetYardAirborneLists(place, state));
        state.AirborneVelocity = FixedVector<RoomPixels>.Zero;
    }

    /// <summary>Runs the complete bounce/gravity/landing loop.</summary>
    /// <param name="place">The Yard.</param>
    /// <param name="state">The Yard's state.</param>
    /// <param name="samus">Samus, whom a landing Yard turns to face.</param>
    /// <param name="level">The room's terrain the shell bounces off.</param>
    /// <param name="flight">The Yard's flight, holding the animation it lands with.</param>
    private void RunYardAirborneMovement(
        RoomEnemyPlace place,
        YardEnemyState state,
        SamusState samus,
        RoomLevelData level,
        YardMotion.Airborne flight)
    {
        if (state.Behavior != YardBehavior.Dropped)
        {
            if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, state.AirborneVelocity.X))
            {
                state.AirborneVelocity = state.AirborneVelocity.WithX(YardAirborneReversal.Reversed(state.AirborneVelocity.X));
                _audio.QueueEnemySound(YardBounceSound, priority: SoundRequestPriority.Low);
            }
            else
            {
                state.AirborneVelocity = state.AirborneVelocity.WithX(YardAirborneSlowing.Slowed(state.AirborneVelocity.X));
            }
        }

        if (_terrain.MoveEnemyVertically(level, place, state.AirborneVelocity.Y))
        {
            int wholeY = state.AirborneVelocity.Y.WholePixels;
            if (wholeY is >= 0 and < YardMotionDefinitions.LandingWholeSpeedLimit)
            {
                LandYard(place, state, samus, flight);
                return;
            }

            state.AirborneVelocity = state.AirborneVelocity.WithY(YardAirborneReversal.Reversed(state.AirborneVelocity.Y));
            return;
        }

        state.AirborneVelocity = state.AirborneVelocity.WithY(YardFallAcceleration.Accelerated(state.AirborneVelocity.Y));
    }

    /// <summary>Stops Yard's fall, selects normal or aggressive crawling, and restores crawler velocity from the way it faces the floor.</summary>
    /// <param name="place">The Yard.</param>
    /// <param name="state">The Yard's state.</param>
    /// <param name="samus">Samus, whom an aggressive Yard turns to face.</param>
    /// <param name="flight">The flight ending, holding the animation its airborne animation named to land with.</param>
    private static void LandYard(RoomEnemyPlace place, YardEnemyState state, SamusState samus, YardMotion.Airborne flight)
    {
        state.AirborneVelocity = FixedVector<RoomPixels>.Zero;
        state.ResetMissedProbes();
        state.SuppressTurns();

        // A dropped Yard lands with the animation its flight holds; an aggressive one faces Samus and lands with the one
        // the airborne animations of the way it then faces name.
        YardSegment landingAnimation = flight.Landing;
        if (state.Behavior == YardBehavior.Dropped)
        {
            state.Behavior = YardBehavior.NormalCrawling;
        }
        else
        {
            state.Behavior = YardBehavior.AggressiveCrawling;
            state.CrawlSpeed = YardVelocityDefinitions.AggressiveCrawlSpeed;
            MakeYardFaceSamusHorizontally(place, state, samus);
            landingAnimation = SetYardAirborneLists(place, state);
        }

        // After landing, Yard starts crawling as a new crawler does, but keeps its own state and main AI. Only the
        // crawler's starting velocity carries over: the live crawl speed on both axes, the horizontal one reversed for
        // a block on Yard's right.
        state.BlockSide = state.AirborneFacingDirection == HorizontalDirection.Right
            ? CrawlerSurfaceOrientation.OnRightWall
            : CrawlerSurfaceOrientation.OnLeftWall;
        // Surface direction can still describe a wall or ceiling. The landing instead uses
        // the new floor-facing block side for its downward probe and horizontal sign.
        var landing = new PixelVelocity<RoomPixels>(state.CrawlSpeed, state.CrawlSpeed);
        state.CrawlingVelocity = state.BlockSide == CrawlerSurfaceOrientation.OnLeftWall ? landing.ReversedX() : landing;
        place.Frame = EnemyFrameId.PointAtOrigin;
        state.Motion = YardMotion.HoldingStill;
        state.Playback = EnemyAnimationPlayback.Start(place, YardAnimations.StartingWith(landingAnimation));
    }

    /// <summary>Selects visible and hiding animations from Yard's airborne facing direction.</summary>
    /// <returns>The hiding animation selected, which the Yard lands with.</returns>
    private static YardSegment SetYardAirborneLists(
        RoomEnemyPlace place,
        YardEnemyState state)
    {
        YardAirborneAnimationDefinition definition =
            YardDirections.ForAirborneFacing(state.AirborneFacingDirection);
        state.Playback = new(YardAnimations.StartingWith(definition.VisibleAnimation), AnimationPlace.StartOfAnimation);
        state.HidingAnimation = definition.HidingAnimation;
        place.AnimationClock.Restart();
        return definition.HidingAnimation;
    }

    /// <summary>Turns Yard after landing only when its airborne facing points away from Samus.</summary>
    private static void MakeYardFaceSamusHorizontally(
        RoomEnemyPlace place,
        YardEnemyState state,
        SamusState samus)
    {
        bool shouldTurn = state.AirborneFacingDirection == HorizontalDirection.Right
            ? place.Position.X.Whole >= samus.Position.X.Whole
            : place.Position.X.Whole < samus.Position.X.Whole;
        if (shouldTurn)
            TurnYardAround(state);
    }

    /// <summary>Reverses Yard's crawl direction and associated graphics, properties, and velocities unless it is hidden or holding still.</summary>
    internal static void TurnYardAround(YardEnemyState state)
    {
        if (state.Behavior == YardBehavior.Hiding ||
            state.Movement == YardMovement.HoldingStill)
        {
            return;
        }

        state.Direction = YardDirections.ForDirection(state.Direction).OppositeDirection;
        YardDirectionDefinition definition = YardDirections.ForDirection(state.Direction);
        state.Playback = new(YardAnimations.StartingWith(definition.CrawlingAnimation), AnimationPlace.StartOfAnimation);
        state.BlockSide = definition.BlockSide;
        state.HidingAnimation = definition.HidingAnimation;
        state.AirborneFacingDirection = definition.AirborneFacingDirection;
        SetYardCrawlingVelocities(state, state.Direction);
        state.Motion = YardMotion.Crawling;
        state.Crawl = definition.Crawl;
        state.SuppressTurns();
    }

    /// <summary>
    /// Runs the running/airborne kick setup: the shell flies sideways at the camera's last measure of Samus's movement,
    /// reversed when she faces left, and upward at the speed that measure's whole pixels select.
    /// </summary>
    internal static void KickYardIntoAir(
        RoomEnemyPlace place,
        YardEnemyState state,
        SamusState samus,
        YardKickTrackingDistance kickDistance)
    {
        Ensure.NotNull(kickDistance);
        state.Behavior = YardBehavior.Kicked;
        state.Motion = new YardMotion.Airborne(SetYardAirborneLists(place, state));

        SignedSixteenSixteen horizontal = kickDistance.HorizontalDistancePlusOne;
        SignedSixteenSixteen kick = YardKicks.ForHorizontalSpeed(kickDistance.KickChoiceSpeed);
        if (SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left))
            horizontal = YardAirborneReversal.Reversed(horizontal);
        state.AirborneVelocity = new FixedVector<RoomPixels>(
            horizontal,
            kick);
    }

    /// <summary>Runs ordinary-beam launch setup.</summary>
    internal static void ShootYardIntoAir(
        RoomEnemyPlace place,
        YardEnemyState state,
        SamusState samus)
    {
        state.Behavior = YardBehavior.BeamLaunched;
        state.Motion = new YardMotion.Airborne(SetYardAirborneLists(place, state));
        state.AirborneVelocity = YardShotLaunchFractions.Launched(state.AirborneVelocity, new Offset16<RoomPixels>(
            YardMotionDefinitions.BeamLaunchWholeSpeed.Toward(
                SamusPoseMetadata.ReadFacingDirection(samus) == SamusFacingDirection.Left ? HorizontalDirection.Left : HorizontalDirection.Right),
            -YardMotionDefinitions.BeamLaunchWholeSpeed));
    }

    /// <summary>Runs Yard's custom touch entry.</summary>
    private void ResolveYardTouch(
        RoomEnemyOccupant occupant, YardSpawnSettings yardSpawnSettings, RoomEnemyPlace place,
        YardEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        GameplayAction controllerInput)
    {
        bool aggressiveCrawlRejectsKick = state.Behavior == YardBehavior.AggressiveCrawling &&
            state.Movement != YardMovement.HoldingStill &&
            SamusIsDirectingTowardYard(state, controllerInput);
        bool kickRequested = !aggressiveCrawlRejectsKick &&
            (state.Movement == YardMovement.Airborne ||
             samus.HorizontalSpeed.HasRunningMomentum);

        if (kickRequested)
        {
            KickYardIntoAir(place, state, samus, _kickDistance);
            _audio.QueueEnemySound(YardBounceSound, priority: SoundRequestPriority.Low);
            return;
        }

        // A shell holding still, a falling shell, and an already kicked shell
        // deliberately do nothing on overlap. Every other route enters normal enemy touch,
        // restores the population's idle speed, optionally turns, then resumes normal crawling.
        if (state.Movement == YardMovement.HoldingStill ||
            !YardBehaviorDefinitions.Behaviors[state.Behavior].TakesHits)
        {
            return;
        }

        _collision.ResolveOrdinaryTouch(occupant, samus, pseudoScrew);
        state.CrawlSpeed = yardSpawnSettings.CrawlSpeed;
        if (state.Behavior != YardBehavior.NormalCrawling)
            TurnYardAround(state);
        state.Behavior = YardBehavior.NormalCrawling;
    }

    /// <summary>Reads the exact held horizontal direction used by Yard's touch reaction.</summary>
    private static bool SamusIsDirectingTowardYard(
        YardEnemyState state,
        GameplayAction controllerInput)
    {
        bool pressingExactlyRight =
            (controllerInput & GameplayActions.HorizontalDirections) == GameplayAction.Right;
        bool yardFacesRight = state.AirborneFacingDirection == HorizontalDirection.Right;
        return pressingExactlyRight ? !yardFacesRight : yardFacesRight;
    }

    /// <summary>
    /// Completes Yard's reaction to a shot that is not a missile. Collision/beam impact
    /// disposal has already happened in the shared projectile pass; this method owns only
    /// the actor's launch and observable sound event.
    /// </summary>
    internal void ResolveYardBeamLaunch(
        RoomEnemyPlace place,
        YardEnemyState state,
        SamusState samus)
    {
        if (YardBehaviorDefinitions.Behaviors[state.Behavior].TakesHits)
            ShootYardIntoAir(place, state, samus);
        _audio.QueueEnemySound(YardBounceSound, priority: SoundRequestPriority.Low);
    }

    /// <summary>Runs Yard's main AI.</summary>
    internal void RunYardMainAi(RoomEnemyPlace place, YardEnemyState yardEnemyState, IEnemyAiFrame frame)
    {
        RunYardMain(place, yardEnemyState, frame.Samus, frame.Level);
    }

    /// <summary>Advances Yard's typed animation.</summary>
    internal void AdvanceYardTypedAnimation(RoomEnemyPlace place, YardEnemyState state)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<YardSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, YardAnimations.Segments, ref playback,
            (cue, playing) => ApplyYardCue(place, state, cue, playing));
        state.Playback = playback;
    }

    /// <summary>Kicks a struck Yard's shell into the air, or lets a bomb's or a power bomb's vulnerability damage run.</summary>
    /// <param name="kicksShell">The Yard's own method kicking its shell into the air.</param>
    internal static EnemyShotReaction YardShotReaction(EnemyShotStage kicksShell) => new()
    {
        AdmitsBombFamilies = true,
        // A bomb or a power bomb deals the Yard its vulnerability damage; every other shot that strikes it only kicks
        // the shell into the air, after the shot's impact.
        BeforeVulnerability = kicksShell,
    };

    /// <summary>Runs Yard's touch reaction, including its facing-dependent shell response.</summary>
    internal void ReactToYardTouch(RoomEnemyOccupant occupant, YardSpawnSettings yardSpawnSettings, EnemyTouchContext context, YardEnemyState yardEnemyState) =>
        ResolveYardTouch(occupant, yardSpawnSettings, context.Place, yardEnemyState, context.Samus, context.PseudoScrew, context.ControllerInput);

    /// <summary>The enemy-library sound queued by a Yard shell kick, launch or wall bounce.</summary>
    private static readonly SoundEffectId YardBounceSound =
        EnemySoundEffectDefinitions.YardBounceSound;
}
