using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Native two-entry horizontal dispatcher used by Tripper and Kamer (the suspensor
/// platform). These values are indexes, not bank-$A3 function pointers: the ROM doubles
/// each word before indexing the pointer table at <c>$A3:9C97</c>.
/// </summary>
public enum PlatformHorizontalMovement : ushort
{
    /// <summary>Dispatcher index zero selects negative/left horizontal motion; a wall collision switches to index one and installs right-facing artwork.</summary>
    Left = 0,
    /// <summary>Dispatcher index one selects positive/right horizontal motion; a wall collision switches to index zero and installs left-facing artwork.</summary>
    Right = 1,
}

/// <summary>
/// Native two-entry vertical dispatcher selected every enemy frame by the platform/Samus
/// overlap test at <c>$A0:ABE7</c>.
/// </summary>
public enum PlatformVerticalMovement : ushort
{
    /// <summary>Dispatcher index zero, selected without an accepted rider overlap; accelerates upward until the target-height or collision condition leaves the platform vertically still.</summary>
    Rising = 0,
    /// <summary>Dispatcher index one, selected by the asymmetric Samus-above-platform overlap test; accelerates downward and publishes accepted whole-pixel X/Y movement as Samus's extra displacement.</summary>
    Sinking = 1,
}

/// <summary>
/// One platform's two native dispatcher indexes: the patrol direction
/// (<c>Platform.XMovementFunctionIndex</c>, <c>$7E:7804,x</c>) and the rider-selected vertical
/// movement (<c>Platform.YMovementFunctionIndex</c>, <c>$7E:7800,x</c>).
/// </summary>
public readonly record struct PlatformMovementFunctions(
    PlatformHorizontalMovement Horizontal,
    PlatformVerticalMovement Vertical);

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
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Stores movement functions.</summary>
    private readonly PlatformMovementFunctions[] _movementFunctions;
    /// <summary>Stores previous positions.</summary>
    private readonly ushort[] _previousPositions;
    /// <summary>Stores vertically moving flags.</summary>
    private readonly ushort[] _verticallyMovingFlags;
    /// <summary>Stores vertically still flags.</summary>
    private readonly ushort[] _verticallyStillFlags;
    /// <summary>Stores maximum yspeed table indexes.</summary>
    private readonly ushort[] _maximumYSpeedTableIndexes;
    /// <summary>Stores previous ymovement functions.</summary>
    private readonly ushort[] _previousYMovementFunctions;
    /// <summary>Stores suspensor platform flags.</summary>
    private readonly ushort[] _suspensorPlatformFlags;

    /// <summary>Initializes a new platform enemy state instance.</summary>
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
