using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Platforms;

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
    /// <summary>Enemy slot whose variable words carry target height and velocity data.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Shared per-slot horizontal and rider-selected vertical dispatcher indexes.</summary>
    private readonly PlatformMovementFunctions[] _movementFunctions;
    /// <summary>Shared word reused for pre-move X and then pre-move Y within an update.</summary>
    private readonly ushort[] _previousPositions;
    /// <summary>Per-slot flags that suppress reinstalling moving artwork.</summary>
    private readonly ushort[] _verticallyMovingFlags;
    /// <summary>Per-slot flags that suppress reinstalling stationary artwork.</summary>
    private readonly ushort[] _verticallyStillFlags;
    /// <summary>Per-slot upper limits for quadratic vertical acceleration.</summary>
    private readonly ushort[] _maximumYSpeedTableIndexes;
    /// <summary>Previous rider-selected phase, used when vertical motion changes.</summary>
    private readonly ushort[] _previousYMovementFunctions;
    /// <summary>Native all-ones Kamer marker or zero for Tripper.</summary>
    private readonly ushort[] _suspensorPlatformFlags;

    /// <summary>Binds one actor to its entries in the shared platform WRAM-style arrays.</summary>
    internal PlatformEnemyState(RoomEnemySlot slot, PlatformSharedTables tables)
    {
        _slot = slot;
        _movementFunctions = tables.MovementFunctions;
        _previousPositions = tables.PreviousPositions;
        _verticallyMovingFlags = tables.VerticallyMovingFlags;
        _verticallyStillFlags = tables.VerticallyStillFlags;
        _maximumYSpeedTableIndexes = tables.MaximumYSpeedTableIndexes;
        _previousYMovementFunctions = tables.PreviousYMovementFunctions;
        _suspensorPlatformFlags = tables.SuspensorPlatformFlags;
    }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<PlatformProgram> Program { get; set; }

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

    /// <summary>Current rider-selected vertical dispatcher index, native <c>Platform.YMovementFunctionIndex</c> at <c>$7E:7800,x</c>; recomputed each actor update before horizontal then vertical motion.</summary>
    public PlatformVerticalMovement YMovement
    {
        get => _movementFunctions[_slot.SlotIndex].Vertical;
        internal set => _movementFunctions[_slot.SlotIndex] = _movementFunctions[_slot.SlotIndex] with { Vertical = value };
    }

    /// <summary>Pre-horizontal X, then pre-vertical Y, exactly as described above.</summary>
    public ushort PreviousPosition
    {
        get => _previousPositions[_slot.SlotIndex];
        internal set => _previousPositions[_slot.SlotIndex] = value;
    }

    /// <summary>Current horizontal dispatcher index, native <c>Platform.XMovementFunctionIndex</c> at <c>$7E:7804,x</c>; initialized from population parameter 1 and changed by wall collisions or animation commands.</summary>
    public PlatformHorizontalMovement XMovement
    {
        get => _movementFunctions[_slot.SlotIndex].Horizontal;
        internal set => _movementFunctions[_slot.SlotIndex] = _movementFunctions[_slot.SlotIndex] with { Horizontal = value };
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

    /// <summary>Signed 16.16 room-pixel displacement per horizontal actor update, combining the rightward whole and fractional velocity words without changing backing state.</summary>
    public int RightDisplacement => (SnesSixteenSixteen.FromWhole(RightVelocity)) | RightSubvelocity;

    /// <summary>Signed 16.16 room-pixel displacement per horizontal actor update, combining the leftward whole and fractional velocity words and preserving their two's-complement representation.</summary>
    public int LeftDisplacement => (SnesSixteenSixteen.FromWhole(LeftVelocity)) | LeftSubvelocity;
}
