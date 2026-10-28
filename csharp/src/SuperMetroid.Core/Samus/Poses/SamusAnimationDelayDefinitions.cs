
namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Compiled bank-$91 Samus pose animation timing and command streams.</summary>
/// <remarks>
/// The 253 retail pose pointers at $91:B010-$B209 select delay lists beginning at
/// $91:B20A. Three out-of-range pose bytes naturally overread the first six delay
/// bytes as pointers; those aliases calculate from the same delay bytes. The delay region
/// ends before the separately compiled running-cadence pointer at $91:B5D1.
/// These are gameplay cadence/command definitions, not editable sprite artwork; their
/// semantic segments live in <see cref="SamusAnimationDelayPrograms"/>.
/// </remarks>
internal static class SamusAnimationDelayDefinitions
{
    /// <summary>The bytes the compiled pose delay programs fill, before the running cadences.</summary>
    internal const int ProgramsByteCount = 967;

    /// <summary>
    /// The bank-$91 low-half offset the floor update residue starts at (#1338): the low half aliases WRAM, where the
    /// residue's descriptors begin at <c>$02D0</c>.
    /// </summary>
    internal const int FloorUpdateResidueOffset = 0x02d0;

    /// <summary>$91:B56F, AnimationDelays_0_9B: animation selected by ForwardFacingPowerSuitPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList ForwardFacingPowerSuitAnimation = new(158, 0);

    /// <summary>$91:B298, AnimationDelays_01_02: animation selected by FacingRightNormalPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList FacingRightNormalAnimation = new(30, 0);

    /// <summary>$91:B222, AnimationDelays_03_04_85_86: animation selected by StandingAimUpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList StandingAimUpRightAnimation = new(3, 0);

    /// <summary>$91:B2B4, AnimationDelays_Various_91B2B4: animation selected by StandingAimDiagonalUpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList StandingAimDiagonalUpRightAnimation = new(35, 0);

    /// <summary>$91:B20A, AnimationDelays_09_0A_0B_0C_0D_0E_0F_10_11_12_45_46: animation selected by MovingRightNormalPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList MovingRightNormalAnimation = new(0, 0);

    /// <summary>$91:B346, AnimationDelays_13_14_69_6A_6B_6C: animation selected by NormalJumpGunExtendedRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList NormalJumpGunExtendedRightAnimation = new(54, 0);

    /// <summary>$91:B33A, AnimationDelays_15_16: animation selected by NormalJumpAimUpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList NormalJumpAimUpRightAnimation = new(51, 0);

    /// <summary>$91:B33E, AnimationDelays_17_18: animation selected by NormalJumpAimDownRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList NormalJumpAimDownRightAnimation = new(52, 0);

    /// <summary>$91:B384, AnimationDelays_19_1A: animation selected by SpinJumpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList SpinJumpRightAnimation = new(69, 0);

    /// <summary>$91:B391, AnimationDelays_1B_1C: animation selected by SpaceJumpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList SpaceJumpRightAnimation = new(71, 0);

    /// <summary>$91:B378, AnimationDelays_Various_91B378: animation selected by MorphBallGroundRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList MorphBallGroundRightAnimation = new(67, 0);

    /// <summary>$91:B3BB, AnimationDelays_25: animation selected by TurningRightToLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftAnimation = new(75, 0);

    /// <summary>$91:B3C0, AnimationDelays_26: animation selected by TurningLeftToRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightAnimation = new(76, 0);

    /// <summary>$91:B2A3, AnimationDelays_27_28: animation selected by CrouchingRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList CrouchingRightAnimation = new(32, 0);

    /// <summary>$91:B34A, AnimationDelays_29_2A: animation selected by FallingRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList FallingRightAnimation = new(55, 0);

    /// <summary>$91:B35C, AnimationDelays_2B_2C: animation selected by FallingAimUpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList FallingAimUpRightAnimation = new(59, 0);

