namespace SuperMetroid.Core.Game;

/// <summary>Pose-specific facing, movement and idle transitions, independent of editable artwork.</summary>
/// <remarks>
/// Poses 00..FC use semantic cases. FD..FF retain only the bounded byte observations
/// of the unrelated $91:BE11 X-ray HDMA machine-code routine. Those instruction encodings
/// have no managed pose-policy meaning; re-deriving them as pose data would be nonsense.
/// Exact addresses, instructions and the byte-only consumer contract were reviewed in
/// #1165 (stream 1, Batch 30). This exemption covers no ordinary pose value.
/// </remarks>
internal static class SamusPoseDispatchDefinitions
{
    /// <summary>Native PoseDefinitions Facing selection, $91:B629 record stride eight; FD..FF are bounded adjacent code observations.</summary>
    internal static byte ReadFacing(SamusPoseId pose) => (SamusPoseId)pose switch
    {
        SamusPoseId.ForwardFacingPowerSuitPose or
        SamusPoseId.ForwardFacingSuitedPose => (byte)SamusFacingDirection.ForwardOrSpecial,
        // The unused glitch-ball records use distinct native discriminators, not left/right.
        SamusPoseId.UnusedPose21 => 1,
        SamusPoseId.UnusedPose22 => 2,
        SamusPoseId.FacingLeftNormalPose or
        SamusPoseId.StandingAimUpLeftPose or
        SamusPoseId.StandingAimDiagonalUpLeftPose or
        SamusPoseId.StandingAimDiagonalDownLeftPose or
        SamusPoseId.MovingLeftNormalPose or
        SamusPoseId.MovingLeftGunExtendedPose or
        SamusPoseId.RunningAimUpLeftPose or
        SamusPoseId.RunningAimDiagonalUpLeftPose or
        SamusPoseId.RunningAimDiagonalDownLeftPose or
        SamusPoseId.NormalJumpGunExtendedLeftPose or
        SamusPoseId.NormalJumpAimUpLeftPose or
        SamusPoseId.NormalJumpAimDownLeftPose or
        SamusPoseId.SpinJumpLeftPose or
        SamusPoseId.SpaceJumpLeftPose or
        SamusPoseId.MorphBallMovingLeftPose or
        SamusPoseId.UnusedPose23 or
        SamusPoseId.TurningRightToLeftPose or
        SamusPoseId.CrouchingLeftPose or
        SamusPoseId.FallingLeftPose or
        SamusPoseId.FallingAimUpLeftPose or
        SamusPoseId.FallingAimDownLeftPose or
        SamusPoseId.TurningRightToLeftJumpPose or
        SamusPoseId.MorphBallFallingLeftPose or
        SamusPoseId.UnusedKnockbackLeftPose or
        SamusPoseId.CrouchingTransitionLeftPose or
        SamusPoseId.MorphingTransitionLeftPose or
        SamusPoseId.UnusedPose3A or
        SamusPoseId.StandingTransitionLeftPose or
        SamusPoseId.UnmorphingTransitionLeftPose or
        SamusPoseId.UnusedPose40 or
        SamusPoseId.MorphBallGroundLeftPose or
        SamusPoseId.UnusedPose42 or
        SamusPoseId.TurningRightToLeftCrouchingPose or
        SamusPoseId.UnusedPose46 or
        SamusPoseId.UnusedPose48 or
        SamusPoseId.MoonwalkFacingRightPose or
        SamusPoseId.NeutralJumpTransitionLeftPose or
        SamusPoseId.NeutralJumpLeftPose or
        SamusPoseId.DamageBoostRightPose or
        SamusPoseId.NormalJumpForwardLeftPose or
        SamusPoseId.KnockbackLeftPose or
        SamusPoseId.NormalJumpTransitionAimUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
        SamusPoseId.UnusedPose5C or
        SamusPoseId.UnusedPose5E or
        SamusPoseId.UnusedPose60 or
        SamusPoseId.UnusedPose62 or
        SamusPoseId.UnusedPose64 or
        SamusPoseId.UnusedPose66 or
        SamusPoseId.FallingGunExtendedLeftPose or
        SamusPoseId.NormalJumpAimDiagonalUpLeftPose or
        SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
        SamusPoseId.FallingAimDiagonalUpLeftPose or
        SamusPoseId.FallingAimDiagonalDownLeftPose or
        SamusPoseId.CrouchingAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingAimDiagonalDownLeftPose or
        SamusPoseId.MoonwalkAimUpRightPose or
        SamusPoseId.MoonwalkAimDownRightPose or
        SamusPoseId.SpringBallGroundLeftPose or
        SamusPoseId.SpringBallMovingLeftPose or
        SamusPoseId.SpringBallFallingLeftPose or
        SamusPoseId.SpringBallJumpLeftPose or
        SamusPoseId.ScrewAttackLeftPose or
        SamusPoseId.WallJumpLeftPose or
        SamusPoseId.CrouchingAimUpLeftPose or
        SamusPoseId.TurningRightToLeftFallingPose or
        SamusPoseId.RanIntoWallLeftPose or
        SamusPoseId.TurningRightToLeftAimUpPose or
        SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
        SamusPoseId.TurningRightToLeftJumpAimUpPose or
        SamusPoseId.TurningRightToLeftJumpAimDownPose or
        SamusPoseId.TurningRightToLeftFallingAimUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDownPose or
        SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or
        SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or
        SamusPoseId.NormalLandingLeftPose or
        SamusPoseId.SpinLandingLeftPose or
        SamusPoseId.GrappleStandingLeftPose or
        SamusPoseId.GrappleStandingDownLeftPose or
        SamusPoseId.UnusedPoseAD or
        SamusPoseId.UnusedPoseAF or
        SamusPoseId.UnusedPoseB1 or
        SamusPoseId.GrappleSwingLeftPose or
        SamusPoseId.GrappleCrouchingLeftPose or
        SamusPoseId.GrappleCrouchingDownLeftPose or
        SamusPoseId.GrappleWallContactRightPose or
        SamusPoseId.DraygonGrabbedNeutralLeftPose or
        SamusPoseId.DraygonGrabbedAimUpLeftPose or
        SamusPoseId.DraygonGrabbedFiringLeftPose or
        SamusPoseId.DraygonGrabbedAimDownLeftPose or
        SamusPoseId.DraygonGrabbedMovingLeftPose or
        SamusPoseId.MoonwalkTurnJumpLeftPose or
        SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
        SamusPoseId.MoonwalkTurnJumpAimDownLeftPose or
        SamusPoseId.UnusedPoseC5 or
        SamusPoseId.UnusedPoseC6 or
        SamusPoseId.ShinesparkWindupLeftPose or
        SamusPoseId.ShinesparkHorizontalLeftPose or
        SamusPoseId.ShinesparkVerticalLeftPose or
        SamusPoseId.ShinesparkDiagonalLeftPose or
        SamusPoseId.RanIntoWallAimUpLeftPose or
        SamusPoseId.RanIntoWallAimDownLeftPose or
        SamusPoseId.CrystalFlashLeftPose or
        SamusPoseId.XrayingStandingLeftPose or
        SamusPoseId.DeathSequenceLeftPose or
        SamusPoseId.XrayingCrouchingLeftPose or
        SamusPoseId.UnusedPoseDc or
        SamusPoseId.UnusedPoseDe or
        SamusPoseId.UnusedPoseDf or
        SamusPoseId.LandingAimUpLeftPose or
        SamusPoseId.LandingAimDiagonalUpLeftPose or
        SamusPoseId.LandingAimDiagonalDownLeftPose or
        SamusPoseId.FiringLandingLeftPose or
        SamusPoseId.DrainedCrouchingLeftPose or
        SamusPoseId.DrainedStandingLeftPose or
        SamusPoseId.CrouchingTransitionAimUpLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
        SamusPoseId.StandingTransitionAimUpLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalUpLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => (byte)SamusFacingDirection.Left,
        SamusPoseId.FacingRightNormalPose or
        SamusPoseId.StandingAimUpRightPose or
        SamusPoseId.StandingAimDiagonalUpRightPose or
        SamusPoseId.StandingAimDiagonalDownRightPose or
        SamusPoseId.MovingRightNormalPose or
        SamusPoseId.MovingRightGunExtendedPose or
        SamusPoseId.RunningAimUpRightPose or
        SamusPoseId.RunningAimDiagonalUpRightPose or
        SamusPoseId.RunningAimDiagonalDownRightPose or
        SamusPoseId.NormalJumpGunExtendedRightPose or
        SamusPoseId.NormalJumpAimUpRightPose or
        SamusPoseId.NormalJumpAimDownRightPose or
        SamusPoseId.SpinJumpRightPose or
        SamusPoseId.SpaceJumpRightPose or
        SamusPoseId.MorphBallGroundRightPose or
        SamusPoseId.MorphBallMovingRightPose or
        SamusPoseId.UnusedPose20 or
        SamusPoseId.UnusedPose24 or
        SamusPoseId.TurningLeftToRightPose or
        SamusPoseId.CrouchingRightPose or
        SamusPoseId.FallingRightPose or
        SamusPoseId.FallingAimUpRightPose or
        SamusPoseId.FallingAimDownRightPose or
        SamusPoseId.TurningLeftToRightJumpPose or
        SamusPoseId.MorphBallFallingRightPose or
        SamusPoseId.UnusedKnockbackRightPose or
        SamusPoseId.CrouchingTransitionRightPose or
        SamusPoseId.MorphingTransitionRightPose or
        SamusPoseId.UnusedPose39 or
        SamusPoseId.StandingTransitionRightPose or
        SamusPoseId.UnmorphingTransitionRightPose or
        SamusPoseId.UnusedPose3F or
        SamusPoseId.TurningLeftToRightCrouchingPose or
        SamusPoseId.UnusedPose45 or
        SamusPoseId.UnusedPose47 or
        SamusPoseId.MoonwalkFacingLeftPose or
        SamusPoseId.NeutralJumpTransitionRightPose or
        SamusPoseId.NeutralJumpRightPose or
        SamusPoseId.DamageBoostLeftPose or
        SamusPoseId.NormalJumpForwardRightPose or
        SamusPoseId.KnockbackRightPose or
        SamusPoseId.NormalJumpTransitionAimUpRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
        SamusPoseId.UnusedPose5B or
        SamusPoseId.UnusedPose5D or
        SamusPoseId.UnusedPose5F or
        SamusPoseId.UnusedPose61 or
        SamusPoseId.UnusedPose63 or
        SamusPoseId.UnusedPose65 or
        SamusPoseId.FallingGunExtendedRightPose or
        SamusPoseId.NormalJumpAimDiagonalUpRightPose or
        SamusPoseId.NormalJumpAimDiagonalDownRightPose or
        SamusPoseId.FallingAimDiagonalUpRightPose or
        SamusPoseId.FallingAimDiagonalDownRightPose or
        SamusPoseId.CrouchingAimDiagonalUpRightPose or
        SamusPoseId.CrouchingAimDiagonalDownRightPose or
        SamusPoseId.MoonwalkAimUpLeftPose or
        SamusPoseId.MoonwalkAimDownLeftPose or
        SamusPoseId.SpringBallGroundRightPose or
        SamusPoseId.SpringBallMovingRightPose or
        SamusPoseId.SpringBallFallingRightPose or
        SamusPoseId.SpringBallJumpRightPose or
        SamusPoseId.ScrewAttackRightPose or
        SamusPoseId.WallJumpRightPose or
        SamusPoseId.CrouchingAimUpRightPose or
        SamusPoseId.TurningLeftToRightFallingPose or
        SamusPoseId.RanIntoWallRightPose or
        SamusPoseId.TurningLeftToRightAimUpPose or
        SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
        SamusPoseId.TurningLeftToRightJumpAimUpPose or
        SamusPoseId.TurningLeftToRightJumpAimDownPose or
        SamusPoseId.TurningLeftToRightFallingAimUpPose or
        SamusPoseId.TurningLeftToRightFallingAimDownPose or
        SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or
        SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or
        SamusPoseId.NormalLandingRightPose or
        SamusPoseId.SpinLandingRightPose or
        SamusPoseId.GrappleStandingRightPose or
        SamusPoseId.GrappleStandingDownRightPose or
        SamusPoseId.UnusedPoseAC or
        SamusPoseId.UnusedPoseAE or
        SamusPoseId.UnusedPoseB0 or
        SamusPoseId.GrappleSwingRightPose or
        SamusPoseId.GrappleCrouchingRightPose or
        SamusPoseId.GrappleCrouchingDownRightPose or
        SamusPoseId.GrappleWallContactLeftPose or
        SamusPoseId.MoonwalkTurnJumpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimDownRightPose or
        SamusPoseId.ShinesparkWindupRightPose or
        SamusPoseId.ShinesparkHorizontalRightPose or
        SamusPoseId.ShinesparkVerticalRightPose or
        SamusPoseId.ShinesparkDiagonalRightPose or
        SamusPoseId.RanIntoWallAimUpRightPose or
        SamusPoseId.RanIntoWallAimDownRightPose or
        SamusPoseId.CrystalFlashRightPose or
        SamusPoseId.XrayingStandingRightPose or
        SamusPoseId.DeathSequenceRightPose or
        SamusPoseId.XrayingCrouchingRightPose or
        SamusPoseId.UnusedPoseDb or
        SamusPoseId.UnusedPoseDd or
        SamusPoseId.LandingAimUpRightPose or
        SamusPoseId.LandingAimDiagonalUpRightPose or
        SamusPoseId.LandingAimDiagonalDownRightPose or
        SamusPoseId.FiringLandingRightPose or
        SamusPoseId.DrainedCrouchingRightPose or
        SamusPoseId.DrainedStandingRightPose or
        SamusPoseId.DraygonGrabbedNeutralRightPose or
        SamusPoseId.DraygonGrabbedAimUpRightPose or
        SamusPoseId.DraygonGrabbedFiringRightPose or
        SamusPoseId.DraygonGrabbedAimDownRightPose or
        SamusPoseId.DraygonGrabbedMovingRightPose or
        SamusPoseId.CrouchingTransitionAimUpRightPose or
        SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
        SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
        SamusPoseId.StandingTransitionAimUpRightPose or
        SamusPoseId.StandingTransitionAimDiagonalUpRightPose or
        SamusPoseId.StandingTransitionAimDiagonalDownRightPose => (byte)SamusFacingDirection.Right,
        // Bounded indices beyond FC observe Calc_Xray_HDMADataTable_OffScreen opcodes.
        (SamusPoseId)0xfd => 8,
        (SamusPoseId)0xfe => 41,
        (SamusPoseId)0xff => 18,
    };

