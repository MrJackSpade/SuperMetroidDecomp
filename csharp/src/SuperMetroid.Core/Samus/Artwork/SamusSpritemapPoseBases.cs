using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Core.Samus.Artwork.SamusSpritemapPoseDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Where each pose's top and bottom halves begin in the pose-half table.</summary>
internal static class SamusSpritemapPoseBases
{
    /// <summary>$92:9263 pose table: exactly253 named real-pose top-half identities.</summary>
    internal static ushort TopBase(SamusPoseId pose) => pose switch
    {
        SamusPoseId.ForwardFacingPowerSuitPose => TopForwardFacingPowerSuitBase,
        SamusPoseId.FacingRightNormalPose or
        SamusPoseId.UnusedPose47 or
        SamusPoseId.RanIntoWallRightPose or
        SamusPoseId.GrappleStandingRightPose => TopFacingRightNormalBase,
        SamusPoseId.FacingLeftNormalPose or
        SamusPoseId.UnusedPose48 or
        SamusPoseId.RanIntoWallLeftPose or
        SamusPoseId.GrappleStandingLeftPose => TopFacingLeftNormalBase,
        SamusPoseId.StandingAimUpRightPose => TopStandingAimUpRightBase,
        SamusPoseId.StandingAimUpLeftPose => TopStandingAimUpLeftBase,
        SamusPoseId.StandingAimDiagonalUpRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
        SamusPoseId.RanIntoWallAimUpRightPose or
        SamusPoseId.LandingAimDiagonalUpRightPose or
        SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
        SamusPoseId.StandingTransitionAimDiagonalUpRightPose => TopStandingAimDiagonalUpRightBase,
        SamusPoseId.StandingAimDiagonalUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
        SamusPoseId.RanIntoWallAimUpLeftPose or
        SamusPoseId.LandingAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalUpLeftPose => TopStandingAimDiagonalUpLeftBase,
        SamusPoseId.StandingAimDiagonalDownRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
        SamusPoseId.GrappleStandingDownRightPose or
        SamusPoseId.RanIntoWallAimDownRightPose or
        SamusPoseId.LandingAimDiagonalDownRightPose or
        SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
        SamusPoseId.StandingTransitionAimDiagonalDownRightPose => TopStandingAimDiagonalDownRightBase,
        SamusPoseId.StandingAimDiagonalDownLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
        SamusPoseId.GrappleStandingDownLeftPose or
        SamusPoseId.RanIntoWallAimDownLeftPose or
        SamusPoseId.LandingAimDiagonalDownLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => TopStandingAimDiagonalDownLeftBase,
        SamusPoseId.MovingRightNormalPose => TopMovingRightNormalBase,
        SamusPoseId.MovingLeftNormalPose => TopMovingLeftNormalBase,
        SamusPoseId.MovingRightGunExtendedPose => TopMovingRightGunExtendedBase,
        SamusPoseId.MovingLeftGunExtendedPose => TopMovingLeftGunExtendedBase,
        SamusPoseId.RunningAimUpRightPose => TopRunningAimUpRightBase,
        SamusPoseId.RunningAimUpLeftPose => TopRunningAimUpLeftBase,
        SamusPoseId.RunningAimDiagonalUpRightPose => TopRunningAimDiagonalUpRightBase,
        SamusPoseId.RunningAimDiagonalUpLeftPose => TopRunningAimDiagonalUpLeftBase,
        SamusPoseId.RunningAimDiagonalDownRightPose => TopRunningAimDiagonalDownRightBase,
        SamusPoseId.RunningAimDiagonalDownLeftPose => TopRunningAimDiagonalDownLeftBase,
        SamusPoseId.NormalJumpGunExtendedRightPose or
        SamusPoseId.UnusedPoseAC => TopNormalJumpGunExtendedRightBase,
        SamusPoseId.NormalJumpGunExtendedLeftPose or
        SamusPoseId.UnusedPoseAD => TopNormalJumpGunExtendedLeftBase,
        SamusPoseId.NormalJumpAimUpRightPose => TopNormalJumpAimUpRightBase,
        SamusPoseId.NormalJumpAimUpLeftPose => TopNormalJumpAimUpLeftBase,
        SamusPoseId.NormalJumpAimDownRightPose or
        SamusPoseId.UnusedPoseAE => TopNormalJumpAimDownRightBase,
        SamusPoseId.NormalJumpAimDownLeftPose or
        SamusPoseId.UnusedPoseAF => TopNormalJumpAimDownLeftBase,
        SamusPoseId.SpinJumpRightPose or
        SamusPoseId.UnusedPose20 or
        SamusPoseId.UnusedPose21 or
        SamusPoseId.UnusedPose22 or
        SamusPoseId.UnusedPose23 or
        SamusPoseId.UnusedPose24 or
        SamusPoseId.UnusedKnockbackRightPose or
        SamusPoseId.UnusedKnockbackLeftPose or
        SamusPoseId.UnusedPose39 or
        SamusPoseId.UnusedPose3A or
        SamusPoseId.UnusedPose42 => TopSpinJumpRightBase,
        SamusPoseId.SpinJumpLeftPose => TopSpinJumpLeftBase,
        SamusPoseId.SpaceJumpRightPose => TopSpaceJumpRightBase,
        SamusPoseId.SpaceJumpLeftPose => TopSpaceJumpLeftBase,
        SamusPoseId.MorphBallGroundRightPose or
        SamusPoseId.MorphBallFallingRightPose or
        SamusPoseId.MorphBallFallingLeftPose or
        SamusPoseId.UnusedPose3F or
        SamusPoseId.UnusedPose40 => TopMorphBallGroundRightBase,
        SamusPoseId.MorphBallMovingRightPose => TopMorphBallMovingRightBase,
        SamusPoseId.MorphBallMovingLeftPose => TopMorphBallMovingLeftBase,
        SamusPoseId.TurningRightToLeftPose or
        SamusPoseId.TurningRightToLeftJumpPose or
        SamusPoseId.TurningRightToLeftCrouchingPose or
        SamusPoseId.TurningRightToLeftFallingPose or
        SamusPoseId.MoonwalkTurnJumpLeftPose or
        SamusPoseId.UnusedPoseC6 => TopTurningRightToLeftBase,
        SamusPoseId.TurningLeftToRightPose or
        SamusPoseId.TurningLeftToRightJumpPose or
        SamusPoseId.TurningLeftToRightCrouchingPose or
        SamusPoseId.TurningLeftToRightFallingPose or
        SamusPoseId.MoonwalkTurnJumpRightPose => TopTurningLeftToRightBase,
        SamusPoseId.CrouchingRightPose or
        SamusPoseId.GrappleCrouchingRightPose => TopCrouchingRightBase,
        SamusPoseId.CrouchingLeftPose or
        SamusPoseId.GrappleCrouchingLeftPose => TopCrouchingLeftBase,
        SamusPoseId.FallingRightPose => TopFallingRightBase,
        SamusPoseId.FallingLeftPose => TopFallingLeftBase,
        SamusPoseId.FallingAimUpRightPose => TopFallingAimUpRightBase,
        SamusPoseId.FallingAimUpLeftPose => TopFallingAimUpLeftBase,
        SamusPoseId.FallingAimDownRightPose => TopFallingAimDownRightBase,
        SamusPoseId.FallingAimDownLeftPose => TopFallingAimDownLeftBase,
        SamusPoseId.CrouchingTransitionRightPose or
        SamusPoseId.StandingTransitionRightPose => TopCrouchingTransitionRightBase,
        SamusPoseId.CrouchingTransitionLeftPose or
        SamusPoseId.StandingTransitionLeftPose => TopCrouchingTransitionLeftBase,
        SamusPoseId.MorphingTransitionRightPose => TopMorphingTransitionRightBase,
        SamusPoseId.MorphingTransitionLeftPose => TopMorphingTransitionLeftBase,
        SamusPoseId.UnmorphingTransitionRightPose => TopUnmorphingTransitionRightBase,
        SamusPoseId.UnmorphingTransitionLeftPose => TopUnmorphingTransitionLeftBase,
        SamusPoseId.MorphBallGroundLeftPose or
        SamusPoseId.UnusedPoseC5 or
        SamusPoseId.UnusedPoseDf => TopMorphBallGroundLeftBase,
        SamusPoseId.UnusedPose45 => TopUnusedPose45Base,
        SamusPoseId.UnusedPose46 => TopUnusedPose46Base,
        SamusPoseId.MoonwalkFacingLeftPose => TopMoonwalkFacingLeftBase,
        SamusPoseId.MoonwalkFacingRightPose => TopMoonwalkFacingRightBase,
        SamusPoseId.NeutralJumpTransitionRightPose => TopNeutralJumpTransitionRightBase,
        SamusPoseId.NeutralJumpTransitionLeftPose => TopNeutralJumpTransitionLeftBase,
        SamusPoseId.NeutralJumpRightPose or
        SamusPoseId.ShinesparkWindupRightPose => TopNeutralJumpRightBase,
        SamusPoseId.NeutralJumpLeftPose or
        SamusPoseId.ShinesparkWindupLeftPose => TopNeutralJumpLeftBase,
        SamusPoseId.DamageBoostLeftPose => TopDamageBoostLeftBase,
        SamusPoseId.DamageBoostRightPose => TopDamageBoostRightBase,
        SamusPoseId.NormalJumpForwardRightPose => TopNormalJumpForwardRightBase,
        SamusPoseId.NormalJumpForwardLeftPose => TopNormalJumpForwardLeftBase,
        SamusPoseId.KnockbackRightPose => TopKnockbackRightBase,
        SamusPoseId.KnockbackLeftPose => TopKnockbackLeftBase,
        SamusPoseId.NormalJumpTransitionAimUpRightPose or
        SamusPoseId.LandingAimUpRightPose or
        SamusPoseId.CrouchingTransitionAimUpRightPose or
        SamusPoseId.StandingTransitionAimUpRightPose => TopNormalJumpTransitionAimUpRightBase,
        SamusPoseId.NormalJumpTransitionAimUpLeftPose or
        SamusPoseId.LandingAimUpLeftPose or
        SamusPoseId.CrouchingTransitionAimUpLeftPose or
        SamusPoseId.StandingTransitionAimUpLeftPose => TopNormalJumpTransitionAimUpLeftBase,
        SamusPoseId.UnusedPose5B or
        SamusPoseId.GrappleWallContactLeftPose => TopUnusedPose5BBase,
        SamusPoseId.UnusedPose5C or
        SamusPoseId.GrappleWallContactRightPose => TopUnusedPose5CBase,
        SamusPoseId.UnusedPose5D or
        SamusPoseId.UnusedPose5E or
        SamusPoseId.UnusedPose5F => TopUnusedPose5DBase,
        SamusPoseId.UnusedPose60 => TopUnusedPose60Base,
        SamusPoseId.UnusedPose61 or
        SamusPoseId.GrappleSwingRightPose => TopUnusedPose61Base,
        SamusPoseId.UnusedPose62 or
        SamusPoseId.GrappleSwingLeftPose => TopUnusedPose62Base,
        SamusPoseId.UnusedPose63 => TopUnusedPose63Base,
        SamusPoseId.UnusedPose64 => TopUnusedPose64Base,
        SamusPoseId.UnusedPose65 => TopUnusedPose65Base,
        SamusPoseId.UnusedPose66 => TopUnusedPose66Base,
        SamusPoseId.FallingGunExtendedRightPose => TopFallingGunExtendedRightBase,
        SamusPoseId.FallingGunExtendedLeftPose => TopFallingGunExtendedLeftBase,
        SamusPoseId.NormalJumpAimDiagonalUpRightPose => TopNormalJumpAimDiagonalUpRightBase,
        SamusPoseId.NormalJumpAimDiagonalUpLeftPose => TopNormalJumpAimDiagonalUpLeftBase,
        SamusPoseId.NormalJumpAimDiagonalDownRightPose or
        SamusPoseId.UnusedPoseB0 => TopNormalJumpAimDiagonalDownRightBase,
        SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
        SamusPoseId.UnusedPoseB1 => TopNormalJumpAimDiagonalDownLeftBase,
        SamusPoseId.FallingAimDiagonalUpRightPose => TopFallingAimDiagonalUpRightBase,
        SamusPoseId.FallingAimDiagonalUpLeftPose => TopFallingAimDiagonalUpLeftBase,
        SamusPoseId.FallingAimDiagonalDownRightPose => TopFallingAimDiagonalDownRightBase,
        SamusPoseId.FallingAimDiagonalDownLeftPose => TopFallingAimDiagonalDownLeftBase,
        SamusPoseId.CrouchingAimDiagonalUpRightPose => TopCrouchingAimDiagonalUpRightBase,
        SamusPoseId.CrouchingAimDiagonalUpLeftPose => TopCrouchingAimDiagonalUpLeftBase,
        SamusPoseId.CrouchingAimDiagonalDownRightPose or
        SamusPoseId.GrappleCrouchingDownRightPose => TopCrouchingAimDiagonalDownRightBase,
        SamusPoseId.CrouchingAimDiagonalDownLeftPose or
        SamusPoseId.GrappleCrouchingDownLeftPose => TopCrouchingAimDiagonalDownLeftBase,
        SamusPoseId.MoonwalkAimUpLeftPose => TopMoonwalkAimUpLeftBase,
        SamusPoseId.MoonwalkAimUpRightPose => TopMoonwalkAimUpRightBase,
        SamusPoseId.MoonwalkAimDownLeftPose => TopMoonwalkAimDownLeftBase,
        SamusPoseId.MoonwalkAimDownRightPose => TopMoonwalkAimDownRightBase,
        SamusPoseId.SpringBallGroundRightPose or
        SamusPoseId.SpringBallMovingRightPose or
        SamusPoseId.SpringBallFallingRightPose or
        SamusPoseId.SpringBallJumpRightPose => TopSpringBallGroundRightBase,
        SamusPoseId.SpringBallGroundLeftPose or
        SamusPoseId.SpringBallMovingLeftPose or
        SamusPoseId.SpringBallFallingLeftPose or
        SamusPoseId.SpringBallJumpLeftPose => TopSpringBallGroundLeftBase,
        SamusPoseId.ScrewAttackRightPose => TopScrewAttackRightBase,
        SamusPoseId.ScrewAttackLeftPose => TopScrewAttackLeftBase,
        SamusPoseId.WallJumpRightPose => TopWallJumpRightBase,
        SamusPoseId.WallJumpLeftPose => TopWallJumpLeftBase,
        SamusPoseId.CrouchingAimUpRightPose => TopCrouchingAimUpRightBase,
        SamusPoseId.CrouchingAimUpLeftPose => TopCrouchingAimUpLeftBase,
        SamusPoseId.TurningRightToLeftAimUpPose or
        SamusPoseId.TurningRightToLeftJumpAimUpPose or
        SamusPoseId.TurningRightToLeftFallingAimUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
        SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or
        SamusPoseId.MoonwalkTurnJumpAimUpLeftPose => TopTurningRightToLeftAimUpBase,
        SamusPoseId.TurningLeftToRightAimUpPose or
        SamusPoseId.TurningLeftToRightJumpAimUpPose or
        SamusPoseId.TurningLeftToRightFallingAimUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
        SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or
        SamusPoseId.MoonwalkTurnJumpAimUpRightPose => TopTurningLeftToRightAimUpBase,
        SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
        SamusPoseId.TurningRightToLeftJumpAimDownPose or
        SamusPoseId.TurningRightToLeftFallingAimDownPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or
        SamusPoseId.MoonwalkTurnJumpAimDownLeftPose => TopTurningRightToLeftAimDiagonalDownBase,
        SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
        SamusPoseId.TurningLeftToRightJumpAimDownPose or
        SamusPoseId.TurningLeftToRightFallingAimDownPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or
        SamusPoseId.MoonwalkTurnJumpAimDownRightPose => TopTurningLeftToRightAimDiagonalDownBase,
        SamusPoseId.ForwardFacingSuitedPose => TopForwardFacingSuitedBase,
        SamusPoseId.NormalLandingRightPose => TopNormalLandingRightBase,
        SamusPoseId.NormalLandingLeftPose => TopNormalLandingLeftBase,
        SamusPoseId.SpinLandingRightPose => TopSpinLandingRightBase,
        SamusPoseId.SpinLandingLeftPose => TopSpinLandingLeftBase,
        SamusPoseId.DraygonGrabbedNeutralLeftPose => TopDraygonGrabbedNeutralLeftBase,
        SamusPoseId.DraygonGrabbedAimUpLeftPose => TopDraygonGrabbedAimUpLeftBase,
        SamusPoseId.DraygonGrabbedFiringLeftPose => TopDraygonGrabbedFiringLeftBase,
        SamusPoseId.DraygonGrabbedAimDownLeftPose => TopDraygonGrabbedAimDownLeftBase,
        SamusPoseId.DraygonGrabbedMovingLeftPose => TopDraygonGrabbedMovingLeftBase,
        SamusPoseId.ShinesparkHorizontalRightPose => TopShinesparkHorizontalRightBase,
        SamusPoseId.ShinesparkHorizontalLeftPose => TopShinesparkHorizontalLeftBase,
        SamusPoseId.ShinesparkVerticalRightPose => TopShinesparkVerticalRightBase,
        SamusPoseId.ShinesparkVerticalLeftPose => TopShinesparkVerticalLeftBase,
        SamusPoseId.ShinesparkDiagonalRightPose => TopShinesparkDiagonalRightBase,
        SamusPoseId.ShinesparkDiagonalLeftPose => TopShinesparkDiagonalLeftBase,
        SamusPoseId.CrystalFlashRightPose => TopCrystalFlashRightBase,
        SamusPoseId.CrystalFlashLeftPose => TopCrystalFlashLeftBase,
        SamusPoseId.XrayingStandingRightPose or
        SamusPoseId.XrayingCrouchingRightPose => TopXrayingStandingRightBase,
        SamusPoseId.XrayingStandingLeftPose or
        SamusPoseId.XrayingCrouchingLeftPose => TopXrayingStandingLeftBase,
        SamusPoseId.DeathSequenceRightPose => TopDeathSequenceRightBase,
        SamusPoseId.DeathSequenceLeftPose => TopDeathSequenceLeftBase,
        SamusPoseId.UnusedPoseDb => TopUnusedPoseDbBase,
        SamusPoseId.UnusedPoseDc => TopUnusedPoseDcBase,
        SamusPoseId.UnusedPoseDd => TopUnusedPoseDdBase,
        SamusPoseId.UnusedPoseDe => TopUnusedPoseDeBase,
        SamusPoseId.FiringLandingRightPose => TopFiringLandingRightBase,
        SamusPoseId.FiringLandingLeftPose => TopFiringLandingLeftBase,
        SamusPoseId.DrainedCrouchingRightPose => TopDrainedCrouchingRightBase,
        SamusPoseId.DrainedCrouchingLeftPose => TopDrainedCrouchingLeftBase,
        SamusPoseId.DrainedStandingRightPose => TopDrainedStandingRightBase,
        SamusPoseId.DrainedStandingLeftPose => TopDrainedStandingLeftBase,
        SamusPoseId.DraygonGrabbedNeutralRightPose => TopDraygonGrabbedNeutralRightBase,
        SamusPoseId.DraygonGrabbedAimUpRightPose => TopDraygonGrabbedAimUpRightBase,
        SamusPoseId.DraygonGrabbedFiringRightPose => TopDraygonGrabbedFiringRightBase,
        SamusPoseId.DraygonGrabbedAimDownRightPose => TopDraygonGrabbedAimDownRightBase,
        SamusPoseId.DraygonGrabbedMovingRightPose => TopDraygonGrabbedMovingRightBase,
        SamusPoseId.AdjacentCodePoseFD or
            SamusPoseId.AdjacentCodePoseFE or
            SamusPoseId.AdjacentCodePoseFF => throw new ArgumentOutOfRangeException(nameof(pose)),
        var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
    };

