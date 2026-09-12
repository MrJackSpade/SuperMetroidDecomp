using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Exact bank-$A2 function words dispatched by Lower Norfair Rio main AI.</summary>
public enum LowerNorfairRioEnemyFunction : ushort
{
    FollowParent = 0xc72e,
    WaitForAttackOpportunity = 0xc771,
    WaitForTakeoffAnimation = 0xc7bb,
    Dive = 0xc7d6,
    ReturnToPerch = 0xc82d,
    WaitForLandingAnimation = 0xc888,
}

/// <summary>
/// Typed projection of Lower Norfair Rio's C/D/F variables and three family-extra words.
/// This variant shares the physical parent/follower layout with $D2FF but deliberately uses
/// different common velocity words and interprets extra word $02 as a visibility switch.
/// </summary>
public sealed class LowerNorfairRioEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal LowerNorfairRioEnemyState(RoomEnemySlot slot) => _slot = slot;

    public ushort InstalledInstructionList { get; internal set; }

    /// <summary>Extra word $01, set by private animation instruction $C6D2.</summary>
    public bool AnimationSignal { get; internal set; }

    /// <summary>
    /// Extra word $02, cleared/set by $C6DD/$C6E8. The following physical slot reads this
    /// word through the cartridge's spawn-data alias and shows its lower sprite only at one.
    /// </summary>
    public bool FollowerVisible { get; internal set; }

    /// <summary>Signed 8.8 vertical velocity stored in common variable C.</summary>
    public ushort YVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Signed 8.8 horizontal velocity stored in common variable D.</summary>
    public ushort XVelocity
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    public LowerNorfairRioEnemyFunction Function
    {
        get => (LowerNorfairRioEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    public bool IsFollower => (_slot.Parameter1 & 0x8000) != 0;
}

/// <summary>Literal translation of Lower Norfair Rio AI $A2:C6D2-$C8B5.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort LowerNorfairRioDefinition = 0xd33f;

    private const ushort LowerNorfairRioIdleInstructionList = 0xc61a;
    private const ushort LowerNorfairRioTakeoffInstructionList = 0xc630;
    private const ushort LowerNorfairRioDiveInstructionList = 0xc65a;
    private const ushort LowerNorfairRioReturnInstructionList = 0xc662;
    private const ushort LowerNorfairRioLateReturnInstructionList = 0xc674;
    private const ushort LowerNorfairRioLandingInstructionList = 0xc686;
    private const ushort LowerNorfairRioFollowerInstructionList = 0xc6b0;
    private const ushort LowerNorfairRioAnimationSignalInstruction = 0xc6d2;
    private const ushort LowerNorfairRioFollowerHideInstruction = 0xc6dd;
    private const ushort LowerNorfairRioFollowerShowInstruction = 0xc6e8;
    private const ushort LowerNorfairRioHorizontalTriggerDistance = 0x0070;
    private const ushort LowerNorfairRioGravity = 32;
    private const ushort LowerNorfairRioReturnSound = 0x0064;

    private readonly LowerNorfairRioEnemyState?[] _lowerNorfairRioStates =
        new LowerNorfairRioEnemyState?[MaximumEnemyCount];

    public IReadOnlyList<LowerNorfairRioEnemyState?> LowerNorfairRioStates =>
        _lowerNorfairRioStates;

    /// <summary>Most recent library-two return sound request produced during this frame.</summary>
    public ushort? LastLowerNorfairRioSoundEffect { get; private set; }

    private void ResetLowerNorfairRioRoomState()
    {
        Array.Clear(_lowerNorfairRioStates);
        LastLowerNorfairRioSoundEffect = null;
    }

    /// <summary>Ports <c>LowerNorfairRio_Init</c> at $A2:C6F3.</summary>
    private void InitializeLowerNorfairRio(RoomEnemySlot slot)
    {
        var state = new LowerNorfairRioEnemyState(slot)
        {
            AnimationSignal = false,
            FollowerVisible = false,
        };
        _lowerNorfairRioStates[slot.SlotIndex] = state;

        if (state.IsFollower)
        {
            ValidateLowerNorfairRioParent(slot);
            state.InstalledInstructionList = LowerNorfairRioFollowerInstructionList;
            slot.CurrentInstruction = LowerNorfairRioFollowerInstructionList;
            state.Function = LowerNorfairRioEnemyFunction.FollowParent;
            return;
        }

        state.InstalledInstructionList = LowerNorfairRioIdleInstructionList;
        slot.CurrentInstruction = LowerNorfairRioIdleInstructionList;
        state.Function = LowerNorfairRioEnemyFunction.WaitForAttackOpportunity;
    }

    /// <summary>
    /// Ports <c>LowerNorfairRio_Main</c> and functions one through six at $A2:C724-$C8A2.
    /// The shared RNG advances before every parent and follower dispatch, exactly as retail.
    /// </summary>
    private void RunLowerNorfairRioMain(
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        ushort random = _nextRandom!();

        switch (state.Function)
        {
            case LowerNorfairRioEnemyFunction.FollowParent:
                FollowLowerNorfairRioParent(slot);
                return;

            case LowerNorfairRioEnemyFunction.WaitForAttackOpportunity:
                bool canAttack = samus is not null && (random & 0x0101) != 0 &&
                    IsWithinStrictModularDistance(
                        samus.XPosition,
                        slot.XPosition,
                        LowerNorfairRioHorizontalTriggerDistance);
                if (!canAttack)
                {
                    // Unlike upper Norfair Rio, this variant clears the handshake and calls
                    // its list installer on every rejected frame. The helper itself prevents
                    // an unchanged $C61A list from restarting.
                    state.AnimationSignal = false;
                    InstallLowerNorfairRioInstructionList(
                        slot,
                        state,
                        LowerNorfairRioIdleInstructionList);
                    return;
                }

                state.YVelocity = RioLaunchDefinitions.LowerNorfairYVelocity;
                state.XVelocity = RioLaunchDefinitions.LowerNorfairXVelocity;
                if (unchecked((short)(samus!.XPosition - slot.XPosition)) < 0)
                    state.XVelocity = unchecked((ushort)-(short)state.XVelocity);
                InstallLowerNorfairRioInstructionList(
                    slot,
                    state,
                    LowerNorfairRioTakeoffInstructionList);
                state.Function = LowerNorfairRioEnemyFunction.WaitForTakeoffAnimation;
                return;

            case LowerNorfairRioEnemyFunction.WaitForTakeoffAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                InstallLowerNorfairRioInstructionList(
                    slot,
                    state,
                    LowerNorfairRioDiveInstructionList);
                state.Function = LowerNorfairRioEnemyFunction.Dive;
                return;

            case LowerNorfairRioEnemyFunction.Dive:
                RequireLowerNorfairRioLevel(level);
                MoveLowerNorfairRioHorizontally(level!, slot, state);
                if (MoveEnemyVertically(
                        level!,
                        slot,
                        ToEightBitVelocityDisplacement(state.YVelocity)))
                {
                    BeginLowerNorfairRioReturn(slot, state);
                    return;
                }

                state.YVelocity = unchecked((ushort)(state.YVelocity - LowerNorfairRioGravity));
                if (unchecked((short)state.YVelocity) < 0)
                    BeginLowerNorfairRioReturn(slot, state);
                return;

            case LowerNorfairRioEnemyFunction.ReturnToPerch:
                RequireLowerNorfairRioLevel(level);
                MoveLowerNorfairRioHorizontally(level!, slot, state);
                if (MoveEnemyVertically(
                        level!,
                        slot,
                        ToEightBitVelocityDisplacement(state.YVelocity)))
                {
                    InstallLowerNorfairRioInstructionList(
                        slot,
                        state,
                        LowerNorfairRioLandingInstructionList);
                    state.Function = LowerNorfairRioEnemyFunction.WaitForLandingAnimation;
                    return;
                }

                state.YVelocity = unchecked((ushort)(state.YVelocity - LowerNorfairRioGravity));
                if (state.AnimationSignal)
                {
                    state.AnimationSignal = false;
                    InstallLowerNorfairRioInstructionList(
                        slot,
                        state,
                        LowerNorfairRioLateReturnInstructionList);
                }
                return;

            case LowerNorfairRioEnemyFunction.WaitForLandingAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                InstallLowerNorfairRioInstructionList(
                    slot,
                    state,
                    LowerNorfairRioLandingInstructionList);
                state.Function = LowerNorfairRioEnemyFunction.WaitForAttackOpportunity;
                return;

            default:
                throw new InvalidDataException(
                    $"Lower Norfair Rio function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports all three private no-operand instructions at $A2:C6D2-$C6F2.</summary>
    private bool TryProcessLowerNorfairRioInstruction(
        RoomEnemySlot slot,
        ushort opcode,
        ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != LowerNorfairRioDefinition)
            return false;

        LowerNorfairRioEnemyState state = RequireLowerNorfairRioState(slot);
        switch (opcode)
        {
            case LowerNorfairRioAnimationSignalInstruction:
                state.AnimationSignal = true;
                break;
            case LowerNorfairRioFollowerHideInstruction:
                state.FollowerVisible = false;
                break;
            case LowerNorfairRioFollowerShowInstruction:
                state.FollowerVisible = true;
                break;
            default:
                return false;
        }
        cursor = unchecked((ushort)(cursor + 2));
        return true;
    }

    private void FollowLowerNorfairRioParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingLowerNorfairRioSlot(follower);
        if (parent.Health == 0)
        {
            follower.Properties = follower.Properties.With(EnemyProperties.Deleted);
            return;
        }
        ValidateLowerNorfairRioParent(follower);

        LowerNorfairRioEnemyState parentState = RequireLowerNorfairRioState(parent);
        follower.FrozenTimer = parent.FrozenTimer;
        if (parent.FrozenTimer != 0 || !parentState.FollowerVisible)
        {
            follower.Properties = follower.Properties.With(EnemyProperties.Invisible);
            return;
        }

        // gEnemySpawnData(child)[31].field_4 again aliases the preceding parent's family
        // extra word $02. Here it is Boolean rather than a signed offset. The drawing-queue
        // X/Y aliases supply the live parent origin and this variant adds a literal 12 px.
        follower.Properties = follower.Properties.Without(EnemyProperties.Invisible);
        follower.XPosition = parent.XPosition;
        follower.YPosition = unchecked((ushort)(parent.YPosition + 12));
    }

    private void BeginLowerNorfairRioReturn(
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state)
    {
        state.YVelocity = 0xffff;
        InstallLowerNorfairRioInstructionList(
            slot,
            state,
            LowerNorfairRioReturnInstructionList);
        state.Function = LowerNorfairRioEnemyFunction.ReturnToPerch;
        LastLowerNorfairRioSoundEffect = LowerNorfairRioReturnSound;
    }

    private static void MoveLowerNorfairRioHorizontally(
        RoomLevelData level,
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state)
    {
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                ToEightBitVelocityDisplacement(state.XVelocity)))
        {
            state.XVelocity = unchecked((ushort)-(short)state.XVelocity);
        }
    }

    private static void InstallLowerNorfairRioInstructionList(
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state,
        ushort instructionList)
    {
        if (state.InstalledInstructionList == instructionList)
            return;
        state.InstalledInstructionList = instructionList;
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private RoomEnemySlot GetPrecedingLowerNorfairRioSlot(RoomEnemySlot follower)
    {
        if (follower.SlotIndex == 0)
        {
            throw new InvalidDataException(
                "A Lower Norfair Rio follower cannot occupy enemy slot zero.");
        }
        return _slots[follower.SlotIndex - 1];
    }

    private void ValidateLowerNorfairRioParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingLowerNorfairRioSlot(follower);
        if (parent.EnemyDefinitionPointer != LowerNorfairRioDefinition ||
            (parent.Parameter1 & 0x8000) != 0)
        {
            throw new InvalidDataException(
                $"Lower Norfair Rio follower in slot {follower.SlotIndex} must immediately " +
                "follow a parent record of the same definition.");
        }
    }

    private static void RequireLowerNorfairRioLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Lower Norfair Rio movement requires room level data.");
    }

    private LowerNorfairRioEnemyState RequireLowerNorfairRioState(RoomEnemySlot slot) =>
        _lowerNorfairRioStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Lower Norfair Rio state.");
}
