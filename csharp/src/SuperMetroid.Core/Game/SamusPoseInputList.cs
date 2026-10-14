namespace SuperMetroid.Core.Game;

/// <summary>The bank-$91 pose input-transition lists selected by each Samus pose.</summary>
internal enum SamusPoseInputList : ushort
{
    /// <summary>$91:A0DC TransitionTable list for pose $2F: diagnostic list identity.</summary>
    EmptyTransitionList = 0xa0dc,
    /// <summary>$91:A0DE TransitionTable_00_9B_FacingForward: diagnostic list identity.</summary>
    ForwardFacingPowerSuitPoseList = 0xa0de,
    /// <summary>$91:A0EC TransitionTable_01_03_05_07_A4_A6_E0_E2_E4_E6_FacingRight: diagnostic list identity.</summary>
    FacingRightNormalPoseList = 0xa0ec,
    /// <summary>$91:A172 TransitionTable_02_04_06_08_A5_A7_E1_E3_E5_E7_FacingLeft: diagnostic list identity.</summary>
    FacingLeftNormalPoseList = 0xa172,
    /// <summary>$91:A1F8 TransitionTable_09_0D_0F_11_MovingRight: diagnostic list identity.</summary>
    MovingRightNormalPoseList = 0xa1f8,
    /// <summary>$91:A242 TransitionTable_0A_0E_10_12_MovingLeft: diagnostic list identity.</summary>
    MovingLeftNormalPoseList = 0xa242,
    /// <summary>$91:A28C TransitionTable_4B_55_57_59_FacingRight_NormalJumpTransition: diagnostic list identity.</summary>
    NeutralJumpTransitionRightPoseList = 0xa28c,
    /// <summary>$91:A2BE TransitionTable_4C_56_58_5A_FacingLeft_NormalJumpTransition: diagnostic list identity.</summary>
    NeutralJumpTransitionLeftPoseList = 0xa2be,
    /// <summary>$91:A2F6 TransitionTable_15_4D_51_69_6B_FacingRight_NormalJump: diagnostic list identity.</summary>
    NormalJumpAimUpRightPoseList = 0xa2f6,
    /// <summary>$91:A376 TransitionTable_16_4E_52_6A_6C_FacingLeft_NormalJump: diagnostic list identity.</summary>
    NormalJumpAimUpLeftPoseList = 0xa376,
    /// <summary>$91:A3F6 TransitionTable_4F_FacingLeft_DamageBoost: diagnostic list identity.</summary>
    DamageBoostLeftPoseList = 0xa3f6,
    /// <summary>$91:A40A TransitionTable_50_FacingRight_DamageBoost: diagnostic list identity.</summary>
    DamageBoostRightPoseList = 0xa40a,
    /// <summary>$91:A41E TransitionTable_19_FacingRight_SpinJump: diagnostic list identity.</summary>
    SpinJumpRightPoseList = 0xa41e,
    /// <summary>$91:A46E TransitionTable_1A_FacingLeft_SpinJump: diagnostic list identity.</summary>
    SpinJumpLeftPoseList = 0xa46e,
    /// <summary>$91:A4BE TransitionTable_1B_FacingRight_SpaceJump: diagnostic list identity.</summary>
    SpaceJumpRightPoseList = 0xa4be,
    /// <summary>$91:A50E TransitionTable_1C_FacingLeft_SpaceJump: diagnostic list identity.</summary>
    SpaceJumpLeftPoseList = 0xa50e,
    /// <summary>$91:A55E TransitionTable_81_ScrewAttack: diagnostic list identity.</summary>
    ScrewAttackRightPoseList = 0xa55e,
    /// <summary>$91:A5AE TransitionTable_82_FacingLeft_ScrewAttack: diagnostic list identity.</summary>
    ScrewAttackLeftPoseList = 0xa5ae,
    /// <summary>$91:A5FE TransitionTable_1D_FaceRight_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    MorphBallGroundRightPoseList = 0xa5fe,
    /// <summary>$91:A618 TransitionTable_1E_MoveRight_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    MorphBallMovingRightPoseList = 0xa618,
    /// <summary>$91:A632 TransitionTable_1F_MoveLeft_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    MorphBallMovingLeftPoseList = 0xa632,
    /// <summary>$91:A64C TransitionTable_1D_FaceLeft_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    MorphBallGroundLeftPoseList = 0xa64c,
    /// <summary>$91:A666 TransitionTable list for pose $20: diagnostic list identity.</summary>
    UnusedPose20List = 0xa666,
    /// <summary>$91:A668 TransitionTable list for pose $23: diagnostic list identity.</summary>
    UnusedPose23List = 0xa668,
    /// <summary>$91:A66A TransitionTable list for pose $42: diagnostic list identity.</summary>
    UnusedPose42List = 0xa66a,
    /// <summary>$91:A66C TransitionTable_27_71_73_85_FacingRight_Crouching: diagnostic list identity.</summary>
    CrouchingRightPoseList = 0xa66c,
    /// <summary>$91:A6BC TransitionTable_28_72_74_86_Crouching: diagnostic list identity.</summary>
    CrouchingLeftPoseList = 0xa6bc,
    /// <summary>$91:A70C TransitionTable_29_2B_6D_6F_FacingRight_Falling: diagnostic list identity.</summary>
    FallingRightPoseList = 0xa70c,
    /// <summary>$91:A750 TransitionTable_2A_2C_6E_70_FacingLeft_Falling: diagnostic list identity.</summary>
    FallingLeftPoseList = 0xa750,
    /// <summary>$91:A794 TransitionTable_31_FacingRight_MorphBall_NoSpringBall_InAir: diagnostic list identity.</summary>
    MorphBallFallingRightPoseList = 0xa794,
    /// <summary>$91:A7AE TransitionTable_32_FacingLeft_MorphBall_NoSpringBall_InAir: diagnostic list identity.</summary>
    MorphBallFallingLeftPoseList = 0xa7ae,
    /// <summary>$91:A7C8 TransitionTable list for pose $33: diagnostic list identity.</summary>
    UnusedKnockbackRightPoseList = 0xa7c8,
    /// <summary>$91:A7CA TransitionTable list for pose $34: diagnostic list identity.</summary>
    UnusedKnockbackLeftPoseList = 0xa7ca,
    /// <summary>$91:A7CC UNUSED_TransitionTable_45_91A7CC: diagnostic list identity.</summary>
    UnusedPose45List = 0xa7cc,
    /// <summary>$91:A7E0 UNUSED_TransitionTable_46_91A7E0: diagnostic list identity.</summary>
    UnusedPose46List = 0xa7e0,
    /// <summary>$91:A7F4 TransitionTable list for pose $47: diagnostic list identity.</summary>
    UnusedPose47List = 0xa7f4,
    /// <summary>$91:A834 TransitionTable list for pose $48: diagnostic list identity.</summary>
    UnusedPose48List = 0xa834,
    /// <summary>$91:A874 TransitionTable_49_75_77_FacingLeft_Moonwalk: diagnostic list identity.</summary>
    MoonwalkFacingLeftPoseList = 0xa874,
    /// <summary>$91:A8AC TransitionTable_4A_76_78_FacingRight_Moonwalk: diagnostic list identity.</summary>
    MoonwalkFacingRightPoseList = 0xa8ac,
    /// <summary>$91:A8E4 TransitionTable_53_FacingRight_Knockback: diagnostic list identity.</summary>
    KnockbackRightPoseList = 0xa8e4,
    /// <summary>$91:A8EC TransitionTable_54_FacingLeft_Knockback: diagnostic list identity.</summary>
    KnockbackLeftPoseList = 0xa8ec,
    /// <summary>$91:A8FC UNUSED_TransitionTable_5B_91A8FC: diagnostic list identity.</summary>
    UnusedPose5BList = 0xa8fc,
    /// <summary>$91:A904 UNUSED_TransitionTable_5C_91A904: diagnostic list identity.</summary>
    UnusedPose5CList = 0xa904,
    /// <summary>$91:A90C TransitionTable_79_7B_FacingRight_MorphBall_Spring_OnGround: diagnostic list identity.</summary>
    SpringBallGroundRightPoseList = 0xa90c,
    /// <summary>$91:A926 TransitionTable_7A_7C_FacingLeft_MorphBall_Spring_OnGround: diagnostic list identity.</summary>
    SpringBallGroundLeftPoseList = 0xa926,
    /// <summary>$91:A940 TransitionTable_7D_FacingRight_MorphBall_SpringBall_Falling: diagnostic list identity.</summary>
    SpringBallFallingRightPoseList = 0xa940,
    /// <summary>$91:A954 TransitionTable_7E_FacingLeft_MorphBall_SpringBall_Falling: diagnostic list identity.</summary>
    SpringBallFallingLeftPoseList = 0xa954,
    /// <summary>$91:A968 TransitionTable_7F_FacingRight_MorphBall_SpringBall_InAir: diagnostic list identity.</summary>
    SpringBallJumpRightPoseList = 0xa968,
    /// <summary>$91:A97C TransitionTable_80_FacingLeft_MorphBall_SpringBall_InAir: diagnostic list identity.</summary>
    SpringBallJumpLeftPoseList = 0xa97c,
    /// <summary>$91:A990 UNUSED_TransitionTable_63_91A990: diagnostic list identity.</summary>
    UnusedPose63List = 0xa990,
    /// <summary>$91:A998 UNUSED_TransitionTable_64_91A998: diagnostic list identity.</summary>
    UnusedPose64List = 0xa998,
    /// <summary>$91:A9A0 UNUSED_TransitionTable_65_91A9A0: diagnostic list identity.</summary>
    UnusedPose65List = 0xa9a0,
    /// <summary>$91:A9C6 UNUSED_TransitionTable_66_91A9C6: diagnostic list identity.</summary>
    UnusedPose66List = 0xa9c6,
    /// <summary>$91:A9EC TransitionTable_83_FacingRight_WallJump: diagnostic list identity.</summary>
    WallJumpRightPoseList = 0xa9ec,
    /// <summary>$91:AA12 TransitionTable_84_FacingLeft_WallJump: diagnostic list identity.</summary>
    WallJumpLeftPoseList = 0xaa12,
    /// <summary>$91:AA38 TransitionTable_89_CF_D1_FacingRight_RanIntoAWall: diagnostic list identity.</summary>
    RanIntoWallRightPoseList = 0xaa38,
    /// <summary>$91:AA7C TransitionTable_8A_D0_D2_FacingLeft_RanIntoAWall: diagnostic list identity.</summary>
    RanIntoWallLeftPoseList = 0xaa7c,
    /// <summary>$91:AAC0 TransitionTable_13_FaceRight_NormalJump_NotMoving_GunExtend: diagnostic list identity.</summary>
    NormalJumpGunExtendedRightPoseList = 0xaac0,
    /// <summary>$91:AB3A TransitionTable_14_FacingLeft_NormalJump_NotMoving_GunExtend: diagnostic list identity.</summary>
    NormalJumpGunExtendedLeftPoseList = 0xab3a,
    /// <summary>$91:ABB4 TransitionTable_17_FacingRight_NormalJump_AimingDown: diagnostic list identity.</summary>
    NormalJumpAimDownRightPoseList = 0xabb4,
    /// <summary>$91:AC40 TransitionTable_18_FacingLeft_NormalJump_AimingDown: diagnostic list identity.</summary>
    NormalJumpAimDownLeftPoseList = 0xac40,
    /// <summary>$91:ACCC TransitionTable_3D_FacingRight_Unmorphing: diagnostic list identity.</summary>
    UnmorphingTransitionRightPoseList = 0xaccc,
    /// <summary>$91:ACE0 TransitionTable_3E_FacingLeft_Unmorphing: diagnostic list identity.</summary>
    UnmorphingTransitionLeftPoseList = 0xace0,
    /// <summary>$91:ACF4 TransitionTable_25_FacingRight_Turning_Standing: diagnostic list identity.</summary>
    TurningRightToLeftPoseList = 0xacf4,
    /// <summary>$91:AD08 TransitionTable_26_FacingLeft_Turning_Standing: diagnostic list identity.</summary>
    TurningLeftToRightPoseList = 0xad08,
    /// <summary>$91:AD1C TransitionTable_8B_FacingRight_Turning_Standing_AimingUp: diagnostic list identity.</summary>
    TurningRightToLeftAimUpPoseList = 0xad1c,
    /// <summary>$91:AD30 TransitionTable_8C_FacingLeft_Turning_Standing_AimingUp: diagnostic list identity.</summary>
    TurningLeftToRightAimUpPoseList = 0xad30,
    /// <summary>$91:AD44 TransitionTable_8D_FacingRight_Turning_Standing_AimDownRight: diagnostic list identity.</summary>
    TurningRightToLeftAimDiagonalDownPoseList = 0xad44,
    /// <summary>$91:AD58 TransitionTable_8E_FacingLeft_Turning_Standing_AimDownLeft: diagnostic list identity.</summary>
    TurningLeftToRightAimDiagonalDownPoseList = 0xad58,
    /// <summary>$91:AD6C TransitionTable_C7_FacingRight_VerticalShinesparkWindup: diagnostic list identity.</summary>
    ShinesparkWindupRightPoseList = 0xad6c,
    /// <summary>$91:AD80 TransitionTable_C8_FacingLeft_VerticalShinesparkWindup: diagnostic list identity.</summary>
    ShinesparkWindupLeftPoseList = 0xad80,
    /// <summary>$91:AD94 TransitionTable_2D_FacingRight_Falling_AimingDown: diagnostic list identity.</summary>
    FallingAimDownRightPoseList = 0xad94,
    /// <summary>$91:ADD2 TransitionTable_2E_FacingLeft_Falling_AimingDown: diagnostic list identity.</summary>
    FallingAimDownLeftPoseList = 0xadd2,
    /// <summary>$91:AE10 UNUSED_TransitionTable_DF_91AE10: diagnostic list identity.</summary>
    UnusedPoseDfList = 0xae10,
    /// <summary>$91:AE18 TransitionTable_BA_BB_BC_BD_BE_FacingLeft_GrabbedByDraygon: diagnostic list identity.</summary>
    DraygonGrabbedNeutralLeftPoseList = 0xae18,
    /// <summary>$91:AE56 TransitionTable_EC_ED_EE_EF_F0_FacingRight_GrabbedByDraygon: diagnostic list identity.</summary>
    DraygonGrabbedNeutralRightPoseList = 0xae56,
    /// <summary>$91:AE94 TransitionTable_0B_MovingRight_GunExtended: diagnostic list identity.</summary>
    MovingRightGunExtendedPoseList = 0xae94,
    /// <summary>$91:AEDE TransitionTable_0C_MovingLeft_GunExtended: diagnostic list identity.</summary>
    MovingLeftGunExtendedPoseList = 0xaede,
    /// <summary>$91:AF28 TransitionTable_67_FacingRight_Falling_GunExtended: diagnostic list identity.</summary>
    FallingGunExtendedRightPoseList = 0xaf28,
    /// <summary>$91:AF60 TransitionTable_68_FacingLeft_Falling_GunExtended: diagnostic list identity.</summary>
    FallingGunExtendedLeftPoseList = 0xaf60,
    /// <summary>$91:AF98 TransitionTable_BF_FacingRight_Moonwalking_TurnJumpLeft: diagnostic list identity.</summary>
    MoonwalkTurnJumpLeftPoseList = 0xaf98,
    /// <summary>$91:AFAC TransitionTable_C0_FacingLeft_Moonwalking_TurnJumpRight: diagnostic list identity.</summary>
    MoonwalkTurnJumpRightPoseList = 0xafac,
    /// <summary>$91:AFC0 TransitionTable_C1_FaceRight_Moonwalk_TurnJumpLeft_AimUpRight: diagnostic list identity.</summary>
    MoonwalkTurnJumpAimUpLeftPoseList = 0xafc0,
    /// <summary>$91:AFD4 TransitionTable_C2_FaceLeft_Moonwalk_TurnJumpRight_AimUpLeft: diagnostic list identity.</summary>
    MoonwalkTurnJumpAimUpRightPoseList = 0xafd4,
    /// <summary>$91:AFE8 TransitionTable_C3_FaceRight_Moonwalk_TurnJumpLeft_AimDownRight: diagnostic list identity.</summary>
    MoonwalkTurnJumpAimDownLeftPoseList = 0xafe8,
    /// <summary>$91:AFFC TransitionTable_C4_FaceLeft_Moonwalk_TurnJumpRight_AimDownLeft: diagnostic list identity.</summary>
    MoonwalkTurnJumpAimDownRightPoseList = 0xaffc,
}
