using SuperMetroid.Core.Animation;
using System.Diagnostics.CodeAnalysis;
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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new NorfairRioEntity(this, slot, common);

    /// <summary>Provides the shared RNG advanced by every parent and flame follower.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves horizontal and vertical contact during the dive and return.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>The room's enemy places, where each flame follower finds its parent when it is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>The parent each flame follower found when it was placed, which its room lists right before it.</summary>
    private readonly EnemyPartLinks<RoomEnemySlot> _parents = new();
    /// <summary>The state of the parent each flame follower found when it was placed, which a deleted parent leaves (<see cref="DeletedParentLeftovers"/>).</summary>
    private readonly EnemyPartLinks<NorfairRioEnemyState> _parentStates = new();

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Captures the room RNG, terrain, and slot table needed by Rio AI.</summary>
    internal NorfairRioBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemySlotTable slotTable, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _terrain = terrain;
        _slotTable = slotTable;
    }

    /// <summary>Strict wrapped X-distance threshold for a random dive opportunity.</summary>
    private const int NorfairRioHorizontalTriggerDistance = 192;
    /// <summary>Signed 8.8 vertical speed decrement applied during dive and return.</summary>
    private static readonly SignedEightEight NorfairRioGravity = SignedEightEight.FromParts(0, 32);

    /// <summary>Initializes the Norfair Rio.</summary>
    internal void InitializeNorfairRio(RoomEnemySlot slot)
    {
        var state = new NorfairRioEnemyState
        {
            AnimationSignal = false,
            FollowerOffset = Offset16<RoomPixels>.Zero,
            IsFollower = slot.SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower,
        };
        slot.SetSpeciesState(state);

        if (state.IsFollower)
        {
            _parents.Place(slot, _slotTable.ListedBefore(slot, 1));
            ValidateNorfairRioParent(slot);
            _parentStates.Place(slot, GetPrecedingNorfairRioSlot(slot).RequireSpeciesState<NorfairRioEnemyState>());
            state.InstalledAnimation = NorfairRioSegment.FlamesAscending;
            state.Playback = new(NorfairRioAnimations.StartingWith(NorfairRioSegment.FlamesAscending), AnimationPlace.StartOfAnimation);
            state.Action = NorfairRioAction.FollowParent;
            return;
        }

        state.InstalledAnimation = NorfairRioSegment.Idle;
        state.Playback = new(NorfairRioAnimations.StartingWith(NorfairRioSegment.Idle), AnimationPlace.StartOfAnimation);
        state.Action = NorfairRioAction.WaitForAttackOpportunity;
    }

    /// <summary>
    /// Runs the Norfair Rio's main AI and each action it dispatches.
    /// Every parent and follower advances the shared enemy RNG before dispatch.
    /// </summary>
    private void RunNorfairRioMain(
        RoomEnemySlot slot,
        NorfairRioEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        int random = _room.Random.Generate();

        switch (state.Action)
        {
            case NorfairRioAction.FollowParent:
                FollowNorfairRioParent(slot, state);
                return;

            case NorfairRioAction.WaitForAttackOpportunity:
                // The retail gate tests two parts of the newly generated random
                // number, then the strict modular X distance. Failure also consumes a pending
                // animation signal and restores the idle animation without starting an attack.
                if (samus is null ||
                    (!RioLaunchDefinitions.AttackChanceLowBit.IsSetIn(random) && !RioLaunchDefinitions.AttackChanceHighBit.IsSetIn(random)) ||
                    !EnemyMath.IsWithinStrictModularDistance(
                        samus.Position.X.Whole,
                        slot.Position.X.Whole,
                        NorfairRioHorizontalTriggerDistance))
                {
                    if (state.AnimationSignal)
                    {
                        state.AnimationSignal = false;
                        StartNorfairRioAnimation(
                            slot,
                            state,
                            NorfairRioSegment.Idle);
                    }
                    return;
                }

                // The same random number picks the dive's fast or slow downward launch.
                state.Velocity = new EightEightVector<RoomPixels>(
                    RioLaunchDefinitions.NorfairXVelocity,
                    RioLaunches.NorfairYVelocity(random));
                if (CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0)
                    state.Velocity = state.Velocity.ReversedX();
                StartNorfairRioAnimation(
                    slot,
                    state,
                    NorfairRioSegment.StartDescending);
                state.Action = NorfairRioAction.WaitForTakeoffAnimation;
                return;

            case NorfairRioAction.WaitForTakeoffAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                StartNorfairRioAnimation(
                    slot,
                    state,
                    NorfairRioSegment.Descending);
                state.Action = NorfairRioAction.Dive;
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.NorfairRioDiveSound, priority: SoundRequestPriority.Normal);
                return;

            case NorfairRioAction.Dive:
                RequireNorfairRioLevel(level);
                MoveNorfairRioHorizontally(level, slot, state);
                if (_terrain.MoveEnemyVertically(
                        level,
                        slot,
                        state.Velocity.Y.ToSixteenSixteenDelta()))
                {
                    BeginNorfairRioReturn(slot, state);
                    return;
                }

                state.Velocity = state.Velocity.WithY(state.Velocity.Y - NorfairRioGravity);
                if (state.Velocity.Y.IsNegative)
                    BeginNorfairRioReturn(slot, state);
                return;

            case NorfairRioAction.ReturnToPerch:
                RequireNorfairRioLevel(level);
                MoveNorfairRioHorizontally(level, slot, state);
                if (_terrain.MoveEnemyVertically(
                        level,
                        slot,
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
                        slot,
                        state,
                        NorfairRioSegment.Ascending);
                }
                return;

            case NorfairRioAction.FinishLanding:
                // Finishing the landing is intentionally a one-frame state: it changes only
                // the dispatched action. The looping idle animation remains installed, which also keeps
                // the follower half visible while the actor waits for another random attack.
                state.Action = NorfairRioAction.WaitForAttackOpportunity;
                return;

            default:
                throw new InvalidDataException(
                    $"Norfair Rio has no behaviour for action {state.Action?.ToString() ?? "none"}.");
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
            case NorfairRioCueKind.FlamesAt8:
                state.FollowerOffset = new Offset16<RoomPixels>(0, 8);
                break;
            case NorfairRioCueKind.FlamesAt12:
                state.FollowerOffset = new Offset16<RoomPixels>(0, 12);
                break;
            case NorfairRioCueKind.FlamesAtMinus12:
                state.FollowerOffset = new Offset16<RoomPixels>(0, -12);
                break;
            case NorfairRioCueKind.FlamesAt4:
                state.FollowerOffset = new Offset16<RoomPixels>(0, 4);
                break;
            case NorfairRioCueKind.FlamesAt0:
                state.FollowerOffset = Offset16<RoomPixels>.Zero;
                break;
            case NorfairRioCueKind.FlamesAtMinus4:
                state.FollowerOffset = new Offset16<RoomPixels>(0, -4);
                break;
            case NorfairRioCueKind.FlamesAtMinus16:
                state.FollowerOffset = new Offset16<RoomPixels>(0, -16);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(NorfairRioCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Copies parent freeze, visibility, animation, and flame offset to the adjacent follower.</summary>
    private void FollowNorfairRioParent(
        RoomEnemySlot follower,
        NorfairRioEnemyState followerState)
    {
        RoomEnemySlot parent = GetPrecedingNorfairRioSlot(follower);
        if (parent.Health == 0)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Deleted);
            return;
        }
        // The pair is checked when the follower is placed; a parent deleted without a kill is read as it left its place and
        // its animation (DeletedParentLeftovers).
        NorfairRioEnemyState parentState = DeletedParentLeftovers.Parent(
            parent.SpeciesState<NorfairRioEnemyState>(), _parentStates.Of(follower));
        follower.FrozenTimer = parent.FrozenTimer;
        if (parent.FrozenTimer != 0 ||
            parentState.InstalledAnimation == NorfairRioSegment.Idle)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Hidden);
            return;
        }

        // The follower's spawn reads the parent's own state: its installed animation and its signed
        // follower Y offset. Its drawing position follows the parent's live position.
        NorfairRioSegment followerList = parentState.FollowerOffset.Y < 0
            ? NorfairRioSegment.FlamesDescending
            : NorfairRioSegment.FlamesAscending;
        StartNorfairRioAnimation(follower, followerState, followerList);
        follower.Presence = follower.Presence.Without(EnemyPresence.Hidden);
        follower.Position = follower.Position.WithWhole(parent.Position.Whole + parentState.FollowerOffset);
    }

    /// <summary>Reverses vertical travel, installs the ascent animation, and enters the return state.</summary>
    private static void BeginNorfairRioReturn(
        RoomEnemySlot slot,
        NorfairRioEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(-SignedEightEight.FromParts(0, 1));
        StartNorfairRioAnimation(
            slot,
            state,
            NorfairRioSegment.StartAscending);
        state.Action = NorfairRioAction.ReturnToPerch;
    }

    /// <summary>Moves across room terrain and reverses horizontal speed on contact.</summary>
    private void MoveNorfairRioHorizontally(
        RoomLevelData level,
        RoomEnemySlot slot,
        NorfairRioEnemyState state)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                state.Velocity.X.ToSixteenSixteenDelta()))
        {
            state.Velocity = state.Velocity.ReversedX();
        }
    }

    /// <summary>Restarts an animation only when it changes.</summary>
    private static void StartNorfairRioAnimation(
        RoomEnemySlot slot,
        NorfairRioEnemyState state,
        NorfairRioSegment segment) =>
        EnemyAnimationPlayback.StartOnce(slot, state.InstalledAnimation, segment, installed =>
        {
            state.InstalledAnimation = installed;
            state.Playback = new(NorfairRioAnimations.StartingWith(installed), AnimationPlace.StartOfAnimation);
        });

    /// <summary>The parent the follower found when it was placed, the enemy its room lists directly before it.</summary>
    private RoomEnemySlot GetPrecedingNorfairRioSlot(RoomEnemySlot follower) => _parents.Of(follower);

    /// <summary>Requires a flame follower to directly follow a primary Geruta population record.</summary>
    private void ValidateNorfairRioParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingNorfairRioSlot(follower);
        if (parent.DefinitionId != EnemyDefinitionId.Geruta ||
            parent.SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower)
        {
            throw new InvalidDataException(
                $"Norfair Rio follower in slot {follower.SlotIndex} must immediately follow " +
                "a parent record of the same definition.");
        }
    }

    /// <summary>Rejects a movement update without the room terrain data it needs.</summary>
    private static void RequireNorfairRioLevel([NotNull] RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Norfair Rio movement requires room level data.");
    }

    /// <summary>Runs the Norfair Rio's main AI.</summary>
    internal void RunNorfairRioMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunNorfairRioMain(
            slot,
            slot.RequireSpeciesState<NorfairRioEnemyState>(),
            frame.Samus,
            frame.Level);
    }

    /// <summary>Advances the Norfair Rio's typed animation.</summary>
    internal static void AdvanceNorfairRioTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, NorfairRioAnimations.Segments,
            static (NorfairRioEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyNorfairRioCue(state, cue);
                return null;
            });
}
