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
    private static readonly CeresShaftTilt LastForwardTilt = CeresShaftTilt.FullClockwise.OneStepAnticlockwise;

    /// <summary>The last tilt of the backward sweep, one step short of the full anticlockwise tilt.</summary>
    private static readonly CeresShaftTilt LastBackwardTilt = CeresShaftTilt.FullAnticlockwise.OneStepClockwise;

    /// <summary>The further updates the shaft holds its tilt; the update after the last of them turns it.</summary>
    public int HoldFramesLeft { get; private set; }

    /// <summary>
    /// The tilt the shaft turns to next (#1895): from the full anticlockwise tilt through the full clockwise one. It selects
    /// how long the shaft holds the tilt.
    /// </summary>
    public CeresShaftTilt Tilt { get; private set; }

    /// <summary>
    /// Which way the turn is sweeping: forward, clockwise, from the full anticlockwise tilt to one step short of the full
    /// clockwise one; backward from the full clockwise tilt to one step short of the full anticlockwise one.
    /// </summary>
    public CeresShaftSweep Sweep { get; private set; }

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
        Tilt = CeresShaftTilt.Level;
        LeaveTurnStep(carryover);
        HoldFramesLeft = CeresShaftRotationDefinitions.LevelHoldFrames;
    }

    /// <summary>Runs the shaft's room main behaviour once.</summary>
    /// <param name="samus">Samus, whom the elevator trigger inspects.</param>
    /// <param name="elevatorDoorDestroyed">Whether the Ceres elevator door's destruction has released the rotation.</param>
    /// <param name="allowDeparture">
    /// Whether Samus may leave by the elevator: only while the game is still in main gameplay where room main runs (#2534),
    /// not during the Ceres departure, the time-up's white-out or any other game state, when the shaft still turns but no
    /// departure starts. The update that takes her moves the game to the departure, so no later update allows it again.
    /// </param>
    /// <param name="carryover">The room-main value the following room inherits, which receives every turn.</param>
    /// <returns>The update's turn, and whether the elevator took Samus, which the gameplay update acts on (#2545).</returns>
    [IndependentFlag("independent room facts read from the room's data: the elevator door's state and whether departure is allowed")]
    public CeresElevatorShaftRoomMainResult Step(
        SamusState samus,
        bool elevatorDoorDestroyed,
        bool allowDeparture,
        RoomMainCarryover carryover)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(carryover);

        // The room code is entirely dormant until the Ceres door's destruction publishes
        // its escape status. In particular, the initial elevator descent stays level instead of prematurely consuming the
        // turn.
        if (!elevatorDoorDestroyed)
            return new(Transform, TiltChanged: false, ElevatorTookSamus: false);

        bool elevatorTookSamus = allowDeparture && IsInsideDepartureTrigger(samus);
        if (elevatorTookSamus)
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
        }

        if (HoldFramesLeft > 0)
        {
            HoldFramesLeft--;
            return new(Transform, TiltChanged: false, elevatorTookSamus);
        }

        // Both sweeps show the current tilt.
        // The update that turns the shaft to a tilt is the first of the updates it holds it.
        HoldFramesLeft = Tilt.HoldUpdates - 1;
        Transform = new TurningFloorTransform(Tilt.Radians, CeresShaftRotationDefinitions.RotationCenter);

        // The forward sweep turns clockwise one step at a time; after its last tilt the turn reverses at the full
        // clockwise tilt. The backward sweep turns back until its last tilt hands over to the forward sweep at the full
        // anticlockwise tilt, which the backward sweep itself never shows.
        switch (Sweep)
        {
            case CeresShaftSweep.Backward:
                if (Tilt == LastBackwardTilt)
                    Sweep = CeresShaftSweep.Forward;
                Tilt = Tilt.OneStepAnticlockwise;
                break;
            case CeresShaftSweep.Forward when Tilt == LastForwardTilt:
                Sweep = CeresShaftSweep.Backward;
                Tilt = CeresShaftTilt.FullClockwise;
                break;
            case CeresShaftSweep.Forward:
                Tilt = Tilt.OneStepClockwise;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresShaftSweep)} {Sweep}.");
        }
        LeaveTurnStep(carryover);

        return new(Transform, TiltChanged: true, elevatorTookSamus);
    }

    /// <summary>
    /// Applies the elevator trigger's asymmetric bounds (<see cref="CeresElevatorDepartureDefinitions"/>), with Samus
    /// moving neither up nor down.
    /// </summary>
    private static bool IsInsideDepartureTrigger(SamusState samus) =>
        samus.Position.X.Whole > CeresElevatorDepartureDefinitions.LeftOfTriggerX &&
        samus.Position.X.Whole <= CeresElevatorDepartureDefinitions.TriggerRightX &&
        samus.Position.Y.Whole >= CeresElevatorDepartureDefinitions.TriggerTopY &&
        samus.Position.Y.Whole < CeresElevatorDepartureDefinitions.BelowTriggerY &&
        samus.Kinematics.YSpeed.IsZero;


    /// <summary>
    /// Leaves the shaft's turn for the following room's main: its tilt counted from the full anticlockwise tilt, and its
    /// sweep.
    /// </summary>
    private void LeaveTurnStep(RoomMainCarryover carryover) =>
        carryover.LeaveTurnStep(Tilt.StepsFromFullAnticlockwise, backward: Sweep == CeresShaftSweep.Backward);

    /// <summary>Starts the shaft level, as the elevator door's setup leaves it, about its fixed rotation center.</summary>
    private static TurningFloorTransform CreateInitialTransform() => TurningFloorTransform.Level(CeresShaftRotationDefinitions.RotationCenter);
}