    /// <summary>$91:B366, AnimationDelays_2D_2E: animation selected by FallingAimDownRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList FallingAimDownRightAnimation = new(62, 0);

    /// <summary>$91:B3C5, AnimationDelays_2F: animation selected by TurningRightToLeftJumpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftJumpAnimation = new(77, 0);

    /// <summary>$91:B3CA, AnimationDelays_30: animation selected by TurningLeftToRightJumpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightJumpAnimation = new(78, 0);

    /// <summary>$91:B4C2, AnimationDelays_35: animation selected by CrouchingTransitionRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList CrouchingTransitionRightAnimation = new(121, 0);

    /// <summary>$91:B4C5, AnimationDelays_36: animation selected by CrouchingTransitionLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList CrouchingTransitionLeftAnimation = new(122, 0);

    /// <summary>$91:B4C8, AnimationDelays_37: animation selected by MorphingTransitionRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList MorphingTransitionRightAnimation = new(123, 0);

    /// <summary>$91:B4D1, AnimationDelays_38: animation selected by MorphingTransitionLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList MorphingTransitionLeftAnimation = new(124, 0);

    /// <summary>$91:B4DA, UNUSED_AnimationDelays_39_91B4DA: animation selected by UnusedPose39 and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnusedPose39Animation = new(125, 0);

    /// <summary>$91:B4DD, UNUSED_AnimationDelays_3A_91B4DD: animation selected by UnusedPose3A and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnusedPose3AAnimation = new(126, 0);

    /// <summary>$91:B4E0, AnimationDelays_3B: animation selected by StandingTransitionRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList StandingTransitionRightAnimation = new(127, 0);

    /// <summary>$91:B4E3, AnimationDelays_3C: animation selected by StandingTransitionLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList StandingTransitionLeftAnimation = new(128, 0);

    /// <summary>$91:B4E6, AnimationDelays_3D: animation selected by UnmorphingTransitionRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnmorphingTransitionRightAnimation = new(129, 0);

    /// <summary>$91:B4EA, AnimationDelays_3E: animation selected by UnmorphingTransitionLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnmorphingTransitionLeftAnimation = new(130, 0);

    /// <summary>$91:B4EE, UNUSED_AnimationDelays_3F_91B4EE: animation selected by UnusedPose3F and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnusedPose3FAnimation = new(131, 0);

    /// <summary>$91:B4F4, UNUSED_AnimationDelays_40_91B4F4: animation selected by UnusedPose40 and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnusedPose40Animation = new(132, 0);

    /// <summary>$91:B3CF, AnimationDelays_43: animation selected by TurningRightToLeftCrouchingPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftCrouchingAnimation = new(79, 0);

    /// <summary>$91:B3D4, AnimationDelays_44: animation selected by TurningLeftToRightCrouchingPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightCrouchingAnimation = new(80, 0);

    /// <summary>$91:B226, AnimationDelays_49_4A_75_76_77_78: animation selected by MoonwalkFacingLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList MoonwalkFacingLeftAnimation = new(4, 0);

    /// <summary>$91:B308, AnimationDelays_4B: animation selected by NeutralJumpTransitionRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList NeutralJumpTransitionRightAnimation = new(41, 0);

    /// <summary>$91:B30B, AnimationDelays_4C: animation selected by NeutralJumpTransitionLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList NeutralJumpTransitionLeftAnimation = new(42, 0);

    /// <summary>$91:B326, AnimationDelays_4D_4E_C7_C8: animation selected by NeutralJumpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList NeutralJumpRightAnimation = new(49, 0);

    /// <summary>$91:B32E, AnimationDelays_4F_50: animation selected by DamageBoostLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList DamageBoostLeftAnimation = new(50, 0);

    /// <summary>$91:B342, AnimationDelays_51_52: animation selected by NormalJumpForwardRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList NormalJumpForwardRightAnimation = new(53, 0);

    /// <summary>$91:B36A, AnimationDelays_53_54: animation selected by KnockbackRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList KnockbackRightAnimation = new(63, 0);

