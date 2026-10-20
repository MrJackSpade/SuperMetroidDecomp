using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Cartridge-authored room-main state for Ceres room <c>$DF45</c>'s elevator shaft.
/// </summary>
/// <remarks>
/// Door ASM <c>$8F:E4E0</c> seeds two room-main words before the shaft becomes visible.
/// Room main <c>$89:ACC3</c> later consumes them as a bidirectional index through the
/// three-word records at <c>$89:AD5F</c>. The index is the shared <c>RoomMainASMVar1</c>,
/// which the following room inherits; the timer word is owned here.
/// </remarks>
[RoomLifetime]
public sealed class CeresElevatorShaftRoomMainState
{
    /// <summary>Initial value written to RoomMainASMVar1 by door ASM <c>$8F:E4E0</c>.</summary>
    public const ushort InitialRotationIndex = 34;

    /// <summary>Initial value written to RoomMainASMVar2 by door ASM <c>$8F:E4E0</c>.</summary>
    public const ushort InitialRotationTimer = 60;

    /// <summary>Native state-$20 hold written by <c>SamusCode_02_ReachCeresElevator</c>.</summary>
    public const ushort DepartureHoldFrames = 60;

    /// <summary>Whether the active room owns this room-main routine.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Live RoomMainASMVar2 countdown.</summary>
    public NativeCountdown RotationTimer { get; private set; }

    /// <summary>True after the elevator bounds have admitted Samus once.</summary>
    public bool DepartureRequested { get; private set; }

    /// <summary>One-frame publication consumed by the outer game-state dispatcher.</summary>
    public bool DepartureRequestedThisFrame { get; private set; }

    /// <summary>Most recently published Mode 7 matrix.</summary>
    public SamusMode7Transform Transform { get; private set; } = CreateInitialTransform();

    /// <summary>
    /// Builds one room's shaft owner, selecting whether the room owns <c>$89:ACC3</c>. An active entry is the elevator
    /// door, whose ASM <c>$8F:E4E0</c> writes the rotation index and timer.
    /// </summary>
    /// <param name="active">Whether the room's main routine is the shaft and its entry door uses Mode 7.</param>
    /// <param name="scratch">The session's RoomMainASMVar1, which receives the initial rotation index.</param>
    public CeresElevatorShaftRoomMainState(bool active, RoomMainScratchState scratch)
    {
        Ensure.NotNull(scratch);
        IsActive = active;
        if (active)
        {
            scratch.Var1 = InitialRotationIndex;
            RotationTimer = NativeCountdown.Start(InitialRotationTimer);
        }
    }

    /// <summary>
    /// Door setup code leaving the elevator's Mode-7 rendering: the destination's shaft no longer owns the room.
    /// </summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Executes one call to room main <c>$89:ACC3</c>.</summary>
    /// <param name="samus">Live Samus state inspected by the elevator trigger.</param>
    /// <param name="elevatorDoorDestroyed">Whether the Ceres elevator door's destruction has released the rotation.</param>
    /// <param name="allowDeparture">
    /// True only when the outer dispatcher is state eight. States $20/$21 still call the
    /// ordinary gameplay body, but the native room main explicitly rejects their game-state
    /// numbers; keeping that input explicit preserves the same ownership boundary.
    /// </param>
    /// <param name="scratch">Shared mutable room-main scratch: Var1 supplies and receives the encoded forward/reverse rotation index inherited by subsequent room owners; the countdown remains in this state's <see cref="RotationTimer"/>.</param>
    public CeresElevatorShaftRoomMainResult Step(
        SamusState? samus,
        bool elevatorDoorDestroyed,
        bool allowDeparture,
        RoomMainScratchState scratch)
    {
        Ensure.NotNull(scratch);
        DepartureRequestedThisFrame = false;

        // The room code is entirely dormant until the Ceres door's destruction publishes
        // its escape status. In particular, the initial elevator descent retains door ASM's identity
        // matrix instead of prematurely consuming the rotation table.
        if (!IsActive || !elevatorDoorDestroyed)
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
            bool facingLeft = samus.PoseMetadata.ReadPoseXDirection() == 4;
            samus.Pose = facingLeft
                ? SamusPoseId.FacingLeftNormalPose
                : SamusPoseId.FacingRightNormalPose;
            samus.PoseMetadata.RefreshCollisionRadii();
            samus.Presentation.InitializeAnimation();
            samus.Control.LockHandlers();
            DepartureRequested = true;
            DepartureRequestedThisFrame = true;
        }

