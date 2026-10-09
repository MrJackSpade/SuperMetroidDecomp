using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Exact bank-$A2 function words dispatched by Lower Norfair Rio main AI.</summary>
public enum LowerNorfairRioEnemyFunction : ushort
{
    /// <summary>Native <c>Function_Holtz_Flames</c> at <c>$A2:C72E</c>; follows the immediately preceding parent twelve pixels below, mirrors its freeze state and flame visibility, and deletes the follower when the parent dies.</summary>
    FollowParent = 0xc72e,
    /// <summary>Native <c>Function_Holtz_Idle</c> at <c>$A2:C771</c>; admits an attack when the RNG mask <c>$0101</c> is nonzero and Samus is strictly within 112 horizontal room pixels.</summary>
    WaitForAttackOpportunity = 0xc771,
    /// <summary>Native <c>Function_Holtz_PrepareToSwoop</c> at <c>$A2:C7BB</c>; waits for the animation instruction's completion signal before clearing it and starting downward motion.</summary>
    WaitForTakeoffAnimation = 0xc7bb,
    /// <summary>Native <c>Function_Holtz_Swoop_Descending</c> at <c>$A2:C7D6</c>; moves horizontally and downward, reverses X velocity on walls, and transitions to return on vertical collision or a negative vertical velocity.</summary>
    Dive = 0xc7d6,
    /// <summary>Native <c>Function_Holtz_Swoop_Ascending</c> at <c>$A2:C82D</c>; accelerates upward while retaining horizontal movement, switches ascent artwork on the animation signal, and enters landing wait on vertical collision rather than targeting a saved Y.</summary>
    ReturnToPerch = 0xc82d,
    /// <summary>Native <c>Function_Holtz_SwoopCooldown</c> at <c>$A2:C888</c>; consumes the landing animation's signal, installs cooldown artwork, and resumes attack-opportunity waiting.</summary>
    WaitForLandingAnimation = 0xc888,
}

/// <summary>
/// Typed projection of Lower Norfair Rio's C/D/F variables and three family-extra words.
/// This variant shares the physical parent/follower layout with $D2FF but deliberately uses
/// different common velocity words and interprets extra word $02 as a visibility switch.
/// </summary>
public sealed class LowerNorfairRioEnemyState
{
    /// <summary>Physical enemy slot that owns this Rio actor's shared C/D/F variables.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates the typed state projection over an existing enemy slot.</summary>
    /// <param name="slot">Slot whose native variables back the Rio-specific state.</param>
    internal LowerNorfairRioEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Last bank-$A2 instruction-list identity installed for this actor, native <c>Holtz.instList</c> at <c>$7E:7800,x</c>; distinct from the advancing cursor and prevents timer resets when the same list is requested again.</summary>
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

    /// <summary>Current indirect bank-$A2 AI address backed by the owning slot's variable F at native <c>$0FB2,x</c>; parent and flame follower dispatch through this same word.</summary>
    public LowerNorfairRioEnemyFunction Function
    {
        get => (LowerNorfairRioEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>Whether population parameter 1's sign bit selects the lower flame follower instead of the parent; a follower must immediately follow a parent of the same enemy definition.</summary>
    public bool IsFollower => (_slot.Parameter1 & 0x8000) != 0;
}

/// <summary>Literal translation of Lower Norfair Rio AI $A2:C6D2-$C8B5.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Native enemy-definition pointer used to distinguish Lower Norfair Rio slots.</summary>
    internal const ushort LowerNorfairRioDefinition = 0xd33f;

    /// <summary>Strict horizontal distance below which the idle Rio may begin an attack.</summary>
    private const ushort LowerNorfairRioHorizontalTriggerDistance = 0x0070;
    /// <summary>Per-frame decrement applied to the actor's signed 8.8 vertical velocity.</summary>
    private const ushort LowerNorfairRioGravity = 32;
    /// <summary>Library-two sound request emitted when the dive changes into its return ascent.</summary>
    private const ushort LowerNorfairRioReturnSound = 0x0064;

    /// <summary>Per-slot Rio state projections, cleared when room enemy state is reset.</summary>
    private readonly LowerNorfairRioEnemyState?[] _lowerNorfairRioStates =
        new LowerNorfairRioEnemyState?[MaximumEnemyCount];

    /// <summary>Most recent library-two return sound request produced during this frame.</summary>
    public ushort? LastLowerNorfairRioSoundEffect { get; private set; }

    /// <summary>Discards Rio-specific slot projections and the prior frame's sound request.</summary>
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
            state.InstalledInstructionList = LowerNorfairRioInstructionProgramDefinitions.Flames;
            slot.CurrentInstruction = LowerNorfairRioInstructionProgramDefinitions.Flames;
            state.Function = LowerNorfairRioEnemyFunction.FollowParent;
            return;
        }