    /// <summary>Native PoseDefinitions Movement selection, $91:B62A record stride eight; FD..FF are bounded adjacent code observations.</summary>
    internal static byte ReadMovement(SamusPoseId pose) => (SamusPoseId)pose switch
    {
        SamusPoseId.ForwardFacingPowerSuitPose or
        SamusPoseId.FacingRightNormalPose or
        SamusPoseId.FacingLeftNormalPose or
        SamusPoseId.StandingAimUpRightPose or
        SamusPoseId.StandingAimUpLeftPose or
        SamusPoseId.StandingAimDiagonalUpRightPose or
        SamusPoseId.StandingAimDiagonalUpLeftPose or
        SamusPoseId.StandingAimDiagonalDownRightPose or
        SamusPoseId.StandingAimDiagonalDownLeftPose or
        SamusPoseId.UnusedPose47 or
        SamusPoseId.UnusedPose48 or
        SamusPoseId.ForwardFacingSuitedPose or
        SamusPoseId.NormalLandingRightPose or
        SamusPoseId.NormalLandingLeftPose or
        SamusPoseId.SpinLandingRightPose or
        SamusPoseId.SpinLandingLeftPose or
        SamusPoseId.XrayingStandingRightPose or
        SamusPoseId.XrayingStandingLeftPose or
        SamusPoseId.LandingAimUpRightPose or
        SamusPoseId.LandingAimUpLeftPose or
        SamusPoseId.LandingAimDiagonalUpRightPose or
        SamusPoseId.LandingAimDiagonalUpLeftPose or
        SamusPoseId.LandingAimDiagonalDownRightPose or
        SamusPoseId.LandingAimDiagonalDownLeftPose or
        SamusPoseId.FiringLandingRightPose or
        SamusPoseId.FiringLandingLeftPose => (byte)SamusMovementType.Standing,
        SamusPoseId.MovingRightNormalPose or
        SamusPoseId.MovingLeftNormalPose or
        SamusPoseId.MovingRightGunExtendedPose or
        SamusPoseId.MovingLeftGunExtendedPose or
        SamusPoseId.RunningAimUpRightPose or
        SamusPoseId.RunningAimUpLeftPose or
        SamusPoseId.RunningAimDiagonalUpRightPose or
        SamusPoseId.RunningAimDiagonalUpLeftPose or
        SamusPoseId.RunningAimDiagonalDownRightPose or
        SamusPoseId.RunningAimDiagonalDownLeftPose or
        SamusPoseId.UnusedPose45 or
        SamusPoseId.UnusedPose46 => (byte)SamusMovementType.Running,
        SamusPoseId.NormalJumpGunExtendedRightPose or
        SamusPoseId.NormalJumpGunExtendedLeftPose or
        SamusPoseId.NormalJumpAimUpRightPose or
        SamusPoseId.NormalJumpAimUpLeftPose or
        SamusPoseId.NormalJumpAimDownRightPose or
        SamusPoseId.NormalJumpAimDownLeftPose or
        SamusPoseId.NeutralJumpTransitionRightPose or
        SamusPoseId.NeutralJumpTransitionLeftPose or
        SamusPoseId.NeutralJumpRightPose or
        SamusPoseId.NeutralJumpLeftPose or
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
        SamusPoseId.NormalJumpAimDiagonalDownLeftPose => (byte)SamusMovementType.NormalJumping,
        SamusPoseId.SpinJumpRightPose or
        SamusPoseId.SpinJumpLeftPose or
        SamusPoseId.SpaceJumpRightPose or
        SamusPoseId.SpaceJumpLeftPose or
        SamusPoseId.ScrewAttackRightPose or
        SamusPoseId.ScrewAttackLeftPose => (byte)SamusMovementType.SpinJumping,
        SamusPoseId.MorphBallGroundRightPose or
        SamusPoseId.MorphBallMovingRightPose or
        SamusPoseId.MorphBallMovingLeftPose or
        SamusPoseId.MorphBallGroundLeftPose => (byte)SamusMovementType.MorphBallGround,
        SamusPoseId.CrouchingRightPose or
        SamusPoseId.CrouchingLeftPose or
        SamusPoseId.CrouchingAimDiagonalUpRightPose or
        SamusPoseId.CrouchingAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingAimDiagonalDownRightPose or
        SamusPoseId.CrouchingAimDiagonalDownLeftPose or
        SamusPoseId.CrouchingAimUpRightPose or
        SamusPoseId.CrouchingAimUpLeftPose or
        SamusPoseId.XrayingCrouchingRightPose or
        SamusPoseId.XrayingCrouchingLeftPose => (byte)SamusMovementType.Crouching,
        SamusPoseId.FallingRightPose or
        SamusPoseId.FallingLeftPose or
        SamusPoseId.FallingAimUpRightPose or
        SamusPoseId.FallingAimUpLeftPose or
        SamusPoseId.FallingAimDownRightPose or
        SamusPoseId.FallingAimDownLeftPose or
        SamusPoseId.FallingGunExtendedRightPose or
        SamusPoseId.FallingGunExtendedLeftPose or
        SamusPoseId.FallingAimDiagonalUpRightPose or
        SamusPoseId.FallingAimDiagonalUpLeftPose or
        SamusPoseId.FallingAimDiagonalDownRightPose or
        SamusPoseId.FallingAimDiagonalDownLeftPose => (byte)SamusMovementType.Falling,
        SamusPoseId.UnusedPose20 or
        SamusPoseId.UnusedPose21 or
        SamusPoseId.UnusedPose22 or
        SamusPoseId.UnusedPose23 or
        SamusPoseId.UnusedPose24 or
        SamusPoseId.UnusedPose42 => (byte)SamusMovementType.UnusedGlitchBall,
        SamusPoseId.MorphBallFallingRightPose or
        SamusPoseId.MorphBallFallingLeftPose => (byte)SamusMovementType.MorphBallFalling,
        SamusPoseId.UnusedKnockbackRightPose or
        SamusPoseId.UnusedKnockbackLeftPose => (byte)SamusMovementType.UnusedGlitchBallAlternate,
        SamusPoseId.KnockbackRightPose or
        SamusPoseId.KnockbackLeftPose or
        SamusPoseId.DeathSequenceRightPose or
        SamusPoseId.DeathSequenceLeftPose => (byte)SamusMovementType.Knockback,
        SamusPoseId.UnusedPose5D or
        SamusPoseId.UnusedPose5E or
        SamusPoseId.UnusedPose5F or
        SamusPoseId.UnusedPose60 => (byte)SamusMovementType.Unused0B,
        SamusPoseId.UnusedPose63 or
        SamusPoseId.UnusedPose64 or
        SamusPoseId.UnusedPose65 or
        SamusPoseId.UnusedPose66 => (byte)SamusMovementType.Unused0D,
        SamusPoseId.TurningRightToLeftPose or
        SamusPoseId.TurningLeftToRightPose or
        SamusPoseId.TurningRightToLeftCrouchingPose or
        SamusPoseId.TurningLeftToRightCrouchingPose or
        SamusPoseId.TurningRightToLeftAimUpPose or
        SamusPoseId.TurningLeftToRightAimUpPose or
        SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
        SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
        SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
        SamusPoseId.MoonwalkTurnJumpLeftPose or
        SamusPoseId.MoonwalkTurnJumpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
        SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimDownLeftPose or
        SamusPoseId.MoonwalkTurnJumpAimDownRightPose => (byte)SamusMovementType.TurningOnGround,
        SamusPoseId.CrouchingTransitionRightPose or
        SamusPoseId.CrouchingTransitionLeftPose or
        SamusPoseId.MorphingTransitionRightPose or
        SamusPoseId.MorphingTransitionLeftPose or
        SamusPoseId.UnusedPose39 or
        SamusPoseId.UnusedPose3A or
        SamusPoseId.StandingTransitionRightPose or
        SamusPoseId.StandingTransitionLeftPose or
        SamusPoseId.UnmorphingTransitionRightPose or
        SamusPoseId.UnmorphingTransitionLeftPose or
        SamusPoseId.UnusedPose3F or
        SamusPoseId.UnusedPose40 or
        SamusPoseId.UnusedPoseDb or
        SamusPoseId.UnusedPoseDc or
        SamusPoseId.UnusedPoseDd or
        SamusPoseId.UnusedPoseDe or
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
        SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => (byte)SamusMovementType.PostureTransition,
        SamusPoseId.MoonwalkFacingLeftPose or
        SamusPoseId.MoonwalkFacingRightPose or
        SamusPoseId.MoonwalkAimUpLeftPose or
        SamusPoseId.MoonwalkAimUpRightPose or
        SamusPoseId.MoonwalkAimDownLeftPose or
        SamusPoseId.MoonwalkAimDownRightPose => (byte)SamusMovementType.Moonwalking,
        SamusPoseId.SpringBallGroundRightPose or
        SamusPoseId.SpringBallGroundLeftPose or
        SamusPoseId.SpringBallMovingRightPose or
        SamusPoseId.SpringBallMovingLeftPose => (byte)SamusMovementType.SpringBallGround,
        SamusPoseId.SpringBallJumpRightPose or
        SamusPoseId.SpringBallJumpLeftPose => (byte)SamusMovementType.SpringBallInAir,
        SamusPoseId.SpringBallFallingRightPose or
        SamusPoseId.SpringBallFallingLeftPose => (byte)SamusMovementType.SpringBallFalling,
        SamusPoseId.WallJumpRightPose or
        SamusPoseId.WallJumpLeftPose => (byte)SamusMovementType.WallJumping,
        SamusPoseId.RanIntoWallRightPose or
        SamusPoseId.RanIntoWallLeftPose or
        SamusPoseId.RanIntoWallAimUpRightPose or
        SamusPoseId.RanIntoWallAimUpLeftPose or
        SamusPoseId.RanIntoWallAimDownRightPose or
        SamusPoseId.RanIntoWallAimDownLeftPose => (byte)SamusMovementType.RanIntoWall,
        SamusPoseId.UnusedPose5B or
        SamusPoseId.UnusedPose5C or
        SamusPoseId.UnusedPose61 or
        SamusPoseId.UnusedPose62 or
        SamusPoseId.GrappleStandingRightPose or
        SamusPoseId.GrappleStandingLeftPose or
        SamusPoseId.GrappleStandingDownRightPose or
        SamusPoseId.GrappleStandingDownLeftPose or
        SamusPoseId.UnusedPoseAC or
        SamusPoseId.UnusedPoseAD or
        SamusPoseId.UnusedPoseAE or
        SamusPoseId.UnusedPoseAF or
        SamusPoseId.UnusedPoseB0 or
        SamusPoseId.UnusedPoseB1 or
        SamusPoseId.GrappleSwingRightPose or
        SamusPoseId.GrappleSwingLeftPose or
        SamusPoseId.GrappleCrouchingRightPose or
        SamusPoseId.GrappleCrouchingLeftPose or
        SamusPoseId.GrappleCrouchingDownRightPose or
        SamusPoseId.GrappleCrouchingDownLeftPose or
        SamusPoseId.GrappleWallContactLeftPose or
        SamusPoseId.GrappleWallContactRightPose => (byte)SamusMovementType.Grappling,
        SamusPoseId.TurningRightToLeftJumpPose or
        SamusPoseId.TurningLeftToRightJumpPose or
        SamusPoseId.TurningRightToLeftJumpAimUpPose or
        SamusPoseId.TurningLeftToRightJumpAimUpPose or
        SamusPoseId.TurningRightToLeftJumpAimDownPose or
        SamusPoseId.TurningLeftToRightJumpAimDownPose or
        SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or
        SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose => (byte)SamusMovementType.TurningWhileJumping,
        SamusPoseId.TurningRightToLeftFallingPose or
        SamusPoseId.TurningLeftToRightFallingPose or
        SamusPoseId.TurningRightToLeftFallingAimUpPose or
        SamusPoseId.TurningLeftToRightFallingAimUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDownPose or
        SamusPoseId.TurningLeftToRightFallingAimDownPose or
        SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose => (byte)SamusMovementType.TurningWhileFalling,
        SamusPoseId.DamageBoostLeftPose or
        SamusPoseId.DamageBoostRightPose => (byte)SamusMovementType.DamageBoost,
        SamusPoseId.DraygonGrabbedNeutralLeftPose or
        SamusPoseId.DraygonGrabbedAimUpLeftPose or
        SamusPoseId.DraygonGrabbedFiringLeftPose or
        SamusPoseId.DraygonGrabbedAimDownLeftPose or
        SamusPoseId.DraygonGrabbedMovingLeftPose or
        SamusPoseId.UnusedPoseC5 or
        SamusPoseId.UnusedPoseC6 or
        SamusPoseId.UnusedPoseDf or
        SamusPoseId.DraygonGrabbedNeutralRightPose or
        SamusPoseId.DraygonGrabbedAimUpRightPose or
        SamusPoseId.DraygonGrabbedFiringRightPose or
        SamusPoseId.DraygonGrabbedAimDownRightPose or
        SamusPoseId.DraygonGrabbedMovingRightPose => (byte)SamusMovementType.DraygonHeld,
        SamusPoseId.ShinesparkWindupRightPose or
        SamusPoseId.ShinesparkWindupLeftPose or
        SamusPoseId.ShinesparkHorizontalRightPose or
        SamusPoseId.ShinesparkHorizontalLeftPose or
        SamusPoseId.ShinesparkVerticalRightPose or
        SamusPoseId.ShinesparkVerticalLeftPose or
        SamusPoseId.ShinesparkDiagonalRightPose or
        SamusPoseId.ShinesparkDiagonalLeftPose or
        SamusPoseId.CrystalFlashRightPose or
        SamusPoseId.CrystalFlashLeftPose or
        SamusPoseId.DrainedCrouchingRightPose or
        SamusPoseId.DrainedCrouchingLeftPose or
        SamusPoseId.DrainedStandingRightPose or
        SamusPoseId.DrainedStandingLeftPose => (byte)SamusMovementType.Special,
        // Bounded indices beyond FC observe Calc_Xray_HDMADataTable_OffScreen opcodes.
        (SamusPoseId)0xfd => 139,
        (SamusPoseId)0xfe => 0,
        (SamusPoseId)0xff => 56,
    };