    /// <summary>$91:B30E, AnimationDelays_55: animation selected by NormalJumpTransitionAimUpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList NormalJumpTransitionAimUpRightAnimation = new(43, 0);

    /// <summary>$91:B312, AnimationDelays_56: animation selected by NormalJumpTransitionAimUpLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList NormalJumpTransitionAimUpLeftAnimation = new(44, 0);

    /// <summary>$91:B316, AnimationDelays_57: animation selected by NormalJumpTransitionAimDiagonalUpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList NormalJumpTransitionAimDiagonalUpRightAnimation = new(45, 0);

    /// <summary>$91:B31A, AnimationDelays_58: animation selected by NormalJumpTransitionAimDiagonalUpLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList NormalJumpTransitionAimDiagonalUpLeftAnimation = new(46, 0);

    /// <summary>$91:B31E, AnimationDelays_59: animation selected by NormalJumpTransitionAimDiagonalDownRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList NormalJumpTransitionAimDiagonalDownRightAnimation = new(47, 0);

    /// <summary>$91:B322, AnimationDelays_5A: animation selected by NormalJumpTransitionAimDiagonalDownLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList NormalJumpTransitionAimDiagonalDownLeftAnimation = new(48, 0);

    /// <summary>$91:B47E, UNUSED_AnimationDelays_63_91B47E: animation selected by UnusedPose63 and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnusedPose63Animation = new(114, 0);

    /// <summary>$91:B482, UNUSED_AnimationDelays_64_91B482: animation selected by UnusedPose64 and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnusedPose64Animation = new(115, 0);

    /// <summary>$91:B486, UNUSED_AnimationDelays_65_66_91B486: animation selected by UnusedPose65 and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnusedPose65Animation = new(116, 0);

    /// <summary>$91:B353, AnimationDelays_67_68: animation selected by FallingGunExtendedRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList FallingGunExtendedRightAnimation = new(57, 0);

    /// <summary>$91:B361, AnimationDelays_6D_6E_6F_70: animation selected by FallingAimDiagonalUpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList FallingAimDiagonalUpRightAnimation = new(60, 0);

    /// <summary>$91:B39E, AnimationDelays_81_82: animation selected by ScrewAttackRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList ScrewAttackRightAnimation = new(73, 0);

    /// <summary>$91:B491, AnimationDelays_83_84: animation selected by WallJumpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList WallJumpRightAnimation = new(117, 0);

    /// <summary>$91:B3D9, AnimationDelays_87: animation selected by TurningRightToLeftFallingPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftFallingAnimation = new(81, 0);

    /// <summary>$91:B3DE, AnimationDelays_88: animation selected by TurningLeftToRightFallingPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightFallingAnimation = new(82, 0);

    /// <summary>$91:B3E3, AnimationDelays_8B: animation selected by TurningRightToLeftAimUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftAimUpAnimation = new(83, 0);

    /// <summary>$91:B3E8, AnimationDelays_8C: animation selected by TurningLeftToRightAimUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightAimUpAnimation = new(84, 0);

    /// <summary>$91:B3ED, AnimationDelays_8D: animation selected by TurningRightToLeftAimDiagonalDownPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftAimDiagonalDownAnimation = new(85, 0);

    /// <summary>$91:B3F2, AnimationDelays_8E: animation selected by TurningLeftToRightAimDiagonalDownPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightAimDiagonalDownAnimation = new(86, 0);

    /// <summary>$91:B3F7, AnimationDelays_8F: animation selected by TurningRightToLeftJumpAimUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftJumpAimUpAnimation = new(87, 0);

    /// <summary>$91:B3FC, AnimationDelays_90: animation selected by TurningLeftToRightJumpAimUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightJumpAimUpAnimation = new(88, 0);

    /// <summary>$91:B401, AnimationDelays_91: animation selected by TurningRightToLeftJumpAimDownPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftJumpAimDownAnimation = new(89, 0);

