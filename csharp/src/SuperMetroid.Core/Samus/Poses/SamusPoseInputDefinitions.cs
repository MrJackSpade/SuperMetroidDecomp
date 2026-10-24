using System.Collections.Frozen;
using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputList;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Native pose-to-input dispatch and selected-condition diagnostics; no stored transition rows.</summary>
internal static class SamusPoseInputDefinitions
{
    /// <summary>The lists in the later native graph section, whose rules <see cref="SamusPoseInputRulesLate"/> holds.</summary>
    private static readonly FrozenSet<SamusPoseInputList> LaterLists = new[]
    {
        SamusPoseInputList.SpringBallGroundRightPoseList,
        SamusPoseInputList.SpringBallGroundLeftPoseList,
        SamusPoseInputList.SpringBallFallingRightPoseList,
        SamusPoseInputList.SpringBallFallingLeftPoseList,
        SamusPoseInputList.SpringBallJumpRightPoseList,
        SamusPoseInputList.SpringBallJumpLeftPoseList,
        SamusPoseInputList.UnusedPose63List,
        SamusPoseInputList.UnusedPose64List,
        SamusPoseInputList.UnusedPose65List,
        SamusPoseInputList.UnusedPose66List,
        SamusPoseInputList.WallJumpRightPoseList,
        SamusPoseInputList.WallJumpLeftPoseList,
        SamusPoseInputList.RanIntoWallRightPoseList,
        SamusPoseInputList.RanIntoWallLeftPoseList,
        SamusPoseInputList.NormalJumpGunExtendedRightPoseList,
        SamusPoseInputList.NormalJumpGunExtendedLeftPoseList,
        SamusPoseInputList.NormalJumpAimDownRightPoseList,
        SamusPoseInputList.NormalJumpAimDownLeftPoseList,
        SamusPoseInputList.UnmorphingTransitionRightPoseList,
        SamusPoseInputList.UnmorphingTransitionLeftPoseList,
        SamusPoseInputList.TurningRightToLeftPoseList,
        SamusPoseInputList.TurningLeftToRightPoseList,
        SamusPoseInputList.TurningRightToLeftAimUpPoseList,
        SamusPoseInputList.TurningLeftToRightAimUpPoseList,
        SamusPoseInputList.TurningRightToLeftAimDiagonalDownPoseList,
        SamusPoseInputList.TurningLeftToRightAimDiagonalDownPoseList,
        SamusPoseInputList.ShinesparkWindupRightPoseList,
        SamusPoseInputList.ShinesparkWindupLeftPoseList,
        SamusPoseInputList.FallingAimDownRightPoseList,
        SamusPoseInputList.FallingAimDownLeftPoseList,
        SamusPoseInputList.UnusedPoseDfList,
        SamusPoseInputList.DraygonGrabbedNeutralLeftPoseList,
        SamusPoseInputList.DraygonGrabbedNeutralRightPoseList,
        SamusPoseInputList.MovingRightGunExtendedPoseList,
        SamusPoseInputList.MovingLeftGunExtendedPoseList,
        SamusPoseInputList.FallingGunExtendedRightPoseList,
        SamusPoseInputList.FallingGunExtendedLeftPoseList,
        SamusPoseInputList.MoonwalkTurnJumpLeftPoseList,
        SamusPoseInputList.MoonwalkTurnJumpRightPoseList,
        SamusPoseInputList.MoonwalkTurnJumpAimUpLeftPoseList,
        SamusPoseInputList.MoonwalkTurnJumpAimUpRightPoseList,
        SamusPoseInputList.MoonwalkTurnJumpAimDownLeftPoseList,
        SamusPoseInputList.MoonwalkTurnJumpAimDownRightPoseList,
    }.ToFrozenSet();
    /// <summary>Pose $45, UNUSED_TransitionTable_45_91A7CC: preserved unused transition target.</summary>
    internal const SamusPoseId UnusedPose45 = (SamusPoseId)0x45;
    /// <summary>Pose $46, UNUSED_TransitionTable_46_91A7E0: preserved unused transition target.</summary>
    internal const SamusPoseId UnusedPose46 = (SamusPoseId)0x46;