    /// <summary>Native PoseDefinitions NoInputPose selection, $91:B62B record stride eight; FD..FF are bounded adjacent code observations.</summary>
    internal static SamusPoseId ReadNoInputPose(SamusPoseId pose) => (SamusPoseId)pose switch
    {
        SamusPoseId.StandingAimUpRightPose or
        SamusPoseId.StandingAimDiagonalUpRightPose or
        SamusPoseId.StandingAimDiagonalDownRightPose or
        SamusPoseId.MovingRightNormalPose or
        SamusPoseId.MovingRightGunExtendedPose or
        SamusPoseId.RunningAimUpRightPose or
        SamusPoseId.RunningAimDiagonalUpRightPose or
        SamusPoseId.RunningAimDiagonalDownRightPose or
        SamusPoseId.UnusedPose45 or
        SamusPoseId.MoonwalkFacingRightPose or
        SamusPoseId.GrappleStandingRightPose => SamusPoseId.FacingRightNormalPose,
        SamusPoseId.StandingAimUpLeftPose or
        SamusPoseId.StandingAimDiagonalUpLeftPose or
        SamusPoseId.StandingAimDiagonalDownLeftPose or
        SamusPoseId.MovingLeftNormalPose or
        SamusPoseId.MovingLeftGunExtendedPose or
        SamusPoseId.RunningAimUpLeftPose or
        SamusPoseId.RunningAimDiagonalUpLeftPose or
        SamusPoseId.RunningAimDiagonalDownLeftPose or
        SamusPoseId.UnusedPose46 or
        SamusPoseId.MoonwalkFacingLeftPose or
        SamusPoseId.GrappleStandingLeftPose => SamusPoseId.FacingLeftNormalPose,
        SamusPoseId.MoonwalkAimUpRightPose => SamusPoseId.StandingAimDiagonalUpRightPose,
        SamusPoseId.MoonwalkAimUpLeftPose => SamusPoseId.StandingAimDiagonalUpLeftPose,
        SamusPoseId.MoonwalkAimDownRightPose or
        SamusPoseId.GrappleStandingDownRightPose => SamusPoseId.StandingAimDiagonalDownRightPose,
        SamusPoseId.MoonwalkAimDownLeftPose or
        SamusPoseId.GrappleStandingDownLeftPose => SamusPoseId.StandingAimDiagonalDownLeftPose,
        SamusPoseId.WallJumpRightPose => SamusPoseId.SpinJumpRightPose,
        SamusPoseId.WallJumpLeftPose => SamusPoseId.SpinJumpLeftPose,
        SamusPoseId.MorphBallMovingRightPose => SamusPoseId.MorphBallGroundRightPose,
        SamusPoseId.UnusedPose21 or
        SamusPoseId.UnusedPose22 or
        SamusPoseId.UnusedPose24 => SamusPoseId.UnusedPose20,
        SamusPoseId.CrouchingRightPose or
        SamusPoseId.CrouchingAimDiagonalUpRightPose or
        SamusPoseId.CrouchingAimDiagonalDownRightPose or
        SamusPoseId.CrouchingAimUpRightPose or
        SamusPoseId.GrappleCrouchingRightPose or
        SamusPoseId.GrappleCrouchingDownRightPose => SamusPoseId.CrouchingRightPose,
        SamusPoseId.CrouchingLeftPose or
        SamusPoseId.CrouchingAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingAimDiagonalDownLeftPose or
        SamusPoseId.CrouchingAimUpLeftPose or
        SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or
        SamusPoseId.GrappleCrouchingLeftPose or
        SamusPoseId.GrappleCrouchingDownLeftPose => SamusPoseId.CrouchingLeftPose,
        SamusPoseId.FallingAimUpRightPose or
        SamusPoseId.UnusedPose65 or
        SamusPoseId.FallingAimDiagonalUpRightPose or
        SamusPoseId.FallingAimDiagonalDownRightPose => SamusPoseId.FallingRightPose,
        SamusPoseId.FallingAimUpLeftPose or
        SamusPoseId.UnusedPose66 or
        SamusPoseId.FallingAimDiagonalUpLeftPose or
        SamusPoseId.FallingAimDiagonalDownLeftPose => SamusPoseId.FallingLeftPose,
        SamusPoseId.UnusedPoseAE => SamusPoseId.FallingAimDownRightPose,
        SamusPoseId.UnusedPoseAF => SamusPoseId.FallingAimDownLeftPose,
        SamusPoseId.MorphBallMovingLeftPose => SamusPoseId.MorphBallGroundLeftPose,
        SamusPoseId.UnusedPose23 => SamusPoseId.UnusedPose42,
        SamusPoseId.DamageBoostRightPose => SamusPoseId.NeutralJumpRightPose,
        SamusPoseId.DamageBoostLeftPose => SamusPoseId.NeutralJumpLeftPose,
        SamusPoseId.NormalJumpAimUpRightPose or
        SamusPoseId.NormalJumpAimDiagonalUpRightPose or
        SamusPoseId.NormalJumpAimDiagonalDownRightPose => SamusPoseId.NormalJumpForwardRightPose,
        SamusPoseId.NormalJumpAimUpLeftPose or
        SamusPoseId.NormalJumpAimDiagonalUpLeftPose or
        SamusPoseId.NormalJumpAimDiagonalDownLeftPose => SamusPoseId.NormalJumpForwardLeftPose,
        SamusPoseId.UnusedPose5D => SamusPoseId.UnusedPose5D,
        SamusPoseId.UnusedPose5E => SamusPoseId.UnusedPose5E,
        SamusPoseId.UnusedPose5F => SamusPoseId.UnusedPose5F,
        SamusPoseId.UnusedPose60 => SamusPoseId.UnusedPose60,
        SamusPoseId.UnusedPoseAC => SamusPoseId.FallingGunExtendedRightPose,
        SamusPoseId.UnusedPoseAD => SamusPoseId.FallingGunExtendedLeftPose,
        SamusPoseId.UnusedPoseB0 => SamusPoseId.FallingAimDiagonalDownRightPose,
        SamusPoseId.UnusedPoseB1 => SamusPoseId.FallingAimDiagonalDownLeftPose,
        SamusPoseId.SpringBallMovingRightPose => SamusPoseId.SpringBallGroundRightPose,
        SamusPoseId.SpringBallMovingLeftPose => SamusPoseId.SpringBallGroundLeftPose,
        SamusPoseId.RanIntoWallAimUpRightPose or
        SamusPoseId.RanIntoWallAimDownRightPose => SamusPoseId.RanIntoWallRightPose,
        SamusPoseId.RanIntoWallAimUpLeftPose or
        SamusPoseId.RanIntoWallAimDownLeftPose => SamusPoseId.RanIntoWallLeftPose,
        SamusPoseId.UnusedPose61 or
        SamusPoseId.GrappleSwingRightPose => SamusPoseId.GrappleSwingRightPose,
        SamusPoseId.UnusedPose62 or
        SamusPoseId.GrappleSwingLeftPose => SamusPoseId.GrappleSwingLeftPose,
        SamusPoseId.DraygonGrabbedAimUpLeftPose or
        SamusPoseId.DraygonGrabbedFiringLeftPose or
        SamusPoseId.DraygonGrabbedAimDownLeftPose or
        SamusPoseId.DraygonGrabbedMovingLeftPose => SamusPoseId.DraygonGrabbedNeutralLeftPose,
        SamusPoseId.DraygonGrabbedAimUpRightPose or
        SamusPoseId.DraygonGrabbedFiringRightPose or
        SamusPoseId.DraygonGrabbedAimDownRightPose or
        SamusPoseId.DraygonGrabbedMovingRightPose => SamusPoseId.DraygonGrabbedNeutralRightPose,
        SamusPoseId.ForwardFacingPowerSuitPose or
        SamusPoseId.FacingRightNormalPose or
        SamusPoseId.FacingLeftNormalPose or
        SamusPoseId.NormalJumpGunExtendedRightPose or
        SamusPoseId.NormalJumpGunExtendedLeftPose or
        SamusPoseId.NormalJumpAimDownRightPose or
        SamusPoseId.NormalJumpAimDownLeftPose or
        SamusPoseId.SpinJumpRightPose or
        SamusPoseId.SpinJumpLeftPose or
        SamusPoseId.SpaceJumpRightPose or
        SamusPoseId.SpaceJumpLeftPose or
        SamusPoseId.MorphBallGroundRightPose or
        SamusPoseId.UnusedPose20 or
        SamusPoseId.TurningRightToLeftPose or
        SamusPoseId.TurningLeftToRightPose or
        SamusPoseId.FallingRightPose or
        SamusPoseId.FallingLeftPose or
        SamusPoseId.FallingAimDownRightPose or
        SamusPoseId.FallingAimDownLeftPose or
        SamusPoseId.TurningRightToLeftJumpPose or
        SamusPoseId.TurningLeftToRightJumpPose or
        SamusPoseId.MorphBallFallingRightPose or
        SamusPoseId.MorphBallFallingLeftPose or
        SamusPoseId.UnusedKnockbackRightPose or
        SamusPoseId.UnusedKnockbackLeftPose or
        SamusPoseId.CrouchingTransitionRightPose or
        SamusPoseId.CrouchingTransitionLeftPose or
        SamusPoseId.MorphingTransitionRightPose or
        SamusPoseId.MorphingTransitionLeftPose or
        SamusPoseId.UnusedPose39 or
        SamusPoseId.UnusedPose3A or
        SamusPoseId.StandingTransitionRightPose or
        SamusPoseId.StandingTransitionLeftPose or
        SamusPoseId.UnmorphingTransitionRightPose or
        SamusPoseId.UnmorphingTransitionLeftPose or
        SamusPoseId.UnusedPose3F or
        SamusPoseId.UnusedPose40 or
        SamusPoseId.MorphBallGroundLeftPose or
        SamusPoseId.UnusedPose42 or
        SamusPoseId.TurningRightToLeftCrouchingPose or
        SamusPoseId.TurningLeftToRightCrouchingPose or
        SamusPoseId.UnusedPose47 or
        SamusPoseId.UnusedPose48 or
        SamusPoseId.NeutralJumpTransitionRightPose or
        SamusPoseId.NeutralJumpTransitionLeftPose or
        SamusPoseId.NeutralJumpRightPose or
        SamusPoseId.NeutralJumpLeftPose or
        SamusPoseId.NormalJumpForwardRightPose or
        SamusPoseId.NormalJumpForwardLeftPose or
        SamusPoseId.KnockbackRightPose or
        SamusPoseId.KnockbackLeftPose or
        SamusPoseId.NormalJumpTransitionAimUpRightPose or
        SamusPoseId.NormalJumpTransitionAimUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
        SamusPoseId.UnusedPose5B or
        SamusPoseId.UnusedPose5C or
        SamusPoseId.UnusedPose63 or
        SamusPoseId.UnusedPose64 or
        SamusPoseId.FallingGunExtendedRightPose or
        SamusPoseId.FallingGunExtendedLeftPose or
        SamusPoseId.SpringBallGroundRightPose or
        SamusPoseId.SpringBallGroundLeftPose or
        SamusPoseId.SpringBallFallingRightPose or
        SamusPoseId.SpringBallFallingLeftPose or
        SamusPoseId.SpringBallJumpRightPose or
        SamusPoseId.SpringBallJumpLeftPose or
        SamusPoseId.ScrewAttackRightPose or
        SamusPoseId.ScrewAttackLeftPose or
        SamusPoseId.TurningRightToLeftFallingPose or
        SamusPoseId.TurningLeftToRightFallingPose or
        SamusPoseId.RanIntoWallRightPose or
        SamusPoseId.RanIntoWallLeftPose or
        SamusPoseId.TurningRightToLeftAimUpPose or
        SamusPoseId.TurningLeftToRightAimUpPose or
        SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
        SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
        SamusPoseId.TurningRightToLeftJumpAimUpPose or
        SamusPoseId.TurningLeftToRightJumpAimUpPose or
        SamusPoseId.TurningRightToLeftJumpAimDownPose or
        SamusPoseId.TurningLeftToRightJumpAimDownPose or
        SamusPoseId.TurningRightToLeftFallingAimUpPose or
        SamusPoseId.TurningLeftToRightFallingAimUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDownPose or
        SamusPoseId.TurningLeftToRightFallingAimDownPose or
        SamusPoseId.ForwardFacingSuitedPose or
        SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or
        SamusPoseId.NormalLandingRightPose or
        SamusPoseId.NormalLandingLeftPose or
        SamusPoseId.SpinLandingRightPose or
        SamusPoseId.SpinLandingLeftPose or
        SamusPoseId.GrappleWallContactLeftPose or
        SamusPoseId.GrappleWallContactRightPose or
        SamusPoseId.DraygonGrabbedNeutralLeftPose or
        SamusPoseId.MoonwalkTurnJumpLeftPose or
        SamusPoseId.MoonwalkTurnJumpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
        SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimDownLeftPose or
        SamusPoseId.MoonwalkTurnJumpAimDownRightPose or
        SamusPoseId.UnusedPoseC5 or
        SamusPoseId.UnusedPoseC6 or
        SamusPoseId.ShinesparkWindupRightPose or
        SamusPoseId.ShinesparkWindupLeftPose or
        SamusPoseId.ShinesparkHorizontalRightPose or
        SamusPoseId.ShinesparkHorizontalLeftPose or
        SamusPoseId.ShinesparkVerticalRightPose or
        SamusPoseId.ShinesparkVerticalLeftPose or
        SamusPoseId.ShinesparkDiagonalRightPose or
        SamusPoseId.ShinesparkDiagonalLeftPose or
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
        SamusPoseId.LandingAimUpRightPose or
        SamusPoseId.LandingAimUpLeftPose or
        SamusPoseId.LandingAimDiagonalUpRightPose or
        SamusPoseId.LandingAimDiagonalUpLeftPose or
        SamusPoseId.LandingAimDiagonalDownRightPose or
        SamusPoseId.LandingAimDiagonalDownLeftPose or
        SamusPoseId.FiringLandingRightPose or
        SamusPoseId.FiringLandingLeftPose or
        SamusPoseId.DrainedCrouchingRightPose or
        SamusPoseId.DrainedCrouchingLeftPose or
        SamusPoseId.DrainedStandingRightPose or
        SamusPoseId.DrainedStandingLeftPose or
        SamusPoseId.DraygonGrabbedNeutralRightPose or
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
        SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => SamusMovementRomData.Poses.RetainCurrentPoseFallback,
        // Bounded indices beyond FC observe Calc_Xray_HDMADataTable_OffScreen opcodes.
        (SamusPoseId)0xfd => (SamusPoseId)75,
        (SamusPoseId)0xfe => (SamusPoseId)255,
        (SamusPoseId)0xff => (SamusPoseId)229,
    };

}
