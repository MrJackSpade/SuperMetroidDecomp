using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Programs;
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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new YardEntity(this, slot, common);

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

    /// <summary>Binds Yard's room, terrain, quake, and collision owners for its registered AI callbacks.</summary>
    internal YardEnemyBehavior(EnemyRoomBinding room, EnemyTerrain terrain, RoomEarthquake earthquake, EnemyCollision collision, EnemyAudioQueue audio)
    {
        _audio = audio;
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
        RoomEnemySlot slot, YardEnemyState state, YardCue cue, Animation<YardSegment> playing)
    {
        switch (cue)
        {
            case YardCue.SetMovementFunction setMovementFunction:
                // Yard animation owns movement dispatch: the operand is a movement behaviour, not a branch.
                state.MovementFunction = setMovementFunction.MovementFunction;
                return null;
            case YardCue.SetHidingAnimation setHidingAnimation:
                state.HidingAnimation = setHidingAnimation.OptionalTarget;
                return null;
            case YardCue.SetDirection setDirection:
                state.Direction = setDirection.Direction;
                state.AirborneFacingDirection = YardDirections.ForDirection(state.Direction).AirborneFacingDirection;
                return null;
            case YardCue.MoveBy moveBy:
                slot.Position += moveBy.Offset;
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
    internal static void InitializeYard(RoomEnemySlot slot)
    {
        YardSpawnSettings settings = slot.SpawnSettings<YardSpawnSettings>();
        YardDirection direction = settings.Direction;

        var state = new YardEnemyState
        {
            MovementFunction = YardMovementFunction.AwaitingAnimationCue,
            // The population entry selects the initial animations and velocity signs, but
            // does not set the live direction. Only the direction cue does. A
            // first-frame hide can bypass it and keeps the first direction.
            Direction = YardDirection.UpsideRightMovingUp,
            Behavior = YardBehavior.NormalCrawling,
            IdleCrawlSpeed = settings.CrawlSpeed,
            CrawlSpeed = settings.CrawlSpeed,
        };
        slot.SetSpeciesState(state);

        slot.Frame = EnemyFrameId.Empty;
        slot.AnimationClock.StepOnNextUpdate();

        YardDirectionDefinition definition = YardDirections.ForDirection(direction);
        state.Playback = new(YardAnimations.StartingWith(definition.CrawlingAnimation), AnimationPlace.StartOfAnimation);
        state.BlockSide = definition.BlockSide;
        state.HidingAnimation = definition.HidingAnimation;
        state.AirborneFacingDirection = definition.AirborneFacingDirection;
        SetYardCrawlingVelocities(state, direction);
    }

    /// <summary>
    /// Reproduces the sign-pair arithmetic: each axis takes the live crawl speed or its negation wrapped past 65535, rather
    /// than a host signed table.
    /// </summary>
    internal static void SetYardCrawlingVelocities(YardEnemyState state, YardDirection direction) =>
        state.CrawlingVelocity = YardCrawlVelocity.Apply(state.CrawlSpeed, direction);

    /// <summary>Runs the Yard's main AI.</summary>
    private void RunYardMain(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Yard AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Yard movement requires room collision data.");
        YardEnemyState state = slot.RequireSpeciesState<YardEnemyState>();

        // A super-missile earthquake drops an attached/hiding Yard, but cannot restart one
        // that is already falling, kicked, or shot through the air.
        if (state.Behavior is not (YardBehavior.Dropped or YardBehavior.Kicked or YardBehavior.BeamLaunched) &&
            _earthquake.IsShakingWith(SuperMissileDefinitions.WallImpactEarthquake, SuperMissileDefinitions.EarthquakeDuration))
        {
            DropYard(slot, state);
        }

        HandleYardHiding(slot, state, samus);
        UpdateYardSolidProperty(slot, state, samus);

        switch (state.MovementFunction)
        {
            case YardMovementFunction.AwaitingAnimationCue:
                return;
            case YardMovementFunction.Hiding:
                RunYardHidingMovement(slot, state, level);
                return;
            case YardMovementFunction.Airborne:
                RunYardAirborneMovement(slot, state, samus, level);
                return;
            // Direction-changing animation can publish the new direction one frame before
            // it installs the matching movement behaviour. The game dispatches through that behaviour,
            // not by the temporarily newer direction field.
            case YardMovementFunction.CrawlingUpsideUpMovingLeft:
            case YardMovementFunction.CrawlingUpsideLeftMovingDown:
            case YardMovementFunction.CrawlingUpsideDownMovingRight:
            case YardMovementFunction.CrawlingUpsideRightMovingUp:
            case YardMovementFunction.CrawlingUpsideUpMovingRight:
            case YardMovementFunction.CrawlingUpsideRightMovingDown:
            case YardMovementFunction.CrawlingUpsideDownMovingLeft:
            case YardMovementFunction.CrawlingUpsideLeftMovingUp:
                RunYardCrawlingMovement(slot, state, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Yard function {state.MovementFunction?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Runs the look-at/hide test.</summary>
    private static void HandleYardHiding(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        if (state.Behavior is YardBehavior.AggressiveCrawling or YardBehavior.Dropped or YardBehavior.Kicked or YardBehavior.BeamLaunched)
            return;

        bool withinVerticalBand = CoordinateAxis.Difference(slot.Position.Y.Whole, samus.Position.Y.Whole) >= -96;
        SamusFacingDirection samusFacing = SamusPoseMetadata.ReadFacingDirection(samus);
        bool samusLookingAtYard = slot.Position.X.Whole < samus.Position.X.Whole
            ? samusFacing == SamusFacingDirection.Left
            : samusFacing == SamusFacingDirection.Right;
        if (withinVerticalBand && samusLookingAtYard)
        {
            if (state.Behavior == YardBehavior.Hiding)
                return;
            if (state.HidingAnimation is { } hiding)
            {
                state.Playback = new(YardAnimations.StartingWith(hiding), AnimationPlace.StartOfAnimation);
                slot.Frame = EnemyFrameId.Empty;
                slot.AnimationClock.Restart();
                state.Behavior = YardBehavior.Hiding;
                return;
            }
        }

        state.Behavior = YardBehavior.NormalCrawling;
    }

    /// <summary>Runs Yard's dynamic solid-hitbox decision.</summary>
    private static void UpdateYardSolidProperty(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        bool solid;
        if (state.MovementFunction == YardMovementFunction.Airborne)
        {
            solid = false;
        }
        else if (state.Behavior == YardBehavior.AggressiveCrawling)
        {
            solid = !samus.HorizontalSpeed.HasRunningMomentum &&
                state.MovementFunction == YardMovementFunction.AwaitingAnimationCue;
        }
        else if (samus.HorizontalSpeed.HasRunningMomentum)
        {
            solid = false;
        }
        else
        {
            solid = state.Behavior is not (YardBehavior.Dropped or YardBehavior.BeamLaunched);
        }

        slot.Contact = solid
            ? slot.Contact.With(EnemyContact.SolidToSamus)
            : slot.Contact.Without(EnemyContact.SolidToSamus);
    }

    /// <summary>Runs the hidden-animation movement owner.</summary>
    private static void RunYardHidingMovement(
        RoomEnemySlot slot,
        YardEnemyState state,
        RoomLevelData level)
    {
        bool stillAttached = YardDirections.IsOnWall(state.Direction)
            ? EnemyTerrain.EnemyMeetsAttachmentBlockHorizontallyAhead(
                level,
                slot,
                CrawlersBehavior.Shift8AddMagnitude(state.CrawlingVelocity.X, 7))
            : EnemyTerrain.EnemyMeetsAttachmentBlockVerticallyAhead(
                level,
                slot,
                CrawlersBehavior.Shift8AddMagnitude(state.CrawlingVelocity.Y, 7));
        if (!stillAttached)
            DropYard(slot, state);
    }

    /// <summary>Moves Yard along its wall or floor, probes the turn lookahead, and detaches if the supporting surface ends.</summary>
    internal void RunYardCrawlingMovement(
        RoomEnemySlot slot,
        YardEnemyState state,
        RoomLevelData level)
    {
        bool movesVertically = state.MovementFunction is
            YardMovementFunction.CrawlingUpsideLeftMovingDown or
            YardMovementFunction.CrawlingUpsideRightMovingUp or
            YardMovementFunction.CrawlingUpsideRightMovingDown or
            YardMovementFunction.CrawlingUpsideLeftMovingUp;
        YardTurnDefinition turn = YardTurns.ForMovement(
            state.MovementFunction ?? throw new InvalidOperationException("A turning Yard has no movement function."),
            state.TurnTransitionDisabled);

        // The lookahead offset is temporary, but the normal-axis probe movement is not.
        // Adds the offset, performs collision/alignment, then subtracts only the
        // original offset from the resulting center.
        slot.Position += turn.Lookahead;
        bool surfaceCollision = movesVertically
            ? _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                CrawlersBehavior.Shift8AddMagnitude(state.CrawlingVelocity.X, 1))
            : _terrain.MoveEnemyVertically(
                level,
                slot,
                CrawlersBehavior.Shift8AddMagnitude(state.CrawlingVelocity.Y, 1));
        slot.Position -= turn.Lookahead;

        if (!surfaceCollision)
        {
            // The count stays within one through four, so the game's signed comparison is an ordinary one.
            state.ConsecutiveTurnCounter++;
            if (state.ConsecutiveTurnCounter >= YardEnemyState.MissedProbesBeforeFalling)
            {
                DropYard(slot, state);
                return;
            }

            if (movesVertically)
                state.CrawlingVelocity = state.CrawlingVelocity.WithY(NegateEightEight(state.CrawlingVelocity.Y));
            else
                state.CrawlingVelocity = state.CrawlingVelocity.WithX(NegateEightEight(state.CrawlingVelocity.X));
            SetYardAnimationAndDisableTurn(
                slot,
                state,
                turn.OutsideTurnAnimation);
            return;
        }

        state.ConsecutiveTurnCounter = 0;
        if (movesVertically)
        {
            bool slopeAdjusted = EnemyTerrain.AlignEnemyYWithNonSquareSlopeAndReportAdjustment(level, slot);
            HandleYardTurnTransitionDisabling(state, slopeAdjusted);
            SignedSixteenSixteen tangent = state.CrawlingVelocity.Y.ToSixteenSixteenDelta();
            if (_terrain.MoveEnemyVertically(level, slot, tangent))
            {
                state.CrawlingVelocity = state.CrawlingVelocity.WithX(NegateEightEight(state.CrawlingVelocity.X));
                SetYardAnimationAndDisableTurn(
                    slot,
                    state,
                    turn.InsideTurnAnimation);
            }
            return;
        }

        SignedSixteenSixteen horizontalTangent = CrawlersBehavior.GetSurfaceSlopeAdjustedHorizontalDisplacement(
            slot,
            state.CrawlingVelocity,
            level);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalTangent))
        {
            state.CrawlingVelocity = state.CrawlingVelocity.WithY(NegateEightEight(state.CrawlingVelocity.Y));
            SetYardAnimationAndDisableTurn(
                slot,
                state,
                turn.InsideTurnAnimation);
            return;
        }

        bool adjusted = EnemyTerrain.AlignEnemyYWithNonSquareSlopeAndReportAdjustment(level, slot);
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
        if (next >= 16)
        {
            state.TurnTransitionDisabled = false;
            return;
        }
        state.TurnTransitionDisableCounter = next;
    }

    /// <summary>Starts a turn animation immediately and resets the temporary turn suppression counter.</summary>
    private static void SetYardAnimationAndDisableTurn(
        RoomEnemySlot slot,
        YardEnemyState state,
        YardSegment segment)
    {
        state.Playback = new(YardAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.StepOnNextUpdate();
        state.TurnTransitionDisabled = true;
        state.TurnTransitionDisableCounter = 0;
    }

    /// <summary>Runs the common detach path.</summary>
    internal static void DropYard(RoomEnemySlot slot, YardEnemyState state)
    {
        if (state.Behavior == YardBehavior.Dropped)
            return;
        state.Behavior = YardBehavior.Dropped;
        state.MovementFunction = YardMovementFunction.Airborne;
        SetYardAirborneLists(slot, state);
        state.AirborneVelocity = FixedVector<RoomPixels>.Zero;
    }

    /// <summary>Runs the complete bounce/gravity/landing loop.</summary>
    private void RunYardAirborneMovement(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (state.Behavior != YardBehavior.Dropped)
        {
            if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, state.AirborneVelocity.X))
            {
                state.AirborneVelocity = state.AirborneVelocity.WithX(YardAirborneReversal.Reversed(state.AirborneVelocity.X));
                state.BouncedHorizontally = true;
                _audio.QueueEnemySound(YardBounceSound, priority: SoundRequestPriority.Low);
            }
            else
            {
                state.AirborneVelocity = state.AirborneVelocity.WithX(YardAirborneSlowing.Slowed(state.AirborneVelocity.X));
            }
        }

        if (_terrain.MoveEnemyVertically(level, slot, state.AirborneVelocity.Y))
        {
            int wholeY = state.AirborneVelocity.Y.WholePixels;
            if (wholeY is >= 0 and < 3)
            {
                LandYard(slot, state, samus);
                return;
            }

            state.AirborneVelocity = state.AirborneVelocity.WithY(YardAirborneReversal.Reversed(state.AirborneVelocity.Y));
            state.BouncedHorizontally = false;
            return;
        }

        state.AirborneVelocity = state.AirborneVelocity.WithY(YardFallAcceleration.Accelerated(state.AirborneVelocity.Y));
    }

    /// <summary>Stops Yard's fall, selects normal or aggressive crawling, and restores crawler velocity from its floor-facing bits.</summary>
    private static void LandYard(RoomEnemySlot slot, YardEnemyState state, SamusState samus)
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
            MakeYardFaceSamusHorizontally(slot, state, samus);
            SetYardAirborneLists(slot, state);
        }

        // Yard deliberately tail-calls the common crawler initializer after landing, but
        // retains its own state and main AI. Reproduce only its velocity: the live crawl
        // speed on both axes, the horizontal one reversed for a block on Yard's right.
        state.BlockSide = state.AirborneFacingDirection == YardAirborneFacing.Right
            ? CrawlerSurfaceOrientation.UpsideLeft
            : CrawlerSurfaceOrientation.UpsideRight;
        // Surface direction can still describe a wall or ceiling. The landing instead uses
        // the new floor-facing block side for its downward probe and horizontal sign.
        var landing = new EightEightVector<RoomPixels>(state.CrawlSpeed, state.CrawlSpeed);
        state.CrawlingVelocity = state.BlockSide == CrawlerSurfaceOrientation.UpsideRight ? landing.ReversedX() : landing;
        slot.Frame = EnemyFrameId.Empty;
        state.MovementFunction = YardMovementFunction.AwaitingAnimationCue;
        state.Playback = EnemyAnimationPlayback.Start(slot, YardAnimations.StartingWith(state.HidingAnimation ??
            throw new InvalidDataException("A landing Yard has no hiding animation to resume.")));
    }

    /// <summary>Selects visible and hiding animations from Yard's airborne facing direction.</summary>
    private static void SetYardAirborneLists(
        RoomEnemySlot slot,
        YardEnemyState state)
    {
        YardAirborneAnimationDefinition definition =
            YardDirections.ForAirborneFacing(state.AirborneFacingDirection);
        state.Playback = new(YardAnimations.StartingWith(definition.VisibleAnimation), AnimationPlace.StartOfAnimation);
        state.HidingAnimation = definition.HidingAnimation;
        slot.AnimationClock.Restart();
    }

    /// <summary>Turns Yard after landing only when its airborne facing points away from Samus.</summary>
    private static bool MakeYardFaceSamusHorizontally(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        bool shouldTurn = state.AirborneFacingDirection == YardAirborneFacing.Right
            ? slot.Position.X.Whole >= samus.Position.X.Whole
            : slot.Position.X.Whole < samus.Position.X.Whole;
        return shouldTurn && TurnYardAround(state);
    }

    /// <summary>Reverses Yard's crawl direction and associated graphics, properties, and velocities unless it is hidden or awaiting its animation.</summary>
    internal static bool TurnYardAround(YardEnemyState state)
    {
        if (state.Behavior == YardBehavior.Hiding ||
            state.MovementFunction == YardMovementFunction.AwaitingAnimationCue)
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
        state.MovementFunction = definition.MovementFunction;
        state.TurnTransitionDisabled = true;
        state.TurnTransitionDisableCounter = 0;
        return true;
    }

    /// <summary>Runs the running/airborne kick setup.</summary>
    internal static void KickYardIntoAir(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        state.Behavior = YardBehavior.Kicked;
        state.MovementFunction = YardMovementFunction.Airborne;
        SetYardAirborneLists(slot, state);

        SignedSixteenSixteen horizontal = samus.HorizontalDistanceMovedPlusOne;
        SignedSixteenSixteen kick = YardKicks.ForHorizontalSpeed(horizontal.UnsignedWholePixels);
        if (SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left))
            horizontal = YardAirborneReversal.Reversed(horizontal);
        state.AirborneVelocity = new FixedVector<RoomPixels>(
            horizontal,
            kick);
    }

    /// <summary>Runs ordinary-beam launch setup.</summary>
    internal static void ShootYardIntoAir(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        state.Behavior = YardBehavior.BeamLaunched;
        state.MovementFunction = YardMovementFunction.Airborne;
        SetYardAirborneLists(slot, state);
        // Only the whole pixels are stored; both fractions keep their previous values.
        state.AirborneVelocity = state.AirborneVelocity.WithWhole(new Offset16<RoomPixels>(
            SamusPoseMetadata.ReadFacingDirection(samus) == SamusFacingDirection.Left ? -1 : 1,
            -1));
    }

    /// <summary>Negates one raw 8.8 crawl velocity, wrapping in its signed range.</summary>
    private static SignedEightEight NegateEightEight(SignedEightEight velocity) =>
        velocity.Negated();

    /// <summary>Runs Yard's custom touch entry.</summary>
    private void ResolveYardTouch(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus,
        ControllerButton controllerInput)
    {
        bool aggressiveCrawlRejectsKick = state.Behavior == YardBehavior.AggressiveCrawling &&
            state.MovementFunction != YardMovementFunction.AwaitingAnimationCue &&
            SamusIsDirectingTowardYard(state, controllerInput);
        bool kickRequested = !aggressiveCrawlRejectsKick &&
            (state.MovementFunction == YardMovementFunction.Airborne ||
             samus.HorizontalSpeed.HasRunningMomentum);

        if (kickRequested)
        {
            KickYardIntoAir(slot, state, samus);
            _audio.QueueEnemySound(YardBounceSound, priority: SoundRequestPriority.Low);
            return;
        }

        // A hidden shell awaiting its animation, a falling shell, and an already kicked shell
        // deliberately do nothing on overlap. Every other route enters normal enemy touch,
        // restores the population's idle speed, optionally turns, then resumes behavior zero.
        if (state.MovementFunction == YardMovementFunction.AwaitingAnimationCue ||
            state.Behavior is YardBehavior.Dropped or YardBehavior.Kicked)
        {
            return;
        }

        _collision.ResolveNormalEnemyTouch(slot, samus);
        state.CrawlSpeed = state.IdleCrawlSpeed;
        if (state.Behavior != YardBehavior.NormalCrawling)
            TurnYardAround(state);
        state.Behavior = YardBehavior.NormalCrawling;
    }

    /// <summary>Reads the exact held horizontal direction used by Yard's touch callback.</summary>
    private static bool SamusIsDirectingTowardYard(
        YardEnemyState state,
        ControllerButton controllerInput)
    {
        ControllerButton heldButtons = controllerInput;
        bool pressingExactlyRight =
            (heldButtons & ControllerButtons.HorizontalDirections) == ControllerButton.Right;
        bool yardFacesRight = state.AirborneFacingDirection == YardAirborneFacing.Right;
        return pressingExactlyRight ? !yardFacesRight : yardFacesRight;
    }

    /// <summary>
    /// Completes the non-missile branch of Yard shot AI. Collision/beam impact
    /// disposal has already happened in the shared projectile pass; this method owns only
    /// the actor's launch and observable sound event.
    /// </summary>
    private void ResolveYardBeamLaunch(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        if (state.Behavior is not (YardBehavior.Dropped or YardBehavior.Kicked))
            ShootYardIntoAir(slot, state, samus);
        _audio.QueueEnemySound(YardBounceSound, priority: SoundRequestPriority.Low);
    }

    /// <summary>Runs Yard's main AI.</summary>
    internal void RunYardMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunYardMain(slot, frame.Samus, frame.Level);
    }

    /// <summary>Advances Yard's typed animation.</summary>
    internal void AdvanceYardTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        if (!slot.AnimationClock.Expire())
            return;
        var state = slot.RequireSpeciesState<YardEnemyState>();
        AnimationPlayback<YardSegment> playback = state.Playback;
        // Only the repeat cue continues elsewhere, and it continues in the animation already playing.
        Animation<YardSegment> playing = playback.Animation;
        EnemyAnimationPlayback.Play(slot, YardAnimations.Segments, ref playback,
            cue => ApplyYardCue(slot, state, cue, playing));
        state.Playback = playback;
    }

    /// <summary>Routes Yard shots through its custom launch branch before common vulnerability damage.</summary>
    internal void AddYardShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Yard &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.YardShot)
        {
            AdmitsBombFamilies = true,
            // Yard's custom shot AI sends super-missile/power-bomb families through normal vulnerability damage,
            // but every other colliding shot merely kicks the shell into the air. This occurs after projectile
            // impact, exactly where the collision pass has already accepted the collision index.
            BeforeVulnerability = (in EnemyShotContext c) =>
            {
                if (c.Family is SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb)
                    return false;
                if (c.Samus is null)
                    throw new InvalidOperationException("Yard beam launch requires the active Samus actor.");
                ResolveYardBeamLaunch(c.Enemy, c.Enemy.RequireSpeciesState<YardEnemyState>(), c.Samus);
                return true;
            },
        });
    }

    /// <summary>Registers Yard's touch callback, including its facing-dependent shell response.</summary>
    internal void AddYardTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Yard &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.YardTouch,
            context => ResolveYardTouch(context.Slot, context.Slot.RequireSpeciesState<YardEnemyState>(), context.Samus, context.ControllerInput));
    }

    /// <summary>The enemy-library sound queued by a Yard shell kick, launch or wall bounce.</summary>
    private static readonly SoundEffectId YardBounceSound =
        EnemySoundEffectDefinitions.YardBounceSound;
}
