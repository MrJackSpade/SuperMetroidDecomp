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
        HopperKind kind = HopperAnimationChoices.KindOf(place.Occupant?.DefinitionId);
        // The placement's reseed is followed by a draw it never reads, which later consumers observe.
        _room.Random.Reseed(SpeciesRandomReseed.HopperPlacement);
        _room.Random.Generate();

        var state = new HopperEnemyState(HopperAction.ChooseHopSize)
        {
            Kind = kind,
        };
        place.SetSpeciesState(state);

        SetHopperAnimation(place, state, ReadHopperAnimation(place, state, HopperPhase.Landed));
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
                state.HopSize = HopperDecisions.HopsBig.IsTakenBy(_room.Random.Generate()) ? HopperHopSize.Big : HopperHopSize.Small;
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
            case HopperAction.HoppingAway:
                RunHopperHopAway(place, state, level);
                return;
            case HopperAction.Returning:
                RunHopperReturn(place, state, level);
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
        state.Heading = samus.Position.X.Whole < place.Position.X.Whole
            ? HorizontalDirection.Left
            : HorizontalDirection.Right;
        state.Action = HopperAction.StartHop;
    }

    /// <summary>Installs the jumping animation and starts the hop along the chosen heading.</summary>
    internal static void StartHopperJump(RoomEnemyPlace place, HopperEnemyState state)
    {
        SetHopperAnimation(place, state, ReadHopperAnimation(place, state, HopperPhase.Jumping));
        state.Action = HopperAction.HoppingAway;
    }

    /// <summary>Starts the landed animation and returns the actor to its wait-for-hop action.</summary>
    internal static void LandHopper(RoomEnemyPlace place, HopperEnemyState state)
    {
        SetHopperAnimation(place, state, ReadHopperAnimation(place, state, HopperPhase.Landed));
        state.Action = HopperAction.WaitToHop;
    }

    /// <summary>
    /// Advances the half of a hop away from the surface with vertical-first collision. Striking the room beyond the hop, or
    /// a wall, turns the hopper around and back toward its surface; passing the apex turns it back from the curve's start.
    /// </summary>
    private void RunHopperHopAway(
        RoomEnemyPlace place,
        HopperEnemyState state,
        RoomLevelData level)
    {
        // A floor hopper rises away from the floor; a ceiling hopper drops away from the ceiling.
        VerticalDirection away = HopperSurfaces.AwayFrom(SurfaceOf(place));
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
        RoomEnemyPlace place,
        HopperEnemyState state,
        RoomLevelData level)
    {
        // A floor hopper drops back to the floor; a ceiling hopper rises back to the ceiling.
        VerticalDirection back = HopperSurfaces.AwayFrom(SurfaceOf(place)).Reversed();
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
    private static HopperSurface SurfaceOf(RoomEnemyPlace place) => place.SpawnSettings<HopperSpawnSettings>().Surface;

    /// <summary>Selects the animation for this kind on its surface in its jumping or landing half.</summary>
    private static HopperSegment ReadHopperAnimation(RoomEnemyPlace place, HopperEnemyState state, HopperPhase phase) =>
        HopperAnimationChoices.Animation(state.Kind, SurfaceOf(place), phase);

    /// <summary>Starts a new animation, its first step playing on the next update.</summary>
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
