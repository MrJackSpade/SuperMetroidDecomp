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
    private void ApplyHopperCue(RoomEnemyPlace place, HopperEnemyState state, HopperCue cue)
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
    }

    /// <summary>Initializes the Hopper.</summary>
    internal void InitializeHopper(RoomEnemyPlace place)
    {
        HopperKind kind = HopperAnimationChoices.KindOf(place.DefinitionId);
        // The placement's reseed is followed by a draw it never reads, which later consumers observe.
        _room.Random.Reseed(SpeciesRandomReseed.HopperPlacement);
        _room.Random.Generate();

        var state = new HopperEnemyState(HopperAction.ChooseHopSize)
        {
            Falling = false,
            ReadyToHop = false,
            Kind = kind,
            Surface = place.SpawnSettings<HopperSpawnSettings>().Surface,
        };
        place.SetSpeciesState(state);

        SetHopperAnimation(place, state, ReadHopperAnimation(state, HopperPhase.Landed));
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
                // Choosing between the two hops takes a whole update.
                state.HopSize = HopperDecisions.BigHop.IsSetIn(_room.Random.Generate()) ? HopperHopSize.Big : HopperHopSize.Small;
                state.Action = HopperAction.PrepareHop;
                return;
            case HopperAction.PrepareHop:
                PrepareHopperHop(state);
                return;
            case HopperAction.ChooseDirection:
                ChooseHopperDirection(place, state, samus);
                return;
            case HopperAction.StartHop:
                StartHopperJump(place, state);
                return;
            case HopperAction.Landed:
                LandHopper(place, state);
                return;
            case HopperAction.Jumping:
                RunHopperMovement(place, state, level);
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

    /// <summary>
    /// Selects the chosen hop size's hop, which every hopper definition authors alike, and routes to the direction choice
    /// for the next hop: the hop starts at its climb's place along the speed curve.
    /// </summary>
    private static void PrepareHopperHop(HopperEnemyState state)
    {
        QuadraticCurveClimb hop = state.HopSize == HopperHopSize.Big ? HopperHopDefinitions.BigHop : HopperHopDefinitions.SmallHop;
        state.YSpeedCurvePlaceStep = hop.StepPerPass;
        state.YSpeedCurvePlace = hop.StartPlace;
        state.Action = HopperAction.ChooseDirection;
    }

    /// <summary>Heads the hop toward Samus: left when she is left of the actor, otherwise right.</summary>
    private static void ChooseHopperDirection(
        RoomEnemyPlace place,
        HopperEnemyState state,
        SamusState samus)
    {
        state.Heading = CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) < 0
            ? HorizontalDirection.Left
            : HorizontalDirection.Right;
        state.Action = HopperAction.StartHop;
    }

    /// <summary>Installs the jumping animation and starts the hop along the chosen heading.</summary>
    internal static void StartHopperJump(RoomEnemyPlace place, HopperEnemyState state)
    {
        SetHopperAnimation(place, state, ReadHopperAnimation(state, HopperPhase.Jumping));
        state.Action = HopperAction.Jumping;
    }

    /// <summary>Starts the landed animation and returns the actor to its wait-for-hop action.</summary>
    internal static void LandHopper(RoomEnemyPlace place, HopperEnemyState state)
    {
        SetHopperAnimation(place, state, ReadHopperAnimation(state, HopperPhase.Landed));
        state.Action = HopperAction.WaitToHop;
    }

    /// <summary>Advances a floor or ceiling hop with vertical-first collision and phase-dependent horizontal reversal.</summary>
    private void RunHopperMovement(
        RoomEnemyPlace place,
        HopperEnemyState state,
        RoomLevelData level)
    {
        bool movingAwayFromSurface = !state.Falling;
        // A floor hopper rises while it hops away from the floor and drops back to it; a ceiling hopper drops away from
        // the ceiling and rises back. Each moves away from its own surface first and returns to it.
        bool useNegativeSpeed = (state.Surface == HopperSurface.Ceiling) != movingAwayFromSurface;
        SignedSixteenSixteen verticalDisplacement = state.YSpeedCurvePlace.Speed.Toward(useNegativeSpeed ? EnemySpeedSign.Negative : EnemySpeedSign.Positive);

        // Every movement entry resolves Y first. A collision while rising reverses
        // X and changes to the falling half without performing horizontal motion that frame.
        // A collision while falling installs Landed and likewise ends the frame immediately.
        if (_terrain.MoveEnemyVertically(level, place, verticalDisplacement))
        {
            if (movingAwayFromSurface)
            {
                state.Heading = state.Heading.Reversed();
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
            state.Heading = state.Heading.Reversed();
            if (movingAwayFromSurface)
            {
                // A rising hopper that meets a wall starts falling at once, as at a ceiling; a falling hopper
                // only reverses.
                state.Falling = true;
                return;
            }
        }

        if (movingAwayFromSurface)
        {
            // A step back past the curve's start is the apex: the hop turns back toward the surface from the curve's start.
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

        // The returning hop speeds up no further than its fastest mark.
        state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Min(
            state.YSpeedCurvePlace + state.YSpeedCurvePlaceStep,
            HopperHopDefinitions.FastestReturnPlace);
    }

    /// <summary>Selects the animation for this kind on its surface in its jumping or landing half.</summary>
    private static HopperSegment ReadHopperAnimation(HopperEnemyState state, HopperPhase phase) =>
        HopperAnimationChoices.Animation(state.Kind, state.Surface, phase);

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
            cue =>
            {
                ApplyHopperCue(place, state, cue);
                return null;
            });
        state.Playback = playback;
    }
}
