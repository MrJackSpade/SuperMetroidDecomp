using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The Norfair rio species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class GerutaBehavior : IEnemySpecies, IEnemySpawnFactory<EnemyFollowerSpawnSettings>
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Geruta];

    /// <summary>
    /// A parent, which offers the follower its room lists right after it the creation that builds that follower following
    /// it (#2164, #2554); a follower joins the parent listed before it, so the population may not place one first.
    /// </summary>
    /// <exception cref="InvalidDataException">A follower is listed with no parent of its species before it.</exception>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, EnemyFollowerSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        if (Ensure.NotNull(settings).Role == EnemyFollowerRole.Follower)
        {
            throw new InvalidDataException(
                $"Norfair Rio follower in place {Ensure.NotNull(place).Ordinal} must immediately follow a parent of the same definition.");
        }
        var parent = new GerutaEntity(this, occupant, place, common);
        _partOffers.Offer<EnemyFollowerSpawnSettings>(EnemyDefinitionId.Geruta, new ParentFollowing(this, parent.State, parent.Place).CreateLater);
        return EnemyOccupant.Running(parent);
    }

    /// <summary>
    /// The creation a parent offers the entry its room lists after it (#2554): a follower following the parent; another
    /// parent is built as any parent is.
    /// </summary>
    /// <param name="species">The Geruta species owner.</param>
    /// <param name="parentState">
    /// The state of the parent a later follower follows. The joining holds what the creation reads, not the parent's entity, so a restore of the game's state never rebuilds
    /// that entity from inside its own offer.
    /// </param>
    /// <param name="parentPlace">The place of the parent a later follower follows.</param>
    private sealed partial class ParentFollowing(GerutaBehavior species, GerutaEnemyState parentState, RoomEnemyPlace parentPlace)
    {
        /// <summary>A later entry: a follower following the parent, or another parent.</summary>
        internal EnemyOccupant CreateLater(RoomEnemyOccupant occupant, EnemyFollowerSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
            settings.Role == EnemyFollowerRole.Follower
                ? EnemyOccupant.Running(new GerutaFollowerEntity(species, occupant, place, common, parentState, parentPlace))
                : species.Create(occupant, settings, place, common);
    }

    /// <summary>Where a parent offers the follower listed after it the creation that builds it following the parent.</summary>
    private readonly EnemyPartOffers _partOffers;

    /// <summary>Provides the shared RNG advanced by every parent and flame follower.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves horizontal and vertical contact during the dive and return.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>The room's enemy places, where each flame follower finds its parent when it is placed.</summary>
    private readonly EnemyPlaces _places;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Captures the room RNG, terrain, and enemy places needed by Rio AI.</summary>
    internal GerutaBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyPlaces places, EnemyAudioQueue audio,
        EnemyPartOffers partOffers)
    {
        _partOffers = partOffers;
        _audio = audio;
        _room = room;
        _terrain = terrain;
        _places = places;
    }

    /// <summary>Initializes a flame follower: it finds its parent, listed right before it, and starts its ascending flames.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    /// <exception cref="InvalidDataException">The room lists another enemy between the follower and <paramref name="parentPlace"/>.</exception>
    internal GerutaFollowerState InitializeGerutaFollower(RoomEnemyPlace place, GerutaEnemyState parentState, RoomEnemyPlace parentPlace)
    {
        if (_places.Member(place, EnemyFollowerListingDefinitions.Listing, EnemyFollowerRole.Follower, EnemyFollowerRole.Leader) != Ensure.NotNull(parentPlace))
        {
            throw new InvalidDataException(
                $"Norfair Rio follower in place {place.Ordinal} must immediately follow a parent of the same definition.");
        }
        var state = new GerutaFollowerState(Ensure.NotNull(parentState), parentPlace)
        {
            Playback = new(GerutaAnimations.StartingWith(GerutaSegment.FlamesAscending), AnimationPlace.StartOfAnimation),
        };
        return state;
    }

    /// <summary>Initializes a Geruta parent, idle and waiting for its chance to attack.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static GerutaEnemyState InitializeGeruta()
    {
        var state = new GerutaEnemyState(GerutaAction.WaitForAttackOpportunity)
        {
            FollowerYOffset = 0,
            Flames = GerutaFlames.Ascending,
            Playback = new(GerutaAnimations.StartingWith(GerutaSegment.Idle), AnimationPlace.StartOfAnimation),
        };
        return state;
    }

    /// <summary>What <see cref="RunGerutaMain"/> does for each <see cref="GerutaAction"/> member, each member's own arm.</summary>
    private IdentityTable<GerutaAction, Action<RoomEnemyPlace, GerutaEnemyState, SamusState, RoomLevelData, int>> RunGerutaMainForGerutaAction => field ??= new(new Dictionary<GerutaAction, Action<RoomEnemyPlace, GerutaEnemyState, SamusState, RoomLevelData, int>>
    {
        [GerutaAction.WaitForAttackOpportunity] = (place, state, samus, level, random) =>
        {
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
        },
        [GerutaAction.WaitForTakeoffAnimation] = (place, state, samus, level, random) =>
        {
            if (!state.Finish.Take())
                return;
            StartGerutaSwoopAnimation(
                place,
                state,
                GerutaSegment.Descending);
            state.Action = GerutaAction.Dive;
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.GerutaDiveSound, priority: SoundRequestPriority.Normal);
        },
        [GerutaAction.Dive] = (place, state, samus, level, random) =>
        {
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
        },
        [GerutaAction.ReturnToPerch] = (place, state, samus, level, random) =>
        {
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
        },
        [GerutaAction.FinishLanding] = (place, state, samus, level, random) =>
        {
            // Finishing the landing takes one update and changes only the action: the animation the climb left
            // keeps playing, its flames with it, while the Rio waits for another random attack.
            state.Action = GerutaAction.WaitForAttackOpportunity;
        },
    });

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

        RunGerutaMainForGerutaAction[state.Action](place, state, samus, level, random);
    }

    /// <summary>Carries out one cue of the Geruta's animations: the end of its swoop's start, or a flame placement.</summary>
    private static void ApplyGerutaCue(GerutaEnemyState state, GerutaCue cue)
    {
        cue.React(ApplyGerutaCueReactions, state);
    }

    /// <summary>What <see cref="ApplyGerutaCue"/> does on each <see cref="GerutaCue"/>, built once (#2553).</summary>
    private static GerutaCueReactions<GerutaEnemyState> ApplyGerutaCueReactions { get; } = new(
        PlaceFlames: static (placeFlames, state) =>
        {
            state.FollowerYOffset = placeFlames.Below;
            state.Flames = placeFlames.Flames;
        },
        SwoopStartFinished: static (_, state) => state.Finish.Raise());

    /// <summary>Copies parent freeze, visibility, animation, and flame offset to the adjacent follower.</summary>
    private void FollowGerutaParent(
        RoomEnemyPlace follower,
        GerutaFollowerState followerState)
    {
        RoomEnemyPlace parent = followerState.ParentPlace;
        if (parent.Health == 0)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Deleted);
            return;
        }
        // The pair is checked when the follower is placed; a parent deleted without a kill is read as it left its place and
        // its animation (RoomEnemyPlace.FoundEntity).
        GerutaEnemyState parentState = followerState.Parent;
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

    /// <summary>Turns the Rio back upward and starts the climb's animation.</summary>
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

    /// <summary>Runs a Geruta flame follower's main AI: it advances the shared enemy RNG, as every parent does, then follows its parent.</summary>
    internal void RunGerutaFollowerMainAi(RoomEnemyPlace place, GerutaFollowerState gerutaFollowerState)
    {
        _room.Random.Generate();
        FollowGerutaParent(place, gerutaFollowerState);
    }

    /// <summary>Runs a Geruta parent's main AI.</summary>
    internal void RunGerutaMainAi(RoomEnemyPlace place, GerutaEnemyState gerutaEnemyState, IEnemyAiFrame frame) =>
        RunGerutaMain(
            place,
            gerutaEnemyState,
            frame.Samus,
            frame.Level);

    /// <summary>Advances a Geruta flame follower's typed animation, which raises no cue.</summary>
    internal static void AdvanceGerutaFollowerTypedAnimation(RoomEnemyPlace place, GerutaFollowerState gerutaFollowerState) =>
        EnemyAnimationPlayback.Advance(place, gerutaFollowerState, GerutaAnimations.Segments,
            static (GerutaFollowerState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A Norfair Rio flame follower's cue-free animation raised {cue}."));

    /// <summary>Advances a Geruta parent's typed animation, whose cues it carries out.</summary>
    internal static void AdvanceGerutaTypedAnimation(RoomEnemyPlace place, GerutaEnemyState gerutaEnemyState) =>
        EnemyAnimationPlayback.Advance(place, gerutaEnemyState, GerutaAnimations.Segments,
            static (GerutaEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (state, cue) =>
            {
                ApplyGerutaCue(state, cue);
                return null;
            });
}
