using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Pose-specific facing, movement and idle transitions, independent of editable artwork.</summary>
/// <remarks>
/// Every pose has a typed facing and movement here. The three adjacent-code poses FD..FF read the unrelated
/// $91:BE11 X-ray HDMA machine-code routine as their records; their raw facing and movement bytes, and those of
/// the two unused glitch-ball poses whose facing bytes are neither left nor right, are owned by
/// <see cref="SamusPoseRecordCodec"/>, the only place a pose record's bytes exist.
/// </remarks>
internal static class SamusPoseDispatchDefinitions
{
    /// <summary>
    /// The pose's facing, from the native PoseDefinitions facing byte. Poses whose byte names neither left nor right
    /// (the front view, the unused glitch-ball records and two adjacent-code records) face neither way.
    /// </summary>
    internal static SamusFacingDirection ReadFacing(SamusPoseId pose) => pose switch
    {
        SamusPoseId.ForwardFacingPowerSuitPose or
        SamusPoseId.ForwardFacingSuitedPose or
        SamusPoseId.UnusedPose21 or
        SamusPoseId.UnusedPose22 or
        SamusPoseId.AdjacentCodePoseFE or
        SamusPoseId.AdjacentCodePoseFF => SamusFacingDirection.ForwardOrSpecial,
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
        SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => SamusFacingDirection.Left,
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
        SamusPoseId.StandingTransitionAimDiagonalDownRightPose or
        // The adjacent-code record FD happens to read the right-facing byte.
        SamusPoseId.AdjacentCodePoseFD => SamusFacingDirection.Right,
        var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
    };

