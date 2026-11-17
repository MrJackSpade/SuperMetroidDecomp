using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Authored room-main state for the Ceres elevator shaft.
/// </summary>
/// <remarks>
/// The elevator door's setup starts the shaft level, with its hold before the first turn, before the shaft becomes visible. The room's
/// main behaviour later sweeps the tilt clockwise and back. The turn it last reached is what the following room's main
/// inherits (<see cref="RoomMainCarryover"/>).
/// </remarks>
[RoomLifetime]
public sealed class CeresElevatorShaftRoomMainState
{
    /// <summary>The last tilt of the forward sweep, one step short of the full clockwise tilt; the turn reverses after it.</summary>
    private const int LastForwardTilt = CeresShaftRotationDefinitions.FullTiltSteps - 1;

    /// <summary>The last tilt of the backward sweep, one step short of the full anticlockwise tilt.</summary>
    private const int LastBackwardTilt = 1 - CeresShaftRotationDefinitions.FullTiltSteps;

    /// <summary>The updates the shaft holds level, once the elevator door's destruction releases it, before its first turn.</summary>
    public const int LevelHoldFrames = 60;

    /// <summary>The elevator hold reaching the Ceres elevator writes.</summary>
    public const int DepartureHoldFrames = 60;

    /// <summary>The further updates the shaft holds its tilt; the update after the last of them turns it.</summary>
    public int HoldFramesLeft { get; private set; }

    /// <summary>
    /// The shaft's tilt in steps from level, each a 256th of a radian, clockwise positive (#1895): from the full
    /// anticlockwise tilt through the full clockwise one. It selects how long the shaft holds the tilt.
    /// </summary>
    public int TiltSteps { get; private set; }

    /// <summary>
    /// Which way the turn is sweeping: forward, clockwise, from the full anticlockwise tilt to one step short of the full
    /// clockwise one; backward from the full clockwise tilt to one step short of the full anticlockwise one.
    /// </summary>
    public CeresShaftSweep Sweep { get; private set; }

    /// <summary>True after the elevator bounds have admitted Samus once.</summary>
    public bool DepartureRequested { get; private set; }

    /// <summary>One-frame publication consumed by the outer game-state loop.</summary>
    public bool DepartureRequestedThisFrame { get; private set; }

    /// <summary>The shaft's most recently published tilt.</summary>
    public TurningFloorTransform Transform { get; private set; } = CreateInitialTransform();

    /// <summary>
    /// Builds the shaft's turn for the shaft entered through its elevator door, whose setup starts the turn place and timer
    /// (#2287); the shaft entered otherwise never turns, and has no turn.
    /// </summary>
    /// <param name="carryover">The room-main value the following room inherits, which receives the starting turn.</param>
    public CeresElevatorShaftRoomMainState(RoomMainCarryover carryover)
    {
        Ensure.NotNull(carryover);
        TiltSteps = 0;
        LeaveTurnStep(carryover);
        HoldFramesLeft = LevelHoldFrames;
    }

    /// <summary>Runs the shaft's room main behaviour once.</summary>
    /// <param name="samus">Samus, whom the elevator trigger inspects.</param>
    /// <param name="elevatorDoorDestroyed">Whether the Ceres elevator door's destruction has released the rotation.</param>
    /// <param name="allowDeparture">
    /// Whether Samus may leave by the elevator: only while the game is played, not during the elevator's arrival or the
    /// blackout, when the shaft still turns but no departure starts.
    /// </param>
    /// <param name="carryover">The room-main value the following room inherits, which receives every turn.</param>
    [IndependentFlag("independent room facts read from the room's data: the elevator door's state and whether departure is allowed")]
    public CeresElevatorShaftRoomMainResult Step(
        SamusState samus,
        bool elevatorDoorDestroyed,
        bool allowDeparture,
        RoomMainCarryover carryover)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(carryover);
        DepartureRequestedThisFrame = false;

