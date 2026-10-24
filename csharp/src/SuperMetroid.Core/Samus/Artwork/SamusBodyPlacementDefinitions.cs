using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Calculated pose-origin and selected-glyph support relationships for Samus landing, posture and drained transitions.</summary>
internal static class SamusBodyPlacementDefinitions
{
    /// <summary>
    /// $91:B62D + pose*8: the stock visual origin shares the native real-pose byte
    /// with the already calculated physical projectile correction. Installed visual
    /// overrides remain independent. FD..FF adjacent instruction observations are excluded.
    /// </summary>
    internal static sbyte DefaultGraphicsYOffset(SamusPoseId pose)
    {
        if ((int)pose >= SamusBodyArtworkCatalog.PoseCount) throw new ArgumentOutOfRangeException(nameof(pose));
        return unchecked((sbyte)SuperMetroid.Core.Samus.Poses.SamusPoseProjectileOriginDefinitions.ReadYOffset(pose));
    }
    /// <summary>$90:8D28..8D37: normal/spin landing, each with right/left four-byte rows.</summary>
    internal const int LandingPoseBytes = 4;
    /// <summary>$90:8D28..8D37: sixteen true landing bytes; $90:8D38 PLB is the separate narrowly retained observed byte.</summary>
    internal const int LandingDataBytes = 4 * LandingPoseBytes;
    /// <summary>$90:8D80..8D97: twelve transition poses with two bytes each, paired by facing.</summary>
    internal const int PosturePoseBytes = 2;
    /// <summary>$90:8D80..8D97: six right/left transition pairs, including two unused pairs.</summary>
    internal const int PostureDataBytes = 12 * PosturePoseBytes;
    /// <summary>The twelve $35-$40 posture poses whose rows $90:8D80..8D97 hold, in table order.</summary>
    private static readonly SuperMetroid.Core.Samus.Poses.SamusPoseId[] PostureBlockPoses =
    [
        SuperMetroid.Core.Samus.Poses.SamusPoseId.CrouchingTransitionRightPose, SuperMetroid.Core.Samus.Poses.SamusPoseId.CrouchingTransitionLeftPose,
        SuperMetroid.Core.Samus.Poses.SamusPoseId.MorphingTransitionRightPose, SuperMetroid.Core.Samus.Poses.SamusPoseId.MorphingTransitionLeftPose,
        SuperMetroid.Core.Samus.Poses.SamusPoseId.UnusedPose39, SuperMetroid.Core.Samus.Poses.SamusPoseId.UnusedPose3A,
        SuperMetroid.Core.Samus.Poses.SamusPoseId.StandingTransitionRightPose, SuperMetroid.Core.Samus.Poses.SamusPoseId.StandingTransitionLeftPose,
        SuperMetroid.Core.Samus.Poses.SamusPoseId.UnmorphingTransitionRightPose, SuperMetroid.Core.Samus.Poses.SamusPoseId.UnmorphingTransitionLeftPose,
        SuperMetroid.Core.Samus.Poses.SamusPoseId.UnusedPose3F, SuperMetroid.Core.Samus.Poses.SamusPoseId.UnusedPose40,
    ];

    /// <summary>Landing left-facing copies use their right-facing row; each source default follows the named landing/recovery phase.</summary>
    internal static int LandingSourceIndex(int index)
    {
        if ((uint)index > LandingDataBytes) throw new ArgumentOutOfRangeException(nameof(index));
        return index == LandingDataBytes ? index : index / (2 * LandingPoseBytes) * (2 * LandingPoseBytes) + index % LandingPoseBytes;
    }

    /// <summary>$90:8D38: exact PLB instruction encoding observed as the high byte of the final admitted unaligned word.</summary>
    /// <remarks>Only this one byte is retained as unrelated native instruction content; no memory beyond the17-byte window is modeled.</remarks>
    private const byte AdjacentLandingPlb = 0xAB;

