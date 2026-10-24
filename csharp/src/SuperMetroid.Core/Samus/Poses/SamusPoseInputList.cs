namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The bank-$91 pose input-transition lists selected by each Samus pose.</summary>
internal enum SamusPoseInputList
{
    /// <summary>$91:A0DC TransitionTable list for pose $2F: diagnostic list identity.</summary>
    EmptyTransitionList,
    /// <summary>$91:A0DE TransitionTable_00_9B_FacingForward: diagnostic list identity.</summary>
    ForwardFacingPowerSuitPoseList,
    /// <summary>$91:A0EC TransitionTable_01_03_05_07_A4_A6_E0_E2_E4_E6_FacingRight: diagnostic list identity.</summary>
    FacingRightNormalPoseList,
    /// <summary>$91:A172 TransitionTable_02_04_06_08_A5_A7_E1_E3_E5_E7_FacingLeft: diagnostic list identity.</summary>
    FacingLeftNormalPoseList,
    /// <summary>$91:A1F8 TransitionTable_09_0D_0F_11_MovingRight: diagnostic list identity.</summary>
    MovingRightNormalPoseList,
    /// <summary>$91:A242 TransitionTable_0A_0E_10_12_MovingLeft: diagnostic list identity.</summary>
    MovingLeftNormalPoseList,
    /// <summary>$91:A28C TransitionTable_4B_55_57_59_FacingRight_NormalJumpTransition: diagnostic list identity.</summary>
    NeutralJumpTransitionRightPoseList,
    /// <summary>$91:A2BE TransitionTable_4C_56_58_5A_FacingLeft_NormalJumpTransition: diagnostic list identity.</summary>
    NeutralJumpTransitionLeftPoseList,
    /// <summary>$91:A2F6 TransitionTable_15_4D_51_69_6B_FacingRight_NormalJump: diagnostic list identity.</summary>
    NormalJumpAimUpRightPoseList,
    /// <summary>$91:A376 TransitionTable_16_4E_52_6A_6C_FacingLeft_NormalJump: diagnostic list identity.</summary>
    NormalJumpAimUpLeftPoseList,
    /// <summary>$91:A3F6 TransitionTable_4F_FacingLeft_DamageBoost: diagnostic list identity.</summary>
    DamageBoostLeftPoseList,
    /// <summary>$91:A40A TransitionTable_50_FacingRight_DamageBoost: diagnostic list identity.</summary>
    DamageBoostRightPoseList,
    /// <summary>$91:A41E TransitionTable_19_FacingRight_SpinJump: diagnostic list identity.</summary>
    SpinJumpRightPoseList,
    /// <summary>$91:A46E TransitionTable_1A_FacingLeft_SpinJump: diagnostic list identity.</summary>
    SpinJumpLeftPoseList,
    /// <summary>$91:A4BE TransitionTable_1B_FacingRight_SpaceJump: diagnostic list identity.</summary>
    SpaceJumpRightPoseList,
    /// <summary>$91:A50E TransitionTable_1C_FacingLeft_SpaceJump: diagnostic list identity.</summary>
    SpaceJumpLeftPoseList,
    /// <summary>$91:A55E TransitionTable_81_ScrewAttack: diagnostic list identity.</summary>
    ScrewAttackRightPoseList,
    /// <summary>$91:A5AE TransitionTable_82_FacingLeft_ScrewAttack: diagnostic list identity.</summary>
    ScrewAttackLeftPoseList,
    /// <summary>$91:A5FE TransitionTable_1D_FaceRight_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    MorphBallGroundRightPoseList,
    /// <summary>$91:A618 TransitionTable_1E_MoveRight_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    MorphBallMovingRightPoseList,
    /// <summary>$91:A632 TransitionTable_1F_MoveLeft_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    MorphBallMovingLeftPoseList,
    /// <summary>$91:A64C TransitionTable_1D_FaceLeft_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    MorphBallGroundLeftPoseList,
    /// <summary>$91:A666 TransitionTable list for pose $20: diagnostic list identity.</summary>
    UnusedPose20List,
    /// <summary>$91:A668 TransitionTable list for pose $23: diagnostic list identity.</summary>
    UnusedPose23List,
    /// <summary>$91:A66A TransitionTable list for pose $42: diagnostic list identity.</summary>
    UnusedPose42List,
    /// <summary>$91:A66C TransitionTable_27_71_73_85_FacingRight_Crouching: diagnostic list identity.</summary>
    CrouchingRightPoseList,
    /// <summary>$91:A6BC TransitionTable_28_72_74_86_Crouching: diagnostic list identity.</summary>
    CrouchingLeftPoseList,
    /// <summary>$91:A70C TransitionTable_29_2B_6D_6F_FacingRight_Falling: diagnostic list identity.</summary>
    FallingRightPoseList,
    /// <summary>$91:A750 TransitionTable_2A_2C_6E_70_FacingLeft_Falling: diagnostic list identity.</summary>
    FallingLeftPoseList,
    /// <summary>$91:A794 TransitionTable_31_FacingRight_MorphBall_NoSpringBall_InAir: diagnostic list identity.</summary>
    MorphBallFallingRightPoseList,
    /// <summary>$91:A7AE TransitionTable_32_FacingLeft_MorphBall_NoSpringBall_InAir: diagnostic list identity.</summary>
    MorphBallFallingLeftPoseList,
    /// <summary>$91:A7C8 TransitionTable list for pose $33: diagnostic list identity.</summary>
    UnusedKnockbackRightPoseList,
    /// <summary>$91:A7CA TransitionTable list for pose $34: diagnostic list identity.</summary>
    UnusedKnockbackLeftPoseList,
    /// <summary>$91:A7CC UNUSED_TransitionTable_45_91A7CC: diagnostic list identity.</summary>
    UnusedPose45List,
    /// <summary>$91:A7E0 UNUSED_TransitionTable_46_91A7E0: diagnostic list identity.</summary>
    UnusedPose46List,
    /// <summary>$91:A7F4 TransitionTable list for pose $47: diagnostic list identity.</summary>
    UnusedPose47List,
    /// <summary>$91:A834 TransitionTable list for pose $48: diagnostic list identity.</summary>
    UnusedPose48List,
    /// <summary>$91:A874 TransitionTable_49_75_77_FacingLeft_Moonwalk: diagnostic list identity.</summary>
    MoonwalkFacingLeftPoseList,
    /// <summary>$91:A8AC TransitionTable_4A_76_78_FacingRight_Moonwalk: diagnostic list identity.</summary>
    MoonwalkFacingRightPoseList,
    /// <summary>$91:A8E4 TransitionTable_53_FacingRight_Knockback: diagnostic list identity.</summary>
    KnockbackRightPoseList,
    /// <summary>$91:A8EC TransitionTable_54_FacingLeft_Knockback: diagnostic list identity.</summary>
    KnockbackLeftPoseList,
    /// <summary>$91:A8FC UNUSED_TransitionTable_5B_91A8FC: diagnostic list identity.</summary>
    UnusedPose5BList,
    /// <summary>$91:A904 UNUSED_TransitionTable_5C_91A904: diagnostic list identity.</summary>
    UnusedPose5CList,
    /// <summary>$91:A90C TransitionTable_79_7B_FacingRight_MorphBall_Spring_OnGround: diagnostic list identity.</summary>
    SpringBallGroundRightPoseList,
    /// <summary>$91:A926 TransitionTable_7A_7C_FacingLeft_MorphBall_Spring_OnGround: diagnostic list identity.</summary>
    SpringBallGroundLeftPoseList,
    /// <summary>$91:A940 TransitionTable_7D_FacingRight_MorphBall_SpringBall_Falling: diagnostic list identity.</summary>
    SpringBallFallingRightPoseList,
    /// <summary>$91:A954 TransitionTable_7E_FacingLeft_MorphBall_SpringBall_Falling: diagnostic list identity.</summary>
    SpringBallFallingLeftPoseList,
    /// <summary>$91:A968 TransitionTable_7F_FacingRight_MorphBall_SpringBall_InAir: diagnostic list identity.</summary>
    SpringBallJumpRightPoseList,
    /// <summary>$91:A97C TransitionTable_80_FacingLeft_MorphBall_SpringBall_InAir: diagnostic list identity.</summary>
    SpringBallJumpLeftPoseList,
    /// <summary>$91:A990 UNUSED_TransitionTable_63_91A990: diagnostic list identity.</summary>
    UnusedPose63List,
    /// <summary>$91:A998 UNUSED_TransitionTable_64_91A998: diagnostic list identity.</summary>
    UnusedPose64List,
    /// <summary>$91:A9A0 UNUSED_TransitionTable_65_91A9A0: diagnostic list identity.</summary>
    UnusedPose65List,
    /// <summary>$91:A9C6 UNUSED_TransitionTable_66_91A9C6: diagnostic list identity.</summary>
    UnusedPose66List,
    /// <summary>$91:A9EC TransitionTable_83_FacingRight_WallJump: diagnostic list identity.</summary>
    WallJumpRightPoseList,
    /// <summary>$91:AA12 TransitionTable_84_FacingLeft_WallJump: diagnostic list identity.</summary>
    WallJumpLeftPoseList,
    /// <summary>$91:AA38 TransitionTable_89_CF_D1_FacingRight_RanIntoAWall: diagnostic list identity.</summary>
    RanIntoWallRightPoseList,
    /// <summary>$91:AA7C TransitionTable_8A_D0_D2_FacingLeft_RanIntoAWall: diagnostic list identity.</summary>
    RanIntoWallLeftPoseList,
    /// <summary>$91:AAC0 TransitionTable_13_FaceRight_NormalJump_NotMoving_GunExtend: diagnostic list identity.</summary>
    NormalJumpGunExtendedRightPoseList,
    /// <summary>$91:AB3A TransitionTable_14_FacingLeft_NormalJump_NotMoving_GunExtend: diagnostic list identity.</summary>
    NormalJumpGunExtendedLeftPoseList,
    /// <summary>$91:ABB4 TransitionTable_17_FacingRight_NormalJump_AimingDown: diagnostic list identity.</summary>
    NormalJumpAimDownRightPoseList,
    /// <summary>$91:AC40 TransitionTable_18_FacingLeft_NormalJump_AimingDown: diagnostic list identity.</summary>
    NormalJumpAimDownLeftPoseList,
    /// <summary>$91:ACCC TransitionTable_3D_FacingRight_Unmorphing: diagnostic list identity.</summary>
    UnmorphingTransitionRightPoseList,
    /// <summary>$91:ACE0 TransitionTable_3E_FacingLeft_Unmorphing: diagnostic list identity.</summary>
    UnmorphingTransitionLeftPoseList,
    /// <summary>$91:ACF4 TransitionTable_25_FacingRight_Turning_Standing: diagnostic list identity.</summary>
    TurningRightToLeftPoseList,
    /// <summary>$91:AD08 TransitionTable_26_FacingLeft_Turning_Standing: diagnostic list identity.</summary>
    TurningLeftToRightPoseList,
    /// <summary>$91:AD1C TransitionTable_8B_FacingRight_Turning_Standing_AimingUp: diagnostic list identity.</summary>
    TurningRightToLeftAimUpPoseList,
    /// <summary>$91:AD30 TransitionTable_8C_FacingLeft_Turning_Standing_AimingUp: diagnostic list identity.</summary>
    TurningLeftToRightAimUpPoseList,
    /// <summary>$91:AD44 TransitionTable_8D_FacingRight_Turning_Standing_AimDownRight: diagnostic list identity.</summary>
    TurningRightToLeftAimDiagonalDownPoseList,
    /// <summary>$91:AD58 TransitionTable_8E_FacingLeft_Turning_Standing_AimDownLeft: diagnostic list identity.</summary>
    TurningLeftToRightAimDiagonalDownPoseList,
    /// <summary>$91:AD6C TransitionTable_C7_FacingRight_VerticalShinesparkWindup: diagnostic list identity.</summary>
    ShinesparkWindupRightPoseList,
    /// <summary>$91:AD80 TransitionTable_C8_FacingLeft_VerticalShinesparkWindup: diagnostic list identity.</summary>
    ShinesparkWindupLeftPoseList,
    /// <summary>$91:AD94 TransitionTable_2D_FacingRight_Falling_AimingDown: diagnostic list identity.</summary>
    FallingAimDownRightPoseList,
    /// <summary>$91:ADD2 TransitionTable_2E_FacingLeft_Falling_AimingDown: diagnostic list identity.</summary>
    FallingAimDownLeftPoseList,
    /// <summary>$91:AE10 UNUSED_TransitionTable_DF_91AE10: diagnostic list identity.</summary>
    UnusedPoseDfList,
    /// <summary>$91:AE18 TransitionTable_BA_BB_BC_BD_BE_FacingLeft_GrabbedByDraygon: diagnostic list identity.</summary>
    DraygonGrabbedNeutralLeftPoseList,
    /// <summary>$91:AE56 TransitionTable_EC_ED_EE_EF_F0_FacingRight_GrabbedByDraygon: diagnostic list identity.</summary>
    DraygonGrabbedNeutralRightPoseList,
    /// <summary>$91:AE94 TransitionTable_0B_MovingRight_GunExtended: diagnostic list identity.</summary>
    MovingRightGunExtendedPoseList,
    /// <summary>$91:AEDE TransitionTable_0C_MovingLeft_GunExtended: diagnostic list identity.</summary>
    MovingLeftGunExtendedPoseList,
    /// <summary>$91:AF28 TransitionTable_67_FacingRight_Falling_GunExtended: diagnostic list identity.</summary>
    FallingGunExtendedRightPoseList,
    /// <summary>$91:AF60 TransitionTable_68_FacingLeft_Falling_GunExtended: diagnostic list identity.</summary>
    FallingGunExtendedLeftPoseList,
    /// <summary>$91:AF98 TransitionTable_BF_FacingRight_Moonwalking_TurnJumpLeft: diagnostic list identity.</summary>
    MoonwalkTurnJumpLeftPoseList,
    /// <summary>$91:AFAC TransitionTable_C0_FacingLeft_Moonwalking_TurnJumpRight: diagnostic list identity.</summary>
    MoonwalkTurnJumpRightPoseList,
    /// <summary>$91:AFC0 TransitionTable_C1_FaceRight_Moonwalk_TurnJumpLeft_AimUpRight: diagnostic list identity.</summary>
    MoonwalkTurnJumpAimUpLeftPoseList,
    /// <summary>$91:AFD4 TransitionTable_C2_FaceLeft_Moonwalk_TurnJumpRight_AimUpLeft: diagnostic list identity.</summary>
    MoonwalkTurnJumpAimUpRightPoseList,
    /// <summary>$91:AFE8 TransitionTable_C3_FaceRight_Moonwalk_TurnJumpLeft_AimDownRight: diagnostic list identity.</summary>
    MoonwalkTurnJumpAimDownLeftPoseList,
    /// <summary>$91:AFFC TransitionTable_C4_FaceLeft_Moonwalk_TurnJumpRight_AimDownLeft: diagnostic list identity.</summary>
    MoonwalkTurnJumpAimDownRightPoseList,
}
