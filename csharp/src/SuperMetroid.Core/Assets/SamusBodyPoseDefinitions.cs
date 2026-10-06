using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Semantic real-pose selection of native body frame-list identities; selected frame payloads remain separately required.</summary>
internal static class SamusBodyPoseDefinitions
{
    /// <summary>$92:EA24, SamusTilesAnimation_AnimationDefinitions_EA24; selected by ForwardFacingPowerSuitPose and the semantic aliases below.</summary>
    private const ushort ForwardFacingPowerSuitFrames = 0xea24;

    /// <summary>$92:DB48, SamusTilesAnimation_AnimationDefinitions_DB48; selected by FacingRightNormalPose and the semantic aliases below.</summary>
    private const ushort FacingRightNormalFrames = 0xdb48;

    /// <summary>$92:DB6C, SamusTilesAnimation_AnimationDefinitions_DB6C; selected by FacingLeftNormalPose and the semantic aliases below.</summary>
    private const ushort FacingLeftNormalFrames = 0xdb6c;

    /// <summary>$92:E018, SamusTilesAnimation_AnimationDefinitions_E018; selected by StandingAimUpRightPose and the semantic aliases below.</summary>
    private const ushort StandingAimUpRightFrames = 0xe018;

    /// <summary>$92:E020, SamusTilesAnimation_AnimationDefinitions_E020; selected by StandingAimUpLeftPose and the semantic aliases below.</summary>
    private const ushort StandingAimUpLeftFrames = 0xe020;

    /// <summary>$92:E028, SamusTilesAnimation_AnimationDefinitions_E028; selected by StandingAimDiagonalUpRightPose and the semantic aliases below.</summary>
    private const ushort StandingAimDiagonalUpRightFrames = 0xe028;

    /// <summary>$92:E02C, SamusTilesAnimation_AnimationDefinitions_E02C; selected by StandingAimDiagonalUpLeftPose and the semantic aliases below.</summary>
    private const ushort StandingAimDiagonalUpLeftFrames = 0xe02c;

    /// <summary>$92:E030, SamusTilesAnimation_AnimationDefinitions_E030; selected by StandingAimDiagonalDownRightPose and the semantic aliases below.</summary>
    private const ushort StandingAimDiagonalDownRightFrames = 0xe030;

    /// <summary>$92:E034, SamusTilesAnimation_AnimationDefinitions_E034; selected by StandingAimDiagonalDownLeftPose and the semantic aliases below.</summary>
    private const ushort StandingAimDiagonalDownLeftFrames = 0xe034;

    /// <summary>$92:DC48, SamusTilesAnimation_AnimationDefinitions_DC48; selected by MovingRightNormalPose and the semantic aliases below.</summary>
    private const ushort MovingRightNormalFrames = 0xdc48;

    /// <summary>$92:DC70, SamusTilesAnimation_AnimationDefinitions_DC70; selected by MovingLeftNormalPose and the semantic aliases below.</summary>
    private const ushort MovingLeftNormalFrames = 0xdc70;

    /// <summary>$92:DC98, SamusTilesAnimation_AnimationDefinitions_DC98; selected by MovingRightGunExtendedPose and the semantic aliases below.</summary>
    private const ushort MovingRightGunExtendedFrames = 0xdc98;

    /// <summary>$92:DCC0, SamusTilesAnimation_AnimationDefinitions_DCC0; selected by MovingLeftGunExtendedPose and the semantic aliases below.</summary>
    private const ushort MovingLeftGunExtendedFrames = 0xdcc0;

    /// <summary>$92:DF28, SamusTilesAnimation_AnimationDefinitions_DF28; selected by RunningAimUpRightPose and the semantic aliases below.</summary>
    private const ushort RunningAimUpRightFrames = 0xdf28;

    /// <summary>$92:DF50, SamusTilesAnimation_AnimationDefinitions_DF50; selected by RunningAimUpLeftPose and the semantic aliases below.</summary>
    private const ushort RunningAimUpLeftFrames = 0xdf50;

    /// <summary>$92:DF78, SamusTilesAnimation_AnimationDefinitions_DF78; selected by RunningAimDiagonalUpRightPose and the semantic aliases below.</summary>
    private const ushort RunningAimDiagonalUpRightFrames = 0xdf78;

    /// <summary>$92:DFA0, SamusTilesAnimation_AnimationDefinitions_DFA0; selected by RunningAimDiagonalUpLeftPose and the semantic aliases below.</summary>
    private const ushort RunningAimDiagonalUpLeftFrames = 0xdfa0;

    /// <summary>$92:DFC8, SamusTilesAnimation_AnimationDefinitions_DFC8; selected by RunningAimDiagonalDownRightPose and the semantic aliases below.</summary>
    private const ushort RunningAimDiagonalDownRightFrames = 0xdfc8;

    /// <summary>$92:DFF0, SamusTilesAnimation_AnimationDefinitions_DFF0; selected by RunningAimDiagonalDownLeftPose and the semantic aliases below.</summary>
    private const ushort RunningAimDiagonalDownLeftFrames = 0xdff0;

    /// <summary>$92:DD28, SamusTilesAnimation_AnimationDefinitions_DD28; selected by NormalJumpGunExtendedRightPose and the semantic aliases below.</summary>
    private const ushort NormalJumpGunExtendedRightFrames = 0xdd28;

    /// <summary>$92:DD30, SamusTilesAnimation_AnimationDefinitions_DD30; selected by NormalJumpGunExtendedLeftPose and the semantic aliases below.</summary>
    private const ushort NormalJumpGunExtendedLeftFrames = 0xdd30;

    /// <summary>$92:DD38, SamusTilesAnimation_AnimationDefinitions_DD38; selected by NormalJumpAimUpRightPose and the semantic aliases below.</summary>
    private const ushort NormalJumpAimUpRightFrames = 0xdd38;

    /// <summary>$92:DD40, SamusTilesAnimation_AnimationDefinitions_DD40; selected by NormalJumpAimUpLeftPose and the semantic aliases below.</summary>
    private const ushort NormalJumpAimUpLeftFrames = 0xdd40;

    /// <summary>$92:DD18, SamusTilesAnimation_AnimationDefinitions_DD18; selected by NormalJumpAimDownRightPose and the semantic aliases below.</summary>
    private const ushort NormalJumpAimDownRightFrames = 0xdd18;

    /// <summary>$92:DD20, SamusTilesAnimation_AnimationDefinitions_DD20; selected by NormalJumpAimDownLeftPose and the semantic aliases below.</summary>
    private const ushort NormalJumpAimDownLeftFrames = 0xdd20;

    /// <summary>$92:E5F8, SamusTilesAnimation_AnimationDefinitions_E5F8; selected by SpinJumpRightPose and the semantic aliases below.</summary>
    private const ushort SpinJumpRightFrames = 0xe5f8;

    /// <summary>$92:E628, SamusTilesAnimation_AnimationDefinitions_E628; selected by SpinJumpLeftPose and the semantic aliases below.</summary>
    private const ushort SpinJumpLeftFrames = 0xe628;

    /// <summary>$92:E658, SamusTilesAnimation_AnimationDefinitions_E658; selected by SpaceJumpRightPose and the semantic aliases below.</summary>
    private const ushort SpaceJumpRightFrames = 0xe658;

    /// <summary>$92:E688, SamusTilesAnimation_AnimationDefinitions_E688; selected by SpaceJumpLeftPose and the semantic aliases below.</summary>
    private const ushort SpaceJumpLeftFrames = 0xe688;