        // The room code is entirely dormant until the Ceres door's destruction publishes
        // its escape status. In particular, the initial elevator descent stays level instead of prematurely consuming the
        // turn.
        if (!elevatorDoorDestroyed)
            return new(Transform, TiltChanged: false, DepartureRequestedThisFrame);

        if (allowDeparture &&
            !DepartureRequested &&
            IsInsideDepartureTrigger(samus))
        {
            // Reaching the Ceres elevator turns Samus to the standing pose for the way she faces, with its size and
            // animation, and locks out the player; the wait before Samus leaves Ceres is the game state's, which reads
            // the departure this requests.
            bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
            samus.Pose = facingLeft
                ? SamusPoseId.FacingLeftNormalPose
                : SamusPoseId.FacingRightNormalPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            samus.Control.LockOutPlayer();
            DepartureRequested = true;
            DepartureRequestedThisFrame = true;
        }

        if (HoldFramesLeft > 0)
        {
            HoldFramesLeft--;
            return new(Transform, TiltChanged: false, DepartureRequestedThisFrame);
        }

        // Both sweeps show the current tilt.
        // The update that turns the shaft to a tilt is the first of the updates it holds it.
        var rotation = CeresShaftRotation.Read(TiltSteps);
        HoldFramesLeft = rotation.HoldFrames - 1;
        Transform = new TurningFloorTransform(rotation.Tilt, CeresShaftRotationDefinitions.RotationCenter);

        // The forward sweep turns clockwise one step at a time; after its last tilt the turn reverses at the full
        // clockwise tilt. The backward sweep turns back until its last tilt hands over to the forward sweep at the full
        // anticlockwise tilt, which the backward sweep itself never shows.
        switch (Sweep)
        {
            case CeresShaftSweep.Backward:
                if (TiltSteps == LastBackwardTilt)
                    Sweep = CeresShaftSweep.Forward;
                TiltSteps--;
                break;
            case CeresShaftSweep.Forward when TiltSteps == LastForwardTilt:
                Sweep = CeresShaftSweep.Backward;
                TiltSteps = CeresShaftRotationDefinitions.FullTiltSteps;
                break;
            case CeresShaftSweep.Forward:
                TiltSteps++;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresShaftSweep)} {Sweep}.");
        }
        LeaveTurnStep(carryover);

        return new(Transform, TiltChanged: true, DepartureRequestedThisFrame);
    }

    /// <summary>
    /// Applies the elevator trigger's asymmetric bounds (<see cref="CeresElevatorDepartureDefinitions"/>), with both whole
    /// and fractional vertical speed zero.
    /// </summary>
    private static bool IsInsideDepartureTrigger(SamusState samus) =>
        samus.Position.X.Whole > CeresElevatorDepartureDefinitions.LeftOfTriggerX &&
        samus.Position.X.Whole <= CeresElevatorDepartureDefinitions.TriggerRightX &&
        samus.Position.Y.Whole >= CeresElevatorDepartureDefinitions.TriggerTopY &&
        samus.Position.Y.Whole < CeresElevatorDepartureDefinitions.BelowTriggerY &&
        samus.Kinematics.YSpeed.Whole == 0 &&
        samus.Kinematics.YSpeed.Fraction == 0;


    /// <summary>
    /// Leaves the shaft's turn for the following room's main: its tilt counted from the full anticlockwise tilt, and its
    /// sweep.
    /// </summary>
    private void LeaveTurnStep(RoomMainCarryover carryover) =>
        carryover.LeaveTurnStep(TiltSteps + CeresShaftRotationDefinitions.FullTiltSteps, backward: Sweep == CeresShaftSweep.Backward);

    /// <summary>Starts the shaft level, as the elevator door's setup leaves it, about its fixed rotation center.</summary>
    private static TurningFloorTransform CreateInitialTransform() => TurningFloorTransform.Level(CeresShaftRotationDefinitions.RotationCenter);
}
