using System.Buffers.Binary;
using static SuperMetroid.Core.Samus.Poses.SamusAnimationDelayDefinitions;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>How a pose's animation delay list is found and read: the one reader of a delay-stream byte's command bit (#1461).</summary>
[EncodingOwner("Samus animation delay stream bytes")]
internal static class SamusAnimationDelays
{
    /// <summary>Bit 7 of a delay-stream byte: set for an animation command (<c>$80-$FF</c>), clear for a frame delay.</summary>
    private const byte CommandBit = 0x80;

    /// <summary>Whether a delay-stream byte is an animation command rather than a frame delay (the handler's <c>BMI</c>).</summary>
    internal static bool IsCommand(int delayOrCommand) => (delayOrCommand & CommandBit) != 0;

    /// <summary>The delay program any pose byte animates by.</summary>
    /// <remarks>
    /// The three invalid pose bytes overread the pose table into the first delay bytes, whose words name a start in the
    /// floor update residue.
    /// </remarks>
    internal static SamusAnimationDelayList ListForPose(SamusPoseId pose)
    {
        if (!SamusPoseNumbering.HasRecord(pose))
        {
            int position = (SamusPoseNumbering.Number(pose) - SamusPoseNumbering.RecordCount) * sizeof(ushort);
            return new(null, BinaryPrimitives.ReadUInt16LittleEndian([SamusAnimationDelayPrograms.ByteAt(position),
                SamusAnimationDelayPrograms.ByteAt(position + 1)]));
        }
        return pose switch
        {
            SamusPoseId.ForwardFacingPowerSuitPose or
            SamusPoseId.ForwardFacingSuitedPose => ForwardFacingPowerSuitAnimation,
            SamusPoseId.FacingRightNormalPose or
            SamusPoseId.FacingLeftNormalPose => FacingRightNormalAnimation,
            SamusPoseId.StandingAimUpRightPose or
            SamusPoseId.StandingAimUpLeftPose or
            SamusPoseId.CrouchingAimUpRightPose or
            SamusPoseId.CrouchingAimUpLeftPose => StandingAimUpRightAnimation,
            SamusPoseId.StandingAimDiagonalUpRightPose or
            SamusPoseId.StandingAimDiagonalUpLeftPose or
            SamusPoseId.StandingAimDiagonalDownRightPose or
            SamusPoseId.StandingAimDiagonalDownLeftPose or
            SamusPoseId.UnusedPose47 or
            SamusPoseId.UnusedPose48 or
            SamusPoseId.CrouchingAimDiagonalUpRightPose or
            SamusPoseId.CrouchingAimDiagonalUpLeftPose or
            SamusPoseId.CrouchingAimDiagonalDownRightPose or
            SamusPoseId.CrouchingAimDiagonalDownLeftPose or
            SamusPoseId.RanIntoWallRightPose or
            SamusPoseId.RanIntoWallLeftPose or
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
            SamusPoseId.RanIntoWallAimUpRightPose or
            SamusPoseId.RanIntoWallAimUpLeftPose or
            SamusPoseId.RanIntoWallAimDownRightPose or
            SamusPoseId.RanIntoWallAimDownLeftPose or
            SamusPoseId.DraygonGrabbedNeutralRightPose or
            SamusPoseId.DraygonGrabbedAimUpRightPose or
            SamusPoseId.DraygonGrabbedFiringRightPose or
            SamusPoseId.DraygonGrabbedAimDownRightPose => StandingAimDiagonalUpRightAnimation,
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
            SamusPoseId.UnusedPose46 => MovingRightNormalAnimation,
            SamusPoseId.NormalJumpGunExtendedRightPose or
            SamusPoseId.NormalJumpGunExtendedLeftPose or
            SamusPoseId.NormalJumpAimDiagonalUpRightPose or
            SamusPoseId.NormalJumpAimDiagonalUpLeftPose or
            SamusPoseId.NormalJumpAimDiagonalDownRightPose or
            SamusPoseId.NormalJumpAimDiagonalDownLeftPose => NormalJumpGunExtendedRightAnimation,
            SamusPoseId.NormalJumpAimUpRightPose or
            SamusPoseId.NormalJumpAimUpLeftPose => NormalJumpAimUpRightAnimation,
            SamusPoseId.NormalJumpAimDownRightPose or
            SamusPoseId.NormalJumpAimDownLeftPose => NormalJumpAimDownRightAnimation,
            SamusPoseId.SpinJumpRightPose or
            SamusPoseId.SpinJumpLeftPose => SpinJumpRightAnimation,
            SamusPoseId.SpaceJumpRightPose or
            SamusPoseId.SpaceJumpLeftPose => SpaceJumpRightAnimation,
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
            SamusPoseId.MorphBallGroundLeftPose or
            SamusPoseId.UnusedPose42 or
            SamusPoseId.UnusedPose5B or
            SamusPoseId.UnusedPose5C or
            SamusPoseId.UnusedPose5D or
            SamusPoseId.UnusedPose5E or
            SamusPoseId.UnusedPose5F or
            SamusPoseId.UnusedPose60 or
            SamusPoseId.UnusedPose61 or
            SamusPoseId.UnusedPose62 or
            SamusPoseId.SpringBallGroundRightPose or
            SamusPoseId.SpringBallGroundLeftPose or
            SamusPoseId.SpringBallMovingRightPose or
            SamusPoseId.SpringBallMovingLeftPose or
            SamusPoseId.SpringBallFallingRightPose or
            SamusPoseId.SpringBallFallingLeftPose or
            SamusPoseId.SpringBallJumpRightPose or
            SamusPoseId.SpringBallJumpLeftPose or
            SamusPoseId.UnusedPoseC5 or
            SamusPoseId.UnusedPoseDf => MorphBallGroundRightAnimation,
            SamusPoseId.TurningRightToLeftPose => TurningRightToLeftAnimation,
            SamusPoseId.TurningLeftToRightPose => TurningLeftToRightAnimation,
            SamusPoseId.CrouchingRightPose or
            SamusPoseId.CrouchingLeftPose => CrouchingRightAnimation,
            SamusPoseId.FallingRightPose or
            SamusPoseId.FallingLeftPose => FallingRightAnimation,
            SamusPoseId.FallingAimUpRightPose or
            SamusPoseId.FallingAimUpLeftPose => FallingAimUpRightAnimation,
            SamusPoseId.FallingAimDownRightPose or
            SamusPoseId.FallingAimDownLeftPose => FallingAimDownRightAnimation,
            SamusPoseId.TurningRightToLeftJumpPose => TurningRightToLeftJumpAnimation,
            SamusPoseId.TurningLeftToRightJumpPose => TurningLeftToRightJumpAnimation,
            SamusPoseId.CrouchingTransitionRightPose => CrouchingTransitionRightAnimation,
            SamusPoseId.CrouchingTransitionLeftPose => CrouchingTransitionLeftAnimation,
            SamusPoseId.MorphingTransitionRightPose => MorphingTransitionRightAnimation,
            SamusPoseId.MorphingTransitionLeftPose => MorphingTransitionLeftAnimation,
            SamusPoseId.UnusedPose39 => UnusedPose39Animation,
            SamusPoseId.UnusedPose3A => UnusedPose3AAnimation,
            SamusPoseId.StandingTransitionRightPose => StandingTransitionRightAnimation,
            SamusPoseId.StandingTransitionLeftPose => StandingTransitionLeftAnimation,
            SamusPoseId.UnmorphingTransitionRightPose => UnmorphingTransitionRightAnimation,
            SamusPoseId.UnmorphingTransitionLeftPose => UnmorphingTransitionLeftAnimation,
            SamusPoseId.UnusedPose3F => UnusedPose3FAnimation,
            SamusPoseId.UnusedPose40 => UnusedPose40Animation,
            SamusPoseId.TurningRightToLeftCrouchingPose => TurningRightToLeftCrouchingAnimation,
            SamusPoseId.TurningLeftToRightCrouchingPose => TurningLeftToRightCrouchingAnimation,
            SamusPoseId.MoonwalkFacingLeftPose or
            SamusPoseId.MoonwalkFacingRightPose or
            SamusPoseId.MoonwalkAimUpLeftPose or
            SamusPoseId.MoonwalkAimUpRightPose or
            SamusPoseId.MoonwalkAimDownLeftPose or
            SamusPoseId.MoonwalkAimDownRightPose => MoonwalkFacingLeftAnimation,
            SamusPoseId.NeutralJumpTransitionRightPose => NeutralJumpTransitionRightAnimation,
            SamusPoseId.NeutralJumpTransitionLeftPose => NeutralJumpTransitionLeftAnimation,
            SamusPoseId.NeutralJumpRightPose or
            SamusPoseId.NeutralJumpLeftPose or
            SamusPoseId.ShinesparkWindupRightPose or
            SamusPoseId.ShinesparkWindupLeftPose => NeutralJumpRightAnimation,
            SamusPoseId.DamageBoostLeftPose or
            SamusPoseId.DamageBoostRightPose => DamageBoostLeftAnimation,
            SamusPoseId.NormalJumpForwardRightPose or
            SamusPoseId.NormalJumpForwardLeftPose => NormalJumpForwardRightAnimation,
            SamusPoseId.KnockbackRightPose or
            SamusPoseId.KnockbackLeftPose => KnockbackRightAnimation,
            SamusPoseId.NormalJumpTransitionAimUpRightPose => NormalJumpTransitionAimUpRightAnimation,
            SamusPoseId.NormalJumpTransitionAimUpLeftPose => NormalJumpTransitionAimUpLeftAnimation,
            SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose => NormalJumpTransitionAimDiagonalUpRightAnimation,
            SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose => NormalJumpTransitionAimDiagonalUpLeftAnimation,
            SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose => NormalJumpTransitionAimDiagonalDownRightAnimation,
            SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose => NormalJumpTransitionAimDiagonalDownLeftAnimation,
            SamusPoseId.UnusedPose63 => UnusedPose63Animation,
            SamusPoseId.UnusedPose64 => UnusedPose64Animation,
            SamusPoseId.UnusedPose65 or
            SamusPoseId.UnusedPose66 => UnusedPose65Animation,
            SamusPoseId.FallingGunExtendedRightPose or
            SamusPoseId.FallingGunExtendedLeftPose => FallingGunExtendedRightAnimation,
            SamusPoseId.FallingAimDiagonalUpRightPose or
            SamusPoseId.FallingAimDiagonalUpLeftPose or
            SamusPoseId.FallingAimDiagonalDownRightPose or
            SamusPoseId.FallingAimDiagonalDownLeftPose => FallingAimDiagonalUpRightAnimation,
            SamusPoseId.ScrewAttackRightPose or
            SamusPoseId.ScrewAttackLeftPose => ScrewAttackRightAnimation,
            SamusPoseId.WallJumpRightPose or
            SamusPoseId.WallJumpLeftPose => WallJumpRightAnimation,
            SamusPoseId.TurningRightToLeftFallingPose => TurningRightToLeftFallingAnimation,
            SamusPoseId.TurningLeftToRightFallingPose => TurningLeftToRightFallingAnimation,
            SamusPoseId.TurningRightToLeftAimUpPose => TurningRightToLeftAimUpAnimation,
            SamusPoseId.TurningLeftToRightAimUpPose => TurningLeftToRightAimUpAnimation,
            SamusPoseId.TurningRightToLeftAimDiagonalDownPose => TurningRightToLeftAimDiagonalDownAnimation,
            SamusPoseId.TurningLeftToRightAimDiagonalDownPose => TurningLeftToRightAimDiagonalDownAnimation,
            SamusPoseId.TurningRightToLeftJumpAimUpPose => TurningRightToLeftJumpAimUpAnimation,
            SamusPoseId.TurningLeftToRightJumpAimUpPose => TurningLeftToRightJumpAimUpAnimation,
            SamusPoseId.TurningRightToLeftJumpAimDownPose => TurningRightToLeftJumpAimDownAnimation,
            SamusPoseId.TurningLeftToRightJumpAimDownPose => TurningLeftToRightJumpAimDownAnimation,
            SamusPoseId.TurningRightToLeftFallingAimUpPose => TurningRightToLeftFallingAimUpAnimation,
            SamusPoseId.TurningLeftToRightFallingAimUpPose => TurningLeftToRightFallingAimUpAnimation,
            SamusPoseId.TurningRightToLeftFallingAimDownPose => TurningRightToLeftFallingAimDownAnimation,
            SamusPoseId.TurningLeftToRightFallingAimDownPose => TurningLeftToRightFallingAimDownAnimation,
            SamusPoseId.TurningRightToLeftCrouchingAimUpPose => TurningRightToLeftCrouchingAimUpAnimation,
            SamusPoseId.TurningLeftToRightCrouchingAimUpPose => TurningLeftToRightCrouchingAimUpAnimation,
            SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose => TurningRightToLeftCrouchingAimDiagonalDownAnimation,
            SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose => TurningLeftToRightCrouchingAimDiagonalDownAnimation,
            SamusPoseId.TurningRightToLeftAimDiagonalUpPose => TurningRightToLeftAimDiagonalUpAnimation,
            SamusPoseId.TurningLeftToRightAimDiagonalUpPose => TurningLeftToRightAimDiagonalUpAnimation,
            SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose => TurningRightToLeftJumpAimDiagonalUpAnimation,
            SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose => TurningLeftToRightJumpAimDiagonalUpAnimation,
            SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose => TurningRightToLeftFallingAimDiagonalUpAnimation,
            SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose => TurningLeftToRightFallingAimDiagonalUpAnimation,
            SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose => TurningRightToLeftCrouchingAimDiagonalUpAnimation,
            SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose => TurningLeftToRightCrouchingAimDiagonalUpAnimation,
            SamusPoseId.NormalLandingRightPose or
            SamusPoseId.FiringLandingRightPose => NormalLandingRightAnimation,
            SamusPoseId.NormalLandingLeftPose or
            SamusPoseId.FiringLandingLeftPose => NormalLandingLeftAnimation,
            SamusPoseId.SpinLandingRightPose => SpinLandingRightAnimation,
            SamusPoseId.SpinLandingLeftPose => SpinLandingLeftAnimation,
            SamusPoseId.GrappleStandingRightPose or
            SamusPoseId.GrappleStandingLeftPose or
            SamusPoseId.GrappleStandingDownRightPose or
            SamusPoseId.GrappleStandingDownLeftPose => GrappleStandingRightAnimation,
            SamusPoseId.UnusedPoseAC or
            SamusPoseId.UnusedPoseAD => UnusedPoseACAnimation,
            SamusPoseId.UnusedPoseAE or
            SamusPoseId.UnusedPoseAF => UnusedPoseAEAnimation,
            SamusPoseId.UnusedPoseB0 or
            SamusPoseId.UnusedPoseB1 => UnusedPoseB0Animation,
            SamusPoseId.GrappleSwingRightPose or
            SamusPoseId.GrappleSwingLeftPose => GrappleSwingRightAnimation,
            SamusPoseId.DraygonGrabbedMovingLeftPose or
            SamusPoseId.DraygonGrabbedMovingRightPose => DraygonGrabbedMovingLeftAnimation,
            SamusPoseId.MoonwalkTurnJumpLeftPose => MoonwalkTurnJumpLeftAnimation,
            SamusPoseId.MoonwalkTurnJumpRightPose => MoonwalkTurnJumpRightAnimation,
            SamusPoseId.MoonwalkTurnJumpAimUpLeftPose => MoonwalkTurnJumpAimUpLeftAnimation,
            SamusPoseId.MoonwalkTurnJumpAimUpRightPose => MoonwalkTurnJumpAimUpRightAnimation,
            SamusPoseId.MoonwalkTurnJumpAimDownLeftPose => MoonwalkTurnJumpAimDownLeftAnimation,
            SamusPoseId.MoonwalkTurnJumpAimDownRightPose => MoonwalkTurnJumpAimDownRightAnimation,
            SamusPoseId.UnusedPoseC6 => UnusedPoseC6Animation,
            SamusPoseId.ShinesparkHorizontalRightPose or
            SamusPoseId.ShinesparkHorizontalLeftPose or
            SamusPoseId.ShinesparkVerticalRightPose or
            SamusPoseId.ShinesparkVerticalLeftPose or
            SamusPoseId.ShinesparkDiagonalRightPose or
            SamusPoseId.ShinesparkDiagonalLeftPose => ShinesparkHorizontalRightAnimation,
            SamusPoseId.CrystalFlashRightPose => CrystalFlashRightAnimation,
            SamusPoseId.CrystalFlashLeftPose => CrystalFlashLeftAnimation,
            SamusPoseId.XrayingStandingRightPose or
            SamusPoseId.XrayingStandingLeftPose or
            SamusPoseId.XrayingCrouchingRightPose or
            SamusPoseId.XrayingCrouchingLeftPose => XrayingStandingRightAnimation,
            SamusPoseId.DeathSequenceRightPose or
            SamusPoseId.DeathSequenceLeftPose => DeathSequenceRightAnimation,
            SamusPoseId.UnusedPoseDb => UnusedPoseDbAnimation,
            SamusPoseId.UnusedPoseDc => UnusedPoseDcAnimation,
            SamusPoseId.UnusedPoseDd => UnusedPoseDdAnimation,
            SamusPoseId.UnusedPoseDe => UnusedPoseDeAnimation,
            SamusPoseId.LandingAimUpRightPose => LandingAimUpRightAnimation,
            SamusPoseId.LandingAimUpLeftPose => LandingAimUpLeftAnimation,
            SamusPoseId.LandingAimDiagonalUpRightPose => LandingAimDiagonalUpRightAnimation,
            SamusPoseId.LandingAimDiagonalUpLeftPose => LandingAimDiagonalUpLeftAnimation,
            SamusPoseId.LandingAimDiagonalDownRightPose => LandingAimDiagonalDownRightAnimation,
            SamusPoseId.LandingAimDiagonalDownLeftPose => LandingAimDiagonalDownLeftAnimation,
            SamusPoseId.DrainedCrouchingRightPose => DrainedCrouchingRightAnimation,
            SamusPoseId.DrainedCrouchingLeftPose => DrainedCrouchingLeftAnimation,
            SamusPoseId.DrainedStandingRightPose => DrainedStandingRightAnimation,
            SamusPoseId.DrainedStandingLeftPose => DrainedStandingLeftAnimation,
            SamusPoseId.CrouchingTransitionAimUpRightPose => CrouchingTransitionAimUpRightAnimation,
            SamusPoseId.CrouchingTransitionAimUpLeftPose => CrouchingTransitionAimUpLeftAnimation,
            SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose => CrouchingTransitionAimDiagonalUpRightAnimation,
            SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose => CrouchingTransitionAimDiagonalUpLeftAnimation,
            SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose => CrouchingTransitionAimDiagonalDownRightAnimation,
            SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose => CrouchingTransitionAimDiagonalDownLeftAnimation,
            SamusPoseId.StandingTransitionAimUpRightPose => StandingTransitionAimUpRightAnimation,
            SamusPoseId.StandingTransitionAimUpLeftPose => StandingTransitionAimUpLeftAnimation,
            SamusPoseId.StandingTransitionAimDiagonalUpRightPose => StandingTransitionAimDiagonalUpRightAnimation,
            SamusPoseId.StandingTransitionAimDiagonalUpLeftPose => StandingTransitionAimDiagonalUpLeftAnimation,
            SamusPoseId.StandingTransitionAimDiagonalDownRightPose => StandingTransitionAimDiagonalDownRightAnimation,
            SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => StandingTransitionAimDiagonalDownLeftAnimation,
            SamusPoseId.AdjacentCodePoseFD or
                SamusPoseId.AdjacentCodePoseFE or
                SamusPoseId.AdjacentCodePoseFF => throw new ArgumentOutOfRangeException(nameof(pose)),
            var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
        };
    }

    /// <summary>Read an immutable delay/command byte, retaining the reads that land in the transfer queue residue.</summary>
    /// <remarks>
    /// An index wraps within sixteen bits, so a program index past its end reads the following programs. An invalid pose's
    /// program starts at a window position in the bank's low half, which aliases the floor update residue.
    /// </remarks>
    /// <param name="floorUpdateResidue">The session's floor update residue, which a read in the bank's low half reads.</param>
    /// <param name="list">The program read.</param>
    /// <param name="byteIndex">The index into the program, which wraps within sixteen bits.</param>
    /// <param name="windows">Where the programs sit in their bank, which a read leaving them needs; null when none can.</param>
    internal static byte ReadAnimationByte(SuperMetroid.Core.Compatibility.IFloorUpdateResidue? floorUpdateResidue,
        SamusAnimationDelayList list, int byteIndex, SamusAnimationWindows? windows)
    {
        int position;
        if (list.Segment is { } segment)
        {
            int offset = unchecked((ushort)(SamusAnimationDelayPrograms.segments[segment].Offset + byteIndex));
            if (offset < ProgramsByteCount)
                return SamusAnimationDelayPrograms.ByteAt(offset);
            position = unchecked((ushort)(RequireWindows(windows).PoseDelayPrograms + offset));
        }
        else
        {
            // Invalid pose bytes $FD-$FF start their programs in the Mode 7 transfer queue's residue.
            position = unchecked((ushort)(list.ResidueStart + byteIndex));
        }
        if (position < SuperMetroid.Core.Compatibility.FloorUpdateResidue.ResidueWindowEnd)
            return (floorUpdateResidue ?? throw new InvalidOperationException(
                "An invalid pose's delay overread reads the floor update residue; bind the session's to this Samus."))
                .ByteAt(position - FloorUpdateResidueOffset);
        int programOffset = position - RequireWindows(windows).PoseDelayPrograms;
        if ((uint)programOffset < ProgramsByteCount)
            return SamusAnimationDelayPrograms.ByteAt(programOffset);
        throw new InvalidDataException(
            $"Samus animation delay read at window position ${position:X4} is outside the compiled pose programs.");
    }

    /// <summary>The installed window positions a read leaving the pose programs needs.</summary>
    private static SamusAnimationWindows RequireWindows(SamusAnimationWindows? windows) => windows ?? throw new InvalidOperationException(
        "An animation delay read leaving the compiled pose programs needs the installed Samus overread catalog's animation windows.");
}
