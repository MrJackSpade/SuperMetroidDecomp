using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>The Owtch species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed class OwtchBehavior : IEnemySpecies, IEnemyEntityFactory
{

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
    private static void ApplyOwtchCue(OwtchEnemyState state, OwtchCue cue)
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
    }

    /// <summary>Owtch sinks sixteen whole pixels below its walking height.</summary>
    private const int OwtchMaximumBurialDepth = 16;

    /// <summary>Initializes the Owtch.</summary>
    internal static void InitializeOwtch(RoomEnemySlot slot)
    {
        OwtchSpawnSettings settings = slot.SpawnSettings<OwtchSpawnSettings>();
        var state = new OwtchEnemyState
        {
            RightVelocity = settings.Speed,
            LeftVelocity = SignedSixteenSixteen.Zero - settings.Speed,
            Behavior = settings.InitialBehavior,
            BurialDepth = 0,
            UndergroundTimer = settings.UndergroundWait,
            MinimumPosition = new(CoordinateAxis.Wrap(slot.Position.X.Whole - settings.TravelDistance), 0),
            MaximumPosition = new(CoordinateAxis.Wrap(slot.Position.X.Whole + settings.TravelDistance), 0),
        };
        slot.SetSpeciesState(state);
        SetOwtchProgram(slot, state, InitialProgram(settings.InitialBehavior));

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
                if (--state.UndergroundTimer == 0)
                    state.Behavior = OwtchBehaviorState.Rising;
                return;

            case OwtchBehaviorState.Sinking:
                slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + 1);
                // Sinking starts from zero, so the depth stays within one through sixteen and the game's signed
                // comparison is an ordinary one.
                state.BurialDepth++;
                if (state.BurialDepth >= OwtchMaximumBurialDepth)
                {
                    state.Behavior = OwtchBehaviorState.Underground;
                    state.UndergroundTimer = slot.SpawnSettings<OwtchSpawnSettings>().UndergroundWait;
                }
                return;

            case OwtchBehaviorState.Rising:
                slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole - 1);
                if (state.RiseOnePixel())
                {
                    // The underground wait samples the current random number without advancing the RNG. The
                    // gameplay runtime supplies that read seam; isolated callers without it
                    // retain the established advancing fallback used by other AI.
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

    /// <summary>
    /// The walking animation an Owtch starts in for its first behaviour. The game alternates the facing along its order
    /// of first behaviours, so walking left, buried and rising start facing left, and walking right and sinking start
    /// facing right.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="initial"/> is the turn at the left bound, which no placement starts in.</exception>
    private static OwtchProgram InitialProgram(OwtchBehaviorState initial) => initial switch
    {
        OwtchBehaviorState.MovingLeft or OwtchBehaviorState.Underground or OwtchBehaviorState.Rising => OwtchProgram.MovingLeft,
        OwtchBehaviorState.MovingRight or OwtchBehaviorState.Sinking => OwtchProgram.MovingRight,
        OwtchBehaviorState.TurningAtLeftBound => throw new InvalidDataException("No Owtch placement starts turning at its left bound."),
        _ => throw new ArgumentOutOfRangeException(nameof(initial), initial, "Undefined Owtch behaviour."),
    };

    /// <summary>Moves the Owtch with the game's 16.16 additions.</summary>
    private static void MoveOwtch(
        RoomEnemySlot slot,
        OwtchEnemyState state,
        HorizontalDirection direction)
    {
        // The fraction's overflow and the whole pixels' wraparound match the game's paired additions.
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
        if (OwtchMovementDefinitions.Sinks.IsTakenBy(_room.Random.Generate(), advancedBy: slot.FrameCounter))
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
        state.Playback = new(OwtchAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
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

    /// <summary>Runs Owtch's main AI.</summary>
    internal void RunOwtchMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunOwtchMain(slot, slot.RequireSpeciesState<OwtchEnemyState>());
    }

    /// <summary>Advances Owtch's typed animation.</summary>
    internal static void AdvanceOwtchTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, OwtchAnimations.Segments,
            static (OwtchEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyOwtchCue(state, cue);
                return null;
            });

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
