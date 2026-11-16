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
    /// <summary>Provides the active frame's Samus and room data to its behaviours.</summary>
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

    /// <summary>Strict distance around Samus that changes Beetom's action selection.</summary>
    private const int BeetomProximityDistance = 96;
    /// <summary>Input changes required to dislodge an attached draining Beetom.</summary>
    private const int BeetomMashCount = 64;

    /// <summary>Initializes the Beetom.</summary>
    internal void InitializeBeetom(RoomEnemyPlace place, SamusState samus, GameplayAction controllerInput)
    {
        BeetomEnemyState state = _inheritance.Inherit(place.Ordinal, static previous => new BeetomEnemyState(previous, BeetomAction.DecideAction));
        place.SetSpeciesState(state);

        state.ActionTimer = 0;
        state.Falling = false;
        state.AttachedToSamus = false;
        state.ButtonCounter = BeetomMashCount;
        state.PreviousController1Input = controllerInput;

        // Every Beetom's initialization reseeds the game's random numbers, even when several Beetoms share a room.
        _room.Random.Seed(BeetomRandomDefinitions.Seed);

        state.InitialShortLeapYSpeedCurvePlace = BeetomLeapDefinitions.ShortLeap.StartPlace;
        state.InitialLongLeapYSpeedCurvePlace = BeetomLeapDefinitions.LongLeap.StartPlace;
        state.InitialLungeYSpeedCurvePlace = BeetomLeapDefinitions.Lunge.StartPlace;

        // Samus's side picks the facing: a negative Samus-minus-enemy delta
        // selects the left-facing animation; nonnegative selects the right-facing animation.
        SetBeetomAnimation(
            place,
            state,
            CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) < 0
                ? BeetomSegment.CrawlingLeft
                : BeetomSegment.CrawlingRight);
    }

    /// <summary>Runs a Beetom's per-frame behaviour through all of its actions.</summary>
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
                state.Action = CoordinateAxis.Distance(samus.Position.X.Whole, place.Position.X.Whole) < BeetomProximityDistance
                    ? BeetomAction.DecideActionSamusInProximity
                    : BeetomAction.DecideActionSamusNotInProximity;
                return;
            case BeetomAction.DecideActionSamusNotInProximity:
                ChooseDistantBeetomAction(state);
                return;
            case BeetomAction.StartIdling:
                state.ActionTimer = 32;
                state.Action = BeetomAction.Idling;
                return;
            case BeetomAction.StartCrawlingLeft:
                StartBeetomCrawling(place, state, direction: HorizontalDirection.Left);
                return;
            case BeetomAction.StartCrawlingRight:
                StartBeetomCrawling(place, state, direction: HorizontalDirection.Right);
                return;
            case BeetomAction.StartShortHopLeft:
                StartBeetomHop(place, state, state.InitialShortLeapYSpeedCurvePlace, direction: HorizontalDirection.Left, BeetomHopLength.Short);
                return;
            case BeetomAction.StartShortHopRight:
                StartBeetomHop(place, state, state.InitialShortLeapYSpeedCurvePlace, direction: HorizontalDirection.Right, BeetomHopLength.Short);
                return;
            case BeetomAction.StartLongHopLeft:
                StartBeetomHop(place, state, state.InitialLongLeapYSpeedCurvePlace, direction: HorizontalDirection.Left, BeetomHopLength.Long);
                return;
            case BeetomAction.StartLongHopRight:
                StartBeetomHop(place, state, state.InitialLongLeapYSpeedCurvePlace, direction: HorizontalDirection.Right, BeetomHopLength.Long);
                return;
            case BeetomAction.DecideActionSamusInProximity:
                StartBeetomLunge(place, state, samus);
                return;
            case BeetomAction.StartDrainingLeft:
                StartBeetomDrain(place, state, side: HorizontalDirection.Left);
                return;
            case BeetomAction.StartDrainingRight:
                StartBeetomDrain(place, state, side: HorizontalDirection.Right);
                return;
            case BeetomAction.StartDropping:
                StartBeetomDrop(place, state);
                return;
            case BeetomAction.StartBeingFlung:
                state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
                state.Action = BeetomAction.BeingFlung;
                return;
            case BeetomAction.Idling:
                if (--state.ActionTimer < 0)
                    state.Action = BeetomAction.DecideAction;
                return;
            case BeetomAction.CrawlingLeft:
                RunBeetomCrawl(place, state, level, direction: HorizontalDirection.Left);
                return;
            case BeetomAction.CrawlingRight:
                RunBeetomCrawl(place, state, level, direction: HorizontalDirection.Right);
                return;
            case BeetomAction.ShortHopLeft:
                RunBeetomArc(place, state, level, direction: HorizontalDirection.Left, BeetomArcKind.Hop, risingDelta: 4, fallingDelta: 4);
                return;
            case BeetomAction.ShortHopRight:
                RunBeetomArc(place, state, level, direction: HorizontalDirection.Right, BeetomArcKind.Hop, risingDelta: 4, fallingDelta: 4);
                return;
            case BeetomAction.LongHopLeft:
                RunBeetomArc(place, state, level, direction: HorizontalDirection.Left, BeetomArcKind.Hop, risingDelta: 5, fallingDelta: 5);
                return;
            case BeetomAction.LongHopRight:
                RunBeetomArc(place, state, level, direction: HorizontalDirection.Right, BeetomArcKind.Hop, risingDelta: 5, fallingDelta: 5);
                return;
            case BeetomAction.LungeLeft:
                RunBeetomArc(place, state, level, direction: HorizontalDirection.Left, BeetomArcKind.Lunge, risingDelta: 5, fallingDelta: 3);
                return;
            case BeetomAction.LungeRight:
                RunBeetomArc(place, state, level, direction: HorizontalDirection.Right, BeetomArcKind.Lunge, risingDelta: 5, fallingDelta: 3);
                return;
            case BeetomAction.DrainingLeft:
                RunBeetomDrain(place, state, samus, level, controllerInput, side: HorizontalDirection.Left);
                return;
            case BeetomAction.DrainingRight:
                RunBeetomDrain(place, state, samus, level, controllerInput, side: HorizontalDirection.Right);
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

    /// <summary>Uses one random sample's <see cref="BeetomDecisions.NextAction"/> to choose an idle or hop branch.</summary>
    internal void ChooseDistantBeetomAction(BeetomEnemyState state)
    {
        int random = _room.Random.Generate();
        state.Action = BeetomDecisions.NextAction.Of(random) switch
        {
            0 or 1 => BeetomAction.StartIdling,
            2 or 4 => BeetomAction.StartShortHopLeft,
            3 or 7 => BeetomAction.StartLongHopRight,
            5 => BeetomAction.StartShortHopRight,
            _ => BeetomAction.StartLongHopLeft,
        };
        state.FacingRight = BeetomDecisions.FaceRight.IsSetIn(random);
    }

    /// <summary>Starts side-specific crawl AI and animation.</summary>
    internal static void StartBeetomCrawling(RoomEnemyPlace place, BeetomEnemyState state, HorizontalDirection direction)
    {
        state.Action = direction == HorizontalDirection.Left ? BeetomAction.CrawlingLeft : BeetomAction.CrawlingRight;
        SetBeetomAnimation(
            place,
            state,
            direction == HorizontalDirection.Left
                ? BeetomSegment.CrawlingLeft
                : BeetomSegment.CrawlingRight);
    }

    /// <summary>Arms a short or long arc from its precomputed initial speed-curve place.</summary>
    internal static void StartBeetomHop(
        RoomEnemyPlace place,
        BeetomEnemyState state,
        QuadraticSpeedCurvePlace initialYSpeedCurvePlace,
        HorizontalDirection direction,
        BeetomHopLength length)
    {
        state.YSpeedCurvePlace = initialYSpeedCurvePlace;
        state.Action = new BeetomHop(direction, length) switch
        {
            (HorizontalDirection.Left, BeetomHopLength.Short) => BeetomAction.ShortHopLeft,
            (HorizontalDirection.Right, BeetomHopLength.Short) => BeetomAction.ShortHopRight,
            (HorizontalDirection.Left, BeetomHopLength.Long) => BeetomAction.LongHopLeft,
            (HorizontalDirection.Right, BeetomHopLength.Long) => BeetomAction.LongHopRight,
            _ => throw new ArgumentOutOfRangeException(nameof(length), length, "Unknown Beetom hop."),
        };
        state.Falling = false;
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
        bool left = CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) < 0;
        state.YSpeedCurvePlace = state.InitialLungeYSpeedCurvePlace;
        state.FacingRight = !left;
        state.Action = state.FacingRight ? BeetomAction.LungeRight : BeetomAction.LungeLeft;
        state.Falling = false;
        SetBeetomAnimation(
            place,
            state,
            left
                ? BeetomSegment.HopLeft
                : BeetomSegment.HopRight);
    }

    /// <summary>Starts the attached blood-drain animation for the current facing.</summary>
    internal static void StartBeetomDrain(RoomEnemyPlace place, BeetomEnemyState state, HorizontalDirection side)
    {
        SetBeetomAnimation(
            place,
            state,
            side == HorizontalDirection.Left
                ? BeetomSegment.DrainingLeft
                : BeetomSegment.DrainingRight);
        state.Action = side == HorizontalDirection.Left ? BeetomAction.DrainingLeft : BeetomAction.DrainingRight;
    }

    /// <summary>Switches back to crawl artwork before dropping toward the floor.</summary>
    private static void StartBeetomDrop(RoomEnemyPlace place, BeetomEnemyState state)
    {
        SetBeetomAnimation(
            place,
            state,
            state.FacingRight
                ? BeetomSegment.CrawlingRight
                : BeetomSegment.CrawlingLeft);
        state.Falling = false;
        state.Action = BeetomAction.Dropping;
    }

    /// <summary>Probes eight pixels ahead for a ledge, then crawls or reverses at terrain.</summary>
    private void RunBeetomCrawl(RoomEnemyPlace place, BeetomEnemyState state, RoomLevelData level, HorizontalDirection direction)
    {
        if (--state.ActionTimer < 0)
        {
            state.ActionTimer = 64;
            state.Action = BeetomAction.DecideAction;
            return;
        }

        // The ledge probe temporarily moves the center eight pixels ahead, then
        // performs a real one-pixel downward collision move. Restore only the whole pixels,
        // preserving collision-produced subposition just as the game does.
        place.Position = place.Position.WithWrappedWholeX(place.Position.X.Whole + (direction == HorizontalDirection.Left ? -8 : 8));
        if (!_terrain.MoveEnemyVertically(level, place, SignedSixteenSixteen.FromWholePixels(1)))
        {
            state.Action = direction == HorizontalDirection.Left ? BeetomAction.StartCrawlingRight : BeetomAction.StartCrawlingLeft;
            place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole - 1);
            place.Position = place.Position.WithWrappedWholeX(place.Position.X.Whole + (direction == HorizontalDirection.Left ? 8 : -8));
            return;
        }

        place.Position = place.Position.WithWrappedWholeX(place.Position.X.Whole + (direction == HorizontalDirection.Left ? 8 : -8));
        SignedSixteenSixteen displacement = direction == HorizontalDirection.Left ? SignedSixteenSixteen.FromSixteenthsOfPixel(-4) : SignedSixteenSixteen.FromSixteenthsOfPixel(4);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
            state.Action = direction == HorizontalDirection.Left ? BeetomAction.StartCrawlingRight : BeetomAction.StartCrawlingLeft;
    }

    /// <summary>Runs vertical arc before horizontal movement even when the first half changes AI state.</summary>
    private void RunBeetomArc(
        RoomEnemyPlace place,
        BeetomEnemyState state,
        RoomLevelData level,
        HorizontalDirection direction,
        BeetomArcKind arc,
        int risingDelta,
        int fallingDelta)
    {
        RunBeetomVerticalArc(place, state, level, risingDelta, fallingDelta);

        // The horizontal half still runs on the exact frame that vertical collision changes
        // the AI action. That ordering is easy to lose when expressing the two axes
        // as a single combined movement vector.
        SignedSixteenSixteen displacement = arc == BeetomArcKind.Lunge
            ? (direction == HorizontalDirection.Left ? SignedSixteenSixteen.FromWholePixels(-3) : SignedSixteenSixteen.FromWholePixels(3))
            : (direction == HorizontalDirection.Left ? SignedSixteenSixteen.FromSixteenthsOfPixel(-4) : SignedSixteenSixteen.FromSixteenthsOfPixel(4));
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
            return;

        state.FacingRight = !state.FacingRight;
        state.Action = BeetomAction.StartDropping;
    }

    /// <summary>Advances the ascent's or descent's place along the speed curve and switches state at ceiling or ground contact.</summary>
    private void RunBeetomVerticalArc(
        RoomEnemyPlace place,
        BeetomEnemyState state,
        RoomLevelData level,
        int risingDelta,
        int fallingDelta)
    {
        if (!state.Falling)
        {
            if (_terrain.MoveEnemyVertically(level, place, state.YSpeedCurvePlace.Speed.Negative))
            {
                state.Action = BeetomAction.StartDropping;
                return;
            }

            state.StepRisingSpeed(risingDelta);
            return;
        }

        if (_terrain.MoveEnemyVertically(level, place, state.YSpeedCurvePlace.Speed.Positive))
        {
            state.Action = BeetomAction.DecideAction;
            return;
        }

        state.StepFallingSpeed(fallingDelta);
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
            place.Position = place.Position.WithWhole(new(samus.Position.X.Whole, CoordinateAxis.Wrap(samus.Position.Y.Whole - 4)));
            if (controllerInput != state.PreviousController1Input)
            {
                state.PreviousController1Input = controllerInput;
                state.ButtonCounter--;
            }
            return;
        }

        // Escape throws the Beetom sixteen pixels away. A wall reverses the direction and
        // applies an additional 32-pixel move, producing the net sixteen-pixel throw
        // to the other side rather than merely cancelling the first displacement.
        SignedSixteenSixteen initialDisplacement = side == HorizontalDirection.Left ? SignedSixteenSixteen.FromWholePixels(16) : SignedSixteenSixteen.FromWholePixels(-16);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, initialDisplacement))
        {
            state.FacingRight = side == HorizontalDirection.Left;
            SignedSixteenSixteen reflectedDisplacement = side == HorizontalDirection.Left ? SignedSixteenSixteen.FromWholePixels(-32) : SignedSixteenSixteen.FromWholePixels(32);
            _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, reflectedDisplacement);
        }
        state.AttachedToSamus = false;
        state.Action = BeetomAction.StartBeingFlung;
    }

    /// <summary>Falls three pixels per frame and resumes crawling on floor contact.</summary>
    private void RunBeetomDrop(RoomEnemyPlace place, BeetomEnemyState state, RoomLevelData level)
    {
        if (!_terrain.MoveEnemyVertically(level, place, SignedSixteenSixteen.FromWholePixels(3)))
            return;
        state.Action = state.FacingRight
            ? BeetomAction.StartCrawlingRight
            : BeetomAction.StartCrawlingLeft;
    }

    /// <summary>Moves opposite the pre-escape facing until a floor or wall selects its next action.</summary>
    private void RunFlungBeetom(RoomEnemyPlace place, BeetomEnemyState state, RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(level, place, state.YSpeedCurvePlace.Speed.Positive))
            state.Action = BeetomAction.DecideAction;
        else
            state.StepFallingSpeed(1);

        // The facing is the pre-escape facing. The throw deliberately travels in the
        // opposite direction: facing left moves right and facing right moves left.
        SignedSixteenSixteen horizontal = state.FacingRight ? SignedSixteenSixteen.FromWholePixels(-2) : SignedSixteenSixteen.FromWholePixels(2);
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, horizontal))
            return;
        state.FacingRight = !state.FacingRight;
        state.Action = BeetomAction.StartDropping;
    }

    /// <summary>Runs Beetom's own touch response.</summary>
    private void ResolveBeetomTouch(
        RoomEnemyPlace place,
        BeetomEnemyState state,
        SamusState samus)
    {
        if (!state.AttachedToSamus)
        {
            state.Action = state.FacingRight
                ? BeetomAction.StartDrainingRight
                : BeetomAction.StartDrainingLeft;
            state.AttachedToSamus = true;
            state.ButtonCounter = BeetomMashCount;
        }

        if (samus.HorizontalSpeed.BodyAttack != SamusBodyAttack.None)
        {
            _collision.ResolveNormalEnemyTouch(place, samus);
            samus.InvincibilityTimer = WrappingCountdown.Start(0);
            samus.KnockbackTimer = 0;
            return;
        }

        if (BeetomCadenceDefinitions.DrainSoundEveryEighthEnemyPass.IsDue(_frameContext.RandomEnemyCounter) && samus.Health >= 30)
            _audio.QueueEnemySound(RoomSound.IncrementalEnergy, priority: SoundRequestPriority.Normal);

        // Unlike ordinary touch damage, the once-per-64-frame drain explicitly cancels the
        // invincibility and knockback installed by common AI. Apply the suit-scaled health
        // subtraction directly, then reproduce those two clearing stores.
        if (!BeetomCadenceDefinitions.DrainDamageEverySixtyFourthUpdate.IsDue(place.FrameCounter))
            return;
        SamusSuitDamage.Hurt(samus, place.Definition.Damage);
        samus.InvincibilityTimer = WrappingCountdown.Start(0);
        samus.KnockbackTimer = 0;
    }

    /// <summary>
    /// Runs the private post-common-shot tail: a surviving Beetom frozen while draining drops, and lets go of Samus. A
    /// hit that killed the Beetom took it from its place with its state, and the place's next Beetom starts detached, so
    /// the tail has nothing left to change.
    /// </summary>
    private static void ResolveBeetomShotAfterCommon(RoomEnemyPlace place)
    {
        if (place.Health == 0)
            return;
        BeetomEnemyState state = place.RequireSpeciesState<BeetomEnemyState>();
        if (place.FrozenTimer != 0 && state.Action is
            BeetomAction.DrainingLeft or BeetomAction.DrainingRight)
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

    /// <summary>Runs Beetom's attachment-clearing tail after common bomb damage.</summary>
    /// <remarks>Common damage can kill the Beetom before this private tail runs, so the identity is matched before the
    /// reaction runs.</remarks>
    internal static EnemyBombReaction BeetomBombReaction { get; } = new()
    {
        AfterCommon = c => ResolveBeetomShotAfterCommon(c.Enemy),
    };

    /// <summary>Runs Beetom's initialization AI.</summary>
    internal void RunBeetomInitializationAi(RoomEnemyPlace place, EnemySpawnSurroundings surroundings)
    {
        InitializeBeetom(place, surroundings.IntroducedSamus, surroundings.ControllerInput);
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
    /// <remarks>Beetom's private tail runs after common shot AI for every accepted hit, including immune/zero-damage
    /// vulnerability results.</remarks>
    internal static EnemyShotReaction BeetomShotReaction { get; } = new()
    {
        AfterCommon = c => ResolveBeetomShotAfterCommon(c.Enemy),
    };

    /// <summary>Attaches Beetom to Samus on its selected custom contact reaction.</summary>
    internal void ReactToBeetomTouch(EnemyTouchContext context) =>
        ResolveBeetomTouch(context.Place, context.Place.RequireSpeciesState<BeetomEnemyState>(), context.Samus);
}
