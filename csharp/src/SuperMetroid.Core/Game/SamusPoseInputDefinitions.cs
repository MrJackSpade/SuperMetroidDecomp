namespace SuperMetroid.Core.Game;

/// <summary>Canonical action/direction bits documented by bank91 TransitionTable; combinations are required chords.</summary>
[Flags]
internal enum CanonicalPoseButtons : ushort
{
    /// <summary>$91:9EE2 TransitionTable input bits: no required buttons.</summary>
    None = 0,
    /// <summary>$91:9EE2 TransitionTable input bits: canonical aim diagonally up.</summary>
    AimUp = 0x0010,
    /// <summary>$91:9EE2 TransitionTable input bits: canonical aim diagonally down.</summary>
    AimDown = 0x0020,
    /// <summary>$91:9EE2 TransitionTable input bits: canonical Shoot.</summary>
    Shoot = 0x0040,
    /// <summary>$91:9EE2 TransitionTable input bits: canonical Jump.</summary>
    Jump = 0x0080,
    /// <summary>$91:9EE2 TransitionTable input bits: Right direction.</summary>
    Right = 0x0100,
    /// <summary>$91:9EE2 TransitionTable input bits: Left direction.</summary>
    Left = 0x0200,
    /// <summary>$91:9EE2 TransitionTable input bits: Down direction.</summary>
    Down = 0x0400,
    /// <summary>$91:9EE2 TransitionTable input bits: Up direction.</summary>
    Up = 0x0800,
}

/// <summary>The selected native condition, retained for transition diagnostics.</summary>
/// <param name="TargetPose">Pose selected by the matching transition condition.</param>
internal readonly record struct SamusPoseInputRule(ushort TargetPose);

/// <summary>A decision result, including the distinction between an empty list and exhausted conditions.</summary>
/// <param name="Rule">Matched target pose, or null when no condition selected a transition.</param>
/// <param name="HasConditions">True when the source list contains conditions, even if none matched the supplied inputs.</param>
internal readonly record struct SamusPoseInputMatch(SamusPoseInputRule? Rule, bool HasConditions);

