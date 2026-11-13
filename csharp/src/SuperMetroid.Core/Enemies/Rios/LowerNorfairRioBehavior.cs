using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The Lower Norfair rio species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class LowerNorfairRioBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Holtz];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new LowerNorfairRioEntity(this, slot, common);

    /// <summary>Supplies the shared random draw consumed before every parent and follower AI dispatch.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves wall and floor collisions during Rio dives and returns.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>The room's enemy places, where each flame follower finds its parent when it is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>The parent each flame follower found when it was placed, which its room lists right before it.</summary>
    private readonly EnemyPartLinks<RoomEnemySlot> _parents = new();

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds Holtz parent and follower behavior to room RNG, collision, and slot order.</summary>
    internal LowerNorfairRioBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemySlotTable slotTable, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _terrain = terrain;
        _slotTable = slotTable;
    }

    /// <summary>Samus must be strictly within 112 horizontal pixels to permit a swoop.</summary>
    private const int LowerNorfairRioHorizontalTriggerDistance = 112;
    /// <summary>Each flight update subtracts 32 units from the signed 8.8 vertical velocity.</summary>
    private static readonly SignedEightEight LowerNorfairRioGravity = SignedEightEight.FromParts(0, 32);

    /// <summary>Initializes the Lower Norfair Rio.</summary>
    internal void InitializeLowerNorfairRio(RoomEnemySlot slot)
    {
        var state = new LowerNorfairRioEnemyState
        {
            AnimationSignal = false,
            FollowerVisible = false,
            IsFollower = slot.SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower,
        };
        slot.SetSpeciesState(state);

        if (state.IsFollower)
        {
            _parents.Place(slot, _slotTable.ListedBefore(slot, 1));
            ValidateLowerNorfairRioParent(slot);
            state.InstalledProgram = LowerNorfairRioProgram.Flames;
            state.Playback = new(LowerNorfairRioAnimations.StartingWith(LowerNorfairRioProgram.Flames), AnimationPlace.StartOfAnimation);
            state.Function = LowerNorfairRioEnemyFunction.FollowParent;
            return;
        }

        state.InstalledProgram = LowerNorfairRioProgram.Idle;
        state.Playback = new(LowerNorfairRioAnimations.StartingWith(LowerNorfairRioProgram.Idle), AnimationPlace.StartOfAnimation);
        state.Function = LowerNorfairRioEnemyFunction.WaitForAttackOpportunity;
    }

    /// <summary>
    /// Runs the Lower Norfair Rio's main AI and functions one through six.
    /// The shared RNG advances before every parent and follower dispatch, exactly as retail.
    /// </summary>
    private void RunLowerNorfairRioMain(
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        int random = _room.Random.Generate();

        switch (state.Function)
        {
            case LowerNorfairRioEnemyFunction.FollowParent:
                FollowLowerNorfairRioParent(slot);
                return;

            case LowerNorfairRioEnemyFunction.WaitForAttackOpportunity:
                if (samus is null ||
                    (!RioLaunchDefinitions.AttackChanceLowBit.IsSetIn(random) && !RioLaunchDefinitions.AttackChanceHighBit.IsSetIn(random)) ||
                    !EnemyMath.IsWithinStrictModularDistance(
                        samus.Position.X.Whole,
                        slot.Position.X.Whole,
                        LowerNorfairRioHorizontalTriggerDistance))
                {
                    // Unlike upper Norfair Rio, this variant clears the handshake and calls
                    // its list installer on every rejected frame. The helper itself prevents
                    // an unchanged attack animation from restarting.
                    state.AnimationSignal = false;
                    InstallLowerNorfairRioAnimationScript(
                        slot,
                        state,
                        LowerNorfairRioProgram.Idle);
                    return;
                }

                state.Velocity = new(
                    RioLaunchDefinitions.LowerNorfairXVelocity,
                    RioLaunchDefinitions.LowerNorfairYVelocity);
                if (CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0)
                    NegateLowerNorfairRioXVelocity(state);
                InstallLowerNorfairRioAnimationScript(
                    slot,
                    state,
                    LowerNorfairRioProgram.PrepareToSwoop);
                state.Function = LowerNorfairRioEnemyFunction.WaitForTakeoffAnimation;
                return;

            case LowerNorfairRioEnemyFunction.WaitForTakeoffAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                InstallLowerNorfairRioAnimationScript(
                    slot,
                    state,
                    LowerNorfairRioProgram.Descending);
                state.Function = LowerNorfairRioEnemyFunction.Dive;
                return;

            case LowerNorfairRioEnemyFunction.Dive:
                RequireLowerNorfairRioLevel(level);
                MoveLowerNorfairRioHorizontally(level, slot, state);
                if (_terrain.MoveEnemyVertically(
                        level,
                        slot,
                        state.Velocity.Y.ToSixteenSixteenDelta()))
                {
                    BeginLowerNorfairRioReturn(slot, state);
                    return;
                }

                ApplyLowerNorfairRioGravity(state);
                if (state.Velocity.Y.IsNegative)
                    BeginLowerNorfairRioReturn(slot, state);
                return;

            case LowerNorfairRioEnemyFunction.ReturnToPerch:
                RequireLowerNorfairRioLevel(level);
                MoveLowerNorfairRioHorizontally(level, slot, state);
                if (_terrain.MoveEnemyVertically(
                        level,
                        slot,
                        state.Velocity.Y.ToSixteenSixteenDelta()))
                {
                    InstallLowerNorfairRioAnimationScript(
                        slot,
                        state,
                        LowerNorfairRioProgram.Cooldown);
                    state.Function = LowerNorfairRioEnemyFunction.WaitForLandingAnimation;
                    return;
                }

                ApplyLowerNorfairRioGravity(state);
                if (state.AnimationSignal)
                {
                    state.AnimationSignal = false;
                    InstallLowerNorfairRioAnimationScript(
                        slot,
                        state,
                        LowerNorfairRioProgram.AscendingPart2);
                }
                return;

            case LowerNorfairRioEnemyFunction.WaitForLandingAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                InstallLowerNorfairRioAnimationScript(
                    slot,
                    state,
                    LowerNorfairRioProgram.Cooldown);
                state.Function = LowerNorfairRioEnemyFunction.WaitForAttackOpportunity;
                return;

            default:
                throw new InvalidDataException(
                    $"Lower Norfair Rio function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Carries out one cue of the Lower Norfair Rio's animations: the finished handshake and flame visibility.</summary>
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

    /// <summary>Mirrors the preceding parent's freeze and visibility, placing live flames twelve pixels below it.</summary>
    private void FollowLowerNorfairRioParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingLowerNorfairRioSlot(follower);
        if (parent.Health == 0)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Deleted);
            return;
        }
        ValidateLowerNorfairRioParent(follower);

        LowerNorfairRioEnemyState parentState = parent.RequireSpeciesState<LowerNorfairRioEnemyState>();
        follower.FrozenTimer = parent.FrozenTimer;
        if (parent.FrozenTimer != 0 || !parentState.FollowerVisible)
        {
            follower.Presence = follower.Presence.With(EnemyPresence.Hidden);
            return;
        }

        // The follower again reads the preceding parent's visibility switch, a Boolean here
        // rather than a signed offset. The follower takes the parent's live position and
        // this variant adds 12 pixels.
        follower.Presence = follower.Presence.Without(EnemyPresence.Hidden);
        follower.Position = follower.Position.WithWhole(new(parent.Position.X.Whole, CoordinateAxis.Wrap(parent.Position.Y.Whole + 12)));
    }

    /// <summary>Sets upward return velocity, animation, function, and its enemy-library sound event.</summary>
    private void BeginLowerNorfairRioReturn(
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(-SignedEightEight.FromParts(0, 1));
        InstallLowerNorfairRioAnimationScript(
            slot,
            state,
            LowerNorfairRioProgram.AscendingPart1);
        state.Function = LowerNorfairRioEnemyFunction.ReturnToPerch;
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.LowerNorfairRioReturnSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Advances the Rio horizontally and reverses its velocity on a wall collision.</summary>
    private void MoveLowerNorfairRioHorizontally(
        RoomLevelData level,
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                state.Velocity.X.ToSixteenSixteenDelta()))
        {
            NegateLowerNorfairRioXVelocity(state);
        }
    }

    /// <summary>Reverses the full signed 8.8 horizontal velocity after targeting or collision.</summary>
    private static void NegateLowerNorfairRioXVelocity(LowerNorfairRioEnemyState state) =>
        state.Velocity = state.Velocity.ReversedX();

    /// <summary>Applies the negative 8.8 vertical acceleration after a flight step.</summary>
    private static void ApplyLowerNorfairRioGravity(LowerNorfairRioEnemyState state) =>
        state.Velocity = state.Velocity.WithY(state.Velocity.Y - LowerNorfairRioGravity);

    /// <summary>Restarts a different Rio animation list while leaving an unchanged list's timer intact.</summary>
    private static void InstallLowerNorfairRioAnimationScript(
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state,
        LowerNorfairRioProgram animationScript) =>
        EnemyProgramAnimation.InstallOnce(slot, state.InstalledProgram, animationScript, installed =>
        {
            state.InstalledProgram = installed;
            state.Playback = new(LowerNorfairRioAnimations.StartingWith(installed), AnimationPlace.StartOfAnimation);
        });

    /// <summary>The parent the flame follower found when it was placed, the enemy its room lists directly before it.</summary>
    private RoomEnemySlot GetPrecedingLowerNorfairRioSlot(RoomEnemySlot follower) => _parents.Of(follower);

    /// <summary>Requires the preceding slot to be an unfollowing Holtz parent record.</summary>
    private void ValidateLowerNorfairRioParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingLowerNorfairRioSlot(follower);
        if (parent.DefinitionId != EnemyDefinitionId.Holtz ||
            parent.SpawnSettings<EnemyFollowerSpawnSettings>().Role == EnemyFollowerRole.Follower)
        {
            throw new InvalidDataException(
                $"Lower Norfair Rio follower in slot {follower.SlotIndex} must immediately " +
                "follow a parent record of the same definition.");
        }
    }

    /// <summary>Rejects flight movement before collision data for the room is available.</summary>
    private static void RequireLowerNorfairRioLevel([NotNull] RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Lower Norfair Rio movement requires room level data.");
    }

    /// <summary>Runs the Lower Norfair Rio's main AI.</summary>
    internal void RunLowerNorfairRioMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunLowerNorfairRioMain(
            slot,
            slot.RequireSpeciesState<LowerNorfairRioEnemyState>(),
            frame.Samus,
            frame.Level);
    }

    /// <summary>Advances the Lower Norfair Rio's typed animation.</summary>
    internal static void AdvanceLowerNorfairRioTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, LowerNorfairRioAnimations.Segments, LowerNorfairRioFrameDefinitions.ShownFrames.Of,
            static (LowerNorfairRioEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyLowerNorfairRioCue(state, cue);
                return null;
            });
}