    /// <summary>$91:B406, AnimationDelays_92: animation selected by TurningLeftToRightJumpAimDownPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightJumpAimDownAnimation = new(90, 0);

    /// <summary>$91:B40B, AnimationDelays_93: animation selected by TurningRightToLeftFallingAimUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftFallingAimUpAnimation = new(91, 0);

    /// <summary>$91:B410, AnimationDelays_94: animation selected by TurningLeftToRightFallingAimUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightFallingAimUpAnimation = new(92, 0);

    /// <summary>$91:B415, AnimationDelays_95: animation selected by TurningRightToLeftFallingAimDownPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftFallingAimDownAnimation = new(93, 0);

    /// <summary>$91:B41A, AnimationDelays_96: animation selected by TurningLeftToRightFallingAimDownPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightFallingAimDownAnimation = new(94, 0);

    /// <summary>$91:B41F, AnimationDelays_97: animation selected by TurningRightToLeftCrouchingAimUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftCrouchingAimUpAnimation = new(95, 0);

    /// <summary>$91:B424, AnimationDelays_98: animation selected by TurningLeftToRightCrouchingAimUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightCrouchingAimUpAnimation = new(96, 0);

    /// <summary>$91:B429, AnimationDelays_99: animation selected by TurningRightToLeftCrouchingAimDiagonalDownPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftCrouchingAimDiagonalDownAnimation = new(97, 0);

    /// <summary>$91:B42E, AnimationDelays_9A: animation selected by TurningLeftToRightCrouchingAimDiagonalDownPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightCrouchingAimDiagonalDownAnimation = new(98, 0);

    /// <summary>$91:B433, AnimationDelays_9C: animation selected by TurningRightToLeftAimDiagonalUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftAimDiagonalUpAnimation = new(99, 0);

    /// <summary>$91:B438, AnimationDelays_9D: animation selected by TurningLeftToRightAimDiagonalUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightAimDiagonalUpAnimation = new(100, 0);

    /// <summary>$91:B43D, AnimationDelays_9E: animation selected by TurningRightToLeftJumpAimDiagonalUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftJumpAimDiagonalUpAnimation = new(101, 0);

    /// <summary>$91:B442, AnimationDelays_9F: animation selected by TurningLeftToRightJumpAimDiagonalUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightJumpAimDiagonalUpAnimation = new(102, 0);

    /// <summary>$91:B447, AnimationDelays_A0: animation selected by TurningRightToLeftFallingAimDiagonalUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftFallingAimDiagonalUpAnimation = new(103, 0);

    /// <summary>$91:B44C, AnimationDelays_A1: animation selected by TurningLeftToRightFallingAimDiagonalUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightFallingAimDiagonalUpAnimation = new(104, 0);

    /// <summary>$91:B451, AnimationDelays_A2: animation selected by TurningRightToLeftCrouchingAimDiagonalUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningRightToLeftCrouchingAimDiagonalUpAnimation = new(105, 0);

    /// <summary>$91:B456, AnimationDelays_A3: animation selected by TurningLeftToRightCrouchingAimDiagonalUpPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList TurningLeftToRightCrouchingAimDiagonalUpAnimation = new(106, 0);

    /// <summary>$91:B22D, AnimationDelays_A4_E6: animation selected by NormalLandingRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList NormalLandingRightAnimation = new(5, 0);

    /// <summary>$91:B231, AnimationDelays_A5_E7: animation selected by NormalLandingLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList NormalLandingLeftAnimation = new(6, 0);

    /// <summary>$91:B235, AnimationDelays_A6: animation selected by SpinLandingRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList SpinLandingRightAnimation = new(7, 0);

    /// <summary>$91:B23A, AnimationDelays_A7: animation selected by SpinLandingLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList SpinLandingLeftAnimation = new(8, 0);

    /// <summary>$91:B2B6, AnimationDelays_A8_A9_AA_AB: animation selected by GrappleStandingRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList GrappleStandingRightAnimation = new(36, 0);