    /// <summary>Pose $39, TransitionTable pointer at $91:9F54: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose39 = (SamusPoseId)0x39;
    /// <summary>Pose $3A, TransitionTable pointer at $91:9F56: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose3A = (SamusPoseId)0x3a;
    /// <summary>Pose $3F, TransitionTable pointer at $91:9F60: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose3F = (SamusPoseId)0x3f;
    /// <summary>Pose $40, TransitionTable pointer at $91:9F62: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose40 = (SamusPoseId)0x40;
    /// <summary>Pose $5D, TransitionTable pointer at $91:9F9C: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose5D = (SamusPoseId)0x5d;
    /// <summary>Pose $5E, TransitionTable pointer at $91:9F9E: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose5E = (SamusPoseId)0x5e;
    /// <summary>Pose $5F, TransitionTable pointer at $91:9FA0: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose5F = (SamusPoseId)0x5f;
    /// <summary>Pose $60, TransitionTable pointer at $91:9FA2: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose60 = (SamusPoseId)0x60;
    /// <summary>Pose $61, TransitionTable pointer at $91:9FA4: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose61 = (SamusPoseId)0x61;
    /// <summary>Pose $62, TransitionTable pointer at $91:9FA6: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose62 = (SamusPoseId)0x62;
    /// <summary>Pose $AC, TransitionTable pointer at $91:A03A: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePoseAC = (SamusPoseId)0xac;
    /// <summary>Pose $AD, TransitionTable pointer at $91:A03C: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePoseAD = (SamusPoseId)0xad;
    /// <summary>Pose $AE, TransitionTable pointer at $91:A03E: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePoseAE = (SamusPoseId)0xae;
    /// <summary>Pose $AF, TransitionTable pointer at $91:A040: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePoseAF = (SamusPoseId)0xaf;
    /// <summary>Pose $B0, TransitionTable pointer at $91:A042: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePoseB0 = (SamusPoseId)0xb0;
    /// <summary>Pose $B1, TransitionTable pointer at $91:A044: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePoseB1 = (SamusPoseId)0xb1;
    /// <summary>Pose $C5, TransitionTable pointer at $91:A06C: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePoseC5 = (SamusPoseId)0xc5;
    /// <summary>Pose $C6, TransitionTable pointer at $91:A06E: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePoseC6 = (SamusPoseId)0xc6;
    /// <summary>Pose $20, TransitionTable pointer at $91:9F22: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose20 = (SamusPoseId)0x20;
    /// <summary>Pose $21, TransitionTable pointer at $91:9F24: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose21 = (SamusPoseId)0x21;
    /// <summary>Pose $22, TransitionTable pointer at $91:9F26: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose22 = (SamusPoseId)0x22;
    /// <summary>Pose $24, TransitionTable pointer at $91:9F2A: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose24 = (SamusPoseId)0x24;
    /// <summary>Pose $23, TransitionTable pointer at $91:9F28: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose23 = (SamusPoseId)0x23;
    /// <summary>Pose $42, TransitionTable pointer at $91:9F66: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose42 = (SamusPoseId)0x42;
    /// <summary>Pose $47, TransitionTable pointer at $91:9F70: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose47 = (SamusPoseId)0x47;
    /// <summary>Pose $48, TransitionTable pointer at $91:9F72: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose48 = (SamusPoseId)0x48;
    /// <summary>Pose $5B, TransitionTable pointer at $91:9F98: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose5B = (SamusPoseId)0x5b;
    /// <summary>Pose $5C, TransitionTable pointer at $91:9F9A: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose5C = (SamusPoseId)0x5c;
    /// <summary>Pose $63, TransitionTable pointer at $91:9FA8: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose63 = (SamusPoseId)0x63;
    /// <summary>Pose $64, TransitionTable pointer at $91:9FAA: preserved native input-dispatch identity.</summary>
    private const SamusPoseId NativePose64 = (SamusPoseId)0x64;

