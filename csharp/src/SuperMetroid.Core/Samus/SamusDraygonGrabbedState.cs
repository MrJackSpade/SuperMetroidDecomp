using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// Samus's half of Draygon's hold (<see cref="SamusMovementOwner.DraygonHold"/>) and
/// the Draygon-held movement type.
/// </summary>
/// <remarks>
/// Draygon is an enemy actor, so its AI computes the boss's flight position and then
/// carries Samus to it. This object takes that position as an input; Samus's movement
/// computes no flight of Draygon's own.
/// This object places her at the claw's offset and keeps her pose family, her escape presses, her release pose,
/// the speeds her release clears and the movement type's side effect.
/// </remarks>
public sealed class SamusDraygonGrabbedState
{
    /// <summary>Where Samus is in Draygon's hold: free, held, held while another owner moves her, or released unseen.</summary>
    private SamusDraygonHoldPhase Phase { get; set; }

    /// <summary>
    /// True while Draygon holds Samus: from the grab until her release, the escape check runs after each of her
    /// movements and counts her presses of the directions.
    /// </summary>
    public bool IsActive => Phase is SamusDraygonHoldPhase.Held or SamusDraygonHoldPhase.HeldMovementTakenOver;

    /// <summary>True only while the hold still keeps Samus still, before any other owner takes her movement.</summary>
    public bool OwnsMovement => Phase == SamusDraygonHoldPhase.Held;

    /// <summary>
    /// Ends the hold's keeping Samus still when a newer owner takes her movement (<see cref="SamusMovementHandOver"/>),
    /// while the hold and its escape check continue. A hold that does not keep her still has nothing to give up.
    /// </summary>
    internal void RelinquishMovement()
    {
        if (Phase == SamusDraygonHoldPhase.Held)
            Phase = SamusDraygonHoldPhase.HeldMovementTakenOver;
    }

    /// <summary>The count of escape presses, which Crystal Flash's ammunition drain and the suit pickup's stage share.</summary>
    private readonly SharedEscapeAndDrainCount<SamusSuitPickupStage> escapePresses;

    /// <summary>Creates a Draygon-held owner whose escape presses count on Samus's shared escape and drain count.</summary>
    /// <param name="escapePresses">Samus's shared escape and drain count.</param>
    internal SamusDraygonGrabbedState(SharedEscapeAndDrainCount<SamusSuitPickupStage> escapePresses) => this.escapePresses = Ensure.NotNull(escapePresses);

    /// <summary>
    /// The directions newly pressed at the last counted press of Draygon's escape. The whole set of directions counts, not
    /// one of them, so a different diagonal is a new press too.
    /// </summary>
    public GameplayAction PreviousDirectionsPressed { get; private set; }

    /// <summary>
    /// Sets Samus into the grabbed-by-Draygon pose.
    /// </summary>
    public void Begin(SamusState samus, HorizontalDirection draygonFacing)
    {
        Ensure.NotNull(samus);

        samus.Pose = draygonFacing == HorizontalDirection.Right
            ? SamusPoseId.DraygonGrabbedNeutralRightPose
            : SamusPoseId.DraygonGrabbedNeutralLeftPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);