/// <summary>Native pose-to-input dispatch and selected-condition diagnostics; no stored transition rows.</summary>
internal static class SamusPoseInputDefinitions
{
    /// <summary>$91:A90C: first list in the later native graph section.</summary>
    private const ushort LaterListsBegin = 0xa90c;
    /// <summary>Pose $45, UNUSED_TransitionTable_45_91A7CC: preserved unused transition target.</summary>
    internal const SamusPoseId UnusedPose45 = (SamusPoseId)0x45;
    /// <summary>Pose $46, UNUSED_TransitionTable_46_91A7E0: preserved unused transition target.</summary>
    internal const SamusPoseId UnusedPose46 = (SamusPoseId)0x46;
    /// <summary>$91:A0DC TransitionTable list for pose $2F: diagnostic list identity.</summary>
    internal const ushort EmptyTransitionList = 0xa0dc;
    /// <summary>$91:A0DE TransitionTable_00_9B_FacingForward: diagnostic list identity.</summary>
    internal const ushort ForwardFacingPowerSuitPoseList = 0xa0de;
    /// <summary>$91:A0EC TransitionTable_01_03_05_07_A4_A6_E0_E2_E4_E6_FacingRight: diagnostic list identity.</summary>
    internal const ushort FacingRightNormalPoseList = 0xa0ec;
    /// <summary>$91:A172 TransitionTable_02_04_06_08_A5_A7_E1_E3_E5_E7_FacingLeft: diagnostic list identity.</summary>
    internal const ushort FacingLeftNormalPoseList = 0xa172;
    /// <summary>$91:A1F8 TransitionTable_09_0D_0F_11_MovingRight: diagnostic list identity.</summary>
    internal const ushort MovingRightNormalPoseList = 0xa1f8;
    /// <summary>$91:A242 TransitionTable_0A_0E_10_12_MovingLeft: diagnostic list identity.</summary>
    internal const ushort MovingLeftNormalPoseList = 0xa242;
    /// <summary>$91:A28C TransitionTable_4B_55_57_59_FacingRight_NormalJumpTransition: diagnostic list identity.</summary>
    internal const ushort NeutralJumpTransitionRightPoseList = 0xa28c;
    /// <summary>$91:A2BE TransitionTable_4C_56_58_5A_FacingLeft_NormalJumpTransition: diagnostic list identity.</summary>
    internal const ushort NeutralJumpTransitionLeftPoseList = 0xa2be;
    /// <summary>$91:A2F6 TransitionTable_15_4D_51_69_6B_FacingRight_NormalJump: diagnostic list identity.</summary>
    internal const ushort NormalJumpAimUpRightPoseList = 0xa2f6;
    /// <summary>$91:A376 TransitionTable_16_4E_52_6A_6C_FacingLeft_NormalJump: diagnostic list identity.</summary>
    internal const ushort NormalJumpAimUpLeftPoseList = 0xa376;
    /// <summary>$91:A3F6 TransitionTable_4F_FacingLeft_DamageBoost: diagnostic list identity.</summary>
    internal const ushort DamageBoostLeftPoseList = 0xa3f6;
    /// <summary>$91:A40A TransitionTable_50_FacingRight_DamageBoost: diagnostic list identity.</summary>
    internal const ushort DamageBoostRightPoseList = 0xa40a;
    /// <summary>$91:A41E TransitionTable_19_FacingRight_SpinJump: diagnostic list identity.</summary>
    internal const ushort SpinJumpRightPoseList = 0xa41e;
    /// <summary>$91:A46E TransitionTable_1A_FacingLeft_SpinJump: diagnostic list identity.</summary>
    internal const ushort SpinJumpLeftPoseList = 0xa46e;
    /// <summary>$91:A4BE TransitionTable_1B_FacingRight_SpaceJump: diagnostic list identity.</summary>
    internal const ushort SpaceJumpRightPoseList = 0xa4be;
    /// <summary>$91:A50E TransitionTable_1C_FacingLeft_SpaceJump: diagnostic list identity.</summary>
    internal const ushort SpaceJumpLeftPoseList = 0xa50e;
    /// <summary>$91:A55E TransitionTable_81_ScrewAttack: diagnostic list identity.</summary>
    internal const ushort ScrewAttackRightPoseList = 0xa55e;
    /// <summary>$91:A5AE TransitionTable_82_FacingLeft_ScrewAttack: diagnostic list identity.</summary>
    internal const ushort ScrewAttackLeftPoseList = 0xa5ae;
    /// <summary>$91:A5FE TransitionTable_1D_FaceRight_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    internal const ushort MorphBallGroundRightPoseList = 0xa5fe;
    /// <summary>$91:A618 TransitionTable_1E_MoveRight_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    internal const ushort MorphBallMovingRightPoseList = 0xa618;
    /// <summary>$91:A632 TransitionTable_1F_MoveLeft_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    internal const ushort MorphBallMovingLeftPoseList = 0xa632;
    /// <summary>$91:A64C TransitionTable_1D_FaceLeft_MorphBall_NoSpringBall_OnGround: diagnostic list identity.</summary>
    internal const ushort MorphBallGroundLeftPoseList = 0xa64c;
    /// <summary>$91:A666 TransitionTable list for pose $20: diagnostic list identity.</summary>
    internal const ushort UnusedPose20List = 0xa666;
    /// <summary>$91:A668 TransitionTable list for pose $23: diagnostic list identity.</summary>
    internal const ushort UnusedPose23List = 0xa668;
    /// <summary>$91:A66A TransitionTable list for pose $42: diagnostic list identity.</summary>
    internal const ushort UnusedPose42List = 0xa66a;
    /// <summary>$91:A66C TransitionTable_27_71_73_85_FacingRight_Crouching: diagnostic list identity.</summary>
    internal const ushort CrouchingRightPoseList = 0xa66c;
    /// <summary>$91:A6BC TransitionTable_28_72_74_86_Crouching: diagnostic list identity.</summary>
    internal const ushort CrouchingLeftPoseList = 0xa6bc;
    /// <summary>$91:A70C TransitionTable_29_2B_6D_6F_FacingRight_Falling: diagnostic list identity.</summary>
    internal const ushort FallingRightPoseList = 0xa70c;
    /// <summary>$91:A750 TransitionTable_2A_2C_6E_70_FacingLeft_Falling: diagnostic list identity.</summary>
    internal const ushort FallingLeftPoseList = 0xa750;
    /// <summary>$91:A794 TransitionTable_31_FacingRight_MorphBall_NoSpringBall_InAir: diagnostic list identity.</summary>
    internal const ushort MorphBallFallingRightPoseList = 0xa794;
    /// <summary>$91:A7AE TransitionTable_32_FacingLeft_MorphBall_NoSpringBall_InAir: diagnostic list identity.</summary>
    internal const ushort MorphBallFallingLeftPoseList = 0xa7ae;
    /// <summary>$91:A7C8 TransitionTable list for pose $33: diagnostic list identity.</summary>
    internal const ushort UnusedKnockbackRightPoseList = 0xa7c8;
    /// <summary>$91:A7CA TransitionTable list for pose $34: diagnostic list identity.</summary>
    internal const ushort UnusedKnockbackLeftPoseList = 0xa7ca;
    /// <summary>$91:A7CC UNUSED_TransitionTable_45_91A7CC: diagnostic list identity.</summary>
    internal const ushort UnusedPose45List = 0xa7cc;
    /// <summary>$91:A7E0 UNUSED_TransitionTable_46_91A7E0: diagnostic list identity.</summary>
    internal const ushort UnusedPose46List = 0xa7e0;
    /// <summary>$91:A7F4 TransitionTable list for pose $47: diagnostic list identity.</summary>
    internal const ushort UnusedPose47List = 0xa7f4;
    /// <summary>$91:A834 TransitionTable list for pose $48: diagnostic list identity.</summary>
    internal const ushort UnusedPose48List = 0xa834;
    /// <summary>$91:A874 TransitionTable_49_75_77_FacingLeft_Moonwalk: diagnostic list identity.</summary>
    internal const ushort MoonwalkFacingLeftPoseList = 0xa874;
    /// <summary>$91:A8AC TransitionTable_4A_76_78_FacingRight_Moonwalk: diagnostic list identity.</summary>
    internal const ushort MoonwalkFacingRightPoseList = 0xa8ac;
    /// <summary>$91:A8E4 TransitionTable_53_FacingRight_Knockback: diagnostic list identity.</summary>
    internal const ushort KnockbackRightPoseList = 0xa8e4;
    /// <summary>$91:A8EC TransitionTable_54_FacingLeft_Knockback: diagnostic list identity.</summary>
    internal const ushort KnockbackLeftPoseList = 0xa8ec;
    /// <summary>$91:A8FC UNUSED_TransitionTable_5B_91A8FC: diagnostic list identity.</summary>
    internal const ushort UnusedPose5BList = 0xa8fc;
    /// <summary>$91:A904 UNUSED_TransitionTable_5C_91A904: diagnostic list identity.</summary>
    internal const ushort UnusedPose5CList = 0xa904;
    /// <summary>$91:A90C TransitionTable_79_7B_FacingRight_MorphBall_Spring_OnGround: diagnostic list identity.</summary>
    internal const ushort SpringBallGroundRightPoseList = 0xa90c;
    /// <summary>$91:A926 TransitionTable_7A_7C_FacingLeft_MorphBall_Spring_OnGround: diagnostic list identity.</summary>
    internal const ushort SpringBallGroundLeftPoseList = 0xa926;
    /// <summary>$91:A940 TransitionTable_7D_FacingRight_MorphBall_SpringBall_Falling: diagnostic list identity.</summary>
    internal const ushort SpringBallFallingRightPoseList = 0xa940;
    /// <summary>$91:A954 TransitionTable_7E_FacingLeft_MorphBall_SpringBall_Falling: diagnostic list identity.</summary>
    internal const ushort SpringBallFallingLeftPoseList = 0xa954;
    /// <summary>$91:A968 TransitionTable_7F_FacingRight_MorphBall_SpringBall_InAir: diagnostic list identity.</summary>
    internal const ushort SpringBallJumpRightPoseList = 0xa968;
    /// <summary>$91:A97C TransitionTable_80_FacingLeft_MorphBall_SpringBall_InAir: diagnostic list identity.</summary>
    internal const ushort SpringBallJumpLeftPoseList = 0xa97c;
    /// <summary>$91:A990 UNUSED_TransitionTable_63_91A990: diagnostic list identity.</summary>
    internal const ushort UnusedPose63List = 0xa990;
    /// <summary>$91:A998 UNUSED_TransitionTable_64_91A998: diagnostic list identity.</summary>
    internal const ushort UnusedPose64List = 0xa998;
    /// <summary>$91:A9A0 UNUSED_TransitionTable_65_91A9A0: diagnostic list identity.</summary>
    internal const ushort UnusedPose65List = 0xa9a0;
    /// <summary>$91:A9C6 UNUSED_TransitionTable_66_91A9C6: diagnostic list identity.</summary>
    internal const ushort UnusedPose66List = 0xa9c6;
    /// <summary>$91:A9EC TransitionTable_83_FacingRight_WallJump: diagnostic list identity.</summary>
    internal const ushort WallJumpRightPoseList = 0xa9ec;
    /// <summary>$91:AA12 TransitionTable_84_FacingLeft_WallJump: diagnostic list identity.</summary>
    internal const ushort WallJumpLeftPoseList = 0xaa12;
    /// <summary>$91:AA38 TransitionTable_89_CF_D1_FacingRight_RanIntoAWall: diagnostic list identity.</summary>
    internal const ushort RanIntoWallRightPoseList = 0xaa38;
    /// <summary>$91:AA7C TransitionTable_8A_D0_D2_FacingLeft_RanIntoAWall: diagnostic list identity.</summary>
    internal const ushort RanIntoWallLeftPoseList = 0xaa7c;
    /// <summary>$91:AAC0 TransitionTable_13_FaceRight_NormalJump_NotMoving_GunExtend: diagnostic list identity.</summary>
    internal const ushort NormalJumpGunExtendedRightPoseList = 0xaac0;
    /// <summary>$91:AB3A TransitionTable_14_FacingLeft_NormalJump_NotMoving_GunExtend: diagnostic list identity.</summary>
    internal const ushort NormalJumpGunExtendedLeftPoseList = 0xab3a;
    /// <summary>$91:ABB4 TransitionTable_17_FacingRight_NormalJump_AimingDown: diagnostic list identity.</summary>
    internal const ushort NormalJumpAimDownRightPoseList = 0xabb4;
    /// <summary>$91:AC40 TransitionTable_18_FacingLeft_NormalJump_AimingDown: diagnostic list identity.</summary>
    internal const ushort NormalJumpAimDownLeftPoseList = 0xac40;
    /// <summary>$91:ACCC TransitionTable_3D_FacingRight_Unmorphing: diagnostic list identity.</summary>
    internal const ushort UnmorphingTransitionRightPoseList = 0xaccc;
    /// <summary>$91:ACE0 TransitionTable_3E_FacingLeft_Unmorphing: diagnostic list identity.</summary>
    internal const ushort UnmorphingTransitionLeftPoseList = 0xace0;
    /// <summary>$91:ACF4 TransitionTable_25_FacingRight_Turning_Standing: diagnostic list identity.</summary>
    internal const ushort TurningRightToLeftPoseList = 0xacf4;
    /// <summary>$91:AD08 TransitionTable_26_FacingLeft_Turning_Standing: diagnostic list identity.</summary>
    internal const ushort TurningLeftToRightPoseList = 0xad08;
    /// <summary>$91:AD1C TransitionTable_8B_FacingRight_Turning_Standing_AimingUp: diagnostic list identity.</summary>
    internal const ushort TurningRightToLeftAimUpPoseList = 0xad1c;
    /// <summary>$91:AD30 TransitionTable_8C_FacingLeft_Turning_Standing_AimingUp: diagnostic list identity.</summary>
    internal const ushort TurningLeftToRightAimUpPoseList = 0xad30;
    /// <summary>$91:AD44 TransitionTable_8D_FacingRight_Turning_Standing_AimDownRight: diagnostic list identity.</summary>
    internal const ushort TurningRightToLeftAimDiagonalDownPoseList = 0xad44;
    /// <summary>$91:AD58 TransitionTable_8E_FacingLeft_Turning_Standing_AimDownLeft: diagnostic list identity.</summary>
    internal const ushort TurningLeftToRightAimDiagonalDownPoseList = 0xad58;
    /// <summary>$91:AD6C TransitionTable_C7_FacingRight_VerticalShinesparkWindup: diagnostic list identity.</summary>
    internal const ushort ShinesparkWindupRightPoseList = 0xad6c;
    /// <summary>$91:AD80 TransitionTable_C8_FacingLeft_VerticalShinesparkWindup: diagnostic list identity.</summary>
    internal const ushort ShinesparkWindupLeftPoseList = 0xad80;
    /// <summary>$91:AD94 TransitionTable_2D_FacingRight_Falling_AimingDown: diagnostic list identity.</summary>
    internal const ushort FallingAimDownRightPoseList = 0xad94;
    /// <summary>$91:ADD2 TransitionTable_2E_FacingLeft_Falling_AimingDown: diagnostic list identity.</summary>
    internal const ushort FallingAimDownLeftPoseList = 0xadd2;
    /// <summary>$91:AE10 UNUSED_TransitionTable_DF_91AE10: diagnostic list identity.</summary>
    internal const ushort UnusedPoseDfList = 0xae10;
    /// <summary>$91:AE18 TransitionTable_BA_BB_BC_BD_BE_FacingLeft_GrabbedByDraygon: diagnostic list identity.</summary>
    internal const ushort DraygonGrabbedNeutralLeftPoseList = 0xae18;
    /// <summary>$91:AE56 TransitionTable_EC_ED_EE_EF_F0_FacingRight_GrabbedByDraygon: diagnostic list identity.</summary>
    internal const ushort DraygonGrabbedNeutralRightPoseList = 0xae56;
    /// <summary>$91:AE94 TransitionTable_0B_MovingRight_GunExtended: diagnostic list identity.</summary>
    internal const ushort MovingRightGunExtendedPoseList = 0xae94;
    /// <summary>$91:AEDE TransitionTable_0C_MovingLeft_GunExtended: diagnostic list identity.</summary>
    internal const ushort MovingLeftGunExtendedPoseList = 0xaede;
    /// <summary>$91:AF28 TransitionTable_67_FacingRight_Falling_GunExtended: diagnostic list identity.</summary>
    internal const ushort FallingGunExtendedRightPoseList = 0xaf28;
    /// <summary>$91:AF60 TransitionTable_68_FacingLeft_Falling_GunExtended: diagnostic list identity.</summary>
    internal const ushort FallingGunExtendedLeftPoseList = 0xaf60;
    /// <summary>$91:AF98 TransitionTable_BF_FacingRight_Moonwalking_TurnJumpLeft: diagnostic list identity.</summary>
    internal const ushort MoonwalkTurnJumpLeftPoseList = 0xaf98;
    /// <summary>$91:AFAC TransitionTable_C0_FacingLeft_Moonwalking_TurnJumpRight: diagnostic list identity.</summary>
    internal const ushort MoonwalkTurnJumpRightPoseList = 0xafac;
    /// <summary>$91:AFC0 TransitionTable_C1_FaceRight_Moonwalk_TurnJumpLeft_AimUpRight: diagnostic list identity.</summary>
    internal const ushort MoonwalkTurnJumpAimUpLeftPoseList = 0xafc0;
    /// <summary>$91:AFD4 TransitionTable_C2_FaceLeft_Moonwalk_TurnJumpRight_AimUpLeft: diagnostic list identity.</summary>
    internal const ushort MoonwalkTurnJumpAimUpRightPoseList = 0xafd4;
    /// <summary>$91:AFE8 TransitionTable_C3_FaceRight_Moonwalk_TurnJumpLeft_AimDownRight: diagnostic list identity.</summary>
    internal const ushort MoonwalkTurnJumpAimDownLeftPoseList = 0xafe8;
    /// <summary>$91:AFFC TransitionTable_C4_FaceLeft_Moonwalk_TurnJumpRight_AimDownLeft: diagnostic list identity.</summary>
    internal const ushort MoonwalkTurnJumpAimDownRightPoseList = 0xaffc;

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

