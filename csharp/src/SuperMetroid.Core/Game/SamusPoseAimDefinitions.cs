namespace SuperMetroid.Core.Game;

/// <summary>Pose-specific cannon direction and firing restrictions, independent of editable artwork.</summary>
/// <remarks>
/// Poses 00..FC use semantic cases. FD..FF retain only the bounded byte observations
/// of the unrelated $91:BE11 X-ray HDMA machine-code routine. Those instruction encodings
/// have no managed pose-policy meaning; re-deriving them as pose data would be nonsense.
/// Exact addresses, instructions and the byte-only consumer contract are recorded in
/// docs/lookup-1165-stream-1.md, Batch 30. This exemption covers no ordinary pose value.
/// </remarks>
internal static class SamusPoseAimDefinitions
{
    /// <summary>$91:B62C shot-direction column: upper-aim turning restriction $FA, distinct from any firing direction.</summary>
    private const byte TurningUpperRestriction = 0xfa;
    /// <summary>$91:B62C shot-direction column: horizontal turning restriction $FB, including unused pose C6.</summary>
    private const byte TurningHorizontalRestriction = 0xfb;
    /// <summary>$91:B62C shot-direction column: lower-aim turning restriction $FC.</summary>
    private const byte TurningLowerRestriction = 0xfc;
    /// <summary>$91:B62C shot-direction column: $FF disallows shooting and cancels Grapple.</summary>
    private const byte ShootingDisabled = 0xff;
    /// <summary>Native PoseDefinitions column at $91:B62C with eight-byte stride; FD..FF preserve bounded adjacent instruction bytes.</summary>
    internal static byte Read(byte pose) => (SamusPoseId)pose switch
    {
        SamusPoseId.StandingAimUpRightPose or
        SamusPoseId.RunningAimUpRightPose or
        SamusPoseId.NormalJumpAimUpRightPose or
        SamusPoseId.FallingAimUpRightPose or
        SamusPoseId.NormalJumpTransitionAimUpRightPose or
        SamusPoseId.CrouchingAimUpRightPose or
        SamusPoseId.ShinesparkVerticalRightPose or
        SamusPoseId.LandingAimUpRightPose or
        SamusPoseId.CrouchingTransitionAimUpRightPose or
        SamusPoseId.StandingTransitionAimUpRightPose => (byte)SamusProjectileDirection.UpFacingRight,
        SamusPoseId.StandingAimDiagonalUpRightPose or
        SamusPoseId.RunningAimDiagonalUpRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
        SamusPoseId.NormalJumpAimDiagonalUpRightPose or
        SamusPoseId.FallingAimDiagonalUpRightPose or
        SamusPoseId.CrouchingAimDiagonalUpRightPose or
        SamusPoseId.MoonwalkAimUpRightPose or
        SamusPoseId.ShinesparkDiagonalRightPose or
        SamusPoseId.RanIntoWallAimUpRightPose or
        SamusPoseId.LandingAimDiagonalUpRightPose or
        SamusPoseId.DraygonGrabbedAimUpRightPose or
        SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
        SamusPoseId.StandingTransitionAimDiagonalUpRightPose => (byte)SamusProjectileDirection.UpRight,
        SamusPoseId.FacingRightNormalPose or
        SamusPoseId.MovingRightNormalPose or
        SamusPoseId.MovingRightGunExtendedPose or
        SamusPoseId.NormalJumpGunExtendedRightPose or
        SamusPoseId.CrouchingRightPose or
        SamusPoseId.FallingRightPose or
        SamusPoseId.CrouchingTransitionRightPose or
        SamusPoseId.StandingTransitionRightPose or
        SamusPoseId.UnusedPose46 or
        SamusPoseId.UnusedPose47 or
        SamusPoseId.MoonwalkFacingRightPose or
        SamusPoseId.NeutralJumpTransitionRightPose or
        SamusPoseId.NeutralJumpRightPose or
        SamusPoseId.NormalJumpForwardRightPose or
        SamusPoseId.FallingGunExtendedRightPose or
        SamusPoseId.RanIntoWallRightPose or
        SamusPoseId.NormalLandingRightPose or
        SamusPoseId.SpinLandingRightPose or
        SamusPoseId.GrappleStandingRightPose or
        SamusPoseId.UnusedPoseAC or
        SamusPoseId.GrappleCrouchingRightPose or
        SamusPoseId.ShinesparkHorizontalRightPose or
        SamusPoseId.XrayingStandingRightPose or
        SamusPoseId.DeathSequenceRightPose or
        SamusPoseId.XrayingCrouchingRightPose or
        SamusPoseId.FiringLandingRightPose or
        SamusPoseId.DraygonGrabbedNeutralRightPose or
        SamusPoseId.DraygonGrabbedFiringRightPose => (byte)SamusProjectileDirection.Right,
        SamusPoseId.StandingAimDiagonalDownRightPose or
        SamusPoseId.RunningAimDiagonalDownRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
        SamusPoseId.NormalJumpAimDiagonalDownRightPose or
        SamusPoseId.FallingAimDiagonalDownRightPose or
        SamusPoseId.CrouchingAimDiagonalDownRightPose or
        SamusPoseId.MoonwalkAimDownRightPose or
        SamusPoseId.GrappleStandingDownRightPose or
        SamusPoseId.UnusedPoseB0 or
        SamusPoseId.GrappleCrouchingDownRightPose or
        SamusPoseId.GrappleWallContactLeftPose or
        SamusPoseId.RanIntoWallAimDownRightPose or
        SamusPoseId.LandingAimDiagonalDownRightPose or
        SamusPoseId.DraygonGrabbedAimDownRightPose or
        SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
        SamusPoseId.StandingTransitionAimDiagonalDownRightPose => (byte)SamusProjectileDirection.DownRight,
        SamusPoseId.NormalJumpAimDownRightPose or
        SamusPoseId.FallingAimDownRightPose or
        SamusPoseId.UnusedPoseAE => (byte)SamusProjectileDirection.DownFacingRight,
        SamusPoseId.NormalJumpAimDownLeftPose or
        SamusPoseId.FallingAimDownLeftPose or
        SamusPoseId.UnusedPoseAF => (byte)SamusProjectileDirection.DownFacingLeft,
        SamusPoseId.StandingAimDiagonalDownLeftPose or
        SamusPoseId.RunningAimDiagonalDownLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
        SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
        SamusPoseId.FallingAimDiagonalDownLeftPose or
        SamusPoseId.CrouchingAimDiagonalDownLeftPose or
        SamusPoseId.MoonwalkAimDownLeftPose or
        SamusPoseId.GrappleStandingDownLeftPose or
        SamusPoseId.UnusedPoseB1 or
        SamusPoseId.GrappleCrouchingDownLeftPose or
        SamusPoseId.GrappleWallContactRightPose or
        SamusPoseId.DraygonGrabbedAimDownLeftPose or
        SamusPoseId.RanIntoWallAimDownLeftPose or
        SamusPoseId.LandingAimDiagonalDownLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => (byte)SamusProjectileDirection.DownLeft,
        SamusPoseId.FacingLeftNormalPose or
        SamusPoseId.MovingLeftNormalPose or
        SamusPoseId.MovingLeftGunExtendedPose or
        SamusPoseId.NormalJumpGunExtendedLeftPose or
        SamusPoseId.CrouchingLeftPose or
        SamusPoseId.FallingLeftPose or
        SamusPoseId.CrouchingTransitionLeftPose or
        SamusPoseId.StandingTransitionLeftPose or
        SamusPoseId.UnusedPose45 or
        SamusPoseId.UnusedPose48 or
        SamusPoseId.MoonwalkFacingLeftPose or
        SamusPoseId.NeutralJumpTransitionLeftPose or
        SamusPoseId.NeutralJumpLeftPose or
        SamusPoseId.NormalJumpForwardLeftPose or
        SamusPoseId.FallingGunExtendedLeftPose or
        SamusPoseId.RanIntoWallLeftPose or
        SamusPoseId.NormalLandingLeftPose or
        SamusPoseId.SpinLandingLeftPose or
        SamusPoseId.GrappleStandingLeftPose or
        SamusPoseId.UnusedPoseAD or
        SamusPoseId.GrappleCrouchingLeftPose or
        SamusPoseId.DraygonGrabbedNeutralLeftPose or
        SamusPoseId.DraygonGrabbedFiringLeftPose or
        SamusPoseId.ShinesparkHorizontalLeftPose or
        SamusPoseId.XrayingStandingLeftPose or
        SamusPoseId.DeathSequenceLeftPose or
        SamusPoseId.XrayingCrouchingLeftPose or
        SamusPoseId.FiringLandingLeftPose => (byte)SamusProjectileDirection.Left,
        SamusPoseId.StandingAimDiagonalUpLeftPose or
        SamusPoseId.RunningAimDiagonalUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
        SamusPoseId.NormalJumpAimDiagonalUpLeftPose or
        SamusPoseId.FallingAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingAimDiagonalUpLeftPose or
        SamusPoseId.MoonwalkAimUpLeftPose or
        SamusPoseId.DraygonGrabbedAimUpLeftPose or
        SamusPoseId.ShinesparkDiagonalLeftPose or
        SamusPoseId.RanIntoWallAimUpLeftPose or
        SamusPoseId.LandingAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalUpLeftPose => (byte)SamusProjectileDirection.UpLeft,
        SamusPoseId.StandingAimUpLeftPose or
        SamusPoseId.RunningAimUpLeftPose or
        SamusPoseId.NormalJumpAimUpLeftPose or
        SamusPoseId.FallingAimUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimUpLeftPose or
        SamusPoseId.CrouchingAimUpLeftPose or
        SamusPoseId.ShinesparkVerticalLeftPose or
        SamusPoseId.LandingAimUpLeftPose or
        SamusPoseId.CrouchingTransitionAimUpLeftPose or
        SamusPoseId.StandingTransitionAimUpLeftPose => (byte)SamusProjectileDirection.UpFacingLeft,
        SamusPoseId.TurningRightToLeftAimUpPose or
        SamusPoseId.TurningLeftToRightAimUpPose or
        SamusPoseId.TurningRightToLeftJumpAimUpPose or
        SamusPoseId.TurningLeftToRightJumpAimUpPose or
        SamusPoseId.TurningRightToLeftFallingAimUpPose or
        SamusPoseId.TurningLeftToRightFallingAimUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
        SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or
        SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
        SamusPoseId.MoonwalkTurnJumpAimUpRightPose => TurningUpperRestriction,
        SamusPoseId.TurningRightToLeftPose or
        SamusPoseId.TurningLeftToRightPose or
        SamusPoseId.TurningRightToLeftJumpPose or
        SamusPoseId.TurningLeftToRightJumpPose or
        SamusPoseId.TurningRightToLeftCrouchingPose or
        SamusPoseId.TurningLeftToRightCrouchingPose or
        SamusPoseId.TurningRightToLeftFallingPose or
        SamusPoseId.TurningLeftToRightFallingPose or
        SamusPoseId.MoonwalkTurnJumpLeftPose or
        SamusPoseId.MoonwalkTurnJumpRightPose or
        SamusPoseId.UnusedPoseC6 => TurningHorizontalRestriction,
        SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
        SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
        SamusPoseId.TurningRightToLeftJumpAimDownPose or
        SamusPoseId.TurningLeftToRightJumpAimDownPose or
        SamusPoseId.TurningRightToLeftFallingAimDownPose or
        SamusPoseId.TurningLeftToRightFallingAimDownPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or
        SamusPoseId.MoonwalkTurnJumpAimDownLeftPose or
        SamusPoseId.MoonwalkTurnJumpAimDownRightPose => TurningLowerRestriction,
        SamusPoseId.ForwardFacingPowerSuitPose or
        SamusPoseId.SpinJumpRightPose or
        SamusPoseId.SpinJumpLeftPose or
        SamusPoseId.SpaceJumpRightPose or
        SamusPoseId.SpaceJumpLeftPose or
        SamusPoseId.MorphBallGroundRightPose or
        SamusPoseId.MorphBallMovingRightPose or
        SamusPoseId.MorphBallMovingLeftPose or
        SamusPoseId.UnusedPose20 or
        SamusPoseId.UnusedPose21 or
        SamusPoseId.UnusedPose22 or
        SamusPoseId.UnusedPose23 or
        SamusPoseId.UnusedPose24 or
        SamusPoseId.MorphBallFallingRightPose or
        SamusPoseId.MorphBallFallingLeftPose or
        SamusPoseId.UnusedKnockbackRightPose or
        SamusPoseId.UnusedKnockbackLeftPose or
        SamusPoseId.MorphingTransitionRightPose or
        SamusPoseId.MorphingTransitionLeftPose or
        SamusPoseId.UnusedPose39 or
        SamusPoseId.UnusedPose3A or
        SamusPoseId.UnmorphingTransitionRightPose or
        SamusPoseId.UnmorphingTransitionLeftPose or
        SamusPoseId.UnusedPose3F or
        SamusPoseId.UnusedPose40 or
        SamusPoseId.MorphBallGroundLeftPose or
        SamusPoseId.UnusedPose42 or
        SamusPoseId.DamageBoostLeftPose or
        SamusPoseId.DamageBoostRightPose or
        SamusPoseId.KnockbackRightPose or
        SamusPoseId.KnockbackLeftPose or
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
        SamusPoseId.UnusedPose65 or
        SamusPoseId.UnusedPose66 or
        SamusPoseId.SpringBallGroundRightPose or
        SamusPoseId.SpringBallGroundLeftPose or
        SamusPoseId.SpringBallMovingRightPose or
        SamusPoseId.SpringBallMovingLeftPose or
        SamusPoseId.SpringBallFallingRightPose or
        SamusPoseId.SpringBallFallingLeftPose or
        SamusPoseId.SpringBallJumpRightPose or
        SamusPoseId.SpringBallJumpLeftPose or
        SamusPoseId.ScrewAttackRightPose or
        SamusPoseId.ScrewAttackLeftPose or
        SamusPoseId.WallJumpRightPose or
        SamusPoseId.WallJumpLeftPose or
        SamusPoseId.ForwardFacingSuitedPose or
        SamusPoseId.GrappleSwingRightPose or
        SamusPoseId.GrappleSwingLeftPose or
        SamusPoseId.DraygonGrabbedMovingLeftPose or
        SamusPoseId.UnusedPoseC5 or
        SamusPoseId.ShinesparkWindupRightPose or
        SamusPoseId.ShinesparkWindupLeftPose or
        SamusPoseId.CrystalFlashRightPose or
        SamusPoseId.CrystalFlashLeftPose or
        SamusPoseId.UnusedPoseDb or
        SamusPoseId.UnusedPoseDc or
        SamusPoseId.UnusedPoseDd or
        SamusPoseId.UnusedPoseDe or
        SamusPoseId.UnusedPoseDf or
        SamusPoseId.DrainedCrouchingRightPose or
        SamusPoseId.DrainedCrouchingLeftPose or
        SamusPoseId.DrainedStandingRightPose or
        SamusPoseId.DrainedStandingLeftPose or
        SamusPoseId.DraygonGrabbedMovingRightPose => ShootingDisabled,
        // These byte indices observe Calc_Xray_HDMADataTable_OffScreen, not additional poses.
        (SamusPoseId)0xfd => 171,
        (SamusPoseId)0xfe => 133,
        (SamusPoseId)0xff => 20,
    };
}