    /// <summary>$92:E508, SamusTilesAnimation_AnimationDefinitions_E508; selected by MorphBallGroundRightPose and the semantic aliases below.</summary>
    private const ushort MorphBallGroundRightFrames = 0xe508;

    /// <summary>$92:E558, SamusTilesAnimation_AnimationDefinitions_E558; selected by MorphBallMovingRightPose and the semantic aliases below.</summary>
    private const ushort MorphBallMovingRightFrames = 0xe558;

    /// <summary>$92:E580, SamusTilesAnimation_AnimationDefinitions_E580; selected by MorphBallMovingLeftPose and the semantic aliases below.</summary>
    private const ushort MorphBallMovingLeftFrames = 0xe580;

    /// <summary>$92:E798, SamusTilesAnimation_AnimationDefinitions_E798; selected by TurningRightToLeftPose and the semantic aliases below.</summary>
    private const ushort TurningRightToLeftFrames = 0xe798;

    /// <summary>$92:E7A4, SamusTilesAnimation_AnimationDefinitions_E7A4; selected by TurningLeftToRightPose and the semantic aliases below.</summary>
    private const ushort TurningLeftToRightFrames = 0xe7a4;

    /// <summary>$92:DE18, SamusTilesAnimation_AnimationDefinitions_DE18; selected by CrouchingRightPose and the semantic aliases below.</summary>
    private const ushort CrouchingRightFrames = 0xde18;

    /// <summary>$92:DE3C, SamusTilesAnimation_AnimationDefinitions_DE3C; selected by CrouchingLeftPose and the semantic aliases below.</summary>
    private const ushort CrouchingLeftFrames = 0xde3c;

    /// <summary>$92:DE60, SamusTilesAnimation_AnimationDefinitions_DE60; selected by FallingRightPose and the semantic aliases below.</summary>
    private const ushort FallingRightFrames = 0xde60;

    /// <summary>$92:DE7C, SamusTilesAnimation_AnimationDefinitions_DE7C; selected by FallingLeftPose and the semantic aliases below.</summary>
    private const ushort FallingLeftFrames = 0xde7c;

    /// <summary>$92:DE98, SamusTilesAnimation_AnimationDefinitions_DE98; selected by FallingAimUpRightPose and the semantic aliases below.</summary>
    private const ushort FallingAimUpRightFrames = 0xde98;

    /// <summary>$92:DEA4, SamusTilesAnimation_AnimationDefinitions_DEA4; selected by FallingAimUpLeftPose and the semantic aliases below.</summary>
    private const ushort FallingAimUpLeftFrames = 0xdea4;

    /// <summary>$92:DEB0, SamusTilesAnimation_AnimationDefinitions_DEB0; selected by FallingAimDownRightPose and the semantic aliases below.</summary>
    private const ushort FallingAimDownRightFrames = 0xdeb0;

    /// <summary>$92:DEB8, SamusTilesAnimation_AnimationDefinitions_DEB8; selected by FallingAimDownLeftPose and the semantic aliases below.</summary>
    private const ushort FallingAimDownLeftFrames = 0xdeb8;

    /// <summary>$92:E7E0, SamusTilesAnimation_AnimationDefinitions_E7E0; selected by TurningRightToLeftJumpPose and the semantic aliases below.</summary>
    private const ushort TurningRightToLeftJumpFrames = 0xe7e0;

    /// <summary>$92:E7EC, SamusTilesAnimation_AnimationDefinitions_E7EC; selected by TurningLeftToRightJumpPose and the semantic aliases below.</summary>
    private const ushort TurningLeftToRightJumpFrames = 0xe7ec;

    /// <summary>$92:E4B0, SamusTilesAnimation_AnimationDefinitions_E4B0; selected by CrouchingTransitionRightPose and the semantic aliases below.</summary>
    private const ushort CrouchingTransitionRightFrames = 0xe4b0;

    /// <summary>$92:E4B4, SamusTilesAnimation_AnimationDefinitions_E4B4; selected by CrouchingTransitionLeftPose and the semantic aliases below.</summary>
    private const ushort CrouchingTransitionLeftFrames = 0xe4b4;

    /// <summary>$92:E4B8, SamusTilesAnimation_AnimationDefinitions_E4B8; selected by MorphingTransitionRightPose and the semantic aliases below.</summary>
    private const ushort MorphingTransitionRightFrames = 0xe4b8;

    /// <summary>$92:E4C0, SamusTilesAnimation_AnimationDefinitions_E4C0; selected by MorphingTransitionLeftPose and the semantic aliases below.</summary>
    private const ushort MorphingTransitionLeftFrames = 0xe4c0;

    /// <summary>$92:E4C8, SamusTilesAnimation_AnimationDefinitions_E4C8; selected by UnmorphingTransitionRightPose and the semantic aliases below.</summary>
    private const ushort UnmorphingTransitionRightFrames = 0xe4c8;

    /// <summary>$92:E4D0, SamusTilesAnimation_AnimationDefinitions_E4D0; selected by UnmorphingTransitionLeftPose and the semantic aliases below.</summary>
    private const ushort UnmorphingTransitionLeftFrames = 0xe4d0;

    /// <summary>$92:E530, SamusTilesAnimation_AnimationDefinitions_E530; selected by MorphBallGroundLeftPose and the semantic aliases below.</summary>
    private const ushort MorphBallGroundLeftFrames = 0xe530;

    /// <summary>$92:E048, SamusTilesAnimation_AnimationDefinitions_E048; selected by UnusedPose45 and the semantic aliases below.</summary>
    private const ushort UnusedPose45Frames = 0xe048;

    /// <summary>$92:DCE8, SamusTilesAnimation_AnimationDefinitions_DCE8; selected by MoonwalkFacingLeftPose and the semantic aliases below.</summary>
    private const ushort MoonwalkFacingLeftFrames = 0xdce8;

    /// <summary>$92:DD00, SamusTilesAnimation_AnimationDefinitions_DD00; selected by MoonwalkFacingRightPose and the semantic aliases below.</summary>
    private const ushort MoonwalkFacingRightFrames = 0xdd00;

    /// <summary>$92:DD78, SamusTilesAnimation_AnimationDefinitions_DD78; selected by NeutralJumpTransitionRightPose and the semantic aliases below.</summary>
    private const ushort NeutralJumpTransitionRightFrames = 0xdd78;

    /// <summary>$92:DD7C, SamusTilesAnimation_AnimationDefinitions_DD7C; selected by NeutralJumpTransitionLeftPose and the semantic aliases below.</summary>
    private const ushort NeutralJumpTransitionLeftFrames = 0xdd7c;

    /// <summary>$92:DD98, SamusTilesAnimation_AnimationDefinitions_DD98; selected by NeutralJumpRightPose and the semantic aliases below.</summary>
    private const ushort NeutralJumpRightFrames = 0xdd98;

    /// <summary>$92:DDB0, SamusTilesAnimation_AnimationDefinitions_DDB0; selected by NeutralJumpLeftPose and the semantic aliases below.</summary>
    private const ushort NeutralJumpLeftFrames = 0xddb0;

    /// <summary>$92:DDC8, SamusTilesAnimation_AnimationDefinitions_DDC8; selected by DamageBoostLeftPose and the semantic aliases below.</summary>
    private const ushort DamageBoostLeftFrames = 0xddc8;

    /// <summary>$92:DDF0, SamusTilesAnimation_AnimationDefinitions_DDF0; selected by DamageBoostRightPose and the semantic aliases below.</summary>
    private const ushort DamageBoostRightFrames = 0xddf0;

