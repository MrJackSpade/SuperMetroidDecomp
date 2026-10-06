using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

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
    internal const int PointerTableAddress = 0x91B010;
    internal const int DelayStreamsAddress = 0x91B20A;
    internal const int DelayStreamsEndExclusive = 0x91B5D1;

    /// <summary>$91:B010-$B209, AnimationDelayTable: the real pose selector domain before running delay bytes.</summary>
    private const int RealPoseCount = (DelayStreamsAddress - PointerTableAddress) / sizeof(ushort);

    /// <summary>$91:B56F, AnimationDelays_0_9B: animation selected by ForwardFacingPowerSuitPose and its explicit pose aliases below.</summary>
    private const ushort ForwardFacingPowerSuitAnimation = 0xB56F;

    /// <summary>$91:B298, AnimationDelays_01_02: animation selected by FacingRightNormalPose and its explicit pose aliases below.</summary>
    private const ushort FacingRightNormalAnimation = 0xB298;

    /// <summary>$91:B222, AnimationDelays_03_04_85_86: animation selected by StandingAimUpRightPose and its explicit pose aliases below.</summary>
    private const ushort StandingAimUpRightAnimation = 0xB222;

    /// <summary>$91:B2B4, AnimationDelays_Various_91B2B4: animation selected by StandingAimDiagonalUpRightPose and its explicit pose aliases below.</summary>
    private const ushort StandingAimDiagonalUpRightAnimation = 0xB2B4;

    /// <summary>$91:B20A, AnimationDelays_09_0A_0B_0C_0D_0E_0F_10_11_12_45_46: animation selected by MovingRightNormalPose and its explicit pose aliases below.</summary>
    private const ushort MovingRightNormalAnimation = 0xB20A;

    /// <summary>$91:B346, AnimationDelays_13_14_69_6A_6B_6C: animation selected by NormalJumpGunExtendedRightPose and its explicit pose aliases below.</summary>
    private const ushort NormalJumpGunExtendedRightAnimation = 0xB346;

    /// <summary>$91:B33A, AnimationDelays_15_16: animation selected by NormalJumpAimUpRightPose and its explicit pose aliases below.</summary>
    private const ushort NormalJumpAimUpRightAnimation = 0xB33A;

    /// <summary>$91:B33E, AnimationDelays_17_18: animation selected by NormalJumpAimDownRightPose and its explicit pose aliases below.</summary>
    private const ushort NormalJumpAimDownRightAnimation = 0xB33E;

    /// <summary>$91:B384, AnimationDelays_19_1A: animation selected by SpinJumpRightPose and its explicit pose aliases below.</summary>
    private const ushort SpinJumpRightAnimation = 0xB384;

    /// <summary>$91:B391, AnimationDelays_1B_1C: animation selected by SpaceJumpRightPose and its explicit pose aliases below.</summary>
    private const ushort SpaceJumpRightAnimation = 0xB391;

    /// <summary>$91:B378, AnimationDelays_Various_91B378: animation selected by MorphBallGroundRightPose and its explicit pose aliases below.</summary>
    private const ushort MorphBallGroundRightAnimation = 0xB378;

    /// <summary>$91:B3BB, AnimationDelays_25: animation selected by TurningRightToLeftPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftAnimation = 0xB3BB;

    /// <summary>$91:B3C0, AnimationDelays_26: animation selected by TurningLeftToRightPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightAnimation = 0xB3C0;

    /// <summary>$91:B2A3, AnimationDelays_27_28: animation selected by CrouchingRightPose and its explicit pose aliases below.</summary>
    private const ushort CrouchingRightAnimation = 0xB2A3;

    /// <summary>$91:B34A, AnimationDelays_29_2A: animation selected by FallingRightPose and its explicit pose aliases below.</summary>
    private const ushort FallingRightAnimation = 0xB34A;

    /// <summary>$91:B35C, AnimationDelays_2B_2C: animation selected by FallingAimUpRightPose and its explicit pose aliases below.</summary>
    private const ushort FallingAimUpRightAnimation = 0xB35C;

    /// <summary>$91:B366, AnimationDelays_2D_2E: animation selected by FallingAimDownRightPose and its explicit pose aliases below.</summary>
    private const ushort FallingAimDownRightAnimation = 0xB366;

    /// <summary>$91:B3C5, AnimationDelays_2F: animation selected by TurningRightToLeftJumpPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftJumpAnimation = 0xB3C5;

    /// <summary>$91:B3CA, AnimationDelays_30: animation selected by TurningLeftToRightJumpPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightJumpAnimation = 0xB3CA;

    /// <summary>$91:B4C2, AnimationDelays_35: animation selected by CrouchingTransitionRightPose and its explicit pose aliases below.</summary>
    private const ushort CrouchingTransitionRightAnimation = 0xB4C2;

    /// <summary>$91:B4C5, AnimationDelays_36: animation selected by CrouchingTransitionLeftPose and its explicit pose aliases below.</summary>
    private const ushort CrouchingTransitionLeftAnimation = 0xB4C5;

    /// <summary>$91:B4C8, AnimationDelays_37: animation selected by MorphingTransitionRightPose and its explicit pose aliases below.</summary>
    private const ushort MorphingTransitionRightAnimation = 0xB4C8;

    /// <summary>$91:B4D1, AnimationDelays_38: animation selected by MorphingTransitionLeftPose and its explicit pose aliases below.</summary>
    private const ushort MorphingTransitionLeftAnimation = 0xB4D1;

    /// <summary>$91:B4DA, UNUSED_AnimationDelays_39_91B4DA: animation selected by UnusedPose39 and its explicit pose aliases below.</summary>
    private const ushort UnusedPose39Animation = 0xB4DA;

    /// <summary>$91:B4DD, UNUSED_AnimationDelays_3A_91B4DD: animation selected by UnusedPose3A and its explicit pose aliases below.</summary>
    private const ushort UnusedPose3AAnimation = 0xB4DD;

    /// <summary>$91:B4E0, AnimationDelays_3B: animation selected by StandingTransitionRightPose and its explicit pose aliases below.</summary>
    private const ushort StandingTransitionRightAnimation = 0xB4E0;

    /// <summary>$91:B4E3, AnimationDelays_3C: animation selected by StandingTransitionLeftPose and its explicit pose aliases below.</summary>
    private const ushort StandingTransitionLeftAnimation = 0xB4E3;

    /// <summary>$91:B4E6, AnimationDelays_3D: animation selected by UnmorphingTransitionRightPose and its explicit pose aliases below.</summary>
    private const ushort UnmorphingTransitionRightAnimation = 0xB4E6;

    /// <summary>$91:B4EA, AnimationDelays_3E: animation selected by UnmorphingTransitionLeftPose and its explicit pose aliases below.</summary>
    private const ushort UnmorphingTransitionLeftAnimation = 0xB4EA;

    /// <summary>$91:B4EE, UNUSED_AnimationDelays_3F_91B4EE: animation selected by UnusedPose3F and its explicit pose aliases below.</summary>
    private const ushort UnusedPose3FAnimation = 0xB4EE;

    /// <summary>$91:B4F4, UNUSED_AnimationDelays_40_91B4F4: animation selected by UnusedPose40 and its explicit pose aliases below.</summary>
    private const ushort UnusedPose40Animation = 0xB4F4;

    /// <summary>$91:B3CF, AnimationDelays_43: animation selected by TurningRightToLeftCrouchingPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftCrouchingAnimation = 0xB3CF;

    /// <summary>$91:B3D4, AnimationDelays_44: animation selected by TurningLeftToRightCrouchingPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightCrouchingAnimation = 0xB3D4;

    /// <summary>$91:B226, AnimationDelays_49_4A_75_76_77_78: animation selected by MoonwalkFacingLeftPose and its explicit pose aliases below.</summary>
    private const ushort MoonwalkFacingLeftAnimation = 0xB226;

    /// <summary>$91:B308, AnimationDelays_4B: animation selected by NeutralJumpTransitionRightPose and its explicit pose aliases below.</summary>
    private const ushort NeutralJumpTransitionRightAnimation = 0xB308;

    /// <summary>$91:B30B, AnimationDelays_4C: animation selected by NeutralJumpTransitionLeftPose and its explicit pose aliases below.</summary>
    private const ushort NeutralJumpTransitionLeftAnimation = 0xB30B;

    /// <summary>$91:B326, AnimationDelays_4D_4E_C7_C8: animation selected by NeutralJumpRightPose and its explicit pose aliases below.</summary>
    private const ushort NeutralJumpRightAnimation = 0xB326;

    /// <summary>$91:B32E, AnimationDelays_4F_50: animation selected by DamageBoostLeftPose and its explicit pose aliases below.</summary>
    private const ushort DamageBoostLeftAnimation = 0xB32E;

    /// <summary>$91:B342, AnimationDelays_51_52: animation selected by NormalJumpForwardRightPose and its explicit pose aliases below.</summary>
    private const ushort NormalJumpForwardRightAnimation = 0xB342;

    /// <summary>$91:B36A, AnimationDelays_53_54: animation selected by KnockbackRightPose and its explicit pose aliases below.</summary>
    private const ushort KnockbackRightAnimation = 0xB36A;

    /// <summary>$91:B30E, AnimationDelays_55: animation selected by NormalJumpTransitionAimUpRightPose and its explicit pose aliases below.</summary>
    private const ushort NormalJumpTransitionAimUpRightAnimation = 0xB30E;

    /// <summary>$91:B312, AnimationDelays_56: animation selected by NormalJumpTransitionAimUpLeftPose and its explicit pose aliases below.</summary>
    private const ushort NormalJumpTransitionAimUpLeftAnimation = 0xB312;

    /// <summary>$91:B316, AnimationDelays_57: animation selected by NormalJumpTransitionAimDiagonalUpRightPose and its explicit pose aliases below.</summary>
    private const ushort NormalJumpTransitionAimDiagonalUpRightAnimation = 0xB316;

    /// <summary>$91:B31A, AnimationDelays_58: animation selected by NormalJumpTransitionAimDiagonalUpLeftPose and its explicit pose aliases below.</summary>
    private const ushort NormalJumpTransitionAimDiagonalUpLeftAnimation = 0xB31A;

    /// <summary>$91:B31E, AnimationDelays_59: animation selected by NormalJumpTransitionAimDiagonalDownRightPose and its explicit pose aliases below.</summary>
    private const ushort NormalJumpTransitionAimDiagonalDownRightAnimation = 0xB31E;

    /// <summary>$91:B322, AnimationDelays_5A: animation selected by NormalJumpTransitionAimDiagonalDownLeftPose and its explicit pose aliases below.</summary>
    private const ushort NormalJumpTransitionAimDiagonalDownLeftAnimation = 0xB322;

    /// <summary>$91:B47E, UNUSED_AnimationDelays_63_91B47E: animation selected by UnusedPose63 and its explicit pose aliases below.</summary>
    private const ushort UnusedPose63Animation = 0xB47E;

    /// <summary>$91:B482, UNUSED_AnimationDelays_64_91B482: animation selected by UnusedPose64 and its explicit pose aliases below.</summary>
    private const ushort UnusedPose64Animation = 0xB482;

    /// <summary>$91:B486, UNUSED_AnimationDelays_65_66_91B486: animation selected by UnusedPose65 and its explicit pose aliases below.</summary>
    private const ushort UnusedPose65Animation = 0xB486;

    /// <summary>$91:B353, AnimationDelays_67_68: animation selected by FallingGunExtendedRightPose and its explicit pose aliases below.</summary>
    private const ushort FallingGunExtendedRightAnimation = 0xB353;

    /// <summary>$91:B361, AnimationDelays_6D_6E_6F_70: animation selected by FallingAimDiagonalUpRightPose and its explicit pose aliases below.</summary>
    private const ushort FallingAimDiagonalUpRightAnimation = 0xB361;

    /// <summary>$91:B39E, AnimationDelays_81_82: animation selected by ScrewAttackRightPose and its explicit pose aliases below.</summary>
    private const ushort ScrewAttackRightAnimation = 0xB39E;

    /// <summary>$91:B491, AnimationDelays_83_84: animation selected by WallJumpRightPose and its explicit pose aliases below.</summary>
    private const ushort WallJumpRightAnimation = 0xB491;

    /// <summary>$91:B3D9, AnimationDelays_87: animation selected by TurningRightToLeftFallingPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftFallingAnimation = 0xB3D9;

    /// <summary>$91:B3DE, AnimationDelays_88: animation selected by TurningLeftToRightFallingPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightFallingAnimation = 0xB3DE;

    /// <summary>$91:B3E3, AnimationDelays_8B: animation selected by TurningRightToLeftAimUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftAimUpAnimation = 0xB3E3;

    /// <summary>$91:B3E8, AnimationDelays_8C: animation selected by TurningLeftToRightAimUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightAimUpAnimation = 0xB3E8;

    /// <summary>$91:B3ED, AnimationDelays_8D: animation selected by TurningRightToLeftAimDiagonalDownPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftAimDiagonalDownAnimation = 0xB3ED;

    /// <summary>$91:B3F2, AnimationDelays_8E: animation selected by TurningLeftToRightAimDiagonalDownPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightAimDiagonalDownAnimation = 0xB3F2;

    /// <summary>$91:B3F7, AnimationDelays_8F: animation selected by TurningRightToLeftJumpAimUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftJumpAimUpAnimation = 0xB3F7;

    /// <summary>$91:B3FC, AnimationDelays_90: animation selected by TurningLeftToRightJumpAimUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightJumpAimUpAnimation = 0xB3FC;

    /// <summary>$91:B401, AnimationDelays_91: animation selected by TurningRightToLeftJumpAimDownPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftJumpAimDownAnimation = 0xB401;

    /// <summary>$91:B406, AnimationDelays_92: animation selected by TurningLeftToRightJumpAimDownPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightJumpAimDownAnimation = 0xB406;

    /// <summary>$91:B40B, AnimationDelays_93: animation selected by TurningRightToLeftFallingAimUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftFallingAimUpAnimation = 0xB40B;

    /// <summary>$91:B410, AnimationDelays_94: animation selected by TurningLeftToRightFallingAimUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightFallingAimUpAnimation = 0xB410;

    /// <summary>$91:B415, AnimationDelays_95: animation selected by TurningRightToLeftFallingAimDownPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftFallingAimDownAnimation = 0xB415;

    /// <summary>$91:B41A, AnimationDelays_96: animation selected by TurningLeftToRightFallingAimDownPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightFallingAimDownAnimation = 0xB41A;

    /// <summary>$91:B41F, AnimationDelays_97: animation selected by TurningRightToLeftCrouchingAimUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftCrouchingAimUpAnimation = 0xB41F;

    /// <summary>$91:B424, AnimationDelays_98: animation selected by TurningLeftToRightCrouchingAimUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightCrouchingAimUpAnimation = 0xB424;

    /// <summary>$91:B429, AnimationDelays_99: animation selected by TurningRightToLeftCrouchingAimDiagonalDownPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftCrouchingAimDiagonalDownAnimation = 0xB429;

    /// <summary>$91:B42E, AnimationDelays_9A: animation selected by TurningLeftToRightCrouchingAimDiagonalDownPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightCrouchingAimDiagonalDownAnimation = 0xB42E;

    /// <summary>$91:B433, AnimationDelays_9C: animation selected by TurningRightToLeftAimDiagonalUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftAimDiagonalUpAnimation = 0xB433;

    /// <summary>$91:B438, AnimationDelays_9D: animation selected by TurningLeftToRightAimDiagonalUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightAimDiagonalUpAnimation = 0xB438;

    /// <summary>$91:B43D, AnimationDelays_9E: animation selected by TurningRightToLeftJumpAimDiagonalUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftJumpAimDiagonalUpAnimation = 0xB43D;

    /// <summary>$91:B442, AnimationDelays_9F: animation selected by TurningLeftToRightJumpAimDiagonalUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightJumpAimDiagonalUpAnimation = 0xB442;

    /// <summary>$91:B447, AnimationDelays_A0: animation selected by TurningRightToLeftFallingAimDiagonalUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftFallingAimDiagonalUpAnimation = 0xB447;

    /// <summary>$91:B44C, AnimationDelays_A1: animation selected by TurningLeftToRightFallingAimDiagonalUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightFallingAimDiagonalUpAnimation = 0xB44C;

    /// <summary>$91:B451, AnimationDelays_A2: animation selected by TurningRightToLeftCrouchingAimDiagonalUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningRightToLeftCrouchingAimDiagonalUpAnimation = 0xB451;

    /// <summary>$91:B456, AnimationDelays_A3: animation selected by TurningLeftToRightCrouchingAimDiagonalUpPose and its explicit pose aliases below.</summary>
    private const ushort TurningLeftToRightCrouchingAimDiagonalUpAnimation = 0xB456;

    /// <summary>$91:B22D, AnimationDelays_A4_E6: animation selected by NormalLandingRightPose and its explicit pose aliases below.</summary>
    private const ushort NormalLandingRightAnimation = 0xB22D;

    /// <summary>$91:B231, AnimationDelays_A5_E7: animation selected by NormalLandingLeftPose and its explicit pose aliases below.</summary>
    private const ushort NormalLandingLeftAnimation = 0xB231;

    /// <summary>$91:B235, AnimationDelays_A6: animation selected by SpinLandingRightPose and its explicit pose aliases below.</summary>
    private const ushort SpinLandingRightAnimation = 0xB235;

    /// <summary>$91:B23A, AnimationDelays_A7: animation selected by SpinLandingLeftPose and its explicit pose aliases below.</summary>
    private const ushort SpinLandingLeftAnimation = 0xB23A;

    /// <summary>$91:B2B6, AnimationDelays_A8_A9_AA_AB: animation selected by GrappleStandingRightPose and its explicit pose aliases below.</summary>
    private const ushort GrappleStandingRightAnimation = 0xB2B6;

    /// <summary>$91:B2B8, AnimationDelays_AC_AD: animation selected by UnusedPoseAC and its explicit pose aliases below.</summary>
    private const ushort UnusedPoseACAnimation = 0xB2B8;

    /// <summary>$91:B2BC, AnimationDelays_AE_AF: animation selected by UnusedPoseAE and its explicit pose aliases below.</summary>
    private const ushort UnusedPoseAEAnimation = 0xB2BC;

    /// <summary>$91:B2C0, AnimationDelays_B0_B1: animation selected by UnusedPoseB0 and its explicit pose aliases below.</summary>
    private const ushort UnusedPoseB0Animation = 0xB2C0;

    /// <summary>$91:B2C4, AnimationDelays_B2_B3: animation selected by GrappleSwingRightPose and its explicit pose aliases below.</summary>
    private const ushort GrappleSwingRightAnimation = 0xB2C4;

    /// <summary>$91:B53C, AnimationDelays_BE_F0: animation selected by DraygonGrabbedMovingLeftPose and its explicit pose aliases below.</summary>
    private const ushort DraygonGrabbedMovingLeftAnimation = 0xB53C;

    /// <summary>$91:B45B, AnimationDelays_BF: animation selected by MoonwalkTurnJumpLeftPose and its explicit pose aliases below.</summary>
    private const ushort MoonwalkTurnJumpLeftAnimation = 0xB45B;

    /// <summary>$91:B460, AnimationDelays_C0: animation selected by MoonwalkTurnJumpRightPose and its explicit pose aliases below.</summary>
    private const ushort MoonwalkTurnJumpRightAnimation = 0xB460;

    /// <summary>$91:B465, AnimationDelays_C1: animation selected by MoonwalkTurnJumpAimUpLeftPose and its explicit pose aliases below.</summary>
    private const ushort MoonwalkTurnJumpAimUpLeftAnimation = 0xB465;

    /// <summary>$91:B46A, AnimationDelays_C2: animation selected by MoonwalkTurnJumpAimUpRightPose and its explicit pose aliases below.</summary>
    private const ushort MoonwalkTurnJumpAimUpRightAnimation = 0xB46A;

    /// <summary>$91:B46F, AnimationDelays_C3: animation selected by MoonwalkTurnJumpAimDownLeftPose and its explicit pose aliases below.</summary>
    private const ushort MoonwalkTurnJumpAimDownLeftAnimation = 0xB46F;

    /// <summary>$91:B474, AnimationDelays_C4: animation selected by MoonwalkTurnJumpAimDownRightPose and its explicit pose aliases below.</summary>
    private const ushort MoonwalkTurnJumpAimDownRightAnimation = 0xB474;

    /// <summary>$91:B479, UNUSED_AnimationDelays_C6_91B479: animation selected by UnusedPoseC6 and its explicit pose aliases below.</summary>
    private const ushort UnusedPoseC6Animation = 0xB479;

    /// <summary>$91:B543, AnimationDelays_C9_CA_CB_CC_CD_CE: animation selected by ShinesparkHorizontalRightPose and its explicit pose aliases below.</summary>
    private const ushort ShinesparkHorizontalRightAnimation = 0xB543;

    /// <summary>$91:B545, AnimationDelays_D3: animation selected by CrystalFlashRightPose and its explicit pose aliases below.</summary>
    private const ushort CrystalFlashRightAnimation = 0xB545;

    /// <summary>$91:B556, AnimationDelays_D4: animation selected by CrystalFlashLeftPose and its explicit pose aliases below.</summary>
    private const ushort CrystalFlashLeftAnimation = 0xB556;

    /// <summary>$91:B2AE, AnimationDelays_D5_D6_D9_DA: animation selected by XrayingStandingRightPose and its explicit pose aliases below.</summary>
    private const ushort XrayingStandingRightAnimation = 0xB2AE;

    /// <summary>$91:B567, AnimationDelays_D7_D8: animation selected by DeathSequenceRightPose and its explicit pose aliases below.</summary>
    private const ushort DeathSequenceRightAnimation = 0xB567;

    /// <summary>$91:B4FA, UNUSED_AnimationDelays_DB_91B4FA: animation selected by UnusedPoseDb and its explicit pose aliases below.</summary>
    private const ushort UnusedPoseDbAnimation = 0xB4FA;

    /// <summary>$91:B504, UNUSED_AnimationDelays_DC_91B504: animation selected by UnusedPoseDc and its explicit pose aliases below.</summary>
    private const ushort UnusedPoseDcAnimation = 0xB504;

    /// <summary>$91:B50E, UNUSED_AnimationDelays_DD_91B50E: animation selected by UnusedPoseDd and its explicit pose aliases below.</summary>
    private const ushort UnusedPoseDdAnimation = 0xB50E;

    /// <summary>$91:B513, UNUSED_AnimationDelays_DE_91B513: animation selected by UnusedPoseDe and its explicit pose aliases below.</summary>
    private const ushort UnusedPoseDeAnimation = 0xB513;

    /// <summary>$91:B23F, AnimationDelays_E0: animation selected by LandingAimUpRightPose and its explicit pose aliases below.</summary>
    private const ushort LandingAimUpRightAnimation = 0xB23F;

    /// <summary>$91:B243, AnimationDelays_E1: animation selected by LandingAimUpLeftPose and its explicit pose aliases below.</summary>
    private const ushort LandingAimUpLeftAnimation = 0xB243;

    /// <summary>$91:B247, AnimationDelays_E2: animation selected by LandingAimDiagonalUpRightPose and its explicit pose aliases below.</summary>
    private const ushort LandingAimDiagonalUpRightAnimation = 0xB247;

    /// <summary>$91:B24B, AnimationDelays_E3: animation selected by LandingAimDiagonalUpLeftPose and its explicit pose aliases below.</summary>
    private const ushort LandingAimDiagonalUpLeftAnimation = 0xB24B;

    /// <summary>$91:B24F, AnimationDelays_E4: animation selected by LandingAimDiagonalDownRightPose and its explicit pose aliases below.</summary>
    private const ushort LandingAimDiagonalDownRightAnimation = 0xB24F;

    /// <summary>$91:B253, AnimationDelays_E5: animation selected by LandingAimDiagonalDownLeftPose and its explicit pose aliases below.</summary>
    private const ushort LandingAimDiagonalDownLeftAnimation = 0xB253;

    /// <summary>$91:B257, AnimationDelays_E8: animation selected by DrainedCrouchingRightPose and its explicit pose aliases below.</summary>
    private const ushort DrainedCrouchingRightAnimation = 0xB257;

    /// <summary>$91:B268, AnimationDelays_E9: animation selected by DrainedCrouchingLeftPose and its explicit pose aliases below.</summary>
    private const ushort DrainedCrouchingLeftAnimation = 0xB268;

    /// <summary>$91:B288, AnimationDelays_EA: animation selected by DrainedStandingRightPose and its explicit pose aliases below.</summary>
    private const ushort DrainedStandingRightAnimation = 0xB288;

    /// <summary>$91:B290, AnimationDelays_EB: animation selected by DrainedStandingLeftPose and its explicit pose aliases below.</summary>
    private const ushort DrainedStandingLeftAnimation = 0xB290;

    /// <summary>$91:B518, AnimationDelays_F1: animation selected by CrouchingTransitionAimUpRightPose and its explicit pose aliases below.</summary>
    private const ushort CrouchingTransitionAimUpRightAnimation = 0xB518;

    /// <summary>$91:B51B, AnimationDelays_F2: animation selected by CrouchingTransitionAimUpLeftPose and its explicit pose aliases below.</summary>
    private const ushort CrouchingTransitionAimUpLeftAnimation = 0xB51B;

    /// <summary>$91:B51E, AnimationDelays_F3: animation selected by CrouchingTransitionAimDiagonalUpRightPose and its explicit pose aliases below.</summary>
    private const ushort CrouchingTransitionAimDiagonalUpRightAnimation = 0xB51E;

    /// <summary>$91:B521, AnimationDelays_F4: animation selected by CrouchingTransitionAimDiagonalUpLeftPose and its explicit pose aliases below.</summary>
    private const ushort CrouchingTransitionAimDiagonalUpLeftAnimation = 0xB521;

    /// <summary>$91:B524, AnimationDelays_F5: animation selected by CrouchingTransitionAimDiagonalDownRightPose and its explicit pose aliases below.</summary>
    private const ushort CrouchingTransitionAimDiagonalDownRightAnimation = 0xB524;

    /// <summary>$91:B527, AnimationDelays_F6: animation selected by CrouchingTransitionAimDiagonalDownLeftPose and its explicit pose aliases below.</summary>
    private const ushort CrouchingTransitionAimDiagonalDownLeftAnimation = 0xB527;

    /// <summary>$91:B52A, AnimationDelays_F7: animation selected by StandingTransitionAimUpRightPose and its explicit pose aliases below.</summary>
    private const ushort StandingTransitionAimUpRightAnimation = 0xB52A;

    /// <summary>$91:B52D, AnimationDelays_F8: animation selected by StandingTransitionAimUpLeftPose and its explicit pose aliases below.</summary>
    private const ushort StandingTransitionAimUpLeftAnimation = 0xB52D;

    /// <summary>$91:B530, AnimationDelays_F9: animation selected by StandingTransitionAimDiagonalUpRightPose and its explicit pose aliases below.</summary>
    private const ushort StandingTransitionAimDiagonalUpRightAnimation = 0xB530;

    /// <summary>$91:B533, AnimationDelays_FA: animation selected by StandingTransitionAimDiagonalUpLeftPose and its explicit pose aliases below.</summary>
    private const ushort StandingTransitionAimDiagonalUpLeftAnimation = 0xB533;

    /// <summary>$91:B536, AnimationDelays_FB: animation selected by StandingTransitionAimDiagonalDownRightPose and its explicit pose aliases below.</summary>
    private const ushort StandingTransitionAimDiagonalDownRightAnimation = 0xB536;

    /// <summary>$91:B539, AnimationDelays_FC: animation selected by StandingTransitionAimDiagonalDownLeftPose and its explicit pose aliases below.</summary>
    private const ushort StandingTransitionAimDiagonalDownLeftAnimation = 0xB539;

    /// <summary>Resolve the native bank-$91 list pointer for any pose byte.</summary>
    internal static ushort PointerForPose(byte pose)
    {
        if (pose >= RealPoseCount)
        {
            int offset = DelayStreamsAddress + (pose - RealPoseCount) * sizeof(ushort);
            return (ushort)(ReadCompiledByte(offset) | ReadCompiledByte(offset + 1) << 8);
        }
        return (SamusPoseId)pose switch
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
            _ => throw new ArgumentOutOfRangeException(nameof(pose)),
        };
    }

    /// <summary>Read an immutable delay/command byte, retaining low-bank mutable aliases.</summary>
    internal static byte ReadAnimationByte(ISnesAddressSpace bus, ushort listPointer, ushort byteIndex)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int address = 0x910000 | unchecked((ushort)(listPointer + byteIndex));
        if (address >= DelayStreamsAddress && address < DelayStreamsEndExclusive)
            return SamusAnimationDelayPrograms.ByteAt(address & ushort.MaxValue);
        // Invalid pose bytes $FD-$FF select $0302 through the native table overread.
        // Bank $91's lower half aliases mutable WRAM and must not be compiled.
        if (address < 0x918000)
            return (bus as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Mutable animation-delay alias requires WRAM access."))
                .ReadWorkRamByte(address);
        throw new InvalidDataException(
            $"Samus animation delay byte ${address:X6} is outside the compiled pose streams.");
    }

    /// <summary>Read the bounded native table image for byte-for-byte verification.</summary>
    internal static byte ReadCompiledByte(int address)
    {
        if (address >= PointerTableAddress && address < DelayStreamsAddress)
        {
            int byteIndex = address - PointerTableAddress;
            ushort pointer = PointerForPose((byte)(byteIndex / 2));
            return unchecked((byte)(pointer >> ((byteIndex & 1) * 8)));
        }
        if (address >= DelayStreamsAddress && address < DelayStreamsEndExclusive)
            return SamusAnimationDelayPrograms.ByteAt(address & ushort.MaxValue);
        throw new ArgumentOutOfRangeException(nameof(address), address,
            "Address is outside the compiled Samus animation definition catalog.");
    }
}
