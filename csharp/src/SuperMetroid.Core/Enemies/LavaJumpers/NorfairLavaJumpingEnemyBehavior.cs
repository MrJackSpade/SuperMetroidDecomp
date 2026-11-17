using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>The Norfair lava-jumping enemy species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class NorfairLavaJumpingEnemyBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Squeept];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) =>
        Ensure.NotNull(place).SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower
            ? new NorfairLavaJumperFollowerEntity(this, place, common)
            : new NorfairLavaJumpingEnemyEntity(this, place, common);

    /// <summary>Supplies one random number per parent and follower AI frame.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The room's enemy places, where each follower finds its parent when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The parent each follower found when it was placed, which its room lists right before it.</summary>
    private readonly EnemyPartLinks<RoomEnemyPlace> _parents = new();

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds lava-jumper behavior to room RNG and the room's enemies.</summary>
    internal NorfairLavaJumpingEnemyBehavior(EnemyRoomBinding room, EnemyPlaces places, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _places = places;
    }

    /// <summary>Velocity increment applied after each moving frame, 56 256ths of a pixel per update.</summary>
    private static readonly SignedEightEight NorfairLavaJumpingEnemyGravity = SignedEightEight.FromParts(0, 56);
    /// <summary>The rising velocity at and above which, while still rising, the jumper switches to the visible jump animation.</summary>
    private static readonly SignedEightEight NorfairLavaJumpingEnemyAnimationSwitchVelocity = -SignedEightEight.FromParts(4, 0);

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
        ValidateNorfairLavaJumpingEnemyParent(place);
    }

    /// <summary>Initializes a jumping lava jumper, hidden in the lava at its population position.</summary>
    internal static void InitializeNorfairLavaJumpingEnemy(RoomEnemyPlace place)
    {
        var state = new NorfairLavaJumpingEnemyState(NorfairLavaJumpingEnemyAction.BeginJump)
        {
            AnimationFinished = false,
        };
        place.SetSpeciesState(state);

        // The jumping parent returns to exactly these population coordinates after every
        // arc. Subpositions are deliberately not saved or cleared by the jump.
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
        FollowNorfairLavaJumpingEnemyParent(follower);
    }

    /// <summary>
    /// Runs the jumping parent's main behaviour: its rising, flipping and falling actions. Main advances the shared
    /// random numbers once each update.
    /// </summary>
    internal void RunNorfairLavaJumpingEnemyMain(
        RoomEnemyPlace place,
        NorfairLavaJumpingEnemyState state)
    {
        int random = _room.Random.Generate();

        switch (state.Action)
        {
            case NorfairLavaJumpingEnemyAction.BeginJump:
                state.YVelocity = NorfairLavaJumps.InitialVerticalVelocity(random);
                state.Action = NorfairLavaJumpingEnemyAction.RiseBeforeAnimationSwitch;
                place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.NorfairLavaJumpingEnemyJumpSound, priority: SoundRequestPriority.Normal);
                return;

            case NorfairLavaJumpingEnemyAction.RiseBeforeAnimationSwitch:
                MoveNorfairLavaJumpingEnemyVertically(place, state);
                state.ApplyGravity(NorfairLavaJumpingEnemyGravity);

                // The flip starts once the rise has slowed to four pixels an update. Jumps start rising at about eight,
                // so the flip always comes before the top of the arc.
                if (state.YVelocity.IsNegative && state.YVelocity >= NorfairLavaJumpingEnemyAnimationSwitchVelocity)
                {
                    StartNorfairLavaJumpingEnemyAnimation(place, state, NorfairLavaJumperSegment.Jump);
                    state.Action = NorfairLavaJumpingEnemyAction.MoveUntilAnimationSignal;
                }
                return;

            case NorfairLavaJumpingEnemyAction.MoveUntilAnimationSignal:
                MoveNorfairLavaJumpingEnemyVertically(place, state);
                state.ApplyGravity(NorfairLavaJumpingEnemyGravity);
                if (state.AnimationFinished)
                {
                    state.AnimationFinished = false;
                    state.Action = NorfairLavaJumpingEnemyAction.FallBackIntoLava;
                }
                return;

            case NorfairLavaJumpingEnemyAction.FallBackIntoLava:
                MoveNorfairLavaJumpingEnemyVertically(place, state);

                // Reaching the bottom block row of any room screen is how the jumper knows it is back in the lava,
                // wherever the lava lies (gameplay-observable: a jumper whose fall crosses a screen's bottom row
                // higher up is back in the lava there). Once the actor reaches that row it snaps to its exact
                // population origin, restores the hidden animation, and rests off-screen until the next jump begins.
                if (RoomGridConversions.LiesOnScreensBottomBlockRow(place.Position.Y.Whole))
                {
                    place.Position = place.Position.WithWhole(state.SpawnPosition);
                    StartNorfairLavaJumpingEnemyAnimation(place, state, NorfairLavaJumperSegment.Hidden);
                    state.Action = NorfairLavaJumpingEnemyAction.BeginJump;
                    place.Processing = place.Processing.Without(EnemyProcessing.OffScreen);
                }
                else
                {
                    state.ApplyGravity(NorfairLavaJumpingEnemyGravity);
                }
                return;

            default:
                throw new InvalidDataException(
                    $"Norfair lava-jumping enemy has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Carries out the jump animation's finishing cue, the private handshake.</summary>
    private static void ApplyNorfairLavaJumperCue(NorfairLavaJumpingEnemyState state, NorfairLavaJumperCue cue)
    {
        if (cue.Kind != NorfairLavaJumperCueKind.FinishAnimation)
            throw new InvalidOperationException($"Undefined {nameof(NorfairLavaJumperCueKind)} {cue.Kind}.");
        state.AnimationFinished = true;
    }

    /// <summary>Mirrors the preceding parent's frozen timer and upward Y position into the cosmetic follower.</summary>
    private void FollowNorfairLavaJumpingEnemyParent(RoomEnemyPlace follower)
    {
        RoomEnemyPlace parent = GetPrecedingNorfairLavaJumpingEnemyPlace(follower);

        // The follower reads its parent's health, freeze, vertical speed and position in the parent's place; a parent
        // deleted without a kill is read as it left them (RoomEnemyPlace.FoundState), and a killed parent's emptied place
        // ends the follower.
        if (parent.Health == 0)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Deleted);
            return;
        }

        // The pair is checked when the follower is placed, not on every update.
        follower.FrozenTimer = parent.FrozenTimer;
        NorfairLavaJumpingEnemyState parentState = parent.FoundState<NorfairLavaJumpingEnemyState>() ?? throw new InvalidOperationException(
            $"Lava jumper follower {follower.Ordinal} finds no lava jumper in its parent's place {parent.Ordinal}.");
        if (parent.FrozenTimer != 0 || !parentState.YVelocity.IsNegative)
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
    private RoomEnemyPlace GetPrecedingNorfairLavaJumpingEnemyPlace(RoomEnemyPlace follower) => _parents.Of(follower);

    /// <summary>Rejects a follower population entry without a same-species parent directly before it.</summary>
    private void ValidateNorfairLavaJumpingEnemyParent(RoomEnemyPlace follower)
    {
        RoomEnemyPlace parent = GetPrecedingNorfairLavaJumpingEnemyPlace(follower);
        if (parent.Entity is not NorfairLavaJumpingEnemyEntity)
        {
            throw new InvalidDataException(
                $"Norfair lava-jumping follower in place {follower.Ordinal} must immediately follow " +
                "a parent of the same definition. " +
                $"Preceding place: definition={parent.DefinitionId}, " +
                $"health={parent.Health}, spawn={parent.Spawn.Population.SpawnSettings}, contact={parent.Contact}, processing={parent.Processing}, presence={parent.Presence}.");
        }
    }

    /// <summary>Advances position by the parent's signed 1/256-pixel vertical velocity.</summary>
    private static void MoveNorfairLavaJumpingEnemyVertically(
        RoomEnemyPlace place,
        NorfairLavaJumpingEnemyState state)
    {
        place.Position = place.Position.AdvanceY(state.YVelocity.ToSixteenSixteenDelta());
    }

    /// <summary>Starts an animation without restarting the one playing.</summary>
    private static void StartNorfairLavaJumpingEnemyAnimation(
        RoomEnemyPlace place,
        NorfairLavaJumpingEnemyState state,
        NorfairLavaJumperSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, NorfairLavaJumperAnimations.StartingWith(segment));

    /// <summary>Runs a jumping lava jumper's main AI.</summary>
    internal void RunNorfairLavaJumpingEnemyMainAi(RoomEnemyPlace place) =>
        RunNorfairLavaJumpingEnemyMain(
            place,
            place.RequireSpeciesState<NorfairLavaJumpingEnemyState>());

    /// <summary>Advances a lava jumper follower's looping typed animation, which raises no cue.</summary>
    internal static void AdvanceNorfairLavaJumperFollowerTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, NorfairLavaJumperAnimations.Segments,
            static (NorfairLavaJumperFollowerState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"The Squeept follower's animation raised {cue}."));

    /// <summary>Advances a jumping lava jumper's typed animation, whose jump raises its finishing cue.</summary>
    internal static void AdvanceNorfairLavaJumpingEnemyTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, NorfairLavaJumperAnimations.Segments,
            static (NorfairLavaJumpingEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyNorfairLavaJumperCue(state, cue);
                return null;
            });
}