        state.InstalledInstructionList = LowerNorfairRioInstructionProgramDefinitions.Idle;
        slot.CurrentInstruction = LowerNorfairRioInstructionProgramDefinitions.Idle;
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
                        LowerNorfairRioInstructionProgramDefinitions.Idle);
                    return;
                }

                state.YVelocity = RioLaunchDefinitions.LowerNorfairYVelocity;
                state.XVelocity = RioLaunchDefinitions.LowerNorfairXVelocity;
                if (unchecked((short)(samus!.XPosition - slot.XPosition)) < 0)
                    state.XVelocity = unchecked((ushort)-(short)state.XVelocity);
                InstallLowerNorfairRioInstructionList(
                    slot,
                    state,
                    LowerNorfairRioInstructionProgramDefinitions.PrepareToSwoop);
                state.Function = LowerNorfairRioEnemyFunction.WaitForTakeoffAnimation;
                return;

            case LowerNorfairRioEnemyFunction.WaitForTakeoffAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                InstallLowerNorfairRioInstructionList(
                    slot,
                    state,
                    LowerNorfairRioInstructionProgramDefinitions.Descending);
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
                        LowerNorfairRioInstructionProgramDefinitions.Cooldown);
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
                        LowerNorfairRioInstructionProgramDefinitions.AscendingPart2);
                }
                return;

            case LowerNorfairRioEnemyFunction.WaitForLandingAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                InstallLowerNorfairRioInstructionList(
                    slot,
                    state,
                    LowerNorfairRioInstructionProgramDefinitions.Cooldown);
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
            case LowerNorfairRioInstructionCodes.SetAnimationFinishedFlag:
                state.AnimationSignal = true;
                break;
            case LowerNorfairRioInstructionCodes.HideFlames:
                state.FollowerVisible = false;
                break;
            case LowerNorfairRioInstructionCodes.ShowFlames:
                state.FollowerVisible = true;
                break;
            default:
                return false;
        }
        cursor = unchecked((ushort)(cursor + 2));
        return true;
    }

    /// <summary>Updates the flame follower from its preceding parent or deletes/hides it when the parent is unavailable.</summary>
    /// <param name="follower">Follower slot whose position, visibility, and freeze timer mirror its parent.</param>
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

    /// <summary>Starts the return ascent, seeds upward velocity, and publishes its sound request.</summary>
    /// <param name="slot">Parent enemy slot entering the return phase.</param>
    /// <param name="state">Rio state whose velocity and function are updated.</param>
    private void BeginLowerNorfairRioReturn(
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state)
    {
        state.YVelocity = 0xffff;
        InstallLowerNorfairRioInstructionList(
            slot,
            state,
            LowerNorfairRioInstructionProgramDefinitions.AscendingPart1);
        state.Function = LowerNorfairRioEnemyFunction.ReturnToPerch;
        LastLowerNorfairRioSoundEffect = LowerNorfairRioReturnSound;
    }

    /// <summary>Moves the Rio horizontally and reverses its signed velocity after a blocking collision.</summary>
    /// <param name="level">Room geometry used by the horizontal collision sweep.</param>
    /// <param name="slot">Enemy slot whose position is moved.</param>
    /// <param name="state">Rio state supplying the signed 8.8 horizontal velocity.</param>
    private void MoveLowerNorfairRioHorizontally(
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

    /// <summary>Installs a new animation list and resets its timers only when the list identity changes.</summary>
    /// <param name="slot">Enemy slot receiving the instruction pointer and fresh timers.</param>
    /// <param name="state">State tracking the list most recently installed for this actor.</param>
    /// <param name="instructionList">Bank-$A2 instruction-list address to install.</param>
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

    /// <summary>Returns the physical slot immediately before a follower in the enemy-slot array.</summary>
    /// <param name="follower">Follower whose preceding slot contains the expected parent.</param>
    /// <returns>The preceding physical enemy slot.</returns>
    /// <exception cref="InvalidDataException">The follower occupies slot zero and has no preceding slot.</exception>
    private RoomEnemySlot GetPrecedingLowerNorfairRioSlot(RoomEnemySlot follower)
    {
        if (follower.SlotIndex == 0)
        {
            throw new InvalidDataException(
                "A Lower Norfair Rio follower cannot occupy enemy slot zero.");
        }
        return _slots[follower.SlotIndex - 1];
    }

    /// <summary>Ensures a follower's preceding slot is a parent using the same Rio definition.</summary>
    /// <param name="follower">Follower whose parent relationship must be valid.</param>
    /// <exception cref="InvalidDataException">The preceding slot is not a non-follower Rio parent.</exception>
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

    /// <summary>Rejects movement dispatch when room collision geometry is unavailable.</summary>
    /// <param name="level">Room geometry required by the Rio's movement handlers.</param>
    /// <exception cref="InvalidOperationException">No room level data was supplied.</exception>
    private static void RequireLowerNorfairRioLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Lower Norfair Rio movement requires room level data.");
    }

    /// <summary>Gets the initialized Rio state associated with an enemy slot.</summary>
    /// <param name="slot">Enemy slot whose Rio state is requested.</param>
    /// <returns>The state projection created during Rio initialization.</returns>
    /// <exception cref="InvalidOperationException">The slot has no initialized Rio state.</exception>
    private LowerNorfairRioEnemyState RequireLowerNorfairRioState(RoomEnemySlot slot) =>
        _lowerNorfairRioStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Lower Norfair Rio state.");
}