    /// <summary>Maps a supported pose to its bank-91 transition list, preserving shared and empty-list identities.</summary>
    /// <returns>False only when the pose lies beyond the native 253-pose dispatch table.</returns>
    internal static bool TryGetInputList(SamusPoseId pose, out SamusPoseInputList inputList)
    {
        // Indexes past the 253 native poses read adjacent bank-$91 code, not a graph.
        if ((int)pose >= SuperMetroid.Core.Samus.Artwork.SamusBodyArtworkCatalog.PoseCount)
        {
            inputList = default;
            return false;
        }
        inputList = pose switch
        {
            SamusPoseId.TurningRightToLeftJumpPose or SamusPoseId.TurningLeftToRightJumpPose or SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionLeftPose or SamusPoseId.MorphingTransitionRightPose or SamusPoseId.MorphingTransitionLeftPose or NativePose39 or NativePose3A or SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionLeftPose or NativePose3F or NativePose40 or SamusPoseId.TurningRightToLeftCrouchingPose or SamusPoseId.TurningLeftToRightCrouchingPose or NativePose5D or NativePose5E or NativePose5F or NativePose60 or NativePose61 or NativePose62 or SamusPoseId.TurningRightToLeftFallingPose or SamusPoseId.TurningLeftToRightFallingPose or SamusPoseId.TurningRightToLeftJumpAimUpPose or SamusPoseId.TurningLeftToRightJumpAimUpPose or SamusPoseId.TurningRightToLeftJumpAimDownPose or SamusPoseId.TurningLeftToRightJumpAimDownPose or SamusPoseId.TurningRightToLeftFallingAimUpPose or SamusPoseId.TurningLeftToRightFallingAimUpPose or SamusPoseId.TurningRightToLeftFallingAimDownPose or SamusPoseId.TurningLeftToRightFallingAimDownPose or SamusPoseId.TurningRightToLeftCrouchingAimUpPose or SamusPoseId.TurningLeftToRightCrouchingAimUpPose or SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or SamusPoseId.TurningRightToLeftAimDiagonalUpPose or SamusPoseId.TurningLeftToRightAimDiagonalUpPose or SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or SamusPoseId.GrappleStandingRightPose or SamusPoseId.GrappleStandingLeftPose or SamusPoseId.GrappleStandingDownRightPose or SamusPoseId.GrappleStandingDownLeftPose or NativePoseAC or NativePoseAD or NativePoseAE or NativePoseAF or NativePoseB0 or NativePoseB1 or SamusPoseId.GrappleSwingRightPose or SamusPoseId.GrappleSwingLeftPose or SamusPoseId.GrappleCrouchingRightPose or SamusPoseId.GrappleCrouchingLeftPose or SamusPoseId.GrappleCrouchingDownRightPose or SamusPoseId.GrappleCrouchingDownLeftPose or SamusPoseId.GrappleWallContactLeftPose or SamusPoseId.GrappleWallContactRightPose or NativePoseC5 or NativePoseC6 or SamusPoseId.ShinesparkHorizontalRightPose or SamusPoseId.ShinesparkHorizontalLeftPose or SamusPoseId.ShinesparkVerticalRightPose or SamusPoseId.ShinesparkVerticalLeftPose or SamusPoseId.ShinesparkDiagonalRightPose or SamusPoseId.ShinesparkDiagonalLeftPose or SamusPoseId.CrystalFlashRightPose or SamusPoseId.CrystalFlashLeftPose or SamusPoseId.XrayingStandingRightPose or SamusPoseId.XrayingStandingLeftPose or SamusPoseId.DeathSequenceRightPose or SamusPoseId.DeathSequenceLeftPose or SamusPoseId.XrayingCrouchingRightPose or SamusPoseId.XrayingCrouchingLeftPose or SamusPoseId.UnusedPoseDb or SamusPoseId.UnusedPoseDc or SamusPoseId.UnusedPoseDd or SamusPoseId.UnusedPoseDe or SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose or SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose or SamusPoseId.CrouchingTransitionAimUpRightPose or SamusPoseId.CrouchingTransitionAimUpLeftPose or SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or SamusPoseId.StandingTransitionAimUpRightPose or SamusPoseId.StandingTransitionAimUpLeftPose or SamusPoseId.StandingTransitionAimDiagonalUpRightPose or SamusPoseId.StandingTransitionAimDiagonalUpLeftPose or SamusPoseId.StandingTransitionAimDiagonalDownRightPose or SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => EmptyTransitionList,
            SamusPoseId.ForwardFacingPowerSuitPose or SamusPoseId.ForwardFacingSuitedPose => ForwardFacingPowerSuitPoseList,
            SamusPoseId.FacingRightNormalPose or SamusPoseId.StandingAimUpRightPose or SamusPoseId.StandingAimDiagonalUpRightPose or SamusPoseId.StandingAimDiagonalDownRightPose or SamusPoseId.NormalLandingRightPose or SamusPoseId.SpinLandingRightPose or SamusPoseId.LandingAimUpRightPose or SamusPoseId.LandingAimDiagonalUpRightPose or SamusPoseId.LandingAimDiagonalDownRightPose or SamusPoseId.FiringLandingRightPose => FacingRightNormalPoseList,
            SamusPoseId.FacingLeftNormalPose or SamusPoseId.StandingAimUpLeftPose or SamusPoseId.StandingAimDiagonalUpLeftPose or SamusPoseId.StandingAimDiagonalDownLeftPose or SamusPoseId.NormalLandingLeftPose or SamusPoseId.SpinLandingLeftPose or SamusPoseId.LandingAimUpLeftPose or SamusPoseId.LandingAimDiagonalUpLeftPose or SamusPoseId.LandingAimDiagonalDownLeftPose or SamusPoseId.FiringLandingLeftPose => FacingLeftNormalPoseList,
            SamusPoseId.MovingRightNormalPose or SamusPoseId.RunningAimUpRightPose or SamusPoseId.RunningAimDiagonalUpRightPose or SamusPoseId.RunningAimDiagonalDownRightPose => MovingRightNormalPoseList,
            SamusPoseId.MovingLeftNormalPose or SamusPoseId.RunningAimUpLeftPose or SamusPoseId.RunningAimDiagonalUpLeftPose or SamusPoseId.RunningAimDiagonalDownLeftPose => MovingLeftNormalPoseList,
            SamusPoseId.NeutralJumpTransitionRightPose or SamusPoseId.NormalJumpTransitionAimUpRightPose or SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose => NeutralJumpTransitionRightPoseList,
            SamusPoseId.NeutralJumpTransitionLeftPose or SamusPoseId.NormalJumpTransitionAimUpLeftPose or SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose => NeutralJumpTransitionLeftPoseList,
            SamusPoseId.NormalJumpAimUpRightPose or SamusPoseId.NeutralJumpRightPose or SamusPoseId.NormalJumpForwardRightPose or SamusPoseId.NormalJumpAimDiagonalUpRightPose or SamusPoseId.NormalJumpAimDiagonalDownRightPose => NormalJumpAimUpRightPoseList,
            SamusPoseId.NormalJumpAimUpLeftPose or SamusPoseId.NeutralJumpLeftPose or SamusPoseId.NormalJumpForwardLeftPose or SamusPoseId.NormalJumpAimDiagonalUpLeftPose or SamusPoseId.NormalJumpAimDiagonalDownLeftPose => NormalJumpAimUpLeftPoseList,
            SamusPoseId.DamageBoostLeftPose => DamageBoostLeftPoseList,
            SamusPoseId.DamageBoostRightPose => DamageBoostRightPoseList,
            SamusPoseId.SpinJumpRightPose => SpinJumpRightPoseList,
            SamusPoseId.SpinJumpLeftPose => SpinJumpLeftPoseList,
            SamusPoseId.SpaceJumpRightPose => SpaceJumpRightPoseList,
            SamusPoseId.SpaceJumpLeftPose => SpaceJumpLeftPoseList,
            SamusPoseId.ScrewAttackRightPose => ScrewAttackRightPoseList,
            SamusPoseId.ScrewAttackLeftPose => ScrewAttackLeftPoseList,
            SamusPoseId.MorphBallGroundRightPose => MorphBallGroundRightPoseList,
            SamusPoseId.MorphBallMovingRightPose => MorphBallMovingRightPoseList,
            SamusPoseId.MorphBallMovingLeftPose => MorphBallMovingLeftPoseList,
            SamusPoseId.MorphBallGroundLeftPose => MorphBallGroundLeftPoseList,
            NativePose20 or NativePose21 or NativePose22 or NativePose24 => UnusedPose20List,
            NativePose23 => UnusedPose23List,
            NativePose42 => UnusedPose42List,
            SamusPoseId.CrouchingRightPose or SamusPoseId.CrouchingAimDiagonalUpRightPose or SamusPoseId.CrouchingAimDiagonalDownRightPose or SamusPoseId.CrouchingAimUpRightPose => CrouchingRightPoseList,
            SamusPoseId.CrouchingLeftPose or SamusPoseId.CrouchingAimDiagonalUpLeftPose or SamusPoseId.CrouchingAimDiagonalDownLeftPose or SamusPoseId.CrouchingAimUpLeftPose => CrouchingLeftPoseList,
            SamusPoseId.FallingRightPose or SamusPoseId.FallingAimUpRightPose or SamusPoseId.FallingAimDiagonalUpRightPose or SamusPoseId.FallingAimDiagonalDownRightPose => FallingRightPoseList,
            SamusPoseId.FallingLeftPose or SamusPoseId.FallingAimUpLeftPose or SamusPoseId.FallingAimDiagonalUpLeftPose or SamusPoseId.FallingAimDiagonalDownLeftPose => FallingLeftPoseList,
            SamusPoseId.MorphBallFallingRightPose => MorphBallFallingRightPoseList,
            SamusPoseId.MorphBallFallingLeftPose => MorphBallFallingLeftPoseList,
            SamusPoseId.UnusedKnockbackRightPose => UnusedKnockbackRightPoseList,
            SamusPoseId.UnusedKnockbackLeftPose => UnusedKnockbackLeftPoseList,
            UnusedPose45 => UnusedPose45List,
            UnusedPose46 => UnusedPose46List,
            NativePose47 => UnusedPose47List,
            NativePose48 => UnusedPose48List,
            SamusPoseId.MoonwalkFacingLeftPose or SamusPoseId.MoonwalkAimUpLeftPose or SamusPoseId.MoonwalkAimDownLeftPose => MoonwalkFacingLeftPoseList,
            SamusPoseId.MoonwalkFacingRightPose or SamusPoseId.MoonwalkAimUpRightPose or SamusPoseId.MoonwalkAimDownRightPose => MoonwalkFacingRightPoseList,
            SamusPoseId.KnockbackRightPose => KnockbackRightPoseList,
            SamusPoseId.KnockbackLeftPose => KnockbackLeftPoseList,
            NativePose5B => UnusedPose5BList,
            NativePose5C => UnusedPose5CList,
            SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallMovingRightPose => SpringBallGroundRightPoseList,
            SamusPoseId.SpringBallGroundLeftPose or SamusPoseId.SpringBallMovingLeftPose => SpringBallGroundLeftPoseList,
            SamusPoseId.SpringBallFallingRightPose => SpringBallFallingRightPoseList,
            SamusPoseId.SpringBallFallingLeftPose => SpringBallFallingLeftPoseList,
            SamusPoseId.SpringBallJumpRightPose => SpringBallJumpRightPoseList,
            SamusPoseId.SpringBallJumpLeftPose => SpringBallJumpLeftPoseList,
            NativePose63 => UnusedPose63List,
            NativePose64 => UnusedPose64List,
            SamusPoseId.UnusedPose65 => UnusedPose65List,
            SamusPoseId.UnusedPose66 => UnusedPose66List,
            SamusPoseId.WallJumpRightPose => WallJumpRightPoseList,
            SamusPoseId.WallJumpLeftPose => WallJumpLeftPoseList,
            SamusPoseId.RanIntoWallRightPose or SamusPoseId.RanIntoWallAimUpRightPose or SamusPoseId.RanIntoWallAimDownRightPose => RanIntoWallRightPoseList,
            SamusPoseId.RanIntoWallLeftPose or SamusPoseId.RanIntoWallAimUpLeftPose or SamusPoseId.RanIntoWallAimDownLeftPose => RanIntoWallLeftPoseList,
            SamusPoseId.NormalJumpGunExtendedRightPose => NormalJumpGunExtendedRightPoseList,
            SamusPoseId.NormalJumpGunExtendedLeftPose => NormalJumpGunExtendedLeftPoseList,
            SamusPoseId.NormalJumpAimDownRightPose => NormalJumpAimDownRightPoseList,
            SamusPoseId.NormalJumpAimDownLeftPose => NormalJumpAimDownLeftPoseList,
            SamusPoseId.UnmorphingTransitionRightPose => UnmorphingTransitionRightPoseList,
            SamusPoseId.UnmorphingTransitionLeftPose => UnmorphingTransitionLeftPoseList,
            SamusPoseId.TurningRightToLeftPose => TurningRightToLeftPoseList,
            SamusPoseId.TurningLeftToRightPose => TurningLeftToRightPoseList,
            SamusPoseId.TurningRightToLeftAimUpPose => TurningRightToLeftAimUpPoseList,
            SamusPoseId.TurningLeftToRightAimUpPose => TurningLeftToRightAimUpPoseList,
            SamusPoseId.TurningRightToLeftAimDiagonalDownPose => TurningRightToLeftAimDiagonalDownPoseList,
            SamusPoseId.TurningLeftToRightAimDiagonalDownPose => TurningLeftToRightAimDiagonalDownPoseList,
            SamusPoseId.ShinesparkWindupRightPose => ShinesparkWindupRightPoseList,
            SamusPoseId.ShinesparkWindupLeftPose => ShinesparkWindupLeftPoseList,
            SamusPoseId.FallingAimDownRightPose => FallingAimDownRightPoseList,
            SamusPoseId.FallingAimDownLeftPose => FallingAimDownLeftPoseList,
            SamusPoseId.UnusedPoseDf => UnusedPoseDfList,
            SamusPoseId.DraygonGrabbedNeutralLeftPose or SamusPoseId.DraygonGrabbedAimUpLeftPose or SamusPoseId.DraygonGrabbedFiringLeftPose or SamusPoseId.DraygonGrabbedAimDownLeftPose or SamusPoseId.DraygonGrabbedMovingLeftPose => DraygonGrabbedNeutralLeftPoseList,
            SamusPoseId.DraygonGrabbedNeutralRightPose or SamusPoseId.DraygonGrabbedAimUpRightPose or SamusPoseId.DraygonGrabbedFiringRightPose or SamusPoseId.DraygonGrabbedAimDownRightPose or SamusPoseId.DraygonGrabbedMovingRightPose => DraygonGrabbedNeutralRightPoseList,
            SamusPoseId.MovingRightGunExtendedPose => MovingRightGunExtendedPoseList,
            SamusPoseId.MovingLeftGunExtendedPose => MovingLeftGunExtendedPoseList,
            SamusPoseId.FallingGunExtendedRightPose => FallingGunExtendedRightPoseList,
            SamusPoseId.FallingGunExtendedLeftPose => FallingGunExtendedLeftPoseList,
            SamusPoseId.MoonwalkTurnJumpLeftPose => MoonwalkTurnJumpLeftPoseList,
            SamusPoseId.MoonwalkTurnJumpRightPose => MoonwalkTurnJumpRightPoseList,
            SamusPoseId.MoonwalkTurnJumpAimUpLeftPose => MoonwalkTurnJumpAimUpLeftPoseList,
            SamusPoseId.MoonwalkTurnJumpAimUpRightPose => MoonwalkTurnJumpAimUpRightPoseList,
            SamusPoseId.MoonwalkTurnJumpAimDownLeftPose => MoonwalkTurnJumpAimDownLeftPoseList,
            SamusPoseId.MoonwalkTurnJumpAimDownRightPose => MoonwalkTurnJumpAimDownRightPoseList,
            SamusPoseId.AdjacentCodePoseFD or
                SamusPoseId.AdjacentCodePoseFE or
                SamusPoseId.AdjacentCodePoseFF => throw new InvalidOperationException($"Pose ${(int)pose:X2} has no compiled input graph."),
            var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
        };
        return true;
    }

