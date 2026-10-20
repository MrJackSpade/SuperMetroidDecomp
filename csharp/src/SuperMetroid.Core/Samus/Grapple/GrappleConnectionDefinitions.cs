using static SuperMetroid.Core.Samus.Grapple.SamusGrappleRomData.Connections;
using static SuperMetroid.Core.Samus.Grapple.GrappleConnectionHandler;
using static SuperMetroid.Core.Samus.Poses.SamusPoseId;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Native Grapple connection, cancellation and dropped-pose mechanics, not visual assets.</summary>
internal static class GrappleConnectionDefinitions
{
    /// <summary>
    /// $9B:C43E GrappleBeamSpecialAngles stores eight exact collision-stop records. They are four
    /// authored stops, each immediately followed by its left/right reflection: the angle reflects
    /// as $0100 - angle, X negates and the pose swaps facing; Y and the next function are shared.
    /// </summary>
    private static readonly SpecialConnection[] AuthoredStops =
    [
        new(SnesAngle.FromRaw(0xd680), GrappleCrouchingDownRightPose, new(-30, -24), GrappleConnectionFunction.LockedInPlace),
        new(SnesAngle.FromRaw(0xb380), GrappleCrouchingDownRightPose, new(-28, -8), GrappleConnectionFunction.LockedInPlace),
        new(SnesAngle.FromRaw(0x6a80), GrappleWallContactRightPose, new(24, 16), GrappleConnectionFunction.WallGrab),
        new(SnesAngle.FromRaw(0x7380), GrappleWallContactLeftPose, new(-8, 16), GrappleConnectionFunction.WallGrab),
    ];

    /// <summary>One collision-stop record.</summary>
    /// <param name="Angle">The rope angle that triggers the stop.</param>
    /// <param name="Pose">Samus's pose at the stop.</param>
    /// <param name="SamusOffset">Samus's room-pixel position relative to the grapple anchor.</param>
    /// <param name="Function">The connection function installed at the stop.</param>
    internal readonly record struct SpecialConnection(SnesAngle Angle, SamusPoseId Pose, Offset16<RoomPixels> SamusOffset, GrappleConnectionFunction Function);

    /// <summary>Number of native special-angle records, in their native order.</summary>
    internal static int SpecialAngleCount => AuthoredStops.Length * 2;

    /// <summary>The native record at <paramref name="index"/>: an authored stop or the reflection that follows it.</summary>
    internal static SpecialConnection SpecialAngle(int index)
    {
        if ((uint)index >= SpecialAngleCount) throw new ArgumentOutOfRangeException(nameof(index));
        SpecialConnection stop = AuthoredStops[index >> 1];
        return (index & 1) == 0 ? stop : new(SnesAngle.FromRaw(unchecked((ushort)(0x0100 - stop.Angle.RawValue))), MirrorPose(stop.Pose),
            stop.SamusOffset.MirroredX(), stop.Function);
    }

