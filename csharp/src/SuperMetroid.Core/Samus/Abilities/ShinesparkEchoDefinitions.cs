using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Native identities for the ordinary projectile slots used by crash echoes.</summary>
public static class ShinesparkEchoDefinitions
{
    /// <summary>$90:D40D writes type $8029 before InitializeShinesparkEchoOrSpazerSba.</summary>
    public const ushort EchoType = 0x8029;
    /// <summary>
    /// $90:D4C6..D4D1, indexed by (pose - $C9) * 2: the crash's two echo angle indices. The six
    /// crash poses select an axis and its opposite.
    /// </summary>
    /// <remarks>
    /// The routine indexes with whatever pose is installed when the crash finishes. A drained
    /// controller ($91:E4F8) can replace the crash pose with $E8/$E9 while the crash keeps its
    /// movement handler, and those indices read on into the code of $90:D4D2: the bytes at
    /// $90:D504/$D505 and $90:D506/$D507.
    /// </remarks>
    internal static ShinesparkDepartureAngles DepartureAngles(SamusPoseId pose)
    {
        return pose switch
        {
            SamusPoseId.ShinesparkHorizontalRightPose => Departure(0x00, 0x80),
            SamusPoseId.ShinesparkHorizontalLeftPose => Departure(0x00, 0x80),
            SamusPoseId.ShinesparkVerticalRightPose => Departure(0x40, 0xc0),
            SamusPoseId.ShinesparkVerticalLeftPose => Departure(0x40, 0xc0),
            SamusPoseId.ShinesparkDiagonalRightPose => Departure(0xe0, 0x60),
            SamusPoseId.ShinesparkDiagonalLeftPose => Departure(0x20, 0xa0),
            SamusPoseId.DrainedCrouchingRightPose => Departure(0x16, 0x9d),
            SamusPoseId.DrainedCrouchingLeftPose => Departure(0xb6, 0x0a),
            SamusPoseId.ForwardFacingPowerSuitPose or
                SamusPoseId.ForwardFacingSuitedPose or
                SamusPoseId.FacingRightNormalPose or
                SamusPoseId.FacingLeftNormalPose or
                SamusPoseId.MovingRightNormalPose or
                SamusPoseId.MovingLeftNormalPose or
                SamusPoseId.MovingRightGunExtendedPose or
                SamusPoseId.MovingLeftGunExtendedPose or
                SamusPoseId.UnusedPose65 or
                SamusPoseId.UnusedPose66 or
                SamusPoseId.MoonwalkFacingLeftPose or
                SamusPoseId.MoonwalkFacingRightPose or
                SamusPoseId.MoonwalkAimUpLeftPose or
                SamusPoseId.MoonwalkAimUpRightPose or
                SamusPoseId.MoonwalkAimDownLeftPose or
                SamusPoseId.MoonwalkAimDownRightPose or
                SamusPoseId.MoonwalkTurnJumpLeftPose or
                SamusPoseId.MoonwalkTurnJumpRightPose or
                SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
                SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
                SamusPoseId.MoonwalkTurnJumpAimDownLeftPose or
                SamusPoseId.MoonwalkTurnJumpAimDownRightPose or
                SamusPoseId.ShinesparkWindupRightPose or
                SamusPoseId.ShinesparkWindupLeftPose or
                SamusPoseId.CrystalFlashRightPose or
                SamusPoseId.CrystalFlashLeftPose or
                SamusPoseId.XrayingStandingRightPose or
                SamusPoseId.XrayingStandingLeftPose or
                SamusPoseId.DeathSequenceRightPose or
                SamusPoseId.DeathSequenceLeftPose or
                SamusPoseId.XrayingCrouchingRightPose or
                SamusPoseId.XrayingCrouchingLeftPose or
                SamusPoseId.UnusedPoseDb or
                SamusPoseId.UnusedPoseDc or
                SamusPoseId.UnusedPoseDd or
                SamusPoseId.UnusedPoseDe or
                SamusPoseId.UnusedPoseDf or
                SamusPoseId.DrainedStandingRightPose or
                SamusPoseId.DrainedStandingLeftPose or
                SamusPoseId.RanIntoWallRightPose or
                SamusPoseId.RanIntoWallLeftPose or
                SamusPoseId.RanIntoWallAimUpRightPose or
                SamusPoseId.RanIntoWallAimUpLeftPose or
                SamusPoseId.RanIntoWallAimDownRightPose or
                SamusPoseId.RanIntoWallAimDownLeftPose or
                SamusPoseId.TurningRightToLeftPose or
                SamusPoseId.TurningLeftToRightPose or
                SamusPoseId.SpinJumpRightPose or
                SamusPoseId.SpinJumpLeftPose or
                SamusPoseId.SpaceJumpRightPose or
                SamusPoseId.SpaceJumpLeftPose or
                SamusPoseId.ScrewAttackRightPose or
                SamusPoseId.ScrewAttackLeftPose or
                SamusPoseId.TurningRightToLeftJumpPose or
                SamusPoseId.TurningLeftToRightJumpPose or
                SamusPoseId.WallJumpRightPose or
                SamusPoseId.WallJumpLeftPose or
                SamusPoseId.GrappleSwingRightPose or
                SamusPoseId.GrappleSwingLeftPose or
                SamusPoseId.GrappleStandingRightPose or
                SamusPoseId.GrappleStandingLeftPose or
                SamusPoseId.GrappleStandingDownRightPose or
                SamusPoseId.GrappleStandingDownLeftPose or
                SamusPoseId.GrappleCrouchingRightPose or
                SamusPoseId.GrappleCrouchingLeftPose or
                SamusPoseId.GrappleCrouchingDownRightPose or
                SamusPoseId.GrappleCrouchingDownLeftPose or
                SamusPoseId.GrappleWallContactLeftPose or
                SamusPoseId.GrappleWallContactRightPose or
                SamusPoseId.DraygonGrabbedNeutralLeftPose or
                SamusPoseId.DraygonGrabbedAimUpLeftPose or
                SamusPoseId.DraygonGrabbedFiringLeftPose or
                SamusPoseId.DraygonGrabbedAimDownLeftPose or
                SamusPoseId.DraygonGrabbedMovingLeftPose or
                SamusPoseId.DraygonGrabbedNeutralRightPose or
                SamusPoseId.DraygonGrabbedAimUpRightPose or
                SamusPoseId.DraygonGrabbedFiringRightPose or
                SamusPoseId.DraygonGrabbedAimDownRightPose or
                SamusPoseId.DraygonGrabbedMovingRightPose or
                SamusPoseId.DamageBoostLeftPose or
                SamusPoseId.DamageBoostRightPose or
                SamusPoseId.KnockbackRightPose or
                SamusPoseId.KnockbackLeftPose or
                SamusPoseId.TurningRightToLeftFallingPose or
                SamusPoseId.TurningLeftToRightFallingPose or
                SamusPoseId.TurningRightToLeftJumpAimUpPose or
                SamusPoseId.TurningLeftToRightJumpAimUpPose or
                SamusPoseId.TurningRightToLeftJumpAimDownPose or
                SamusPoseId.TurningLeftToRightJumpAimDownPose or
                SamusPoseId.TurningRightToLeftFallingAimUpPose or
                SamusPoseId.TurningLeftToRightFallingAimUpPose or
                SamusPoseId.TurningRightToLeftFallingAimDownPose or
                SamusPoseId.TurningLeftToRightFallingAimDownPose or
                SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
                SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or
                SamusPoseId.MorphBallGroundRightPose or
                SamusPoseId.MorphBallMovingRightPose or
                SamusPoseId.MorphBallMovingLeftPose or
                SamusPoseId.MorphBallFallingRightPose or
                SamusPoseId.MorphBallFallingLeftPose or
                SamusPoseId.UnusedKnockbackRightPose or
                SamusPoseId.UnusedKnockbackLeftPose or
                SamusPoseId.MorphingTransitionRightPose or
                SamusPoseId.MorphingTransitionLeftPose or
                SamusPoseId.UnmorphingTransitionRightPose or
                SamusPoseId.UnmorphingTransitionLeftPose or
                SamusPoseId.MorphBallGroundLeftPose or
                SamusPoseId.SpringBallGroundRightPose or
                SamusPoseId.SpringBallGroundLeftPose or
                SamusPoseId.SpringBallMovingRightPose or
                SamusPoseId.SpringBallMovingLeftPose or
                SamusPoseId.SpringBallFallingRightPose or
                SamusPoseId.SpringBallFallingLeftPose or
                SamusPoseId.SpringBallJumpRightPose or
                SamusPoseId.SpringBallJumpLeftPose or
                SamusPoseId.FallingRightPose or
                SamusPoseId.FallingLeftPose or
                SamusPoseId.FallingGunExtendedRightPose or
                SamusPoseId.FallingGunExtendedLeftPose or
                SamusPoseId.StandingAimUpRightPose or
                SamusPoseId.StandingAimUpLeftPose or
                SamusPoseId.StandingAimDiagonalUpRightPose or
                SamusPoseId.StandingAimDiagonalUpLeftPose or
                SamusPoseId.StandingAimDiagonalDownRightPose or
                SamusPoseId.StandingAimDiagonalDownLeftPose or
                SamusPoseId.RunningAimUpRightPose or
                SamusPoseId.RunningAimUpLeftPose or
                SamusPoseId.RunningAimDiagonalUpRightPose or
                SamusPoseId.RunningAimDiagonalUpLeftPose or
                SamusPoseId.RunningAimDiagonalDownRightPose or
                SamusPoseId.RunningAimDiagonalDownLeftPose or
                SamusPoseId.NormalJumpGunExtendedRightPose or
                SamusPoseId.NormalJumpGunExtendedLeftPose or
                SamusPoseId.NormalJumpAimUpRightPose or
                SamusPoseId.NormalJumpAimUpLeftPose or
                SamusPoseId.NormalJumpAimDownRightPose or
                SamusPoseId.NormalJumpAimDownLeftPose or
                SamusPoseId.NormalJumpForwardRightPose or
                SamusPoseId.NormalJumpForwardLeftPose or
                SamusPoseId.NormalJumpTransitionAimUpRightPose or
                SamusPoseId.NormalJumpTransitionAimUpLeftPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
                SamusPoseId.NormalJumpAimDiagonalUpRightPose or
                SamusPoseId.NormalJumpAimDiagonalUpLeftPose or
                SamusPoseId.NormalJumpAimDiagonalDownRightPose or
                SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
                SamusPoseId.FallingAimUpRightPose or
                SamusPoseId.FallingAimUpLeftPose or
                SamusPoseId.FallingAimDownRightPose or
                SamusPoseId.FallingAimDownLeftPose or
                SamusPoseId.FallingAimDiagonalUpRightPose or
                SamusPoseId.FallingAimDiagonalUpLeftPose or
                SamusPoseId.FallingAimDiagonalDownRightPose or
                SamusPoseId.FallingAimDiagonalDownLeftPose or
                SamusPoseId.CrouchingAimDiagonalUpRightPose or
                SamusPoseId.CrouchingAimDiagonalUpLeftPose or
                SamusPoseId.CrouchingAimDiagonalDownRightPose or
                SamusPoseId.CrouchingAimDiagonalDownLeftPose or
                SamusPoseId.CrouchingAimUpRightPose or
                SamusPoseId.CrouchingAimUpLeftPose or
                SamusPoseId.TurningRightToLeftCrouchingPose or
                SamusPoseId.TurningLeftToRightCrouchingPose or
                SamusPoseId.TurningRightToLeftAimUpPose or
                SamusPoseId.TurningLeftToRightAimUpPose or
                SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
                SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
                SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
                SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
                SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
                SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or
                SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or
                SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or
                SamusPoseId.LandingAimUpRightPose or
                SamusPoseId.LandingAimUpLeftPose or
                SamusPoseId.LandingAimDiagonalUpRightPose or
                SamusPoseId.LandingAimDiagonalUpLeftPose or
                SamusPoseId.LandingAimDiagonalDownRightPose or
                SamusPoseId.LandingAimDiagonalDownLeftPose or
                SamusPoseId.FiringLandingRightPose or
                SamusPoseId.FiringLandingLeftPose or
                SamusPoseId.CrouchingTransitionAimUpRightPose or
                SamusPoseId.CrouchingTransitionAimUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
                SamusPoseId.StandingTransitionAimUpRightPose or
                SamusPoseId.StandingTransitionAimUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalUpRightPose or
                SamusPoseId.StandingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalDownRightPose or
                SamusPoseId.StandingTransitionAimDiagonalDownLeftPose or
                SamusPoseId.CrouchingRightPose or
                SamusPoseId.CrouchingLeftPose or
                SamusPoseId.CrouchingTransitionRightPose or
                SamusPoseId.CrouchingTransitionLeftPose or
                SamusPoseId.StandingTransitionRightPose or
                SamusPoseId.StandingTransitionLeftPose or
                SamusPoseId.NeutralJumpTransitionRightPose or
                SamusPoseId.NeutralJumpTransitionLeftPose or
                SamusPoseId.NeutralJumpRightPose or
                SamusPoseId.NeutralJumpLeftPose or
                SamusPoseId.NormalLandingRightPose or
                SamusPoseId.NormalLandingLeftPose or
                SamusPoseId.SpinLandingRightPose or
                SamusPoseId.SpinLandingLeftPose or
                SamusPoseId.UnusedPose20 or
                SamusPoseId.UnusedPose21 or
                SamusPoseId.UnusedPose22 or
                SamusPoseId.UnusedPose23 or
                SamusPoseId.UnusedPose24 or
                SamusPoseId.UnusedPose39 or
                SamusPoseId.UnusedPose3A or
                SamusPoseId.UnusedPose3F or
                SamusPoseId.UnusedPose40 or
                SamusPoseId.UnusedPose42 or
                SamusPoseId.UnusedPose45 or
                SamusPoseId.UnusedPose46 or
                SamusPoseId.UnusedPose47 or
                SamusPoseId.UnusedPose48 or
                SamusPoseId.UnusedPose5B or
                SamusPoseId.UnusedPose5C or
                SamusPoseId.UnusedPose5D or
                SamusPoseId.UnusedPose5E or
                SamusPoseId.UnusedPose5F or
                SamusPoseId.UnusedPose60 or
                SamusPoseId.UnusedPose61 or
                SamusPoseId.UnusedPose62 or
                SamusPoseId.UnusedPose63 or
                SamusPoseId.UnusedPose64 or
                SamusPoseId.UnusedPoseAC or
                SamusPoseId.UnusedPoseAD or
                SamusPoseId.UnusedPoseAE or
                SamusPoseId.UnusedPoseAF or
                SamusPoseId.UnusedPoseB0 or
                SamusPoseId.UnusedPoseB1 or
                SamusPoseId.UnusedPoseC5 or
                SamusPoseId.UnusedPoseC6 => throw new InvalidOperationException(
                $"Shinespark crash finish has no modeled echo angles for pose ${(int)pose:X2}."),
            SamusPoseId.AdjacentCodePoseFD or
                SamusPoseId.AdjacentCodePoseFE or
                SamusPoseId.AdjacentCodePoseFF => throw new InvalidOperationException(
                $"Shinespark crash finish has no modeled echo angles for pose ${(int)pose:X2}."),
            var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
        };
    }

    /// <summary>The departure angles at two table indexes.</summary>
    private static ShinesparkDepartureAngles Departure(byte first, byte second) =>
        new ShinesparkDepartureAngles(SnesAngle.FromTableIndex(first), SnesAngle.FromTableIndex(second));
}