    /// <summary>$92:DD48, SamusTilesAnimation_AnimationDefinitions_DD48; selected by NormalJumpForwardRightPose and the semantic aliases below.</summary>
    private const ushort NormalJumpForwardRightFrames = 0xdd48;

    /// <summary>$92:DD50, SamusTilesAnimation_AnimationDefinitions_DD50; selected by NormalJumpForwardLeftPose and the semantic aliases below.</summary>
    private const ushort NormalJumpForwardLeftFrames = 0xdd50;

    /// <summary>$92:E038, SamusTilesAnimation_AnimationDefinitions_E038; selected by KnockbackRightPose and the semantic aliases below.</summary>
    private const ushort KnockbackRightFrames = 0xe038;

    /// <summary>$92:E040, SamusTilesAnimation_AnimationDefinitions_E040; selected by KnockbackLeftPose and the semantic aliases below.</summary>
    private const ushort KnockbackLeftFrames = 0xe040;

    /// <summary>$92:DD80, SamusTilesAnimation_AnimationDefinitions_DD80; selected by NormalJumpTransitionAimUpRightPose and the semantic aliases below.</summary>
    private const ushort NormalJumpTransitionAimUpRightFrames = 0xdd80;

    /// <summary>$92:DD84, SamusTilesAnimation_AnimationDefinitions_DD84; selected by NormalJumpTransitionAimUpLeftPose and the semantic aliases below.</summary>
    private const ushort NormalJumpTransitionAimUpLeftFrames = 0xdd84;

    /// <summary>$92:DD88, SamusTilesAnimation_AnimationDefinitions_DD88; selected by NormalJumpTransitionAimDiagonalUpRightPose and the semantic aliases below.</summary>
    private const ushort NormalJumpTransitionAimDiagonalUpRightFrames = 0xdd88;

    /// <summary>$92:DD8C, SamusTilesAnimation_AnimationDefinitions_DD8C; selected by NormalJumpTransitionAimDiagonalUpLeftPose and the semantic aliases below.</summary>
    private const ushort NormalJumpTransitionAimDiagonalUpLeftFrames = 0xdd8c;

    /// <summary>$92:DD90, SamusTilesAnimation_AnimationDefinitions_DD90; selected by NormalJumpTransitionAimDiagonalDownRightPose and the semantic aliases below.</summary>
    private const ushort NormalJumpTransitionAimDiagonalDownRightFrames = 0xdd90;

    /// <summary>$92:DD94, SamusTilesAnimation_AnimationDefinitions_DD94; selected by NormalJumpTransitionAimDiagonalDownLeftPose and the semantic aliases below.</summary>
    private const ushort NormalJumpTransitionAimDiagonalDownLeftFrames = 0xdd94;

    /// <summary>$92:E04C, SamusTilesAnimation_AnimationDefinitions_E04C; selected by UnusedPose5C and the semantic aliases below.</summary>
    private const ushort UnusedPose5CFrames = 0xe04c;

    /// <summary>$92:E050, SamusTilesAnimation_AnimationDefinitions_E050; selected by UnusedPose5D and the semantic aliases below.</summary>
    private const ushort UnusedPose5DFrames = 0xe050;

    /// <summary>$92:E158, SamusTilesAnimation_AnimationDefinitions_E158; selected by UnusedPose62 and the semantic aliases below.</summary>
    private const ushort UnusedPose62Frames = 0xe158;

    /// <summary>$92:E260, UNUSED_SamusTilesAnimation_AnimationDefinitions_92E260; selected by UnusedPose63 and the semantic aliases below.</summary>
    private const ushort UnusedPose63Frames = 0xe260;

    /// <summary>$92:E268, UNUSED_SamusTilesAnimation_AnimationDefinitions_92E268; selected by UnusedPose64 and the semantic aliases below.</summary>
    private const ushort UnusedPose64Frames = 0xe268;

    /// <summary>$92:E270, UNUSED_SamusTilesAnimation_AnimationDefinitions_92E270; selected by UnusedPose65 and the semantic aliases below.</summary>
    private const ushort UnusedPose65Frames = 0xe270;

    /// <summary>$92:E294, UNUSED_SamusTilesAnimation_AnimationDefinitions_92E294; selected by UnusedPose66 and the semantic aliases below.</summary>
    private const ushort UnusedPose66Frames = 0xe294;

    /// <summary>$92:DEC0, SamusTilesAnimation_AnimationDefinitions_DEC0; selected by FallingGunExtendedRightPose and the semantic aliases below.</summary>
    private const ushort FallingGunExtendedRightFrames = 0xdec0;

    /// <summary>$92:DEDC, SamusTilesAnimation_AnimationDefinitions_DEDC; selected by FallingGunExtendedLeftPose and the semantic aliases below.</summary>
    private const ushort FallingGunExtendedLeftFrames = 0xdedc;

    /// <summary>$92:DD58, SamusTilesAnimation_AnimationDefinitions_DD58; selected by NormalJumpAimDiagonalUpRightPose and the semantic aliases below.</summary>
    private const ushort NormalJumpAimDiagonalUpRightFrames = 0xdd58;

    /// <summary>$92:DD60, SamusTilesAnimation_AnimationDefinitions_DD60; selected by NormalJumpAimDiagonalUpLeftPose and the semantic aliases below.</summary>
    private const ushort NormalJumpAimDiagonalUpLeftFrames = 0xdd60;

    /// <summary>$92:DD68, SamusTilesAnimation_AnimationDefinitions_DD68; selected by NormalJumpAimDiagonalDownRightPose and the semantic aliases below.</summary>
    private const ushort NormalJumpAimDiagonalDownRightFrames = 0xdd68;

    /// <summary>$92:DD70, SamusTilesAnimation_AnimationDefinitions_DD70; selected by NormalJumpAimDiagonalDownLeftPose and the semantic aliases below.</summary>
    private const ushort NormalJumpAimDiagonalDownLeftFrames = 0xdd70;

    /// <summary>$92:DEF8, SamusTilesAnimation_AnimationDefinitions_DEF8; selected by FallingAimDiagonalUpRightPose and the semantic aliases below.</summary>
    private const ushort FallingAimDiagonalUpRightFrames = 0xdef8;

    /// <summary>$92:DF04, SamusTilesAnimation_AnimationDefinitions_DF04; selected by FallingAimDiagonalUpLeftPose and the semantic aliases below.</summary>
    private const ushort FallingAimDiagonalUpLeftFrames = 0xdf04;

    /// <summary>$92:DF10, SamusTilesAnimation_AnimationDefinitions_DF10; selected by FallingAimDiagonalDownRightPose and the semantic aliases below.</summary>
    private const ushort FallingAimDiagonalDownRightFrames = 0xdf10;

    /// <summary>$92:DF1C, SamusTilesAnimation_AnimationDefinitions_DF1C; selected by FallingAimDiagonalDownLeftPose and the semantic aliases below.</summary>
    private const ushort FallingAimDiagonalDownLeftFrames = 0xdf1c;

    /// <summary>$92:E430, SamusTilesAnimation_AnimationDefinitions_E430; selected by CrouchingAimDiagonalUpRightPose and the semantic aliases below.</summary>
    private const ushort CrouchingAimDiagonalUpRightFrames = 0xe430;

    /// <summary>$92:E434, SamusTilesAnimation_AnimationDefinitions_E434; selected by CrouchingAimDiagonalUpLeftPose and the semantic aliases below.</summary>
    private const ushort CrouchingAimDiagonalUpLeftFrames = 0xe434;

