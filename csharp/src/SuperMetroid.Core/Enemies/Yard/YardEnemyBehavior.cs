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
internal sealed class YardEnemyBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Yard];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new YardEntity(this, place, common);

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
    /// Carries out one cue of the Yard's animations. Repeating the previous pose continues <paramref name="playing"/> at the
    /// pose shown just before the cue, as the cue steps the animation back one pose; every other cue plays on.
    /// </summary>
    private AnimationContinuation<YardSegment>? ApplyYardCue(
        RoomEnemyPlace place, YardEnemyState state, YardCue cue, Animation<YardSegment> playing)
    {
        switch (cue)
        {
            case YardCue.Signal { Kind: YardCueKind.BeginTurn }:
                // The Yard holds still while it turns the corner; the turn's animation begins its next crawl.
                state.Movement = YardMovement.HoldingStill;
                return null;
            case YardCue.BeginCrawling beginCrawling:
                state.Movement = beginCrawling.Crawl;
                return null;
            case YardCue.Signal { Kind: YardCueKind.BeginHiding }:
                state.Movement = YardMovement.Hiding;
                return null;
            case YardCue.Signal { Kind: YardCueKind.BeginFlight }:
                state.Movement = YardMovement.Airborne;
                return null;
            case YardCue.SetHidingAnimation setHidingAnimation:
                state.HidingAnimation = setHidingAnimation.OptionalTarget;
                return null;
            case YardCue.SetDirection setDirection:
                state.Direction = setDirection.Direction;
                state.AirborneFacingDirection = YardDirections.ForDirection(state.Direction).AirborneFacingDirection;
                return null;
            case YardCue.MoveBy moveBy:
                place.Position += moveBy.Offset;
                return null;
            case YardCue.Signal { Kind: YardCueKind.RepeatPreviousPoseUnlessContinuing }:
                if (state.Behavior != YardBehavior.Hiding && !YardDecisions.RepeatPreviousPose.IsSetIn(_room.Random.Generate()))
                    return null;
                return new AnimationContinuation<YardSegment>(playing, PoseBeforeRepeatCue(playing));
            default:
                throw new InvalidOperationException($"Undefined {nameof(YardCueKind)} {cue}.");
        }
    }

    /// <summary>
    /// The place of <paramref name="playing"/> whose next beat is the pose just before its repeat cue: the animation plays
    /// one phase with that cue, and the pose sits immediately before it in every authored segment.
    /// </summary>
    /// <exception cref="InvalidDataException">The animation has no single repeat cue, or the cue does not follow a pose.</exception>
    private static AnimationPlace PoseBeforeRepeatCue(Animation<YardSegment> playing)
    {
        AnimationPlace? found = null;
        for (int phase = 0; phase < playing.Phases.Count; phase++)
        {
            AnimationPhase<YardSegment> run = playing.Phases[phase];
            IReadOnlyList<AnimationBeat<EnemyFrameId, YardCue>> beats = YardAnimations.Segments.Beats(run.Segment);
            for (int beat = 0; beat < run.BeatCount; beat++)
            {
                if (beats[run.FirstBeat + beat] is not AnimationBeat<EnemyFrameId, YardCue>.Event { Raised: YardCue.Signal { Kind: YardCueKind.RepeatPreviousPoseUnlessContinuing } })
                    continue;
                if (found is not null)
                    throw new InvalidDataException($"A Yard animation through {run.Segment} has more than one repeat cue.");
                if (beat == 0 || beats[run.FirstBeat + beat - 1] is not AnimationBeat<EnemyFrameId, YardCue>.Frame)
                    throw new InvalidDataException($"Yard repeat cue in {run.Segment} does not follow a pose.");
                found = new AnimationPlace(phase, beat - 1);
            }
        }
        return found ?? throw new InvalidDataException("A Yard animation raised a repeat cue it does not play.");
    }

    /// <summary>Initializes the Yard.</summary>
    internal static void InitializeYard(RoomEnemyPlace place)
    {
        YardSpawnSettings settings = place.SpawnSettings<YardSpawnSettings>();
        YardDirection direction = settings.Direction;

        var state = new YardEnemyState(YardMovement.HoldingStill)
        {
            // The population entry selects the initial animations and velocity signs, but
            // does not set the live direction. Only the direction cue does. A
            // first-frame hide can bypass it and keeps the first direction.
            Direction = YardDirection.UpsideRightMovingUp,
            Behavior = YardBehavior.NormalCrawling,
            IdleCrawlSpeed = settings.CrawlSpeed,
            CrawlSpeed = settings.CrawlSpeed,
        };
        place.SetSpeciesState(state);

        place.Frame = EnemyFrameId.Empty;
        place.AnimationClock.StepOnNextUpdate();

        YardDirectionDefinition definition = YardDirections.ForDirection(direction);
        state.Playback = new(YardAnimations.StartingWith(definition.CrawlingAnimation), AnimationPlace.StartOfAnimation);
        state.BlockSide = definition.BlockSide;
        state.HidingAnimation = definition.HidingAnimation;
        state.AirborneFacingDirection = definition.AirborneFacingDirection;
        SetYardCrawlingVelocities(state, direction);
    }

    /// <summary>
    /// Sets the Yard's crawl velocity for <paramref name="direction"/>: each axis takes its current crawl speed, or that
    /// speed reversed.
    /// </summary>
    internal static void SetYardCrawlingVelocities(YardEnemyState state, YardDirection direction) =>
        state.CrawlingVelocity = YardCrawlVelocity.Apply(state.CrawlSpeed, direction);

    /// <summary>Runs the Yard's main AI.</summary>
    private void RunYardMain(RoomEnemyPlace place, SamusState samus, RoomLevelData level)
    {
        YardEnemyState state = place.RequireSpeciesState<YardEnemyState>();

        // A super-missile earthquake drops an attached/hiding Yard, but cannot restart one
        // that is already falling, kicked, or shot through the air.
        if (state.Behavior is not (YardBehavior.Dropped or YardBehavior.Kicked or YardBehavior.BeamLaunched) &&
            _earthquake.IsShakingWith(SuperMissileDefinitions.WallImpactEarthquake, SuperMissileDefinitions.EarthquakeDuration))
        {
            DropYard(place, state);
        }

        HandleYardHiding(place, state, samus);
        UpdateYardSolidProperty(place, state, samus);

        switch (state.Movement)
        {
            case YardMovement.HoldingStill:
                return;
            case YardMovement.Hiding:
                RunYardHidingMovement(place, state, level);
                return;
            case YardMovement.Airborne:
                RunYardAirborneMovement(place, state, samus, level);
                return;
            // Direction-changing animation can publish the new direction one frame before
            // it installs the matching movement behaviour. The game moves by that behaviour,
            // not by the temporarily newer direction field.
            case YardMovement.CrawlingUpsideUpMovingLeft:
            case YardMovement.CrawlingUpsideLeftMovingDown:
            case YardMovement.CrawlingUpsideDownMovingRight:
            case YardMovement.CrawlingUpsideRightMovingUp:
            case YardMovement.CrawlingUpsideUpMovingRight:
            case YardMovement.CrawlingUpsideRightMovingDown:
            case YardMovement.CrawlingUpsideDownMovingLeft:
            case YardMovement.CrawlingUpsideLeftMovingUp:
                RunYardCrawlingMovement(place, state, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Yard has no behaviour for movement {state.Movement}.");
        }
    }

    /// <summary>Runs the look-at/hide test.</summary>
    private static void HandleYardHiding(
        RoomEnemyPlace place,
        YardEnemyState state,
        SamusState samus)
    {
        if (state.Behavior is YardBehavior.AggressiveCrawling or YardBehavior.Dropped or YardBehavior.Kicked or YardBehavior.BeamLaunched)
            return;

        bool withinVerticalBand = CoordinateAxis.Difference(place.Position.Y.Whole, samus.Position.Y.Whole) >= -YardMotionDefinitions.HidingReachAbove;
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
                place.Frame = EnemyFrameId.Empty;
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
            solid = state.Behavior is not (YardBehavior.Dropped or YardBehavior.BeamLaunched);
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
                CrawlersBehavior.WithProbeMargin(state.CrawlingVelocity.X, 7))
            : EnemyTerrain.EnemyMeetsAttachmentBlockVerticallyAhead(
                level,
                place,
                CrawlersBehavior.WithProbeMargin(state.CrawlingVelocity.Y, 7));
        if (!stillAttached)
            DropYard(place, state);
    }

    /// <summary>Moves Yard along its wall or floor, probes the turn lookahead, and detaches if the supporting surface ends.</summary>
    internal void RunYardCrawlingMovement(
        RoomEnemyPlace place,
        YardEnemyState state,
        RoomLevelData level)
    {
        bool movesVertically = state.Movement is
            YardMovement.CrawlingUpsideLeftMovingDown or
            YardMovement.CrawlingUpsideRightMovingUp or
            YardMovement.CrawlingUpsideRightMovingDown or
            YardMovement.CrawlingUpsideLeftMovingUp;
        YardTurnDefinition turn = YardTurns.ForMovement(
            state.Movement,
            state.TurnTransitionDisabled);

        // The lookahead offset is temporary, but the normal-axis probe movement is not.
        // Adds the offset, performs collision/alignment, then subtracts only the
        // original offset from the resulting center.
        place.Position += turn.Lookahead;
        bool surfaceCollision = movesVertically
            ? _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                CrawlersBehavior.WithProbeMargin(state.CrawlingVelocity.X, 1))
            : _terrain.MoveEnemyVertically(
                level,
                place,
                CrawlersBehavior.WithProbeMargin(state.CrawlingVelocity.Y, 1));
        place.Position -= turn.Lookahead;

        if (!surfaceCollision)
        {
            state.ConsecutiveTurnCounter++;
            if (state.ConsecutiveTurnCounter >= YardEnemyState.MissedProbesBeforeFalling)
            {
                DropYard(place, state);
                return;
            }

            if (movesVertically)
                state.CrawlingVelocity = state.CrawlingVelocity.WithY(NegateEightEight(state.CrawlingVelocity.Y));
            else
                state.CrawlingVelocity = state.CrawlingVelocity.WithX(NegateEightEight(state.CrawlingVelocity.X));
            SetYardAnimationAndDisableTurn(
                place,
                state,
                turn.OutsideTurnAnimation);
            return;
        }

        state.ConsecutiveTurnCounter = 0;
        if (movesVertically)
        {
            bool slopeAdjusted = EnemyTerrain.AlignEnemyYWithNonSquareSlopeAndReportAdjustment(level, place);
            HandleYardTurnTransitionDisabling(state, slopeAdjusted);
            SignedSixteenSixteen tangent = state.CrawlingVelocity.Y.ToSixteenSixteenDelta();
            if (_terrain.MoveEnemyVertically(level, place, tangent))
            {
                state.CrawlingVelocity = state.CrawlingVelocity.WithX(NegateEightEight(state.CrawlingVelocity.X));
                SetYardAnimationAndDisableTurn(
                    place,
                    state,
                    turn.InsideTurnAnimation);
            }
            return;
        }

        SignedSixteenSixteen horizontalTangent = CrawlersBehavior.GetSurfaceSlopeAdjustedHorizontalDisplacement(
            place,
            state.CrawlingVelocity,
            level);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, horizontalTangent))
        {
            state.CrawlingVelocity = state.CrawlingVelocity.WithY(NegateEightEight(state.CrawlingVelocity.Y));
            SetYardAnimationAndDisableTurn(
                place,
                state,
                turn.InsideTurnAnimation);
            return;
        }

        bool adjusted = EnemyTerrain.AlignEnemyYWithNonSquareSlopeAndReportAdjustment(level, place);
        HandleYardTurnTransitionDisabling(state, adjusted);
    }

    /// <summary>Keeps turn transitions disabled on a slope adjustment and reenables them after sixteen clear passes.</summary>
    private static void HandleYardTurnTransitionDisabling(
        YardEnemyState state,
        bool slopeAdjusted)
    {
        if (slopeAdjusted)
        {
            state.TurnTransitionDisabled = true;
            state.TurnTransitionDisableCounter = 0;
            return;
        }

        int next = state.TurnTransitionDisableCounter + 1;
        if (next >= YardMotionDefinitions.TurnSuppressionPasses)
        {
            state.TurnTransitionDisabled = false;
            return;
        }
        state.TurnTransitionDisableCounter = next;
    }

    /// <summary>Starts a turn animation immediately and resets the temporary turn suppression counter.</summary>
    private static void SetYardAnimationAndDisableTurn(
        RoomEnemyPlace place,
        YardEnemyState state,
        YardSegment segment)
    {
        state.Playback = new(YardAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
        state.TurnTransitionDisabled = true;
        state.TurnTransitionDisableCounter = 0;
    }

    /// <summary>Runs the common detach path.</summary>
    internal static void DropYard(RoomEnemyPlace place, YardEnemyState state)
    {
        if (state.Behavior == YardBehavior.Dropped)
            return;
        state.Behavior = YardBehavior.Dropped;
        state.Movement = YardMovement.Airborne;
        SetYardAirborneLists(place, state);
        state.AirborneVelocity = FixedVector<RoomPixels>.Zero;
    }

    /// <summary>Runs the complete bounce/gravity/landing loop.</summary>
    private void RunYardAirborneMovement(
        RoomEnemyPlace place,
        YardEnemyState state,
        SamusState samus,
        RoomLevelData level)
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
                LandYard(place, state, samus);
                return;
            }

            state.AirborneVelocity = state.AirborneVelocity.WithY(YardAirborneReversal.Reversed(state.AirborneVelocity.Y));
            return;
        }

        state.AirborneVelocity = state.AirborneVelocity.WithY(YardFallAcceleration.Accelerated(state.AirborneVelocity.Y));
    }

    /// <summary>Stops Yard's fall, selects normal or aggressive crawling, and restores crawler velocity from its floor-facing bits.</summary>
    private static void LandYard(RoomEnemyPlace place, YardEnemyState state, SamusState samus)
    {
        state.AirborneVelocity = FixedVector<RoomPixels>.Zero;
        state.ConsecutiveTurnCounter = 0;
        state.TurnTransitionDisableCounter = 0;
        state.TurnTransitionDisabled = true;

        if (state.Behavior == YardBehavior.Dropped)
        {
            state.Behavior = YardBehavior.NormalCrawling;
        }
        else
        {
            state.Behavior = YardBehavior.AggressiveCrawling;
            state.CrawlSpeed = YardVelocityDefinitions.AggressiveCrawlSpeed;
            MakeYardFaceSamusHorizontally(place, state, samus);
            SetYardAirborneLists(place, state);
        }

        // After landing, Yard starts crawling as a new crawler does, but keeps its own state and main AI. Only the
        // crawler's starting velocity carries over: the live crawl speed on both axes, the horizontal one reversed for
        // a block on Yard's right.
        state.BlockSide = state.AirborneFacingDirection == YardAirborneFacing.Right
            ? CrawlerSurfaceOrientation.UpsideLeft
            : CrawlerSurfaceOrientation.UpsideRight;
        // Surface direction can still describe a wall or ceiling. The landing instead uses
        // the new floor-facing block side for its downward probe and horizontal sign.
        var landing = new EightEightVector<RoomPixels>(state.CrawlSpeed, state.CrawlSpeed);
        state.CrawlingVelocity = state.BlockSide == CrawlerSurfaceOrientation.UpsideRight ? landing.ReversedX() : landing;
        place.Frame = EnemyFrameId.Empty;
        state.Movement = YardMovement.HoldingStill;
        state.Playback = EnemyAnimationPlayback.Start(place, YardAnimations.StartingWith(state.HidingAnimation ??
            throw new InvalidDataException("A landing Yard has no hiding animation to resume.")));
    }

    /// <summary>Selects visible and hiding animations from Yard's airborne facing direction.</summary>
    private static void SetYardAirborneLists(
        RoomEnemyPlace place,
        YardEnemyState state)
    {
        YardAirborneAnimationDefinition definition =
            YardDirections.ForAirborneFacing(state.AirborneFacingDirection);
        state.Playback = new(YardAnimations.StartingWith(definition.VisibleAnimation), AnimationPlace.StartOfAnimation);
        state.HidingAnimation = definition.HidingAnimation;
        place.AnimationClock.Restart();
    }

    /// <summary>Turns Yard after landing only when its airborne facing points away from Samus.</summary>
    private static bool MakeYardFaceSamusHorizontally(
        RoomEnemyPlace place,
        YardEnemyState state,
        SamusState samus)
    {
        bool shouldTurn = state.AirborneFacingDirection == YardAirborneFacing.Right
            ? place.Position.X.Whole >= samus.Position.X.Whole
            : place.Position.X.Whole < samus.Position.X.Whole;
        return shouldTurn && TurnYardAround(state);
    }

    /// <summary>Reverses Yard's crawl direction and associated graphics, properties, and velocities unless it is hidden or holding still.</summary>
    internal static bool TurnYardAround(YardEnemyState state)
    {
        if (state.Behavior == YardBehavior.Hiding ||
            state.Movement == YardMovement.HoldingStill)
        {
            return false;
        }

        state.Direction = YardDirections.ForDirection(state.Direction).OppositeDirection;
        YardDirectionDefinition definition = YardDirections.ForDirection(state.Direction);
        state.Playback = new(YardAnimations.StartingWith(definition.CrawlingAnimation), AnimationPlace.StartOfAnimation);
        state.BlockSide = definition.BlockSide;
        state.HidingAnimation = definition.HidingAnimation;
        state.AirborneFacingDirection = definition.AirborneFacingDirection;
        SetYardCrawlingVelocities(state, state.Direction);
        state.Movement = definition.Movement;
        state.TurnTransitionDisabled = true;
        state.TurnTransitionDisableCounter = 0;
        return true;
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
        state.Movement = YardMovement.Airborne;
        SetYardAirborneLists(place, state);

        SignedSixteenSixteen horizontal = kickDistance.HorizontalDistancePlusOne;
        SignedSixteenSixteen kick = YardKicks.ForHorizontalSpeed(horizontal.UnsignedWholePixels);
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
        state.Movement = YardMovement.Airborne;
        SetYardAirborneLists(place, state);
        // Only the whole pixels change; both fractions keep their previous values.
        state.AirborneVelocity = state.AirborneVelocity.WithWhole(new Offset16<RoomPixels>(
            SamusPoseMetadata.ReadFacingDirection(samus) == SamusFacingDirection.Left ? -1 : 1,
            -1));
    }

    /// <summary>Reverses one axis of the crawl velocity.</summary>
    private static SignedEightEight NegateEightEight(SignedEightEight velocity) =>
        velocity.Negated();

    /// <summary>Runs Yard's custom touch entry.</summary>
    private void ResolveYardTouch(
        RoomEnemyPlace place,
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
            state.Behavior is YardBehavior.Dropped or YardBehavior.Kicked)
        {
            return;
        }

        _collision.ResolveNormalEnemyTouch(place, samus, pseudoScrew);
        state.CrawlSpeed = state.IdleCrawlSpeed;
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
        bool yardFacesRight = state.AirborneFacingDirection == YardAirborneFacing.Right;
        return pressingExactlyRight ? !yardFacesRight : yardFacesRight;
    }

    /// <summary>
    /// Completes the non-missile branch of Yard shot AI. Collision/beam impact
    /// disposal has already happened in the shared projectile pass; this method owns only
    /// the actor's launch and observable sound event.
    /// </summary>
    private void ResolveYardBeamLaunch(
        RoomEnemyPlace place,
        YardEnemyState state,
        SamusState samus)
    {
        if (state.Behavior is not (YardBehavior.Dropped or YardBehavior.Kicked))
            ShootYardIntoAir(place, state, samus);
        _audio.QueueEnemySound(YardBounceSound, priority: SoundRequestPriority.Low);
    }

    /// <summary>Runs Yard's main AI.</summary>
    internal void RunYardMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunYardMain(place, frame.Samus, frame.Level);
    }

    /// <summary>Advances Yard's typed animation.</summary>
    internal void AdvanceYardTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
    {
        if (!place.AnimationClock.Expire())
            return;
        var state = place.RequireSpeciesState<YardEnemyState>();
        AnimationPlayback<YardSegment> playback = state.Playback;
        // Only the repeat cue continues elsewhere, and it continues in the animation already playing.
        Animation<YardSegment> playing = playback.Animation;
        EnemyAnimationPlayback.Play(place, YardAnimations.Segments, ref playback,
            cue => ApplyYardCue(place, state, cue, playing));
        state.Playback = playback;
    }

    /// <summary>Routes Yard shots through its custom launch branch before common vulnerability damage.</summary>
    internal EnemyShotReaction YardShotReaction => field ??= new()
    {
        AdmitsBombFamilies = true,
        // Yard's custom shot AI sends super-missile/power-bomb families through normal vulnerability damage,
        // but every other colliding shot merely kicks the shell into the air. This occurs after projectile
        // impact, once the collision pass has accepted the collision.
        BeforeVulnerability = (in EnemyShotContext c) =>
        {
            if (c.Family is SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb)
                return false;
            ResolveYardBeamLaunch(c.Enemy, c.Enemy.RequireSpeciesState<YardEnemyState>(), c.Samus);
            return true;
        },
    };

    /// <summary>Runs Yard's touch reaction, including its facing-dependent shell response.</summary>
    internal void ReactToYardTouch(EnemyTouchContext context) =>
        ResolveYardTouch(context.Place, context.Place.RequireSpeciesState<YardEnemyState>(), context.Samus, context.PseudoScrew, context.ControllerInput);

    /// <summary>The enemy-library sound queued by a Yard shell kick, launch or wall bounce.</summary>
    private static readonly SoundEffectId YardBounceSound =
        EnemySoundEffectDefinitions.YardBounceSound;
}
