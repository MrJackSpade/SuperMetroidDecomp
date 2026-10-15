using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.Core.Fx;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The Norfair rio species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class NorfairRioBehavior : IEnemySpecies
{
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

    /// <summary>Per-slot Rio and flame-follower state, cleared at room load.</summary>
    internal readonly NorfairRioEnemyState?[] _norfairRioStates =
        new NorfairRioEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Most recent library-two dive sound request produced during this frame.</summary>
    public ushort? LastNorfairRioSoundEffect { get; private set; }

    /// <summary>Clears per-slot Rio state and the last dive sound when leaving a room.</summary>
    internal void ResetNorfairRioRoomState()
    {
        Array.Clear(_norfairRioStates);
        LastNorfairRioSoundEffect = null;
    }

    /// <summary>Ports <c>NorfairRio_Init</c> at $A2:C242.</summary>
    internal void InitializeNorfairRio(RoomEnemySlot slot)
    {
        var state = new NorfairRioEnemyState(slot)
        {
            AnimationSignal = false,
            FollowerYOffset = 0,
        };
        _norfairRioStates[slot.SlotIndex] = state;

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
        ushort random = _room.Random.Generate();

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
                    new SnesSignedEightEight(RioLaunchDefinitions.NorfairXVelocity),
                    new SnesSignedEightEight(RioLaunchDefinitions.NorfairYVelocity(random)));
                if (unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0)
                    state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.X.SignedRawValue)));
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
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.Y.RawValue)))
                {
                    BeginNorfairRioReturn(slot, state);
                    return;
                }

                state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue - NorfairRioGravity))));
                if (state.Velocity.Y.SignedRawValue < 0)
                    BeginNorfairRioReturn(slot, state);
                return;

            case NorfairRioEnemyFunction.ReturnToPerch:
                RequireNorfairRioLevel(level);
                MoveNorfairRioHorizontally(level, slot, state);
                if (_terrain.MoveEnemyVertically(
                        level,
                        slot,
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.Y.RawValue)))
                {
                    state.Function = NorfairRioEnemyFunction.FinishLanding;
                    return;
                }

                state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue - NorfairRioGravity))));
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
                    $"Norfair Rio function $A2:{(ushort)state.Function:X4} is not translated.");
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
                state.FollowerYOffset = 8;
                break;
            case NorfairRioCueKind.FlamesAt12:
                state.FollowerYOffset = 12;
                break;
            case NorfairRioCueKind.FlamesAtMinus12:
                state.FollowerYOffset = unchecked((ushort)-12);
                break;
            case NorfairRioCueKind.FlamesAt4:
                state.FollowerYOffset = 4;
                break;
            case NorfairRioCueKind.FlamesAt0:
                state.FollowerYOffset = 0;
                break;
            case NorfairRioCueKind.FlamesAtMinus4:
                state.FollowerYOffset = unchecked((ushort)-4);
                break;
            case NorfairRioCueKind.FlamesAtMinus16:
                state.FollowerYOffset = unchecked((ushort)-16);
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

        NorfairRioEnemyState parentState = RequireNorfairRioState(parent);
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
        NorfairRioProgram followerList = EnemyMath.IsNegative16(parentState.FollowerYOffset)
            ? NorfairRioProgram.FlamesDescending
            : NorfairRioProgram.FlamesAscending;
        InstallNorfairRioInstructionList(follower, followerState, followerList);
        follower.Properties = follower.Properties.Without(EnemyProperties.Invisible);
        follower.Position = follower.Position.WithWhole(new(parent.Position.X.Whole, unchecked((ushort)(
            parent.Position.Y.Whole + (short)parentState.FollowerYOffset))));
    }

    /// <summary>Reverses vertical travel, installs the ascent animation, and enters the return state.</summary>
    private static void BeginNorfairRioReturn(
        RoomEnemySlot slot,
        NorfairRioEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-1)));
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
                EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.X.RawValue)))
        {
            state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.X.SignedRawValue)));
        }
    }

    /// <summary>Restarts an animation only when it changes.</summary>
    private static void InstallNorfairRioInstructionList(
        RoomEnemySlot slot,
        NorfairRioEnemyState state,
        NorfairRioProgram program)
    {
        if (state.InstalledProgram == program)
            return;

        state.InstalledProgram = program;
        state.Program = AnimationProgramCursor<NorfairRioProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Returns the physical slot immediately before a follower, rejecting slot zero.</summary>
    private RoomEnemySlot GetPrecedingNorfairRioSlot(RoomEnemySlot follower)
    {
        if (follower.SlotIndex == 0)
            throw new InvalidDataException("A Norfair Rio follower cannot occupy enemy slot zero.");
        return _slotTable._slots[follower.SlotIndex - 1];
    }

    /// <summary>Requires a flame follower to directly follow a primary Geruta population record.</summary>
    private void ValidateNorfairRioParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingNorfairRioSlot(follower);
        if (parent.EnemyDefinitionPointer != EnemyDefinitionId.Geruta ||
            (parent.Parameter1 & EnemyParameterMasks.FollowerBit) != 0)
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

    /// <summary>Gets initialized AI state for a Rio slot or reports a missing binding.</summary>
    private NorfairRioEnemyState RequireNorfairRioState(RoomEnemySlot slot) =>
        _norfairRioStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Norfair Rio state.");

    /// <summary>Registers initialization for primary and follower Geruta records.</summary>
    internal void RegisterNorfairRioInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Geruta, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Geruta, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeNorfairRio(slot);
        });
    }

    /// <summary>Registers the Rio state machine and its shared-RNG update.</summary>
    internal void RegisterNorfairRioMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Geruta, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Geruta, (slot, frame, nmiFrameCounter8) =>
        {
            RunNorfairRioMain(
                slot,
                RequireNorfairRioState(slot),
                frame.Samus,
                frame.Level);
        });
    }

    /// <summary>Runs the Norfair Rio's and its flames' animations as typed programs (#641).</summary>
    internal void RegisterNorfairRioTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Geruta, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequireNorfairRioState(slot);
            AnimationProgramCursor<NorfairRioProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, NorfairRioAnimations.Programs, ref cursor, NorfairRioFrameDefinitions.Spritemap,
                    (cue, _) => ApplyNorfairRioCue(state, cue));
            state.Program = cursor;
        });


    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetNorfairRioRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastNorfairRioSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastNorfairRioSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
