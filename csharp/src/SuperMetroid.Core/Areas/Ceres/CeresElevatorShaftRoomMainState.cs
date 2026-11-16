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

    /// <summary>Whether the active room owns this room-main behaviour.</summary>
    public bool IsActive { get; private set; }

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
    /// Builds one room's shaft owner, selecting whether the room runs the shaft's main behaviour. An active entry is the elevator
    /// door, whose setup starts the turn place and timer.
    /// </summary>
    /// <param name="active">Whether the room's main behaviour is the shaft and its entry door uses the turning floor.</param>
    /// <param name="carryover">The room-main value the following room inherits, which receives the starting turn.</param>
    public CeresElevatorShaftRoomMainState(bool active, RoomMainCarryover carryover)
    {
        Ensure.NotNull(carryover);
        IsActive = active;
        if (active)
        {
            TiltSteps = 0;
            LeaveTurnStep(carryover);
            HoldFramesLeft = LevelHoldFrames;
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
    /// <param name="carryover">The room-main value the following room inherits, which receives every turn.</param>
    [IndependentFlag("independent room facts read from the room's data: the elevator door's state and whether departure is allowed")]
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
        // turn.
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
        Transform = new TurningFloorTransform(rotation.Tilt, CeresShaftRotationCenter);

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


    /// <summary>
    /// Leaves the shaft's turn for the following room's main: its tilt counted from the full anticlockwise tilt, and its
    /// sweep.
    /// </summary>
    private void LeaveTurnStep(RoomMainCarryover carryover) =>
        carryover.LeaveTurnStep(TiltSteps + CeresShaftRotationDefinitions.FullTiltSteps, backward: Sweep == CeresShaftSweep.Backward);

    /// <summary>Starts the shaft level, as the elevator door's setup leaves it, about its fixed rotation center.</summary>
    private static TurningFloorTransform CreateInitialTransform() => TurningFloorTransform.Level(CeresShaftRotationCenter);

    /// <summary>The room point the shaft's turning floor always rotates about, set by both the elevator door's setup and room main.</summary>
    internal static readonly Position16<RoomPixels> CeresShaftRotationCenter = new(128, 1008);
}
