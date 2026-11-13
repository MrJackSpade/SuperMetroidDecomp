using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.LavaJumpers.Artwork;
using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>The Norfair lava-jumping enemy species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class NorfairLavaJumpingEnemyBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Squeept];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new NorfairLavaJumpingEnemyEntity(this, slot, common);

    /// <summary>Supplies one random number per parent and follower AI frame.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The room's enemy places, where each follower finds its parent when it is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>The parent each follower found when it was placed, which its room lists right before it.</summary>
    private readonly EnemyPartLinks<RoomEnemySlot> _parents = new();
    /// <summary>The state of the parent each follower found when it was placed, which a deleted parent leaves (<see cref="DeletedParentLeftovers"/>).</summary>
    private readonly EnemyPartLinks<NorfairLavaJumpingEnemyState> _parentStates = new();

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds lava-jumper behavior to room RNG and physical enemy slots.</summary>
    internal NorfairLavaJumpingEnemyBehavior(EnemyRoomBinding room, EnemySlotTable slotTable, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _slotTable = slotTable;
    }

    /// <summary>Velocity increment applied after each moving frame, 56 256ths of a pixel per update.</summary>
    private static readonly SignedEightEight NorfairLavaJumpingEnemyGravity = SignedEightEight.FromParts(0, 56);
    /// <summary>The rising velocity at and above which, while still rising, the jumper switches to the visible jump list.</summary>
    private static readonly SignedEightEight NorfairLavaJumpingEnemyAnimationSwitchVelocity = -SignedEightEight.FromParts(4, 0);

    /// <summary>Initializes the Norfair lava jumper.</summary>
    internal void InitializeNorfairLavaJumpingEnemy(RoomEnemySlot slot)
    {
        var state = new NorfairLavaJumpingEnemyState
        {
            AnimationFinished = false,
            InstalledProgram = null,
            IsFollower = slot.SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower,
        };
        slot.SetSpeciesState(state);

        if (state.IsFollower)
        {
            // The retail population always places this cosmetic second half immediately
            // after its moving parent. Its main AI relies on aliasing across those two
            // neighbouring enemies, so reject reordered/custom populations explicitly.
            _parents.Place(slot, _slotTable.ListedBefore(slot, 1));
            ValidateNorfairLavaJumpingEnemyParent(slot);
            _parentStates.Place(slot, GetPrecedingNorfairLavaJumpingEnemySlot(slot).RequireSpeciesState<NorfairLavaJumpingEnemyState>());
            state.Playback = new(NorfairLavaJumperAnimations.StartingWith(NorfairLavaJumperProgram.Follower), AnimationPlace.StartOfAnimation);
            state.Function = NorfairLavaJumpingEnemyFunction.FollowParent;
            return;
        }

        // The jumping parent returns to exactly these population coordinates after every
        // arc. Subpositions are deliberately not saved or cleared by the retail jump.
        state.SpawnPosition = slot.Position.Whole;
        state.Playback = new(NorfairLavaJumperAnimations.StartingWith(NorfairLavaJumperProgram.Hidden), AnimationPlace.StartOfAnimation);
        state.Function = NorfairLavaJumpingEnemyFunction.BeginJump;
    }

    /// <summary>
    /// Runs the lava jumper's main behaviour and
    /// its rising, flipping and falling functions. Main advances the shared enemy RNG once for every parent and follower
    /// frame; even the follower discards the number, which still affects later room actors.
    /// </summary>
    internal void RunNorfairLavaJumpingEnemyMain(
        RoomEnemySlot slot,
        NorfairLavaJumpingEnemyState state)
    {
        int random = _room.Random.Generate();

        switch (state.Function)
        {
            case NorfairLavaJumpingEnemyFunction.FollowParent:
                FollowNorfairLavaJumpingEnemyParent(slot);
                return;

            case NorfairLavaJumpingEnemyFunction.BeginJump:
                state.YVelocity = NorfairLavaJumps.InitialVerticalVelocity(random);
                state.Function = NorfairLavaJumpingEnemyFunction.RiseBeforeAnimationSwitch;
                slot.Processing = slot.Processing.With(EnemyProcessing.OffScreen);
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.NorfairLavaJumpingEnemyJumpSound, priority: SoundRequestPriority.Normal);
                return;

            case NorfairLavaJumpingEnemyFunction.RiseBeforeAnimationSwitch:
                MoveNorfairLavaJumpingEnemyVertically(slot, state);
                state.ApplyGravity(NorfairLavaJumpingEnemyGravity);

                // The game compares the velocity unsigned, which holds only for a rising velocity of -1024 or
                // slower: starting jump velocities are near -2048, so the switch comes before the arc apex.
                if (state.YVelocity.IsNegative && state.YVelocity >= NorfairLavaJumpingEnemyAnimationSwitchVelocity)
                {
                    InstallNorfairLavaJumpingEnemyProgram(slot, state, NorfairLavaJumperProgram.Jump);
                    state.Function = NorfairLavaJumpingEnemyFunction.MoveUntilAnimationSignal;
                }
                return;

            case NorfairLavaJumpingEnemyFunction.MoveUntilAnimationSignal:
                MoveNorfairLavaJumpingEnemyVertically(slot, state);
                state.ApplyGravity(NorfairLavaJumpingEnemyGravity);
                if (state.AnimationFinished)
                {
                    state.AnimationFinished = false;
                    state.Function = NorfairLavaJumpingEnemyFunction.FallBackIntoLava;
                }
                return;

            case NorfairLavaJumpingEnemyFunction.FallBackIntoLava:
                MoveNorfairLavaJumpingEnemyVertically(slot, state);

                // Reaching a screen's last block row is how the jumper knows it is back in the lava. Once the
                // actor reaches that row it snaps to its exact population origin, restores
                // the hidden list, and sleeps off-screen until the next jump begins.
                if (RoomGridConversions.BlockRowWithinScreen(slot.Position.Y.Whole) == 15)
                {
                    slot.Position = slot.Position.WithWhole(state.SpawnPosition);
                    InstallNorfairLavaJumpingEnemyProgram(slot, state, NorfairLavaJumperProgram.Hidden);
                    state.Function = NorfairLavaJumpingEnemyFunction.BeginJump;
                    slot.Processing = slot.Processing.Without(EnemyProcessing.OffScreen);
                }
                else
                {
                    state.ApplyGravity(NorfairLavaJumpingEnemyGravity);
                }
                return;

            default:
                throw new InvalidDataException(
                    $"Norfair lava-jumping enemy function {state.Function?.ToString() ?? "none"} is not translated.");
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
    private void FollowNorfairLavaJumpingEnemyParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingNorfairLavaJumpingEnemySlot(follower);

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
    private RoomEnemySlot GetPrecedingNorfairLavaJumpingEnemySlot(RoomEnemySlot follower) => _parents.Of(follower);

    /// <summary>Rejects a follower population record without a same-species parent directly before it.</summary>
    private void ValidateNorfairLavaJumpingEnemyParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingNorfairLavaJumpingEnemySlot(follower);
        if (parent.DefinitionId != EnemyDefinitionId.Squeept ||
            parent.SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower)
        {
            throw new InvalidDataException(
                $"Norfair lava-jumping follower in slot {follower.SlotIndex} must immediately follow " +
                "a parent record of the same definition. " +
                $"Preceding slot: definition={parent.DefinitionId}, " +
                $"health={parent.Health}, spawn={parent.Spawn.Population.SpawnSettings}, contact={parent.Contact}, processing={parent.Processing}, presence={parent.Presence}.");
        }
    }

    /// <summary>Advances position by the parent's signed 8.8 vertical velocity.</summary>
    private static void MoveNorfairLavaJumpingEnemyVertically(
        RoomEnemySlot slot,
        NorfairLavaJumpingEnemyState state)
    {
        slot.Position = slot.Position.AdvanceY(state.YVelocity.ToSixteenSixteenDelta());
    }

    /// <summary>Runs function six without restarting an unchanged animation.</summary>
    private static void InstallNorfairLavaJumpingEnemyProgram(
        RoomEnemySlot slot,
        NorfairLavaJumpingEnemyState state,
        NorfairLavaJumperProgram program) =>
        EnemyProgramAnimation.InstallOnce(slot, state.InstalledProgram, program, installed =>
        {
            state.InstalledProgram = installed;
            state.Playback = new(NorfairLavaJumperAnimations.StartingWith(installed), AnimationPlace.StartOfAnimation);
        });

    /// <summary>Runs the Norfair lava-jumping enemy's main AI.</summary>
    internal void RunNorfairLavaJumpingEnemyMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunNorfairLavaJumpingEnemyMain(
            slot,
            slot.RequireSpeciesState<NorfairLavaJumpingEnemyState>());
    }

    /// <summary>Advances the Norfair lava-jumping enemy's typed animation.</summary>
    internal static void AdvanceNorfairLavaJumpingEnemyTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, NorfairLavaJumperAnimations.Segments, NorfairLavaJumperFrameDefinitions.ShownFrames.Of,
            static (NorfairLavaJumpingEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyNorfairLavaJumperCue(state, cue);
                return null;
            });
}