    /// <summary>Resolves a native pose identifier to its transition-list address when that pose has a compiled list.</summary>
    /// <param name="pose">Native pose byte whose input-transition list is requested.</param>
    /// <param name="pointer">Receives the list address on success, or zero when no list is compiled for the pose.</param>
    /// <returns><see langword="true"/> when the pose maps to a transition-list address.</returns>
    internal static bool TryGetPointer(byte pose, out ushort pointer)
    {
        pointer = (SamusPoseId)pose switch
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
            _ => 0,
        };
        return pointer != 0;
    }

    /// <summary>Evaluates the conditions for a transition list using the matching early- or late-table rules.</summary>
    /// <param name="pointer">Native transition-list address, used to select the rule table.</param>
    /// <param name="held">Canonical buttons currently held.</param>
    /// <param name="newlyPressed">Canonical buttons pressed on this update.</param>
    /// <returns>The selected target and whether the list contained any conditions.</returns>
    internal static SamusPoseInputMatch Match(ushort pointer, ushort held, ushort newlyPressed) =>
        pointer < LaterListsBegin
            ? SamusPoseInputRulesEarly.Match(pointer, held, newlyPressed)
            : SamusPoseInputRulesLate.Match(pointer, held, newlyPressed);

    /// <summary>Tests whether an input word contains every button required by a transition condition.</summary>
    /// <param name="input">Canonical held or newly pressed buttons to test.</param>
    /// <param name="required">Required button combination from the native transition table.</param>
    /// <returns><see langword="true"/> when all required bits are present in <paramref name="input"/>.</returns>
    internal static bool Has(ushort input, CanonicalPoseButtons required) =>
        (input & (ushort)required) == (ushort)required;

    /// <summary>Constructs a match result for a satisfied transition condition.</summary>
    /// <param name="index">Condition ordinal, retained for call-site parity with table evaluation.</param>
    /// <param name="newlyPressed">Buttons newly pressed while evaluating the condition.</param>
    /// <param name="held">Buttons held while evaluating the condition.</param>
    /// <param name="target">Pose selected by the satisfied condition.</param>
    /// <returns>A result containing the target pose and indicating that the list has conditions.</returns>
    internal static SamusPoseInputMatch Accept(int index, CanonicalPoseButtons newlyPressed,
        CanonicalPoseButtons held, SamusPoseId target) =>
        new(new((ushort)target), HasConditions: true);
}
