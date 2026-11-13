using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// Exact Samus-owned half of the grabbed-by-Draygon route and
/// the Draygon-held movement type.
/// </summary>
/// <remarks>
/// Draygon is an enemy actor, so its AI computes the boss's flight position and then
/// calls <c>MoveSamusWithDraygon</c>. This object deliberately accepts that actor position
/// as an explicit input. It does not fabricate an enemy trajectory inside Samus physics.
/// Everything after that boundary—the signed claw offset, pose family, escape counter,
/// release pose, velocity cleanup, and movement-type side effect—is modelled here.
/// </remarks>
public sealed class SamusDraygonGrabbedState
{
    /// <summary>Escape threshold.</summary>
    public const int EscapeButtonCounterTarget = 60;

    /// <summary>True while the escape handler is installed as Samus's timer/hack handler.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Another callback replaced the empty movement handler without removing grab gamma.</summary>
    public bool MovementHandlerReplaced { get; private set; }

    /// <summary>True only while the grab's empty handler still owns beta movement.</summary>
    public bool OwnsMovement => IsActive && !MovementHandlerReplaced;

    /// <summary>Preserves grab gamma while a newer movement callback replaces the empty handler.</summary>
    internal void RelinquishMovementHandler() => MovementHandlerReplaced = true;

    /// <summary>The count of escape presses, which Crystal Flash's ammunition drain and the suit pickup's stage share.</summary>
    private readonly SharedEscapeAndDrainCount _escapePresses;

    /// <summary>Creates a Draygon-held owner whose escape presses count on Samus's shared escape and drain count.</summary>
    /// <param name="escapePresses">Samus's shared escape and drain count.</param>
    internal SamusDraygonGrabbedState(SharedEscapeAndDrainCount escapePresses) => _escapePresses = Ensure.NotNull(escapePresses);

    /// <summary>
    /// D-pad presses counted toward escaping, 0 through 65535, on the count Crystal Flash's ammunition drain and the suit
    /// pickup's stage share (<see cref="SharedEscapeAndDrainCount"/>), so it holds any value those leave, including a
    /// wrapped countdown. A press continues from zero past 65535, and the release test is the game's signed comparison
    /// against the target.
    /// </summary>
    public int EscapeButtonCounter => _escapePresses.EscapePresses;

    /// <summary>
    /// Whether a counted press reached the target. The game compares the shared count with a signed subtraction,
    /// so a count of 32828 or more, left by another owner, reads as below it.
    /// </summary>
    private static bool ReachedEscapeTarget(int counter) =>
        CoordinateAxis.Difference(counter, EscapeButtonCounterTarget) >= 0;

    /// <summary>
    /// The D-pad input of Draygon's escape. The game stores the entire masked D-pad
    /// directions, not merely one, so a different diagonal chord is also a new input.
    /// </summary>
    public ControllerButton PreviousDpadInput { get; private set; }

    /// <summary>
    /// Set by the release, the signal Draygon reads when Samus breaks free. Draygon's
    /// next <c>MoveSamusWithDraygon</c> call consumes the signal and starts flying upward.
    /// </summary>
    public bool ReleasePublishedToOwner { get; private set; }

    /// <summary>
    /// Sets Samus into the grabbed-by-Draygon pose.
    /// </summary>
    public void Begin(SamusState samus, bool draygonFacingRight)
    {
        Ensure.NotNull(samus);

        samus.Pose = draygonFacingRight
            ? SamusPoseId.DraygonGrabbedNeutralRightPose
            : SamusPoseId.DraygonGrabbedNeutralLeftPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);