    /// <summary>$92:945D pose table: exactly253 named real-pose bottom-half identities.</summary>
    internal static ushort BottomBase(SamusPoseId pose) => pose switch
    {
        SamusPoseId.ForwardFacingPowerSuitPose => BottomForwardFacingPowerSuitBase,
        SamusPoseId.FacingRightNormalPose or
        SamusPoseId.UnusedPose47 or
        SamusPoseId.RanIntoWallRightPose or
        SamusPoseId.GrappleStandingRightPose => BottomFacingRightNormalBase,
        SamusPoseId.FacingLeftNormalPose or
        SamusPoseId.UnusedPose48 or
        SamusPoseId.RanIntoWallLeftPose or
        SamusPoseId.GrappleStandingLeftPose => BottomFacingLeftNormalBase,
        SamusPoseId.StandingAimUpRightPose or
        SamusPoseId.StandingAimDiagonalUpRightPose or
        SamusPoseId.StandingAimDiagonalDownRightPose or
        SamusPoseId.GrappleStandingDownRightPose or
        SamusPoseId.RanIntoWallAimUpRightPose or
        SamusPoseId.RanIntoWallAimDownRightPose => BottomStandingAimUpRightBase,
        SamusPoseId.StandingAimUpLeftPose or
        SamusPoseId.StandingAimDiagonalUpLeftPose or
        SamusPoseId.StandingAimDiagonalDownLeftPose or
        SamusPoseId.GrappleStandingDownLeftPose or
        SamusPoseId.RanIntoWallAimUpLeftPose or
        SamusPoseId.RanIntoWallAimDownLeftPose => BottomStandingAimUpLeftBase,
        SamusPoseId.MovingRightNormalPose or
        SamusPoseId.MovingRightGunExtendedPose or
        SamusPoseId.RunningAimUpRightPose or
        SamusPoseId.RunningAimDiagonalUpRightPose or
        SamusPoseId.RunningAimDiagonalDownRightPose or
        SamusPoseId.UnusedPose45 => BottomMovingRightNormalBase,
        SamusPoseId.MovingLeftNormalPose or
        SamusPoseId.MovingLeftGunExtendedPose or
        SamusPoseId.RunningAimUpLeftPose or
        SamusPoseId.RunningAimDiagonalUpLeftPose or
        SamusPoseId.RunningAimDiagonalDownLeftPose or
        SamusPoseId.UnusedPose46 => BottomMovingLeftNormalBase,
        SamusPoseId.NormalJumpGunExtendedRightPose or
        SamusPoseId.UnusedPoseAC => BottomNormalJumpGunExtendedRightBase,
        SamusPoseId.NormalJumpGunExtendedLeftPose or
        SamusPoseId.UnusedPoseAD => BottomNormalJumpGunExtendedLeftBase,
        SamusPoseId.NormalJumpAimUpRightPose => BottomNormalJumpAimUpRightBase,
        SamusPoseId.NormalJumpAimUpLeftPose => BottomNormalJumpAimUpLeftBase,
        SamusPoseId.NormalJumpAimDownRightPose or
        SamusPoseId.UnusedPoseAE => BottomNormalJumpAimDownRightBase,
        SamusPoseId.NormalJumpAimDownLeftPose or
        SamusPoseId.UnusedPoseAF => BottomNormalJumpAimDownLeftBase,
        SamusPoseId.SpinJumpRightPose => BottomSpinJumpRightBase,
        SamusPoseId.SpinJumpLeftPose => BottomSpinJumpLeftBase,
        SamusPoseId.SpaceJumpRightPose => BottomSpaceJumpRightBase,
        SamusPoseId.SpaceJumpLeftPose => BottomSpaceJumpLeftBase,
        SamusPoseId.MorphBallGroundRightPose or
        SamusPoseId.MorphBallFallingRightPose or
        SamusPoseId.MorphBallFallingLeftPose or
        SamusPoseId.UnusedPose3F or
        SamusPoseId.UnusedPose40 => BottomMorphBallGroundRightBase,
        SamusPoseId.MorphBallMovingRightPose => BottomMorphBallMovingRightBase,
        SamusPoseId.MorphBallMovingLeftPose => BottomMorphBallMovingLeftBase,
        SamusPoseId.UnusedPose20 or
        SamusPoseId.UnusedPose21 or
        SamusPoseId.UnusedPose22 or
        SamusPoseId.UnusedPose23 or
        SamusPoseId.UnusedPose24 or
        SamusPoseId.UnusedKnockbackRightPose or
        SamusPoseId.UnusedKnockbackLeftPose or
        SamusPoseId.UnusedPose39 or
        SamusPoseId.UnusedPose3A or
        SamusPoseId.UnusedPose42 => BottomUnusedPose20Base,
        SamusPoseId.TurningRightToLeftPose or
        SamusPoseId.TurningRightToLeftAimUpPose or
        SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
        SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
        SamusPoseId.MoonwalkTurnJumpLeftPose or
        SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
        SamusPoseId.MoonwalkTurnJumpAimDownLeftPose or
        SamusPoseId.UnusedPoseC6 => BottomTurningRightToLeftBase,
        SamusPoseId.TurningLeftToRightPose or
        SamusPoseId.TurningLeftToRightAimUpPose or
        SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
        SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
        SamusPoseId.MoonwalkTurnJumpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimDownRightPose => BottomTurningLeftToRightBase,
        SamusPoseId.CrouchingRightPose or
        SamusPoseId.GrappleCrouchingRightPose => BottomCrouchingRightBase,
        SamusPoseId.CrouchingLeftPose or
        SamusPoseId.GrappleCrouchingLeftPose => BottomCrouchingLeftBase,
        SamusPoseId.FallingRightPose or
        SamusPoseId.FallingGunExtendedRightPose => BottomFallingRightBase,
        SamusPoseId.FallingLeftPose or
        SamusPoseId.FallingGunExtendedLeftPose => BottomFallingLeftBase,
        SamusPoseId.FallingAimUpRightPose => BottomFallingAimUpRightBase,
        SamusPoseId.FallingAimUpLeftPose => BottomFallingAimUpLeftBase,
        SamusPoseId.FallingAimDownRightPose => BottomFallingAimDownRightBase,
        SamusPoseId.FallingAimDownLeftPose => BottomFallingAimDownLeftBase,
        SamusPoseId.TurningRightToLeftJumpPose or
        SamusPoseId.TurningRightToLeftCrouchingPose or
        SamusPoseId.TurningRightToLeftFallingPose or
        SamusPoseId.TurningRightToLeftJumpAimUpPose or
        SamusPoseId.TurningRightToLeftJumpAimDownPose or
        SamusPoseId.TurningRightToLeftFallingAimUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDownPose or
        SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or
        SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose => BottomTurningRightToLeftJumpBase,
        SamusPoseId.TurningLeftToRightJumpPose or
        SamusPoseId.TurningLeftToRightCrouchingPose or
        SamusPoseId.TurningLeftToRightJumpAimUpPose or
        SamusPoseId.TurningLeftToRightJumpAimDownPose or
        SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or
        SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose => BottomTurningLeftToRightJumpBase,
        SamusPoseId.CrouchingTransitionRightPose or
        SamusPoseId.StandingTransitionRightPose or
        SamusPoseId.CrouchingTransitionAimUpRightPose or
        SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
        SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
        SamusPoseId.StandingTransitionAimUpRightPose or
        SamusPoseId.StandingTransitionAimDiagonalUpRightPose or
        SamusPoseId.StandingTransitionAimDiagonalDownRightPose => BottomCrouchingTransitionRightBase,
        SamusPoseId.CrouchingTransitionLeftPose or
        SamusPoseId.StandingTransitionLeftPose or
        SamusPoseId.CrouchingTransitionAimUpLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
        SamusPoseId.StandingTransitionAimUpLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalUpLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => BottomCrouchingTransitionLeftBase,
        SamusPoseId.MorphingTransitionRightPose => BottomMorphingTransitionRightBase,
        SamusPoseId.MorphingTransitionLeftPose => BottomMorphingTransitionLeftBase,
        SamusPoseId.UnmorphingTransitionRightPose => BottomUnmorphingTransitionRightBase,
        SamusPoseId.UnmorphingTransitionLeftPose => BottomUnmorphingTransitionLeftBase,
        SamusPoseId.MorphBallGroundLeftPose or
        SamusPoseId.UnusedPoseC5 or
        SamusPoseId.UnusedPoseDf => BottomMorphBallGroundLeftBase,
        SamusPoseId.MoonwalkFacingLeftPose or
        SamusPoseId.MoonwalkAimUpLeftPose or
        SamusPoseId.MoonwalkAimDownLeftPose => BottomMoonwalkFacingLeftBase,
        SamusPoseId.MoonwalkFacingRightPose or
        SamusPoseId.MoonwalkAimUpRightPose or
        SamusPoseId.MoonwalkAimDownRightPose => BottomMoonwalkFacingRightBase,
        SamusPoseId.NeutralJumpTransitionRightPose or
        SamusPoseId.NormalJumpTransitionAimUpRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose => BottomNeutralJumpTransitionRightBase,
        SamusPoseId.NeutralJumpTransitionLeftPose or
        SamusPoseId.NormalJumpTransitionAimUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose => BottomNeutralJumpTransitionLeftBase,
        SamusPoseId.NeutralJumpRightPose or
        SamusPoseId.ShinesparkWindupRightPose => BottomNeutralJumpRightBase,
        SamusPoseId.NeutralJumpLeftPose or
        SamusPoseId.ShinesparkWindupLeftPose => BottomNeutralJumpLeftBase,
        SamusPoseId.DamageBoostLeftPose => BottomDamageBoostLeftBase,
        SamusPoseId.DamageBoostRightPose => BottomDamageBoostRightBase,
        SamusPoseId.NormalJumpForwardRightPose => BottomNormalJumpForwardRightBase,
        SamusPoseId.NormalJumpForwardLeftPose => BottomNormalJumpForwardLeftBase,
        SamusPoseId.KnockbackRightPose => BottomKnockbackRightBase,
        SamusPoseId.KnockbackLeftPose => BottomKnockbackLeftBase,
        SamusPoseId.UnusedPose5B or
        SamusPoseId.GrappleWallContactLeftPose => BottomUnusedPose5BBase,
        SamusPoseId.UnusedPose5C or
        SamusPoseId.GrappleWallContactRightPose => BottomUnusedPose5CBase,
        SamusPoseId.UnusedPose5D or
        SamusPoseId.UnusedPose5E or
        SamusPoseId.UnusedPose5F or
        SamusPoseId.UnusedPose60 or
        SamusPoseId.UnusedPose61 or
        SamusPoseId.GrappleSwingRightPose => BottomUnusedPose5DBase,
        SamusPoseId.UnusedPose62 or
        SamusPoseId.GrappleSwingLeftPose => BottomUnusedPose62Base,
        SamusPoseId.UnusedPose63 => BottomUnusedPose63Base,
        SamusPoseId.UnusedPose64 => BottomUnusedPose64Base,
        SamusPoseId.UnusedPose65 => BottomUnusedPose65Base,
        SamusPoseId.UnusedPose66 => BottomUnusedPose66Base,
        SamusPoseId.NormalJumpAimDiagonalUpRightPose => BottomNormalJumpAimDiagonalUpRightBase,
        SamusPoseId.NormalJumpAimDiagonalUpLeftPose => BottomNormalJumpAimDiagonalUpLeftBase,
        SamusPoseId.NormalJumpAimDiagonalDownRightPose or
        SamusPoseId.UnusedPoseB0 => BottomNormalJumpAimDiagonalDownRightBase,
        SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
        SamusPoseId.UnusedPoseB1 => BottomNormalJumpAimDiagonalDownLeftBase,
        SamusPoseId.FallingAimDiagonalUpRightPose => BottomFallingAimDiagonalUpRightBase,
        SamusPoseId.FallingAimDiagonalUpLeftPose => BottomFallingAimDiagonalUpLeftBase,
        SamusPoseId.FallingAimDiagonalDownRightPose => BottomFallingAimDiagonalDownRightBase,
        SamusPoseId.FallingAimDiagonalDownLeftPose => BottomFallingAimDiagonalDownLeftBase,
        SamusPoseId.CrouchingAimDiagonalUpRightPose => BottomCrouchingAimDiagonalUpRightBase,
        SamusPoseId.CrouchingAimDiagonalUpLeftPose => BottomCrouchingAimDiagonalUpLeftBase,
        SamusPoseId.CrouchingAimDiagonalDownRightPose or
        SamusPoseId.GrappleCrouchingDownRightPose => BottomCrouchingAimDiagonalDownRightBase,
        SamusPoseId.CrouchingAimDiagonalDownLeftPose or
        SamusPoseId.GrappleCrouchingDownLeftPose => BottomCrouchingAimDiagonalDownLeftBase,
        SamusPoseId.SpringBallGroundRightPose or
        SamusPoseId.SpringBallMovingRightPose or
        SamusPoseId.SpringBallFallingRightPose or
        SamusPoseId.SpringBallJumpRightPose => BottomSpringBallGroundRightBase,
        SamusPoseId.SpringBallGroundLeftPose or
        SamusPoseId.SpringBallMovingLeftPose or
        SamusPoseId.SpringBallFallingLeftPose or
        SamusPoseId.SpringBallJumpLeftPose => BottomSpringBallGroundLeftBase,
        SamusPoseId.ScrewAttackRightPose => BottomScrewAttackRightBase,
        SamusPoseId.ScrewAttackLeftPose => BottomScrewAttackLeftBase,
        SamusPoseId.WallJumpRightPose => BottomWallJumpRightBase,
        SamusPoseId.WallJumpLeftPose => BottomWallJumpLeftBase,
        SamusPoseId.CrouchingAimUpRightPose => BottomCrouchingAimUpRightBase,
        SamusPoseId.CrouchingAimUpLeftPose => BottomCrouchingAimUpLeftBase,
        SamusPoseId.TurningLeftToRightFallingPose or
        SamusPoseId.TurningLeftToRightFallingAimUpPose or
        SamusPoseId.TurningLeftToRightFallingAimDownPose or
        SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose => BottomTurningLeftToRightFallingBase,
        SamusPoseId.ForwardFacingSuitedPose => BottomForwardFacingSuitedBase,
        SamusPoseId.NormalLandingRightPose or
        SamusPoseId.LandingAimUpRightPose or
        SamusPoseId.LandingAimDiagonalUpRightPose or
        SamusPoseId.LandingAimDiagonalDownRightPose or
        SamusPoseId.FiringLandingRightPose => BottomNormalLandingRightBase,
        SamusPoseId.NormalLandingLeftPose or
        SamusPoseId.LandingAimUpLeftPose or
        SamusPoseId.LandingAimDiagonalUpLeftPose or
        SamusPoseId.LandingAimDiagonalDownLeftPose or
        SamusPoseId.FiringLandingLeftPose => BottomNormalLandingLeftBase,
        SamusPoseId.SpinLandingRightPose => BottomSpinLandingRightBase,
        SamusPoseId.SpinLandingLeftPose => BottomSpinLandingLeftBase,
        SamusPoseId.DraygonGrabbedNeutralLeftPose or
        SamusPoseId.DraygonGrabbedAimUpLeftPose or
        SamusPoseId.DraygonGrabbedFiringLeftPose or
        SamusPoseId.DraygonGrabbedAimDownLeftPose => BottomDraygonGrabbedNeutralLeftBase,
        SamusPoseId.DraygonGrabbedMovingLeftPose => BottomDraygonGrabbedMovingLeftBase,
        SamusPoseId.ShinesparkHorizontalRightPose => BottomShinesparkHorizontalRightBase,
        SamusPoseId.ShinesparkHorizontalLeftPose => BottomShinesparkHorizontalLeftBase,
        SamusPoseId.ShinesparkVerticalRightPose => BottomShinesparkVerticalRightBase,
        SamusPoseId.ShinesparkVerticalLeftPose => BottomShinesparkVerticalLeftBase,
        SamusPoseId.ShinesparkDiagonalRightPose => BottomShinesparkDiagonalRightBase,
        SamusPoseId.ShinesparkDiagonalLeftPose => BottomShinesparkDiagonalLeftBase,
        SamusPoseId.CrystalFlashRightPose => BottomCrystalFlashRightBase,
        SamusPoseId.CrystalFlashLeftPose => BottomCrystalFlashLeftBase,
        SamusPoseId.XrayingStandingRightPose => BottomXrayingStandingRightBase,
        SamusPoseId.XrayingStandingLeftPose => BottomXrayingStandingLeftBase,
        SamusPoseId.DeathSequenceRightPose => BottomDeathSequenceRightBase,
        SamusPoseId.DeathSequenceLeftPose => BottomDeathSequenceLeftBase,
        SamusPoseId.XrayingCrouchingRightPose => BottomXrayingCrouchingRightBase,
        SamusPoseId.XrayingCrouchingLeftPose => BottomXrayingCrouchingLeftBase,
        SamusPoseId.UnusedPoseDb => BottomUnusedPoseDbBase,
        SamusPoseId.UnusedPoseDc => BottomUnusedPoseDcBase,
        SamusPoseId.UnusedPoseDd => BottomUnusedPoseDdBase,
        SamusPoseId.UnusedPoseDe => BottomUnusedPoseDeBase,
        SamusPoseId.DrainedCrouchingRightPose => BottomDrainedCrouchingRightBase,
        SamusPoseId.DrainedCrouchingLeftPose => BottomDrainedCrouchingLeftBase,
        SamusPoseId.DrainedStandingRightPose => BottomDrainedStandingRightBase,
        SamusPoseId.DrainedStandingLeftPose => BottomDrainedStandingLeftBase,
        SamusPoseId.DraygonGrabbedNeutralRightPose or
        SamusPoseId.DraygonGrabbedAimUpRightPose or
        SamusPoseId.DraygonGrabbedFiringRightPose or
        SamusPoseId.DraygonGrabbedAimDownRightPose => BottomDraygonGrabbedNeutralRightBase,
        SamusPoseId.DraygonGrabbedMovingRightPose => BottomDraygonGrabbedMovingRightBase,
        SamusPoseId.AdjacentCodePoseFD or
            SamusPoseId.AdjacentCodePoseFE or
            SamusPoseId.AdjacentCodePoseFF => throw new ArgumentOutOfRangeException(nameof(pose)),
        var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
    };
}