    /// <summary>Reflects only the crouching-down and wall-contact poses used by the four authored stops.</summary>
    private static SamusPoseId MirrorPose(SamusPoseId pose) => pose switch
    {
        GrappleCrouchingDownRightPose => GrappleCrouchingDownLeftPose,
        GrappleCrouchingDownLeftPose => GrappleCrouchingDownRightPose,
        GrappleWallContactRightPose => GrappleWallContactLeftPose,
        GrappleWallContactLeftPose => GrappleWallContactRightPose,
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
            SamusPoseId.UnusedPoseC6 => throw new ArgumentOutOfRangeException(nameof(pose)),
        SamusPoseId.AdjacentCodePoseFD or
            SamusPoseId.AdjacentCodePoseFE or
            SamusPoseId.AdjacentCodePoseFF => throw new ArgumentOutOfRangeException(nameof(pose)),
        var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
    };
    /// <summary>$9B:B8B8 cancellation policy for the 28 movement dispatch identities.</summary>
    internal static bool CancelsFiring(SamusMovementType movement) => movement switch
    {
        SamusMovementType.Standing or SamusMovementType.Running or SamusMovementType.NormalJumping
            or SamusMovementType.Crouching or SamusMovementType.Falling or SamusMovementType.Unused0B
            or SamusMovementType.Unused0C or SamusMovementType.Moonwalking or SamusMovementType.RanIntoWall
            or SamusMovementType.Grappling or SamusMovementType.DraygonHeld => false,
        SamusMovementType.Standing or
            SamusMovementType.Running or
            SamusMovementType.NormalJumping or
            SamusMovementType.SpinJumping or
            SamusMovementType.MorphBallGround or
            SamusMovementType.Crouching or
            SamusMovementType.Falling or
            SamusMovementType.UnusedGlitchBall or
            SamusMovementType.MorphBallFalling or
            SamusMovementType.UnusedGlitchBallAlternate or
            SamusMovementType.Knockback or
            SamusMovementType.Unused0B or
            SamusMovementType.Unused0C or
            SamusMovementType.Unused0D or
            SamusMovementType.TurningOnGround or
            SamusMovementType.PostureTransition or
            SamusMovementType.Moonwalking or
            SamusMovementType.SpringBallGround or
            SamusMovementType.SpringBallInAir or
            SamusMovementType.SpringBallFalling or
            SamusMovementType.WallJumping or
            SamusMovementType.RanIntoWall or
            SamusMovementType.Grappling or
            SamusMovementType.TurningWhileJumping or
            SamusMovementType.TurningWhileFalling or
            SamusMovementType.DamageBoost or
            SamusMovementType.DraygonHeld or
            SamusMovementType.Special => true,
        _ => throw new ArgumentOutOfRangeException(nameof(movement)),
    };

    /// <summary>$9B:C9BA/$C9C4 directional standing/crouching poses after the rope drops.</summary>
    internal static SamusPoseId DroppedPose(byte direction, bool compact) => direction switch
    {
        0 => compact ? CrouchingAimUpRightPose : StandingAimUpRightPose,
        1 => compact ? CrouchingAimDiagonalUpRightPose : StandingAimDiagonalUpRightPose,
        2 or 4 => compact ? CrouchingRightPose : FacingRightNormalPose,
        3 => compact ? CrouchingAimDiagonalDownRightPose : StandingAimDiagonalDownRightPose,
        5 or 7 => compact ? CrouchingLeftPose : FacingLeftNormalPose,
        6 => compact ? CrouchingAimDiagonalDownLeftPose : StandingAimDiagonalDownLeftPose,
        8 => compact ? CrouchingAimDiagonalUpLeftPose : StandingAimDiagonalUpLeftPose,
        9 => compact ? CrouchingAimUpLeftPose : StandingAimUpLeftPose,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };

    /// <summary>$9B:C3C6/$C3EE/$C416 default, vertical and crouching directional connection policies.</summary>
    internal static (GrappleConnectionFunction Function, GrappleConnectionHandler Handler) ResolveConnection(int address)
    {
        int offset = address - DefaultTable;
        if (offset < 0 || offset >= 30 * 4 || (offset & 3) != 0)
            throw new InvalidDataException(
                $"Grapple connection record ${address:X6} is outside the compiled definitions.");
        int record = offset / 4;
        int direction = record % 10;
        bool compact = record >= 20;
        GrappleConnectionHandler handler = record is >= 10 and < 20
            ? direction < 5 ? SwingClockwise : SwingAnticlockwise
            : direction switch
            {
                0 or 1 => SwingClockwise,
                2 => compact ? CrouchingUpRight : StandingUpRight,
                3 => compact ? CrouchingRight : StandingRight,
                4 or 5 => StandingDown,
                6 => compact ? CrouchingDownLeft : StandingDown,
                7 => compact ? CrouchingUpLeft : StandingUpLeft,
                _ => SwingAnticlockwise,
            };
        GrappleConnectionFunction function = handler is SwingClockwise or SwingAnticlockwise
            ? GrappleConnectionFunction.Swinging
            : GrappleConnectionFunction.LockedInPlace;
        return (function, handler);
    }
}
