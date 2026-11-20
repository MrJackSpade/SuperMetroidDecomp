using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The Lower Norfair rio species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class HoltzBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Holtz];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup) =>
        Ensure.NotNull(place).SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower
            ? new HoltzFollowerEntity(this, place, common)
            : new HoltzEntity(this, place, common);

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
    internal HoltzFollowerState InitializeHoltzFollower(RoomEnemyPlace place)
    {
        _parents.Place(place, _places.ListedBefore(place, 1));
        var state = new HoltzFollowerState(ValidateHoltzParent(place))
        {
            Playback = new(HoltzAnimations.StartingWith(HoltzSegment.Flames), AnimationPlace.StartOfAnimation),
        };
        return state;
    }

    /// <summary>Initializes a Holtz parent, idle with its flames hidden.</summary>
    internal static HoltzEnemyState InitializeHoltz()
    {
        var state = new HoltzEnemyState(HoltzAction.WaitForAttackOpportunity)
        {
            FollowerVisible = false,
            Playback = new(HoltzAnimations.StartingWith(HoltzSegment.Idle), AnimationPlace.StartOfAnimation),
        };
        return state;
    }

    /// <summary>
    /// Runs the Holtz's main AI and each action it runs.
    /// The shared RNG advances before every parent and follower acts, exactly as the game does.
    /// </summary>
    private void RunHoltzMain(
        RoomEnemyPlace place,
        HoltzEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        int random = _room.Random.Generate();

        switch (state.Action)
        {
            case HoltzAction.WaitForAttackOpportunity:
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
                return;

            case HoltzAction.WaitForTakeoffAnimation:
                if (!state.Finish.Take())
                    return;
                StartHoltzAnimation(
                    place,
                    state,
                    HoltzSegment.Descending);
                state.Action = HoltzAction.Dive;
                return;

            case HoltzAction.Dive:
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
                return;

            case HoltzAction.ReturnToPerch:
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
                return;

            case HoltzAction.WaitForLandingAnimation:
                if (!state.Finish.Take())
                    return;
                StartHoltzAnimation(
                    place,
                    state,
                    HoltzSegment.Cooldown);
                state.Action = HoltzAction.WaitForAttackOpportunity;
                return;

            default:
                throw new InvalidDataException(
                    $"Lower Norfair Rio has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Carries out one cue of the Holtz's animations: the end of an animation, or showing or hiding its flames.</summary>
    private static void ApplyHoltzCue(HoltzEnemyState state, HoltzCue cue)
    {
        switch (cue.Kind)
        {
            case HoltzCueKind.AnimationFinished:
                state.Finish.Raise();
                break;
            case HoltzCueKind.HideFlames:
                state.FollowerVisible = false;
                break;
            case HoltzCueKind.ShowFlames:
                state.FollowerVisible = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(HoltzCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Mirrors the preceding parent's freeze and visibility, placing live flames their drop below it.</summary>
    private void FollowHoltzParent(RoomEnemyPlace follower, HoltzFollowerState state)
    {
        RoomEnemyPlace parent = GetPrecedingHoltzPlace(follower);
        if (parent.Health == 0)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Deleted);
            return;
        }

        HoltzEnemyState parentState = state.Parent;
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

    /// <summary>The state of the Holtz parent the follower directly follows, which its population entry needs.</summary>
    /// <exception cref="InvalidDataException">The room lists no Holtz right before the follower.</exception>
    private HoltzEnemyState ValidateHoltzParent(RoomEnemyPlace follower)
    {
        RoomEnemyPlace parent = GetPrecedingHoltzPlace(follower);
        if (parent.Entity is not HoltzEntity holtz)
        {
            throw new InvalidDataException(
                $"Lower Norfair Rio follower in place {follower.Ordinal} must immediately " +
                "follow a parent of the same definition.");
        }
        return holtz.State;
    }

    /// <summary>
    /// Runs a Holtz flame follower's main AI: it advances the shared enemy RNG, as every parent does, then
    /// follows its parent.
    /// </summary>
    internal void RunHoltzFollowerMainAi(RoomEnemyPlace place, HoltzFollowerState state)
    {
        _room.Random.Generate();
        FollowHoltzParent(place, state);
    }

    /// <summary>Runs a Holtz parent's main AI.</summary>
    internal void RunHoltzMainAi(RoomEnemyPlace place, HoltzEnemyState state, IEnemyAiFrame frame) =>
        RunHoltzMain(
            place,
            state,
            frame.Samus,
            frame.Level);

    /// <summary>Advances a Holtz flame follower's typed animation, which raises no cue.</summary>
    internal static void AdvanceHoltzFollowerTypedAnimation(RoomEnemyPlace place, HoltzFollowerState holtzFollowerState) =>
        EnemyAnimationPlayback.Advance(place, holtzFollowerState, HoltzAnimations.Segments,
            static (HoltzFollowerState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A Lower Norfair Rio flame follower's cue-free animation raised {cue}."));

    /// <summary>Advances a Holtz parent's typed animation, whose cues it carries out.</summary>
    internal static void AdvanceHoltzTypedAnimation(RoomEnemyPlace place, HoltzEnemyState holtzEnemyState, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, holtzEnemyState, HoltzAnimations.Segments,
            static (HoltzEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (state, cue) =>
            {
                ApplyHoltzCue(state, cue);
                return null;
            });
}
