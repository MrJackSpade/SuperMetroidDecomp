using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Owtch.Artwork;
namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>The Owtch species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class OwtchBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The five states an authored initial state value selects, in its counting order.</summary>
    private static readonly IdentitySequence<OwtchBehaviorState> InitialStates = new(
        OwtchBehaviorState.MovingLeft, OwtchBehaviorState.MovingRight, OwtchBehaviorState.Underground,
        OwtchBehaviorState.Sinking, OwtchBehaviorState.Rising);

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Owtch];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new OwtchEntity(this, slot, common);

    /// <summary>Supplies the shared room random sequence used for burial and emergence.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Binds Owtch AI to the room's shared random source.</summary>
    internal OwtchBehavior(EnemyRoomBinding room)
    {
        _room = room;
    }

    /// <summary>Carries out one cue of Owtch's animations: each sets the walking direction main AI applies.</summary>
    private static OwtchProgram? ApplyOwtchCue(OwtchEnemyState state, OwtchCue cue)
    {
        switch (cue.Kind)
        {
            case OwtchCueKind.SetMovingLeft:
                SetOwtchMoving(state, OwtchBehaviorState.MovingLeft);
                break;
            case OwtchCueKind.SetMovingRight:
                SetOwtchMoving(state, OwtchBehaviorState.MovingRight);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(OwtchCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Owtch sinks sixteen whole pixels below its walking height.</summary>
    private const int OwtchMaximumBurialDepth = 16;

    /// <summary>Initializes the Owtch.</summary>
    internal static void InitializeOwtch(RoomEnemySlot slot)
    {
        OwtchSpawnSettings settings = slot.SpawnSettings<OwtchSpawnSettings>();
        int initialState = settings.InitialState;
        int undergroundTimerIndex = settings.UndergroundTimerIndex;
        int speedIndex = settings.SpeedIndex;
        int distanceIndex = settings.DistanceIndex;
        if (initialState >= 5)
        {
            throw new InvalidDataException(
                $"Owtch initial state {initialState} is outside its five-entry main-AI table.");
        }
        if (speedIndex >= EnemyLinearSpeedDefinitions.RowCount)
        {
            throw new InvalidDataException(
                $"Owtch linear-speed index {speedIndex} is outside the NTSC retail table.");
        }
        int undergroundTimer =
            OwtchPatrol.UndergroundTimer(WordWrap.LowByte(undergroundTimerIndex));
        int travelDistance =
            OwtchPatrol.TravelDistance(WordWrap.LowByte(distanceIndex));

        // The initializer masks the raw state only for its two-entry facing table. Thus
        // states two and four begin with the left list, while state three begins right.
        OwtchProgram initialProgram = (initialState & 1) == 0 ? OwtchProgram.MovingLeft : OwtchProgram.MovingRight;

        EnemyLinearSpeedRow speeds = EnemyLinearSpeeds.Row(speedIndex);
        var state = new OwtchEnemyState
        {
            RightVelocity = speeds.Positive,
            LeftVelocity = speeds.Negative,
            Behavior = InitialStates.At(initialState),
            BurialDepth = 0,
            UndergroundTimer = WrappingCountdown.FromRaw(undergroundTimer),
            MinimumPosition = new(CoordinateAxis.Wrap(slot.Position.X.Whole - travelDistance), 0),
            MaximumPosition = new(CoordinateAxis.Wrap(slot.Position.X.Whole + travelDistance), 0),
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);
        SetOwtchProgram(slot, state, initialProgram);

        // State two is the game's special initially-buried form. It moves only the
        // whole coordinate and records the same sixteen-pixel depth used by normal sinking.
        if (state.Behavior == OwtchBehaviorState.Underground)
        {
            state.BurialDepth = OwtchMaximumBurialDepth;
            slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + OwtchMaximumBurialDepth);
        }
    }

    /// <summary>Runs the Owtch's main AI and its indirect state table.</summary>
    private void RunOwtchMain(RoomEnemySlot slot, OwtchEnemyState state)
    {
        switch (state.Behavior)
        {
            case OwtchBehaviorState.MovingLeft:
                MoveOwtch(slot, state, direction: HorizontalDirection.Left);
                MaybeMakeOwtchSink(slot, state);
                return;

            case OwtchBehaviorState.MovingRight:
                MoveOwtch(slot, state, direction: HorizontalDirection.Right);
                MaybeMakeOwtchSink(slot, state);
                return;

            case OwtchBehaviorState.Underground:
                state.UndergroundTimer = state.UndergroundTimer.Decrement().Remaining;
                if (state.UndergroundTimer.IsZero)
                    state.Behavior = OwtchBehaviorState.Rising;
                return;

            case OwtchBehaviorState.Sinking:
                slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + 1);
                // Sinking starts from zero, so the depth stays within one through sixteen and the game's signed
                // word comparison is an ordinary one.
                state.BurialDepth++;
                if (state.BurialDepth >= OwtchMaximumBurialDepth)
                {
                    state.Behavior = OwtchBehaviorState.Underground;
                    state.UndergroundTimer = WrappingCountdown.FromRaw(OwtchPatrol.UndergroundTimer(
                        slot.SpawnSettings<OwtchSpawnSettings>().UndergroundTimerIndex));
                }
                return;

            case OwtchBehaviorState.Rising:
                slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole - 1);
                if (state.RiseOnePixel())
                {
                    // The underground wait samples the random seed's high byte without advancing the RNG. The
                    // gameplay runtime supplies that read seam; isolated callers without it
                    // retain the established advancing fallback used by other translated AI.
                    int random = _room.Random.Read();
                    state.Behavior = OwtchMovementDefinitions.MoveRightChoice.IsSetIn(random)
                        ? OwtchBehaviorState.MovingRight
                        : OwtchBehaviorState.MovingLeft;
                }
                return;

            case OwtchBehaviorState.TurningAtLeftBound:
                // Owtch does not move this update; it restarts the right-facing animation, whose first cue sets it
                // walking right.
                SetOwtchProgram(slot, state, OwtchProgram.MovingRight);
                return;

            default:
                throw new InvalidDataException(
                    $"Owtch state {state.Behavior} is not translated.");
        }
    }

    /// <summary>Moves the Owtch with the game's carry-sensitive additions.</summary>
    private static void MoveOwtch(
        RoomEnemySlot slot,
        OwtchEnemyState state,
        HorizontalDirection direction)
    {
        // The fraction's carry and the whole pixels' wraparound match the game's paired additions.
        slot.Position = slot.Position.AdvanceX(direction == HorizontalDirection.Right ? state.RightVelocity : state.LeftVelocity);

        if (direction != HorizontalDirection.Right)
        {
            if ((slot.Position.Whole - state.MinimumPosition).X >= 0)
                return;

            // Unlike the right bound, the left bound does not turn Owtch at once: it spends the next update turning.
            state.Behavior = OwtchBehaviorState.TurningAtLeftBound;
            return;
        }

        if ((slot.Position.Whole - state.MaximumPosition).X >= 0)
            state.Behavior = OwtchBehaviorState.MovingLeft;
    }

    /// <summary>Runs Owtch's six-in-256 per-frame burial gate.</summary>
    private void MaybeMakeOwtchSink(RoomEnemySlot slot, OwtchEnemyState state)
    {
        int random = _room.Random.Generate();
        int decision = WordWrap.LowByte(SharedRandomState.LowerByte(random) + WordWrap.LowByte(slot.FrameCounter));
        if (decision < 6)
            state.Behavior = OwtchBehaviorState.Sinking;
    }

    /// <summary>Animation cues for left and right: select the moving function index.</summary>
    private static void SetOwtchMoving(OwtchEnemyState state, OwtchBehaviorState moving) =>
        state.Behavior = moving;

    /// <summary>
    /// Owtch's shell takes common damage only while it walks left or turns at the left bound; every other state is
    /// immune.
    /// </summary>
    private static bool OwtchAcceptsOrdinaryShot(OwtchEnemyState state) =>
        state.Behavior is OwtchBehaviorState.MovingLeft or OwtchBehaviorState.TurningAtLeftBound;

    /// <summary>Restarts the selected facing animation at its first step.</summary>
    private static void SetOwtchProgram(RoomEnemySlot slot, OwtchEnemyState state, OwtchProgram program)
    {
        state.Program = AnimationProgramCursor<OwtchProgram>.Start(program);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Allows common bomb damage only while Owtch occupies vulnerable movement state zero.</summary>
    internal static void AddOwtchBombReactions(EnemySharedReactions table)
    {
        // Owtch's shell accepts common damage only during its vulnerable state zero.
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Owtch &&
                                                       c.SelectedShotAi == EnemyReaction.OwtchShot)
        {
            Rejects = c => !OwtchAcceptsOrdinaryShot(c.Enemy.RequireSpeciesState<OwtchEnemyState>()),
        });
    }

    /// <summary>Ports Owtch's main AI.</summary>
    internal void RunOwtchMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunOwtchMain(slot, slot.RequireSpeciesState<OwtchEnemyState>());
    }

    /// <summary>Advances Owtch's typed animation.</summary>
    internal static void AdvanceOwtchTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, OwtchAnimations.Programs, OwtchFrameDefinitions.ShownFrames.Of,
            static (OwtchEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyOwtchCue(state, cue));

    /// <summary>Preserves collision marking while rejecting ordinary shots against Owtch's shell.</summary>
    internal static void AddOwtchShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Owtch &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.OwtchShot)
        {
            // Owtch's private shot callback returns before common shot AI unless signed(state - 1) is negative.
            // The collision prelude has already marked ordinary non-plasma shots as collided at this point,
            // but it has not converted them into an impact animation. Preserve that split so the shell is genuinely
            // immune while moving right, sinking, buried, or rising.
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (OwtchAcceptsOrdinaryShot(c.Enemy.RequireSpeciesState<OwtchEnemyState>()))
                    return false;
                if (c.MarksCollision)
                    c.Projectile.IsSpent = true;
                return true;
            },
        });
    }
}