    /// <summary>$92:E438, SamusTilesAnimation_AnimationDefinitions_E438; selected by CrouchingAimDiagonalDownRightPose and the semantic aliases below.</summary>
    private const ushort CrouchingAimDiagonalDownRightFrames = 0xe438;

    /// <summary>$92:E43C, SamusTilesAnimation_AnimationDefinitions_E43C; selected by CrouchingAimDiagonalDownLeftPose and the semantic aliases below.</summary>
    private const ushort CrouchingAimDiagonalDownLeftFrames = 0xe43c;

    /// <summary>$92:E450, SamusTilesAnimation_AnimationDefinitions_E450; selected by MoonwalkAimUpLeftPose and the semantic aliases below.</summary>
    private const ushort MoonwalkAimUpLeftFrames = 0xe450;

    /// <summary>$92:E468, SamusTilesAnimation_AnimationDefinitions_E468; selected by MoonwalkAimUpRightPose and the semantic aliases below.</summary>
    private const ushort MoonwalkAimUpRightFrames = 0xe468;

    /// <summary>$92:E480, SamusTilesAnimation_AnimationDefinitions_E480; selected by MoonwalkAimDownLeftPose and the semantic aliases below.</summary>
    private const ushort MoonwalkAimDownLeftFrames = 0xe480;

    /// <summary>$92:E498, SamusTilesAnimation_AnimationDefinitions_E498; selected by MoonwalkAimDownRightPose and the semantic aliases below.</summary>
    private const ushort MoonwalkAimDownRightFrames = 0xe498;

    /// <summary>$92:E5A8, SamusTilesAnimation_AnimationDefinitions_E5A8; selected by SpringBallGroundRightPose and the semantic aliases below.</summary>
    private const ushort SpringBallGroundRightFrames = 0xe5a8;

    /// <summary>$92:E5D0, SamusTilesAnimation_AnimationDefinitions_E5D0; selected by SpringBallGroundLeftPose and the semantic aliases below.</summary>
    private const ushort SpringBallGroundLeftFrames = 0xe5d0;

    /// <summary>$92:E6B8, SamusTilesAnimation_AnimationDefinitions_E6B8; selected by ScrewAttackRightPose and the semantic aliases below.</summary>
    private const ushort ScrewAttackRightFrames = 0xe6b8;

    /// <summary>$92:E728, SamusTilesAnimation_AnimationDefinitions_E728; selected by ScrewAttackLeftPose and the semantic aliases below.</summary>
    private const ushort ScrewAttackLeftFrames = 0xe728;

    /// <summary>$92:E2B8, SamusTilesAnimation_AnimationDefinitions_E2B8; selected by WallJumpRightPose and the semantic aliases below.</summary>
    private const ushort WallJumpRightFrames = 0xe2b8;

    /// <summary>$92:E374, SamusTilesAnimation_AnimationDefinitions_E374; selected by WallJumpLeftPose and the semantic aliases below.</summary>
    private const ushort WallJumpLeftFrames = 0xe374;

    /// <summary>$92:E440, SamusTilesAnimation_AnimationDefinitions_E440; selected by CrouchingAimUpRightPose and the semantic aliases below.</summary>
    private const ushort CrouchingAimUpRightFrames = 0xe440;

    /// <summary>$92:E448, SamusTilesAnimation_AnimationDefinitions_E448; selected by CrouchingAimUpLeftPose and the semantic aliases below.</summary>
    private const ushort CrouchingAimUpLeftFrames = 0xe448;

    /// <summary>$92:E7B0, SamusTilesAnimation_AnimationDefinitions_E7B0; selected by TurningRightToLeftAimUpPose and the semantic aliases below.</summary>
    private const ushort TurningRightToLeftAimUpFrames = 0xe7b0;

    /// <summary>$92:E7BC, SamusTilesAnimation_AnimationDefinitions_E7BC; selected by TurningLeftToRightAimUpPose and the semantic aliases below.</summary>
    private const ushort TurningLeftToRightAimUpFrames = 0xe7bc;

    /// <summary>$92:E7C8, SamusTilesAnimation_AnimationDefinitions_E7C8; selected by TurningRightToLeftAimDiagonalDownPose and the semantic aliases below.</summary>
    private const ushort TurningRightToLeftAimDiagonalDownFrames = 0xe7c8;

    /// <summary>$92:E7D4, SamusTilesAnimation_AnimationDefinitions_E7D4; selected by TurningLeftToRightAimDiagonalDownPose and the semantic aliases below.</summary>
    private const ushort TurningLeftToRightAimDiagonalDownFrames = 0xe7d4;

    /// <summary>$92:E7F8, SamusTilesAnimation_AnimationDefinitions_E7F8; selected by TurningRightToLeftJumpAimUpPose and the semantic aliases below.</summary>
    private const ushort TurningRightToLeftJumpAimUpFrames = 0xe7f8;

    /// <summary>$92:E804, SamusTilesAnimation_AnimationDefinitions_E804; selected by TurningLeftToRightJumpAimUpPose and the semantic aliases below.</summary>
    private const ushort TurningLeftToRightJumpAimUpFrames = 0xe804;

    /// <summary>$92:E810, SamusTilesAnimation_AnimationDefinitions_E810; selected by TurningRightToLeftJumpAimDownPose and the semantic aliases below.</summary>
    private const ushort TurningRightToLeftJumpAimDownFrames = 0xe810;

    /// <summary>$92:E81C, SamusTilesAnimation_AnimationDefinitions_E81C; selected by TurningLeftToRightJumpAimDownPose and the semantic aliases below.</summary>
    private const ushort TurningLeftToRightJumpAimDownFrames = 0xe81c;

    /// <summary>$92:EBA4, SamusTilesAnimation_AnimationDefinitions_EBA4; selected by ForwardFacingSuitedPose and the semantic aliases below.</summary>
    private const ushort ForwardFacingSuitedFrames = 0xeba4;

    /// <summary>$92:DB90, SamusTilesAnimation_AnimationDefinitions_DB90; selected by NormalLandingRightPose and the semantic aliases below.</summary>
    private const ushort NormalLandingRightFrames = 0xdb90;

    /// <summary>$92:DB98, SamusTilesAnimation_AnimationDefinitions_DB98; selected by NormalLandingLeftPose and the semantic aliases below.</summary>
    private const ushort NormalLandingLeftFrames = 0xdb98;

    /// <summary>$92:DBA0, SamusTilesAnimation_AnimationDefinitions_DBA0; selected by SpinLandingRightPose and the semantic aliases below.</summary>
    private const ushort SpinLandingRightFrames = 0xdba0;

    /// <summary>$92:DBAC, SamusTilesAnimation_AnimationDefinitions_DBAC; selected by SpinLandingLeftPose and the semantic aliases below.</summary>
    private const ushort SpinLandingLeftFrames = 0xdbac;

    /// <summary>$92:E838, SamusTilesAnimation_AnimationDefinitions_E838; selected by DraygonGrabbedNeutralLeftPose and the semantic aliases below.</summary>
    private const ushort DraygonGrabbedNeutralLeftFrames = 0xe838;

    /// <summary>$92:E83C, SamusTilesAnimation_AnimationDefinitions_E83C; selected by DraygonGrabbedAimUpLeftPose and the semantic aliases below.</summary>
    private const ushort DraygonGrabbedAimUpLeftFrames = 0xe83c;

