using SuperMetroid.Core.Input;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>The Beetom species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class BeetomBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Beetom];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new BeetomEntity(this, place, common);

    /// <summary>Supplies the shared RNG seed and generation used by Beetom action choice.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves Beetom ledge, wall, and landing collisions.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Queues sounds produced during Beetom movement and contact.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>The current enemy pass's clocks and flags.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Handles common contact before Beetom's attachment behavior.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Hands each new Beetom the previous Beetom in its place, whose facing it inherits.</summary>
    private readonly PlaceInheritance<BeetomEnemyState> _inheritance;

    /// <summary>Binds room, terrain, audio, collision and place-inheritance services for Beetom AI.</summary>
    internal BeetomBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyAudioQueue audio, EnemyFrameContext frameContext,
        EnemyCollision collision, PlaceInheritance<BeetomEnemyState> inheritance)
    {
        _room = room;
        _terrain = terrain;
        _audio = audio;
        _frameContext = frameContext;
        _collision = collision;
        _inheritance = inheritance;
    }

    /// <summary>Carries out one cue of the Beetom's animations: whether it keeps processing off screen.</summary>
    private static BeetomSegment? ApplyBeetomCue(RoomEnemyPlace place, BeetomCue cue)
    {
        switch (cue.Kind)
        {
            case BeetomCueKind.ProcessOffScreen:
                place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
                break;
            case BeetomCueKind.StopProcessingOffScreen:
                place.Processing = place.Processing.Without(EnemyProcessing.OffScreen);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BeetomCueKind)} {cue.Kind}.");
        }
        return null;
    }


    /// <summary>Initializes the Beetom.</summary>
    private void InitializeBeetom(RoomEnemyPlace place, SamusState samus, GameplayAction controllerInput)
    {
        BeetomEnemyState state = _inheritance.Inherit(place.Ordinal, static previous => new BeetomEnemyState(previous, BeetomAction.DecideAction));
        place.SetSpeciesState(state);

        state.ActionTimer = UpdateCountdown.Ended();
        state.AttachedToSamus = false;
        state.ButtonCounter = BeetomMotionDefinitions.MashCount;
        state.LastSeenPlayerActions = controllerInput;

        _room.Random.Reseed(SpeciesRandomReseed.BeetomPlacement);

        // Samus's side picks the facing: a negative Samus-minus-enemy delta
        // selects the left-facing animation; nonnegative selects the right-facing animation.
        SetBeetomAnimation(
            place,
            state,
            samus.Position.X.Whole < place.Position.X.Whole
                ? BeetomSegment.CrawlingLeft
                : BeetomSegment.CrawlingRight);
    }

    /// <summary>Runs a Beetom for one update through all of its actions.</summary>
    private void RunBeetomMain(
        RoomEnemyPlace place,
        BeetomEnemyState state,
        SamusState samus,
        RoomLevelData level,
        GameplayAction controllerInput)
    {
        switch (state.Action)
        {
            case BeetomAction.DecideAction:
                state.Action = Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) < BeetomMotionDefinitions.ProximityDistance
                    ? BeetomAction.DecideActionSamusInProximity
                    : BeetomAction.DecideActionSamusNotInProximity;
                return;
            case BeetomAction.DecideActionSamusNotInProximity:
                ChooseDistantBeetomAction(state);
                return;
            case BeetomAction.StartIdling:
                state.ActionTimer = UpdateCountdown.For(BeetomMotionDefinitions.IdleDuration);
                state.Action = BeetomAction.Idling;
                return;
            case BeetomAction.StartCrawling:
                StartBeetomCrawling(place, state, state.Heading);
                return;
            case BeetomAction.StartShortHop:
                StartBeetomHop(place, state, BeetomLeapDefinitions.ShortLeap.StartPlace, state.Heading, BeetomHopLength.Short);
                return;
            case BeetomAction.StartLongHop:
                StartBeetomHop(place, state, BeetomLeapDefinitions.LongLeap.StartPlace, state.Heading, BeetomHopLength.Long);
                return;
            case BeetomAction.DecideActionSamusInProximity:
                StartBeetomLunge(place, state, samus);
                return;
            case BeetomAction.StartDraining:
                StartBeetomDrain(place, state, side: state.Heading);
                return;
            case BeetomAction.StartDropping:
                StartBeetomDrop(place, state);
                return;
            case BeetomAction.StartBeingFlung:
                state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
                state.Action = BeetomAction.BeingFlung;
                return;
            case BeetomAction.Idling:
                if (state.ActionTimer.Step())
                    state.Action = BeetomAction.DecideAction;
                return;
            case BeetomAction.Crawling:
                RunBeetomCrawl(place, state, level, state.Heading);
                return;
            case BeetomAction.ShortHopRising:
                RiseBeetom(place, state, level, BeetomMotionDefinitions.ShortHopMarks, fall: BeetomAction.ShortHopFalling);
                MoveBeetomAcrossArc(place, state, level, state.Heading, BeetomArcKind.Hop);
                return;
            case BeetomAction.ShortHopFalling:
                FallBeetom(place, state, level, BeetomMotionDefinitions.ShortHopMarks);
                MoveBeetomAcrossArc(place, state, level, state.Heading, BeetomArcKind.Hop);
                return;
            case BeetomAction.LongHopRising:
                RiseBeetom(place, state, level, BeetomMotionDefinitions.LongHopMarks, fall: BeetomAction.LongHopFalling);
                MoveBeetomAcrossArc(place, state, level, state.Heading, BeetomArcKind.Hop);
                return;
            case BeetomAction.LongHopFalling:
                FallBeetom(place, state, level, BeetomMotionDefinitions.LongHopMarks);
                MoveBeetomAcrossArc(place, state, level, state.Heading, BeetomArcKind.Hop);
                return;
            case BeetomAction.LungeRising:
                RiseBeetom(place, state, level, BeetomMotionDefinitions.LungeRisingMarks, fall: BeetomAction.LungeFalling);
                MoveBeetomAcrossArc(place, state, level, state.Heading, BeetomArcKind.Lunge);
                return;
            case BeetomAction.LungeFalling:
                FallBeetom(place, state, level, BeetomMotionDefinitions.LungeFallingMarks);
                MoveBeetomAcrossArc(place, state, level, state.Heading, BeetomArcKind.Lunge);
                return;
            case BeetomAction.Draining:
                RunBeetomDrain(place, state, samus, level, controllerInput, side: state.Heading);
                return;
            case BeetomAction.Dropping:
                RunBeetomDrop(place, state, level);
                return;
            case BeetomAction.BeingFlung:
                RunFlungBeetom(place, state, level);
                return;
            default:
                throw new InvalidDataException($"Beetom has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>
    /// Uses one random sample to choose to idle or hop (<see cref="BeetomDecisions.DistantChoice"/>, #2369) and which way to
    /// face.
    /// </summary>
    private void ChooseDistantBeetomAction(BeetomEnemyState state)
    {
        int random = _room.Random.Generate();
        BeetomDistantChoice choice = BeetomDecisions.DistantChoice.Choose(random);
        if (choice.Heading is { } heading)
            state.Begin(choice.Action, heading);
        else
            state.Action = choice.Action;
        state.Facing = BeetomDecisions.FacesRight.IsTakenBy(random) ? HorizontalDirection.Right : HorizontalDirection.Left;
    }

    /// <summary>Starts the crawl toward <paramref name="direction"/> and its animation.</summary>
    private static void StartBeetomCrawling(RoomEnemyPlace place, BeetomEnemyState state, HorizontalDirection direction)
    {
        state.Begin(BeetomAction.Crawling, direction);
        SetBeetomAnimation(
            place,
            state,
            direction == HorizontalDirection.Left
                ? BeetomSegment.CrawlingLeft
                : BeetomSegment.CrawlingRight);
    }

    /// <summary>Arms a short or long arc from its precomputed initial speed-curve place.</summary>
    private static void StartBeetomHop(
        RoomEnemyPlace place,
        BeetomEnemyState state,
        QuadraticSpeedCurvePlace initialYSpeedCurvePlace,
        HorizontalDirection direction,
        BeetomHopLength length)
    {
        state.YSpeedCurvePlace = initialYSpeedCurvePlace;
        state.Begin(length switch
        {
            BeetomHopLength.Short => BeetomAction.ShortHopRising,
            BeetomHopLength.Long => BeetomAction.LongHopRising,
            _ => throw new ArgumentOutOfRangeException(nameof(length), length, "Unknown Beetom hop."),
        }, direction);
        SetBeetomAnimation(
            place,
            state,
            direction == HorizontalDirection.Left
                ? BeetomSegment.HopLeft
                : BeetomSegment.HopRight);
    }

    /// <summary>Aims the lunge toward Samus using the signed horizontal difference.</summary>
    private static void StartBeetomLunge(RoomEnemyPlace place, BeetomEnemyState state, SamusState samus)
    {
        HorizontalDirection towardSamus = samus.Position.X.Whole < place.Position.X.Whole
            ? HorizontalDirection.Left
            : HorizontalDirection.Right;
        state.YSpeedCurvePlace = BeetomLeapDefinitions.Lunge.StartPlace;
        state.Facing = towardSamus;
        state.Begin(BeetomAction.LungeRising, towardSamus);
        SetBeetomAnimation(
            place,
            state,
            towardSamus == HorizontalDirection.Left
                ? BeetomSegment.HopLeft
                : BeetomSegment.HopRight);
    }

    /// <summary>Starts the attached blood-drain animation for the current facing.</summary>
    private static void StartBeetomDrain(RoomEnemyPlace place, BeetomEnemyState state, HorizontalDirection side)
    {
        SetBeetomAnimation(
            place,
            state,
            side == HorizontalDirection.Left
                ? BeetomSegment.DrainingLeft
                : BeetomSegment.DrainingRight);
        state.Begin(BeetomAction.Draining, side);
    }

    /// <summary>Switches back to crawl artwork before dropping toward the floor.</summary>
    private static void StartBeetomDrop(RoomEnemyPlace place, BeetomEnemyState state)
    {
        SetBeetomAnimation(
            place,
            state,
            state.Facing == HorizontalDirection.Right
                ? BeetomSegment.CrawlingRight
                : BeetomSegment.CrawlingLeft);
        state.Action = BeetomAction.Dropping;
    }

    /// <summary>Probes eight pixels ahead for a ledge, then crawls or reverses at terrain.</summary>
    private void RunBeetomCrawl(RoomEnemyPlace place, BeetomEnemyState state, RoomLevelData level, HorizontalDirection direction)
    {
        if (state.ActionTimer.Step())
        {
            state.ActionTimer = UpdateCountdown.For(BeetomMotionDefinitions.CrawlDuration);
            state.Action = BeetomAction.DecideAction;
            return;
        }

        // The ledge probe temporarily moves the center ahead, then performs a real downward
        // collision move. Restore only the whole pixels: where the collision left it within a pixel stays, just as
        // the game does.
        int reach = direction == HorizontalDirection.Left ? -BeetomMotionDefinitions.LedgeProbeReach : BeetomMotionDefinitions.LedgeProbeReach;
        place.Position = place.Position.WithWholeX(place.Position.X.Whole + reach);
        if (!_terrain.MoveEnemyVertically(level, place, SignedSixteenSixteen.FromWholePixels(BeetomMotionDefinitions.LedgeProbeDrop)))
        {
            state.Begin(BeetomAction.StartCrawling, direction.Reversed());
            place.Position = place.Position.WithWholeY(place.Position.Y.Whole - BeetomMotionDefinitions.LedgeProbeDrop);
            place.Position = place.Position.WithWholeX(place.Position.X.Whole - reach);
            return;
        }

        place.Position = place.Position.WithWholeX(place.Position.X.Whole - reach);
        SignedSixteenSixteen displacement = direction == HorizontalDirection.Left ? -BeetomMotionDefinitions.CrawlSpeed : BeetomMotionDefinitions.CrawlSpeed;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
            state.Begin(BeetomAction.StartCrawling, direction.Reversed());
    }

    /// <summary>
    /// Moves an arc's Beetom toward <paramref name="direction"/> after its rise or fall, even on the update that rise or fall
    /// changed its action, and drops it at a wall.
    /// </summary>
    private void MoveBeetomAcrossArc(
        RoomEnemyPlace place,
        BeetomEnemyState state,
        RoomLevelData level,
        HorizontalDirection direction,
        BeetomArcKind arc)
    {
        // The horizontal half still runs on the exact frame that vertical collision changes
        // the AI action. That ordering is easy to lose when expressing the two axes
        // as a single combined movement vector.
        SignedSixteenSixteen displacement = arc == BeetomArcKind.Lunge
            ? (direction == HorizontalDirection.Left ? -BeetomMotionDefinitions.LungeSpeed : BeetomMotionDefinitions.LungeSpeed)
            : (direction == HorizontalDirection.Left ? -BeetomMotionDefinitions.HopSpeed : BeetomMotionDefinitions.HopSpeed);
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
            return;

        state.Facing = state.Facing.Reversed();
        state.Action = BeetomAction.StartDropping;
    }

    /// <summary>
    /// Raises an arc's Beetom by its place along the speed curve, dropping it at a ceiling; passing the apex moves it on to
    /// the arc's <paramref name="fall"/> (#2531).
    /// </summary>
    /// <param name="place">The Beetom's place.</param>
    /// <param name="state">The Beetom.</param>
    /// <param name="level">The room's level data.</param>
    /// <param name="marks">The arc's rising step along the speed curve.</param>
    /// <param name="fall">The arc's falling half.</param>
    private void RiseBeetom(RoomEnemyPlace place, BeetomEnemyState state, RoomLevelData level, int marks, BeetomAction fall)
    {
        if (_terrain.MoveEnemyVertically(level, place, state.YSpeedCurvePlace.Speed.Negative))
        {
            state.Action = BeetomAction.StartDropping;
            return;
        }

        if (state.StepRisingSpeed(marks))
            state.Action = fall;
    }

    /// <summary>Lowers an arc's Beetom by its place along the speed curve; landing ends the arc and asks for a new action.</summary>
    /// <param name="place">The Beetom's place.</param>
    /// <param name="state">The Beetom.</param>
    /// <param name="level">The room's level data.</param>
    /// <param name="marks">The arc's falling step along the speed curve.</param>
    private void FallBeetom(RoomEnemyPlace place, BeetomEnemyState state, RoomLevelData level, int marks)
    {
        if (_terrain.MoveEnemyVertically(level, place, state.YSpeedCurvePlace.Speed.Positive))
        {
            state.Action = BeetomAction.DecideAction;
            return;
        }

        state.StepFallingSpeed(marks);
    }

    /// <summary>Follows Samus until enough input changes occur, then throws Beetom clear of terrain.</summary>
    private void RunBeetomDrain(
        RoomEnemyPlace place,
        BeetomEnemyState state,
        SamusState samus,
        RoomLevelData level,
        GameplayAction controllerInput,
        HorizontalDirection side)
    {
        if (state.ButtonCounter != 0)
        {
            place.Position = place.Position.WithWhole(new(samus.Position.X.Whole, samus.Position.Y.Whole - BeetomMotionDefinitions.DrainHoldAbove));
            if (controllerInput != state.LastSeenPlayerActions)
            {
                state.LastSeenPlayerActions = controllerInput;
                state.ButtonCounter--;
            }
            return;
        }

        // Escape throws the Beetom away. A wall reverses the direction and applies the rebound,
        // producing the net throw to the other side rather than merely cancelling the first displacement.
        SignedSixteenSixteen initialDisplacement = SignedSixteenSixteen.FromWholePixels(side == HorizontalDirection.Left ? BeetomMotionDefinitions.EscapeThrow : -BeetomMotionDefinitions.EscapeThrow);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, initialDisplacement))
        {
            state.Facing = side.Reversed();
            SignedSixteenSixteen reflectedDisplacement = SignedSixteenSixteen.FromWholePixels(side == HorizontalDirection.Left ? -BeetomMotionDefinitions.EscapeRebound : BeetomMotionDefinitions.EscapeRebound);
            _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, reflectedDisplacement);
        }
        state.AttachedToSamus = false;
        state.Action = BeetomAction.StartBeingFlung;
    }

    /// <summary>Falls three pixels per frame and resumes crawling on floor contact.</summary>
    private void RunBeetomDrop(RoomEnemyPlace place, BeetomEnemyState state, RoomLevelData level)
    {
        if (!_terrain.MoveEnemyVertically(level, place, BeetomMotionDefinitions.DropSpeed))
            return;
        state.Begin(BeetomAction.StartCrawling, state.Facing);
    }

    /// <summary>Moves opposite the pre-escape facing until a floor or wall selects its next action.</summary>
    private void RunFlungBeetom(RoomEnemyPlace place, BeetomEnemyState state, RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(level, place, state.YSpeedCurvePlace.Speed.Positive))
            state.Action = BeetomAction.DecideAction;
        else
            state.StepFallingSpeed(BeetomMotionDefinitions.FlungFallingMarks);

        // The facing is the pre-escape facing. The throw deliberately travels in the
        // opposite direction: facing left moves right and facing right moves left.
        SignedSixteenSixteen horizontal = state.Facing == HorizontalDirection.Right ? -BeetomMotionDefinitions.FlungSpeed : BeetomMotionDefinitions.FlungSpeed;
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, horizontal))
            return;
        state.Facing = state.Facing.Reversed();
        state.Action = BeetomAction.StartDropping;
    }

    /// <summary>Runs Beetom's own touch response.</summary>
    private void ResolveBeetomTouch(
        RoomEnemyPlace place,
        BeetomEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        if (!state.AttachedToSamus)
        {
            state.Begin(BeetomAction.StartDraining, state.Facing);
            state.AttachedToSamus = true;
            state.ButtonCounter = BeetomMotionDefinitions.MashCount;
        }

        if (samus.HorizontalSpeed.BodyAttack != SamusBodyAttack.None)
        {
            _collision.ResolveNormalEnemyTouch(place, samus, pseudoScrew);
            samus.ClearHurtTimers();
            return;
        }

        if (BeetomCadenceDefinitions.DrainSoundEveryEighthEnemyPass.IsDue(_frameContext.EnemyPassCount) && samus.Health >= BeetomCadenceDefinitions.DrainSoundLeastHealth)
            _audio.QueueEnemySound(RoomSound.IncrementalEnergy, priority: SoundRequestPriority.Normal);

        // Unlike ordinary touch damage, the drain every 64th update hurts Samus by her suit-scaled damage and then
        // cancels the invincibility and knockback that touch gives her.
        if (!BeetomCadenceDefinitions.DrainDamageEverySixtyFourthUpdate.IsDue(place.FrameCounter))
            return;
        SamusSuitDamage.Hurt(samus, place.Definition.Damage);
        samus.ClearHurtTimers();
    }

    /// <summary>
    /// Finishes Beetom's reaction to a shot, after its ordinary damage: a surviving Beetom frozen while draining drops,
    /// and lets go of Samus. A hit that killed the Beetom took it from its place with its state, and the place's next
    /// Beetom starts detached, so there is nothing left to change.
    /// </summary>
    private static void ResolveBeetomShotAfterCommon(RoomEnemyPlace place)
    {
        if (place.Health == 0)
            return;
        BeetomEnemyState state = place.RequireSpeciesState<BeetomEnemyState>();
        if (place.Freeze.IsRunning && state.Action == BeetomAction.Draining)
        {
            state.Action = BeetomAction.StartDropping;
        }
        state.AttachedToSamus = false;
    }

    /// <summary>Installs a Beetom animation and primes its animation timer.</summary>
    private static void SetBeetomAnimation(
        RoomEnemyPlace place,
        BeetomEnemyState state,
        BeetomSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, BeetomAnimations.StartingWith(segment));
    }

    /// <summary>Beetom's reaction to a bomb: after the common damage, it lets go of Samus.</summary>
    /// <remarks>Common damage can kill the Beetom before it lets go, so the identity is matched before the
    /// reaction runs.</remarks>
    internal static EnemyBombReaction BeetomBombReaction { get; } = new()
    {
        AfterCommon = c => ResolveBeetomShotAfterCommon(c.Enemy),
    };

    /// <summary>Runs Beetom's initialization AI.</summary>
    internal void RunBeetomInitializationAi(RoomEnemyPlace place, EnemyPopulationSurroundings surroundings)
    {
        InitializeBeetom(place, surroundings.Samus, surroundings.ControllerInput);
    }

    /// <summary>Runs Beetom's main AI.</summary>
    internal void RunBeetomMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunBeetomMain(place, place.RequireSpeciesState<BeetomEnemyState>(), frame.Samus, frame.Level, frame.ControllerInput);
    }

    /// <summary>Advances Beetom's typed animation.</summary>
    internal static void AdvanceBeetomTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, BeetomAnimations.Segments,
            static (BeetomEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyBeetomCue(place, cue) is { } target
                ? new AnimationContinuation<BeetomSegment>(BeetomAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Clears attachment after every accepted common shot, including zero damage.</summary>
    /// <remarks>Beetom lets go after the common damage of every accepted hit, including a hit that deals it no
    /// damage.</remarks>
    internal static EnemyShotReaction BeetomShotReaction { get; } = new()
    {
        AfterCommon = c => ResolveBeetomShotAfterCommon(c.Enemy),
    };

    /// <summary>Attaches Beetom to Samus on its selected custom contact reaction.</summary>
    internal void ReactToBeetomTouch(EnemyTouchContext context) =>
        ResolveBeetomTouch(context.Place, context.Place.RequireSpeciesState<BeetomEnemyState>(), context.Samus, context.PseudoScrew);
}
