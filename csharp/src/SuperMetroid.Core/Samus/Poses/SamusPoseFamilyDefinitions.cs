namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The pose families gameplay tests, each listed by name (#1517); <see cref="SamusPoseMetadata"/> answers whether a pose
/// belongs to one.
/// </summary>
internal static class SamusPoseFamilyDefinitions
{
    /// <summary>The six shinespark flights: horizontal, vertical and diagonal, facing right and left.</summary>
    internal static readonly SamusPoseId[] ShinesparkPoses =
    [
        SamusPoseId.ShinesparkHorizontalRightPose,
        SamusPoseId.ShinesparkHorizontalLeftPose,
        SamusPoseId.ShinesparkVerticalRightPose,
        SamusPoseId.ShinesparkVerticalLeftPose,
        SamusPoseId.ShinesparkDiagonalRightPose,
        SamusPoseId.ShinesparkDiagonalLeftPose,
    ];

    /// <summary>
    /// The poses an elevator arms under: the power-suit front view and the standing poses facing right or left, normal or
    /// aimed up, diagonally up or diagonally down. The Varia and Gravity front view does not arm it.
    /// </summary>
    internal static readonly SamusPoseId[] ElevatorArmingPoses =
    [
        SamusPoseId.ForwardFacingPowerSuitPose,
        SamusPoseId.FacingRightNormalPose,
        SamusPoseId.FacingLeftNormalPose,
        SamusPoseId.StandingAimUpRightPose,
        SamusPoseId.StandingAimUpLeftPose,
        SamusPoseId.StandingAimDiagonalUpRightPose,
        SamusPoseId.StandingAimDiagonalUpLeftPose,
        SamusPoseId.StandingAimDiagonalDownRightPose,
        SamusPoseId.StandingAimDiagonalDownLeftPose,
    ];

    /// <summary>
    /// The posture-transition weapon handler's answer for each pose it does not read beyond its table for; every pose not
    /// listed is <see cref="SamusPostureTransitionAdmission.BeyondTable"/>.
    /// </summary>
    internal static readonly Dictionary<SamusPoseId, SamusPostureTransitionAdmission> PostureTransitionAdmissions = new()
    {
        [SamusPoseId.CrouchingTransitionRightPose] = SamusPostureTransitionAdmission.TableAdmits,
        [SamusPoseId.CrouchingTransitionLeftPose] = SamusPostureTransitionAdmission.TableAdmits,
        [SamusPoseId.MorphingTransitionRightPose] = SamusPostureTransitionAdmission.TablePreservesCharge,
        [SamusPoseId.MorphingTransitionLeftPose] = SamusPostureTransitionAdmission.TablePreservesCharge,
        [SamusPoseId.GlitchBallMorphingRightPose] = SamusPostureTransitionAdmission.TablePreservesCharge,
        [SamusPoseId.GlitchBallMorphingLeftPose] = SamusPostureTransitionAdmission.TablePreservesCharge,
        [SamusPoseId.StandingTransitionRightPose] = SamusPostureTransitionAdmission.TableAdmits,
        [SamusPoseId.StandingTransitionLeftPose] = SamusPostureTransitionAdmission.TableAdmits,
        [SamusPoseId.UnmorphingTransitionRightPose] = SamusPostureTransitionAdmission.TablePreservesCharge,
        [SamusPoseId.UnmorphingTransitionLeftPose] = SamusPostureTransitionAdmission.TablePreservesCharge,
        [SamusPoseId.InstantMorphingRightPose] = SamusPostureTransitionAdmission.TablePreservesCharge,
        [SamusPoseId.InstantMorphingLeftPose] = SamusPostureTransitionAdmission.TablePreservesCharge,
        [SamusPoseId.CompactMorphingRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DraygonHeldMorphingLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.CompactStandingUpRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DraygonHeldStandingUpLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DraygonHeldBallLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.LandingAimUpRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.LandingAimUpLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.LandingAimDiagonalUpRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.LandingAimDiagonalUpLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.LandingAimDiagonalDownRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.LandingAimDiagonalDownLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.FiringLandingRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.FiringLandingLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DrainedCrouchingRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DrainedCrouchingLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DrainedStandingRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DrainedStandingLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DraygonGrabbedNeutralRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DraygonGrabbedAimUpRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DraygonGrabbedFiringRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DraygonGrabbedAimDownRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DraygonGrabbedMovingRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.CrouchingTransitionAimUpRightPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.CrouchingTransitionAimUpLeftPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.StandingTransitionAimUpRightPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.StandingTransitionAimUpLeftPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.StandingTransitionAimDiagonalUpRightPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.StandingTransitionAimDiagonalUpLeftPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.StandingTransitionAimDiagonalDownRightPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.StandingTransitionAimDiagonalDownLeftPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.TallStillRightPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.ForwardStandingPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.ForwardStillCrouchHeightPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
    };
}
