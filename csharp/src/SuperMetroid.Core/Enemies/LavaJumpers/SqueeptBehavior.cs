using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>The Squeept species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class SqueeptBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.Squeept], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="place">The place, holding the new identity and the entry it is spawned from.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common) =>
        Ensure.NotNull(place).SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower
            ? EnemyOccupant.Of(new NorfairLavaJumperFollowerEntity(this, place, common))
            : EnemyOccupant.Of(new SqueeptEntity(this, place, common));

    /// <summary>Supplies one random number per parent and follower AI frame.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The room's enemy places, where each follower finds its parent when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The parent each follower found when it was placed, which its room lists right before it.</summary>
    private readonly EnemyPartLinks<RoomEnemyPlace> _parents = new();

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds lava-jumper behavior to room RNG and the room's enemies.</summary>
    internal SqueeptBehavior(EnemyRoomBinding room, EnemyPlaces places, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _places = places;
    }

    /// <summary>Initializes a lava jumper's cosmetic follower.</summary>
    internal void InitializeNorfairLavaJumperFollower(RoomEnemyPlace place)
    {
        // Every room with it places this cosmetic second half immediately after its moving parent, which it
        // finds when it is placed and follows from then on; a population that places anything else there is refused.
        place.SetSpeciesState(new NorfairLavaJumperFollowerState
        {
            Playback = new(NorfairLavaJumperAnimations.StartingWith(NorfairLavaJumperSegment.Follower), AnimationPlace.StartOfAnimation),
        });
        _parents.Place(place, _places.ListedBefore(place, 1));
        ValidateSqueeptParent(place);
    }

    /// <summary>Initializes a jumping lava jumper, hidden in the lava at its population position.</summary>
    internal static void InitializeSqueept(RoomEnemyPlace place)
    {
        var state = new SqueeptState(SqueeptAction.BeginJump);
        place.SetSpeciesState(state);

        // The jumping parent returns to exactly these whole population coordinates after every arc; where it lies
        // within a pixel carries over from one jump to the next.
        state.SpawnPosition = place.Position.Whole;
        state.Playback = new(NorfairLavaJumperAnimations.StartingWith(NorfairLavaJumperSegment.Hidden), AnimationPlace.StartOfAnimation);
    }

    /// <summary>
    /// Runs the follower's main behaviour. Main advances the shared random numbers once for the follower as for the
    /// parent; the follower discards the number, which still affects later room actors.
    /// </summary>
    internal void RunNorfairLavaJumperFollowerMain(RoomEnemyPlace follower)
    {
        _room.Random.Generate();
        FollowSqueeptParent(follower);
    }

    /// <summary>What <c>RunSqueeptMain</c> does for each <see cref="SqueeptAction"/> member, each member's own arm.</summary>
    private IdentityTable<SqueeptAction, Action<RoomEnemyPlace, SqueeptState, int>> RunSqueeptMainForSqueeptAction => field ??= new(new Dictionary<SqueeptAction, Action<RoomEnemyPlace, SqueeptState, int>>
    {
        [SqueeptAction.BeginJump] = (place, state, random) =>
        {
            state.YVelocity = NorfairLavaJumpDefinitions.Launch.Choose(random);
            state.Action = SqueeptAction.RiseBeforeAnimationSwitch;
            place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.SqueeptJumpSound, priority: SoundRequestPriority.Normal);
        },
        [SqueeptAction.RiseBeforeAnimationSwitch] = (place, state, random) =>
        {
            MoveSqueeptVertically(place, state);
            state.ApplyGravity(NorfairLavaJumpDefinitions.Gravity);

            // The flip starts once the rise has slowed to its switch speed. Jumps start rising much faster, so the
            // flip always comes before the top of the arc.
            if (state.YVelocity.IsNegative && state.YVelocity >= NorfairLavaJumpDefinitions.AnimationSwitchVelocity)
            {
                StartSqueeptAnimation(place, state, NorfairLavaJumperSegment.Jump);
                state.Action = SqueeptAction.MoveUntilAnimationSignal;
            }
        },
        [SqueeptAction.MoveUntilAnimationSignal] = (place, state, random) =>
        {
            MoveSqueeptVertically(place, state);
            state.ApplyGravity(NorfairLavaJumpDefinitions.Gravity);
        },
        [SqueeptAction.Flipped] = (place, state, random) =>
        {
            // The update that finds the flip finished still moves as the flip did, then falls from the next.
            MoveSqueeptVertically(place, state);
            state.ApplyGravity(NorfairLavaJumpDefinitions.Gravity);
            state.Action = SqueeptAction.FallBackIntoLava;
        },
        [SqueeptAction.FallBackIntoLava] = (place, state, random) =>
        {
            MoveSqueeptVertically(place, state);

            // Reaching the bottom block row of any room screen is how the jumper knows it is back in the lava,
            // wherever the lava lies (gameplay-observable: a jumper whose fall crosses a screen's bottom row
            // higher up is back in the lava there). Once the actor reaches that row it snaps to its exact
            // population origin, restores the hidden animation, and rests off-screen until the next jump begins.
            if (RoomGridConversions.LiesOnScreensBottomBlockRow(place.Position.Y.Whole))
            {
                place.Position = place.Position.WithWhole(state.SpawnPosition);
                StartSqueeptAnimation(place, state, NorfairLavaJumperSegment.Hidden);
                state.Action = SqueeptAction.BeginJump;
                place.Processing = place.Processing.Without(EnemyProcessing.OffScreen);
            }
            else
            {
                state.ApplyGravity(NorfairLavaJumpDefinitions.Gravity);
            }
        },
    });

    /// <summary>
    /// Runs the jumping parent's main behaviour: its rising, flipping and falling actions. Main advances the shared
    /// random numbers once each update.
    /// </summary>
    private void RunSqueeptMain(
        RoomEnemyPlace place,
        SqueeptState state)
    {
        int random = _room.Random.Generate();

        RunSqueeptMainForSqueeptAction[state.Action](place, state, random);
    }

    /// <summary>Carries out the flip animation's finishing cue (#2439): the flip has finished, and the next update falls.</summary>
    /// <exception cref="InvalidOperationException">The jumper is not flipping.</exception>
    private static void ApplyNorfairLavaJumperCue(SqueeptState state, NorfairLavaJumperCue cue)
    {
        if (cue.Kind != NorfairLavaJumperCueKind.FinishAnimation)
            throw new InvalidOperationException($"Undefined {nameof(NorfairLavaJumperCueKind)} {cue.Kind}.");
        if (state.Action != SqueeptAction.MoveUntilAnimationSignal)
            throw new InvalidOperationException($"A Squeept's flip finished while it was {state.Action}.");
        state.Action = SqueeptAction.Flipped;
    }

    /// <summary>
    /// Shows the cosmetic follower at the preceding parent's height while the parent rises and is not frozen, and hides
    /// it otherwise.
    /// </summary>
    private void FollowSqueeptParent(RoomEnemyPlace follower)
    {
        RoomEnemyPlace parent = GetPrecedingSqueeptPlace(follower);

        // The follower reads its parent's health, freeze, vertical speed and position in the parent's place; a parent
        // deleted without a kill is read as it left them (RoomEnemyPlace.FoundState), and a killed parent's emptied place
        // ends the follower.
        if (parent.Health == 0)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Deleted);
            return;
        }

        // The pair is checked when the follower is placed, not on every update.
        SqueeptState parentState = parent.FoundState<SqueeptState>() ?? throw new InvalidOperationException(
            $"Lava jumper follower {follower.Ordinal} finds no lava jumper in its parent's place {parent.Ordinal}.");
        if (parent.Freeze.IsRunning || !parentState.YVelocity.IsNegative)
        {
            // The second half appears only while the parent is travelling upward. At the
            // apex and during descent, the parent animation alone owns the visible shape.
            follower.Presence = follower.Presence.With(EnemyPresence.Hidden);
            return;
        }

        follower.Presence = follower.Presence.Without(EnemyPresence.Hidden);
        follower.Position = follower.Position.WithWholeY(parent.Position.Y.Whole);
    }

    /// <summary>The parent the follower found when it was placed, the enemy its room lists directly before it.</summary>
    private RoomEnemyPlace GetPrecedingSqueeptPlace(RoomEnemyPlace follower) => _parents.Of(follower);

    /// <summary>Rejects a follower population entry without a same-species parent directly before it.</summary>
    private void ValidateSqueeptParent(RoomEnemyPlace follower)
    {
        RoomEnemyPlace parent = GetPrecedingSqueeptPlace(follower);
        if (parent.Entity is not SqueeptEntity)
        {
            throw new InvalidDataException(
                $"Norfair lava-jumping follower in place {follower.Ordinal} must immediately follow " +
                "a parent of the same definition. " +
                $"Preceding place: definition={parent.DefinitionId}, " +
                $"health={parent.Health}, spawn={parent.Spawn.Population.SpawnSettings}, contact={parent.Contact}, processing={parent.Processing}, presence={parent.Presence}.");
        }
    }

    /// <summary>Moves the jumper up or down by one update of its vertical speed.</summary>
    private static void MoveSqueeptVertically(
        RoomEnemyPlace place,
        SqueeptState state)
    {
        place.Position = place.Position.AdvanceY(state.YVelocity.ToSixteenSixteenDelta());
    }

    /// <summary>Starts an animation without restarting the one playing.</summary>
    private static void StartSqueeptAnimation(
        RoomEnemyPlace place,
        SqueeptState state,
        NorfairLavaJumperSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, NorfairLavaJumperAnimations.StartingWith(segment));

    /// <summary>Runs a jumping lava jumper's main AI.</summary>
    internal void RunSqueeptMainAi(RoomEnemyPlace place) =>
        RunSqueeptMain(
            place,
            place.RequireSpeciesState<SqueeptState>());

    /// <summary>Advances a lava jumper follower's looping typed animation, which raises no cue.</summary>
    internal static void AdvanceNorfairLavaJumperFollowerTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, NorfairLavaJumperAnimations.Segments,
            static (NorfairLavaJumperFollowerState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"The Squeept follower's animation raised {cue}."));

    /// <summary>Advances a jumping lava jumper's typed animation, whose jump raises its finishing cue.</summary>
    internal static void AdvanceSqueeptTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, NorfairLavaJumperAnimations.Segments,
            static (SqueeptState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyNorfairLavaJumperCue(state, cue);
                return null;
            });
}
