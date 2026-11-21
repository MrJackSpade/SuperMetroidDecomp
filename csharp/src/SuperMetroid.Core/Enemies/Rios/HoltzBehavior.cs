using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The Lower Norfair rio species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class HoltzBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.Holtz], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and the entry it is spawned from.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        occupant.SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower
            ? EnemyOccupant.Of(new HoltzFollowerEntity(this, occupant, place, common))
            : EnemyOccupant.Of(new HoltzEntity(this, occupant, place, common));

    /// <summary>Supplies the shared random draw taken before every parent's and follower's AI runs.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves wall and floor collisions during Rio dives and returns.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>The room's enemy places, where each flame follower finds its parent when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The parent each flame follower found when it was placed, which its room lists right before it.</summary>
    private readonly EnemyPartLinks<RoomEnemyPlace> _parents = new();

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds Holtz parent and follower behavior to room RNG, collision, and place order.</summary>
    internal HoltzBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyPlaces places, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _terrain = terrain;
        _places = places;
    }

    /// <summary>Initializes a flame follower: it finds its parent, listed right before it, and starts its looping flames.</summary>
    internal void InitializeHoltzFollower(RoomEnemyPlace place)
    {
        _parents.Place(place, _places.Member(place, EnemyFollowerListingDefinitions.Listing, EnemyFollowerRole.Follower, EnemyFollowerRole.Leader));
        ValidateHoltzParent(place);
        place.SetSpeciesState(new HoltzFollowerState
        {
            Playback = new(HoltzAnimations.StartingWith(HoltzSegment.Flames), AnimationPlace.StartOfAnimation),
        });
    }

    /// <summary>Initializes a Holtz parent, idle with its flames hidden.</summary>
    internal static void InitializeHoltz(RoomEnemyPlace place)
    {
        place.SetSpeciesState(new HoltzEnemyState(HoltzAction.WaitForAttackOpportunity)
        {
            FollowerVisible = false,
            Playback = new(HoltzAnimations.StartingWith(HoltzSegment.Idle), AnimationPlace.StartOfAnimation),
        });
    }

    /// <summary>What <see cref="RunHoltzMain"/> does for each <see cref="HoltzAction"/> member, each member's own arm.</summary>
    private IdentityTable<HoltzAction, Action<RoomEnemyPlace, HoltzEnemyState, SamusState, RoomLevelData, int>> RunHoltzMainForHoltzAction => field ??= new(new Dictionary<HoltzAction, Action<RoomEnemyPlace, HoltzEnemyState, SamusState, RoomLevelData, int>>
    {
        [HoltzAction.WaitForAttackOpportunity] = (place, state, samus, level, random) =>
        {
            if (RioLaunchDefinitions.AttackHeldBack.IsTakenBy(random) ||
                Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) >= RioLaunchDefinitions.LowerNorfairTriggerDistance)
            {
                // Unlike the Geruta, a Holtz that does not attack forgets that its animation finished
                // and asks for its idle animation on every such update; the idle animation already playing does not
                // restart.
                state.Finish.Forget();
                StartHoltzAnimation(
                    place,
                    state,
                    HoltzSegment.Idle);
                return;
            }

            state.Velocity = new(
                RioLaunchDefinitions.LowerNorfairXVelocity.Toward(RioLaunches.SideOfSamus(place, samus)),
                RioLaunchDefinitions.LowerNorfairYVelocity);
            StartHoltzAnimation(
                place,
                state,
                HoltzSegment.PrepareToSwoop);
            state.Action = HoltzAction.WaitForTakeoffAnimation;
        },
        [HoltzAction.WaitForTakeoffAnimation] = (place, state, samus, level, random) =>
        {
            if (!state.Finish.Take())
                return;
            StartHoltzAnimation(
                place,
                state,
                HoltzSegment.Descending);
            state.Action = HoltzAction.Dive;
        },
        [HoltzAction.Dive] = (place, state, samus, level, random) =>
        {
            MoveHoltzHorizontally(level, place, state);
            if (_terrain.MoveEnemyVertically(
                    level,
                    place,
                    state.Velocity.Y.ToSixteenSixteenDelta()))
            {
                BeginHoltzReturn(place, state);
                return;
            }

            ApplyHoltzGravity(state);
            if (state.Velocity.Y.IsNegative)
                BeginHoltzReturn(place, state);
        },
        [HoltzAction.ReturnToPerch] = (place, state, samus, level, random) =>
        {
            MoveHoltzHorizontally(level, place, state);
            if (_terrain.MoveEnemyVertically(
                    level,
                    place,
                    state.Velocity.Y.ToSixteenSixteenDelta()))
            {
                StartHoltzAnimation(
                    place,
                    state,
                    HoltzSegment.Cooldown);
                state.Action = HoltzAction.WaitForLandingAnimation;
                return;
            }

            ApplyHoltzGravity(state);
            if (state.Finish.Take())
            {
                StartHoltzAnimation(
                    place,
                    state,
                    HoltzSegment.Ascending);
            }
        },
        [HoltzAction.WaitForLandingAnimation] = (place, state, samus, level, random) =>
        {
            if (!state.Finish.Take())
                return;
            StartHoltzAnimation(
                place,
                state,
                HoltzSegment.Cooldown);
            state.Action = HoltzAction.WaitForAttackOpportunity;
        },
    });

    /// <summary>
    /// Runs the Holtz's main AI and each action it runs.
    /// The shared random number advances before every parent and follower acts.
    /// </summary>
    private void RunHoltzMain(
        RoomEnemyPlace place,
        HoltzEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        int random = _room.Random.Generate();

        RunHoltzMainForHoltzAction[state.Action](place, state, samus, level, random);
    }

    /// <summary>What <see cref="ApplyHoltzCue"/> does for each <see cref="HoltzCueKind"/> member, each member's own arm.</summary>
    private static IdentityTable<HoltzCueKind, Action<HoltzEnemyState>> ApplyHoltzCueForHoltzCueKind => field ??= new(new Dictionary<HoltzCueKind, Action<HoltzEnemyState>>
    {
        [HoltzCueKind.AnimationFinished] = static state => state.Finish.Raise(),
        [HoltzCueKind.HideFlames] = static state => state.FollowerVisible = false,
        [HoltzCueKind.ShowFlames] = static state => state.FollowerVisible = true,
    });

    /// <summary>Carries out one cue of the Holtz's animations: the end of an animation, or showing or hiding its flames.</summary>
    private static void ApplyHoltzCue(HoltzEnemyState state, HoltzCue cue)
    {
        ApplyHoltzCueForHoltzCueKind[cue.Kind](state);
    }

    /// <summary>Mirrors the preceding parent's freeze and visibility, placing live flames their drop below it.</summary>
    private void FollowHoltzParent(RoomEnemyPlace follower)
    {
        RoomEnemyPlace parent = GetPrecedingHoltzPlace(follower);
        if (parent.Health == 0)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Deleted);
            return;
        }

        HoltzEnemyState parentState = parent.RequireSpeciesState<HoltzEnemyState>();
        follower.Freeze.Restart(parent.Freeze.UpdatesLeft);
        if (parent.Freeze.IsRunning || !parentState.FollowerVisible)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Hidden);
            return;
        }

        // The follower shows while its parent's flames are visible, at the parent's live position lowered by the
        // flames' drop.
        follower.Presence = follower.Presence.Without(EnemyPresence.Hidden);
        follower.Position = follower.Position.WithWhole(new(parent.Position.X.Whole, parent.Position.Y.Whole + RioLaunchDefinitions.LowerNorfairFlameDrop));
    }

    /// <summary>Sets upward return velocity, animation, action, and its enemy-library sound event.</summary>
    private void BeginHoltzReturn(
        RoomEnemyPlace place,
        HoltzEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(RioLaunchDefinitions.ReturnYVelocity);
        StartHoltzAnimation(
            place,
            state,
            HoltzSegment.AscentOpening);
        state.Action = HoltzAction.ReturnToPerch;
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.HoltzReturnSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Advances the Rio horizontally and reverses its velocity on a wall collision.</summary>
    private void MoveHoltzHorizontally(
        RoomLevelData level,
        RoomEnemyPlace place,
        HoltzEnemyState state)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                state.Velocity.X.ToSixteenSixteenDelta()))
        {
            NegateHoltzXVelocity(state);
        }
    }

    /// <summary>Reverses the sideways velocity after targeting or collision.</summary>
    private static void NegateHoltzXVelocity(HoltzEnemyState state) =>
        state.Velocity = state.Velocity.ReversedX();

    /// <summary>Slows the vertical speed by its gravity after a flight step.</summary>
    private static void ApplyHoltzGravity(HoltzEnemyState state) =>
        state.Velocity = state.Velocity.WithY(state.Velocity.Y - RioLaunchDefinitions.LowerNorfairGravity);

    /// <summary>Restarts a different Rio animation while leaving the playing animation's timer intact (#2145).</summary>
    private static void StartHoltzAnimation(
        RoomEnemyPlace place,
        HoltzEnemyState state,
        HoltzSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, HoltzAnimations.StartingWith(segment));

    /// <summary>The parent the flame follower found when it was placed, the enemy its room lists directly before it.</summary>
    private RoomEnemyPlace GetPrecedingHoltzPlace(RoomEnemyPlace follower) => _parents.Of(follower);

    /// <summary>Requires the preceding place to hold an unfollowing Holtz parent.</summary>
    private void ValidateHoltzParent(RoomEnemyPlace follower)
    {
        RoomEnemyPlace parent = GetPrecedingHoltzPlace(follower);
        if (parent.Entity is not HoltzEntity)
        {
            throw new InvalidDataException(
                $"Lower Norfair Rio follower in place {follower.Ordinal} must immediately " +
                "follow a parent of the same definition.");
        }
    }

    /// <summary>
    /// Runs a Holtz flame follower's main AI: it advances the shared enemy RNG, as every parent does, then
    /// follows its parent.
    /// </summary>
    internal void RunHoltzFollowerMainAi(RoomEnemyPlace place)
    {
        _room.Random.Generate();
        FollowHoltzParent(place);
    }

    /// <summary>Runs a Holtz parent's main AI.</summary>
    internal void RunHoltzMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        RunHoltzMain(
            place,
            place.RequireSpeciesState<HoltzEnemyState>(),
            frame.Samus,
            frame.Level);

    /// <summary>Advances a Holtz flame follower's typed animation, which raises no cue.</summary>
    internal static void AdvanceHoltzFollowerTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, HoltzAnimations.Segments,
            static (HoltzFollowerState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A Lower Norfair Rio flame follower's cue-free animation raised {cue}."));

    /// <summary>Advances a Holtz parent's typed animation, whose cues it carries out.</summary>
    internal static void AdvanceHoltzTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, HoltzAnimations.Segments,
            static (HoltzEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (state, cue) =>
            {
                ApplyHoltzCue(state, cue);
                return null;
            });
}
