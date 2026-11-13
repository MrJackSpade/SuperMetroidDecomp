using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>The Beetom species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class BeetomBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Beetom];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new BeetomEntity(this, slot, common);

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
    /// <summary>Hands each new Beetom the previous Beetom in its slot, whose facing it inherits.</summary>
    private readonly SlotInheritance<BeetomEnemyState> _inheritance;

    /// <summary>Binds room, terrain, audio, collision and slot-inheritance services for Beetom AI.</summary>
    internal BeetomBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyAudioQueue audio, EnemyFrameContext frameContext,
        EnemyCollision collision, SlotInheritance<BeetomEnemyState> inheritance)
    {
        _room = room;
        _terrain = terrain;
        _audio = audio;
        _frameContext = frameContext;
        _collision = collision;
        _inheritance = inheritance;
    }

    /// <summary>Carries out one cue of the Beetom's animations: off-screen processing, or the drain's no-op stub.</summary>
    private static BeetomSegment? ApplyBeetomCue(RoomEnemySlot slot, BeetomCue cue)
    {
        switch (cue.Kind)
        {
            case BeetomCueKind.ProcessOffScreen:
                slot.Processing = slot.Processing.With(EnemyProcessing.OffScreen);
                break;
            case BeetomCueKind.StopProcessingOffScreen:
                slot.Processing = slot.Processing.Without(EnemyProcessing.OffScreen);
                break;
            case BeetomCueKind.Nothing:
                // Beetom's initial drain animation calls a cue that does nothing before it falls through into the looping
                // blood-spray frames. It changes no state.
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
    internal void InitializeBeetom(RoomEnemySlot slot, SamusState? samus, ControllerButton controllerInput)
    {
        if (samus is null)
            throw new InvalidOperationException("Beetom initialization requires the active Samus actor.");

        BeetomEnemyState state = _inheritance.Inherit(slot.SlotIndex, static previous => new BeetomEnemyState(previous));
        slot.SetSpeciesState(state);

        state.ActionTimer = 0;
        state.Action = null;
        state.Falling = false;
        state.AttachedToSamus = false;
        state.ButtonCounter = BeetomMashCount;
        state.PreviousController1Input = controllerInput;

        // Every Beetom initializer writes the global seed, even when several Beetoms share
        // a room. This is observable game behavior, not a per-actor random stream.
        Action<int> setRandomNumber = _room.Random.Seed;
        setRandomNumber(23);

        state.InitialShortLeapYSpeedIndex = CalculateInitialBeetomYSpeedIndex(12288, 4);
        state.InitialLongLeapYSpeedIndex = CalculateInitialBeetomYSpeedIndex(16384, 5);
        state.InitialLungeYSpeedIndex = CalculateInitialBeetomYSpeedIndex(12288, 3);

        // The source label's branch name is reversed. A negative Samus-minus-enemy delta
        // selects the left-facing animation; nonnegative selects the right-facing animation.
        SetBeetomAnimation(
            slot,
            state,
            CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0
                ? BeetomSegment.CrawlingLeft
                : BeetomSegment.CrawlingRight);
        state.Action = BeetomAction.DecideAction;
    }

    /// <summary>Runs a Beetom's per-frame behaviour through all of its functions.</summary>
    private void RunBeetomMain(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        ControllerButton controllerInput)
    {
        switch (state.Action)
        {
            case BeetomAction.DecideAction:
                RequireBeetomSamus(samus);
                state.Action = CoordinateAxis.Distance(samus.Position.X.Whole, slot.Position.X.Whole) < BeetomProximityDistance
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
                StartBeetomCrawling(slot, state, direction: HorizontalDirection.Left);
                return;
            case BeetomAction.StartCrawlingRight:
                StartBeetomCrawling(slot, state, direction: HorizontalDirection.Right);
                return;
            case BeetomAction.StartShortHopLeft:
                StartBeetomHop(slot, state, state.InitialShortLeapYSpeedIndex, direction: HorizontalDirection.Left, BeetomHopLength.Short);
                return;
            case BeetomAction.StartShortHopRight:
                StartBeetomHop(slot, state, state.InitialShortLeapYSpeedIndex, direction: HorizontalDirection.Right, BeetomHopLength.Short);
                return;
            case BeetomAction.StartLongHopLeft:
                StartBeetomHop(slot, state, state.InitialLongLeapYSpeedIndex, direction: HorizontalDirection.Left, BeetomHopLength.Long);
                return;
            case BeetomAction.StartLongHopRight:
                StartBeetomHop(slot, state, state.InitialLongLeapYSpeedIndex, direction: HorizontalDirection.Right, BeetomHopLength.Long);
                return;
            case BeetomAction.DecideActionSamusInProximity:
                RequireBeetomSamus(samus);
                StartBeetomLunge(slot, state, samus);
                return;
            case BeetomAction.StartDrainingLeft:
                StartBeetomDrain(slot, state, side: HorizontalDirection.Left);
                return;
            case BeetomAction.StartDrainingRight:
                StartBeetomDrain(slot, state, side: HorizontalDirection.Right);
                return;
            case BeetomAction.StartDropping:
                StartBeetomDrop(slot, state);
                return;
            case BeetomAction.StartBeingFlung:
                state.YSpeedTableIndex = EnemyQuadraticSpeedIndex.Start;
                state.Action = BeetomAction.BeingFlung;
                return;
            case BeetomAction.Idling:
                if (--state.ActionTimer < 0)
                    state.Action = BeetomAction.DecideAction;
                return;
            case BeetomAction.CrawlingLeft:
                RequireBeetomLevel(level);
                RunBeetomCrawl(slot, state, level, direction: HorizontalDirection.Left);
                return;
            case BeetomAction.CrawlingRight:
                RequireBeetomLevel(level);
                RunBeetomCrawl(slot, state, level, direction: HorizontalDirection.Right);
                return;
            case BeetomAction.ShortHopLeft:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level, direction: HorizontalDirection.Left, BeetomArcKind.Hop, risingDelta: 4, fallingDelta: 4);
                return;
            case BeetomAction.ShortHopRight:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level, direction: HorizontalDirection.Right, BeetomArcKind.Hop, risingDelta: 4, fallingDelta: 4);
                return;
            case BeetomAction.LongHopLeft:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level, direction: HorizontalDirection.Left, BeetomArcKind.Hop, risingDelta: 5, fallingDelta: 5);
                return;
            case BeetomAction.LongHopRight:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level, direction: HorizontalDirection.Right, BeetomArcKind.Hop, risingDelta: 5, fallingDelta: 5);
                return;
            case BeetomAction.LungeLeft:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level, direction: HorizontalDirection.Left, BeetomArcKind.Lunge, risingDelta: 5, fallingDelta: 3);
                return;
            case BeetomAction.LungeRight:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level, direction: HorizontalDirection.Right, BeetomArcKind.Lunge, risingDelta: 5, fallingDelta: 3);
                return;
            case BeetomAction.DrainingLeft:
                RequireBeetomSamus(samus);
                RequireBeetomLevel(level);
                RunBeetomDrain(slot, state, samus, level, controllerInput, side: HorizontalDirection.Left);
                return;
            case BeetomAction.DrainingRight:
                RequireBeetomSamus(samus);
                RequireBeetomLevel(level);
                RunBeetomDrain(slot, state, samus, level, controllerInput, side: HorizontalDirection.Right);
                return;
            case BeetomAction.Dropping:
                RequireBeetomLevel(level);
                RunBeetomDrop(slot, state, level);
                return;
            case BeetomAction.BeingFlung:
                RequireBeetomLevel(level);
                RunFlungBeetom(slot, state, level);
                return;
            default:
                throw new InvalidDataException($"Beetom function {state.Action?.ToString() ?? "none"} is not translated.");
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
    internal static void StartBeetomCrawling(RoomEnemySlot slot, BeetomEnemyState state, HorizontalDirection direction)
    {
        state.Action = direction == HorizontalDirection.Left ? BeetomAction.CrawlingLeft : BeetomAction.CrawlingRight;
        SetBeetomAnimation(
            slot,
            state,
            direction == HorizontalDirection.Left
                ? BeetomSegment.CrawlingLeft
                : BeetomSegment.CrawlingRight);
    }

    /// <summary>Arms a short or long arc from its precomputed initial speed index.</summary>
    internal static void StartBeetomHop(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        EnemyQuadraticSpeedIndex initialYSpeedIndex,
        HorizontalDirection direction,
        BeetomHopLength length)
    {
        state.YSpeedTableIndex = initialYSpeedIndex;
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
            slot,
            state,
            direction == HorizontalDirection.Left
                ? BeetomSegment.HopLeft
                : BeetomSegment.HopRight);
    }

    /// <summary>Aims the lunge toward Samus using the signed horizontal difference.</summary>
    private static void StartBeetomLunge(RoomEnemySlot slot, BeetomEnemyState state, SamusState samus)
    {
        bool left = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0;
        state.YSpeedTableIndex = state.InitialLungeYSpeedIndex;
        state.FacingRight = !left;
        state.Action = state.FacingRight ? BeetomAction.LungeRight : BeetomAction.LungeLeft;
        state.Falling = false;
        SetBeetomAnimation(
            slot,
            state,
            left
                ? BeetomSegment.HopLeft
                : BeetomSegment.HopRight);
    }

    /// <summary>Starts the attached blood-drain animation for the current facing.</summary>
    internal static void StartBeetomDrain(RoomEnemySlot slot, BeetomEnemyState state, HorizontalDirection side)
    {
        SetBeetomAnimation(
            slot,
            state,
            side == HorizontalDirection.Left
                ? BeetomSegment.DrainingLeft
                : BeetomSegment.DrainingRight);
        state.Action = side == HorizontalDirection.Left ? BeetomAction.DrainingLeft : BeetomAction.DrainingRight;
    }

    /// <summary>Switches back to crawl artwork before dropping toward the floor.</summary>
    private static void StartBeetomDrop(RoomEnemySlot slot, BeetomEnemyState state)
    {
        SetBeetomAnimation(
            slot,
            state,
            state.FacingRight
                ? BeetomSegment.CrawlingRight
                : BeetomSegment.CrawlingLeft);
        state.Falling = false;
        state.Action = BeetomAction.Dropping;
    }

    /// <summary>Probes eight pixels ahead for a ledge, then crawls or reverses at terrain.</summary>
    private void RunBeetomCrawl(RoomEnemySlot slot, BeetomEnemyState state, RoomLevelData level, HorizontalDirection direction)
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
        slot.Position = slot.Position.WithWrappedWholeX(slot.Position.X.Whole + (direction == HorizontalDirection.Left ? -8 : 8));
        if (!_terrain.MoveEnemyVertically(level, slot, SignedSixteenSixteen.FromWholePixels(1)))
        {
            state.Action = direction == HorizontalDirection.Left ? BeetomAction.StartCrawlingRight : BeetomAction.StartCrawlingLeft;
            slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole - 1);
            slot.Position = slot.Position.WithWrappedWholeX(slot.Position.X.Whole + (direction == HorizontalDirection.Left ? 8 : -8));
            return;
        }

        slot.Position = slot.Position.WithWrappedWholeX(slot.Position.X.Whole + (direction == HorizontalDirection.Left ? 8 : -8));
        SignedSixteenSixteen displacement = direction == HorizontalDirection.Left ? SignedSixteenSixteen.FromSixteenthsOfPixel(-4) : SignedSixteenSixteen.FromSixteenthsOfPixel(4);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            state.Action = direction == HorizontalDirection.Left ? BeetomAction.StartCrawlingRight : BeetomAction.StartCrawlingLeft;
    }

    /// <summary>Runs vertical arc before horizontal movement even when the first half changes AI state.</summary>
    private void RunBeetomArc(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        RoomLevelData level,
        HorizontalDirection direction,
        BeetomArcKind arc,
        int risingDelta,
        int fallingDelta)
    {
        RunBeetomVerticalArc(slot, state, level, risingDelta, fallingDelta);

        // The horizontal half still runs on the exact frame that vertical collision changes
        // the AI function. That ordering is easy to lose when expressing the two axes
        // as a single host movement vector.
        SignedSixteenSixteen displacement = arc == BeetomArcKind.Lunge
            ? (direction == HorizontalDirection.Left ? SignedSixteenSixteen.FromWholePixels(-3) : SignedSixteenSixteen.FromWholePixels(3))
            : (direction == HorizontalDirection.Left ? SignedSixteenSixteen.FromSixteenthsOfPixel(-4) : SignedSixteenSixteen.FromSixteenthsOfPixel(4));
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            return;

        state.FacingRight = !state.FacingRight;
        state.Action = BeetomAction.StartDropping;
    }

    /// <summary>Advances quadratic ascent/descent indices and switches state at ceiling or ground contact.</summary>
    private void RunBeetomVerticalArc(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        RoomLevelData level,
        int risingDelta,
        int fallingDelta)
    {
        if (!state.Falling)
        {
            if (_terrain.MoveEnemyVertically(level, slot, state.YSpeedTableIndex.Row.Negative))
            {
                state.Action = BeetomAction.StartDropping;
                return;
            }

            state.StepRisingSpeed(risingDelta);
            return;
        }

        if (_terrain.MoveEnemyVertically(level, slot, state.YSpeedTableIndex.Row.Positive))
        {
            state.Action = BeetomAction.DecideAction;
            return;
        }

        state.StepFallingSpeed(fallingDelta);
    }

    /// <summary>Follows Samus until enough input changes occur, then throws Beetom clear of terrain.</summary>
    private void RunBeetomDrain(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        SamusState samus,
        RoomLevelData level,
        ControllerButton controllerInput,
        HorizontalDirection side)
    {
        if (state.ButtonCounter != 0)
        {
            slot.Position = slot.Position.WithWhole(new(samus.Position.X.Whole, CoordinateAxis.Wrap(samus.Position.Y.Whole - 4)));
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
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, initialDisplacement))
        {
            state.FacingRight = side == HorizontalDirection.Left;
            SignedSixteenSixteen reflectedDisplacement = side == HorizontalDirection.Left ? SignedSixteenSixteen.FromWholePixels(-32) : SignedSixteenSixteen.FromWholePixels(32);
            _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, reflectedDisplacement);
        }
        state.AttachedToSamus = false;
        state.Action = BeetomAction.StartBeingFlung;
    }

    /// <summary>Falls three pixels per frame and resumes crawling on floor contact.</summary>
    private void RunBeetomDrop(RoomEnemySlot slot, BeetomEnemyState state, RoomLevelData level)
    {
        if (!_terrain.MoveEnemyVertically(level, slot, SignedSixteenSixteen.FromWholePixels(3)))
            return;
        state.Action = state.FacingRight
            ? BeetomAction.StartCrawlingRight
            : BeetomAction.StartCrawlingLeft;
    }

    /// <summary>Moves opposite the pre-escape facing until a floor or wall selects its next action.</summary>
    private void RunFlungBeetom(RoomEnemySlot slot, BeetomEnemyState state, RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(level, slot, state.YSpeedTableIndex.Row.Positive))
            state.Action = BeetomAction.DecideAction;
        else
            state.StepFallingSpeed(1);

        // The facing is the pre-escape facing. The throw deliberately travels in the
        // opposite direction: facing left moves right and facing right moves left.
        SignedSixteenSixteen horizontal = state.FacingRight ? SignedSixteenSixteen.FromWholePixels(-2) : SignedSixteenSixteen.FromWholePixels(2);
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontal))
            return;
        state.FacingRight = !state.FacingRight;
        state.Action = BeetomAction.StartDropping;
    }

    /// <summary>Sums the quadratic table's coarse speeds until a jump height is reached.</summary>
    internal static EnemyQuadraticSpeedIndex CalculateInitialBeetomYSpeedIndex(int targetHeight, int tableIndexDelta)
    {
        EnemyQuadraticSpeedIndex tableIndex = EnemyQuadraticSpeedIndex.Start;
        int accumulatedHeight = 0;
        // Each pass moves the row on by the delta, so the height is reached within the table's rows or never: past them
        // the row lookup rejects the climb before the place could grow further, and a delta of zero sums the first row's
        // speed of zero only (#1627).
        for (int iteration = 0; iteration < EnemyQuadraticSpeedDefinitions.RowCount; iteration++)
        {
            tableIndex += EnemyQuadraticSpeedIndex.FromRows(tableIndexDelta);
            // The height sums each row's coarse speed, in 256ths of a pixel; summing full speeds changes every arc.
            accumulatedHeight = WrappingCounter.AddCount(accumulatedHeight,
                tableIndex.Row.CoarseToward(EnemySpeedSign.Positive));
            if (CoordinateAxis.Difference(accumulatedHeight, targetHeight) >= 0)
                return tableIndex;
        }
        throw new InvalidDataException("Beetom initial-speed calculation did not reach its authored target height.");
    }

    /// <summary>Runs Beetom's custom touch handler.</summary>
    private void ResolveBeetomTouch(
        RoomEnemySlot slot,
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

        if (samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None)
        {
            _collision.ResolveNormalEnemyTouch(slot, samus);
            samus.InvincibilityTimer = WrappingCountdown.Start(0);
            samus.KnockbackTimer = 0;
            return;
        }

        if (BeetomCadenceDefinitions.DrainSoundEveryEighthEnemyPass.IsDue(_frameContext.RandomEnemyCounter) && samus.Health >= 30)
            _audio.QueueEnemySound(RoomSound.IncrementalEnergy, priority: SoundRequestPriority.Normal);

        // Unlike ordinary touch damage, the once-per-64-frame drain explicitly cancels the
        // invincibility and knockback installed by common AI. Apply the suit-scaled health
        // subtraction directly, then reproduce those two clearing stores.
        if (!BeetomCadenceDefinitions.DrainDamageEverySixtyFourthUpdate.IsDue(slot.FrameCounter))
            return;
        SamusSuitDamage.Hurt(samus, slot.Definition.Damage);
        samus.InvincibilityTimer = WrappingCountdown.Start(0);
        samus.KnockbackTimer = 0;
    }

    /// <summary>
    /// Runs the private post-common-shot tail: a surviving Beetom frozen while draining drops, and lets go of Samus. A
    /// hit that killed the Beetom took it from its slot with its state, and the slot's next Beetom starts detached, so
    /// the tail has nothing left to change.
    /// </summary>
    private static void ResolveBeetomShotAfterCommon(RoomEnemySlot slot)
    {
        if (slot.Health == 0)
            return;
        BeetomEnemyState state = slot.RequireSpeciesState<BeetomEnemyState>();
        if (slot.FrozenTimer != 0 && state.Action is
            BeetomAction.DrainingLeft or BeetomAction.DrainingRight)
        {
            state.Action = BeetomAction.StartDropping;
        }
        state.AttachedToSamus = false;
    }

    /// <summary>Installs a Beetom animation and primes its animation timer.</summary>
    private static void SetBeetomAnimation(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        BeetomSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(slot, BeetomAnimations.StartingWith(segment));
    }

    /// <summary>Requires a live Samus actor for attachment and targeting phases.</summary>
    private static void RequireBeetomSamus([NotNull] SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Beetom AI requires the active Samus actor.");
    }

    /// <summary>Requires room collision data before moving Beetom through terrain.</summary>
    private static void RequireBeetomLevel([NotNull] RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Beetom movement requires room level data.");
    }

    /// <summary>Runs Beetom's attachment-clearing tail after common bomb damage.</summary>
    internal static void AddBeetomBombReactions(EnemySharedReactions table)
    {
        // Common damage can kill the Beetom before this private tail runs, so the identity is matched before dispatch.
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Beetom &&
                                                       EnemyCollision.ShotAi(c.Enemy) == EnemyReaction.BeetomShot &&
                                                       c.SelectedShotAi == EnemyReaction.BeetomShot)
        {
            AfterCommon = c => ResolveBeetomShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Runs Beetom's initialization AI.</summary>
    internal void RunBeetomInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeBeetom(slot, surroundings.Samus, surroundings.ControllerInput);
    }

    /// <summary>Runs Beetom's main AI.</summary>
    internal void RunBeetomMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunBeetomMain(slot, slot.RequireSpeciesState<BeetomEnemyState>(), frame.Samus, frame.Level, frame.ControllerInput);
    }

    /// <summary>Advances Beetom's typed animation.</summary>
    internal static void AdvanceBeetomTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, BeetomAnimations.Segments,
            static (BeetomEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyBeetomCue(slot, cue) is { } target
                ? new AnimationContinuation<BeetomSegment>(BeetomAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Clears attachment after every accepted common shot, including zero damage.</summary>
    internal static void AddBeetomShotReactions(EnemySharedReactions table)
    {
        // Beetom's private tail runs after common shot AI for every accepted hit, including immune/zero-damage
        // vulnerability results.
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Beetom &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.BeetomShot)
        {
            AfterCommon = c => ResolveBeetomShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Attaches Beetom to Samus on its selected custom contact reaction.</summary>
    internal void AddBeetomTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Beetom &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.BeetomTouch,
            context => ResolveBeetomTouch(context.Slot, context.Slot.RequireSpeciesState<BeetomEnemyState>(), context.Samus));
    }
}