        IsActive = true;
        MovementHandlerReplaced = false;
        _escapePresses.ClearEscapePresses();
        // Grab installs an empty movement handler, but leaves the Flash palette and its shared
        // shine timer alive. A later release must not resume Flash draining.
        samus.CrystalFlash.RelinquishMovementHandler();
        PreviousDpadInput = 0;
        ReleasePublishedToOwner = false;
        // Forced grab owns its history shift; no ordinary input slot is consumed.
        samus.CommitPoseHistory();
        // Empties every prospective-pose slot. The super-special one is Samus's;
        // the runtime clears the others after the enemy update (EnemyRoomStatus publishes the grab).
        samus.PendingTransitionalPose = null;
    }

    /// <summary>
    /// Runs <c>MoveSamusWithDraygon</c> after enemy AI has updated the
    /// boss body. Facing left places Samus eight pixels left; facing right places her eight
    /// pixels right; both place her 40 pixels below the body center. Draygon also places a
    /// Samus who has just escaped, until it consumes the escape flag right after this placement.
    /// </summary>
    public DraygonOwnerPlacement ApplyOwnerPosition(
        SamusState samus,
        Position16<RoomPixels> ownerPosition,
        bool draygonFacingRight)
    {
        Ensure.NotNull(samus);
        if (!IsActive && !ReleasePublishedToOwner)
        {
            throw new InvalidOperationException(
                "Draygon places only a held Samus or one whose escape it has not yet seen.");
        }

        samus.Position = samus.Position.WithWhole(ownerPosition + new Offset16<RoomPixels>(draygonFacingRight ? 8 : -8, 40));

        // The placement writes whole positions only. It does not clear subpositions,
        // so preserving the subpositions is intentional even though the actor overwrites
        // the visible whole coordinates on every owner update.
        return new DraygonOwnerPlacement();
    }

    /// <summary>
    /// Runs <see cref="SamusMovementType.DraygonHeld"/> movement for every pose of that movement type: the grabbed poses,
    /// whose position enemy AI has already supplied through <see cref="ApplyOwnerPosition"/>, and the Draygon-held ball the
    /// Baby's Hyper Beam freeze can install without Draygon (#1535), which therefore stays where it is. Its entire normal
    /// beta handler clears one result.
    /// </summary>
    public static void StepMovement(SamusState samus)
    {
        Ensure.NotNull(samus);
        EnsureDraygonHeldMovement(samus);

        samus.SolidVerticalCollisionResult = SamusVerticalCollisionResult.None;
    }

    /// <summary>
    /// Runs the escape handler: optionally suppress the already-selected input transition while a
    /// grapple is locked in place, then count a newly pressed D-pad pattern only when it is
    /// nonzero and different from the last counted pattern.
    /// </summary>
    public DraygonEscapeResult StepEscapeHandler(
        SamusState samus,
        ControllerButton newlyPressedInput,
        bool grappleLockedInPlace)
    {
        Ensure.NotNull(samus);
        EnsureActiveGrabbedPose();

        ControllerButton newlyPressed = newlyPressedInput;
        ControllerButton dpad = newlyPressed & ControllerButtons.DirectionalPad;
        bool counted = false;
        if (dpad != ControllerButton.None && dpad != PreviousDpadInput)
        {
            PreviousDpadInput = dpad;
            _escapePresses.CountEscapePress();
            counted = true;
        }

        // The game tests the signed difference only on a newly counted D-pad edge.
        bool released = counted && ReachedEscapeTarget(EscapeButtonCounter);
        if (released)
            Release(samus);

        return new DraygonEscapeResult(
            grappleLockedInPlace,
            released);
    }

    /// <summary>
    /// Runs the release. The release keeps extra run speed exactly as the game does,
    /// but clears base X speed, all vertical velocity, bounce state, and acceleration mode.
    /// </summary>
    public void Release(SamusState samus)
    {
        Ensure.NotNull(samus);

        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        samus.Pose = facingLeft
            ? SamusPoseId.FacingLeftNormalPose
            : SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        if (samus.Pose != samus.PoseHistory.PreviousPose)
            SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        // Death calls this even when Samus was never grabbed. Installing normal
        // movement interrupts a spark without resetting its independent boost/palette.
        samus.Shinespark.RelinquishMovementHandler();
        samus.CrystalFlash.RelinquishMovementHandler();

        // Release clears pending input transitions, so commit here rather than
        // relying on the normal pose dispatcher to observe the standing pose.
        samus.CommitPoseHistory();

        samus.HorizontalSpeed.BaseSpeed = default;
        samus.Kinematics.YSpeed = default;
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;

        IsActive = false;
        ReleasePublishedToOwner = true;
    }

    /// <summary>Consumes the Draygon-facing release bit exactly once.</summary>
    public bool ConsumeOwnerReleaseSignal()
    {
        bool published = ReleasePublishedToOwner;
        ReleasePublishedToOwner = false;
        return published;
    }

    /// <summary>Rejects escape-handler calls before Draygon installs Samus's grab state.</summary>
    private void EnsureActiveGrabbedPose()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException(
                "Draygon owner/hack handling requires SetSamusIntoTheGrabbedByDraygonPose first.");
        }

        // Gamma and the enemy's placement callback are independent of the pose
        // and movement handler. Crystal Flash can replace both while still held.
    }

    /// <summary>Requires a pose of the Draygon-held movement type before running Draygon-held movement.</summary>
    private static void EnsureDraygonHeldMovement(SamusState samus)
    {
        if (SamusPoseMetadata.ReadMovementType(samus.Pose) != SamusMovementType.DraygonHeld)
        {
            throw new InvalidOperationException(
                $"Draygon-held movement requires a pose of the Draygon-held movement type, not {samus.Pose}.");
        }
    }
}