    /// <summary>$90:8D28..8D38: named landing compression, standing recovery, inactive padding and adjacent instruction observation.</summary>
    internal static byte DefaultLandingByte(int index)
    {
        int source = LandingSourceIndex(index);
        if (source == LandingDataBytes) return AdjacentLandingPlb;
        bool spin = source >= 2 * LandingPoseBytes;
        int frame = source % LandingPoseBytes;
        int recoveryFrame = spin ? 2 : 1;
        if (frame > recoveryFrame) return 0;
        SuperMetroid.Core.Samus.Poses.SamusPoseId pose = frame == recoveryFrame ? SuperMetroid.Core.Samus.Poses.SamusPoseId.FacingRightNormalPose
            : spin ? SuperMetroid.Core.Samus.Poses.SamusPoseId.SpinLandingRightPose : SuperMetroid.Core.Samus.Poses.SamusPoseId.NormalLandingRightPose;
        return unchecked((byte)DefaultGraphicsYOffset(pose));
    }
    /// <summary>$90:8D8C: selected standing-transition join one pixel above its destination support; narrowly retained visual composition.</summary>
    private const int StandingSupportJoin = -1;
    /// <summary>$90:8D91: selected second-unmorph join one pixel below crouching support; narrowly retained visual composition.</summary>
    private const int UnmorphSupportJoin = 1;

