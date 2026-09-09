using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

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

    /// <summary>WRAM <c>DraygonEscapeButtonCounter</c>.</summary>
    public ushort EscapeButtonCounter { get; private set; }

    /// <summary>
    /// WRAM <c>DraygonEscapePreviousDpadInput</c>. Native stores the entire masked D-pad
    /// nibble, not merely one direction, so a different diagonal chord is also a new input.
    /// </summary>
    public ushort PreviousDpadInput { get; private set; }

    /// <summary>
    /// Set by release exactly like grapple-connected-flags bit one at `$90:E353`. Draygon's
    /// next <c>MoveSamusWithDraygon</c> call consumes the signal and starts flying upward.
    /// </summary>
    public bool ReleasePublishedToOwner { get; private set; }

    /// <summary>Most recently supplied whole-pixel Draygon body X coordinate.</summary>
    public ushort OwnerXPosition { get; private set; }

    /// <summary>Most recently supplied whole-pixel Draygon body Y coordinate.</summary>
    public ushort OwnerYPosition { get; private set; }

    /// <summary>Direction word used by bank `$A5`: false/zero left, true/nonzero right.</summary>
    public bool OwnerFacingRight { get; private set; }

    /// <summary>
    /// Ports <c>SetSamusIntoTheGrabbedByDraygonPose</c> at `$90:E23B`.
    /// </summary>
    public void Begin(ISnesAddressSpace bus, SamusState samus, bool draygonFacingRight)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        samus.Pose = draygonFacingRight
            ? SamusPoseIds.DraygonGrabbedNeutralRightPose
            : SamusPoseIds.DraygonGrabbedNeutralLeftPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);

        IsActive = true;
        EscapeButtonCounter = 0;
        PreviousDpadInput = 0;
        ReleasePublishedToOwner = false;
        OwnerFacingRight = draygonFacingRight;
        // Forced grab owns its history shift; no ordinary input slot is consumed.
        samus.CommitPoseHistory(bus);
    }

    /// <summary>
    /// Ports <c>MoveSamusWithDraygon</c> at `$A5:94A9` after enemy AI has updated the
    /// boss body. Facing left places Samus eight pixels left; facing right places her eight
    /// pixels right; both place her 40 pixels below the body center.
    /// </summary>
    public DraygonOwnerPlacement ApplyOwnerPosition(
        SamusState samus,
        ushort ownerXPosition,
        ushort ownerYPosition,
        bool draygonFacingRight)
    {
        ArgumentNullException.ThrowIfNull(samus);
        EnsureActiveGrabbedPose(samus);

        OwnerXPosition = ownerXPosition;
        OwnerYPosition = ownerYPosition;
        OwnerFacingRight = draygonFacingRight;

        short xOffset = draygonFacingRight ? (short)8 : (short)-8;
        samus.XPosition = unchecked((ushort)(ownerXPosition + xOffset));
        samus.YPosition = unchecked((ushort)(ownerYPosition + 0x28));

        // The native routine writes whole positions only. It does not clear subpositions,
        // so preserving the split low words is intentional even though the actor overwrites
        // the visible whole coordinates on every owner update.
        return new DraygonOwnerPlacement(
            ownerXPosition,
            ownerYPosition,
            draygonFacingRight,
            xOffset,
            0x28,
            samus.XPosition,
            samus.YPosition);
    }

    /// <summary>
    /// Ports movement type `$1A`. Its entire normal beta handler is one <c>STZ</c>; enemy
    /// AI has already supplied position through <see cref="ApplyOwnerPosition"/>.
    /// </summary>
    public static DraygonGrabbedMovementResult StepMovement(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        EnsureGrabbedPose(samus);

        ushort previousResult = samus.SolidVerticalCollisionResult;
        samus.SolidVerticalCollisionResult = 0;
        return new DraygonGrabbedMovementResult(
            samus.Pose,
            samus.XPosition,
            samus.YPosition,
            previousResult,
            samus.SolidVerticalCollisionResult);
    }

    /// <summary>
    /// Ports `$90:E2A1`: optionally suppress the already-selected input transition while a
    /// grapple is locked in place, then count a newly pressed D-pad pattern only when it is
    /// nonzero and different from the last counted pattern.
    /// </summary>
    public DraygonEscapeResult StepEscapeHandler(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort newlyPressedInput,
        bool grappleLockedInPlace)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        EnsureActiveGrabbedPose(samus);

        SnesButton newlyPressed = SnesButtons.FromRaw(
            newlyPressedInput,
            "Draygon-grab escape input");
        ushort dpad = (ushort)(newlyPressed & SnesButtons.DirectionalPad);
        bool counted = false;
        if (dpad != 0 && dpad != PreviousDpadInput)
        {
            PreviousDpadInput = dpad;
            EscapeButtonCounter = unchecked((ushort)(EscapeButtonCounter + 1));
            counted = true;
        }

        bool released = EscapeButtonCounter >= EscapeButtonCounterTarget;
        byte poseBeforeRelease = samus.Pose;
        if (released)
            Release(bus, samus);

        return new DraygonEscapeResult(
            poseBeforeRelease,
            samus.Pose,
            dpad,
            counted,
            EscapeButtonCounter,
            grappleLockedInPlace,
            released);
    }

    /// <summary>
    /// Ports `$90:E2DE-$E359`. The release keeps extra run speed exactly as native does,
    /// but clears base X speed, all vertical velocity, bounce state, and acceleration mode.
    /// </summary>
    public void Release(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        EnsureActiveGrabbedPose(samus);

        bool facingLeft = samus.IsFacingLeft(bus);
        samus.Pose = facingLeft
            ? SamusPoseIds.FacingLeftNormalPose
            : SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);

        // Release clears pending input transitions, so commit here rather than
        // relying on the normal pose dispatcher to observe the standing pose.
        samus.CommitPoseHistory(bus);

        samus.HorizontalSpeed.BaseSpeed = 0;
        samus.HorizontalSpeed.BaseSubspeed = 0;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
        samus.Kinematics.YDirection = 0;
        samus.MorphBallBounceState = 0;
        samus.HorizontalSpeed.AccelerationMode = 0;

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

    private void EnsureActiveGrabbedPose(SamusState samus)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException(
                "Draygon owner/hack handling requires SetSamusIntoTheGrabbedByDraygonPose first.");
        }

        EnsureGrabbedPose(samus);
    }

    private static void EnsureGrabbedPose(SamusState samus)
    {
        if (!SamusState.IsDraygonGrabbedPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Draygon movement requires pose $BA-$BE/$EC-$F0, not ${samus.Pose:X2}.");
        }
    }
}

/// <summary>Debugger-visible result of the otherwise one-instruction `$90:A7D2` handler.</summary>
public readonly record struct DraygonGrabbedMovementResult(
    byte Pose,
    ushort XPosition,
    ushort YPosition,
    ushort PreviousSolidVerticalCollisionResult,
    ushort SolidVerticalCollisionResult);

/// <summary>Literal claw offsets and resulting Samus position from `$A5:94A9`.</summary>
public readonly record struct DraygonOwnerPlacement(
    ushort OwnerXPosition,
    ushort OwnerYPosition,
    bool OwnerFacingRight,
    short XOffset,
    short YOffset,
    ushort SamusXPosition,
    ushort SamusYPosition);

/// <summary>One timer/hack-handler pass at `$90:E2A1`.</summary>
public readonly record struct DraygonEscapeResult(
    byte PoseBeforeRelease,
    byte PoseAfterRelease,
    ushort NewlyPressedDpad,
    bool CountedInput,
    ushort EscapeButtonCounter,
    bool SuppressProspectivePose,
    bool Released);
