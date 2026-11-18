using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The Lower Norfair rio species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class LowerNorfairRioBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Holtz];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) =>
        Ensure.NotNull(place).SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower
            ? new LowerNorfairRioFollowerEntity(this, place, common)
            : new LowerNorfairRioEntity(this, place, common);

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
    internal LowerNorfairRioBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyPlaces places, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _terrain = terrain;
        _places = places;
    }

    /// <summary>Initializes a flame follower: it finds its parent, listed right before it, and starts its looping flames.</summary>
    internal void InitializeLowerNorfairRioFollower(RoomEnemyPlace place)
    {
        _parents.Place(place, _places.ListedBefore(place, 1));
        ValidateLowerNorfairRioParent(place);
        place.SetSpeciesState(new LowerNorfairRioFollowerState
        {
            Playback = new(LowerNorfairRioAnimations.StartingWith(LowerNorfairRioSegment.Flames), AnimationPlace.StartOfAnimation),
        });
    }

    /// <summary>Initializes a Lower Norfair Rio parent, idle with its flames hidden.</summary>
    internal static void InitializeLowerNorfairRio(RoomEnemyPlace place)
    {
        place.SetSpeciesState(new LowerNorfairRioEnemyState(LowerNorfairRioAction.WaitForAttackOpportunity)
        {
            AnimationSignal = false,
            FollowerVisible = false,
            Playback = new(LowerNorfairRioAnimations.StartingWith(LowerNorfairRioSegment.Idle), AnimationPlace.StartOfAnimation),
        });
    }

    /// <summary>
    /// Runs the Lower Norfair Rio's main AI and each action it runs.
    /// The shared RNG advances before every parent and follower acts, exactly as the game does.
    /// </summary>
    private void RunLowerNorfairRioMain(
        RoomEnemyPlace place,
        LowerNorfairRioEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        int random = _room.Random.Generate();

        switch (state.Action)
        {
            case LowerNorfairRioAction.WaitForAttackOpportunity:
                if (RioLaunchDefinitions.AttackHeldBack.IsTakenBy(random) ||
                    Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) >= RioLaunchDefinitions.LowerNorfairTriggerDistance)
                {
                    // Unlike the Norfair Rio, a Lower Norfair Rio that does not attack forgets that its animation finished
                    // and asks for its idle animation on every such update; the idle animation already playing does not
                    // restart.
                    state.AnimationSignal = false;
                    StartLowerNorfairRioAnimation(
                        place,
                        state,
                        LowerNorfairRioSegment.Idle);
                    return;
                }

                state.Velocity = new(
                    RioLaunchDefinitions.LowerNorfairXVelocity,
                    RioLaunchDefinitions.LowerNorfairYVelocity);
                if (samus.Position.X.Whole < place.Position.X.Whole)
                    NegateLowerNorfairRioXVelocity(state);
                StartLowerNorfairRioAnimation(
                    place,
                    state,
                    LowerNorfairRioSegment.PrepareToSwoop);
                state.Action = LowerNorfairRioAction.WaitForTakeoffAnimation;
                return;

            case LowerNorfairRioAction.WaitForTakeoffAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                StartLowerNorfairRioAnimation(
                    place,
                    state,
                    LowerNorfairRioSegment.Descending);
                state.Action = LowerNorfairRioAction.Dive;
                return;

            case LowerNorfairRioAction.Dive:
                MoveLowerNorfairRioHorizontally(level, place, state);
                if (_terrain.MoveEnemyVertically(
                        level,
                        place,
                        state.Velocity.Y.ToSixteenSixteenDelta()))
                {
                    BeginLowerNorfairRioReturn(place, state);
                    return;
                }

                ApplyLowerNorfairRioGravity(state);
                if (state.Velocity.Y.IsNegative)
                    BeginLowerNorfairRioReturn(place, state);
                return;

            case LowerNorfairRioAction.ReturnToPerch:
                MoveLowerNorfairRioHorizontally(level, place, state);
                if (_terrain.MoveEnemyVertically(
                        level,
                        place,
                        state.Velocity.Y.ToSixteenSixteenDelta()))
                {
                    StartLowerNorfairRioAnimation(
                        place,
                        state,
                        LowerNorfairRioSegment.Cooldown);
                    state.Action = LowerNorfairRioAction.WaitForLandingAnimation;
                    return;
                }

                ApplyLowerNorfairRioGravity(state);
                if (state.AnimationSignal)
                {
                    state.AnimationSignal = false;
                    StartLowerNorfairRioAnimation(
                        place,
                        state,
                        LowerNorfairRioSegment.Ascending);
                }
                return;

            case LowerNorfairRioAction.WaitForLandingAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                StartLowerNorfairRioAnimation(
                    place,
                    state,
                    LowerNorfairRioSegment.Cooldown);
                state.Action = LowerNorfairRioAction.WaitForAttackOpportunity;
                return;

            default:
                throw new InvalidDataException(
                    $"Lower Norfair Rio has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Carries out one cue of the Lower Norfair Rio's animations: the end of an animation, or showing or hiding its flames.</summary>
    private static void ApplyLowerNorfairRioCue(LowerNorfairRioEnemyState state, LowerNorfairRioCue cue)
    {
        switch (cue.Kind)
        {
            case LowerNorfairRioCueKind.AnimationFinished:
                state.AnimationSignal = true;
                break;
            case LowerNorfairRioCueKind.HideFlames:
                state.FollowerVisible = false;
                break;
            case LowerNorfairRioCueKind.ShowFlames:
                state.FollowerVisible = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(LowerNorfairRioCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Mirrors the preceding parent's freeze and visibility, placing live flames their drop below it.</summary>
    private void FollowLowerNorfairRioParent(RoomEnemyPlace follower)
    {
        RoomEnemyPlace parent = GetPrecedingLowerNorfairRioPlace(follower);
        if (parent.Health == 0)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Deleted);
            return;
        }

        LowerNorfairRioEnemyState parentState = parent.RequireSpeciesState<LowerNorfairRioEnemyState>();
        follower.FrozenTimer = parent.FrozenTimer;
        if (parent.FrozenTimer != 0 || !parentState.FollowerVisible)
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
    private void BeginLowerNorfairRioReturn(
        RoomEnemyPlace place,
        LowerNorfairRioEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(RioLaunchDefinitions.ReturnYVelocity);
        StartLowerNorfairRioAnimation(
            place,
            state,
            LowerNorfairRioSegment.AscentOpening);
        state.Action = LowerNorfairRioAction.ReturnToPerch;
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.LowerNorfairRioReturnSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Advances the Rio horizontally and reverses its velocity on a wall collision.</summary>
    private void MoveLowerNorfairRioHorizontally(
        RoomLevelData level,
        RoomEnemyPlace place,
        LowerNorfairRioEnemyState state)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                state.Velocity.X.ToSixteenSixteenDelta()))
        {
            NegateLowerNorfairRioXVelocity(state);
        }
    }

    /// <summary>Reverses the sideways velocity after targeting or collision.</summary>
    private static void NegateLowerNorfairRioXVelocity(LowerNorfairRioEnemyState state) =>
        state.Velocity = state.Velocity.ReversedX();

    /// <summary>Slows the vertical speed by its gravity after a flight step.</summary>
    private static void ApplyLowerNorfairRioGravity(LowerNorfairRioEnemyState state) =>
        state.Velocity = state.Velocity.WithY(state.Velocity.Y - RioLaunchDefinitions.LowerNorfairGravity);

    /// <summary>Restarts a different Rio animation while leaving the playing animation's timer intact (#2145).</summary>
    private static void StartLowerNorfairRioAnimation(
        RoomEnemyPlace place,
        LowerNorfairRioEnemyState state,
        LowerNorfairRioSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, LowerNorfairRioAnimations.StartingWith(segment));

    /// <summary>The parent the flame follower found when it was placed, the enemy its room lists directly before it.</summary>
    private RoomEnemyPlace GetPrecedingLowerNorfairRioPlace(RoomEnemyPlace follower) => _parents.Of(follower);

    /// <summary>Requires the preceding place to hold an unfollowing Holtz parent.</summary>
    private void ValidateLowerNorfairRioParent(RoomEnemyPlace follower)
    {
        RoomEnemyPlace parent = GetPrecedingLowerNorfairRioPlace(follower);
        if (parent.Entity is not LowerNorfairRioEntity)
        {
            throw new InvalidDataException(
                $"Lower Norfair Rio follower in place {follower.Ordinal} must immediately " +
                "follow a parent of the same definition.");
        }
    }

    /// <summary>
    /// Runs a Lower Norfair Rio flame follower's main AI: it advances the shared enemy RNG, as every parent does, then
    /// follows its parent.
    /// </summary>
    internal void RunLowerNorfairRioFollowerMainAi(RoomEnemyPlace place)
    {
        _room.Random.Generate();
        FollowLowerNorfairRioParent(place);
    }

    /// <summary>Runs a Lower Norfair Rio parent's main AI.</summary>
    internal void RunLowerNorfairRioMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        RunLowerNorfairRioMain(
            place,
            place.RequireSpeciesState<LowerNorfairRioEnemyState>(),
            frame.Samus,
            frame.Level);

    /// <summary>Advances a Lower Norfair Rio flame follower's typed animation, which raises no cue.</summary>
    internal static void AdvanceLowerNorfairRioFollowerTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, LowerNorfairRioAnimations.Segments,
            static (LowerNorfairRioFollowerState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A Lower Norfair Rio flame follower's cue-free animation raised {cue}."));

    /// <summary>Advances a Lower Norfair Rio parent's typed animation, whose cues it carries out.</summary>
    internal static void AdvanceLowerNorfairRioTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, LowerNorfairRioAnimations.Segments,
            static (LowerNorfairRioEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (state, cue) =>
            {
                ApplyLowerNorfairRioCue(state, cue);
                return null;
            });
}
