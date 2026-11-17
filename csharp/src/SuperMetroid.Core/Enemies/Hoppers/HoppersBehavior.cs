using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The Hoppers species owner: its per-room state, AI behaviours and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class HoppersBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Dessgeega, EnemyDefinitionId.DessgeegaLarge, EnemyDefinitionId.Sidehopper, EnemyDefinitionId.SidehopperLarge, EnemyDefinitionId.SidehopperTourian];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new HoppersEntity(this, place, common);

    /// <summary>Supplies the shared RNG seed consumed during Hopper initialization.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves vertical and horizontal collisions in both floor and ceiling hops.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds the five Hopper definitions to shared room RNG and terrain movement.</summary>
    internal HoppersBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _terrain = terrain;
    }

    /// <summary>Carries out one cue of the hoppers' animations.</summary>
    private HopperSegment? ApplyHopperCue(RoomEnemyPlace place, HopperEnemyState state, HopperCue cue)
    {
        switch (cue)
        {
            case HopperCue.Signal { Kind: HopperCueKind.ProcessOffScreen }:
                place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
                break;
            case HopperCue.Signal { Kind: HopperCueKind.StopProcessingOffScreen }:
                place.Processing = place.Processing.Without(EnemyProcessing.OffScreen);
                break;
            case HopperCue.PlaySound sound:
                _audio.QueueEnemySound(sound.Sound, priority: SoundRequestPriority.Normal);
                break;
            case HopperCue.Signal { Kind: HopperCueKind.ReadyToHop }:
                state.ReadyToHop = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined hopper cue {cue}.");
        }
        return null;
    }

    /// <summary>Global RNG seed written when a Hopper initializes.</summary>
    private const int HopperRandomSeed = 37;
    /// <summary>Terminal place along the quadratic speed curve, mark 64, that caps the falling half of a hop.</summary>
    private static readonly QuadraticSpeedCurvePlace MaximumHopperYSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(64);

    /// <summary>Initializes the Hopper.</summary>
    internal void InitializeHopper(RoomEnemyPlace place)
    {
        HopperKind kind = HopperAnimationChoices.KindOf(place.DefinitionId);
        // The setup overwrites the global RNG seed and consumes one generated value
        // even though it never reads that value. This changes later enemy randomness and is
        // therefore observable whenever a hopper appears earlier in population order.
        _room.Random.Seed(HopperRandomSeed);
        _room.Random.Generate();

        var state = new HopperEnemyState(HopperAction.ChooseHopSize)
        {
            Falling = false,
            ReadyToHop = false,
            Kind = kind,
            UpsideDown = place.SpawnSettings<HopperSpawnSettings>().Mounting == HopperMounting.Ceiling,
        };
        place.SetSpeciesState(state);

        SetHopperAnimation(place, state, ReadHopperAnimation(state, SurfaceOf(state), HopperPhase.Landed));

        // Every Hopper definition authors the same small and big hops, so the definition selects nothing here; the
        // two hop sizes' starting speeds are still calculated independently.
        state.SmallHopInitialYSpeedCurvePlace = HopperHopDefinitions.SmallHop.StartPlace;
        state.BigHopInitialYSpeedCurvePlace = HopperHopDefinitions.BigHop.StartPlace;
    }

    /// <summary>Runs the Hopper's current action.</summary>
    private void RunHopperMain(
        RoomEnemyPlace place,
        SamusState samus,
        RoomLevelData level)
    {
        HopperEnemyState state = place.RequireSpeciesState<HopperEnemyState>();
        switch (state.Action)
        {
            case HopperAction.ChooseHopSize:
                // The game consumes an entire frame choosing one of two hop setups.
                state.Action = HopperDecisions.BigHop.IsSetIn(_room.Random.Generate())
                    ? HopperAction.PrepareBigHop
                    : HopperAction.PrepareSmallHop;
                return;
            case HopperAction.PrepareSmallHop:
                PrepareHopperHop(state, HopperHopSize.Small);
                return;
            case HopperAction.PrepareBigHop:
                PrepareHopperHop(state, HopperHopSize.Big);
                return;
            case HopperAction.ChooseDirection:
                ChooseHopperDirection(place, state, samus);
                return;
            case HopperAction.StartBackwardHop:
                StartHopperJump(place, state, HopperHopDirection.Backward);
                return;
            case HopperAction.StartForwardHop:
                StartHopperJump(place, state, HopperHopDirection.Forward);
                return;
            case HopperAction.Landed:
                LandHopper(place, state);
                return;
            case HopperAction.Jumping:
                RunHopperMovement(place, state, level, SurfaceOf(state));
                return;
            case HopperAction.WaitToHop:
                if (state.ReadyToHop)
                {
                    state.ReadyToHop = false;
                    state.Action = HopperAction.ChooseHopSize;
                }
                return;
            default:
                throw new InvalidDataException(
                    $"Hopper has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Selects big or small quadratic-speed parameters and routes to the direction choice for the next hop.</summary>
    private static void PrepareHopperHop(HopperEnemyState state, HopperHopSize size)
    {
        QuadraticCurveClimb hop = size == HopperHopSize.Big ? HopperHopDefinitions.BigHop : HopperHopDefinitions.SmallHop;
        state.YSpeedCurvePlaceStep = hop.StepPerPass;
        state.XVelocity = HopperHopDefinitions.HorizontalSpeed;
        state.YSpeedCurvePlace = size == HopperHopSize.Big
            ? state.BigHopInitialYSpeedCurvePlace
            : state.SmallHopInitialYSpeedCurvePlace;
        state.Action = HopperAction.ChooseDirection;
    }

    /// <summary>Chooses the hop's direction from Samus's signed horizontal position relative to the actor.</summary>
    private static void ChooseHopperDirection(
        RoomEnemyPlace place,
        HopperEnemyState state,
        SamusState samus)
    {
        state.Action = CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) < 0
            ? HopperAction.StartBackwardHop
            : HopperAction.StartForwardHop;
    }

    /// <summary>Installs the jumping animation and negates X velocity only for the leftward branch.</summary>
    internal static void StartHopperJump(
        RoomEnemyPlace place,
        HopperEnemyState state,
        HopperHopDirection direction)
    {
        // “Backward” is the leftward branch selected when Samus is left of the actor. The
        // initial setup always gives three pixels rightward, so only this branch reverses the speed.
        if (direction == HopperHopDirection.Backward)
            state.XVelocity = -state.XVelocity;

        SetHopperAnimation(place, state, ReadHopperAnimation(state, SurfaceOf(state), HopperPhase.Jumping));
        state.Action = HopperAction.Jumping;
    }

    /// <summary>Starts the landed animation and returns the actor to its wait-for-hop action.</summary>
    internal static void LandHopper(RoomEnemyPlace place, HopperEnemyState state)
    {
        SetHopperAnimation(place, state, ReadHopperAnimation(state, SurfaceOf(state), HopperPhase.Landed));
        state.Action = HopperAction.WaitToHop;
    }

    /// <summary>Advances a floor or ceiling hop with vertical-first collision and phase-dependent horizontal reversal.</summary>
    private void RunHopperMovement(
        RoomEnemyPlace place,
        HopperEnemyState state,
        RoomLevelData level,
        HopperSurface surface)
    {
        bool movingAwayFromSurface = !state.Falling;
        // Has four movements: floor actors use the negative half while
        // jumping and the positive half while falling; ceiling actors do the inverse.
        // Inequality expresses that table exactly. Equality reverses both phases, making
        // each actor collide with its starting surface and then migrate to the opposite one.
        bool useNegativeSpeed = (surface == HopperSurface.Ceiling) != movingAwayFromSurface;
        SignedSixteenSixteen verticalDisplacement = state.YSpeedCurvePlace.Speed.Toward(useNegativeSpeed ? EnemySpeedSign.Negative : EnemySpeedSign.Positive);

        // Every movement entry resolves Y first. A collision while rising reverses
        // X and changes to the falling half without performing horizontal motion that frame.
        // A collision while falling installs Landed and likewise ends the frame immediately.
        if (_terrain.MoveEnemyVertically(level, place, verticalDisplacement))
        {
            if (movingAwayFromSurface)
            {
                state.XVelocity = -state.XVelocity;
                state.Falling = true;
            }
            else
            {
                state.Falling = false;
                state.Action = HopperAction.Landed;
            }
            return;
        }

        SignedSixteenSixteen horizontalDisplacement = SignedSixteenSixteen.FromWholePixels(state.XVelocity);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, horizontalDisplacement))
        {
            state.XVelocity = -state.XVelocity;
            if (movingAwayFromSurface)
            {
                // Rising code treats a wall exactly like a ceiling/floor interruption and
                // immediately enters the falling half. Falling code only reverses X.
                state.Falling = true;
                return;
            }
        }

        if (movingAwayFromSurface)
        {
            // The game stores the moved-back place and checks whether it is negative: a place before the curve's start is the apex,
            // replaced by the curve's start as the hop turns back toward the surface.
            QuadraticSpeedCurvePlace next = state.YSpeedCurvePlace - state.YSpeedCurvePlaceStep;
            if (next.IsNegative)
            {
                state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
                state.Falling = true;
            }
            else
            {
                state.YSpeedCurvePlace = next;
            }
            return;
        }

        // The game caps the returning mark by whether its wrapped difference from the cap is negative; both stay
        // far inside the signed range, so that is the ordinary minimum.
        state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Min(
            state.YSpeedCurvePlace + state.YSpeedCurvePlaceStep,
            MaximumHopperYSpeedCurvePlace);
    }

    /// <summary>Selects the animation for this kind's facing and jumping or landing half.</summary>
    private static HopperSegment ReadHopperAnimation(
        HopperEnemyState state,
        HopperSurface surface,
        HopperPhase phase) =>
        HopperAnimationChoices.Animation(state.Kind, surface, phase);

    /// <summary>The surface its spawn settings placed this hopper on.</summary>
    private static HopperSurface SurfaceOf(HopperEnemyState state) =>
        state.UpsideDown ? HopperSurface.Ceiling : HopperSurface.Floor;

    /// <summary>Starts a new animation with a one-tick countdown.</summary>
    private static void SetHopperAnimation(
        RoomEnemyPlace place,
        HopperEnemyState state,
        HopperSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, HopperAnimations.StartingWith(segment));
    }

    /// <summary>Runs the hoppers' main AI.</summary>
    internal void RunHoppersMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunHopperMain(place, frame.Samus, frame.Level);
    }

    /// <summary>Advances Hoppers's typed animation.</summary>
    internal void AdvanceHoppersTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
    {
        if (!place.AnimationClock.Expire())
            return;
        HopperEnemyState state = place.RequireSpeciesState<HopperEnemyState>();
        AnimationPlayback<HopperSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, HopperAnimations.Segments, ref playback,
            cue => ApplyHopperCue(place, state, cue) is { } target
                ? new AnimationContinuation<HopperSegment>(HopperAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }
}