    /// <summary>
    /// The pose's movement type, from the native PoseDefinitions movement byte, which selects the movement handler.
    /// The adjacent-code records FD and FF read bytes beyond the retail dispatcher, which is corrupt metadata and fails
    /// here; FE reads a standing byte.
    /// </summary>
    /// <exception cref="InvalidDataException">The pose is FD or FF.</exception>
    internal static SamusMovementType ReadMovement(SamusPoseId pose) => pose switch
    {
        SamusPoseId.AdjacentCodePoseFE or
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
        SamusPoseId.FiringLandingLeftPose => SamusMovementType.Standing,
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
        SamusPoseId.UnusedPose46 => SamusMovementType.Running,
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
        SamusPoseId.NormalJumpAimDiagonalDownLeftPose => SamusMovementType.NormalJumping,
        SamusPoseId.SpinJumpRightPose or
        SamusPoseId.SpinJumpLeftPose or
        SamusPoseId.SpaceJumpRightPose or
        SamusPoseId.SpaceJumpLeftPose or
        SamusPoseId.ScrewAttackRightPose or
        SamusPoseId.ScrewAttackLeftPose => SamusMovementType.SpinJumping,
        SamusPoseId.MorphBallGroundRightPose or
        SamusPoseId.MorphBallMovingRightPose or
        SamusPoseId.MorphBallMovingLeftPose or
        SamusPoseId.MorphBallGroundLeftPose => SamusMovementType.MorphBallGround,
        SamusPoseId.CrouchingRightPose or
        SamusPoseId.CrouchingLeftPose or
        SamusPoseId.CrouchingAimDiagonalUpRightPose or
        SamusPoseId.CrouchingAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingAimDiagonalDownRightPose or
        SamusPoseId.CrouchingAimDiagonalDownLeftPose or
        SamusPoseId.CrouchingAimUpRightPose or
        SamusPoseId.CrouchingAimUpLeftPose or
        SamusPoseId.XrayingCrouchingRightPose or
        SamusPoseId.XrayingCrouchingLeftPose => SamusMovementType.Crouching,
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
        SamusPoseId.FallingAimDiagonalDownLeftPose => SamusMovementType.Falling,
        SamusPoseId.UnusedPose20 or
        SamusPoseId.UnusedPose21 or
        SamusPoseId.UnusedPose22 or
        SamusPoseId.UnusedPose23 or
        SamusPoseId.UnusedPose24 or
        SamusPoseId.UnusedPose42 => SamusMovementType.UnusedGlitchBall,
        SamusPoseId.MorphBallFallingRightPose or
        SamusPoseId.MorphBallFallingLeftPose => SamusMovementType.MorphBallFalling,
        SamusPoseId.UnusedKnockbackRightPose or
        SamusPoseId.UnusedKnockbackLeftPose => SamusMovementType.UnusedGlitchBallAlternate,
        SamusPoseId.KnockbackRightPose or
        SamusPoseId.KnockbackLeftPose or
        SamusPoseId.DeathSequenceRightPose or
        SamusPoseId.DeathSequenceLeftPose => SamusMovementType.Knockback,
        SamusPoseId.UnusedPose5D or
        SamusPoseId.UnusedPose5E or
        SamusPoseId.UnusedPose5F or
        SamusPoseId.UnusedPose60 => SamusMovementType.Unused0B,
        SamusPoseId.UnusedPose63 or
        SamusPoseId.UnusedPose64 or
        SamusPoseId.UnusedPose65 or
        SamusPoseId.UnusedPose66 => SamusMovementType.Unused0D,
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
        SamusPoseId.MoonwalkTurnJumpAimDownRightPose => SamusMovementType.TurningOnGround,
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
        SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => SamusMovementType.PostureTransition,
        SamusPoseId.MoonwalkFacingLeftPose or
        SamusPoseId.MoonwalkFacingRightPose or
        SamusPoseId.MoonwalkAimUpLeftPose or
        SamusPoseId.MoonwalkAimUpRightPose or
        SamusPoseId.MoonwalkAimDownLeftPose or
        SamusPoseId.MoonwalkAimDownRightPose => SamusMovementType.Moonwalking,
        SamusPoseId.SpringBallGroundRightPose or
        SamusPoseId.SpringBallGroundLeftPose or
        SamusPoseId.SpringBallMovingRightPose or
        SamusPoseId.SpringBallMovingLeftPose => SamusMovementType.SpringBallGround,
        SamusPoseId.SpringBallJumpRightPose or
        SamusPoseId.SpringBallJumpLeftPose => SamusMovementType.SpringBallInAir,
        SamusPoseId.SpringBallFallingRightPose or
        SamusPoseId.SpringBallFallingLeftPose => SamusMovementType.SpringBallFalling,
        SamusPoseId.WallJumpRightPose or
        SamusPoseId.WallJumpLeftPose => SamusMovementType.WallJumping,
        SamusPoseId.RanIntoWallRightPose or
        SamusPoseId.RanIntoWallLeftPose or
        SamusPoseId.RanIntoWallAimUpRightPose or
        SamusPoseId.RanIntoWallAimUpLeftPose or
        SamusPoseId.RanIntoWallAimDownRightPose or
        SamusPoseId.RanIntoWallAimDownLeftPose => SamusMovementType.RanIntoWall,
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
        SamusPoseId.GrappleWallContactRightPose => SamusMovementType.Grappling,
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
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose => SamusMovementType.TurningWhileJumping,
        SamusPoseId.TurningRightToLeftFallingPose or
        SamusPoseId.TurningLeftToRightFallingPose or
        SamusPoseId.TurningRightToLeftFallingAimUpPose or
        SamusPoseId.TurningLeftToRightFallingAimUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDownPose or
        SamusPoseId.TurningLeftToRightFallingAimDownPose or
        SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose => SamusMovementType.TurningWhileFalling,
        SamusPoseId.DamageBoostLeftPose or
        SamusPoseId.DamageBoostRightPose => SamusMovementType.DamageBoost,
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
        SamusPoseId.DraygonGrabbedMovingRightPose => SamusMovementType.DraygonHeld,
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
        SamusPoseId.DrainedStandingLeftPose => SamusMovementType.Special,
        // Bounded indices beyond FC observe Calc_Xray_HDMADataTable_OffScreen opcodes.
        SamusPoseId.AdjacentCodePoseFD or
        SamusPoseId.AdjacentCodePoseFF => throw new InvalidDataException(
            $"Pose {pose} reads adjacent code as its movement type, beyond the retail dispatcher's {nameof(SamusMovementType.Special)}."),
        var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
    };

    /// <summary>Native PoseDefinitions NoInputPose selection, $91:B62B record stride eight; FD..FF are bounded adjacent code observations.</summary>
    internal static SamusPoseId ReadNoInputPose(SamusPoseId pose) => pose switch
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
        SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => SamusMovementDefinitions.Poses.RetainCurrentPoseFallback,
        // Bounded indices beyond FC observe Calc_Xray_HDMADataTable_OffScreen opcodes.
        SamusPoseId.AdjacentCodePoseFD => SamusPoseId.NeutralJumpTransitionRightPose,
        SamusPoseId.AdjacentCodePoseFE => SamusMovementDefinitions.Poses.RetainCurrentPoseFallback,
        SamusPoseId.AdjacentCodePoseFF => SamusPoseId.LandingAimDiagonalDownLeftPose,
        var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
    };

}
