using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Norfair rio species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class NorfairRioBehavior : IEnemySpecies
{
    private readonly EnemyRoomBinding _room;
    private readonly EnemyTerrain _terrain;
    private readonly EnemySlotTable _slotTable;

    internal NorfairRioBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemySlotTable slotTable)
    {
        _room = room;
        _terrain = terrain;
        _slotTable = slotTable;
    }

    private const ushort NorfairRioHorizontalTriggerDistance = 0x00c0;
    private const ushort NorfairRioGravity = 32;
    private const ushort NorfairRioDiveSound = 0x0065;

    internal readonly NorfairRioEnemyState?[] _norfairRioStates =
        new NorfairRioEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Most recent library-two dive sound request produced during this frame.</summary>
    public ushort? LastNorfairRioSoundEffect { get; private set; }

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
            state.InstalledInstructionList = NorfairRioInstructionProgramDefinitions.FlamesAscending;
            slot.CurrentInstruction = NorfairRioInstructionProgramDefinitions.FlamesAscending;
            state.Function = NorfairRioEnemyFunction.FollowParent;
            return;
        }

        state.InstalledInstructionList = NorfairRioInstructionProgramDefinitions.Idle;
        slot.CurrentInstruction = NorfairRioInstructionProgramDefinitions.Idle;
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
                bool canAttack = samus is not null && (random & 0x0101) != 0 &&
                    EnemyMath.IsWithinStrictModularDistance(
                        samus.Position.X.Whole,
                        slot.Position.X.Whole,
                        NorfairRioHorizontalTriggerDistance);
                if (!canAttack)
                {
                    if (state.AnimationSignal)
                    {
                        state.AnimationSignal = false;
                        InstallNorfairRioInstructionList(
                            slot,
                            state,
                            NorfairRioInstructionProgramDefinitions.Idle);
                    }
                    return;
                }

                // (random >> 1) & 2 selects one of two adjacent signed 8.8 Y speeds; that
                // expression is already the byte offset into the word table.
                state.Velocity = new EightEightVector<RoomPixels>(
                    new SnesSignedEightEight(RioLaunchDefinitions.NorfairXVelocity),
                    new SnesSignedEightEight(RioLaunchDefinitions.NorfairYVelocity(random)));
                if (unchecked((short)(samus!.Position.X.Whole - slot.Position.X.Whole)) < 0)
                    state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.X.SignedRawValue)));
                InstallNorfairRioInstructionList(
                    slot,
                    state,
                    NorfairRioInstructionProgramDefinitions.StartDescending);
                state.Function = NorfairRioEnemyFunction.WaitForTakeoffAnimation;
                return;

            case NorfairRioEnemyFunction.WaitForTakeoffAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                InstallNorfairRioInstructionList(
                    slot,
                    state,
                    NorfairRioInstructionProgramDefinitions.Descending);
                state.Function = NorfairRioEnemyFunction.Dive;
                LastNorfairRioSoundEffect = NorfairRioDiveSound;
                return;

            case NorfairRioEnemyFunction.Dive:
                RequireNorfairRioLevel(level);
                MoveNorfairRioHorizontally(level!, slot, state);
                if (_terrain.MoveEnemyVertically(
                        level!,
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
                MoveNorfairRioHorizontally(level!, slot, state);
                if (_terrain.MoveEnemyVertically(
                        level!,
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
                        NorfairRioInstructionProgramDefinitions.Ascending);
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

    /// <summary>Ports the eleven private no-operand instructions at $A2:C1C9-$C237.</summary>
    private bool TryProcessNorfairRioInstruction(
        RoomEnemySlot slot,
        ushort opcode,
        ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Geruta)
            return false;

        NorfairRioEnemyState state = RequireNorfairRioState(slot);
        if (!Enum.IsDefined((NorfairRioInstruction)opcode))
            return false;
        switch ((NorfairRioInstruction)opcode)
        {
            case NorfairRioInstruction.Instruction_Geruta_SetFinishedSwoopStartAnimationFlag:
                state.AnimationSignal = true;
                break;
            case NorfairRioInstruction.Instruction_Geruta_SetFlamesYOffset_8:
            case NorfairRioInstruction.Instruction_Geruta_SetFlamesYOffset_8_duplicate:
                state.FollowerYOffset = 8;
                break;
            case NorfairRioInstruction.Instruction_Geruta_SetFlamesYOffset_C:
            case NorfairRioInstruction.Instruction_Geruta_SetFlamesYOffset_C_duplicate:
                state.FollowerYOffset = 12;
                break;
            case NorfairRioInstruction.Instruction_Geruta_SetFlamesYOffset_negativeC:
            case NorfairRioInstruction.Instruction_Geruta_SetFlamesYOffset_negativeC_duplicate:
                state.FollowerYOffset = unchecked((ushort)-12);
                break;
            case NorfairRioInstruction.Instruction_Geruta_SetFlamesYOffset_4:
                state.FollowerYOffset = 4;
                break;
            case NorfairRioInstruction.Instruction_Geruta_SetFlamesYOffset_0:
                state.FollowerYOffset = 0;
                break;
            case NorfairRioInstruction.Instruction_Geruta_SetFlamesYOffset_negative4:
                state.FollowerYOffset = unchecked((ushort)-4);
                break;
            case NorfairRioInstruction.Instruction_Geruta_SetFlamesYOffset_negative10:
                state.FollowerYOffset = unchecked((ushort)-16);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(NorfairRioInstruction)} {opcode:X4}.");
        }

        cursor = unchecked((ushort)(cursor + 2));
        return true;
    }

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
            parentState.InstalledInstructionList == NorfairRioInstructionProgramDefinitions.Idle)
        {
            follower.Properties = follower.Properties.With(EnemyProperties.Invisible);
            return;
        }

        // gEnemySpawnData(child)[31] is another deliberate WRAM alias. For an immediately
        // following child, +$7C0 reaches the parent's family-extra block: some_flag is the
        // installed-list word and field_4 is its signed follower Y offset. The drawing-queue
        // offsets in the same routine alias the parent's live X/Y position words.
        ushort followerList = (parentState.FollowerYOffset & 0x8000) != 0
            ? NorfairRioInstructionProgramDefinitions.FlamesDescending
            : NorfairRioInstructionProgramDefinitions.FlamesAscending;
        InstallNorfairRioInstructionList(follower, followerState, followerList);
        follower.Properties = follower.Properties.Without(EnemyProperties.Invisible);
        follower.Position = follower.Position.WithWhole(new(parent.Position.X.Whole, unchecked((ushort)(
            parent.Position.Y.Whole + (short)parentState.FollowerYOffset))));
    }

    private static void BeginNorfairRioReturn(
        RoomEnemySlot slot,
        NorfairRioEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(0xffff));
        InstallNorfairRioInstructionList(
            slot,
            state,
            NorfairRioInstructionProgramDefinitions.StartAscending);
        state.Function = NorfairRioEnemyFunction.ReturnToPerch;
    }

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

    private static void InstallNorfairRioInstructionList(
        RoomEnemySlot slot,
        NorfairRioEnemyState state,
        ushort instructionList)
    {
        if (state.InstalledInstructionList == instructionList)
            return;

        state.InstalledInstructionList = instructionList;
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    private RoomEnemySlot GetPrecedingNorfairRioSlot(RoomEnemySlot follower)
    {
        if (follower.SlotIndex == 0)
            throw new InvalidDataException("A Norfair Rio follower cannot occupy enemy slot zero.");
        return _slotTable._slots[follower.SlotIndex - 1];
    }

    private void ValidateNorfairRioParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingNorfairRioSlot(follower);
        if (parent.EnemyDefinitionPointer != EnemyDefinitionId.Geruta ||
            (parent.Parameter1 & 0x8000) != 0)
        {
            throw new InvalidDataException(
                $"Norfair Rio follower in slot {follower.SlotIndex} must immediately follow " +
                "a parent record of the same definition.");
        }
    }

    private static void RequireNorfairRioLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Norfair Rio movement requires room level data.");
    }

    private NorfairRioEnemyState RequireNorfairRioState(RoomEnemySlot slot) =>
        _norfairRioStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Norfair Rio state.");

    internal void RegisterNorfairRioInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Geruta, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Geruta, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeNorfairRio(slot);
        });
    }

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

    internal void RegisterNorfairRioOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessNorfairRioInstruction(slot, word, ref cursor);
        });
    }

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
