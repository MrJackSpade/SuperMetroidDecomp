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
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new NorfairLavaJumpingEnemyEntity(this, place, common);

    /// <summary>Supplies one random number per parent and follower AI frame.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The room's enemy places, where each follower finds its parent when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The parent each follower found when it was placed, which its room lists right before it.</summary>
    private readonly EnemyPartLinks<RoomEnemyPlace> _parents = new();
    /// <summary>The state of the parent each follower found when it was placed, which a deleted parent leaves (<see cref="DeletedParentLeftovers"/>).</summary>
    private readonly EnemyPartLinks<NorfairLavaJumpingEnemyState> _parentStates = new();

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

    /// <summary>Initializes the Norfair lava jumper.</summary>
    internal void InitializeNorfairLavaJumpingEnemy(RoomEnemyPlace place)
    {
        var state = new NorfairLavaJumpingEnemyState
        {
            AnimationFinished = false,
            InstalledAnimation = null,
            IsFollower = place.SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower,
        };
        place.SetSpeciesState(state);

        if (state.IsFollower)
        {
            // The retail population always places this cosmetic second half immediately after its moving parent, which
            // it finds when it is placed and follows from then on; a population that places anything else there is
            // refused.
            _parents.Place(place, _places.ListedBefore(place, 1));
            ValidateNorfairLavaJumpingEnemyParent(place);
            _parentStates.Place(place, GetPrecedingNorfairLavaJumpingEnemyPlace(place).RequireSpeciesState<NorfairLavaJumpingEnemyState>());
            state.Playback = new(NorfairLavaJumperAnimations.StartingWith(NorfairLavaJumperSegment.Follower), AnimationPlace.StartOfAnimation);
            state.Action = NorfairLavaJumpingEnemyAction.FollowParent;
            return;
        }

        // The jumping parent returns to exactly these population coordinates after every
        // arc. Subpositions are deliberately not saved or cleared by the retail jump.
        state.SpawnPosition = place.Position.Whole;
        state.Playback = new(NorfairLavaJumperAnimations.StartingWith(NorfairLavaJumperSegment.Hidden), AnimationPlace.StartOfAnimation);
        state.Action = NorfairLavaJumpingEnemyAction.BeginJump;
    }

    /// <summary>
    /// Runs the lava jumper's main behaviour and
    /// its rising, flipping and falling actions. Main advances the shared enemy RNG once for every parent and follower
    /// frame; even the follower discards the number, which still affects later room actors.
    /// </summary>
    internal void RunNorfairLavaJumpingEnemyMain(
        RoomEnemyPlace place,
        NorfairLavaJumpingEnemyState state)
    {
        int random = _room.Random.Generate();

        switch (state.Action)
        {
            case NorfairLavaJumpingEnemyAction.FollowParent:
                FollowNorfairLavaJumpingEnemyParent(place);
                return;

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
                    $"Norfair lava-jumping enemy has no behaviour for action {state.Action?.ToString() ?? "none"}.");
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

        // The follower reads its parent's health, freeze, vertical speed and position; a parent deleted without a kill is
        // read as it left them (DeletedParentLeftovers), and a killed parent's emptied place ends the follower.
        if (parent.Health == 0)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Deleted);
            return;
        }

        // The pair is checked when the follower is placed, not on every update.
        follower.FrozenTimer = parent.FrozenTimer;
        NorfairLavaJumpingEnemyState parentState = DeletedParentLeftovers.Parent(
            parent.SpeciesState<NorfairLavaJumpingEnemyState>(), _parentStates.Of(follower));
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
        if (parent.DefinitionId != EnemyDefinitionId.Squeept ||
            parent.SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower)
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

    /// <summary>Starts an animation without restarting an unchanged one.</summary>
    private static void StartNorfairLavaJumpingEnemyAnimation(
        RoomEnemyPlace place,
        NorfairLavaJumpingEnemyState state,
        NorfairLavaJumperSegment segment) =>
        EnemyAnimationPlayback.StartOnce(place, state.InstalledAnimation, segment, installed =>
        {
            state.InstalledAnimation = installed;
            state.Playback = new(NorfairLavaJumperAnimations.StartingWith(installed), AnimationPlace.StartOfAnimation);
        });

    /// <summary>Runs the Norfair lava-jumping enemy's main AI.</summary>
    internal void RunNorfairLavaJumpingEnemyMainAi(RoomEnemyPlace place, IEnemyAiFrame _)
    {
        RunNorfairLavaJumpingEnemyMain(
            place,
            place.RequireSpeciesState<NorfairLavaJumpingEnemyState>());
    }

    /// <summary>Advances the Norfair lava-jumping enemy's typed animation.</summary>
    internal static void AdvanceNorfairLavaJumpingEnemyTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, NorfairLavaJumperAnimations.Segments,
            static (NorfairLavaJumpingEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyNorfairLavaJumperCue(state, cue);
                return null;
            });
}