    /// <summary>$91:B378: eight ordinary Morph Ball rolling frames before the loop command; both facing sequences share this domain.</summary>
    private const int RollingSupportFrames = 8;
    /// <summary>$90:8D80..8D97: align each selected transition's opaque support to its destination pose.</summary>
    /// <remarks>Unavailable or independently edited non-body geometry remains supplied content rather than narrowing the editable schema.</remarks>
    internal static bool TryDefaultPostureByte(SamusBodyArtworkCatalog art, int index, out sbyte value)
    {
        int source = PostureSourceIndex(index);
        SuperMetroid.Core.Samus.Poses.SamusPoseId pose = PostureBlockPoses[source / PosturePoseBytes];
        int frame = source % PosturePoseBytes;
        SuperMetroid.Core.Samus.Poses.SamusPoseId target;
        bool sourceBottom;
        int join = 0;
        switch (pose)
        {
            case SuperMetroid.Core.Samus.Poses.SamusPoseId.CrouchingTransitionRightPose:
                if (frame != 0) { value = 0; return true; }
                target = SuperMetroid.Core.Samus.Poses.SamusPoseId.CrouchingRightPose; sourceBottom = true;
                break;
            case SuperMetroid.Core.Samus.Poses.SamusPoseId.StandingTransitionRightPose:
                if (frame != 0) { value = 0; return true; }
                target = SuperMetroid.Core.Samus.Poses.SamusPoseId.FacingRightNormalPose; sourceBottom = true; join = StandingSupportJoin;
                break;
            case SuperMetroid.Core.Samus.Poses.SamusPoseId.MorphingTransitionRightPose:
                target = SuperMetroid.Core.Samus.Poses.SamusPoseId.MorphBallGroundRightPose; sourceBottom = false;
                break;
            case SuperMetroid.Core.Samus.Poses.SamusPoseId.UnmorphingTransitionRightPose:
                target = SuperMetroid.Core.Samus.Poses.SamusPoseId.CrouchingRightPose; sourceBottom = false;
                join = frame == 1 ? UnmorphSupportJoin : 0;
                break;
            // The rest of the $35-$40 posture block derives no support row.
            case SuperMetroid.Core.Samus.Poses.SamusPoseId.CrouchingTransitionLeftPose:
            case SuperMetroid.Core.Samus.Poses.SamusPoseId.MorphingTransitionLeftPose:
            case SuperMetroid.Core.Samus.Poses.SamusPoseId.UnusedPose39:
            case SuperMetroid.Core.Samus.Poses.SamusPoseId.UnusedPose3A:
            case SuperMetroid.Core.Samus.Poses.SamusPoseId.StandingTransitionLeftPose:
            case SuperMetroid.Core.Samus.Poses.SamusPoseId.UnmorphingTransitionLeftPose:
            case SuperMetroid.Core.Samus.Poses.SamusPoseId.UnusedPose3F:
            case SuperMetroid.Core.Samus.Poses.SamusPoseId.UnusedPose40:
                value = 0; return true;
            case SamusPoseId.AdjacentCodePoseFD:
            case SamusPoseId.AdjacentCodePoseFE:
            case SamusPoseId.AdjacentCodePoseFF:
            case SamusPoseId.ForwardFacingPowerSuitPose:
            case SamusPoseId.ForwardFacingSuitedPose:
            case SamusPoseId.FacingRightNormalPose:
            case SamusPoseId.FacingLeftNormalPose:
            case SamusPoseId.MovingRightNormalPose:
            case SamusPoseId.MovingLeftNormalPose:
            case SamusPoseId.MovingRightGunExtendedPose:
            case SamusPoseId.MovingLeftGunExtendedPose:
            case SamusPoseId.UnusedPose65:
            case SamusPoseId.UnusedPose66:
            case SamusPoseId.MoonwalkFacingLeftPose:
            case SamusPoseId.MoonwalkFacingRightPose:
            case SamusPoseId.MoonwalkAimUpLeftPose:
            case SamusPoseId.MoonwalkAimUpRightPose:
            case SamusPoseId.MoonwalkAimDownLeftPose:
            case SamusPoseId.MoonwalkAimDownRightPose:
            case SamusPoseId.MoonwalkTurnJumpLeftPose:
            case SamusPoseId.MoonwalkTurnJumpRightPose:
            case SamusPoseId.MoonwalkTurnJumpAimUpLeftPose:
            case SamusPoseId.MoonwalkTurnJumpAimUpRightPose:
            case SamusPoseId.MoonwalkTurnJumpAimDownLeftPose:
            case SamusPoseId.MoonwalkTurnJumpAimDownRightPose:
            case SamusPoseId.ShinesparkWindupRightPose:
            case SamusPoseId.ShinesparkWindupLeftPose:
            case SamusPoseId.ShinesparkHorizontalRightPose:
            case SamusPoseId.ShinesparkHorizontalLeftPose:
            case SamusPoseId.ShinesparkVerticalRightPose:
            case SamusPoseId.ShinesparkVerticalLeftPose:
            case SamusPoseId.ShinesparkDiagonalRightPose:
            case SamusPoseId.ShinesparkDiagonalLeftPose:
            case SamusPoseId.CrystalFlashRightPose:
            case SamusPoseId.CrystalFlashLeftPose:
            case SamusPoseId.XrayingStandingRightPose:
            case SamusPoseId.XrayingStandingLeftPose:
            case SamusPoseId.DeathSequenceRightPose:
            case SamusPoseId.DeathSequenceLeftPose:
            case SamusPoseId.XrayingCrouchingRightPose:
            case SamusPoseId.XrayingCrouchingLeftPose:
            case SamusPoseId.UnusedPoseDb:
            case SamusPoseId.UnusedPoseDc:
            case SamusPoseId.UnusedPoseDd:
            case SamusPoseId.UnusedPoseDe:
            case SamusPoseId.UnusedPoseDf:
            case SamusPoseId.DrainedCrouchingRightPose:
            case SamusPoseId.DrainedCrouchingLeftPose:
            case SamusPoseId.DrainedStandingRightPose:
            case SamusPoseId.DrainedStandingLeftPose:
            case SamusPoseId.RanIntoWallRightPose:
            case SamusPoseId.RanIntoWallLeftPose:
            case SamusPoseId.RanIntoWallAimUpRightPose:
            case SamusPoseId.RanIntoWallAimUpLeftPose:
            case SamusPoseId.RanIntoWallAimDownRightPose:
            case SamusPoseId.RanIntoWallAimDownLeftPose:
            case SamusPoseId.TurningRightToLeftPose:
            case SamusPoseId.TurningLeftToRightPose:
            case SamusPoseId.SpinJumpRightPose:
            case SamusPoseId.SpinJumpLeftPose:
            case SamusPoseId.SpaceJumpRightPose:
            case SamusPoseId.SpaceJumpLeftPose:
            case SamusPoseId.ScrewAttackRightPose:
            case SamusPoseId.ScrewAttackLeftPose:
            case SamusPoseId.TurningRightToLeftJumpPose:
            case SamusPoseId.TurningLeftToRightJumpPose:
            case SamusPoseId.WallJumpRightPose:
            case SamusPoseId.WallJumpLeftPose:
            case SamusPoseId.GrappleSwingRightPose:
            case SamusPoseId.GrappleSwingLeftPose:
            case SamusPoseId.GrappleStandingRightPose:
            case SamusPoseId.GrappleStandingLeftPose:
            case SamusPoseId.GrappleStandingDownRightPose:
            case SamusPoseId.GrappleStandingDownLeftPose:
            case SamusPoseId.GrappleCrouchingRightPose:
            case SamusPoseId.GrappleCrouchingLeftPose:
            case SamusPoseId.GrappleCrouchingDownRightPose:
            case SamusPoseId.GrappleCrouchingDownLeftPose:
            case SamusPoseId.GrappleWallContactLeftPose:
            case SamusPoseId.GrappleWallContactRightPose:
            case SamusPoseId.DraygonGrabbedNeutralLeftPose:
            case SamusPoseId.DraygonGrabbedAimUpLeftPose:
            case SamusPoseId.DraygonGrabbedFiringLeftPose:
            case SamusPoseId.DraygonGrabbedAimDownLeftPose:
            case SamusPoseId.DraygonGrabbedMovingLeftPose:
            case SamusPoseId.DraygonGrabbedNeutralRightPose:
            case SamusPoseId.DraygonGrabbedAimUpRightPose:
            case SamusPoseId.DraygonGrabbedFiringRightPose:
            case SamusPoseId.DraygonGrabbedAimDownRightPose:
            case SamusPoseId.DraygonGrabbedMovingRightPose:
            case SamusPoseId.DamageBoostLeftPose:
            case SamusPoseId.DamageBoostRightPose:
            case SamusPoseId.KnockbackRightPose:
            case SamusPoseId.KnockbackLeftPose:
            case SamusPoseId.TurningRightToLeftFallingPose:
            case SamusPoseId.TurningLeftToRightFallingPose:
            case SamusPoseId.TurningRightToLeftJumpAimUpPose:
            case SamusPoseId.TurningLeftToRightJumpAimUpPose:
            case SamusPoseId.TurningRightToLeftJumpAimDownPose:
            case SamusPoseId.TurningLeftToRightJumpAimDownPose:
            case SamusPoseId.TurningRightToLeftFallingAimUpPose:
            case SamusPoseId.TurningLeftToRightFallingAimUpPose:
            case SamusPoseId.TurningRightToLeftFallingAimDownPose:
            case SamusPoseId.TurningLeftToRightFallingAimDownPose:
            case SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose:
            case SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose:
            case SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose:
            case SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose:
            case SamusPoseId.MorphBallGroundRightPose:
            case SamusPoseId.MorphBallMovingRightPose:
            case SamusPoseId.MorphBallMovingLeftPose:
            case SamusPoseId.MorphBallFallingRightPose:
            case SamusPoseId.MorphBallFallingLeftPose:
            case SamusPoseId.UnusedKnockbackRightPose:
            case SamusPoseId.UnusedKnockbackLeftPose:
            case SamusPoseId.MorphBallGroundLeftPose:
            case SamusPoseId.SpringBallGroundRightPose:
            case SamusPoseId.SpringBallGroundLeftPose:
            case SamusPoseId.SpringBallMovingRightPose:
            case SamusPoseId.SpringBallMovingLeftPose:
            case SamusPoseId.SpringBallFallingRightPose:
            case SamusPoseId.SpringBallFallingLeftPose:
            case SamusPoseId.SpringBallJumpRightPose:
            case SamusPoseId.SpringBallJumpLeftPose:
            case SamusPoseId.FallingRightPose:
            case SamusPoseId.FallingLeftPose:
            case SamusPoseId.FallingGunExtendedRightPose:
            case SamusPoseId.FallingGunExtendedLeftPose:
            case SamusPoseId.StandingAimUpRightPose:
            case SamusPoseId.StandingAimUpLeftPose:
            case SamusPoseId.StandingAimDiagonalUpRightPose:
            case SamusPoseId.StandingAimDiagonalUpLeftPose:
            case SamusPoseId.StandingAimDiagonalDownRightPose:
            case SamusPoseId.StandingAimDiagonalDownLeftPose:
            case SamusPoseId.RunningAimUpRightPose:
            case SamusPoseId.RunningAimUpLeftPose:
            case SamusPoseId.RunningAimDiagonalUpRightPose:
            case SamusPoseId.RunningAimDiagonalUpLeftPose:
            case SamusPoseId.RunningAimDiagonalDownRightPose:
            case SamusPoseId.RunningAimDiagonalDownLeftPose:
            case SamusPoseId.NormalJumpGunExtendedRightPose:
            case SamusPoseId.NormalJumpGunExtendedLeftPose:
            case SamusPoseId.NormalJumpAimUpRightPose:
            case SamusPoseId.NormalJumpAimUpLeftPose:
            case SamusPoseId.NormalJumpAimDownRightPose:
            case SamusPoseId.NormalJumpAimDownLeftPose:
            case SamusPoseId.NormalJumpForwardRightPose:
            case SamusPoseId.NormalJumpForwardLeftPose:
            case SamusPoseId.NormalJumpTransitionAimUpRightPose:
            case SamusPoseId.NormalJumpTransitionAimUpLeftPose:
            case SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose:
            case SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose:
            case SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose:
            case SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose:
            case SamusPoseId.NormalJumpAimDiagonalUpRightPose:
            case SamusPoseId.NormalJumpAimDiagonalUpLeftPose:
            case SamusPoseId.NormalJumpAimDiagonalDownRightPose:
            case SamusPoseId.NormalJumpAimDiagonalDownLeftPose:
            case SamusPoseId.FallingAimUpRightPose:
            case SamusPoseId.FallingAimUpLeftPose:
            case SamusPoseId.FallingAimDownRightPose:
            case SamusPoseId.FallingAimDownLeftPose:
            case SamusPoseId.FallingAimDiagonalUpRightPose:
            case SamusPoseId.FallingAimDiagonalUpLeftPose:
            case SamusPoseId.FallingAimDiagonalDownRightPose:
            case SamusPoseId.FallingAimDiagonalDownLeftPose:
            case SamusPoseId.CrouchingAimDiagonalUpRightPose:
            case SamusPoseId.CrouchingAimDiagonalUpLeftPose:
            case SamusPoseId.CrouchingAimDiagonalDownRightPose:
            case SamusPoseId.CrouchingAimDiagonalDownLeftPose:
            case SamusPoseId.CrouchingAimUpRightPose:
            case SamusPoseId.CrouchingAimUpLeftPose:
            case SamusPoseId.TurningRightToLeftCrouchingPose:
            case SamusPoseId.TurningLeftToRightCrouchingPose:
            case SamusPoseId.TurningRightToLeftAimUpPose:
            case SamusPoseId.TurningLeftToRightAimUpPose:
            case SamusPoseId.TurningRightToLeftAimDiagonalDownPose:
            case SamusPoseId.TurningLeftToRightAimDiagonalDownPose:
            case SamusPoseId.TurningRightToLeftAimDiagonalUpPose:
            case SamusPoseId.TurningLeftToRightAimDiagonalUpPose:
            case SamusPoseId.TurningRightToLeftCrouchingAimUpPose:
            case SamusPoseId.TurningLeftToRightCrouchingAimUpPose:
            case SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose:
            case SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose:
            case SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose:
            case SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose:
            case SamusPoseId.LandingAimUpRightPose:
            case SamusPoseId.LandingAimUpLeftPose:
            case SamusPoseId.LandingAimDiagonalUpRightPose:
            case SamusPoseId.LandingAimDiagonalUpLeftPose:
            case SamusPoseId.LandingAimDiagonalDownRightPose:
            case SamusPoseId.LandingAimDiagonalDownLeftPose:
            case SamusPoseId.FiringLandingRightPose:
            case SamusPoseId.FiringLandingLeftPose:
            case SamusPoseId.CrouchingTransitionAimUpRightPose:
            case SamusPoseId.CrouchingTransitionAimUpLeftPose:
            case SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose:
            case SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose:
            case SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose:
            case SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose:
            case SamusPoseId.StandingTransitionAimUpRightPose:
            case SamusPoseId.StandingTransitionAimUpLeftPose:
            case SamusPoseId.StandingTransitionAimDiagonalUpRightPose:
            case SamusPoseId.StandingTransitionAimDiagonalUpLeftPose:
            case SamusPoseId.StandingTransitionAimDiagonalDownRightPose:
            case SamusPoseId.StandingTransitionAimDiagonalDownLeftPose:
            case SamusPoseId.CrouchingRightPose:
            case SamusPoseId.CrouchingLeftPose:
            case SamusPoseId.NeutralJumpTransitionRightPose:
            case SamusPoseId.NeutralJumpTransitionLeftPose:
            case SamusPoseId.NeutralJumpRightPose:
            case SamusPoseId.NeutralJumpLeftPose:
            case SamusPoseId.NormalLandingRightPose:
            case SamusPoseId.NormalLandingLeftPose:
            case SamusPoseId.SpinLandingRightPose:
            case SamusPoseId.SpinLandingLeftPose:
            case SamusPoseId.UnusedPose20:
            case SamusPoseId.UnusedPose21:
            case SamusPoseId.UnusedPose22:
            case SamusPoseId.UnusedPose23:
            case SamusPoseId.UnusedPose24:
            case SamusPoseId.UnusedPose42:
            case SamusPoseId.UnusedPose45:
            case SamusPoseId.UnusedPose46:
            case SamusPoseId.UnusedPose47:
            case SamusPoseId.UnusedPose48:
            case SamusPoseId.UnusedPose5B:
            case SamusPoseId.UnusedPose5C:
            case SamusPoseId.UnusedPose5D:
            case SamusPoseId.UnusedPose5E:
            case SamusPoseId.UnusedPose5F:
            case SamusPoseId.UnusedPose60:
            case SamusPoseId.UnusedPose61:
            case SamusPoseId.UnusedPose62:
            case SamusPoseId.UnusedPose63:
            case SamusPoseId.UnusedPose64:
            case SamusPoseId.UnusedPoseAC:
            case SamusPoseId.UnusedPoseAD:
            case SamusPoseId.UnusedPoseAE:
            case SamusPoseId.UnusedPoseAF:
            case SamusPoseId.UnusedPoseB0:
            case SamusPoseId.UnusedPoseB1:
            case SamusPoseId.UnusedPoseC5:
            case SamusPoseId.UnusedPoseC6:
            default:
                throw new InvalidOperationException($"{pose} is outside the posture source block.");
        }
        value = 0;
        try
        {
            bool targetBottom = target != SuperMetroid.Core.Samus.Poses.SamusPoseId.MorphBallGroundRightPose;
            if (!TryOpaqueBottom(art, pose, (ushort)frame, sourceBottom, out int sourceY) ||
                !TryOpaqueBottom(art, target, 0, targetBottom, out int targetY)) return false;
            if (!targetBottom)
            {
                // Rolling frames bob by one pixel in either direction. Align the
                // incoming morph to the support envelope, independent of facing's
                // initial rolling phase, rather than selecting a sampled target Y.
                for (ushort phase = 1; phase < RollingSupportFrames; phase++)
                {
                    if (!TryOpaqueBottom(art, target, phase, false, out int phaseBottom)) return false;
                    targetY = Math.Max(targetY, phaseBottom);
                }
            }
            int offset = targetY - art.GraphicsYOffset(target) - sourceY + join;
            if (offset is < sbyte.MinValue or > sbyte.MaxValue) return false;
            value = (sbyte)offset;
            return true;
        }
        catch (InvalidDataException)
        {
            // Edited frame identities may be admitted by the document yet unavailable
            // until selected for drawing. Preserve the supplied offset independently.
            return false;
        }
    }

