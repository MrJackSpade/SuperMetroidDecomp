using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The Norfair rio species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class NorfairRioBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Geruta];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) =>
        Ensure.NotNull(place).SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower
            ? new NorfairRioFollowerEntity(this, place, common)
            : new NorfairRioEntity(this, place, common);

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
    internal NorfairRioBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyPlaces places, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _terrain = terrain;
        _places = places;
    }

    /// <summary>Initializes a flame follower: it finds its parent, listed right before it, and starts its ascending flames.</summary>
    internal void InitializeNorfairRioFollower(RoomEnemyPlace place)
    {
        _parents.Place(place, _places.ListedBefore(place, 1));
        ValidateNorfairRioParent(place);
        place.SetSpeciesState(new NorfairRioFollowerState
        {
            Playback = new(NorfairRioAnimations.StartingWith(NorfairRioSegment.FlamesAscending), AnimationPlace.StartOfAnimation),
        });
    }

    /// <summary>Initializes a Norfair Rio parent, idle and waiting for its chance to attack.</summary>
    internal static void InitializeNorfairRio(RoomEnemyPlace place)
    {
        place.SetSpeciesState(new NorfairRioEnemyState(NorfairRioAction.WaitForAttackOpportunity)
        {
            AnimationSignal = false,
            FollowerYOffset = 0,
            Flames = NorfairRioFlames.Ascending,
            Playback = new(NorfairRioAnimations.StartingWith(NorfairRioSegment.Idle), AnimationPlace.StartOfAnimation),
        });
    }

    /// <summary>
    /// Runs the Norfair Rio parent's main AI and each action it runs.
    /// Every parent, like every follower, advances the shared enemy RNG before it acts.
    /// </summary>
    private void RunNorfairRioMain(
        RoomEnemyPlace place,
        NorfairRioEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        int random = _room.Random.Generate();

        switch (state.Action)
        {
            case NorfairRioAction.WaitForAttackOpportunity:
                // The Rio attacks only when Samus is horizontally within its trigger distance and this update's random
                // number does not hold it back. A Rio that does not attack, and whose swoop's start has finished, rests
                // idle again.
                if (RioLaunchDefinitions.AttackHeldBack.IsTakenBy(random) ||
                    Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) >= RioLaunchDefinitions.NorfairTriggerDistance)
                {
                    if (state.AnimationSignal)
                    {
                        state.AnimationSignal = false;
                        RestNorfairRio(place, state);
                    }
                    return;
                }

                // The same random number picks the dive's fast or slow downward launch.
                state.Velocity = new PixelVelocity<RoomPixels>(
                    RioLaunchDefinitions.NorfairXVelocity,
                    RioLaunches.NorfairYVelocity(random));
                if (samus.Position.X.Whole < place.Position.X.Whole)
                    state.Velocity = state.Velocity.ReversedX();
                StartNorfairRioSwoopAnimation(
                    place,
                    state,
                    NorfairRioSegment.StartDescending);
                state.Action = NorfairRioAction.WaitForTakeoffAnimation;
                return;

            case NorfairRioAction.WaitForTakeoffAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                StartNorfairRioSwoopAnimation(
                    place,
                    state,
                    NorfairRioSegment.Descending);
                state.Action = NorfairRioAction.Dive;
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.NorfairRioDiveSound, priority: SoundRequestPriority.Normal);
                return;

            case NorfairRioAction.Dive:
                MoveNorfairRioHorizontally(level, place, state);
                if (_terrain.MoveEnemyVertically(
                        level,
                        place,
                        state.Velocity.Y.ToSixteenSixteenDelta()))
                {
                    BeginNorfairRioReturn(place, state);
                    return;
                }

                state.Velocity = state.Velocity.WithY(state.Velocity.Y - RioLaunchDefinitions.NorfairGravity);
                if (state.Velocity.Y.IsNegative)
                    BeginNorfairRioReturn(place, state);
                return;

            case NorfairRioAction.ReturnToPerch:
                MoveNorfairRioHorizontally(level, place, state);
                if (_terrain.MoveEnemyVertically(
                        level,
                        place,
                        state.Velocity.Y.ToSixteenSixteenDelta()))
                {
                    state.Action = NorfairRioAction.FinishLanding;
                    return;
                }

                state.Velocity = state.Velocity.WithY(state.Velocity.Y - RioLaunchDefinitions.NorfairGravity);
                if (state.AnimationSignal)
                {
                    state.AnimationSignal = false;
                    StartNorfairRioSwoopAnimation(
                        place,
                        state,
                        NorfairRioSegment.Ascending);
                }
                return;

            case NorfairRioAction.FinishLanding:
                // Finishing the landing takes one update and changes only the action: the animation the climb left
                // keeps playing, its flames with it, while the Rio waits for another random attack.
                state.Action = NorfairRioAction.WaitForAttackOpportunity;
                return;

            default:
                throw new InvalidDataException(
                    $"Norfair Rio has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Carries out one cue of the Norfair Rio's animations: the end of its swoop's start, or a flame placement.</summary>
    private static void ApplyNorfairRioCue(NorfairRioEnemyState state, NorfairRioCue cue)
    {
        switch (cue)
        {
            case NorfairRioCue.SwoopStartFinished:
                state.AnimationSignal = true;
                break;
            case NorfairRioCue.PlaceFlames placeFlames:
                state.FollowerYOffset = placeFlames.Below;
                state.Flames = placeFlames.Flames;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(NorfairRioCue)} {cue}.");
        }
    }

    /// <summary>Copies parent freeze, visibility, animation, and flame offset to the adjacent follower.</summary>
    private void FollowNorfairRioParent(
        RoomEnemyPlace follower,
        NorfairRioFollowerState followerState)
    {
        RoomEnemyPlace parent = GetPrecedingNorfairRioPlace(follower);
        if (parent.Health == 0)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Deleted);
            return;
        }
        // The pair is checked when the follower is placed; a parent deleted without a kill is read as it left its place and
        // its animation (RoomEnemyPlace.FoundState).
        NorfairRioEnemyState parentState = parent.FoundState<NorfairRioEnemyState>() ?? throw new InvalidOperationException(
            $"Rio flame follower {follower.Ordinal} finds no Rio in its parent's place {parent.Ordinal}.");
        follower.FrozenTimer = parent.FrozenTimer;
        if (parent.FrozenTimer != 0 || parentState.Idles)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Hidden);
            return;
        }

        // The follower reads the parent's own state: the flames it placed and where. Its drawing position follows the
        // parent's live position.
        NorfairRioSegment followerList = parentState.Flames switch
        {
            NorfairRioFlames.Descending => NorfairRioSegment.FlamesDescending,
            NorfairRioFlames.Ascending => NorfairRioSegment.FlamesAscending,
            _ => throw new InvalidOperationException($"Undefined {nameof(NorfairRioFlames)} {parentState.Flames}."),
        };
        StartNorfairRioAnimation(follower, followerState, followerList);
        follower.Presence = follower.Presence.Without(EnemyPresence.Hidden);
        follower.Position = follower.Position.WithWhole(parent.Position.Whole + new Offset16<RoomPixels>(0, parentState.FollowerYOffset));
    }

    /// <summary>Reverses vertical travel, installs the ascent animation, and enters the return state.</summary>
    private static void BeginNorfairRioReturn(
        RoomEnemyPlace place,
        NorfairRioEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(RioLaunchDefinitions.ReturnYVelocity);
        StartNorfairRioSwoopAnimation(
            place,
            state,
            NorfairRioSegment.StartAscending);
        state.Action = NorfairRioAction.ReturnToPerch;
    }

    /// <summary>Moves across room terrain and reverses horizontal speed on contact.</summary>
    private void MoveNorfairRioHorizontally(
        RoomLevelData level,
        RoomEnemyPlace place,
        NorfairRioEnemyState state)
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
    private static void RestNorfairRio(RoomEnemyPlace place, NorfairRioEnemyState state) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, NorfairRioAnimations.StartingWith(NorfairRioSegment.Idle));

    /// <summary>
    /// Starts one of a parent's swoop animations, restarting it only when it changes (#2145); its flames show while it
    /// plays (#2168).
    /// </summary>
    private static void StartNorfairRioSwoopAnimation(
        RoomEnemyPlace place,
        NorfairRioEnemyState state,
        NorfairRioSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, NorfairRioAnimations.StartingWith(segment));

    /// <summary>Restarts a flame follower's animation only when it changes (#2145).</summary>
    private static void StartNorfairRioAnimation(
        RoomEnemyPlace place,
        NorfairRioFollowerState state,
        NorfairRioSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, NorfairRioAnimations.StartingWith(segment));

    /// <summary>The parent the follower found when it was placed, the enemy its room lists directly before it.</summary>
    private RoomEnemyPlace GetPrecedingNorfairRioPlace(RoomEnemyPlace follower) => _parents.Of(follower);

    /// <summary>Requires a flame follower to directly follow a primary Geruta population entry.</summary>
    private void ValidateNorfairRioParent(RoomEnemyPlace follower)
    {
        RoomEnemyPlace parent = GetPrecedingNorfairRioPlace(follower);
        if (parent.Entity is not NorfairRioEntity)
        {
            throw new InvalidDataException(
                $"Norfair Rio follower in place {follower.Ordinal} must immediately follow " +
                "a parent of the same definition.");
        }
    }

    /// <summary>Runs a Norfair Rio flame follower's main AI: it advances the shared enemy RNG, as every parent does, then follows its parent.</summary>
    internal void RunNorfairRioFollowerMainAi(RoomEnemyPlace place)
    {
        _room.Random.Generate();
        FollowNorfairRioParent(place, place.RequireSpeciesState<NorfairRioFollowerState>());
    }

    /// <summary>Runs a Norfair Rio parent's main AI.</summary>
    internal void RunNorfairRioMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        RunNorfairRioMain(
            place,
            place.RequireSpeciesState<NorfairRioEnemyState>(),
            frame.Samus,
            frame.Level);

    /// <summary>Advances a Norfair Rio flame follower's typed animation, which raises no cue.</summary>
    internal static void AdvanceNorfairRioFollowerTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, NorfairRioAnimations.Segments,
            static (NorfairRioFollowerState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A Norfair Rio flame follower's cue-free animation raised {cue}."));

    /// <summary>Advances a Norfair Rio parent's typed animation, whose cues it carries out.</summary>
    internal static void AdvanceNorfairRioTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, NorfairRioAnimations.Segments,
            static (NorfairRioEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (state, cue) =>
            {
                ApplyNorfairRioCue(state, cue);
                return null;
            });
}
