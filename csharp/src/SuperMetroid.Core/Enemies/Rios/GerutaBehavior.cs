using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The Norfair rio species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class GerutaBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Geruta];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        Ensure.NotNull(place).SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower
            ? new GerutaFollowerEntity(this, occupant, place, common)
            : new GerutaEntity(this, occupant, place, common);

    /// <summary>Provides the shared RNG advanced by every parent and flame follower.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves horizontal and vertical contact during the dive and return.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>The room's enemy places, where each flame follower finds its parent when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The parent each flame follower found when it was placed, which its room lists right before it.</summary>
    private readonly EnemyPartLinks<RoomEnemyPlace> _parents = new();

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Captures the room RNG, terrain, and enemy places needed by Rio AI.</summary>
    internal GerutaBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyPlaces places, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _terrain = terrain;
        _places = places;
    }

    /// <summary>Initializes a flame follower: it finds its parent, listed right before it, and starts its ascending flames.</summary>
    internal void InitializeGerutaFollower(RoomEnemyPlace place)
    {
        _parents.Place(place, _places.ListedBefore(place, 1));
        ValidateGerutaParent(place);
        place.SetSpeciesState(new GerutaFollowerState
        {
            Playback = new(GerutaAnimations.StartingWith(GerutaSegment.FlamesAscending), AnimationPlace.StartOfAnimation),
        });
    }

    /// <summary>Initializes a Geruta parent, idle and waiting for its chance to attack.</summary>
    internal static void InitializeGeruta(RoomEnemyPlace place)
    {
        place.SetSpeciesState(new GerutaEnemyState(GerutaAction.WaitForAttackOpportunity)
        {
            FollowerYOffset = 0,
            Flames = GerutaFlames.Ascending,
            Playback = new(GerutaAnimations.StartingWith(GerutaSegment.Idle), AnimationPlace.StartOfAnimation),
        });
    }

    /// <summary>
    /// Runs the Geruta parent's main AI and each action it runs.
    /// Every parent, like every follower, advances the shared enemy RNG before it acts.
    /// </summary>
    private void RunGerutaMain(
        RoomEnemyPlace place,
        GerutaEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        int random = _room.Random.Generate();

        switch (state.Action)
        {
            case GerutaAction.WaitForAttackOpportunity:
                // The Rio attacks only when Samus is horizontally within its trigger distance and this update's random
                // number does not hold it back. A Rio that does not attack, and whose swoop's start has finished, rests
                // idle again.
                if (RioLaunchDefinitions.AttackHeldBack.IsTakenBy(random) ||
                    Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) >= RioLaunchDefinitions.NorfairTriggerDistance)
                {
                    if (state.Finish.Take())
                    {
                        RestGeruta(place, state);
                    }
                    return;
                }

                // The same random number picks the dive's fast or slow downward launch.
                state.Velocity = new PixelVelocity<RoomPixels>(
                    RioLaunchDefinitions.NorfairXVelocity.Toward(RioLaunches.SideOfSamus(place, samus)),
                    RioLaunches.NorfairYVelocity(random));
                StartGerutaSwoopAnimation(
                    place,
                    state,
                    GerutaSegment.StartDescending);
                state.Action = GerutaAction.WaitForTakeoffAnimation;
                return;

            case GerutaAction.WaitForTakeoffAnimation:
                if (!state.Finish.Take())
                    return;
                StartGerutaSwoopAnimation(
                    place,
                    state,
                    GerutaSegment.Descending);
                state.Action = GerutaAction.Dive;
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.GerutaDiveSound, priority: SoundRequestPriority.Normal);
                return;

            case GerutaAction.Dive:
                MoveGerutaHorizontally(level, place, state);
                if (_terrain.MoveEnemyVertically(
                        level,
                        place,
                        state.Velocity.Y.ToSixteenSixteenDelta()))
                {
                    BeginGerutaReturn(place, state);
                    return;
                }

                state.Velocity = state.Velocity.WithY(state.Velocity.Y - RioLaunchDefinitions.NorfairGravity);
                if (state.Velocity.Y.IsNegative)
                    BeginGerutaReturn(place, state);
                return;

            case GerutaAction.ReturnToPerch:
                MoveGerutaHorizontally(level, place, state);
                if (_terrain.MoveEnemyVertically(
                        level,
                        place,
                        state.Velocity.Y.ToSixteenSixteenDelta()))
                {
                    state.Action = GerutaAction.FinishLanding;
                    return;
                }

                state.Velocity = state.Velocity.WithY(state.Velocity.Y - RioLaunchDefinitions.NorfairGravity);
                if (state.Finish.Take())
                {
                    StartGerutaSwoopAnimation(
                        place,
                        state,
                        GerutaSegment.Ascending);
                }
                return;

            case GerutaAction.FinishLanding:
                // Finishing the landing takes one update and changes only the action: the animation the climb left
                // keeps playing, its flames with it, while the Rio waits for another random attack.
                state.Action = GerutaAction.WaitForAttackOpportunity;
                return;

            default:
                throw new InvalidDataException(
                    $"Norfair Rio has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Carries out one cue of the Geruta's animations: the end of its swoop's start, or a flame placement.</summary>
    private static void ApplyGerutaCue(GerutaEnemyState state, GerutaCue cue)
    {
        switch (cue)
        {
            case GerutaCue.SwoopStartFinished:
                state.Finish.Raise();
                break;
            case GerutaCue.PlaceFlames placeFlames:
                state.FollowerYOffset = placeFlames.Below;
                state.Flames = placeFlames.Flames;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(GerutaCue)} {cue}.");
        }
    }

    /// <summary>Copies parent freeze, visibility, animation, and flame offset to the adjacent follower.</summary>
    private void FollowGerutaParent(
        RoomEnemyPlace follower,
        GerutaFollowerState followerState)
    {
        RoomEnemyPlace parent = GetPrecedingGerutaPlace(follower);
        if (parent.Health == 0)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Deleted);
            return;
        }
        // The pair is checked when the follower is placed; a parent deleted without a kill is read as it left its place and
        // its animation (RoomEnemyPlace.FoundState).
        GerutaEnemyState parentState = parent.FoundState<GerutaEnemyState>() ?? throw new InvalidOperationException(
            $"Rio flame follower {follower.Ordinal} finds no Rio in its parent's place {parent.Ordinal}.");
        follower.Freeze.Restart(parent.Freeze.UpdatesLeft);
        if (parent.Freeze.IsRunning || parentState.Idles)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Hidden);
            return;
        }

        // The follower reads the parent's own state: the flames it placed and where. Its drawing position follows the
        // parent's live position.
        GerutaSegment followerFlames = parentState.Flames switch
        {
            GerutaFlames.Descending => GerutaSegment.FlamesDescending,
            GerutaFlames.Ascending => GerutaSegment.FlamesAscending,
            _ => throw new InvalidOperationException($"Undefined {nameof(GerutaFlames)} {parentState.Flames}."),
        };
        StartGerutaAnimation(follower, followerState, followerFlames);
        follower.Presence = follower.Presence.Without(EnemyPresence.Hidden);
        follower.Position = follower.Position.WithWhole(parent.Position.Whole + new Offset16<RoomPixels>(0, parentState.FollowerYOffset));
    }

    /// <summary>Reverses vertical travel, installs the ascent animation, and enters the return state.</summary>
    private static void BeginGerutaReturn(
        RoomEnemyPlace place,
        GerutaEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(RioLaunchDefinitions.ReturnYVelocity);
        StartGerutaSwoopAnimation(
            place,
            state,
            GerutaSegment.StartAscending);
        state.Action = GerutaAction.ReturnToPerch;
    }

    /// <summary>Moves across room terrain and reverses horizontal speed on contact.</summary>
    private void MoveGerutaHorizontally(
        RoomLevelData level,
        RoomEnemyPlace place,
        GerutaEnemyState state)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                state.Velocity.X.ToSixteenSixteenDelta()))
        {
            state.Velocity = state.Velocity.ReversedX();
        }
    }

    /// <summary>Returns a parent to its idle animation, restarting it only when it changes (#2145), which hides its flames (#2168).</summary>
    private static void RestGeruta(RoomEnemyPlace place, GerutaEnemyState state) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, GerutaAnimations.StartingWith(GerutaSegment.Idle));

    /// <summary>
    /// Starts one of a parent's swoop animations, restarting it only when it changes (#2145); its flames show while it
    /// plays (#2168).
    /// </summary>
    private static void StartGerutaSwoopAnimation(
        RoomEnemyPlace place,
        GerutaEnemyState state,
        GerutaSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, GerutaAnimations.StartingWith(segment));

    /// <summary>Restarts a flame follower's animation only when it changes (#2145).</summary>
    private static void StartGerutaAnimation(
        RoomEnemyPlace place,
        GerutaFollowerState state,
        GerutaSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, GerutaAnimations.StartingWith(segment));

    /// <summary>The parent the follower found when it was placed, the enemy its room lists directly before it.</summary>
    private RoomEnemyPlace GetPrecedingGerutaPlace(RoomEnemyPlace follower) => _parents.Of(follower);

    /// <summary>Requires a flame follower to directly follow a primary Geruta population entry.</summary>
    private void ValidateGerutaParent(RoomEnemyPlace follower)
    {
        RoomEnemyPlace parent = GetPrecedingGerutaPlace(follower);
        if (parent.Entity is not GerutaEntity)
        {
            throw new InvalidDataException(
                $"Norfair Rio follower in place {follower.Ordinal} must immediately follow " +
                "a parent of the same definition.");
        }
    }

    /// <summary>Runs a Geruta flame follower's main AI: it advances the shared enemy RNG, as every parent does, then follows its parent.</summary>
    internal void RunGerutaFollowerMainAi(RoomEnemyPlace place)
    {
        _room.Random.Generate();
        FollowGerutaParent(place, place.RequireSpeciesState<GerutaFollowerState>());
    }

    /// <summary>Runs a Geruta parent's main AI.</summary>
    internal void RunGerutaMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        RunGerutaMain(
            place,
            place.RequireSpeciesState<GerutaEnemyState>(),
            frame.Samus,
            frame.Level);

    /// <summary>Advances a Geruta flame follower's typed animation, which raises no cue.</summary>
    internal static void AdvanceGerutaFollowerTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, GerutaAnimations.Segments,
            static (GerutaFollowerState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A Norfair Rio flame follower's cue-free animation raised {cue}."));

    /// <summary>Advances a Geruta parent's typed animation, whose cues it carries out.</summary>
    internal static void AdvanceGerutaTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, GerutaAnimations.Segments,
            static (GerutaEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (state, cue) =>
            {
                ApplyGerutaCue(state, cue);
                return null;
            });
}