        Phase = SamusDraygonHoldPhase.Held;
        escapePresses.ClearEscapePresses();
        // The grab takes Samus's movement and keeps her still, but leaves the Flash shade and its shared
        // shine timer alive. A later release must not resume Flash draining.
        SamusMovementHandOver.To(samus, SamusMovementOwner.DraygonHold);
        PreviousDirectionsPressed = GameplayAction.None;
        // Forced grab owns its history shift; no ordinary input transition is consumed.
        samus.CommitPoseHistory();
        // The grab clears every pending pose. The pose the animation left pending is Samus's to clear; the runtime clears the
        // others after the enemy update, which publishes the grab.
        samus.CuePose.Forget();
    }

    /// <summary>
    /// Places Samus where Draygon carries her, after enemy AI has updated the
    /// boss body. Facing left places Samus eight pixels left; facing right places her eight
    /// pixels right; both place her 40 pixels below the body center. Draygon also places a
    /// Samus who has just escaped: the carry that places her reports her escape, which he sees once.
    /// </summary>
    /// <returns>Whether she had broken free since his last carry, which this carry has him see.</returns>
    public bool CarryTo(
        SamusState samus,
        Position16<RoomPixels> ownerPosition,
        HorizontalDirection draygonFacing)
    {
        Ensure.NotNull(samus);
        if (Phase == SamusDraygonHoldPhase.Inactive)
        {
            throw new InvalidOperationException(
                "Draygon places only a held Samus or one whose escape it has not yet seen.");
        }

        samus.Position = samus.Position.WithWhole(ownerPosition + new Offset16<RoomPixels>(
            SamusDraygonHoldDefinitions.HeldAhead.Toward(draygonFacing),
            SamusDraygonHoldDefinitions.HeldBelow));

        // The placement writes whole positions only. Where Samus lies within a pixel stays, even though the actor
        // overwrites the visible whole coordinates on every owner update.
        if (Phase != SamusDraygonHoldPhase.ReleasedUnseen)
            return false;
        Phase = SamusDraygonHoldPhase.Inactive;
        return true;
    }

    /// <summary>
    /// Runs <see cref="SamusMovementType.DraygonHeld"/> movement for every pose of that movement type: the grabbed poses,
    /// whose position enemy AI has already supplied through <see cref="CarryTo"/>, and the Draygon-held ball the
    /// Baby's Hyper Beam freeze can install without Draygon (#1535), which therefore stays where it is. Her movement only
    /// clears her vertical collision with a solid.
    /// </summary>
    public static void StepMovement(SamusState samus)
    {
        Ensure.NotNull(samus);
        EnsureDraygonHeldMovement(samus);

        samus.ForgetVerticalCollisionResult();
    }

    /// <summary>
    /// Runs the escape check after Samus's movement: counts a newly pressed pattern of directions only when it is nonzero
    /// and different from the last counted pattern, and releases her once the count reaches its target.
    /// </summary>
    /// <returns>Whether the check released her.</returns>
    public bool CheckDraygonEscape(
        SamusState samus,
        GameplayAction newlyPressedInput)
    {
        Ensure.NotNull(samus);
        EnsureGrabbed();

        GameplayAction directions = newlyPressedInput & GameplayActions.Directions;
        bool counted = false;
        if (directions != GameplayAction.None && directions != PreviousDirectionsPressed)
        {
            PreviousDirectionsPressed = directions;
            escapePresses.CountEscapePress();
            counted = true;
        }

        // The game tests the count against the target only on a newly counted press of the directions.
        bool released = counted && escapePresses.EscapePressesReach(SamusDraygonHoldDefinitions.EscapePresses);
        if (released)
            Release(samus);
        return released;
    }

    /// <summary>
    /// Releases Samus as she breaks free: the release Draygon has not yet seen, which his next carry of her reports
    /// (<see cref="CarryTo"/>).
    /// </summary>
    public void Release(SamusState samus)
    {
        LetGo(samus);
        Phase = SamusDraygonHoldPhase.ReleasedUnseen;
    }

    /// <summary>
    /// Draygon releases Samus himself, as his final whips end or he dies: he sees his own release, so his next carry has no
    /// escape to report. Death releases her even when she was never grabbed.
    /// </summary>
    public void ReleaseByDraygon(SamusState samus)
    {
        LetGo(samus);
        Phase = SamusDraygonHoldPhase.Inactive;
    }

    /// <summary>
    /// Runs the release. The release keeps extra run speed exactly as the game does,
    /// but clears base X speed, all vertical velocity, bounce state, and acceleration mode.
    /// </summary>
    private static void LetGo(SamusState samus)
    {
        Ensure.NotNull(samus);

        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        samus.Pose = facingLeft
            ? SamusPoseId.FacingLeftNormalPose
            : SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        if (samus.Pose != samus.PoseHistory.PreviousPose)
            SamusPresentation.InitializeAnimation(samus);
        // Death calls this even when Samus was never grabbed. Returning her movement to her
        // pose interrupts a spark without resetting its independent boost/shade.
        SamusMovementHandOver.BackToPose(samus, SamusMovementOwner.DraygonHold);

        // Release clears pending input transitions, so commit here rather than
        // relying on the normal pose step to observe the standing pose.
        samus.CommitPoseHistory();

        samus.HorizontalSpeed.BaseSpeed = default;
        samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.Still;
        samus.MorphBallBounceState = SamusBallBounceState.None;
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
    }

    /// <summary>Rejects escape checks before Draygon grabs Samus.</summary>
    private void EnsureGrabbed()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException(
                "Draygon's escape check requires Draygon to have grabbed Samus first.");
        }

        // The escape check and the enemy's placement of Samus are independent of her pose
        // and of who moves her. Crystal Flash can take both while she is still held.
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