    /// <summary>$92:E840, SamusTilesAnimation_AnimationDefinitions_E840; selected by DraygonGrabbedFiringLeftPose and the semantic aliases below.</summary>
    private const ushort DraygonGrabbedFiringLeftFrames = 0xe840;

    /// <summary>$92:E844, SamusTilesAnimation_AnimationDefinitions_E844; selected by DraygonGrabbedAimDownLeftPose and the semantic aliases below.</summary>
    private const ushort DraygonGrabbedAimDownLeftFrames = 0xe844;

    /// <summary>$92:E860, SamusTilesAnimation_AnimationDefinitions_E860; selected by DraygonGrabbedMovingLeftPose and the semantic aliases below.</summary>
    private const ushort DraygonGrabbedMovingLeftFrames = 0xe860;

    /// <summary>$92:E880, SamusTilesAnimation_AnimationDefinitions_E880; selected by ShinesparkHorizontalRightPose and the semantic aliases below.</summary>
    private const ushort ShinesparkHorizontalRightFrames = 0xe880;

    /// <summary>$92:E884, SamusTilesAnimation_AnimationDefinitions_E884; selected by ShinesparkHorizontalLeftPose and the semantic aliases below.</summary>
    private const ushort ShinesparkHorizontalLeftFrames = 0xe884;

    /// <summary>$92:E878, SamusTilesAnimation_AnimationDefinitions_E878; selected by ShinesparkVerticalRightPose and the semantic aliases below.</summary>
    private const ushort ShinesparkVerticalRightFrames = 0xe878;

    /// <summary>$92:E87C, SamusTilesAnimation_AnimationDefinitions_E87C; selected by ShinesparkVerticalLeftPose and the semantic aliases below.</summary>
    private const ushort ShinesparkVerticalLeftFrames = 0xe87c;

    /// <summary>$92:E888, SamusTilesAnimation_AnimationDefinitions_E888; selected by ShinesparkDiagonalRightPose and the semantic aliases below.</summary>
    private const ushort ShinesparkDiagonalRightFrames = 0xe888;

    /// <summary>$92:E88C, SamusTilesAnimation_AnimationDefinitions_E88C; selected by ShinesparkDiagonalLeftPose and the semantic aliases below.</summary>
    private const ushort ShinesparkDiagonalLeftFrames = 0xe88c;

    /// <summary>$92:E890, SamusTilesAnimation_AnimationDefinitions_E890; selected by CrystalFlashRightPose and the semantic aliases below.</summary>
    private const ushort CrystalFlashRightFrames = 0xe890;

    /// <summary>$92:E8CC, SamusTilesAnimation_AnimationDefinitions_E8CC; selected by CrystalFlashLeftPose and the semantic aliases below.</summary>
    private const ushort CrystalFlashLeftFrames = 0xe8cc;

    /// <summary>$92:DBF8, SamusTilesAnimation_AnimationDefinitions_DBF8; selected by XrayingStandingRightPose and the semantic aliases below.</summary>
    private const ushort XrayingStandingRightFrames = 0xdbf8;

    /// <summary>$92:DC0C, SamusTilesAnimation_AnimationDefinitions_DC0C; selected by XrayingStandingLeftPose and the semantic aliases below.</summary>
    private const ushort XrayingStandingLeftFrames = 0xdc0c;

    /// <summary>$92:E908, SamusTilesAnimation_AnimationDefinitions_E908; selected by DeathSequenceRightPose and the semantic aliases below.</summary>
    private const ushort DeathSequenceRightFrames = 0xe908;

    /// <summary>$92:E920, SamusTilesAnimation_AnimationDefinitions_E920; selected by DeathSequenceLeftPose and the semantic aliases below.</summary>
    private const ushort DeathSequenceLeftFrames = 0xe920;

    /// <summary>$92:DC20, SamusTilesAnimation_AnimationDefinitions_DC20; selected by XrayingCrouchingRightPose and the semantic aliases below.</summary>
    private const ushort XrayingCrouchingRightFrames = 0xdc20;

    /// <summary>$92:DC34, SamusTilesAnimation_AnimationDefinitions_DC34; selected by XrayingCrouchingLeftPose and the semantic aliases below.</summary>
    private const ushort XrayingCrouchingLeftFrames = 0xdc34;

    /// <summary>$92:E4D8, UNUSED_SamusTilesAnimation_AnimationDefinitions_92E4D8; selected by UnusedPoseDb and the semantic aliases below.</summary>
    private const ushort UnusedPoseDbFrames = 0xe4d8;

    /// <summary>$92:E4E4, UNUSED_SamusTilesAnimation_AnimationDefinitions_92E4E4; selected by UnusedPoseDc and the semantic aliases below.</summary>
    private const ushort UnusedPoseDcFrames = 0xe4e4;

    /// <summary>$92:E4F0, UNUSED_SamusTilesAnimation_AnimationDefinitions_92E4F0; selected by UnusedPoseDd and the semantic aliases below.</summary>
    private const ushort UnusedPoseDdFrames = 0xe4f0;

    /// <summary>$92:E4FC, UNUSED_SamusTilesAnimation_AnimationDefinitions_92E4FC; selected by UnusedPoseDe and the semantic aliases below.</summary>
    private const ushort UnusedPoseDeFrames = 0xe4fc;

    /// <summary>$92:DBB8, SamusTilesAnimation_AnimationDefinitions_DBB8; selected by LandingAimUpRightPose and the semantic aliases below.</summary>
    private const ushort LandingAimUpRightFrames = 0xdbb8;

    /// <summary>$92:DBC0, SamusTilesAnimation_AnimationDefinitions_DBC0; selected by LandingAimUpLeftPose and the semantic aliases below.</summary>
    private const ushort LandingAimUpLeftFrames = 0xdbc0;

    /// <summary>$92:DBC8, SamusTilesAnimation_AnimationDefinitions_DBC8; selected by LandingAimDiagonalUpRightPose and the semantic aliases below.</summary>
    private const ushort LandingAimDiagonalUpRightFrames = 0xdbc8;

    /// <summary>$92:DBD0, SamusTilesAnimation_AnimationDefinitions_DBD0; selected by LandingAimDiagonalUpLeftPose and the semantic aliases below.</summary>
    private const ushort LandingAimDiagonalUpLeftFrames = 0xdbd0;

    /// <summary>$92:DBD8, SamusTilesAnimation_AnimationDefinitions_DBD8; selected by LandingAimDiagonalDownRightPose and the semantic aliases below.</summary>
    private const ushort LandingAimDiagonalDownRightFrames = 0xdbd8;

    /// <summary>$92:DBE0, SamusTilesAnimation_AnimationDefinitions_DBE0; selected by LandingAimDiagonalDownLeftPose and the semantic aliases below.</summary>
    private const ushort LandingAimDiagonalDownLeftFrames = 0xdbe0;

    /// <summary>$92:DBE8, SamusTilesAnimation_AnimationDefinitions_DBE8; selected by FiringLandingRightPose and the semantic aliases below.</summary>
    private const ushort FiringLandingRightFrames = 0xdbe8;

    /// <summary>$92:DBF0, SamusTilesAnimation_AnimationDefinitions_DBF0; selected by FiringLandingLeftPose and the semantic aliases below.</summary>
    private const ushort FiringLandingLeftFrames = 0xdbf0;

    /// <summary>$92:E938, SamusTilesAnimation_AnimationDefinitions_E938; selected by DrainedCrouchingRightPose and the semantic aliases below.</summary>
    private const ushort DrainedCrouchingRightFrames = 0xe938;