    /// <summary>$91:B2B8, AnimationDelays_AC_AD: animation selected by UnusedPoseAC and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnusedPoseACAnimation = new(37, 0);

    /// <summary>$91:B2BC, AnimationDelays_AE_AF: animation selected by UnusedPoseAE and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnusedPoseAEAnimation = new(38, 0);

    /// <summary>$91:B2C0, AnimationDelays_B0_B1: animation selected by UnusedPoseB0 and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnusedPoseB0Animation = new(39, 0);

    /// <summary>$91:B2C4, AnimationDelays_B2_B3: animation selected by GrappleSwingRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList GrappleSwingRightAnimation = new(40, 0);

    /// <summary>$91:B53C, AnimationDelays_BE_F0: animation selected by DraygonGrabbedMovingLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList DraygonGrabbedMovingLeftAnimation = new(149, 0);

    /// <summary>$91:B45B, AnimationDelays_BF: animation selected by MoonwalkTurnJumpLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList MoonwalkTurnJumpLeftAnimation = new(107, 0);

    /// <summary>$91:B460, AnimationDelays_C0: animation selected by MoonwalkTurnJumpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList MoonwalkTurnJumpRightAnimation = new(108, 0);

    /// <summary>$91:B465, AnimationDelays_C1: animation selected by MoonwalkTurnJumpAimUpLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList MoonwalkTurnJumpAimUpLeftAnimation = new(109, 0);

    /// <summary>$91:B46A, AnimationDelays_C2: animation selected by MoonwalkTurnJumpAimUpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList MoonwalkTurnJumpAimUpRightAnimation = new(110, 0);

    /// <summary>$91:B46F, AnimationDelays_C3: animation selected by MoonwalkTurnJumpAimDownLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList MoonwalkTurnJumpAimDownLeftAnimation = new(111, 0);

    /// <summary>$91:B474, AnimationDelays_C4: animation selected by MoonwalkTurnJumpAimDownRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList MoonwalkTurnJumpAimDownRightAnimation = new(112, 0);

    /// <summary>$91:B479, UNUSED_AnimationDelays_C6_91B479: animation selected by UnusedPoseC6 and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnusedPoseC6Animation = new(113, 0);

    /// <summary>$91:B543, AnimationDelays_C9_CA_CB_CC_CD_CE: animation selected by ShinesparkHorizontalRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList ShinesparkHorizontalRightAnimation = new(150, 0);

    /// <summary>$91:B545, AnimationDelays_D3: animation selected by CrystalFlashRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList CrystalFlashRightAnimation = new(151, 0);

    /// <summary>$91:B556, AnimationDelays_D4: animation selected by CrystalFlashLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList CrystalFlashLeftAnimation = new(154, 0);

    /// <summary>$91:B2AE, AnimationDelays_D5_D6_D9_DA: animation selected by XrayingStandingRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList XrayingStandingRightAnimation = new(34, 0);

    /// <summary>$91:B567, AnimationDelays_D7_D8: animation selected by DeathSequenceRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList DeathSequenceRightAnimation = new(157, 0);

    /// <summary>$91:B4FA, UNUSED_AnimationDelays_DB_91B4FA: animation selected by UnusedPoseDb and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnusedPoseDbAnimation = new(133, 0);

    /// <summary>$91:B504, UNUSED_AnimationDelays_DC_91B504: animation selected by UnusedPoseDc and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnusedPoseDcAnimation = new(134, 0);

    /// <summary>$91:B50E, UNUSED_AnimationDelays_DD_91B50E: animation selected by UnusedPoseDd and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnusedPoseDdAnimation = new(135, 0);

    /// <summary>$91:B513, UNUSED_AnimationDelays_DE_91B513: animation selected by UnusedPoseDe and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList UnusedPoseDeAnimation = new(136, 0);

    /// <summary>$91:B23F, AnimationDelays_E0: animation selected by LandingAimUpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList LandingAimUpRightAnimation = new(9, 0);

