using SuperMetroid.Core.Samus.Movement;
namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Value-only X-ray display input sampled after the main loop: the HDMA object's setup and beam
/// phases, and the cone geometry its window table is built from. Facing and crouch are resolved
/// from pose metadata here, so rendering needs no cartridge definition access.
/// </summary>
/// <param name="FinishedThisFrame">Whether this logic frame's beam step completed phase five, which still owns this displayed frame.</param>
/// <param name="SetupStage">The next X-ray setup call; <see cref="XraySetupStage.Complete"/> once setup has finished.</param>
/// <param name="BeamPhase">The beam's current HDMA phase.</param>
/// <param name="Angle">Beam aim in 256 table units per turn.</param>
/// <param name="AngularWidth">Beam half-width in whole angle-table units, 0 through 10.</param>
/// <param name="SamusPosition">Samus's whole room-pixel position.</param>
/// <param name="FacingLeft">Whether Samus's current pose faces left.</param>
/// <param name="Crouching">Whether Samus's current pose has the crouching movement type.</param>
public readonly record struct SamusXrayDisplaySnapshot(
    bool FinishedThisFrame,
    XraySetupStage SetupStage,
    XrayBeamPhase BeamPhase,
    SnesAngle Angle,
    int AngularWidth,
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
        return new(finished, samus.Xray.SetupStage, samus.Xray.BeamPhase, samus.Xray.Angle,
            samus.Xray.AngularWidth, samus.Position.Whole, samus.PoseMetadata.IsFacingLeft(),
            samus.PoseMetadata.ReadMovementType() == SamusMovementType.Crouching);
    }
}
