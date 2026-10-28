using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Hud;

/// <summary>Compiled weapon-admission policy; HUD artwork and labels are separate assets.</summary>
internal static class SamusHudDefinitions
{
    /// <summary>The first posture-transition pose; the transition admission indexes its flags from here.</summary>
    internal const SamusPoseId FirstTransitionPose = SamusPoseId.CrouchingTransitionRightPose;

    /// <summary>The first of the transition poses that never admit firing.</summary>
    internal const SamusPoseId NonFiringTransitionStart = SamusPoseId.UnusedPoseDb;

    /// <summary>The first transition pose that the standard weapon handler admits.</summary>
    internal const SamusPoseId StandardTransitionStart = SamusPoseId.CrouchingTransitionAimUpRightPose;

    /// <summary>$90:DD05: movement-state HUD admission dispatch, preserving all 28 native handler identities.</summary>
    internal static SamusHudHandler MovementHandler(SamusMovementType movement)
    {
        return movement switch
        {
            SamusMovementType.SpinJumping or SamusMovementType.Knockback or SamusMovementType.Unused0D or
            SamusMovementType.WallJumping or SamusMovementType.DamageBoost or SamusMovementType.Special => SamusHudHandler.Jump,
            SamusMovementType.MorphBallGround or SamusMovementType.UnusedGlitchBall or SamusMovementType.MorphBallFalling or
            SamusMovementType.UnusedGlitchBallAlternate or SamusMovementType.SpringBallGround or
            SamusMovementType.SpringBallInAir or SamusMovementType.SpringBallFalling => SamusHudHandler.MorphBall,
            SamusMovementType.Unused0B or SamusMovementType.Unused0C or SamusMovementType.Grappling => SamusHudHandler.Grapple,
            SamusMovementType.TurningOnGround or SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling => SamusHudHandler.Turning,
            SamusMovementType.PostureTransition => SamusHudHandler.Transition,
            SamusMovementType.DraygonHeld => SamusHudHandler.DraygonHeld,
            SamusMovementType.Standing or SamusMovementType.Running or SamusMovementType.NormalJumping or
                SamusMovementType.Crouching or SamusMovementType.Falling or SamusMovementType.Moonwalking or
                SamusMovementType.RanIntoWall => SamusHudHandler.Standard,
            _ => throw new ArgumentOutOfRangeException(nameof(movement), movement, "Undefined movement type."),
        };
    }

    /// <summary>$90:DDAA-$DDB5 actual transition flags: crouch/stand admit weapons; morph/unmorph preserve charge unless cancelling Grapple. Other admitted indices preserve only their bounded native instruction observations.</summary>
    /// <remarks>
    /// The admission reads a byte per pose. Outside the transition block that byte is the turning, posture, spin, held
    /// and X-ray handlers' code around the real flags, which <paramref name="overreads"/> holds.
    /// </remarks>
    internal static byte PostureObservation(SamusPoseId pose, SamusOverreadCatalog? overreads)
    {
        if (!SamusPoseNumbering.Precedes(pose, NonFiringTransitionStart))
            throw new ArgumentOutOfRangeException(nameof(pose));
        if (SamusPoseNumbering.Precedes(pose, FirstTransitionPose))
            return RequireOverreads(overreads).HudPrecedingPostureByte(SamusPoseNumbering.Number(pose));
        if (!SamusPoseNumbering.InRange(pose, FirstTransitionPose, SamusPoseId.UnusedPose40))
            return RequireOverreads(overreads).HudFollowingPostureByte(SamusPoseNumbering.Number(pose) - (SamusPoseNumbering.Number(SamusPoseId.UnusedPose40) + 1));
        return pose switch
        {
            SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionLeftPose or
            SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionLeftPose => 0,
            SamusPoseId.MorphingTransitionRightPose or SamusPoseId.MorphingTransitionLeftPose or
            SamusPoseId.UnmorphingTransitionRightPose or SamusPoseId.UnmorphingTransitionLeftPose or
            SamusPoseId.UnusedPose39 or SamusPoseId.UnusedPose3A or
            SamusPoseId.UnusedPose3F or SamusPoseId.UnusedPose40 => 1,
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
                SamusPoseId.DrainedCrouchingRightPose or
                SamusPoseId.DrainedCrouchingLeftPose or
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
                SamusPoseId.UnusedPoseC6 => throw new InvalidOperationException($"Pose {pose} is outside the $35-$40 transition block."),
            SamusPoseId.AdjacentCodePoseFD or
                SamusPoseId.AdjacentCodePoseFE or
                SamusPoseId.AdjacentCodePoseFF => throw new InvalidOperationException($"Pose {pose} is outside the $35-$40 transition block."),
            var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
        };
    }

    /// <summary>Requires the installed overread catalog for a pose outside the transition block.</summary>
    private static SamusOverreadCatalog RequireOverreads(SamusOverreadCatalog? overreads) =>
        overreads ?? throw new InvalidOperationException(
            "A posture-transition admission outside the transition block needs the installed Samus overread catalog.");
}
