using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A2 behavior-function words stored in Oum's common variable A. Keeping the
/// cartridge addresses in the enum makes debugger state directly comparable with $7E:0F92.
/// </summary>
public enum MaridiaLargeSnailEnemyFunction : ushort
{
    Idle = 0xcde6,
    Rolling = 0xce2b,
    Attacking = 0xcf40,
}

/// <summary>
/// Literal bank-$A2 bounce-function words stored in Oum's common variable F. The enemy uses
/// the shared quadratic-speed table for a diminishing three-impact settling animation.
/// </summary>
public enum MaridiaLargeSnailBounceFunction : ushort
{
    Falling = 0xcf66,
    Rising = 0xcfa9,
}

/// <summary>
/// Typed debugger projection of Oum's six common enemy words and ten family-extra words.
/// Common variables remain backed by the physical enemy slot; family-extra values are named
/// host fields because this family has no cross-slot WRAM aliases.
/// </summary>
public sealed class MaridiaLargeSnailEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal MaridiaLargeSnailEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Extra word $00: requested index into the eight-entry list table at $CB77.</summary>
    public ushort RequestedInstructionListIndex { get; internal set; }

    /// <summary>Extra word $01: last list-table index accepted by <c>SetOumInstList</c>.</summary>
    public ushort InstalledInstructionListIndex { get; internal set; }

    /// <summary>Extra word $02: set by instruction $CCB3 when the attack animation ends.</summary>
    public bool AttackAnimationFinished { get; internal set; }

    /// <summary>
    /// Extra word $03: set only during the rolling frames where Oum may turn into its next
    /// attack. The native name is "attack allowing rotation flag"; it is not a hitbox flag.
    /// </summary>
    public bool AttackAllowsRotation { get; internal set; }

    /// <summary>Extra word $04: Samus is within the strict 24-by-32 proximity window.</summary>
    public bool SamusInPushingRange { get; internal set; }

    /// <summary>Extra word $05: one when Samus is to the left, zero when to the right.</summary>
    public bool SamusIsLeft { get; internal set; }

    /// <summary>Extra word $06: whole-pixel X position captured before this frame's AI.</summary>
    public ushort PreviousXPosition { get; internal set; }

    /// <summary>Extra word $08: whole-pixel Y position captured before this frame's AI.</summary>
    public ushort PreviousYPosition { get; internal set; }

    /// <summary>
    /// Extra word $0A is cleared by initialization and otherwise unused by the retail code.
    /// It remains visible so a debugger can prove that the untranslated-looking write exists.
    /// </summary>
    public ushort UnusedExtra0A { get; internal set; }

    /// <summary>
    /// Extra word $0B: movement suppression produced after AI and consumed on the next frame.
    /// This one-frame pipeline is intentional cartridge ordering, not input latency invented
    /// by the host translation.
    /// </summary>
    public bool StopMovementNextFrame { get; internal set; }

    /// <summary>Common variable A: outer behavior function.</summary>
    public MaridiaLargeSnailEnemyFunction Function
    {
        get => (MaridiaLargeSnailEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Common variable B: $0180-stepped index used by the bounce arc.</summary>
    public ushort YSpeedTableIndex
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Common variable C: signed-underflow attack cooldown, initialized to 128.</summary>
    public ushort AttackCooldown
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Common variable D: zero moves right; one moves left.</summary>
    public bool MovingLeft
    {
        get => _slot.VariableD != 0;
        internal set => _slot.VariableD = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>Common variable E: remaining landing impacts before Oum is settled.</summary>
    public ushort RemainingBounces
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Common variable F: falling/rising half of the quadratic bounce.</summary>
    public MaridiaLargeSnailBounceFunction BounceFunction
    {
        get => (MaridiaLargeSnailBounceFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}

/// <summary>
/// Literal translation of Maridia large snail / Oum enemy $D37F at $A2:CA4B-$D3BF.
/// Movement, Samus carrying, private animation opcodes, extended hitboxes, and combat tails
/// all remain driven by retail ROM tables rather than host-authored frame data.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort MaridiaLargeSnailDefinition = 0xd37f;

    private const ushort MaridiaLargeSnailInitialInstructionList = 0xca4b;
    private const int MaridiaLargeSnailInstructionListTable = 0xa2cb77;
    private const ushort MaridiaLargeSnailSplashInstruction = 0xcb6b;
    private const ushort MaridiaLargeSnailAttackFinishedInstruction = 0xccb3;
    private const ushort MaridiaLargeSnailAllowRotationInstruction = 0xccbe;
    private const ushort MaridiaLargeSnailDisallowRotationInstruction = 0xccc9;
    private const ushort MaridiaLargeSnailPushingXDistance = 0x0018;
    private const ushort MaridiaLargeSnailPushingYDistance = 0x0020;
    private const ushort MaridiaLargeSnailAttackXDistance = 0x0020;
    private const ushort MaridiaLargeSnailInitialAttackCooldown = 0x0080;
    private const ushort MaridiaLargeSnailInitialBounceCount = 3;
    private const ushort MaridiaLargeSnailBounceAcceleration = 0x0180;
    private const ushort MaridiaLargeSnailMaximumFallIndex = 0x4000;
    private const ushort MaridiaLargeSnailImpactIndexLoss = 0x1000;
    private const ushort MaridiaLargeSnailSplashSound = 0x000e;
    private const ushort MaridiaLargeSnailShotSound = 0x0057;
    private const ushort MaridiaLargeSnailDamagingTouchAi =
        EnemyAiCodePointers.BankA2.MaridiaLargeSnailDamagingTouch;
    private const ushort MaridiaLargeSnailNonDamagingTouchAi =
        EnemyAiCodePointers.BankA2.MaridiaLargeSnailNonDamagingTouch;
    private const ushort MaridiaLargeSnailShotAi =
        EnemyAiCodePointers.BankA2.MaridiaLargeSnailShot;
    private const ushort MaridiaLargeSnailNoOpHitboxAi = EnemyAiCodePointers.BankA0.NoOp;

    private readonly MaridiaLargeSnailEnemyState?[] _maridiaLargeSnailStates =
        new MaridiaLargeSnailEnemyState?[MaximumEnemyCount];

    /// <summary>Typed Oum state for all 32 physical enemy slots.</summary>
    public IReadOnlyList<MaridiaLargeSnailEnemyState?> MaridiaLargeSnailStates =>
        _maridiaLargeSnailStates;

    /// <summary>
    /// Most recent library-two sound requested by Oum this frame. The outer audio mixer is
    /// still a frontend seam, so the enemy publishes the exact native sound identifier.
    /// </summary>
    public ushort? LastMaridiaLargeSnailSoundEffect { get; private set; }

    private void ResetMaridiaLargeSnailRoomState()
    {
        Array.Clear(_maridiaLargeSnailStates);
        LastMaridiaLargeSnailSoundEffect = null;
    }

    /// <summary>Ports <c>InitAI_Oum</c> at $A2:CCD4.</summary>
    private void InitializeMaridiaLargeSnail(RoomEnemySlot slot)
    {
        var state = new MaridiaLargeSnailEnemyState(slot)
        {
            RequestedInstructionListIndex = 0,
            InstalledInstructionListIndex = 0,
            AttackAnimationFinished = false,
            AttackAllowsRotation = false,
            SamusInPushingRange = false,
            SamusIsLeft = false,
            PreviousXPosition = 0,
            PreviousYPosition = 0,
            UnusedExtra0A = 0,
            StopMovementNextFrame = false,
            Function = MaridiaLargeSnailEnemyFunction.Idle,
            YSpeedTableIndex = 0,
            AttackCooldown = MaridiaLargeSnailInitialAttackCooldown,
            MovingLeft = false,
            RemainingBounces = MaridiaLargeSnailInitialBounceCount,
            BounceFunction = MaridiaLargeSnailBounceFunction.Falling,
        };
        _maridiaLargeSnailStates[slot.SlotIndex] = state;

        // The initial left-facing list intentionally disagrees with installed selector zero,
        // whose table entry is right-facing idle. Because SetOumInstList suppresses equal
        // selectors, this oddity is observable until Samus first selects a different index.
        slot.CurrentInstruction = MaridiaLargeSnailInitialInstructionList;
    }

    /// <summary>
    /// Ports <c>MainAI_Oum</c> and functions $CD23-$CFFF. Ordering is exact: proximity and
    /// previous positions, behavior, Samus carrying, then the input-derived stop flag.
    /// </summary>
    private void RunMaridiaLargeSnailMain(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        ushort controllerInput)
    {
        if (samus is null)
            throw new InvalidOperationException("Maridia Large Snail AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Maridia Large Snail AI requires room level data.");

        UpdateMaridiaLargeSnailSamusProximity(slot, state, samus);
        state.PreviousXPosition = slot.XPosition;
        state.PreviousYPosition = slot.YPosition;

        switch (state.Function)
        {
            case MaridiaLargeSnailEnemyFunction.Idle:
                RunMaridiaLargeSnailIdle(slot, state, samus, level);
                break;
            case MaridiaLargeSnailEnemyFunction.Rolling:
                RunMaridiaLargeSnailRolling(slot, state, samus, level);
                break;
            case MaridiaLargeSnailEnemyFunction.Attacking:
                RunMaridiaLargeSnailAttack(slot, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Maridia Large Snail function $A2:{(ushort)state.Function:X4} is not translated.");
        }

        CarrySamusWithMaridiaLargeSnail(slot, state, samus);
        UpdateMaridiaLargeSnailInputStop(slot, state, controllerInput);
    }

    private void RunMaridiaLargeSnailIdle(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (state.RemainingBounces != 0)
        {
            RunMaridiaLargeSnailBounce(slot, state, level);
            return;
        }

        state.RequestedInstructionListIndex = 0;
        if (unchecked((short)(samus.XPosition - slot.XPosition)) < 0)
        {
            state.MovingLeft = true;
            state.RequestedInstructionListIndex = 1;
        }
        InstallMaridiaLargeSnailInstructionList(slot, state);

        if (!IsWithinStrictModularDistance(
                samus.XPosition,
                slot.XPosition,
                MaridiaLargeSnailPushingXDistance))
        {
            return;
        }

        state.RequestedInstructionListIndex = unchecked((ushort)(
            state.RequestedInstructionListIndex | 2));
        InstallMaridiaLargeSnailInstructionList(slot, state);
        state.Function = MaridiaLargeSnailEnemyFunction.Rolling;
    }

    private void RunMaridiaLargeSnailRolling(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (state.RemainingBounces != 0)
        {
            RunMaridiaLargeSnailBounce(slot, state, level);
        }
        else if (!MoveEnemyVertically(level, slot, 1 << 16))
        {
            // No floor one pixel below means Oum walked off a ledge. Retail restarts the
            // complete three-impact settling sequence without skipping horizontal work.
            state.YSpeedTableIndex = 0;
            state.BounceFunction = MaridiaLargeSnailBounceFunction.Falling;
            state.RemainingBounces = MaridiaLargeSnailInitialBounceCount;
        }

        // StopMovementNextFrame was produced at the end of the preceding main-AI call. The
        // current frame clears/recomputes it only after this behavior returns.
        if (state.StopMovementNextFrame)
            return;

        state.AttackCooldown = unchecked((ushort)(state.AttackCooldown - 1));
        bool cooldownExpired = unchecked((short)state.AttackCooldown) < 0;
        if (cooldownExpired)
        {
            state.AttackCooldown = 0;
            bool mayAttack = IsWithinStrictModularDistance(
                    samus.XPosition,
                    slot.XPosition,
                    MaridiaLargeSnailAttackXDistance) &&
                state.AttackAllowsRotation &&
                state.RemainingBounces == 0;
            if (mayAttack)
            {
                state.RequestedInstructionListIndex = 0;
                state.MovingLeft = false;
                if (unchecked((short)(samus.XPosition - slot.XPosition)) < 0)
                {
                    state.RequestedInstructionListIndex = 1;
                    state.MovingLeft = true;
                }

                state.AttackCooldown = MaridiaLargeSnailInitialAttackCooldown;
                state.RequestedInstructionListIndex = unchecked((ushort)(
                    (state.RequestedInstructionListIndex & 1) | 4));
                InstallMaridiaLargeSnailInstructionList(slot, state);
                state.Function = MaridiaLargeSnailEnemyFunction.Attacking;
                return;
            }
        }

        MoveMaridiaLargeSnailHorizontally(slot, state, level);
    }

    private void RunMaridiaLargeSnailAttack(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state)
    {
        if (!state.AttackAnimationFinished)
            return;

        state.AttackAnimationFinished = false;
        state.RequestedInstructionListIndex = unchecked((ushort)(
            state.RequestedInstructionListIndex - 2));
        InstallMaridiaLargeSnailInstructionList(slot, state);
        state.Function = MaridiaLargeSnailEnemyFunction.Rolling;
    }

    private void RunMaridiaLargeSnailBounce(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        RoomLevelData level)
    {
        switch (state.BounceFunction)
        {
            case MaridiaLargeSnailBounceFunction.Falling:
                state.YSpeedTableIndex = unchecked((ushort)(
                    state.YSpeedTableIndex + MaridiaLargeSnailBounceAcceleration));
                if (unchecked((short)(
                        state.YSpeedTableIndex - MaridiaLargeSnailMaximumFallIndex)) >= 0)
                {
                    state.YSpeedTableIndex = MaridiaLargeSnailMaximumFallIndex;
                }

                if (!MoveEnemyVertically(
                        level,
                        slot,
                        ReadQuadraticEnemySpeed(
                            unchecked((ushort)(state.YSpeedTableIndex >> 8)),
                            negative: false)))
                {
                    return;
                }

                state.RemainingBounces = unchecked((ushort)(state.RemainingBounces - 1));
                state.YSpeedTableIndex = unchecked((ushort)(
                    state.YSpeedTableIndex - MaridiaLargeSnailImpactIndexLoss));
                if (unchecked((short)state.YSpeedTableIndex) < 0)
                    state.RemainingBounces = 0;
                state.BounceFunction = MaridiaLargeSnailBounceFunction.Rising;
                return;

            case MaridiaLargeSnailBounceFunction.Rising:
                state.YSpeedTableIndex = unchecked((ushort)(
                    state.YSpeedTableIndex - MaridiaLargeSnailBounceAcceleration));
                if (unchecked((short)state.YSpeedTableIndex) < 0)
                {
                    state.YSpeedTableIndex = 0;
                    state.BounceFunction = MaridiaLargeSnailBounceFunction.Falling;
                    return;
                }

                // The rising half reads the negative member of the same quadratic row. The
                // native AND $7F00 is retained even though valid bounce indexes stay below
                // $4000; it documents the exact table-index formation seen in a debugger.
                ushort tableIndex = unchecked((ushort)(
                    (state.YSpeedTableIndex & 0x7f00) >> 8));
                MoveEnemyVertically(
                    level,
                    slot,
                    ReadQuadraticEnemySpeed(tableIndex, negative: true));
                return;

            default:
                throw new InvalidDataException(
                    $"Maridia Large Snail bounce function " +
                    $"$A2:{(ushort)state.BounceFunction:X4} is not translated.");
        }
    }

    private void MoveMaridiaLargeSnailHorizontally(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        RoomLevelData level)
    {
        ushort byteOffset = state.MovingLeft ? (ushort)132 : (ushort)128;
        (short whole, ushort fraction) = ReadLinearEnemySpeed(byteOffset);
        bool temporarilyReachTowardSamus = state.SamusInPushingRange &&
            state.MovingLeft == state.SamusIsLeft;
        if (temporarilyReachTowardSamus)
        {
            // Native temporarily extends the whole-pixel move sixteen pixels toward Samus,
            // performs collision, then retracts the center. Net velocity remains the table
            // speed while wall clipping naturally transfers the lost distance to Samus.
            whole = unchecked((short)(whole + (state.MovingLeft ? -16 : 16)));
        }

        int displacement = unchecked((whole << 16) | fraction);
        bool collided = MoveEnemyHorizontallyTreatingSlopesAsWalls(
            level,
            slot,
            displacement);

        if (temporarilyReachTowardSamus)
        {
            slot.XPosition = unchecked((ushort)(
                slot.XPosition + (state.MovingLeft ? 16 : -16)));
        }

        if (!collided)
            return;

        state.MovingLeft = !state.MovingLeft;
        state.RequestedInstructionListIndex = unchecked((ushort)(
            state.RequestedInstructionListIndex ^ 4));
        InstallMaridiaLargeSnailInstructionList(slot, state);
    }

    private static void UpdateMaridiaLargeSnailSamusProximity(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState samus)
    {
        state.SamusInPushingRange = false;
        state.SamusIsLeft = false;
        if (!IsWithinStrictModularDistance(
                samus.YPosition,
                slot.YPosition,
                MaridiaLargeSnailPushingYDistance) ||
            !IsWithinStrictModularDistance(
                samus.XPosition,
                slot.XPosition,
                MaridiaLargeSnailPushingXDistance))
        {
            return;
        }

        state.SamusInPushingRange = true;
        state.SamusIsLeft = unchecked((short)(samus.XPosition - slot.XPosition)) < 0;
    }

    private static void CarrySamusWithMaridiaLargeSnail(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState samus)
    {
        state.StopMovementNextFrame = false;
        if (MaridiaLargeSnailTouchesSamusFromBelow(slot, samus))
        {
            // Unlike most moving-platform producers, Oum assigns—not adds—the vertical
            // displacement word. This preserves the last touching Oum's native ownership.
            samus.Kinematics.ExtraYDisplacement = unchecked((ushort)(
                slot.YPosition - state.PreviousYPosition));
        }

        if (!state.SamusInPushingRange)
            return;

        ushort deltaX = unchecked((ushort)(slot.XPosition - state.PreviousXPosition));
        bool movedLeft = unchecked((short)deltaX) < 0;
        if (movedLeft != state.SamusIsLeft)
            return;

        samus.Kinematics.ExtraXDisplacement = unchecked((ushort)(
            samus.Kinematics.ExtraXDisplacement + deltaX));
    }

    private static void UpdateMaridiaLargeSnailInputStop(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        ushort controllerInput)
    {
        if (!state.SamusInPushingRange)
            return;

        short deltaX = unchecked((short)(slot.XPosition - state.PreviousXPosition));
        bool stop;
        if (deltaX == 0)
        {
            stop = state.SamusIsLeft
                ? state.MovingLeft &&
                    (controllerInput & (ushort)SnesButton.Right) != 0
                : !state.MovingLeft &&
                    (controllerInput & (ushort)SnesButton.Left) != 0;
        }
        else if (deltaX < 0)
        {
            stop = (controllerInput & (ushort)SnesButton.Right) != 0;
        }
        else
        {
            stop = (controllerInput & (ushort)SnesButton.Left) != 0;
        }

        if (!stop)
            return;

        state.StopMovementNextFrame = true;
        slot.XPosition = state.PreviousXPosition;
    }

    /// <summary>
    /// Exact $A0:ABE7 asymmetric "touching from below" predicate used for Oum carrying and
    /// its post-contact shove. It is intentionally not interchangeable with radius overlap.
    /// </summary>
    private static bool MaridiaLargeSnailTouchesSamusFromBelow(
        RoomEnemySlot slot,
        SamusState samus)
    {
        ushort xDistance = WrappedMagnitude(unchecked((ushort)(
            samus.XPosition - slot.XPosition)));
        bool centerWithinSamusRadius = xDistance < samus.Kinematics.XRadius;
        ushort remainingX = unchecked((ushort)(xDistance - samus.Kinematics.XRadius));
        if (!centerWithinSamusRadius && remainingX >= slot.XRadius)
            return false;

        ushort biasedSamusY = unchecked((ushort)(samus.YPosition + 3));
        if (unchecked((short)(biasedSamusY - slot.YPosition)) >= 0)
            return false;

        ushort verticalDistance = unchecked((ushort)(slot.YPosition - biasedSamusY));
        ushort remainingY = unchecked((ushort)(verticalDistance - samus.Kinematics.YRadius));
        return verticalDistance < samus.Kinematics.YRadius ||
            remainingY == slot.YRadius ||
            remainingY < slot.YRadius;
    }

    /// <summary>Ports all four private no-operand instruction callbacks.</summary>
    private bool TryProcessMaridiaLargeSnailInstruction(
        RoomEnemySlot slot,
        ushort opcode,
        ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != MaridiaLargeSnailDefinition)
            return false;

        MaridiaLargeSnailEnemyState state = RequireMaridiaLargeSnailState(slot);
        switch (opcode)
        {
            case MaridiaLargeSnailSplashInstruction:
                LastMaridiaLargeSnailSoundEffect = MaridiaLargeSnailSplashSound;
                break;
            case MaridiaLargeSnailAttackFinishedInstruction:
                state.AttackAnimationFinished = true;
                break;
            case MaridiaLargeSnailAllowRotationInstruction:
                state.AttackAllowsRotation = true;
                break;
            case MaridiaLargeSnailDisallowRotationInstruction:
                state.AttackAllowsRotation = false;
                break;
            default:
                return false;
        }

        cursor = unchecked((ushort)(cursor + 2));
        return true;
    }

    private void InstallMaridiaLargeSnailInstructionList(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state)
    {
        if (state.RequestedInstructionListIndex == state.InstalledInstructionListIndex)
            return;
        if (state.RequestedInstructionListIndex >= 8)
        {
            throw new InvalidDataException(
                $"Maridia Large Snail list index {state.RequestedInstructionListIndex} " +
                "is outside the retail eight-entry table.");
        }

        state.InstalledInstructionListIndex = state.RequestedInstructionListIndex;
        slot.CurrentInstruction = ReadWord(
            _bus!,
            MaridiaLargeSnailInstructionListTable +
                state.RequestedInstructionListIndex * 2);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>
    /// Runs the common/non-damaging Oum hitbox tail after the multibox dispatcher selected a
    /// concrete frame hitbox. Damaging hitboxes call common touch before entering this tail.
    /// </summary>
    private static void ResolveMaridiaLargeSnailTouchAfterCommon(
        RoomEnemySlot slot,
        SamusState samus)
    {
        if (MaridiaLargeSnailTouchesSamusFromBelow(slot, samus))
            return;

        ushort shove = unchecked((short)(samus.XPosition - slot.XPosition)) < 0
            ? unchecked((ushort)-4)
            : (ushort)4;
        samus.Kinematics.ExtraXDisplacement = unchecked((ushort)(
            samus.Kinematics.ExtraXDisplacement + shove));
    }

    /// <summary>
    /// Ports the private tail of <c>EnemyShot_Oum</c> at $A2:D3B8. It runs after common shot
    /// AI for damaging, freezing, and zero-damage vulnerability results alike.
    /// </summary>
    private void ResolveMaridiaLargeSnailShotAfterCommon() =>
        LastMaridiaLargeSnailSoundEffect = MaridiaLargeSnailShotSound;

    private MaridiaLargeSnailEnemyState RequireMaridiaLargeSnailState(RoomEnemySlot slot) =>
        _maridiaLargeSnailStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Maridia Large Snail state.");
}