        // DEC followed by BMI is deliberately modeled as a wrapping 16-bit decrement.
        // Zero remains visible for one complete call; only the following call makes it
        // $FFFF and consumes the next record. This differs from a conventional <= 0 timer.
        NativeWordCounterStep timer = RotationTimer.Decrement();
        RotationTimer = timer.Remaining;
        if (timer.IsNonNegative)
            return Snapshot(matrixChanged: false);

        // Native computes `(uint16)(6 * index) >> 1`, not a conventional array index.
        // The encoded phase $8044 relies on the multiplication wrapping before the shift,
        // mapping back into the tail of this very table during the reverse sweep.
        ushort rotationIndex = checked((ushort)scratch.Var1);
        var record = CeresShaftRotationDefinitions.Read(rotationIndex);
        RotationTimer = NativeCountdown.FromRaw(record.Timer);
        ushort sine = record.Sine;
        ushort cosine = record.Cosine;
        Transform = new SamusMode7Transform(
            MatrixA: unchecked((short)cosine),
            MatrixB: unchecked((short)sine),
            MatrixC: unchecked((short)-sine),
            Center: CeresShaftMode7Center);

        // Values 0..67 sweep forward. The comparison occurs after INC, so 67 becomes the
        // reversing phase 68. Reversing phases decrement until reversing phase 1 maps to
        // zero, restarting the forward sweep without ever exposing the bare reversing flag.
        if (unchecked((short)rotationIndex) < 0)
        {
            scratch.Var1 = rotationIndex == (CeresShaftRotationPhaseFlags.Reversing | 1)
                ? (ushort)0
                : unchecked((ushort)(rotationIndex - 1));
        }
        else
        {
            scratch.Var1 = rotationIndex == 67
                ? (ushort)(CeresShaftRotationPhaseFlags.Reversing | 68)
                : unchecked((ushort)(rotationIndex + 1));
        }

        return Snapshot(matrixChanged: true);
    }

    /// <summary>
    /// Applies $89:ACD0's asymmetric elevator bounds: X in (112,144], Y in [75,128),
    /// with both whole and fractional vertical speed zero.
    /// </summary>
    private static bool IsInsideDepartureTrigger(SamusState samus) =>
        samus.Position.X.Whole > 112 &&
        samus.Position.X.Whole <= 144 &&
        samus.Position.Y.Whole >= 75 &&
        samus.Position.Y.Whole < 128 &&
        samus.Kinematics.YSpeed.Whole == 0 &&
        samus.Kinematics.YSpeed.Fraction == 0;

    /// <summary>Publishes the current Mode 7 matrix and this call's departure edge to the dispatcher.</summary>
    private CeresElevatorShaftRoomMainResult Snapshot(bool matrixChanged) => new(
        Transform,
        matrixChanged,
        DepartureRequestedThisFrame);

    /// <summary>Starts the shaft at the door ASM's identity matrix and fixed rotation center.</summary>
    private static SamusMode7Transform CreateInitialTransform() => new(
        MatrixA: (short)SnesEightEight.FromWhole(1),
        MatrixB: 0,
        MatrixC: 0,
        Center: CeresShaftMode7Center);

    /// <summary>The shaft's fixed M7X/M7Y rotation centre, written by both <c>$8F:E4E0</c> and room main.</summary>
    internal static readonly Position16<RoomPixels> CeresShaftMode7Center = new(128, 1008);
}
