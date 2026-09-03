using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Cartridge-authored room-main state for Ceres room <c>$DF45</c>'s elevator shaft.
/// </summary>
/// <remarks>
/// Door ASM <c>$8F:E4E0</c> seeds two otherwise anonymous room-main words before the
/// shaft becomes visible. Room main <c>$89:ACC3</c> later consumes those words as a
/// bidirectional index through the three-word records at <c>$89:AD5F</c>. Giving those
/// words a room-scoped owner prevents their unusual signed wrap from leaking into the
/// general runtime and makes the final elevator trigger independently testable.
/// </remarks>
public sealed class CeresElevatorShaftRoomMainState
{
    /// <summary>First timer/sine/cosine record used by <c>$89:ACC3</c>.</summary>
    public const int RotationTableAddress = 0x89ad5f;

    /// <summary>Initial value written to RoomMainASMVar1 by door ASM <c>$8F:E4E0</c>.</summary>
    public const ushort InitialRotationIndex = 0x0022;

    /// <summary>Initial value written to RoomMainASMVar2 by door ASM <c>$8F:E4E0</c>.</summary>
    public const ushort InitialRotationTimer = 0x003c;

    /// <summary>Native state-$20 hold written by <c>SamusCode_02_ReachCeresElevator</c>.</summary>
    public const ushort DepartureHoldFrames = 60;

    /// <summary>Whether the active room owns this room-main routine.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Live RoomMainASMVar1, including its encoded negative sweep.</summary>
    public ushort RotationIndex { get; private set; }

    /// <summary>Live RoomMainASMVar2 countdown.</summary>
    public ushort RotationTimer { get; private set; }

    /// <summary>True after the elevator bounds have admitted Samus once.</summary>
    public bool DepartureRequested { get; private set; }

    /// <summary>One-frame publication consumed by the outer game-state dispatcher.</summary>
    public bool DepartureRequestedThisFrame { get; private set; }

    /// <summary>Most recently published Mode 7 matrix.</summary>
    public SamusMode7Transform Transform { get; private set; } = CreateInitialTransform();

    /// <summary>
    /// Applies the exact room-entry values written by door ASM <c>$8F:E4E0</c>.
    /// </summary>
    public void Reset(bool active)
    {
        IsActive = active;
        RotationIndex = InitialRotationIndex;
        RotationTimer = InitialRotationTimer;
        DepartureRequested = false;
        DepartureRequestedThisFrame = false;
        Transform = CreateInitialTransform();
    }

