using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Native two-entry horizontal dispatcher used by Tripper and Kamer (the suspensor
/// platform). These values are indexes, not bank-$A3 function pointers: the ROM doubles
/// each word before indexing the pointer table at <c>$A3:9C97</c>.
/// </summary>
public enum PlatformHorizontalMovement : ushort
{
    Left = 0,
    Right = 1,
}

/// <summary>
/// Native two-entry vertical dispatcher selected every enemy frame by the platform/Samus
/// overlap test at <c>$A0:ABE7</c>.
/// </summary>
public enum PlatformVerticalMovement : ushort
{
    Rising = 0,
    Sinking = 1,
}

/// <summary>
/// Typed view of the six common enemy-slot words and eight extra-WRAM words shared by
/// Tripper <c>$D7FF</c> and Kamer <c>$D83F</c>.
/// </summary>
/// <remarks>
/// The ROM deliberately aliases <see cref="PreviousPosition"/>: horizontal AI stores the
/// pre-move X position there, then sinking AI consumes that X delta and overwrites the same
/// word with the pre-move Y position. Keeping one property instead of convenient X/Y copies
/// preserves that ordering and makes the debugger expose the actual WRAM contract.
/// </remarks>
public sealed class PlatformEnemyState
{
    private readonly RoomEnemySlot _slot;
    private readonly ushort[] _yMovementFunctions;
    private readonly ushort[] _previousPositions;
    private readonly ushort[] _xMovementFunctions;
    private readonly ushort[] _verticallyMovingFlags;
    private readonly ushort[] _verticallyStillFlags;
    private readonly ushort[] _maximumYSpeedTableIndexes;
    private readonly ushort[] _previousYMovementFunctions;
    private readonly ushort[] _suspensorPlatformFlags;

    internal PlatformEnemyState(
        RoomEnemySlot slot,
        ushort[] yMovementFunctions,
        ushort[] previousPositions,
        ushort[] xMovementFunctions,
        ushort[] verticallyMovingFlags,
        ushort[] verticallyStillFlags,
        ushort[] maximumYSpeedTableIndexes,
        ushort[] previousYMovementFunctions,
        ushort[] suspensorPlatformFlags)
    {
        _slot = slot;
        _yMovementFunctions = yMovementFunctions;
        _previousPositions = previousPositions;
        _xMovementFunctions = xMovementFunctions;
        _verticallyMovingFlags = verticallyMovingFlags;
        _verticallyStillFlags = verticallyStillFlags;
        _maximumYSpeedTableIndexes = maximumYSpeedTableIndexes;
        _previousYMovementFunctions = previousYMovementFunctions;
        _suspensorPlatformFlags = suspensorPlatformFlags;
    }

    /// <summary>One pixel below the population Y position, written by common init.</summary>
    public ushort TargetYPosition
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Fractional half of the positive/right common linear speed.</summary>
    public ushort RightSubvelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Signed whole half of the positive/right common linear speed.</summary>
    public short RightVelocity
    {
        get => unchecked((short)_slot.VariableC);
        internal set => _slot.VariableC = unchecked((ushort)value);
    }

    /// <summary>Fractional half of the negative/left common linear speed.</summary>
    public ushort LeftSubvelocity
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Signed whole half of the negative/left common linear speed.</summary>
    public short LeftVelocity
    {
        get => unchecked((short)_slot.VariableE);
        internal set => _slot.VariableE = unchecked((ushort)value);
    }

    /// <summary>Logical index into the eight-byte common quadratic-speed records.</summary>
    public ushort YSpeedTableIndex
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    public PlatformVerticalMovement YMovement
    {
        get => (PlatformVerticalMovement)_yMovementFunctions[_slot.SlotIndex];
        internal set => _yMovementFunctions[_slot.SlotIndex] = (ushort)value;
    }

    /// <summary>Pre-horizontal X, then pre-vertical Y, exactly as described above.</summary>
    public ushort PreviousPosition
    {
        get => _previousPositions[_slot.SlotIndex];
        internal set => _previousPositions[_slot.SlotIndex] = value;
    }

    public PlatformHorizontalMovement XMovement
    {
        get => (PlatformHorizontalMovement)_xMovementFunctions[_slot.SlotIndex];
        internal set => _xMovementFunctions[_slot.SlotIndex] = (ushort)value;
    }

    /// <summary>Nonzero after moving art has been installed for the current phase.</summary>
    public bool VerticallyMovingArtInstalled
    {
        get => _verticallyMovingFlags[_slot.SlotIndex] != 0;
        internal set => _verticallyMovingFlags[_slot.SlotIndex] = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>Nonzero after vertically-still art has been installed.</summary>
    public bool VerticallyStillArtInstalled
    {
        get => _verticallyStillFlags[_slot.SlotIndex] != 0;
        internal set => _verticallyStillFlags[_slot.SlotIndex] = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>Parameter-two high byte, used as an inclusive acceleration clamp.</summary>
    public ushort MaximumYSpeedTableIndex
    {
        get => _maximumYSpeedTableIndexes[_slot.SlotIndex];
        internal set => _maximumYSpeedTableIndexes[_slot.SlotIndex] = value;
    }

    /// <summary>Prior frame's rider-selected vertical dispatcher index.</summary>
    public PlatformVerticalMovement PreviousYMovement
    {
        get => (PlatformVerticalMovement)_previousYMovementFunctions[_slot.SlotIndex];
        internal set => _previousYMovementFunctions[_slot.SlotIndex] = (ushort)value;
    }

    /// <summary>
    /// True only for Kamer. Native WRAM stores <c>$FFFF</c> for Kamer and zero for Tripper;
    /// this semantic projection retains those exact backing words.
    /// </summary>
    public bool IsSuspensorPlatform
    {
        get => unchecked((short)_suspensorPlatformFlags[_slot.SlotIndex]) < 0;
        internal set => _suspensorPlatformFlags[_slot.SlotIndex] =
            value ? ushort.MaxValue : (ushort)0;
    }

    public int RightDisplacement => (RightVelocity << 16) | RightSubvelocity;

    public int LeftDisplacement => (LeftVelocity << 16) | LeftSubvelocity;
}

/// <summary>
/// Literal shared translation of Tripper <c>$D7FF</c> and Kamer <c>$D83F</c> at
/// <c>$A3:9BBB-$9F28</c>.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort TripperDefinition = 0xd7ff;
    internal const ushort KamerDefinition = 0xd83f;

    private const ushort KamerMovingLeftInstruction = 0x9bbb;
    private const ushort KamerMovingRightInstruction = 0x9bd1;
    private const ushort KamerStillLeftInstruction = 0x9be7;
    private const ushort KamerStillRightInstruction = 0x9bfd;
    private const ushort TripperMovingLeftInstruction = 0x9c13;
    private const ushort TripperMovingRightInstruction = 0x9c29;
    private const ushort TripperStillMovingLeftInstruction = 0x9c3f;
    private const ushort TripperStillMovingRightInstruction = 0x9c55;

    internal const ushort PlatformNoOpTouchAi = EnemyAiCodePointers.BankA3.PlatformNoOpTouch;
    internal const ushort TripperShotAi = EnemyAiCodePointers.BankA3.TripperShot;
    internal const ushort TripperFrozenMovingLeftSpritemap = 0xa009;
    internal const ushort TripperFrozenMovingRightSpritemap = 0xa015;

    private readonly ushort[] _platformYMovementFunctions = new ushort[MaximumEnemyCount];
    private readonly ushort[] _platformPreviousPositions = new ushort[MaximumEnemyCount];
    private readonly ushort[] _platformXMovementFunctions = new ushort[MaximumEnemyCount];
    private readonly ushort[] _platformVerticallyMovingFlags = new ushort[MaximumEnemyCount];
    private readonly ushort[] _platformVerticallyStillFlags = new ushort[MaximumEnemyCount];
    private readonly ushort[] _platformMaximumYSpeedTableIndexes = new ushort[MaximumEnemyCount];
    private readonly ushort[] _platformPreviousYMovementFunctions = new ushort[MaximumEnemyCount];
    private readonly ushort[] _platformSuspensorPlatformFlags = new ushort[MaximumEnemyCount];
    private readonly PlatformEnemyState?[] _platformStates =
        new PlatformEnemyState?[MaximumEnemyCount];

    /// <summary>Typed Tripper/Kamer state for all 32 physical enemy slots.</summary>
    public IReadOnlyList<PlatformEnemyState?> PlatformStates => _platformStates;

    private static bool IsPlatformDefinition(ushort definition) =>
        definition is TripperDefinition or KamerDefinition;

    /// <summary>Ports the two entry points and common initializer at $A3:9C9F-$9D15.</summary>
    private void InitializePlatform(RoomEnemySlot slot)
    {
        bool isKamer = slot.EnemyDefinitionPointer == KamerDefinition;
        var state = new PlatformEnemyState(
            slot,
            _platformYMovementFunctions,
            _platformPreviousPositions,
            _platformXMovementFunctions,
            _platformVerticallyMovingFlags,
            _platformVerticallyStillFlags,
            _platformMaximumYSpeedTableIndexes,
            _platformPreviousYMovementFunctions,
            _platformSuspensorPlatformFlags);
        _platformStates[slot.SlotIndex] = state;

        state.IsSuspensorPlatform = isKamer;
        // Main AI dispatches the complete word through a two-entry table. Initialization's
        // art branch merely tests zero/nonzero, so preserve a corrupt value here and let the
        // typed dispatcher reject it instead of silently normalizing it to index one.
        state.XMovement = (PlatformHorizontalMovement)slot.Parameter1;

        // Kamer and Tripper use deliberately different apparent facing names for their
        // vertically-still lists. Select the literal list pointer here; later helpers retain
        // that same species-specific table instead of trying to infer art orientation.
        slot.CurrentInstruction = isKamer
            ? state.XMovement == PlatformHorizontalMovement.Left
                ? KamerStillLeftInstruction
                : KamerStillRightInstruction
            : state.XMovement == PlatformHorizontalMovement.Left
                ? TripperStillMovingLeftInstruction
                : TripperStillMovingRightInstruction;

        // The low byte is a common-linear-speed magnitude. Multiplication by eight is the
        // native byte offset across {right whole/fraction, left whole/fraction}.
        ushort speedTableOffset = unchecked((ushort)((slot.Parameter2 & 0x00ff) * 8));
        (state.RightVelocity, state.RightSubvelocity) =
            ReadLinearEnemySpeed(speedTableOffset);
        (state.LeftVelocity, state.LeftSubvelocity) =
            ReadLinearEnemySpeed(unchecked((ushort)(speedTableOffset + 4)));

        state.YMovement = PlatformVerticalMovement.Rising;
        state.PreviousYMovement = PlatformVerticalMovement.Rising;
        state.VerticallyMovingArtInstalled = false;
        state.VerticallyStillArtInstalled = false;
        state.TargetYPosition = unchecked((ushort)(slot.YPosition + 1));
        state.YSpeedTableIndex = 0;
        state.MaximumYSpeedTableIndex = unchecked((ushort)(slot.Parameter2 >> 8));
    }

    /// <summary>Ports <c>MainAI_Tripper_Kamer2</c> at $A3:9D16.</summary>
    private void RunPlatformMain(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Platform AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Platform AI requires active room collision data.");

        // Every frame starts in the rising dispatcher and changes to sinking only when the
        // exact asymmetric "Samus from below" test succeeds. This is also the native solid-
        // platform riding gate; ordinary radius overlap would trigger from the sides.
        state.YMovement = IsSamusRidingPlatform(slot, samus)
            ? PlatformVerticalMovement.Sinking
            : PlatformVerticalMovement.Rising;

        MovePlatformHorizontally(slot, state, level);
        MovePlatformVertically(slot, state, samus, level);

        // Native main AI resets acceleration after running the first frame in a newly
        // selected vertical state. Preserve that slightly surprising ordering: a new phase
        // can use quadratic entry one now, then begins from entry zero on its next frame.
        if (state.YMovement != state.PreviousYMovement)
            state.YSpeedTableIndex = 0;
        state.PreviousYMovement = state.YMovement;
    }

    /// <summary>Exact wrapped-word port of <c>$A0:ABE7-$AC28</c>.</summary>
    private static bool IsSamusRidingPlatform(RoomEnemySlot slot, SamusState samus)
    {
        ushort xDistance = unchecked((ushort)(samus.XPosition - slot.XPosition));
        if (unchecked((short)xDistance) < 0)
            xDistance = unchecked((ushort)-xDistance);

        // Carry clear after subtracting Samus's radius jumps directly to Y. Otherwise the
        // remainder must be strictly less than the platform radius; equality is not overlap.
        if (xDistance >= samus.Kinematics.XRadius)
        {
            ushort remainder = unchecked((ushort)(xDistance - samus.Kinematics.XRadius));
            if (remainder >= slot.XRadius)
                return false;
        }

        // The +3 bias requires Samus's center to remain above the enemy center. After taking
        // the wrapped magnitude, Y accepts equality with the enemy radius (unlike X).
        ushort yDifference = unchecked((ushort)(samus.YPosition + 3 - slot.YPosition));
        if (unchecked((short)yDifference) >= 0)
            return false;
        ushort yDistance = unchecked((ushort)-yDifference);
        if (yDistance < samus.Kinematics.YRadius)
            return true;
        ushort yRemainder = unchecked((ushort)(yDistance - samus.Kinematics.YRadius));
        return yRemainder <= slot.YRadius;
    }

    private static void MovePlatformHorizontally(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        RoomLevelData level)
    {
        state.PreviousPosition = slot.XPosition;
        bool movingLeft = state.XMovement switch
        {
            PlatformHorizontalMovement.Left => true,
            PlatformHorizontalMovement.Right => false,
            _ => throw new InvalidDataException(
                $"Platform X movement index {(ushort)state.XMovement} exceeds its two-entry table."),
        };
        int displacement = movingLeft ? state.LeftDisplacement : state.RightDisplacement;
        if (!MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            return;

        // A wall collision reverses the dispatcher and immediately installs the opposite
        // vertically-still list. The first opcode in that list republishes the same index
        // when instruction processing runs later in this enemy frame.
        state.XMovement = movingLeft
            ? PlatformHorizontalMovement.Right
            : PlatformHorizontalMovement.Left;
        InstallPlatformVerticallyStillForCurrentDirection(slot, state);
    }

    private void MovePlatformVertically(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        switch (state.YMovement)
        {
            case PlatformVerticalMovement.Rising:
                RaisePlatform(slot, state, level);
                return;
            case PlatformVerticalMovement.Sinking:
                SinkPlatform(slot, state, samus, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Platform Y movement index {(ushort)state.YMovement} exceeds its two-entry table.");
        }
    }

    private void RaisePlatform(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        RoomLevelData level)
    {
        // Signed BMI comparison is intentional. Normal platforms stop once they have risen
        // from their depressed position to one pixel below their original spawn center.
        if (unchecked((short)(slot.YPosition - state.TargetYPosition)) < 0)
        {
            state.YSpeedTableIndex = 0;
            SetPlatformVerticallyStillInstruction(slot, state);
            return;
        }

        SetPlatformVerticallyMovingInstruction(slot, state);
        IncrementAndClampPlatformYSpeed(state);
        int displacement = ReadQuadraticEnemySpeed(
            state.YSpeedTableIndex,
            negative: true);
        if (!MoveEnemyVertically(level, slot, displacement))
            return;

        state.YSpeedTableIndex = 0;
        SetPlatformVerticallyStillInstruction(slot, state);
    }

    private void SinkPlatform(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // `$A3:9DE7-$9DED` performs a comparison followed by a JSR to an immediate RTS; it
        // has no branch or side effect. Sinking therefore always continues to acceleration.
        IncrementAndClampPlatformYSpeed(state);

        // Horizontal movement saved the pre-move X position in the aliased word. Add the
        // wrapped whole-pixel delta to the shared producer word; fractional displacement is
        // deliberately untouched because this enemy writes only `$0B58`.
        ushort xDelta = unchecked((ushort)(slot.XPosition - state.PreviousPosition));
        samus.Kinematics.ExtraXDisplacement = unchecked((ushort)(
            samus.Kinematics.ExtraXDisplacement + xDelta));

        state.PreviousPosition = slot.YPosition;
        int displacement = ReadQuadraticEnemySpeed(
            state.YSpeedTableIndex,
            negative: false);
        if (MoveEnemyVertically(level, slot, displacement))
        {
            state.YSpeedTableIndex = 0;
            SetPlatformVerticallyStillInstruction(slot, state);
        }

        // This delta is measured after collision clipping, so Samus follows the platform's
        // accepted motion rather than the requested quadratic speed.
        ushort yDelta = unchecked((ushort)(slot.YPosition - state.PreviousPosition));
        samus.Kinematics.ExtraYDisplacement = unchecked((ushort)(
            samus.Kinematics.ExtraYDisplacement + yDelta));
    }

    private static void IncrementAndClampPlatformYSpeed(PlatformEnemyState state)
    {
        state.YSpeedTableIndex = unchecked((ushort)(state.YSpeedTableIndex + 1));
        if (unchecked((short)(
                state.YSpeedTableIndex - state.MaximumYSpeedTableIndex)) >= 0)
        {
            state.YSpeedTableIndex = state.MaximumYSpeedTableIndex;
        }
    }

    private static void SetPlatformVerticallyMovingInstruction(
        RoomEnemySlot slot,
        PlatformEnemyState state)
    {
        if (!state.VerticallyMovingArtInstalled)
        {
            state.VerticallyMovingArtInstalled = true;
            InstallPlatformVerticallyMovingForCurrentDirection(slot, state);
        }
        // `$A3:9ED9` is below the already-installed branch target, so it executes even when
        // no new list was needed this frame.
        state.VerticallyStillArtInstalled = false;
    }

    private static void SetPlatformVerticallyStillInstruction(
        RoomEnemySlot slot,
        PlatformEnemyState state)
    {
        if (!state.VerticallyStillArtInstalled)
        {
            state.VerticallyStillArtInstalled = true;
            InstallPlatformVerticallyStillForCurrentDirection(slot, state);
        }
        // Same merge-label detail as the moving helper: the opposite flag is always clear.
        state.VerticallyMovingArtInstalled = false;
    }

    private static void InstallPlatformVerticallyMovingForCurrentDirection(
        RoomEnemySlot slot,
        PlatformEnemyState state)
    {
        ushort instruction = state.IsSuspensorPlatform
            ? state.XMovement == PlatformHorizontalMovement.Left
                ? KamerMovingLeftInstruction
                : KamerMovingRightInstruction
            : state.XMovement == PlatformHorizontalMovement.Left
                ? TripperMovingLeftInstruction
                : TripperMovingRightInstruction;
        InstallPlatformInstruction(slot, instruction);
    }

    private static void InstallPlatformVerticallyStillForCurrentDirection(
        RoomEnemySlot slot,
        PlatformEnemyState state)
    {
        ushort instruction = state.IsSuspensorPlatform
            ? state.XMovement == PlatformHorizontalMovement.Left
                ? KamerStillLeftInstruction
                : KamerStillRightInstruction
            : state.XMovement == PlatformHorizontalMovement.Left
                ? TripperStillMovingLeftInstruction
                : TripperStillMovingRightInstruction;
        InstallPlatformInstruction(slot, instruction);
    }

    private static void InstallPlatformInstruction(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Ports the four equivalent list commands at $A3:9C6B/$9C76/$9C81/$9C8C.</summary>
    private void SetPlatformHorizontalMovementFromInstruction(
        RoomEnemySlot slot,
        PlatformHorizontalMovement movement)
    {
        RequirePlatformState(slot).XMovement = movement;
    }

    private PlatformEnemyState RequirePlatformState(RoomEnemySlot slot) =>
        _platformStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Tripper/Kamer state.");
}
