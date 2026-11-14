using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Movement;
namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Value-only X-ray display input sampled after the update: the cone effect's warm-up, beam
/// phase and closing, and the cone geometry its window table is built from. Facing and crouch are resolved
/// from pose metadata here, so rendering needs no pose definition access.
/// </summary>
/// <param name="FinishedThisFrame">Whether this logic frame's beam step was its last, which still owns this displayed frame.</param>
/// <param name="WarmUp">The beam's warm-up; <see cref="XrayWarmUp.Over"/> once the beam steps.</param>
/// <param name="BeamPhase">The beam's current phase.</param>
/// <param name="Closing">The beam's closing; <see cref="XrayClosing.Over"/> whenever the beam is not closing.</param>
/// <param name="Angle">Beam aim in 256 table units per turn.</param>
/// <param name="AngularWidth">Beam half-width in whole steps, none through ten.</param>
/// <param name="SamusPosition">Samus's whole room-pixel position.</param>
/// <param name="FacingLeft">Whether Samus's current pose faces left.</param>
/// <param name="Crouching">Whether Samus's current pose has the crouching movement type.</param>
[IndependentFlag("independent facts the x-ray display reads")]
public readonly record struct SamusXrayDisplaySnapshot(
    bool FinishedThisFrame,
    XrayWarmUp WarmUp,
    XrayBeamPhase BeamPhase,
    XrayClosing Closing,
    RotationAngle Angle,
    TrigonometryAngle AngularWidth,
    Position16<RoomPixels> SamusPosition,
    bool FacingLeft,
    bool Crouching)
{
    /// <summary>
    /// Captures the X-ray display inputs, or null when Samus is absent or X-ray neither is active
    /// nor finished during this logic frame.
    /// </summary>
    /// <param name="samus">Samus, when present.</param>
    /// <param name="lastBeamStep">This frame's beam step result, when one ran.</param>
    public static SamusXrayDisplaySnapshot? Capture(SamusState? samus, XrayBeamStepResult? lastBeamStep)
    {
        bool finished = lastBeamStep is { Completed: true, PhaseAtStart: XrayBeamPhase.Finish };
        if (samus is null || !samus.Xray.IsActive && !finished) return null;
        return new(finished, samus.Xray.WarmUp, samus.Xray.BeamPhase, samus.Xray.Closing, samus.Xray.Angle,
            samus.Xray.AngularWidth, samus.Position.Whole, SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left),
            SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Crouching);
    }
}
