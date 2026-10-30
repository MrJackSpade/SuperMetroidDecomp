using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Cartridge-authored room-main state for Ceres room <c>$DF45</c>'s elevator shaft.
/// </summary>
/// <remarks>
/// Door ASM <c>$8F:E4E0</c> seeds the shaft's turn step and timer before the shaft becomes visible.
/// Room main <c>$89:ACC3</c> later sweeps the step forward and back through the records at <c>$89:AD5F</c>. The
/// step it last reached is what the following room's main inherits (<see cref="RoomMainCarryover"/>).
/// </remarks>
[RoomLifetime]
public sealed class CeresElevatorShaftRoomMainState
{
    /// <summary>The turn step door ASM <c>$8F:E4E0</c> starts the shaft at, sweeping forward.</summary>
    public const int InitialRotationIndex = 34;

    /// <summary>The last step of the forward sweep; the turn reverses after it.</summary>
    private const int LastForwardStep = 67;

    /// <summary>The step the backward sweep starts at.</summary>
    private const int FirstBackwardStep = 68;

    /// <summary>Initial turn timer written by door ASM <c>$8F:E4E0</c>.</summary>
    public const ushort InitialRotationTimer = 60;

    /// <summary>Native state-$20 hold written by <c>SamusCode_02_ReachCeresElevator</c>.</summary>
    public const ushort DepartureHoldFrames = 60;

    /// <summary>Whether the active room owns this room-main routine.</summary>
    public bool IsActive { get; private set; }

    /// <summary>The countdown to the shaft's next turn step.</summary>
    public WrappingCountdown RotationTimer { get; private set; }

    /// <summary>The shaft's turn step: its rotation record, 0 through 68.</summary>
    public int RotationStep { get; private set; }

    /// <summary>Whether the turn is sweeping backward, from step 68 down to 1, rather than forward from 0 to 67.</summary>
    public bool RotatingBackward { get; private set; }

    /// <summary>True after the elevator bounds have admitted Samus once.</summary>
    public bool DepartureRequested { get; private set; }

    /// <summary>One-frame publication consumed by the outer game-state dispatcher.</summary>
    public bool DepartureRequestedThisFrame { get; private set; }

    /// <summary>Most recently published Mode 7 matrix.</summary>
    public TurningFloorTransform Transform { get; private set; } = CreateInitialTransform();

    /// <summary>
    /// Builds one room's shaft owner, selecting whether the room owns <c>$89:ACC3</c>. An active entry is the elevator
    /// door, whose ASM <c>$8F:E4E0</c> writes the rotation index and timer.
    /// </summary>
    /// <param name="active">Whether the room's main routine is the shaft and its entry door uses Mode 7.</param>
    /// <param name="carryover">The room-main value the following room inherits, which receives the initial turn step.</param>
    public CeresElevatorShaftRoomMainState(bool active, RoomMainCarryover carryover)
    {
        Ensure.NotNull(carryover);
        IsActive = active;
        if (active)
        {
            RotationStep = InitialRotationIndex;
            carryover.LeaveTurnStep(RotationStep, RotatingBackward);
            RotationTimer = WrappingCountdown.Start(InitialRotationTimer);
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
    /// <param name="carryover">The room-main value the following room inherits, which receives every turn step.</param>
    [IndependentFlag("independent room facts the caller reads from data: the elevator door's state and whether departure is allowed")]
    public CeresElevatorShaftRoomMainResult Step(
        SamusState? samus,
        bool elevatorDoorDestroyed,
        bool allowDeparture,
        RoomMainCarryover carryover)
    {
        Ensure.NotNull(carryover);
        DepartureRequestedThisFrame = false;

        // The room code is entirely dormant until the Ceres door's destruction publishes
        // its escape status. In particular, the initial elevator descent retains door ASM's identity
        // matrix instead of prematurely consuming the rotation table.
        if (!IsActive || !elevatorDoorDestroyed)
            return new(Transform, MatrixChanged: false, DepartureRequestedThisFrame);

        if (allowDeparture &&
            !DepartureRequested &&
            samus is not null &&
            IsInsideDepartureTrigger(samus))
        {
            // `SamusCode_02_ReachCeresElevator` chooses pose $01/$02 from the current
            // cartridge x-direction metadata, refreshes pose radii/animation, seeds the
            // 60-frame state-$20 hold, then installs SamusCode_00's input lock. The hold is
            // owned by the frontend dispatcher; everything Samus-visible remains here.
            bool facingLeft = SamusPoseMetadata.ReadPoseXDirection(samus) == 4;
            samus.Pose = facingLeft
                ? SamusPoseId.FacingLeftNormalPose
                : SamusPoseId.FacingRightNormalPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            samus.Control.LockHandlers();
            DepartureRequested = true;
            DepartureRequestedThisFrame = true;
        }

        // DEC followed by BMI is deliberately modeled as a wrapping 16-bit decrement.
        // Zero remains visible for one complete call; only the following call makes it
        // $FFFF and consumes the next record. This differs from a conventional <= 0 timer.
        CountdownStep timer = RotationTimer.Decrement();
        RotationTimer = timer.Remaining;
        if (timer.IsNonNegative)
            return new(Transform, MatrixChanged: false, DepartureRequestedThisFrame);

        // Both sweeps read the record of the current step.
        var record = CeresShaftRotation.Read(RotationStep);
        RotationTimer = WrappingCountdown.FromRaw(record.Timer);
        int sine = record.Sine;
        int cosine = record.Cosine;
        Transform = new TurningFloorTransform(
            MatrixA: unchecked((short)cosine),
            MatrixB: unchecked((short)sine),
            MatrixC: unchecked((short)-sine),
            Center: CeresShaftRotationCenter);

        // Steps 0..67 sweep forward; after 67 the turn reverses at step 68. The backward sweep counts down until
        // step 1 hands over to forward step 0, so the backward sweep never reaches step 0 itself.
        if (RotatingBackward)
        {
            if (RotationStep == 1)
                RotatingBackward = false;
            RotationStep--;
        }
        else if (RotationStep == LastForwardStep)
        {
            RotatingBackward = true;
            RotationStep = FirstBackwardStep;
        }
        else
        {
            RotationStep++;
        }
        carryover.LeaveTurnStep(RotationStep, RotatingBackward);

        return new(Transform, MatrixChanged: true, DepartureRequestedThisFrame);
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


    /// <summary>Starts the shaft at the door ASM's identity matrix and fixed rotation center.</summary>
    private static TurningFloorTransform CreateInitialTransform() => new(
        MatrixA: (short)EightEight.FromWhole(1),
        MatrixB: 0,
        MatrixC: 0,
        Center: CeresShaftRotationCenter);

    /// <summary>The room point the shaft's turning floor always rotates about, set by both <c>$8F:E4E0</c> and room main.</summary>
    internal static readonly Position16<RoomPixels> CeresShaftRotationCenter = new(128, 1008);
}