    /// <summary>$91:B243, AnimationDelays_E1: animation selected by LandingAimUpLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList LandingAimUpLeftAnimation = new(10, 0);

    /// <summary>$91:B247, AnimationDelays_E2: animation selected by LandingAimDiagonalUpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList LandingAimDiagonalUpRightAnimation = new(11, 0);

    /// <summary>$91:B24B, AnimationDelays_E3: animation selected by LandingAimDiagonalUpLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList LandingAimDiagonalUpLeftAnimation = new(12, 0);

    /// <summary>$91:B24F, AnimationDelays_E4: animation selected by LandingAimDiagonalDownRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList LandingAimDiagonalDownRightAnimation = new(13, 0);

    /// <summary>$91:B253, AnimationDelays_E5: animation selected by LandingAimDiagonalDownLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList LandingAimDiagonalDownLeftAnimation = new(14, 0);

    /// <summary>$91:B257, AnimationDelays_E8: animation selected by DrainedCrouchingRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList DrainedCrouchingRightAnimation = new(15, 0);

    /// <summary>$91:B268, AnimationDelays_E9: animation selected by DrainedCrouchingLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList DrainedCrouchingLeftAnimation = new(19, 0);

    /// <summary>$91:B288, AnimationDelays_EA: animation selected by DrainedStandingRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList DrainedStandingRightAnimation = new(26, 0);

    /// <summary>$91:B290, AnimationDelays_EB: animation selected by DrainedStandingLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList DrainedStandingLeftAnimation = new(28, 0);

    /// <summary>$91:B518, AnimationDelays_F1: animation selected by CrouchingTransitionAimUpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList CrouchingTransitionAimUpRightAnimation = new(137, 0);

    /// <summary>$91:B51B, AnimationDelays_F2: animation selected by CrouchingTransitionAimUpLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList CrouchingTransitionAimUpLeftAnimation = new(138, 0);

    /// <summary>$91:B51E, AnimationDelays_F3: animation selected by CrouchingTransitionAimDiagonalUpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList CrouchingTransitionAimDiagonalUpRightAnimation = new(139, 0);

    /// <summary>$91:B521, AnimationDelays_F4: animation selected by CrouchingTransitionAimDiagonalUpLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList CrouchingTransitionAimDiagonalUpLeftAnimation = new(140, 0);

    /// <summary>$91:B524, AnimationDelays_F5: animation selected by CrouchingTransitionAimDiagonalDownRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList CrouchingTransitionAimDiagonalDownRightAnimation = new(141, 0);

    /// <summary>$91:B527, AnimationDelays_F6: animation selected by CrouchingTransitionAimDiagonalDownLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList CrouchingTransitionAimDiagonalDownLeftAnimation = new(142, 0);

    /// <summary>$91:B52A, AnimationDelays_F7: animation selected by StandingTransitionAimUpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList StandingTransitionAimUpRightAnimation = new(143, 0);

    /// <summary>$91:B52D, AnimationDelays_F8: animation selected by StandingTransitionAimUpLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList StandingTransitionAimUpLeftAnimation = new(144, 0);

    /// <summary>$91:B530, AnimationDelays_F9: animation selected by StandingTransitionAimDiagonalUpRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList StandingTransitionAimDiagonalUpRightAnimation = new(145, 0);

    /// <summary>$91:B533, AnimationDelays_FA: animation selected by StandingTransitionAimDiagonalUpLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList StandingTransitionAimDiagonalUpLeftAnimation = new(146, 0);

    /// <summary>$91:B536, AnimationDelays_FB: animation selected by StandingTransitionAimDiagonalDownRightPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList StandingTransitionAimDiagonalDownRightAnimation = new(147, 0);

    /// <summary>$91:B539, AnimationDelays_FC: animation selected by StandingTransitionAimDiagonalDownLeftPose and its explicit pose aliases below.</summary>
    internal static readonly SamusAnimationDelayList StandingTransitionAimDiagonalDownLeftAnimation = new(148, 0);
}