    /// <summary>Evaluates the selected transition list in its early or late native graph section using held and edge inputs.</summary>
    internal static SamusPoseInputMatch Match(SamusPoseInputList inputList, SnesButton held, SnesButton newlyPressed) =>
        LaterLists.Contains(inputList)
            ? SamusPoseInputRulesLate.Match(inputList, held, newlyPressed)
            : SamusPoseInputRulesEarly.Match(inputList, held, newlyPressed);

    /// <summary>
    /// Exchanges Left and Right in <paramref name="input"/>. A left-facing list is its right-facing list read with the
    /// directions exchanged, its selected pose mirrored back.
    /// </summary>
    internal static SnesButton MirrorButtons(SnesButton input) =>
        (input & ~(SnesButton.Left | SnesButton.Right)) |
        ((input & SnesButton.Left) != 0 ? SnesButton.Right : 0) |
        ((input & SnesButton.Right) != 0 ? SnesButton.Left : 0);

    /// <summary>Requires every bit of an authored canonical input chord to be present.</summary>
    internal static bool Has(SnesButton input, CanonicalPoseButtons required) =>
        (input & Buttons(required)) == Buttons(required);

    /// <summary>
    /// The buttons an authored chord names: canonical pose bits use the SNES joypad layout, with the
    /// default action buttons standing for their bindings.
    /// </summary>
    [ControllerWordBoundary("authored pose chords are joypad-layout words until #1301 types them")]
    private static SnesButton Buttons(CanonicalPoseButtons chord) => (SnesButton)(ushort)chord;

    /// <summary>Marks a selected target as a matched condition, distinct from an empty transition list.</summary>
    internal static SamusPoseInputMatch Accept(SamusPoseId target) =>
        new(new(target), HasConditions: true);
}