    /// <summary>$92:E974, SamusTilesAnimation_AnimationDefinitions_E974; selected by DrainedCrouchingLeftPose and the semantic aliases below.</summary>
    private const ushort DrainedCrouchingLeftFrames = 0xe974;

    /// <summary>$92:E9F4, SamusTilesAnimation_AnimationDefinitions_E9F4; selected by DrainedStandingRightPose and the semantic aliases below.</summary>
    private const ushort DrainedStandingRightFrames = 0xe9f4;

    /// <summary>$92:EA0C, SamusTilesAnimation_AnimationDefinitions_EA0C; selected by DrainedStandingLeftPose and the semantic aliases below.</summary>
    private const ushort DrainedStandingLeftFrames = 0xea0c;

    /// <summary>$92:E828, SamusTilesAnimation_AnimationDefinitions_E828; selected by DraygonGrabbedNeutralRightPose and the semantic aliases below.</summary>
    private const ushort DraygonGrabbedNeutralRightFrames = 0xe828;

    /// <summary>$92:E82C, SamusTilesAnimation_AnimationDefinitions_E82C; selected by DraygonGrabbedAimUpRightPose and the semantic aliases below.</summary>
    private const ushort DraygonGrabbedAimUpRightFrames = 0xe82c;

    /// <summary>$92:E830, SamusTilesAnimation_AnimationDefinitions_E830; selected by DraygonGrabbedFiringRightPose and the semantic aliases below.</summary>
    private const ushort DraygonGrabbedFiringRightFrames = 0xe830;

    /// <summary>$92:E834, SamusTilesAnimation_AnimationDefinitions_E834; selected by DraygonGrabbedAimDownRightPose and the semantic aliases below.</summary>
    private const ushort DraygonGrabbedAimDownRightFrames = 0xe834;

    /// <summary>$92:E848, SamusTilesAnimation_AnimationDefinitions_E848; selected by DraygonGrabbedMovingRightPose and the semantic aliases below.</summary>
    private const ushort DraygonGrabbedMovingRightFrames = 0xe848;

