using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The Norfair rio species owner: its per-room state, AI routines and the interactions it registers.</summary>
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
    /// <summary>Resolves each flame follower's immediately preceding parent slot.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Captures the room RNG, terrain, and slot table needed by Rio AI.</summary>
    internal NorfairRioBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemySlotTable slotTable)
    {
        _room = room;
        _terrain = terrain;
        _slotTable = slotTable;
    }

    /// <summary>Strict wrapped X-distance threshold for a random dive opportunity.</summary>
    private const ushort NorfairRioHorizontalTriggerDistance = 192;
    /// <summary>Signed 8.8 vertical speed decrement applied during dive and return.</summary>
    private const ushort NorfairRioGravity = 32;

    /// <summary>Most recent library-two dive sound request produced during this frame.</summary>
    public SoundEffectId? LastNorfairRioSoundEffect { get; private set; }

    /// <summary>Ports <c>NorfairRio_Init</c> at $A2:C242.</summary>
    internal void InitializeNorfairRio(RoomEnemySlot slot)
    {
        var state = new NorfairRioEnemyState
        {
            AnimationSignal = false,
            FollowerOffset = Offset16<RoomPixels>.Zero,
            IsFollower = EnemyFollowers.FollowsParent(slot.Spawn.Population.Parameter1),
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        if (state.IsFollower)
        {
            ValidateNorfairRioParent(slot);
            state.InstalledProgram = NorfairRioProgram.FlamesAscending;
            state.Program = AnimationProgramCursor<NorfairRioProgram>.Start(NorfairRioProgram.FlamesAscending);
            state.Function = NorfairRioEnemyFunction.FollowParent;
            return;
        }

        state.InstalledProgram = NorfairRioProgram.Idle;
        state.Program = AnimationProgramCursor<NorfairRioProgram>.Start(NorfairRioProgram.Idle);
        state.Function = NorfairRioEnemyFunction.WaitForAttackOpportunity;
    }

    /// <summary>
    /// Ports <c>NorfairRio_Main</c> and functions one through six at $A2:C277-$C40C.
    /// Every parent and follower advances the shared enemy RNG before dispatch.
    /// </summary>
    private void RunNorfairRioMain(
        RoomEnemySlot slot,
        NorfairRioEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        int random = _room.Random.Generate();

        switch (state.Function)
        {
            case NorfairRioEnemyFunction.FollowParent:
                FollowNorfairRioParent(slot, state);
                return;

            case NorfairRioEnemyFunction.WaitForAttackOpportunity:
                // The retail gate tests bits eight and zero of the newly-generated random
                // word, then the strict modular X distance. Failure also consumes a pending
                // animation signal and restores the idle list without starting an attack.
                if (samus is null || (random & RioLaunchDefinitions.AttackRandomBits) == 0 ||
                    !EnemyMath.IsWithinStrictModularDistance(
                        samus.Position.X.Whole,
                        slot.Position.X.Whole,
                        NorfairRioHorizontalTriggerDistance))
                {
                    if (state.AnimationSignal)
                    {
                        state.AnimationSignal = false;
                        InstallNorfairRioInstructionList(
                            slot,
                            state,
                            NorfairRioProgram.Idle);
                    }
                    return;
                }

                // (random >> 1) & 2 selects one of two adjacent signed 8.8 Y speeds; that
                // expression is already the byte offset into the word table.
                state.Velocity = new EightEightVector<RoomPixels>(
                    new SignedEightEight(RioLaunchDefinitions.NorfairXVelocity),
                    new SignedEightEight(RioLaunches.NorfairYVelocity(random)));
                if (unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0)
                    state.Velocity = state.Velocity.WithX(new SignedEightEight(unchecked((ushort)-state.Velocity.X.SignedRawValue)));
                InstallNorfairRioInstructionList(
                    slot,
                    state,
                    NorfairRioProgram.StartDescending);
                state.Function = NorfairRioEnemyFunction.WaitForTakeoffAnimation;
                return;

            case NorfairRioEnemyFunction.WaitForTakeoffAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                InstallNorfairRioInstructionList(
                    slot,
                    state,
                    NorfairRioProgram.Descending);
                state.Function = NorfairRioEnemyFunction.Dive;
                LastNorfairRioSoundEffect = EnemySoundEffectDefinitions.NorfairRioDiveSound;
                return;

            case NorfairRioEnemyFunction.Dive:
                RequireNorfairRioLevel(level);
                MoveNorfairRioHorizontally(level, slot, state);
                if (_terrain.MoveEnemyVertically(
                        level,
                        slot,
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.Y)))
                {
                    BeginNorfairRioReturn(slot, state);
                    return;
                }

                state.Velocity = state.Velocity.WithY(new SignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue - NorfairRioGravity))));
                if (state.Velocity.Y.SignedRawValue < 0)
                    BeginNorfairRioReturn(slot, state);
                return;

            case NorfairRioEnemyFunction.ReturnToPerch:
                RequireNorfairRioLevel(level);
                MoveNorfairRioHorizontally(level, slot, state);
                if (_terrain.MoveEnemyVertically(
                        level,
                        slot,
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.Y)))
                {
                    state.Function = NorfairRioEnemyFunction.FinishLanding;
                    return;
                }

                state.Velocity = state.Velocity.WithY(new SignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue - NorfairRioGravity))));
                if (state.AnimationSignal)
                {
                    state.AnimationSignal = false;
                    InstallNorfairRioInstructionList(
                        slot,
                        state,
                        NorfairRioProgram.Ascending);
                }
                return;

            case NorfairRioEnemyFunction.FinishLanding:
                // Native function six is intentionally a one-frame state: it changes only
                // the dispatch word. Looping list $C179 remains installed, which also keeps
                // the follower half visible while the actor waits for another random attack.
                state.Function = NorfairRioEnemyFunction.WaitForAttackOpportunity;
                return;

            default:
                throw new InvalidDataException(
                    $"Norfair Rio function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Carries out one cue of the Norfair Rio's animations: the swoop-start handshake or a flame placement.</summary>
    private static NorfairRioProgram? ApplyNorfairRioCue(NorfairRioEnemyState state, NorfairRioCue cue)
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
        return null;
    }

    /// <summary>Copies parent freeze, visibility, animation, and flame offset to the adjacent follower.</summary>
    private void FollowNorfairRioParent(
        RoomEnemySlot follower,
        NorfairRioEnemyState followerState)
    {
        RoomEnemySlot parent = GetPrecedingNorfairRioSlot(follower);
        if (parent.Health == 0)
        {
            follower.Properties = follower.Properties.With(EnemyProperties.Deleted);
            return;
        }
        ValidateNorfairRioParent(follower);

        NorfairRioEnemyState parentState = parent.RequireSpeciesState<NorfairRioEnemyState>();
        follower.FrozenTimer = parent.FrozenTimer;
        if (!parent.FrozenTimer.IsZero ||
            parentState.InstalledProgram == NorfairRioProgram.Idle)
        {
            follower.Properties = follower.Properties.With(EnemyProperties.Invisible);
            return;
        }

        // gEnemySpawnData(child)[31] is another deliberate WRAM alias. For an immediately
        // following child, +$7C0 reaches the parent's family-extra block: some_flag is the
        // installed-list word and field_4 is its signed follower Y offset. The drawing-queue
        // offsets in the same routine alias the parent's live X/Y position words.
        NorfairRioProgram followerList = parentState.FollowerOffset.Y < 0
            ? NorfairRioProgram.FlamesDescending
            : NorfairRioProgram.FlamesAscending;
        InstallNorfairRioInstructionList(follower, followerState, followerList);
        follower.Properties = follower.Properties.Without(EnemyProperties.Invisible);
        follower.Position = follower.Position.WithWhole(parent.Position.Whole + parentState.FollowerOffset);
    }

    /// <summary>Reverses vertical travel, installs the ascent animation, and enters the return state.</summary>
    private static void BeginNorfairRioReturn(
        RoomEnemySlot slot,
        NorfairRioEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(new SignedEightEight(unchecked((ushort)-1)));
        InstallNorfairRioInstructionList(
            slot,
            state,
            NorfairRioProgram.StartAscending);
        state.Function = NorfairRioEnemyFunction.ReturnToPerch;
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
                EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.X)))
        {
            state.Velocity = state.Velocity.WithX(new SignedEightEight(unchecked((ushort)-state.Velocity.X.SignedRawValue)));
        }
    }

    /// <summary>Restarts an animation only when it changes.</summary>
    private static void InstallNorfairRioInstructionList(
        RoomEnemySlot slot,
        NorfairRioEnemyState state,
        NorfairRioProgram program) =>
        EnemyProgramAnimation.InstallOnce(slot, state.InstalledProgram, program, installed =>
        {
            state.InstalledProgram = installed;
            state.Program = AnimationProgramCursor<NorfairRioProgram>.Start(installed);
        });

    /// <summary>Returns the physical slot immediately before a follower, rejecting slot zero.</summary>
    private RoomEnemySlot GetPrecedingNorfairRioSlot(RoomEnemySlot follower)
    {
        if (follower.SlotIndex == 0)
            throw new InvalidDataException("A Norfair Rio follower cannot occupy enemy slot zero.");
        return _slotTable.Slots[follower.SlotIndex - 1];
    }

    /// <summary>Requires a flame follower to directly follow a primary Geruta population record.</summary>
    private void ValidateNorfairRioParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingNorfairRioSlot(follower);
        if (parent.DefinitionId != EnemyDefinitionId.Geruta ||
            EnemyFollowers.FollowsParent(parent.Spawn.Population.Parameter1))
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

    /// <summary>Ports NorfairRio's main AI.</summary>
    internal void RunNorfairRioMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunNorfairRioMain(
            slot,
            slot.RequireSpeciesState<NorfairRioEnemyState>(),
            frame.Samus,
            frame.Level);
    }

    /// <summary>Advances NorfairRio's typed animation.</summary>
    internal static void AdvanceNorfairRioTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, NorfairRioAnimations.Programs, NorfairRioFrameDefinitions.ShownFrames.Of,
            static (NorfairRioEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyNorfairRioCue(state, cue));

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastNorfairRioSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueEnemySoundIfAny(LastNorfairRioSoundEffect, maximumQueued: SoundQueueLimit.Max6);
    }
}
