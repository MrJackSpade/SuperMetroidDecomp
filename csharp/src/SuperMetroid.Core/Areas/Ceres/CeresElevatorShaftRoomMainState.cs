using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Authored room-main state for the Ceres elevator shaft.
/// </summary>
/// <remarks>
/// The elevator door's setup seeds the shaft's turn step and timer before the shaft becomes visible.
/// The room's main behaviour later sweeps the step forward and back through its rotations. The
/// step it last reached is what the following room's main inherits (<see cref="RoomMainCarryover"/>).
/// </remarks>
[RoomLifetime]
public sealed class CeresElevatorShaftRoomMainState
{
    /// <summary>The turn step the elevator door's setup starts the shaft at, sweeping forward.</summary>
    public const int InitialRotationIndex = 34;

    /// <summary>The last step of the forward sweep; the turn reverses after it.</summary>
    private const int LastForwardStep = 67;

    /// <summary>The step the backward sweep starts at.</summary>
    private const int FirstBackwardStep = 68;

    /// <summary>Initial turn timer written by the elevator door's setup.</summary>
    public const int InitialRotationTimer = 60;

    /// <summary>The elevator hold reaching the Ceres elevator writes.</summary>
    public const int DepartureHoldFrames = 60;

    /// <summary>Whether the active room owns this room-main behaviour.</summary>
    public bool IsActive { get; private set; }

    /// <summary>The countdown to the shaft's next turn step.</summary>
    public int RotationTimer { get; private set; }

    /// <summary>The shaft's turn step: its rotation, 0 through 68.</summary>
    public int RotationStep { get; private set; }

    /// <summary>Which way the turn is sweeping: forward from step 0 to 67, or backward from step 68 down to 1.</summary>
    public CeresShaftSweep Sweep { get; private set; }

    /// <summary>True after the elevator bounds have admitted Samus once.</summary>
    public bool DepartureRequested { get; private set; }

    /// <summary>One-frame publication consumed by the outer game-state loop.</summary>
    public bool DepartureRequestedThisFrame { get; private set; }

    /// <summary>The shaft's most recently published tilt.</summary>
    public TurningFloorTransform Transform { get; private set; } = CreateInitialTransform();

    /// <summary>
    /// Builds one room's shaft owner, selecting whether the room runs the shaft's main behaviour. An active entry is the elevator
    /// door, whose setup writes the rotation index and timer.
    /// </summary>
    /// <param name="active">Whether the room's main behaviour is the shaft and its entry door uses the turning floor.</param>
    /// <param name="carryover">The room-main value the following room inherits, which receives the initial turn step.</param>
    public CeresElevatorShaftRoomMainState(bool active, RoomMainCarryover carryover)
    {
        Ensure.NotNull(carryover);
        IsActive = active;
        if (active)
        {
            RotationStep = InitialRotationIndex;
            LeaveTurnStep(carryover);
            RotationTimer = InitialRotationTimer;
        }
    }

    /// <summary>
    /// The door setup leaving the elevator's turning-floor rendering: the destination's shaft no longer owns the room.
    /// </summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Runs the shaft's room main behaviour once.</summary>
    /// <param name="samus">Live Samus state inspected by the elevator trigger.</param>
    /// <param name="elevatorDoorDestroyed">Whether the Ceres elevator door's destruction has released the rotation.</param>
    /// <param name="allowDeparture">
    /// True only in main gameplay. The elevator-arrival and blackout states still call the
    /// ordinary gameplay body, but the room main explicitly rejects them; keeping that input
    /// explicit preserves the same ownership boundary.
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
        // its escape status. In particular, the initial elevator descent stays level instead of prematurely consuming the
        // rotation steps.
        if (!IsActive || !elevatorDoorDestroyed)
            return new(Transform, TiltChanged: false, DepartureRequestedThisFrame);

        if (allowDeparture &&
            !DepartureRequested &&
            samus is not null &&
            IsInsideDepartureTrigger(samus))
        {
            // Reaching the Ceres elevator chooses the facing-left or facing-right elevator pose from the current
            // facing, refreshes pose radii/animation, seeds the
            // 60-frame elevator hold, then locks Samus's input. The hold is
            // owned by the frontend game-state loop; everything Samus-visible remains here.
            bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
            samus.Pose = facingLeft
                ? SamusPoseId.FacingLeftNormalPose
                : SamusPoseId.FacingRightNormalPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            samus.Control.LockHandlers();
            DepartureRequested = true;
            DepartureRequestedThisFrame = true;
        }

        // The turn steps when the timer runs past zero: zero remains visible for one complete call; only the following
        // call makes it -1 and turns to the next rotation. This differs from a conventional <= 0 timer.
        if (--RotationTimer >= 0)
            return new(Transform, TiltChanged: false, DepartureRequestedThisFrame);

        // Both sweeps read the rotation of the current step.
        var rotation = CeresShaftRotation.Read(RotationStep);
        RotationTimer = rotation.Timer;
        Transform = new TurningFloorTransform(rotation.Tilt, CeresShaftRotationCenter);

        // Steps 0..67 sweep forward; after 67 the turn reverses at step 68. The backward sweep counts down until
        // step 1 hands over to forward step 0, so the backward sweep never reaches step 0 itself.
        switch (Sweep)
        {
            case CeresShaftSweep.Backward:
                if (RotationStep == 1)
                    Sweep = CeresShaftSweep.Forward;
                RotationStep--;
                break;
            case CeresShaftSweep.Forward when RotationStep == LastForwardStep:
                Sweep = CeresShaftSweep.Backward;
                RotationStep = FirstBackwardStep;
                break;
            case CeresShaftSweep.Forward:
                RotationStep++;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresShaftSweep)} {Sweep}.");
        }
        LeaveTurnStep(carryover);

        return new(Transform, TiltChanged: true, DepartureRequestedThisFrame);
    }

    /// <summary>
    /// Applies the elevator trigger's asymmetric bounds: X in (112,144], Y in [75,128),
    /// with both whole and fractional vertical speed zero.
    /// </summary>
    private static bool IsInsideDepartureTrigger(SamusState samus) =>
        samus.Position.X.Whole > 112 &&
        samus.Position.X.Whole <= 144 &&
        samus.Position.Y.Whole >= 75 &&
        samus.Position.Y.Whole < 128 &&
        samus.Kinematics.YSpeed.Whole == 0 &&
        samus.Kinematics.YSpeed.Fraction == 0;


    /// <summary>Leaves the shaft's turn step and sweep for the following room's main.</summary>
    private void LeaveTurnStep(RoomMainCarryover carryover) =>
        carryover.LeaveTurnStep(RotationStep, backward: Sweep == CeresShaftSweep.Backward);

    /// <summary>Starts the shaft level, as the elevator door's setup leaves it, about its fixed rotation center.</summary>
    private static TurningFloorTransform CreateInitialTransform() => TurningFloorTransform.Level(CeresShaftRotationCenter);

    /// <summary>The room point the shaft's turning floor always rotates about, set by both the elevator door's setup and room main.</summary>
    internal static readonly Position16<RoomPixels> CeresShaftRotationCenter = new(128, 1008);
}
