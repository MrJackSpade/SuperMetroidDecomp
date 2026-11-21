using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The Hoppers species owner: its per-room state, AI behaviours and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class HoppersBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    /// <remarks>Each hopper definition's creation carries the kind <see cref="HopperKindDefinitions.Kinds"/> gives it.</remarks>
    [field: NonSerialized]
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= new Dictionary<EnemyDefinitionId, EnemyEntityCreation>
    {
        [EnemyDefinitionId.Sidehopper] = (occupant, place, common) => Create(occupant, place, common, HopperKindDefinitions.Kinds[EnemyDefinitionId.Sidehopper]),
        [EnemyDefinitionId.SidehopperLarge] = (occupant, place, common) => Create(occupant, place, common, HopperKindDefinitions.Kinds[EnemyDefinitionId.SidehopperLarge]),
        [EnemyDefinitionId.SidehopperTourian] = (occupant, place, common) => Create(occupant, place, common, HopperKindDefinitions.Kinds[EnemyDefinitionId.SidehopperTourian]),
        [EnemyDefinitionId.DessgeegaLarge] = (occupant, place, common) => Create(occupant, place, common, HopperKindDefinitions.Kinds[EnemyDefinitionId.DessgeegaLarge]),
        [EnemyDefinitionId.Dessgeega] = (occupant, place, common) => Create(occupant, place, common, HopperKindDefinitions.Kinds[EnemyDefinitionId.Dessgeega]),
    };

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and the entry it is spawned from.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    /// <param name="kind">The kind of hopper the occupant's definition is.</param>
    private EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, HopperKind kind) =>
        EnemyOccupant.Of(new HoppersEntity(this, occupant, place, common, kind));

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

    /// <summary>What <see cref="ApplyHopperCue"/> does for each <see cref="HopperCueKind"/> member, each member's own arm.</summary>
    private static IdentityTable<HopperCueKind, Action<RoomEnemyPlace, HopperEnemyState>> ApplyHopperCueForHopperCueKind => field ??= new(new Dictionary<HopperCueKind, Action<RoomEnemyPlace, HopperEnemyState>>
    {
        [HopperCueKind.ProcessOffScreen] = static (place, state) => place.Processing = place.Processing.With(EnemyProcessing.OffScreen),
        [HopperCueKind.StopProcessingOffScreen] = static (place, state) => place.Processing = place.Processing.Without(EnemyProcessing.OffScreen),
        [HopperCueKind.ReadyToHop] = static (place, state) => state.ReadyToHop = true,
    });

    /// <summary>Carries out one cue of the hoppers' animations.</summary>
    private void ApplyHopperCue(RoomEnemyPlace place, HopperEnemyState state, HopperCue cue)
    {
        cue.React(ApplyHopperCueReactions, new ApplyHopperCueContext(place, state, this));
    }

    /// <summary>What <see cref="ApplyHopperCue"/> does on each <see cref="HopperCue"/>, built once (#2553).</summary>
    private static HopperCueReactions<ApplyHopperCueContext> ApplyHopperCueReactions { get; } = new(
        PlaySound: static (sound, context) => context.Owner._audio.QueueEnemySound(sound.Sound, priority: SoundRequestPriority.Normal),
        Signal: static (signal, context) =>
        {
            ApplyHopperCueForHopperCueKind[signal.Kind](context.Place, context.State);
        });

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Place">The place the cases read.</param>
    /// <param name="State">The state the cases read.</param>
    /// <param name="Owner">The <see cref="HoppersBehavior"/> reacting.</param>
    private readonly record struct ApplyHopperCueContext(RoomEnemyPlace Place, HopperEnemyState State, HoppersBehavior Owner);

    /// <summary>Initializes the Hopper.</summary>
    internal void InitializeHopper(RoomEnemyPlace place, RoomEnemyOccupant occupant, HopperKind kind)
    {
        // The placement's reseed is followed by a draw it never reads, which later consumers observe.
        _room.Random.Reseed(SpeciesRandomReseed.HopperPlacement);
        _room.Random.Generate();

        var state = new HopperEnemyState(HopperAction.ChooseHopSize)
        {
            Kind = kind,
        };
        place.SetSpeciesState(state);

        SetHopperAnimation(place, state, ReadHopperAnimation(occupant, state, HopperPhase.Landed));
    }

    /// <summary>What <see cref="RunHopperMain"/> does for each <see cref="HopperAction"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<HopperAction, Action<RoomEnemyOccupant, RoomEnemyPlace, SamusState, RoomLevelData, HopperEnemyState>> RunHopperMainForHopperAction => field ??= new(new Dictionary<HopperAction, Action<RoomEnemyOccupant, RoomEnemyPlace, SamusState, RoomLevelData, HopperEnemyState>>
    {
        [HopperAction.ChooseHopSize] = (_, place, samus, level, state) =>
        {
            // Choosing between the two hops takes a whole update.
            state.HopSize = HopperDecisions.HopsBig.IsTakenBy(_room.Random.Generate()) ? HopperHopSize.Big : HopperHopSize.Small;
            state.Action = HopperAction.PrepareHop;
        },
        [HopperAction.PrepareHop] = (_, place, samus, level, state) => PrepareHopperHop(state),
        [HopperAction.ChooseDirection] = (_, place, samus, level, state) => ChooseHopperDirection(place, state, samus),
        [HopperAction.StartHop] = (occupant, place, samus, level, state) => StartHopperJump(occupant, place, state),
        [HopperAction.Landed] = (occupant, place, samus, level, state) => LandHopper(occupant, place, state),
        [HopperAction.HoppingAway] = (occupant, place, samus, level, state) => RunHopperHopAway(occupant, place, state, level),
        [HopperAction.Returning] = (occupant, place, samus, level, state) => RunHopperReturn(occupant, place, state, level),
        [HopperAction.WaitToHop] = (_, place, samus, level, state) =>
        {
            if (state.ReadyToHop)
            {
                state.ReadyToHop = false;
                state.Action = HopperAction.ChooseHopSize;
            }
        },
    });

    /// <summary>Runs the Hopper's current action.</summary>
    private void RunHopperMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        SamusState samus,
        RoomLevelData level)
    {
        HopperEnemyState state = place.RequireSpeciesState<HopperEnemyState>();
        RunHopperMainForHopperAction[state.Action](occupant, place, samus, level, state);
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
        state.Heading = samus.Position.X.Whole < place.Position.X.Whole
            ? HorizontalDirection.Left
            : HorizontalDirection.Right;
        state.Action = HopperAction.StartHop;
    }

    /// <summary>Starts the jumping animation and the hop along the chosen heading.</summary>
    internal static void StartHopperJump(RoomEnemyOccupant occupant, RoomEnemyPlace place, HopperEnemyState state)
    {
        SetHopperAnimation(place, state, ReadHopperAnimation(occupant, state, HopperPhase.Jumping));
        state.Action = HopperAction.HoppingAway;
    }

    /// <summary>Starts the landed animation and returns the actor to its wait-for-hop action.</summary>
    internal static void LandHopper(RoomEnemyOccupant occupant, RoomEnemyPlace place, HopperEnemyState state)
    {
        SetHopperAnimation(place, state, ReadHopperAnimation(occupant, state, HopperPhase.Landed));
        state.Action = HopperAction.WaitToHop;
    }

    /// <summary>
    /// Advances the half of a hop away from the surface with vertical-first collision. Striking the room beyond the hop, or
    /// a wall, turns the hopper around and back toward its surface; passing the apex turns it back from the curve's start.
    /// </summary>
    private void RunHopperHopAway(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        HopperEnemyState state,
        RoomLevelData level)
    {
        // A floor hopper rises away from the floor; a ceiling hopper drops away from the ceiling.
        VerticalDirection away = HopperSurfaces.AwayFrom(SurfaceOf(occupant));
        // A collision with the room beyond reverses X and turns the hop back without horizontal motion that update.
        if (_terrain.MoveEnemyVertically(level, place, state.YSpeedCurvePlace.Speed.Toward(away)))
        {
            state.Heading = state.Heading.Reversed();
            state.Action = HopperAction.Returning;
            return;
        }

        if (MoveHopperHorizontally(place, state, level))
        {
            // A hopper that meets a wall on its way out turns back at once, as at the room beyond.
            state.Action = HopperAction.Returning;
            return;
        }

        // A step back past the curve's start is the apex: the hop turns back toward the surface from the curve's start.
        QuadraticSpeedCurvePlace next = state.YSpeedCurvePlace - state.YSpeedCurvePlaceStep;
        if (next.IsNegative)
        {
            state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
            state.Action = HopperAction.Returning;
        }
        else
        {
            state.YSpeedCurvePlace = next;
        }
    }

    /// <summary>
    /// Advances the half of a hop back to the surface with vertical-first collision: meeting the surface lands the hopper
    /// at once, and a wall only turns it around.
    /// </summary>
    private void RunHopperReturn(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        HopperEnemyState state,
        RoomLevelData level)
    {
        // A floor hopper drops back to the floor; a ceiling hopper rises back to the ceiling.
        VerticalDirection back = HopperSurfaces.AwayFrom(SurfaceOf(occupant)).Reversed();
        if (_terrain.MoveEnemyVertically(level, place, state.YSpeedCurvePlace.Speed.Toward(back)))
        {
            state.Action = HopperAction.Landed;
            return;
        }

        MoveHopperHorizontally(place, state, level);

        // The returning hop speeds up no further than its fastest mark.
        state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Min(
            state.YSpeedCurvePlace + state.YSpeedCurvePlaceStep,
            HopperHopDefinitions.FastestReturnPlace);
    }

    /// <summary>Moves the hopper its hop's horizontal speed along its heading, turning it around at a wall it meets.</summary>
    /// <returns>Whether the hopper met a wall.</returns>
    private bool MoveHopperHorizontally(RoomEnemyPlace place, HopperEnemyState state, RoomLevelData level)
    {
        SignedSixteenSixteen horizontalDisplacement = SignedSixteenSixteen.FromWholePixels(state.XVelocity);
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, horizontalDisplacement))
            return false;
        state.Heading = state.Heading.Reversed();
        return true;
    }

    /// <summary>The surface the hopper's placement mounts it on (#2474), which decides which way is up for its hops.</summary>
    private static HopperSurface SurfaceOf(RoomEnemyOccupant occupant) => occupant.SpawnSettings<HopperSpawnSettings>().Surface;

    /// <summary>Selects the animation for this kind on its surface in its jumping or landing half.</summary>
    private static HopperSegment ReadHopperAnimation(RoomEnemyOccupant occupant, HopperEnemyState state, HopperPhase phase) =>
        HopperAnimationChoices.Animation(state.Kind, SurfaceOf(occupant), phase);

    /// <summary>Starts a new animation, its first step playing on the next update.</summary>
    private static void SetHopperAnimation(
        RoomEnemyPlace place,
        HopperEnemyState state,
        HopperSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, HopperAnimations.StartingWith(segment));
    }

    /// <summary>Runs the hoppers' main AI.</summary>
    internal void RunHoppersMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunHopperMain(occupant, place, frame.Samus, frame.Level);
    }

    /// <summary>Advances Hoppers's typed animation.</summary>
    internal void AdvanceHoppersTypedAnimation(RoomEnemyPlace place)
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