    /// <summary>Executes one call to room main <c>$89:ACC3</c>.</summary>
    /// <param name="bus">Cartridge address space which owns the rotation records.</param>
    /// <param name="samus">Live Samus state inspected by the elevator trigger.</param>
    /// <param name="ceresStatus">Bank-$A6 Ceres status word at native WRAM <c>$093F</c>.</param>
    /// <param name="allowDeparture">
    /// True only when the outer dispatcher is state eight. States $20/$21 still call the
    /// ordinary gameplay body, but the native room main explicitly rejects their game-state
    /// numbers; keeping that input explicit preserves the same ownership boundary.
    /// </param>
    public CeresElevatorShaftRoomMainResult Step(
        ISnesAddressSpace bus,
        SamusState? samus,
        ushort ceresStatus,
        bool allowDeparture)
    {
        ArgumentNullException.ThrowIfNull(bus);
        DepartureRequestedThisFrame = false;

        // The room code is entirely dormant before Ridley's self-destruct sequence sets
        // bit 15. In particular, the initial elevator descent retains door ASM's identity
        // matrix instead of prematurely consuming the rotation table.
        if (!IsActive || (ceresStatus & 0x8000) == 0)
            return Snapshot(matrixChanged: false);

        if (allowDeparture &&
            !DepartureRequested &&
            samus is not null &&
            IsInsideDepartureTrigger(samus))
        {
            // `SamusCode_02_ReachCeresElevator` chooses pose $01/$02 from the current
            // cartridge x-direction metadata, refreshes pose radii/animation, seeds the
            // 60-frame state-$20 hold, then installs SamusCode_00's input lock. The hold is
            // owned by the frontend dispatcher; everything Samus-visible remains here.
            bool facingLeft = samus.ReadPoseXDirection(bus) == 4;
            samus.Pose = facingLeft
                ? SamusPoseIds.FacingLeftNormalPose
                : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.InputLocked = true;
            DepartureRequested = true;
            DepartureRequestedThisFrame = true;
        }

        // DEC followed by BMI is deliberately modeled as a wrapping 16-bit decrement.
        // Zero remains visible for one complete call; only the following call makes it
        // $FFFF and consumes the next record. This differs from a conventional <= 0 timer.
        NativeWordCounterStep timer = NativeWordCounter.Decrement(RotationTimer);
        RotationTimer = timer.Value;
        if (timer.IsNonNegative)
            return Snapshot(matrixChanged: false);

        // Native computes `(uint16)(6 * index) >> 1`, not a conventional array index.
        // The encoded phase $8044 relies on the multiplication wrapping before the shift,
        // mapping back into the tail of this very table during the reverse sweep.
        ushort byteOffset = unchecked((ushort)(6 * RotationIndex));
        int wordIndex = byteOffset >> 1;
        int recordAddress = RotationTableAddress + wordIndex * 2;
        RotationTimer = RomDataReader.ReadWordFixedBank(bus, recordAddress);
        ushort sine = RomDataReader.ReadWordFixedBank(bus, recordAddress + 2);
        ushort cosine = RomDataReader.ReadWordFixedBank(bus, recordAddress + 4);
        Transform = new SamusMode7Transform(
            MatrixA: cosine,
            MatrixB: sine,
            MatrixC: unchecked((ushort)-sine),
            CenterX: 0x0080,
            CenterY: 0x03f0);

        // Values 0..67 sweep forward. The comparison occurs after INC, so 67 becomes
        // encoded negative phase $8044. Negative phases decrement until $8001 maps to
        // zero, restarting the forward sweep without ever exposing $8000.
        if (unchecked((short)RotationIndex) < 0)
        {
            RotationIndex = RotationIndex == 0x8001
                ? (ushort)0
                : unchecked((ushort)(RotationIndex - 1));
        }
        else
        {
            RotationIndex = RotationIndex == 67
                ? (ushort)0x8044
                : unchecked((ushort)(RotationIndex + 1));
        }

        return Snapshot(matrixChanged: true);
    }

    private static bool IsInsideDepartureTrigger(SamusState samus) =>
        // `$89:ACD0` uses strict X > 112, inclusive X <= 144, inclusive Y >= 75,
        // and strict Y < 128. Both whole and fractional vertical speed must be zero.
        samus.XPosition > 112 &&
        samus.XPosition <= 144 &&
        samus.YPosition >= 75 &&
        samus.YPosition < 128 &&
        samus.Kinematics.YSpeed == 0 &&
        samus.Kinematics.YSubspeed == 0;

    private CeresElevatorShaftRoomMainResult Snapshot(bool matrixChanged) => new(
        IsActive,
        RotationIndex,
        RotationTimer,
        Transform,
        matrixChanged,
        DepartureRequestedThisFrame);

    private static SamusMode7Transform CreateInitialTransform() => new(
        MatrixA: 0x0100,
        MatrixB: 0,
        MatrixC: 0,
        CenterX: 0x0080,
        CenterY: 0x03f0);
}

/// <summary>Debugger-visible result of one Ceres elevator shaft room-main call.</summary>
public readonly record struct CeresElevatorShaftRoomMainResult(
    bool IsActive,
    ushort RotationIndex,
    ushort RotationTimer,
    SamusMode7Transform Transform,
    bool MatrixChanged,
    bool DepartureRequestedThisFrame);
