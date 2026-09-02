using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Exact bank-$A2 function words dispatched by Norfair Rio main AI.</summary>
public enum NorfairRioEnemyFunction : ushort
{
    FollowParent = 0xc281,
    WaitForAttackOpportunity = 0xc2e7,
    WaitForTakeoffAnimation = 0xc33f,
    Dive = 0xc361,
    ReturnToPerch = 0xc3b1,
    FinishLanding = 0xc406,
}

/// <summary>
/// Typed projection of Norfair Rio's common A/B/F variables and three family-extra words.
/// The extra words live in bank-$7E family RAM rather than the ordinary enemy record, so
/// they remain explicit host fields while velocities/function stay backed by the slot.
/// </summary>
public sealed class NorfairRioEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal NorfairRioEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Extra word $00: last instruction list installed by function seven.</summary>
    public ushort InstalledInstructionList { get; internal set; }

    /// <summary>Extra word $01: no-operand animation handshake set by opcode $C1C9.</summary>
    public bool AnimationSignal { get; internal set; }

    /// <summary>
    /// Extra word $02: signed vertical placement of the follower half relative to its
    /// parent. Ten private animation opcodes publish the offset directly from ROM lists.
    /// </summary>
    public ushort FollowerYOffset { get; internal set; }

    /// <summary>Signed 8.8 vertical velocity stored in native common variable A.</summary>
    public ushort YVelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Signed 8.8 horizontal velocity stored in native common variable B.</summary>
    public ushort XVelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    public NorfairRioEnemyFunction Function
    {
        get => (NorfairRioEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    public bool IsFollower => (_slot.Parameter1 & 0x8000) != 0;
}

/// <summary>Literal translation of Norfair Rio enemy AI $A2:C1C9-$C41F.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort NorfairRioDefinition = 0xd2ff;

    private const ushort NorfairRioIdleInstructionList = 0xc0f1;
    private const ushort NorfairRioTakeoffInstructionList = 0xc107;
    private const ushort NorfairRioDiveInstructionList = 0xc12f;
    private const ushort NorfairRioReturnInstructionList = 0xc145;
    private const ushort NorfairRioLateReturnInstructionList = 0xc179;
    private const ushort NorfairRioFollowerBelowInstructionList = 0xc18f;
    private const ushort NorfairRioFollowerAboveInstructionList = 0xc1a3;
    private const ushort NorfairRioAnimationSignalInstruction = 0xc1c9;
    private const ushort NorfairRioHorizontalTriggerDistance = 0x00c0;
    private const ushort NorfairRioGravity = 32;
    private const ushort NorfairRioDiveSound = 0x0065;
    private const int NorfairRioYVelocityTableAddress = 0xa2c1c1;
    private const int NorfairRioXVelocityAddress = 0xa2c1c5;

    private readonly NorfairRioEnemyState?[] _norfairRioStates =
        new NorfairRioEnemyState?[MaximumEnemyCount];

    public IReadOnlyList<NorfairRioEnemyState?> NorfairRioStates => _norfairRioStates;

    /// <summary>Most recent library-two dive sound request produced during this frame.</summary>
    public ushort? LastNorfairRioSoundEffect { get; private set; }

    private void ResetNorfairRioRoomState()
    {
        Array.Clear(_norfairRioStates);
        LastNorfairRioSoundEffect = null;
    }

    /// <summary>Ports <c>NorfairRio_Init</c> at $A2:C242.</summary>
    private void InitializeNorfairRio(RoomEnemySlot slot)
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
            state.InstalledInstructionList = NorfairRioFollowerBelowInstructionList;
            slot.CurrentInstruction = NorfairRioFollowerBelowInstructionList;
            state.Function = NorfairRioEnemyFunction.FollowParent;
            return;
        }

        state.InstalledInstructionList = NorfairRioIdleInstructionList;
        slot.CurrentInstruction = NorfairRioIdleInstructionList;
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
        ushort random = _nextRandom!();

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
                    IsWithinStrictModularDistance(
                        samus.XPosition,
                        slot.XPosition,
                        NorfairRioHorizontalTriggerDistance);
                if (!canAttack)
                {
                    if (state.AnimationSignal)
                    {
                        state.AnimationSignal = false;
                        InstallNorfairRioInstructionList(
                            slot,
                            state,
                            NorfairRioIdleInstructionList);
                    }
                    return;
                }

                // (random >> 1) & 2 selects one of two adjacent signed 8.8 Y speeds; that
                // expression is already the byte offset into the word table.
                state.YVelocity = ReadWord(
                    _bus!,
                    NorfairRioYVelocityTableAddress + ((random >> 1) & 0x0002));
                state.XVelocity = ReadWord(_bus!, NorfairRioXVelocityAddress);
                if (unchecked((short)(samus!.XPosition - slot.XPosition)) < 0)
                    state.XVelocity = unchecked((ushort)-(short)state.XVelocity);
                InstallNorfairRioInstructionList(
                    slot,
                    state,
                    NorfairRioTakeoffInstructionList);
                state.Function = NorfairRioEnemyFunction.WaitForTakeoffAnimation;
                return;

            case NorfairRioEnemyFunction.WaitForTakeoffAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                InstallNorfairRioInstructionList(
                    slot,
                    state,
                    NorfairRioDiveInstructionList);
                state.Function = NorfairRioEnemyFunction.Dive;
                LastNorfairRioSoundEffect = NorfairRioDiveSound;
                return;

            case NorfairRioEnemyFunction.Dive:
                RequireNorfairRioLevel(level);
                MoveNorfairRioHorizontally(level!, slot, state);
                if (MoveEnemyVertically(
                        level!,
                        slot,
                        ToEightBitVelocityDisplacement(state.YVelocity)))
                {
                    BeginNorfairRioReturn(slot, state);
                    return;
                }

                state.YVelocity = unchecked((ushort)(state.YVelocity - NorfairRioGravity));
                if (unchecked((short)state.YVelocity) < 0)
                    BeginNorfairRioReturn(slot, state);
                return;

            case NorfairRioEnemyFunction.ReturnToPerch:
                RequireNorfairRioLevel(level);
                MoveNorfairRioHorizontally(level!, slot, state);
                if (MoveEnemyVertically(
                        level!,
                        slot,
                        ToEightBitVelocityDisplacement(state.YVelocity)))
                {
                    state.Function = NorfairRioEnemyFunction.FinishLanding;
                    return;
                }

                state.YVelocity = unchecked((ushort)(state.YVelocity - NorfairRioGravity));
                if (state.AnimationSignal)
                {
                    state.AnimationSignal = false;
                    InstallNorfairRioInstructionList(
                        slot,
                        state,
                        NorfairRioLateReturnInstructionList);
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
        if (slot.EnemyDefinitionPointer != NorfairRioDefinition)
            return false;

        NorfairRioEnemyState state = RequireNorfairRioState(slot);
        switch (opcode)
        {
            case NorfairRioAnimationSignalInstruction:
                state.AnimationSignal = true;
                break;
            case NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_8:
            case NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_8_duplicate:
                state.FollowerYOffset = 8;
                break;
            case NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_C:
            case NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_C_duplicate:
                state.FollowerYOffset = 12;
                break;
            case NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negativeC:
            case NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negativeC_duplicate:
                state.FollowerYOffset = unchecked((ushort)-12);
                break;
            case NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_4:
                state.FollowerYOffset = 4;
                break;
            case NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_0:
                state.FollowerYOffset = 0;
                break;
            case NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negative4:
                state.FollowerYOffset = unchecked((ushort)-4);
                break;
            case NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negative10:
                state.FollowerYOffset = unchecked((ushort)-16);
                break;
            default:
                return false;
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
        if (parent.FrozenTimer != 0 ||
            parentState.InstalledInstructionList == NorfairRioIdleInstructionList)
        {
            follower.Properties = follower.Properties.With(EnemyProperties.Invisible);
            return;
        }

        // gEnemySpawnData(child)[31] is another deliberate WRAM alias. For an immediately
        // following child, +$7C0 reaches the parent's family-extra block: some_flag is the
        // installed-list word and field_4 is its signed follower Y offset. The drawing-queue
        // offsets in the same routine alias the parent's live X/Y position words.
        ushort followerList = (parentState.FollowerYOffset & 0x8000) != 0
            ? NorfairRioFollowerAboveInstructionList
            : NorfairRioFollowerBelowInstructionList;
        InstallNorfairRioInstructionList(follower, followerState, followerList);
        follower.Properties = follower.Properties.Without(EnemyProperties.Invisible);
        follower.XPosition = parent.XPosition;
        follower.YPosition = unchecked((ushort)(
            parent.YPosition + (short)parentState.FollowerYOffset));
    }

    private static void BeginNorfairRioReturn(
        RoomEnemySlot slot,
        NorfairRioEnemyState state)
    {
        state.YVelocity = 0xffff;
        InstallNorfairRioInstructionList(
            slot,
            state,
            NorfairRioReturnInstructionList);
        state.Function = NorfairRioEnemyFunction.ReturnToPerch;
    }

    private void MoveNorfairRioHorizontally(
        RoomLevelData level,
        RoomEnemySlot slot,
        NorfairRioEnemyState state)
    {
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                ToEightBitVelocityDisplacement(state.XVelocity)))
        {
            state.XVelocity = unchecked((ushort)-(short)state.XVelocity);
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
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private RoomEnemySlot GetPrecedingNorfairRioSlot(RoomEnemySlot follower)
    {
        if (follower.SlotIndex == 0)
            throw new InvalidDataException("A Norfair Rio follower cannot occupy enemy slot zero.");
        return _slots[follower.SlotIndex - 1];
    }

    private void ValidateNorfairRioParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingNorfairRioSlot(follower);
        if (parent.EnemyDefinitionPointer != NorfairRioDefinition ||
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
}
