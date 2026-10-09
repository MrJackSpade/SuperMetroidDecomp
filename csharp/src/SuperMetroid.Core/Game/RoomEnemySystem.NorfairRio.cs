using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Exact bank-$A2 function words dispatched by Norfair Rio main AI.</summary>
public enum NorfairRioEnemyFunction : ushort
{
    /// <summary>Native <c>Function_Geruta_Flames</c> at <c>$A2:C281</c>; follows the preceding parent using its signed animation-authored Y offset, hides for parent freeze/idle, and deletes the follower when the parent dies.</summary>
    FollowParent = 0xc281,
    /// <summary>Native <c>Function_Geruta_Idle</c> at <c>$A2:C2E7</c>; admits an attack when RNG mask <c>$0101</c> is nonzero and Samus is strictly within 192 horizontal room pixels, sampling a launch velocity from that RNG word.</summary>
    WaitForAttackOpportunity = 0xc2e7,
    /// <summary>Native <c>Function_Geruta_StartSwoop</c> at <c>$A2:C33F</c>; waits for the animation handshake, then installs descending artwork, enters the dive, and requests library-two sound <c>$65</c>.</summary>
    WaitForTakeoffAnimation = 0xc33f,
    /// <summary>Native <c>Function_Geruta_Swoop_Descending</c> at <c>$A2:C361</c>; moves down and horizontally with wall reversal, transitioning to return on vertical collision or when vertical velocity becomes negative.</summary>
    Dive = 0xc361,
    /// <summary>Native <c>Function_Geruta_Swoop_Ascending</c> at <c>$A2:C3B1</c>; accelerates upward while moving horizontally and responding to animation signals, ending on vertical collision rather than a saved perch coordinate.</summary>
    ReturnToPerch = 0xc3b1,
    /// <summary>Native <c>Function_Geruta_FinishSwoop</c> at <c>$A2:C406</c>; one-update state that restores only the idle dispatch address, leaving the existing looping animation list and follower visibility intact.</summary>
    FinishLanding = 0xc406,
}

/// <summary>
/// Typed projection of Norfair Rio's common A/B/F variables and three family-extra words.
/// The extra words live in bank-$7E family RAM rather than the ordinary enemy record, so
/// they remain explicit host fields while velocities/function stay backed by the slot.
/// </summary>
public sealed class NorfairRioEnemyState
{
    /// <summary>Backing enemy slot that owns native common-variable fields such as velocity and function.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates the host-side Norfair Rio state projection for an initialized enemy slot.</summary>
    /// <param name="slot">Enemy record whose common variables back this state's native fields.</param>
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