    /// <summary>$92:D94E..DB47: exactly253 real-pose selectors; FD..FF are outside the installed body domain.</summary>
    internal static ushort DefaultFrameList(byte pose) => (SamusPoseId)pose switch
    {
        SamusPoseId.ForwardFacingPowerSuitPose => ForwardFacingPowerSuitFrames,
        SamusPoseId.FacingRightNormalPose or
        SamusPoseId.UnusedPose47 or
        SamusPoseId.RanIntoWallRightPose or
        SamusPoseId.GrappleStandingRightPose => FacingRightNormalFrames,
        SamusPoseId.FacingLeftNormalPose or
        SamusPoseId.UnusedPose48 or
        SamusPoseId.RanIntoWallLeftPose or
        SamusPoseId.GrappleStandingLeftPose => FacingLeftNormalFrames,
        SamusPoseId.StandingAimUpRightPose => StandingAimUpRightFrames,
        SamusPoseId.StandingAimUpLeftPose => StandingAimUpLeftFrames,
        SamusPoseId.StandingAimDiagonalUpRightPose or
        SamusPoseId.RanIntoWallAimUpRightPose => StandingAimDiagonalUpRightFrames,
        SamusPoseId.StandingAimDiagonalUpLeftPose or
        SamusPoseId.RanIntoWallAimUpLeftPose => StandingAimDiagonalUpLeftFrames,
        SamusPoseId.StandingAimDiagonalDownRightPose or
        SamusPoseId.GrappleStandingDownRightPose or
        SamusPoseId.RanIntoWallAimDownRightPose => StandingAimDiagonalDownRightFrames,
        SamusPoseId.StandingAimDiagonalDownLeftPose or
        SamusPoseId.GrappleStandingDownLeftPose or
        SamusPoseId.RanIntoWallAimDownLeftPose => StandingAimDiagonalDownLeftFrames,
        SamusPoseId.MovingRightNormalPose => MovingRightNormalFrames,
        SamusPoseId.MovingLeftNormalPose => MovingLeftNormalFrames,
        SamusPoseId.MovingRightGunExtendedPose => MovingRightGunExtendedFrames,
        SamusPoseId.MovingLeftGunExtendedPose => MovingLeftGunExtendedFrames,
        SamusPoseId.RunningAimUpRightPose => RunningAimUpRightFrames,
        SamusPoseId.RunningAimUpLeftPose => RunningAimUpLeftFrames,
        SamusPoseId.RunningAimDiagonalUpRightPose => RunningAimDiagonalUpRightFrames,
        SamusPoseId.RunningAimDiagonalUpLeftPose => RunningAimDiagonalUpLeftFrames,
        SamusPoseId.RunningAimDiagonalDownRightPose => RunningAimDiagonalDownRightFrames,
        SamusPoseId.RunningAimDiagonalDownLeftPose => RunningAimDiagonalDownLeftFrames,
        SamusPoseId.NormalJumpGunExtendedRightPose or
        SamusPoseId.UnusedPoseAC => NormalJumpGunExtendedRightFrames,
        SamusPoseId.NormalJumpGunExtendedLeftPose or
        SamusPoseId.UnusedPoseAD => NormalJumpGunExtendedLeftFrames,
        SamusPoseId.NormalJumpAimUpRightPose => NormalJumpAimUpRightFrames,
        SamusPoseId.NormalJumpAimUpLeftPose => NormalJumpAimUpLeftFrames,
        SamusPoseId.NormalJumpAimDownRightPose or
        SamusPoseId.UnusedPoseAE => NormalJumpAimDownRightFrames,
        SamusPoseId.NormalJumpAimDownLeftPose or
        SamusPoseId.UnusedPoseAF => NormalJumpAimDownLeftFrames,
        SamusPoseId.SpinJumpRightPose or
        SamusPoseId.UnusedPose20 or
        SamusPoseId.UnusedPose21 or
        SamusPoseId.UnusedPose22 or
        SamusPoseId.UnusedPose23 or
        SamusPoseId.UnusedPose24 or
        SamusPoseId.UnusedKnockbackRightPose or
        SamusPoseId.UnusedKnockbackLeftPose or
        SamusPoseId.UnusedPose39 or
        SamusPoseId.UnusedPose3A or
        SamusPoseId.UnusedPose42 => SpinJumpRightFrames,
        SamusPoseId.SpinJumpLeftPose => SpinJumpLeftFrames,
        SamusPoseId.SpaceJumpRightPose => SpaceJumpRightFrames,
        SamusPoseId.SpaceJumpLeftPose => SpaceJumpLeftFrames,
        SamusPoseId.MorphBallGroundRightPose or
        SamusPoseId.MorphBallFallingRightPose or
        SamusPoseId.MorphBallFallingLeftPose or
        SamusPoseId.UnusedPose3F or
        SamusPoseId.UnusedPose40 => MorphBallGroundRightFrames,
        SamusPoseId.MorphBallMovingRightPose => MorphBallMovingRightFrames,
        SamusPoseId.MorphBallMovingLeftPose => MorphBallMovingLeftFrames,
        SamusPoseId.TurningRightToLeftPose or
        SamusPoseId.MoonwalkTurnJumpLeftPose or
        SamusPoseId.UnusedPoseC6 => TurningRightToLeftFrames,
        SamusPoseId.TurningLeftToRightPose or
        SamusPoseId.MoonwalkTurnJumpRightPose => TurningLeftToRightFrames,
        SamusPoseId.CrouchingRightPose or
        SamusPoseId.GrappleCrouchingRightPose => CrouchingRightFrames,
        SamusPoseId.CrouchingLeftPose or
        SamusPoseId.GrappleCrouchingLeftPose => CrouchingLeftFrames,
        SamusPoseId.FallingRightPose => FallingRightFrames,
        SamusPoseId.FallingLeftPose => FallingLeftFrames,
        SamusPoseId.FallingAimUpRightPose => FallingAimUpRightFrames,
        SamusPoseId.FallingAimUpLeftPose => FallingAimUpLeftFrames,
        SamusPoseId.FallingAimDownRightPose => FallingAimDownRightFrames,
        SamusPoseId.FallingAimDownLeftPose => FallingAimDownLeftFrames,
        SamusPoseId.TurningRightToLeftJumpPose or
        SamusPoseId.TurningRightToLeftCrouchingPose or
        SamusPoseId.TurningRightToLeftFallingPose => TurningRightToLeftJumpFrames,
        SamusPoseId.TurningLeftToRightJumpPose or
        SamusPoseId.TurningLeftToRightCrouchingPose or
        SamusPoseId.TurningLeftToRightFallingPose => TurningLeftToRightJumpFrames,
        SamusPoseId.CrouchingTransitionRightPose or
        SamusPoseId.StandingTransitionRightPose => CrouchingTransitionRightFrames,
        SamusPoseId.CrouchingTransitionLeftPose or
        SamusPoseId.StandingTransitionLeftPose => CrouchingTransitionLeftFrames,
        SamusPoseId.MorphingTransitionRightPose => MorphingTransitionRightFrames,
        SamusPoseId.MorphingTransitionLeftPose => MorphingTransitionLeftFrames,
        SamusPoseId.UnmorphingTransitionRightPose => UnmorphingTransitionRightFrames,
        SamusPoseId.UnmorphingTransitionLeftPose => UnmorphingTransitionLeftFrames,
        SamusPoseId.MorphBallGroundLeftPose or
        SamusPoseId.UnusedPoseC5 or
        SamusPoseId.UnusedPoseDf => MorphBallGroundLeftFrames,
        SamusPoseId.UnusedPose45 or
        SamusPoseId.UnusedPose46 or
        SamusPoseId.UnusedPose5B or
        SamusPoseId.GrappleWallContactLeftPose => UnusedPose45Frames,
        SamusPoseId.MoonwalkFacingLeftPose => MoonwalkFacingLeftFrames,
        SamusPoseId.MoonwalkFacingRightPose => MoonwalkFacingRightFrames,
        SamusPoseId.NeutralJumpTransitionRightPose => NeutralJumpTransitionRightFrames,
        SamusPoseId.NeutralJumpTransitionLeftPose => NeutralJumpTransitionLeftFrames,
        SamusPoseId.NeutralJumpRightPose or
        SamusPoseId.ShinesparkWindupRightPose => NeutralJumpRightFrames,
        SamusPoseId.NeutralJumpLeftPose or
        SamusPoseId.ShinesparkWindupLeftPose => NeutralJumpLeftFrames,
        SamusPoseId.DamageBoostLeftPose => DamageBoostLeftFrames,
        SamusPoseId.DamageBoostRightPose => DamageBoostRightFrames,
        SamusPoseId.NormalJumpForwardRightPose => NormalJumpForwardRightFrames,
        SamusPoseId.NormalJumpForwardLeftPose => NormalJumpForwardLeftFrames,
        SamusPoseId.KnockbackRightPose => KnockbackRightFrames,
        SamusPoseId.KnockbackLeftPose => KnockbackLeftFrames,
        SamusPoseId.NormalJumpTransitionAimUpRightPose or
        SamusPoseId.CrouchingTransitionAimUpRightPose or
        SamusPoseId.StandingTransitionAimUpRightPose => NormalJumpTransitionAimUpRightFrames,
        SamusPoseId.NormalJumpTransitionAimUpLeftPose or
        SamusPoseId.CrouchingTransitionAimUpLeftPose or
        SamusPoseId.StandingTransitionAimUpLeftPose => NormalJumpTransitionAimUpLeftFrames,
        SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
        SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
        SamusPoseId.StandingTransitionAimDiagonalUpRightPose => NormalJumpTransitionAimDiagonalUpRightFrames,
        SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalUpLeftPose => NormalJumpTransitionAimDiagonalUpLeftFrames,
        SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
        SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
        SamusPoseId.StandingTransitionAimDiagonalDownRightPose => NormalJumpTransitionAimDiagonalDownRightFrames,
        SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => NormalJumpTransitionAimDiagonalDownLeftFrames,
        SamusPoseId.UnusedPose5C or
        SamusPoseId.GrappleWallContactRightPose => UnusedPose5CFrames,
        SamusPoseId.UnusedPose5D or
        SamusPoseId.UnusedPose5E or
        SamusPoseId.UnusedPose5F or
        SamusPoseId.UnusedPose60 or
        SamusPoseId.UnusedPose61 or
        SamusPoseId.GrappleSwingRightPose => UnusedPose5DFrames,
        SamusPoseId.UnusedPose62 or
        SamusPoseId.GrappleSwingLeftPose => UnusedPose62Frames,
        SamusPoseId.UnusedPose63 => UnusedPose63Frames,
        SamusPoseId.UnusedPose64 => UnusedPose64Frames,
        SamusPoseId.UnusedPose65 => UnusedPose65Frames,
        SamusPoseId.UnusedPose66 => UnusedPose66Frames,
        SamusPoseId.FallingGunExtendedRightPose => FallingGunExtendedRightFrames,
        SamusPoseId.FallingGunExtendedLeftPose => FallingGunExtendedLeftFrames,
        SamusPoseId.NormalJumpAimDiagonalUpRightPose => NormalJumpAimDiagonalUpRightFrames,
        SamusPoseId.NormalJumpAimDiagonalUpLeftPose => NormalJumpAimDiagonalUpLeftFrames,
        SamusPoseId.NormalJumpAimDiagonalDownRightPose or
        SamusPoseId.UnusedPoseB0 => NormalJumpAimDiagonalDownRightFrames,
        SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
        SamusPoseId.UnusedPoseB1 => NormalJumpAimDiagonalDownLeftFrames,
        SamusPoseId.FallingAimDiagonalUpRightPose => FallingAimDiagonalUpRightFrames,
        SamusPoseId.FallingAimDiagonalUpLeftPose => FallingAimDiagonalUpLeftFrames,
        SamusPoseId.FallingAimDiagonalDownRightPose => FallingAimDiagonalDownRightFrames,
        SamusPoseId.FallingAimDiagonalDownLeftPose => FallingAimDiagonalDownLeftFrames,
        SamusPoseId.CrouchingAimDiagonalUpRightPose => CrouchingAimDiagonalUpRightFrames,
        SamusPoseId.CrouchingAimDiagonalUpLeftPose => CrouchingAimDiagonalUpLeftFrames,
        SamusPoseId.CrouchingAimDiagonalDownRightPose or
        SamusPoseId.GrappleCrouchingDownRightPose => CrouchingAimDiagonalDownRightFrames,
        SamusPoseId.CrouchingAimDiagonalDownLeftPose or
        SamusPoseId.GrappleCrouchingDownLeftPose => CrouchingAimDiagonalDownLeftFrames,
        SamusPoseId.MoonwalkAimUpLeftPose => MoonwalkAimUpLeftFrames,
        SamusPoseId.MoonwalkAimUpRightPose => MoonwalkAimUpRightFrames,
        SamusPoseId.MoonwalkAimDownLeftPose => MoonwalkAimDownLeftFrames,
        SamusPoseId.MoonwalkAimDownRightPose => MoonwalkAimDownRightFrames,
        SamusPoseId.SpringBallGroundRightPose or
        SamusPoseId.SpringBallMovingRightPose or
        SamusPoseId.SpringBallFallingRightPose or
        SamusPoseId.SpringBallJumpRightPose => SpringBallGroundRightFrames,
        SamusPoseId.SpringBallGroundLeftPose or
        SamusPoseId.SpringBallMovingLeftPose or
        SamusPoseId.SpringBallFallingLeftPose or
        SamusPoseId.SpringBallJumpLeftPose => SpringBallGroundLeftFrames,
        SamusPoseId.ScrewAttackRightPose => ScrewAttackRightFrames,
        SamusPoseId.ScrewAttackLeftPose => ScrewAttackLeftFrames,
        SamusPoseId.WallJumpRightPose => WallJumpRightFrames,
        SamusPoseId.WallJumpLeftPose => WallJumpLeftFrames,
        SamusPoseId.CrouchingAimUpRightPose => CrouchingAimUpRightFrames,
        SamusPoseId.CrouchingAimUpLeftPose => CrouchingAimUpLeftFrames,
        SamusPoseId.TurningRightToLeftAimUpPose or
        SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
        SamusPoseId.MoonwalkTurnJumpAimUpLeftPose => TurningRightToLeftAimUpFrames,
        SamusPoseId.TurningLeftToRightAimUpPose or
        SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
        SamusPoseId.MoonwalkTurnJumpAimUpRightPose => TurningLeftToRightAimUpFrames,
        SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
        SamusPoseId.MoonwalkTurnJumpAimDownLeftPose => TurningRightToLeftAimDiagonalDownFrames,
        SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
        SamusPoseId.MoonwalkTurnJumpAimDownRightPose => TurningLeftToRightAimDiagonalDownFrames,
        SamusPoseId.TurningRightToLeftJumpAimUpPose or
        SamusPoseId.TurningRightToLeftFallingAimUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
        SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose => TurningRightToLeftJumpAimUpFrames,
        SamusPoseId.TurningLeftToRightJumpAimUpPose or
        SamusPoseId.TurningLeftToRightFallingAimUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
        SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose => TurningLeftToRightJumpAimUpFrames,
        SamusPoseId.TurningRightToLeftJumpAimDownPose or
        SamusPoseId.TurningRightToLeftFallingAimDownPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose => TurningRightToLeftJumpAimDownFrames,
        SamusPoseId.TurningLeftToRightJumpAimDownPose or
        SamusPoseId.TurningLeftToRightFallingAimDownPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose => TurningLeftToRightJumpAimDownFrames,
        SamusPoseId.ForwardFacingSuitedPose => ForwardFacingSuitedFrames,
        SamusPoseId.NormalLandingRightPose => NormalLandingRightFrames,
        SamusPoseId.NormalLandingLeftPose => NormalLandingLeftFrames,
        SamusPoseId.SpinLandingRightPose => SpinLandingRightFrames,
        SamusPoseId.SpinLandingLeftPose => SpinLandingLeftFrames,
        SamusPoseId.DraygonGrabbedNeutralLeftPose => DraygonGrabbedNeutralLeftFrames,
        SamusPoseId.DraygonGrabbedAimUpLeftPose => DraygonGrabbedAimUpLeftFrames,
        SamusPoseId.DraygonGrabbedFiringLeftPose => DraygonGrabbedFiringLeftFrames,
        SamusPoseId.DraygonGrabbedAimDownLeftPose => DraygonGrabbedAimDownLeftFrames,
        SamusPoseId.DraygonGrabbedMovingLeftPose => DraygonGrabbedMovingLeftFrames,
        SamusPoseId.ShinesparkHorizontalRightPose => ShinesparkHorizontalRightFrames,
        SamusPoseId.ShinesparkHorizontalLeftPose => ShinesparkHorizontalLeftFrames,
        SamusPoseId.ShinesparkVerticalRightPose => ShinesparkVerticalRightFrames,
        SamusPoseId.ShinesparkVerticalLeftPose => ShinesparkVerticalLeftFrames,
        SamusPoseId.ShinesparkDiagonalRightPose => ShinesparkDiagonalRightFrames,
        SamusPoseId.ShinesparkDiagonalLeftPose => ShinesparkDiagonalLeftFrames,
        SamusPoseId.CrystalFlashRightPose => CrystalFlashRightFrames,
        SamusPoseId.CrystalFlashLeftPose => CrystalFlashLeftFrames,
        SamusPoseId.XrayingStandingRightPose => XrayingStandingRightFrames,
        SamusPoseId.XrayingStandingLeftPose => XrayingStandingLeftFrames,
        SamusPoseId.DeathSequenceRightPose => DeathSequenceRightFrames,
        SamusPoseId.DeathSequenceLeftPose => DeathSequenceLeftFrames,
        SamusPoseId.XrayingCrouchingRightPose => XrayingCrouchingRightFrames,
        SamusPoseId.XrayingCrouchingLeftPose => XrayingCrouchingLeftFrames,
        SamusPoseId.UnusedPoseDb => UnusedPoseDbFrames,
        SamusPoseId.UnusedPoseDc => UnusedPoseDcFrames,
        SamusPoseId.UnusedPoseDd => UnusedPoseDdFrames,
        SamusPoseId.UnusedPoseDe => UnusedPoseDeFrames,
        SamusPoseId.LandingAimUpRightPose => LandingAimUpRightFrames,
        SamusPoseId.LandingAimUpLeftPose => LandingAimUpLeftFrames,
        SamusPoseId.LandingAimDiagonalUpRightPose => LandingAimDiagonalUpRightFrames,
        SamusPoseId.LandingAimDiagonalUpLeftPose => LandingAimDiagonalUpLeftFrames,
        SamusPoseId.LandingAimDiagonalDownRightPose => LandingAimDiagonalDownRightFrames,
        SamusPoseId.LandingAimDiagonalDownLeftPose => LandingAimDiagonalDownLeftFrames,
        SamusPoseId.FiringLandingRightPose => FiringLandingRightFrames,
        SamusPoseId.FiringLandingLeftPose => FiringLandingLeftFrames,
        SamusPoseId.DrainedCrouchingRightPose => DrainedCrouchingRightFrames,
        SamusPoseId.DrainedCrouchingLeftPose => DrainedCrouchingLeftFrames,
        SamusPoseId.DrainedStandingRightPose => DrainedStandingRightFrames,
        SamusPoseId.DrainedStandingLeftPose => DrainedStandingLeftFrames,
        SamusPoseId.DraygonGrabbedNeutralRightPose => DraygonGrabbedNeutralRightFrames,
        SamusPoseId.DraygonGrabbedAimUpRightPose => DraygonGrabbedAimUpRightFrames,
        SamusPoseId.DraygonGrabbedFiringRightPose => DraygonGrabbedFiringRightFrames,
        SamusPoseId.DraygonGrabbedAimDownRightPose => DraygonGrabbedAimDownRightFrames,
        SamusPoseId.DraygonGrabbedMovingRightPose => DraygonGrabbedMovingRightFrames,
        _ => throw new ArgumentOutOfRangeException(nameof(pose)),
    };
}
