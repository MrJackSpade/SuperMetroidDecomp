namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The pose input-transition lists selected by each Samus pose.</summary>
internal enum SamusPoseInputList
{
    /// <summary>TransitionTable list for <see cref="SamusPoseId.TurningRightToLeftJumpPose"/>: diagnostic list identity.</summary>
    EmptyTransitionList,
    /// <summary>TransitionTable_00_9B_FacingForward: diagnostic list identity.</summary>
    ForwardFacingPowerSuitPoseList,
    /// <summary>TransitionTable_01_03_05_07_A4_A6_E0_E2_E4_E6_FacingRight: diagnostic list identity.</summary>
    FacingRightNormalPoseList,
    /// <summary>TransitionTable_02_04_06_08_A5_A7_E1_E3_E5_E7_FacingLeft: diagnostic list identity.</summary>
    FacingLeftNormalPoseList,
    /// <summary>TransitionTable_09_0D_0F_11_MovingRight: diagnostic list identity.</summary>
    MovingRightNormalPoseList,
    /// <summary>TransitionTable_0A_0E_10_12_MovingLeft: diagnostic list identity.</summary>
    MovingLeftNormalPoseList,
    /// <summary>TransitionTable_4B_55_57_59_FacingRight_NormalJumpTransition: diagnostic list identity.</summary>
    NeutralJumpTransitionRightPoseList,
    /// <summary>TransitionTable_4C_56_58_5A_FacingLeft_NormalJumpTransition: diagnostic list identity.</summary>
    NeutralJumpTransitionLeftPoseList,
    /// <summary>TransitionTable_15_4D_51_69_6B_FacingRight_NormalJump: diagnostic list identity.</summary>
    NormalJumpAimUpRightPoseList,
    /// <summary>TransitionTable_16_4E_52_6A_6C_FacingLeft_NormalJump: diagnostic list identity.</summary>
    NormalJumpAimUpLeftPoseList,
    /// <summary>TransitionTable_4F_FacingLeft_DamageBoost: diagnostic list identity.</summary>
    DamageBoostLeftPoseList,
    /// <summary>TransitionTable_50_FacingRight_DamageBoost: diagnostic list identity.</summary>
    DamageBoostRightPoseList,
    /// <summary>TransitionTable_19_FacingRight_SpinJump: diagnostic list identity.</summary>
    SpinJumpRightPoseList,
    /// <summary>TransitionTable_1A_FacingLeft_SpinJump: diagnostic list identity.</summary>
    SpinJumpLeftPoseList,
    /// <summary>TransitionTable_1B_FacingRight_SpaceJump: diagnostic list identity.</summary>
    SpaceJumpRightPoseList,
    /// <summary>TransitionTable_1C_FacingLeft_SpaceJump: diagnostic list identity.</summary>
    SpaceJumpLeftPoseList,
    /// <summary>TransitionTable_81_ScrewAttack: diagnostic list identity.</summary>
    ScrewAttackRightPoseList,
    /// <summary>TransitionTable_82_FacingLeft_ScrewAttack: diagnostic list identity.</summary>
    ScrewAttackLeftPoseList,
    /// <summary>TransitionTable_1D_FaceRight_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    MorphBallGroundRightPoseList,
    /// <summary>TransitionTable_1E_MoveRight_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    MorphBallMovingRightPoseList,
    /// <summary>TransitionTable_1F_MoveLeft_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    MorphBallMovingLeftPoseList,
    /// <summary>TransitionTable_1D_FaceLeft_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    MorphBallGroundLeftPoseList,
    /// <summary>TransitionTable list for <see cref="SamusPoseId.GlitchBallRightPose"/>: diagnostic list identity.</summary>
    GlitchBallRightPoseList,
    /// <summary>TransitionTable list for <see cref="SamusPoseId.GlitchBallLeftPose"/>: diagnostic list identity.</summary>
    GlitchBallLeftPoseList,
    /// <summary>TransitionTable_27_71_73_85_FacingRight_Crouching: diagnostic list identity.</summary>
    CrouchingRightPoseList,
    /// <summary>TransitionTable_28_72_74_86_Crouching: diagnostic list identity.</summary>
    CrouchingLeftPoseList,
    /// <summary>TransitionTable_29_2B_6D_6F_FacingRight_Falling: diagnostic list identity.</summary>
    FallingRightPoseList,
    /// <summary>TransitionTable_2A_2C_6E_70_FacingLeft_Falling: diagnostic list identity.</summary>
    FallingLeftPoseList,
    /// <summary>TransitionTable_31_FacingRight_MorphBall_NoSpringBall_InAir: diagnostic list identity.</summary>
    MorphBallFallingRightPoseList,
    /// <summary>TransitionTable_32_FacingLeft_MorphBall_NoSpringBall_InAir: diagnostic list identity.</summary>
    MorphBallFallingLeftPoseList,
    /// <summary>TransitionTable list for <see cref="SamusPoseId.UnusedKnockbackRightPose"/>: diagnostic list identity.</summary>
    UnusedKnockbackRightPoseList,
    /// <summary>TransitionTable list for <see cref="SamusPoseId.UnusedKnockbackLeftPose"/>: diagnostic list identity.</summary>
    UnusedKnockbackLeftPoseList,
    /// <summary>TransitionTable_49_75_77_FacingLeft_Moonwalk: diagnostic list identity.</summary>
    MoonwalkFacingLeftPoseList,
    /// <summary>TransitionTable_4A_76_78_FacingRight_Moonwalk: diagnostic list identity.</summary>
    MoonwalkFacingRightPoseList,
    /// <summary>TransitionTable_53_FacingRight_Knockback: diagnostic list identity.</summary>
    KnockbackRightPoseList,
    /// <summary>TransitionTable_54_FacingLeft_Knockback: diagnostic list identity.</summary>
    KnockbackLeftPoseList,
    /// <summary>TransitionTable_79_7B_FacingRight_MorphBall_Spring_OnGround: diagnostic list identity.</summary>
    SpringBallGroundRightPoseList,
    /// <summary>TransitionTable_7A_7C_FacingLeft_MorphBall_Spring_OnGround: diagnostic list identity.</summary>
    SpringBallGroundLeftPoseList,
    /// <summary>TransitionTable_7D_FacingRight_MorphBall_SpringBall_Falling: diagnostic list identity.</summary>
    SpringBallFallingRightPoseList,
    /// <summary>TransitionTable_7E_FacingLeft_MorphBall_SpringBall_Falling: diagnostic list identity.</summary>
    SpringBallFallingLeftPoseList,
    /// <summary>TransitionTable_7F_FacingRight_MorphBall_SpringBall_InAir: diagnostic list identity.</summary>
    SpringBallJumpRightPoseList,
    /// <summary>TransitionTable_80_FacingLeft_MorphBall_SpringBall_InAir: diagnostic list identity.</summary>
    SpringBallJumpLeftPoseList,
    /// <summary>UNUSED_TransitionTable_63_91A990: diagnostic list identity.</summary>
    CompactAirborneRightPoseList,
    /// <summary>UNUSED_TransitionTable_64_91A998: diagnostic list identity.</summary>
    CompactAirborneLeftPoseList,
    /// <summary>UNUSED_TransitionTable_65_91A9A0: diagnostic list identity.</summary>
    CompactAirborneFallingRightPoseList,
    /// <summary>UNUSED_TransitionTable_66_91A9C6: diagnostic list identity.</summary>
    CompactAirborneFallingLeftPoseList,
    /// <summary>TransitionTable_83_FacingRight_WallJump: diagnostic list identity.</summary>
    WallJumpRightPoseList,
    /// <summary>TransitionTable_84_FacingLeft_WallJump: diagnostic list identity.</summary>
    WallJumpLeftPoseList,
    /// <summary>TransitionTable_89_CF_D1_FacingRight_RanIntoAWall: diagnostic list identity.</summary>
    RanIntoWallRightPoseList,
    /// <summary>TransitionTable_8A_D0_D2_FacingLeft_RanIntoAWall: diagnostic list identity.</summary>
    RanIntoWallLeftPoseList,
    /// <summary>TransitionTable_13_FaceRight_NormalJump_NotMoving_GunExtend: diagnostic list identity.</summary>
    NormalJumpGunExtendedRightPoseList,
    /// <summary>TransitionTable_14_FacingLeft_NormalJump_NotMoving_GunExtend: diagnostic list identity.</summary>
    NormalJumpGunExtendedLeftPoseList,
    /// <summary>TransitionTable_17_FacingRight_NormalJump_AimingDown: diagnostic list identity.</summary>
    NormalJumpAimDownRightPoseList,
    /// <summary>TransitionTable_18_FacingLeft_NormalJump_AimingDown: diagnostic list identity.</summary>
    NormalJumpAimDownLeftPoseList,
    /// <summary>TransitionTable_3D_FacingRight_Unmorphing: diagnostic list identity.</summary>
    UnmorphingTransitionRightPoseList,
    /// <summary>TransitionTable_3E_FacingLeft_Unmorphing: diagnostic list identity.</summary>
    UnmorphingTransitionLeftPoseList,
    /// <summary>TransitionTable_25_FacingRight_Turning_Standing: diagnostic list identity.</summary>
    TurningRightToLeftPoseList,
    /// <summary>TransitionTable_26_FacingLeft_Turning_Standing: diagnostic list identity.</summary>
    TurningLeftToRightPoseList,
    /// <summary>TransitionTable_8B_FacingRight_Turning_Standing_AimingUp: diagnostic list identity.</summary>
    TurningRightToLeftAimUpPoseList,
    /// <summary>TransitionTable_8C_FacingLeft_Turning_Standing_AimingUp: diagnostic list identity.</summary>
    TurningLeftToRightAimUpPoseList,
    /// <summary>TransitionTable_8D_FacingRight_Turning_Standing_AimDownRight: diagnostic list identity.</summary>
    TurningRightToLeftAimDiagonalDownPoseList,
    /// <summary>TransitionTable_8E_FacingLeft_Turning_Standing_AimDownLeft: diagnostic list identity.</summary>
    TurningLeftToRightAimDiagonalDownPoseList,
    /// <summary>TransitionTable_C7_FacingRight_VerticalShinesparkWindup: diagnostic list identity.</summary>
    ShinesparkWindupRightPoseList,
    /// <summary>TransitionTable_C8_FacingLeft_VerticalShinesparkWindup: diagnostic list identity.</summary>
    ShinesparkWindupLeftPoseList,
    /// <summary>TransitionTable_2D_FacingRight_Falling_AimingDown: diagnostic list identity.</summary>
    FallingAimDownRightPoseList,
    /// <summary>TransitionTable_2E_FacingLeft_Falling_AimingDown: diagnostic list identity.</summary>
    FallingAimDownLeftPoseList,
    /// <summary>UNUSED_TransitionTable_DF_91AE10: diagnostic list identity.</summary>
    DraygonHeldBallLeftPoseList,
    /// <summary>TransitionTable_BA_BB_BC_BD_BE_FacingLeft_GrabbedByDraygon: diagnostic list identity.</summary>
    DraygonGrabbedNeutralLeftPoseList,
    /// <summary>TransitionTable_EC_ED_EE_EF_F0_FacingRight_GrabbedByDraygon: diagnostic list identity.</summary>
    DraygonGrabbedNeutralRightPoseList,
    /// <summary>TransitionTable_0B_MovingRight_GunExtended: diagnostic list identity.</summary>
    MovingRightGunExtendedPoseList,
    /// <summary>TransitionTable_0C_MovingLeft_GunExtended: diagnostic list identity.</summary>
    MovingLeftGunExtendedPoseList,
    /// <summary>TransitionTable_67_FacingRight_Falling_GunExtended: diagnostic list identity.</summary>
    FallingGunExtendedRightPoseList,
    /// <summary>TransitionTable_68_FacingLeft_Falling_GunExtended: diagnostic list identity.</summary>
    FallingGunExtendedLeftPoseList,
    /// <summary>TransitionTable_BF_FacingRight_Moonwalking_TurnJumpLeft: diagnostic list identity.</summary>
    MoonwalkTurnJumpLeftPoseList,
    /// <summary>TransitionTable_C0_FacingLeft_Moonwalking_TurnJumpRight: diagnostic list identity.</summary>
    MoonwalkTurnJumpRightPoseList,
    /// <summary>TransitionTable_C1_FaceRight_Moonwalk_TurnJumpLeft_AimUpRight: diagnostic list identity.</summary>
    MoonwalkTurnJumpAimUpLeftPoseList,
    /// <summary>TransitionTable_C2_FaceLeft_Moonwalk_TurnJumpRight_AimUpLeft: diagnostic list identity.</summary>
    MoonwalkTurnJumpAimUpRightPoseList,
    /// <summary>TransitionTable_C3_FaceRight_Moonwalk_TurnJumpLeft_AimDownRight: diagnostic list identity.</summary>
    MoonwalkTurnJumpAimDownLeftPoseList,
    /// <summary>TransitionTable_C4_FaceLeft_Moonwalk_TurnJumpRight_AimDownLeft: diagnostic list identity.</summary>
    MoonwalkTurnJumpAimDownRightPoseList,
}