    /// <summary>Current indirect bank-$A2 AI address, native <c>Geruta.function</c> in the owning slot's variable F at <c>$0FB2,x</c>; shared by parent and flame-follower dispatch.</summary>
    public NorfairRioEnemyFunction Function
    {
        get => (NorfairRioEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>Whether population parameter 1's sign bit selects the flame follower instead of the parent; a follower must immediately follow a parent of this same enemy definition.</summary>
    public bool IsFollower => (_slot.Parameter1 & 0x8000) != 0;
}

/// <summary>Literal translation of Norfair Rio enemy AI $A2:C1C9-$C41F.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Enemy-definition pointer identifying the Norfair Rio parent and flame-follower pair.</summary>
    internal const ushort NorfairRioDefinition = 0xd2ff;

    /// <summary>Strict horizontal distance within which the parent may begin an attack.</summary>
    private const ushort NorfairRioHorizontalTriggerDistance = 0x00c0;
    /// <summary>Per-update amount subtracted from the signed 8.8 vertical velocity during a dive or return.</summary>
    private const ushort NorfairRioGravity = 32;
    /// <summary>Library-two sound identifier requested when the descending dive begins.</summary>
    private const ushort NorfairRioDiveSound = 0x0065;

    /// <summary>Initialized host state for each active Norfair Rio enemy slot.</summary>
    private readonly NorfairRioEnemyState?[] _norfairRioStates =
        new NorfairRioEnemyState?[MaximumEnemyCount];

    /// <summary>Most recent library-two dive sound request produced during this frame.</summary>
    public ushort? LastNorfairRioSoundEffect { get; private set; }

    /// <summary>Clears per-slot Rio state and the frame-local dive sound request when room state is reset.</summary>
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
                            NorfairRioInstructionProgramDefinitions.Idle);
                    }
                    return;
                }

                // (random >> 1) & 2 selects one of two adjacent signed 8.8 Y speeds; that
                // expression is already the byte offset into the word table.
                state.YVelocity = RioLaunchDefinitions.NorfairYVelocity(random);
                state.XVelocity = RioLaunchDefinitions.NorfairXVelocity;
                if (unchecked((short)(samus!.XPosition - slot.XPosition)) < 0)
                    state.XVelocity = unchecked((ushort)-(short)state.XVelocity);
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
        if (slot.EnemyDefinitionPointer != NorfairRioDefinition)
            return false;

        NorfairRioEnemyState state = RequireNorfairRioState(slot);
        switch (opcode)
        {
            case NorfairRioInstructionCodes.Instruction_Geruta_SetFinishedSwoopStartAnimationFlag:
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

    /// <summary>Updates the flame follower from its immediately preceding parent, including visibility and signed offset.</summary>
    /// <param name="follower">Enemy slot occupied by the flame half.</param>
    /// <param name="followerState">Host state associated with that follower slot.</param>
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
        follower.XPosition = parent.XPosition;
        follower.YPosition = unchecked((ushort)(
            parent.YPosition + (short)parentState.FollowerYOffset));
    }

    /// <summary>Starts the upward return phase after the dive reaches its turning condition.</summary>
    /// <param name="slot">Parent enemy slot whose motion and instruction list are changing.</param>
    /// <param name="state">Parent state that stores the new velocity and dispatch function.</param>
    private static void BeginNorfairRioReturn(
        RoomEnemySlot slot,
        NorfairRioEnemyState state)
    {
        state.YVelocity = 0xffff;
        InstallNorfairRioInstructionList(
            slot,
            state,
            NorfairRioInstructionProgramDefinitions.StartAscending);
        state.Function = NorfairRioEnemyFunction.ReturnToPerch;
    }

    /// <summary>Moves the parent by its signed horizontal velocity and reverses that velocity after a blocking collision.</summary>
    /// <param name="level">Room collision data used for horizontal movement.</param>
    /// <param name="slot">Parent enemy slot to move.</param>
    /// <param name="state">State containing the velocity to apply and possibly reverse.</param>
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

    /// <summary>Installs a changed Rio instruction list and restarts its instruction and enemy timers.</summary>
    /// <param name="slot">Enemy slot whose instruction stream is updated.</param>
    /// <param name="state">State tracking the list currently installed for that slot.</param>
    /// <param name="instructionList">Bank-$A2 list address to install.</param>
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

    /// <summary>Gets the enemy slot immediately before a follower, which owns the parent half.</summary>
    /// <param name="follower">Follower slot whose parent is being located.</param>
    /// <returns>The preceding enemy slot.</returns>
    /// <exception cref="InvalidDataException">The follower occupies slot zero and has no preceding slot.</exception>
    private RoomEnemySlot GetPrecedingNorfairRioSlot(RoomEnemySlot follower)
    {
        if (follower.SlotIndex == 0)
            throw new InvalidDataException("A Norfair Rio follower cannot occupy enemy slot zero.");
        return _slots[follower.SlotIndex - 1];
    }

    /// <summary>Verifies that a follower is immediately preceded by a non-follower Rio enemy of the same definition.</summary>
    /// <param name="follower">Follower slot whose parent relationship must be checked.</param>
    /// <exception cref="InvalidDataException">The preceding slot is not a valid Rio parent.</exception>
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

    /// <summary>Ensures room collision data is available before advancing Rio movement.</summary>
    /// <param name="level">Optional level data supplied to the current enemy update.</param>
    /// <exception cref="InvalidOperationException">No room level data was supplied.</exception>
    private static void RequireNorfairRioLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Norfair Rio movement requires room level data.");
    }

    /// <summary>Gets the initialized Rio state associated with an enemy slot.</summary>
    /// <param name="slot">Enemy slot whose state is required.</param>
    /// <returns>The slot's initialized Norfair Rio state.</returns>
    /// <exception cref="InvalidOperationException">The slot has not been initialized as a Norfair Rio enemy.</exception>
    private NorfairRioEnemyState RequireNorfairRioState(RoomEnemySlot slot) =>
        _norfairRioStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Norfair Rio state.");
}