    /// <summary>$90:8DEF/8DF0: narrowly retained common support of the two upper-half-only compact drained poses; selected scene composition.</summary>
    private const int DrainedCompactSupport = 17;
    /// <summary>$90:8DF6: narrowly retained impact support before the kneeling cycle; selected scene composition.</summary>
    private const int DrainedImpactSupport = 18;

    /// <summary>$90:8DEF..8E0E: shared drained placement, indexed by byte position in the complete $91:B268 left-facing program.</summary>
    internal static bool TryDefaultDrainedByte(SamusBodyArtworkCatalog art, int index, out sbyte value)
    {
        if ((uint)index >= SuperMetroid.Core.Samus.Drawing.SamusDrawingDefinitions.BodyOffsets.DrainedVerticalOffsetByteCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        value = 0;
        const SamusPoseId pose = SuperMetroid.Core.Samus.Poses.SamusPoseId.DrainedCrouchingLeftPose;
        ushort frame;
        switch (index)
        {
            // Repeated kneeling, tucked recovery, hit and Hyper Beam poses use
            // the drained pose's ordinary drawing origin. Command-only slots
            // remain zero even when a controller briefly publishes their index.
            case 8 or 9 or 10 or 11 or 14 or 19 or 23 or 26 or 29:
                int usual = -art.GraphicsYOffset(pose);
                if (usual > sbyte.MaxValue) return false;
                value = (sbyte)usual; return true;
            case 0 or 1: frame = (ushort)index; break;
            case >= 2 and <= 6: frame = 2; break;
            case 7: frame = 7; break;
            case 15 or 20 or 22: frame = 15; break;
            case 16 or 21: frame = 16; break;
            default: return true;
        }
        try
        {
            if (!TryOpaqueBottom(art, pose, frame, frame >= 2, out int sourceY)) return false;
            int targetY;
            if (frame < 2) targetY = DrainedCompactSupport;
            else if (frame == 7) targetY = DrainedImpactSupport;
            else if (frame == 16)
                // This final rise selects the collision boundary itself, while
                // falling/intermediate rise select standing's visible support.
                targetY = SuperMetroid.Core.Samus.Poses.SamusPoseCollisionDefinitions.ReadVerticalRadius(pose);
            else
            {
                const SamusPoseId standing = SuperMetroid.Core.Samus.Poses.SamusPoseId.FacingLeftNormalPose;
                if (!TryOpaqueBottom(art, standing, 0, true, out targetY)) return false;
                targetY -= art.GraphicsYOffset(standing);
            }
            int offset = targetY - sourceY;
            if (offset is < sbyte.MinValue or > sbyte.MaxValue) return false;
            value = (sbyte)offset;
            return true;
        }
        catch (InvalidDataException) { return false; }
    }

    /// <summary>Lays out the frame's selected top and bottom definitions as body characters and finds the lowest opaque sprite pixel; declines incomplete or unsupported artwork.</summary>
    private static bool TryOpaqueBottom(SamusBodyArtworkCatalog art, SamusPoseId pose, ushort frame,
        bool drawBottom, out int bottom)
    {
        // The window reaches one whole definition past the start of the last half row a definition's part fills.
        int windowBytes = BodyPartByteOffset(SamusBodyHalf.Bottom, BodyTilePart.Second) + SamusBodyArtworkCatalog.BytesPerDefinitionSlot;
        Span<byte> pixels = stackalloc byte[windowBytes];
        Span<bool> defined = stackalloc bool[windowBytes];
        pixels.Clear(); defined.Clear();
        SamusBodyFrameSelection selected = art.Frame(pose, frame);
        Lay(art.GetDefinition(true, selected.TopSet, selected.TopPosition), SamusBodyHalf.Top, pixels, defined);
        if (selected.NamesBottom)
            Lay(art.GetDefinition(false, selected.BottomSet, selected.BottomPosition), SamusBodyHalf.Bottom, pixels, defined);
        bottom = int.MinValue;
        for (int half = 0; half < (drawBottom ? 2 : 1); half++)
        {
            int baseIndex = half == 0 ? art.Spritemaps.TopBase(pose) : art.Spritemaps.BottomBase(pose);
            int index = baseIndex + frame;
            if ((uint)index >= SamusSpritemapArtworkCatalog.PointerCount ||
                !art.Spritemaps.TryGet((ushort)index, out SamusSpritemapDefinition? map)) return false;
            foreach (SamusSpritePart part in map.Parts)
            {
                int size = part.X.IsLarge ? 16 : 8;
                var attributes = part.Attributes;
                if (attributes.TileNumber >= 256) return false;
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    int sx = attributes.FlipHorizontally ? size - 1 - x : x;
                    int sy = attributes.FlipVertically ? size - 1 - y : y;
                    int tile = (attributes.TileNumber + (sy / 8) * 16 + sx / 8) & 255;
                    int row = tile * 32 + (sy % 8) * 2;
                    int mask = 128 >> (sx % 8);
                    bool opaque = false;
                    for (int plane = 0; plane < 4; plane++)
                    {
                        int at = row + plane % 2 + plane / 2 * 16;
                        if ((uint)at >= pixels.Length || !defined[at]) return false;
                        opaque |= (pixels[at] & mask) != 0;
                    }
                    if (opaque) bottom = Math.Max(bottom, part.Y.SignedOffset + y);
                }
            }
        }
        return bottom != int.MinValue;

        // Copy both planar parts of a definition to the body characters they fill and mark
        // their bytes available for opacity sampling in this local calculation.
        static void Lay(SamusBodyTileDefinition definition, SamusBodyHalf half, Span<byte> data, Span<bool> coverage)
        {
            ReadOnlySpan<byte> source = definition.Planar.Span;
            int first = BodyPartByteOffset(half, BodyTilePart.First), second = BodyPartByteOffset(half, BodyTilePart.Second);
            source[..definition.FirstSize].CopyTo(data[first..]);
            coverage.Slice(first, definition.FirstSize).Fill(true);
            source[definition.FirstSize..].CopyTo(data[second..]);
            coverage.Slice(second, definition.SecondSize).Fill(true);
        }
    }
    /// <summary>
    /// The byte at which a definition's part starts among the body's characters, as
    /// <see cref="SamusBodyArtworkCatalog.BodyArtwork"/> lays them out: the first part fills a half row, the top's from
    /// character 0 and the bottom's from the middle of the row, and the second part the half row one full row below.
    /// </summary>
    private static int BodyPartByteOffset(SamusBodyHalf half, BodyTilePart part) =>
        ((half == SamusBodyHalf.Top ? 0 : SpriteArtwork.CharactersPerRow / 2) + (part == BodyTilePart.Second ? SpriteArtwork.CharactersPerRow : 0)) *
        SpriteArtwork.CharacterByteCount;

    /// <summary>One of the two transfers a body tile definition's characters arrive in.</summary>
    private enum BodyTilePart
    {
        /// <summary>The first transfer, filling a half row.</summary>
        First,

        /// <summary>The second transfer, filling the half row one full row below.</summary>
        Second,
    }

    /// <summary>Transition left-facing copies use their right-facing row; source defaults derive from selected support geometry.</summary>
    internal static int PostureSourceIndex(int index)
    {
        if ((uint)index >= PostureDataBytes) throw new ArgumentOutOfRangeException(nameof(index));
        return index / (2 * PosturePoseBytes) * (2 * PosturePoseBytes) + index % PosturePoseBytes;
    }
}
