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
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new NorfairRioEntity(this, place, common);

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

    /// <summary>Strict wrapped X-distance threshold for a random dive opportunity.</summary>
    private const int NorfairRioHorizontalTriggerDistance = 192;
    /// <summary>Signed 1/256-pixel vertical speed decrement applied during dive and return.</summary>
    private static readonly SignedEightEight NorfairRioGravity = SignedEightEight.FromParts(0, 32);

    /// <summary>Initializes the Norfair Rio.</summary>
    internal void InitializeNorfairRio(RoomEnemyPlace place)
    {
        if (place.SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower)
        {
            _parents.Place(place, _places.ListedBefore(place, 1));
            ValidateNorfairRioParent(place);
            place.SetSpeciesState(new NorfairRioFollowerState
            {
                InstalledAnimation = NorfairRioSegment.FlamesAscending,
                Playback = new(NorfairRioAnimations.StartingWith(NorfairRioSegment.FlamesAscending), AnimationPlace.StartOfAnimation),
            });
            return;
        }

        place.SetSpeciesState(new NorfairRioEnemyState(NorfairRioAction.WaitForAttackOpportunity)
        {
            AnimationSignal = false,
            FollowerYOffset = 0,
            InstalledAnimation = NorfairRioSegment.Idle,
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
                // The retail gate tests two parts of the newly generated random
                // number, then the strict modular X distance. Failure also consumes a pending
                // animation signal and restores the idle animation without starting an attack.
                if ((!RioLaunchDefinitions.AttackChanceLowBit.IsSetIn(random) && !RioLaunchDefinitions.AttackChanceHighBit.IsSetIn(random)) ||
                    !EnemyMath.IsWithinStrictModularDistance(
                        samus.Position.X.Whole,
                        place.Position.X.Whole,
                        NorfairRioHorizontalTriggerDistance))
                {
                    if (state.AnimationSignal)
                    {
                        state.AnimationSignal = false;
                        StartNorfairRioAnimation(
                            place,
                            state,
                            NorfairRioSegment.Idle);
                    }
                    return;
                }

                // The same random number picks the dive's fast or slow downward launch.
                state.Velocity = new EightEightVector<RoomPixels>(
                    RioLaunchDefinitions.NorfairXVelocity,
                    RioLaunches.NorfairYVelocity(random));
                if (CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) < 0)
                    state.Velocity = state.Velocity.ReversedX();
                StartNorfairRioAnimation(
                    place,
                    state,
                    NorfairRioSegment.StartDescending);
                state.Action = NorfairRioAction.WaitForTakeoffAnimation;
                return;

            case NorfairRioAction.WaitForTakeoffAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                StartNorfairRioAnimation(
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

                state.Velocity = state.Velocity.WithY(state.Velocity.Y - NorfairRioGravity);
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

                state.Velocity = state.Velocity.WithY(state.Velocity.Y - NorfairRioGravity);
                if (state.AnimationSignal)
                {
                    state.AnimationSignal = false;
                    StartNorfairRioAnimation(
                        place,
                        state,
                        NorfairRioSegment.Ascending);
                }
                return;

            case NorfairRioAction.FinishLanding:
                // Finishing the landing is intentionally a one-frame state: it changes only
                // the action that runs. The looping idle animation remains installed, which also keeps
                // the follower half visible while the actor waits for another random attack.
                state.Action = NorfairRioAction.WaitForAttackOpportunity;
                return;

            default:
                throw new InvalidDataException(
                    $"Norfair Rio has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Carries out one cue of the Norfair Rio's animations: the swoop-start handshake or a flame placement.</summary>
    private static void ApplyNorfairRioCue(NorfairRioEnemyState state, NorfairRioCue cue)
    {
        switch (cue.Kind)
        {
            case NorfairRioCueKind.SwoopStartFinished:
                state.AnimationSignal = true;
                break;
            case NorfairRioCueKind.PlaceFlames:
                state.FollowerYOffset = cue.FlamesBelow;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(NorfairRioCueKind)} {cue.Kind}.");
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
        if (parent.FrozenTimer != 0 ||
            parentState.InstalledAnimation == NorfairRioSegment.Idle)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Hidden);
            return;
        }

        // The follower's spawn reads the parent's own state: its installed animation and its signed
        // follower Y offset. Its drawing position follows the parent's live position.
        NorfairRioSegment followerList = parentState.FollowerYOffset < 0
            ? NorfairRioSegment.FlamesDescending
            : NorfairRioSegment.FlamesAscending;
        StartNorfairRioAnimation(follower, followerState, followerList);
        follower.Presence = follower.Presence.Without(EnemyPresence.Hidden);
        follower.Position = follower.Position.WithWhole(parent.Position.Whole + new Offset16<RoomPixels>(0, parentState.FollowerYOffset));
    }

    /// <summary>Reverses vertical travel, installs the ascent animation, and enters the return state.</summary>
    private static void BeginNorfairRioReturn(
        RoomEnemyPlace place,
        NorfairRioEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(-SignedEightEight.FromParts(0, 1));
        StartNorfairRioAnimation(
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

    /// <summary>Restarts a parent's animation only when it changes.</summary>
    private static void StartNorfairRioAnimation(
        RoomEnemyPlace place,
        NorfairRioEnemyState state,
        NorfairRioSegment segment) =>
        EnemyAnimationPlayback.StartOnce(place, state.InstalledAnimation, segment, installed =>
        {
            state.InstalledAnimation = installed;
            state.Playback = new(NorfairRioAnimations.StartingWith(installed), AnimationPlace.StartOfAnimation);
        });

    /// <summary>Restarts a flame follower's animation only when it changes.</summary>
    private static void StartNorfairRioAnimation(
        RoomEnemyPlace place,
        NorfairRioFollowerState state,
        NorfairRioSegment segment) =>
        EnemyAnimationPlayback.StartOnce(place, state.InstalledAnimation, segment, installed =>
        {
            state.InstalledAnimation = installed;
            state.Playback = new(NorfairRioAnimations.StartingWith(installed), AnimationPlace.StartOfAnimation);
        });

    /// <summary>The parent the follower found when it was placed, the enemy its room lists directly before it.</summary>
    private RoomEnemyPlace GetPrecedingNorfairRioPlace(RoomEnemyPlace follower) => _parents.Of(follower);

    /// <summary>Requires a flame follower to directly follow a primary Geruta population entry.</summary>
    private void ValidateNorfairRioParent(RoomEnemyPlace follower)
    {
        RoomEnemyPlace parent = GetPrecedingNorfairRioPlace(follower);
        if (parent.DefinitionId != EnemyDefinitionId.Geruta ||
            parent.SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower)
        {
            throw new InvalidDataException(
                $"Norfair Rio follower in place {follower.Ordinal} must immediately follow " +
                "a parent of the same definition.");
        }
    }

    /// <summary>
    /// Runs the Norfair Rio's main AI: a flame follower advances the shared enemy RNG, as every parent does, then follows
    /// its parent.
    /// </summary>
    internal void RunNorfairRioMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        if (place.SpeciesState<NorfairRioFollowerState>() is { } follower)
        {
            _room.Random.Generate();
            FollowNorfairRioParent(place, follower);
            return;
        }
        RunNorfairRioMain(
            place,
            place.RequireSpeciesState<NorfairRioEnemyState>(),
            frame.Samus,
            frame.Level);
    }

    /// <summary>Advances the Norfair Rio's typed animation: a parent's, whose cues it carries out, or a flame follower's, which raise none.</summary>
    internal static void AdvanceNorfairRioTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
    {
        if (place.SpeciesState<NorfairRioFollowerState>() is not null)
        {
            EnemyAnimationPlayback.Advance(place, NorfairRioAnimations.Segments,
                static (NorfairRioFollowerState state) => state.Playback, static (state, playback) => state.Playback = playback,
                static (_, cue) => throw new InvalidOperationException($"A Norfair Rio flame follower's cue-free animation raised {cue}."));
            return;
        }
        EnemyAnimationPlayback.Advance(place, NorfairRioAnimations.Segments,
            static (NorfairRioEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (state, cue) =>
            {
                ApplyNorfairRioCue(state, cue);
                return null;
            });
    }
}
