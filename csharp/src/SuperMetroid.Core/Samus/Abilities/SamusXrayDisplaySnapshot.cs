using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Movement;
namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Value-only X-ray display input sampled after the update: the cone effect's warm-up, beam
/// phase and closing, and the cone geometry it is drawn from. Facing and crouch are resolved
/// from pose metadata here, so rendering needs no pose definition access.
/// </summary>
/// <param name="FinishedThisFrame">Whether this logic frame's beam step was its last, which still owns this displayed frame.</param>
/// <param name="WarmUp">The beam's warm-up; <see cref="XrayWarmUp.Over"/> once the beam steps.</param>
/// <param name="BeamPhase">The beam's current phase.</param>
/// <param name="Closing">The beam's closing; <see cref="XrayClosing.Over"/> whenever the beam is not closing.</param>
/// <param name="Angle">The beam's aim.</param>
/// <param name="AngularWidth">Beam half-width in whole steps, none through ten.</param>
/// <param name="SamusPosition">Samus's whole room-pixel position.</param>
/// <param name="Facing">The way Samus faces: left, or right otherwise (#2560).</param>
/// <param name="Crouching">Whether Samus's current pose has the crouching movement type.</param>
public readonly partial record struct SamusXrayDisplaySnapshot(
    bool FinishedThisFrame,
    XrayWarmUp WarmUp,
    XrayBeamPhase BeamPhase,
    XrayClosing Closing,
    RotationAngle Angle,
    TrigonometryAngle AngularWidth,
    Position16<RoomPixels> SamusPosition,
    HorizontalDirection Facing,
    bool Crouching)
{
    /// <summary>
    /// Captures the X-ray display inputs, or null when X-ray neither is active nor finished during this logic frame.
    /// </summary>
    /// <param name="samus">The shown frame's Samus.</param>
    /// <param name="lastBeamStep">This frame's beam step result, when one ran.</param>
    public static SamusXrayDisplaySnapshot? Capture(SamusState samus, XrayBeamStepResult? lastBeamStep)
    {
        Ensure.NotNull(samus);
        bool finished = lastBeamStep is { Completed: true, PhaseAtStart: XrayBeamPhase.Finish };
        if (!samus.Xray.IsActive && !finished) return null;
        return new(finished, samus.Xray.WarmUp, samus.Xray.BeamPhase, samus.Xray.Closing, samus.Xray.Angle,
            samus.Xray.AngularWidth, samus.Position.Whole,
            SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left) ? HorizontalDirection.Left : HorizontalDirection.Right,
            SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Crouching);
    }
}
