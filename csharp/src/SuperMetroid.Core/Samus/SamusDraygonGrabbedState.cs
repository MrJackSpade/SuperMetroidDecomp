using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// Exact Samus-owned half of the grabbed-by-Draygon route at `$90:E23B-$E359` and
/// movement type `$1A` at `$90:A7D2`.
/// </summary>
/// <remarks>
/// Draygon is an enemy actor, so bank `$A5` computes the boss's flight position and then
/// calls <c>MoveSamusWithDraygon</c>. This object deliberately accepts that actor position
/// as an explicit input. It does not fabricate an enemy trajectory inside Samus physics.
/// Everything after that boundary—the signed claw offset, pose family, escape counter,
/// release pose, velocity cleanup, and movement-type side effect—is translated here.
/// </remarks>
public sealed class SamusDraygonGrabbedState
{
    /// <summary>Native escape threshold stored at `$90:E095`.</summary>
    public const ushort EscapeButtonCounterTarget = 60;

    /// <summary>True while `$90:E2A1` is installed as Samus's timer/hack handler.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Another callback replaced the RTS movement pointer without removing grab gamma.</summary>
    public bool MovementHandlerReplaced { get; private set; }

    /// <summary>True only while the grab's RTS still owns beta movement.</summary>
    public bool OwnsMovement => IsActive && !MovementHandlerReplaced;

    /// <summary>Preserves grab gamma while a newer movement callback replaces RTS.</summary>
    internal void RelinquishMovementHandler() => MovementHandlerReplaced = true;

    /// <summary>
    /// D-pad presses counted toward escaping, 0 through 65535. The counter shares one storage word with Crystal
    /// Flash's ammo countdown and the suit pickup's stage number, which mirror their writes into it, so it holds any
    /// value those owners leave, including a wrapped countdown. It advances as a sixteen-bit counter and the release
    /// test is the cartridge's signed comparison against the target; other values are rejected.
    /// </summary>
    public int EscapeButtonCounter { get; private set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }

    /// <summary>Mirrors another owner's write to the shared counter word: Crystal Flash's countdown or a suit pickup stage.</summary>
    internal void SetSharedEscapeCounter(int value) => EscapeButtonCounter = value;

    /// <summary>The counter after one counted press: the shared word advances with sixteen-bit wraparound.</summary>
    private static int CountPress(int counter) => unchecked((ushort)(counter + 1));

    /// <summary>
    /// Whether a counted press reached the target. The cartridge compares the shared word with a signed subtraction,
    /// so a word of 32828 or more, left by another owner, reads as below it.
    /// </summary>
    private static bool ReachedEscapeTarget(int counter) =>
        unchecked((short)(counter - EscapeButtonCounterTarget)) >= 0;

    /// <summary>
    /// WRAM <c>DraygonEscapePreviousDpadInput</c>. Native stores the entire masked D-pad
    /// nibble, not merely one direction, so a different diagonal chord is also a new input.
    /// </summary>
    public ControllerButton PreviousDpadInput { get; private set; }

    /// <summary>
    /// Set by release exactly like grapple-connected-flags bit one at `$90:E353`. Draygon's
    /// next <c>MoveSamusWithDraygon</c> call consumes the signal and starts flying upward.
    /// </summary>
    public bool ReleasePublishedToOwner { get; private set; }

    /// <summary>Most recently supplied whole-pixel Draygon body position.</summary>
    public Position16<RoomPixels> OwnerPosition { get; private set; }

    /// <summary>Direction word used by bank `$A5`: false/zero left, true/nonzero right.</summary>
    public bool OwnerFacingRight { get; private set; }

    /// <summary>
    /// Ports <c>SetSamusIntoTheGrabbedByDraygonPose</c> at `$90:E23B`.
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
        EscapeButtonCounter = 0;
        samus.CrystalFlash.SetSharedAmmoCounter(EscapeButtonCounter);
        // Grab installs RTS movement, but leaves the Flash palette and its shared
        // shine-timer word alive. A later release must not resume Flash draining.
        samus.CrystalFlash.RelinquishMovementHandler();
        PreviousDpadInput = 0;
        ReleasePublishedToOwner = false;
        OwnerFacingRight = draygonFacingRight;
        // Forced grab owns its history shift; no ordinary input slot is consumed.
        samus.CommitPoseHistory();
        // `$90:E289-$E295` empties every prospective-pose slot. The super-special one is Samus's;
        // the runtime clears the others after EnemyMain (EnemyRoomStatus publishes the grab).
        samus.PendingTransitionalPose = null;
    }

    /// <summary>
    /// Ports <c>MoveSamusWithDraygon</c> at `$A5:94A9` after enemy AI has updated the
    /// boss body. Facing left places Samus eight pixels left; facing right places her eight
    /// pixels right; both place her 40 pixels below the body center. Draygon also places a
    /// Samus who has just escaped, until it consumes the escape bit right after this placement.
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

        OwnerPosition = ownerPosition;
        OwnerFacingRight = draygonFacingRight;

        short xOffset = draygonFacingRight ? (short)8 : (short)-8;
        samus.Position = samus.Position.WithWhole(ownerPosition + new Offset16<RoomPixels>(xOffset, 40));

        // The native routine writes whole positions only. It does not clear subpositions,
        // so preserving the split low words is intentional even though the actor overwrites
        // the visible whole coordinates on every owner update.
        return new DraygonOwnerPlacement();
    }

    /// <summary>
    /// Ports movement type `$1A`. Its entire normal beta handler is one <c>STZ</c>; enemy
    /// AI has already supplied position through <see cref="ApplyOwnerPosition"/>.
    /// </summary>
    public static DraygonGrabbedMovementResult StepMovement(SamusState samus)
    {
        Ensure.NotNull(samus);
        EnsureGrabbedPose(samus);

        samus.SolidVerticalCollisionResult = SamusVerticalCollisionResult.None;
        return new DraygonGrabbedMovementResult();
    }

    /// <summary>
    /// Ports `$90:E2A1`: optionally suppress the already-selected input transition while a
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
            EscapeButtonCounter = CountPress(EscapeButtonCounter);
            samus.CrystalFlash.SetSharedAmmoCounter(EscapeButtonCounter);
            counted = true;
        }

        // Native tests the signed difference only on a newly counted D-pad edge.
        bool released = counted && ReachedEscapeTarget(EscapeButtonCounter);
        if (released)
            Release(samus);

        return new DraygonEscapeResult(
            grappleLockedInPlace,
            released);
    }

    /// <summary>
    /// Ports `$90:E2DE-$E359`. The release keeps extra run speed exactly as native does,
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

    /// <summary>Consumes the bank-$A5-facing release bit exactly once.</summary>
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

    /// <summary>Requires one of the native grabbed pose families before running movement type $1A.</summary>
    private static void EnsureGrabbedPose(SamusState samus)
    {
        if (!SamusPoseMetadata.IsDraygonGrabbedPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Draygon movement requires pose $BA-$BE/$EC-$F0, not ${(int)samus.Pose:X2}.");
        }
    }
}
