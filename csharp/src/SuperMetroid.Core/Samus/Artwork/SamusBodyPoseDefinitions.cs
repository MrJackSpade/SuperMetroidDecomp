using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Semantic real-pose selection of native body frame-list identities; selected frame payloads remain separately required.</summary>
internal static class SamusBodyPoseDefinitions
{
    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_EA24; selected by ForwardFacingPowerSuitPose and the semantic aliases below.</summary>
    internal const int ForwardFacingPowerSuitFrames = 951;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DB48; selected by FacingRightNormalPose and the semantic aliases below.</summary>
    internal const int FacingRightNormalFrames = 0;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DB6C; selected by FacingLeftNormalPose and the semantic aliases below.</summary>
    internal const int FacingLeftNormalFrames = 9;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E018; selected by StandingAimUpRightPose and the semantic aliases below.</summary>
    internal const int StandingAimUpRightFrames = 308;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E020; selected by StandingAimUpLeftPose and the semantic aliases below.</summary>
    internal const int StandingAimUpLeftFrames = 310;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E028; selected by StandingAimDiagonalUpRightPose and the semantic aliases below.</summary>
    internal const int StandingAimDiagonalUpRightFrames = 312;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E02C; selected by StandingAimDiagonalUpLeftPose and the semantic aliases below.</summary>
    internal const int StandingAimDiagonalUpLeftFrames = 313;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E030; selected by StandingAimDiagonalDownRightPose and the semantic aliases below.</summary>
    internal const int StandingAimDiagonalDownRightFrames = 314;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E034; selected by StandingAimDiagonalDownLeftPose and the semantic aliases below.</summary>
    internal const int StandingAimDiagonalDownLeftFrames = 315;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DC48; selected by MovingRightNormalPose and the semantic aliases below.</summary>
    internal const int MovingRightNormalFrames = 64;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DC70; selected by MovingLeftNormalPose and the semantic aliases below.</summary>
    internal const int MovingLeftNormalFrames = 74;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DC98; selected by MovingRightGunExtendedPose and the semantic aliases below.</summary>
    internal const int MovingRightGunExtendedFrames = 84;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DCC0; selected by MovingLeftGunExtendedPose and the semantic aliases below.</summary>
    internal const int MovingLeftGunExtendedFrames = 94;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DF28; selected by RunningAimUpRightPose and the semantic aliases below.</summary>
    internal const int RunningAimUpRightFrames = 248;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DF50; selected by RunningAimUpLeftPose and the semantic aliases below.</summary>
    internal const int RunningAimUpLeftFrames = 258;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DF78; selected by RunningAimDiagonalUpRightPose and the semantic aliases below.</summary>
    internal const int RunningAimDiagonalUpRightFrames = 268;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DFA0; selected by RunningAimDiagonalUpLeftPose and the semantic aliases below.</summary>
    internal const int RunningAimDiagonalUpLeftFrames = 278;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DFC8; selected by RunningAimDiagonalDownRightPose and the semantic aliases below.</summary>
    internal const int RunningAimDiagonalDownRightFrames = 288;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DFF0; selected by RunningAimDiagonalDownLeftPose and the semantic aliases below.</summary>
    internal const int RunningAimDiagonalDownLeftFrames = 298;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD28; selected by NormalJumpGunExtendedRightPose and the semantic aliases below.</summary>
    internal const int NormalJumpGunExtendedRightFrames = 120;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD30; selected by NormalJumpGunExtendedLeftPose and the semantic aliases below.</summary>
    internal const int NormalJumpGunExtendedLeftFrames = 122;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD38; selected by NormalJumpAimUpRightPose and the semantic aliases below.</summary>
    internal const int NormalJumpAimUpRightFrames = 124;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD40; selected by NormalJumpAimUpLeftPose and the semantic aliases below.</summary>
    internal const int NormalJumpAimUpLeftFrames = 126;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD18; selected by NormalJumpAimDownRightPose and the semantic aliases below.</summary>
    internal const int NormalJumpAimDownRightFrames = 116;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD20; selected by NormalJumpAimDownLeftPose and the semantic aliases below.</summary>
    internal const int NormalJumpAimDownLeftFrames = 118;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E5F8; selected by SpinJumpRightPose and the semantic aliases below.</summary>
    internal const int SpinJumpRightFrames = 684;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E628; selected by SpinJumpLeftPose and the semantic aliases below.</summary>
    internal const int SpinJumpLeftFrames = 696;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E658; selected by SpaceJumpRightPose and the semantic aliases below.</summary>
    internal const int SpaceJumpRightFrames = 708;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E688; selected by SpaceJumpLeftPose and the semantic aliases below.</summary>
    internal const int SpaceJumpLeftFrames = 720;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E508; selected by MorphBallGroundRightPose and the semantic aliases below.</summary>
    internal const int MorphBallGroundRightFrames = 624;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E558; selected by MorphBallMovingRightPose and the semantic aliases below.</summary>
    internal const int MorphBallMovingRightFrames = 644;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E580; selected by MorphBallMovingLeftPose and the semantic aliases below.</summary>
    internal const int MorphBallMovingLeftFrames = 654;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E798; selected by TurningRightToLeftPose and the semantic aliases below.</summary>
    internal const int TurningRightToLeftFrames = 788;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E7A4; selected by TurningLeftToRightPose and the semantic aliases below.</summary>
    internal const int TurningLeftToRightFrames = 791;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DE18; selected by CrouchingRightPose and the semantic aliases below.</summary>
    internal const int CrouchingRightFrames = 180;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DE3C; selected by CrouchingLeftPose and the semantic aliases below.</summary>
    internal const int CrouchingLeftFrames = 189;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DE60; selected by FallingRightPose and the semantic aliases below.</summary>
    internal const int FallingRightFrames = 198;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DE7C; selected by FallingLeftPose and the semantic aliases below.</summary>
    internal const int FallingLeftFrames = 205;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DE98; selected by FallingAimUpRightPose and the semantic aliases below.</summary>
    internal const int FallingAimUpRightFrames = 212;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DEA4; selected by FallingAimUpLeftPose and the semantic aliases below.</summary>
    internal const int FallingAimUpLeftFrames = 215;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DEB0; selected by FallingAimDownRightPose and the semantic aliases below.</summary>
    internal const int FallingAimDownRightFrames = 218;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DEB8; selected by FallingAimDownLeftPose and the semantic aliases below.</summary>
    internal const int FallingAimDownLeftFrames = 220;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E7E0; selected by TurningRightToLeftJumpPose and the semantic aliases below.</summary>
    internal const int TurningRightToLeftJumpFrames = 806;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E7EC; selected by TurningLeftToRightJumpPose and the semantic aliases below.</summary>
    internal const int TurningLeftToRightJumpFrames = 809;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E4B0; selected by CrouchingTransitionRightPose and the semantic aliases below.</summary>
    internal const int CrouchingTransitionRightFrames = 602;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E4B4; selected by CrouchingTransitionLeftPose and the semantic aliases below.</summary>
    internal const int CrouchingTransitionLeftFrames = 603;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E4B8; selected by MorphingTransitionRightPose and the semantic aliases below.</summary>
    internal const int MorphingTransitionRightFrames = 604;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E4C0; selected by MorphingTransitionLeftPose and the semantic aliases below.</summary>
    internal const int MorphingTransitionLeftFrames = 606;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E4C8; selected by UnmorphingTransitionRightPose and the semantic aliases below.</summary>
    internal const int UnmorphingTransitionRightFrames = 608;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E4D0; selected by UnmorphingTransitionLeftPose and the semantic aliases below.</summary>
    internal const int UnmorphingTransitionLeftFrames = 610;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E530; selected by MorphBallGroundLeftPose and the semantic aliases below.</summary>
    internal const int MorphBallGroundLeftFrames = 634;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E048; selected by UnusedPose45 and the semantic aliases below.</summary>
    internal const int UnusedPose45Frames = 320;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DCE8; selected by MoonwalkFacingLeftPose and the semantic aliases below.</summary>
    internal const int MoonwalkFacingLeftFrames = 104;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD00; selected by MoonwalkFacingRightPose and the semantic aliases below.</summary>
    internal const int MoonwalkFacingRightFrames = 110;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD78; selected by NeutralJumpTransitionRightPose and the semantic aliases below.</summary>
    internal const int NeutralJumpTransitionRightFrames = 140;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD7C; selected by NeutralJumpTransitionLeftPose and the semantic aliases below.</summary>
    internal const int NeutralJumpTransitionLeftFrames = 141;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD98; selected by NeutralJumpRightPose and the semantic aliases below.</summary>
    internal const int NeutralJumpRightFrames = 148;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DDB0; selected by NeutralJumpLeftPose and the semantic aliases below.</summary>
    internal const int NeutralJumpLeftFrames = 154;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DDC8; selected by DamageBoostLeftPose and the semantic aliases below.</summary>
    internal const int DamageBoostLeftFrames = 160;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DDF0; selected by DamageBoostRightPose and the semantic aliases below.</summary>
    internal const int DamageBoostRightFrames = 170;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD48; selected by NormalJumpForwardRightPose and the semantic aliases below.</summary>
    internal const int NormalJumpForwardRightFrames = 128;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD50; selected by NormalJumpForwardLeftPose and the semantic aliases below.</summary>
    internal const int NormalJumpForwardLeftFrames = 130;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E038; selected by KnockbackRightPose and the semantic aliases below.</summary>
    internal const int KnockbackRightFrames = 316;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E040; selected by KnockbackLeftPose and the semantic aliases below.</summary>
    internal const int KnockbackLeftFrames = 318;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD80; selected by NormalJumpTransitionAimUpRightPose and the semantic aliases below.</summary>
    internal const int NormalJumpTransitionAimUpRightFrames = 142;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD84; selected by NormalJumpTransitionAimUpLeftPose and the semantic aliases below.</summary>
    internal const int NormalJumpTransitionAimUpLeftFrames = 143;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD88; selected by NormalJumpTransitionAimDiagonalUpRightPose and the semantic aliases below.</summary>
    internal const int NormalJumpTransitionAimDiagonalUpRightFrames = 144;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD8C; selected by NormalJumpTransitionAimDiagonalUpLeftPose and the semantic aliases below.</summary>
    internal const int NormalJumpTransitionAimDiagonalUpLeftFrames = 145;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD90; selected by NormalJumpTransitionAimDiagonalDownRightPose and the semantic aliases below.</summary>
    internal const int NormalJumpTransitionAimDiagonalDownRightFrames = 146;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD94; selected by NormalJumpTransitionAimDiagonalDownLeftPose and the semantic aliases below.</summary>
    internal const int NormalJumpTransitionAimDiagonalDownLeftFrames = 147;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E04C; selected by UnusedPose5C and the semantic aliases below.</summary>
    internal const int UnusedPose5CFrames = 321;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E050; selected by UnusedPose5D and the semantic aliases below.</summary>
    internal const int UnusedPose5DFrames = 322;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E158; selected by UnusedPose62 and the semantic aliases below.</summary>
    internal const int UnusedPose62Frames = 388;

    /// <summary>The first frame of UNUSED_SamusTilesAnimation_AnimationDefinitions_92E260; selected by UnusedPose63 and the semantic aliases below.</summary>
    internal const int UnusedPose63Frames = 454;

    /// <summary>The first frame of UNUSED_SamusTilesAnimation_AnimationDefinitions_92E268; selected by UnusedPose64 and the semantic aliases below.</summary>
    internal const int UnusedPose64Frames = 456;

    /// <summary>The first frame of UNUSED_SamusTilesAnimation_AnimationDefinitions_92E270; selected by UnusedPose65 and the semantic aliases below.</summary>
    internal const int UnusedPose65Frames = 458;

    /// <summary>The first frame of UNUSED_SamusTilesAnimation_AnimationDefinitions_92E294; selected by UnusedPose66 and the semantic aliases below.</summary>
    internal const int UnusedPose66Frames = 467;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DEC0; selected by FallingGunExtendedRightPose and the semantic aliases below.</summary>
    internal const int FallingGunExtendedRightFrames = 222;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DEDC; selected by FallingGunExtendedLeftPose and the semantic aliases below.</summary>
    internal const int FallingGunExtendedLeftFrames = 229;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD58; selected by NormalJumpAimDiagonalUpRightPose and the semantic aliases below.</summary>
    internal const int NormalJumpAimDiagonalUpRightFrames = 132;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD60; selected by NormalJumpAimDiagonalUpLeftPose and the semantic aliases below.</summary>
    internal const int NormalJumpAimDiagonalUpLeftFrames = 134;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD68; selected by NormalJumpAimDiagonalDownRightPose and the semantic aliases below.</summary>
    internal const int NormalJumpAimDiagonalDownRightFrames = 136;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DD70; selected by NormalJumpAimDiagonalDownLeftPose and the semantic aliases below.</summary>
    internal const int NormalJumpAimDiagonalDownLeftFrames = 138;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DEF8; selected by FallingAimDiagonalUpRightPose and the semantic aliases below.</summary>
    internal const int FallingAimDiagonalUpRightFrames = 236;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DF04; selected by FallingAimDiagonalUpLeftPose and the semantic aliases below.</summary>
    internal const int FallingAimDiagonalUpLeftFrames = 239;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DF10; selected by FallingAimDiagonalDownRightPose and the semantic aliases below.</summary>
    internal const int FallingAimDiagonalDownRightFrames = 242;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DF1C; selected by FallingAimDiagonalDownLeftPose and the semantic aliases below.</summary>
    internal const int FallingAimDiagonalDownLeftFrames = 245;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E430; selected by CrouchingAimDiagonalUpRightPose and the semantic aliases below.</summary>
    internal const int CrouchingAimDiagonalUpRightFrames = 570;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E434; selected by CrouchingAimDiagonalUpLeftPose and the semantic aliases below.</summary>
    internal const int CrouchingAimDiagonalUpLeftFrames = 571;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E438; selected by CrouchingAimDiagonalDownRightPose and the semantic aliases below.</summary>
    internal const int CrouchingAimDiagonalDownRightFrames = 572;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E43C; selected by CrouchingAimDiagonalDownLeftPose and the semantic aliases below.</summary>
    internal const int CrouchingAimDiagonalDownLeftFrames = 573;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E450; selected by MoonwalkAimUpLeftPose and the semantic aliases below.</summary>
    internal const int MoonwalkAimUpLeftFrames = 578;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E468; selected by MoonwalkAimUpRightPose and the semantic aliases below.</summary>
    internal const int MoonwalkAimUpRightFrames = 584;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E480; selected by MoonwalkAimDownLeftPose and the semantic aliases below.</summary>
    internal const int MoonwalkAimDownLeftFrames = 590;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E498; selected by MoonwalkAimDownRightPose and the semantic aliases below.</summary>
    internal const int MoonwalkAimDownRightFrames = 596;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E5A8; selected by SpringBallGroundRightPose and the semantic aliases below.</summary>
    internal const int SpringBallGroundRightFrames = 664;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E5D0; selected by SpringBallGroundLeftPose and the semantic aliases below.</summary>
    internal const int SpringBallGroundLeftFrames = 674;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E6B8; selected by ScrewAttackRightPose and the semantic aliases below.</summary>
    internal const int ScrewAttackRightFrames = 732;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E728; selected by ScrewAttackLeftPose and the semantic aliases below.</summary>
    internal const int ScrewAttackLeftFrames = 760;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E2B8; selected by WallJumpRightPose and the semantic aliases below.</summary>
    internal const int WallJumpRightFrames = 476;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E374; selected by WallJumpLeftPose and the semantic aliases below.</summary>
    internal const int WallJumpLeftFrames = 523;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E440; selected by CrouchingAimUpRightPose and the semantic aliases below.</summary>
    internal const int CrouchingAimUpRightFrames = 574;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E448; selected by CrouchingAimUpLeftPose and the semantic aliases below.</summary>
    internal const int CrouchingAimUpLeftFrames = 576;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E7B0; selected by TurningRightToLeftAimUpPose and the semantic aliases below.</summary>
    internal const int TurningRightToLeftAimUpFrames = 794;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E7BC; selected by TurningLeftToRightAimUpPose and the semantic aliases below.</summary>
    internal const int TurningLeftToRightAimUpFrames = 797;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E7C8; selected by TurningRightToLeftAimDiagonalDownPose and the semantic aliases below.</summary>
    internal const int TurningRightToLeftAimDiagonalDownFrames = 800;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E7D4; selected by TurningLeftToRightAimDiagonalDownPose and the semantic aliases below.</summary>
    internal const int TurningLeftToRightAimDiagonalDownFrames = 803;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E7F8; selected by TurningRightToLeftJumpAimUpPose and the semantic aliases below.</summary>
    internal const int TurningRightToLeftJumpAimUpFrames = 812;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E804; selected by TurningLeftToRightJumpAimUpPose and the semantic aliases below.</summary>
    internal const int TurningLeftToRightJumpAimUpFrames = 815;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E810; selected by TurningRightToLeftJumpAimDownPose and the semantic aliases below.</summary>
    internal const int TurningRightToLeftJumpAimDownFrames = 818;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E81C; selected by TurningLeftToRightJumpAimDownPose and the semantic aliases below.</summary>
    internal const int TurningLeftToRightJumpAimDownFrames = 821;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_EBA4; selected by ForwardFacingSuitedPose and the semantic aliases below.</summary>
    internal const int ForwardFacingSuitedFrames = 1047;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DB90; selected by NormalLandingRightPose and the semantic aliases below.</summary>
    internal const int NormalLandingRightFrames = 18;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DB98; selected by NormalLandingLeftPose and the semantic aliases below.</summary>
    internal const int NormalLandingLeftFrames = 20;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DBA0; selected by SpinLandingRightPose and the semantic aliases below.</summary>
    internal const int SpinLandingRightFrames = 22;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DBAC; selected by SpinLandingLeftPose and the semantic aliases below.</summary>
    internal const int SpinLandingLeftFrames = 25;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E838; selected by DraygonGrabbedNeutralLeftPose and the semantic aliases below.</summary>
    internal const int DraygonGrabbedNeutralLeftFrames = 828;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E83C; selected by DraygonGrabbedAimUpLeftPose and the semantic aliases below.</summary>
    internal const int DraygonGrabbedAimUpLeftFrames = 829;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E840; selected by DraygonGrabbedFiringLeftPose and the semantic aliases below.</summary>
    internal const int DraygonGrabbedFiringLeftFrames = 830;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E844; selected by DraygonGrabbedAimDownLeftPose and the semantic aliases below.</summary>
    internal const int DraygonGrabbedAimDownLeftFrames = 831;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E860; selected by DraygonGrabbedMovingLeftPose and the semantic aliases below.</summary>
    internal const int DraygonGrabbedMovingLeftFrames = 838;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E880; selected by ShinesparkHorizontalRightPose and the semantic aliases below.</summary>
    internal const int ShinesparkHorizontalRightFrames = 846;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E884; selected by ShinesparkHorizontalLeftPose and the semantic aliases below.</summary>
    internal const int ShinesparkHorizontalLeftFrames = 847;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E878; selected by ShinesparkVerticalRightPose and the semantic aliases below.</summary>
    internal const int ShinesparkVerticalRightFrames = 844;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E87C; selected by ShinesparkVerticalLeftPose and the semantic aliases below.</summary>
    internal const int ShinesparkVerticalLeftFrames = 845;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E888; selected by ShinesparkDiagonalRightPose and the semantic aliases below.</summary>
    internal const int ShinesparkDiagonalRightFrames = 848;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E88C; selected by ShinesparkDiagonalLeftPose and the semantic aliases below.</summary>
    internal const int ShinesparkDiagonalLeftFrames = 849;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E890; selected by CrystalFlashRightPose and the semantic aliases below.</summary>
    internal const int CrystalFlashRightFrames = 850;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E8CC; selected by CrystalFlashLeftPose and the semantic aliases below.</summary>
    internal const int CrystalFlashLeftFrames = 865;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DBF8; selected by XrayingStandingRightPose and the semantic aliases below.</summary>
    internal const int XrayingStandingRightFrames = 44;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DC0C; selected by XrayingStandingLeftPose and the semantic aliases below.</summary>
    internal const int XrayingStandingLeftFrames = 49;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E908; selected by DeathSequenceRightPose and the semantic aliases below.</summary>
    internal const int DeathSequenceRightFrames = 880;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E920; selected by DeathSequenceLeftPose and the semantic aliases below.</summary>
    internal const int DeathSequenceLeftFrames = 886;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DC20; selected by XrayingCrouchingRightPose and the semantic aliases below.</summary>
    internal const int XrayingCrouchingRightFrames = 54;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DC34; selected by XrayingCrouchingLeftPose and the semantic aliases below.</summary>
    internal const int XrayingCrouchingLeftFrames = 59;

    /// <summary>The first frame of UNUSED_SamusTilesAnimation_AnimationDefinitions_92E4D8; selected by UnusedPoseDb and the semantic aliases below.</summary>
    internal const int UnusedPoseDbFrames = 612;

    /// <summary>The first frame of UNUSED_SamusTilesAnimation_AnimationDefinitions_92E4E4; selected by UnusedPoseDc and the semantic aliases below.</summary>
    internal const int UnusedPoseDcFrames = 615;

    /// <summary>The first frame of UNUSED_SamusTilesAnimation_AnimationDefinitions_92E4F0; selected by UnusedPoseDd and the semantic aliases below.</summary>
    internal const int UnusedPoseDdFrames = 618;

    /// <summary>The first frame of UNUSED_SamusTilesAnimation_AnimationDefinitions_92E4FC; selected by UnusedPoseDe and the semantic aliases below.</summary>
    internal const int UnusedPoseDeFrames = 621;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DBB8; selected by LandingAimUpRightPose and the semantic aliases below.</summary>
    internal const int LandingAimUpRightFrames = 28;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DBC0; selected by LandingAimUpLeftPose and the semantic aliases below.</summary>
    internal const int LandingAimUpLeftFrames = 30;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DBC8; selected by LandingAimDiagonalUpRightPose and the semantic aliases below.</summary>
    internal const int LandingAimDiagonalUpRightFrames = 32;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DBD0; selected by LandingAimDiagonalUpLeftPose and the semantic aliases below.</summary>
    internal const int LandingAimDiagonalUpLeftFrames = 34;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DBD8; selected by LandingAimDiagonalDownRightPose and the semantic aliases below.</summary>
    internal const int LandingAimDiagonalDownRightFrames = 36;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DBE0; selected by LandingAimDiagonalDownLeftPose and the semantic aliases below.</summary>
    internal const int LandingAimDiagonalDownLeftFrames = 38;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DBE8; selected by FiringLandingRightPose and the semantic aliases below.</summary>
    internal const int FiringLandingRightFrames = 40;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_DBF0; selected by FiringLandingLeftPose and the semantic aliases below.</summary>
    internal const int FiringLandingLeftFrames = 42;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E938; selected by DrainedCrouchingRightPose and the semantic aliases below.</summary>
    internal const int DrainedCrouchingRightFrames = 892;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E974; selected by DrainedCrouchingLeftPose and the semantic aliases below.</summary>
    internal const int DrainedCrouchingLeftFrames = 907;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E9F4; selected by DrainedStandingRightPose and the semantic aliases below.</summary>
    internal const int DrainedStandingRightFrames = 939;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_EA0C; selected by DrainedStandingLeftPose and the semantic aliases below.</summary>
    internal const int DrainedStandingLeftFrames = 945;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E828; selected by DraygonGrabbedNeutralRightPose and the semantic aliases below.</summary>
    internal const int DraygonGrabbedNeutralRightFrames = 824;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E82C; selected by DraygonGrabbedAimUpRightPose and the semantic aliases below.</summary>
    internal const int DraygonGrabbedAimUpRightFrames = 825;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E830; selected by DraygonGrabbedFiringRightPose and the semantic aliases below.</summary>
    internal const int DraygonGrabbedFiringRightFrames = 826;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E834; selected by DraygonGrabbedAimDownRightPose and the semantic aliases below.</summary>
    internal const int DraygonGrabbedAimDownRightFrames = 827;

    /// <summary>The first frame of SamusTilesAnimation_AnimationDefinitions_E848; selected by DraygonGrabbedMovingRightPose and the semantic aliases below.</summary>
    internal const int DraygonGrabbedMovingRightFrames = 832;

    /// <summary>The value each key selects; DefaultFirstFrame looks it up.</summary>
    internal static readonly IReadOnlyDictionary<SamusPoseId, int> DefaultFirstFrames = new Dictionary<SamusPoseId, int>
    {
        [SamusPoseId.ForwardFacingPowerSuitPose] = ForwardFacingPowerSuitFrames,
        [SamusPoseId.FacingRightNormalPose] = FacingRightNormalFrames,
        [SamusPoseId.UnusedPose47] = FacingRightNormalFrames,
        [SamusPoseId.RanIntoWallRightPose] = FacingRightNormalFrames,
        [SamusPoseId.GrappleStandingRightPose] = FacingRightNormalFrames,
        [SamusPoseId.FacingLeftNormalPose] = FacingLeftNormalFrames,
        [SamusPoseId.UnusedPose48] = FacingLeftNormalFrames,
        [SamusPoseId.RanIntoWallLeftPose] = FacingLeftNormalFrames,
        [SamusPoseId.GrappleStandingLeftPose] = FacingLeftNormalFrames,
        [SamusPoseId.StandingAimUpRightPose] = StandingAimUpRightFrames,
        [SamusPoseId.StandingAimUpLeftPose] = StandingAimUpLeftFrames,
        [SamusPoseId.StandingAimDiagonalUpRightPose] = StandingAimDiagonalUpRightFrames,
        [SamusPoseId.RanIntoWallAimUpRightPose] = StandingAimDiagonalUpRightFrames,
        [SamusPoseId.StandingAimDiagonalUpLeftPose] = StandingAimDiagonalUpLeftFrames,
        [SamusPoseId.RanIntoWallAimUpLeftPose] = StandingAimDiagonalUpLeftFrames,
        [SamusPoseId.StandingAimDiagonalDownRightPose] = StandingAimDiagonalDownRightFrames,
        [SamusPoseId.GrappleStandingDownRightPose] = StandingAimDiagonalDownRightFrames,
        [SamusPoseId.RanIntoWallAimDownRightPose] = StandingAimDiagonalDownRightFrames,
        [SamusPoseId.StandingAimDiagonalDownLeftPose] = StandingAimDiagonalDownLeftFrames,
        [SamusPoseId.GrappleStandingDownLeftPose] = StandingAimDiagonalDownLeftFrames,
        [SamusPoseId.RanIntoWallAimDownLeftPose] = StandingAimDiagonalDownLeftFrames,
        [SamusPoseId.MovingRightNormalPose] = MovingRightNormalFrames,
        [SamusPoseId.MovingLeftNormalPose] = MovingLeftNormalFrames,
        [SamusPoseId.MovingRightGunExtendedPose] = MovingRightGunExtendedFrames,
        [SamusPoseId.MovingLeftGunExtendedPose] = MovingLeftGunExtendedFrames,
        [SamusPoseId.RunningAimUpRightPose] = RunningAimUpRightFrames,
        [SamusPoseId.RunningAimUpLeftPose] = RunningAimUpLeftFrames,
        [SamusPoseId.RunningAimDiagonalUpRightPose] = RunningAimDiagonalUpRightFrames,
        [SamusPoseId.RunningAimDiagonalUpLeftPose] = RunningAimDiagonalUpLeftFrames,
        [SamusPoseId.RunningAimDiagonalDownRightPose] = RunningAimDiagonalDownRightFrames,
        [SamusPoseId.RunningAimDiagonalDownLeftPose] = RunningAimDiagonalDownLeftFrames,
        [SamusPoseId.NormalJumpGunExtendedRightPose] = NormalJumpGunExtendedRightFrames,
        [SamusPoseId.UnusedPoseAC] = NormalJumpGunExtendedRightFrames,
        [SamusPoseId.NormalJumpGunExtendedLeftPose] = NormalJumpGunExtendedLeftFrames,
        [SamusPoseId.UnusedPoseAD] = NormalJumpGunExtendedLeftFrames,
        [SamusPoseId.NormalJumpAimUpRightPose] = NormalJumpAimUpRightFrames,
        [SamusPoseId.NormalJumpAimUpLeftPose] = NormalJumpAimUpLeftFrames,
        [SamusPoseId.NormalJumpAimDownRightPose] = NormalJumpAimDownRightFrames,
        [SamusPoseId.UnusedPoseAE] = NormalJumpAimDownRightFrames,
        [SamusPoseId.NormalJumpAimDownLeftPose] = NormalJumpAimDownLeftFrames,
        [SamusPoseId.UnusedPoseAF] = NormalJumpAimDownLeftFrames,
        [SamusPoseId.SpinJumpRightPose] = SpinJumpRightFrames,
        [SamusPoseId.UnusedPose20] = SpinJumpRightFrames,
        [SamusPoseId.UnusedPose21] = SpinJumpRightFrames,
        [SamusPoseId.UnusedPose22] = SpinJumpRightFrames,
        [SamusPoseId.UnusedPose23] = SpinJumpRightFrames,
        [SamusPoseId.UnusedPose24] = SpinJumpRightFrames,
        [SamusPoseId.UnusedKnockbackRightPose] = SpinJumpRightFrames,
        [SamusPoseId.UnusedKnockbackLeftPose] = SpinJumpRightFrames,
        [SamusPoseId.UnusedPose39] = SpinJumpRightFrames,
        [SamusPoseId.UnusedPose3A] = SpinJumpRightFrames,
        [SamusPoseId.UnusedPose42] = SpinJumpRightFrames,
        [SamusPoseId.SpinJumpLeftPose] = SpinJumpLeftFrames,
        [SamusPoseId.SpaceJumpRightPose] = SpaceJumpRightFrames,
        [SamusPoseId.SpaceJumpLeftPose] = SpaceJumpLeftFrames,
        [SamusPoseId.MorphBallGroundRightPose] = MorphBallGroundRightFrames,
        [SamusPoseId.MorphBallFallingRightPose] = MorphBallGroundRightFrames,
        [SamusPoseId.MorphBallFallingLeftPose] = MorphBallGroundRightFrames,
        [SamusPoseId.UnusedPose3F] = MorphBallGroundRightFrames,
        [SamusPoseId.UnusedPose40] = MorphBallGroundRightFrames,
        [SamusPoseId.MorphBallMovingRightPose] = MorphBallMovingRightFrames,
        [SamusPoseId.MorphBallMovingLeftPose] = MorphBallMovingLeftFrames,
        [SamusPoseId.TurningRightToLeftPose] = TurningRightToLeftFrames,
        [SamusPoseId.MoonwalkTurnJumpLeftPose] = TurningRightToLeftFrames,
        [SamusPoseId.UnusedPoseC6] = TurningRightToLeftFrames,
        [SamusPoseId.TurningLeftToRightPose] = TurningLeftToRightFrames,
        [SamusPoseId.MoonwalkTurnJumpRightPose] = TurningLeftToRightFrames,
        [SamusPoseId.CrouchingRightPose] = CrouchingRightFrames,
        [SamusPoseId.GrappleCrouchingRightPose] = CrouchingRightFrames,
        [SamusPoseId.CrouchingLeftPose] = CrouchingLeftFrames,
        [SamusPoseId.GrappleCrouchingLeftPose] = CrouchingLeftFrames,
        [SamusPoseId.FallingRightPose] = FallingRightFrames,
        [SamusPoseId.FallingLeftPose] = FallingLeftFrames,
        [SamusPoseId.FallingAimUpRightPose] = FallingAimUpRightFrames,
        [SamusPoseId.FallingAimUpLeftPose] = FallingAimUpLeftFrames,
        [SamusPoseId.FallingAimDownRightPose] = FallingAimDownRightFrames,
        [SamusPoseId.FallingAimDownLeftPose] = FallingAimDownLeftFrames,
        [SamusPoseId.TurningRightToLeftJumpPose] = TurningRightToLeftJumpFrames,
        [SamusPoseId.TurningRightToLeftCrouchingPose] = TurningRightToLeftJumpFrames,
        [SamusPoseId.TurningRightToLeftFallingPose] = TurningRightToLeftJumpFrames,
        [SamusPoseId.TurningLeftToRightJumpPose] = TurningLeftToRightJumpFrames,
        [SamusPoseId.TurningLeftToRightCrouchingPose] = TurningLeftToRightJumpFrames,
        [SamusPoseId.TurningLeftToRightFallingPose] = TurningLeftToRightJumpFrames,
        [SamusPoseId.CrouchingTransitionRightPose] = CrouchingTransitionRightFrames,
        [SamusPoseId.StandingTransitionRightPose] = CrouchingTransitionRightFrames,
        [SamusPoseId.CrouchingTransitionLeftPose] = CrouchingTransitionLeftFrames,
        [SamusPoseId.StandingTransitionLeftPose] = CrouchingTransitionLeftFrames,
        [SamusPoseId.MorphingTransitionRightPose] = MorphingTransitionRightFrames,
        [SamusPoseId.MorphingTransitionLeftPose] = MorphingTransitionLeftFrames,
        [SamusPoseId.UnmorphingTransitionRightPose] = UnmorphingTransitionRightFrames,
        [SamusPoseId.UnmorphingTransitionLeftPose] = UnmorphingTransitionLeftFrames,
        [SamusPoseId.MorphBallGroundLeftPose] = MorphBallGroundLeftFrames,
        [SamusPoseId.UnusedPoseC5] = MorphBallGroundLeftFrames,
        [SamusPoseId.UnusedPoseDf] = MorphBallGroundLeftFrames,
        [SamusPoseId.UnusedPose45] = UnusedPose45Frames,
        [SamusPoseId.UnusedPose46] = UnusedPose45Frames,
        [SamusPoseId.UnusedPose5B] = UnusedPose45Frames,
        [SamusPoseId.GrappleWallContactLeftPose] = UnusedPose45Frames,
        [SamusPoseId.MoonwalkFacingLeftPose] = MoonwalkFacingLeftFrames,
        [SamusPoseId.MoonwalkFacingRightPose] = MoonwalkFacingRightFrames,
        [SamusPoseId.NeutralJumpTransitionRightPose] = NeutralJumpTransitionRightFrames,
        [SamusPoseId.NeutralJumpTransitionLeftPose] = NeutralJumpTransitionLeftFrames,
        [SamusPoseId.NeutralJumpRightPose] = NeutralJumpRightFrames,
        [SamusPoseId.ShinesparkWindupRightPose] = NeutralJumpRightFrames,
        [SamusPoseId.NeutralJumpLeftPose] = NeutralJumpLeftFrames,
        [SamusPoseId.ShinesparkWindupLeftPose] = NeutralJumpLeftFrames,
        [SamusPoseId.DamageBoostLeftPose] = DamageBoostLeftFrames,
        [SamusPoseId.DamageBoostRightPose] = DamageBoostRightFrames,
        [SamusPoseId.NormalJumpForwardRightPose] = NormalJumpForwardRightFrames,
        [SamusPoseId.NormalJumpForwardLeftPose] = NormalJumpForwardLeftFrames,
        [SamusPoseId.KnockbackRightPose] = KnockbackRightFrames,
        [SamusPoseId.KnockbackLeftPose] = KnockbackLeftFrames,
        [SamusPoseId.NormalJumpTransitionAimUpRightPose] = NormalJumpTransitionAimUpRightFrames,
        [SamusPoseId.CrouchingTransitionAimUpRightPose] = NormalJumpTransitionAimUpRightFrames,
        [SamusPoseId.StandingTransitionAimUpRightPose] = NormalJumpTransitionAimUpRightFrames,
        [SamusPoseId.NormalJumpTransitionAimUpLeftPose] = NormalJumpTransitionAimUpLeftFrames,
        [SamusPoseId.CrouchingTransitionAimUpLeftPose] = NormalJumpTransitionAimUpLeftFrames,
        [SamusPoseId.StandingTransitionAimUpLeftPose] = NormalJumpTransitionAimUpLeftFrames,
        [SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose] = NormalJumpTransitionAimDiagonalUpRightFrames,
        [SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose] = NormalJumpTransitionAimDiagonalUpRightFrames,
        [SamusPoseId.StandingTransitionAimDiagonalUpRightPose] = NormalJumpTransitionAimDiagonalUpRightFrames,
        [SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose] = NormalJumpTransitionAimDiagonalUpLeftFrames,
        [SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose] = NormalJumpTransitionAimDiagonalUpLeftFrames,
        [SamusPoseId.StandingTransitionAimDiagonalUpLeftPose] = NormalJumpTransitionAimDiagonalUpLeftFrames,
        [SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose] = NormalJumpTransitionAimDiagonalDownRightFrames,
        [SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose] = NormalJumpTransitionAimDiagonalDownRightFrames,
        [SamusPoseId.StandingTransitionAimDiagonalDownRightPose] = NormalJumpTransitionAimDiagonalDownRightFrames,
        [SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose] = NormalJumpTransitionAimDiagonalDownLeftFrames,
        [SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose] = NormalJumpTransitionAimDiagonalDownLeftFrames,
        [SamusPoseId.StandingTransitionAimDiagonalDownLeftPose] = NormalJumpTransitionAimDiagonalDownLeftFrames,
        [SamusPoseId.UnusedPose5C] = UnusedPose5CFrames,
        [SamusPoseId.GrappleWallContactRightPose] = UnusedPose5CFrames,
        [SamusPoseId.UnusedPose5D] = UnusedPose5DFrames,
        [SamusPoseId.UnusedPose5E] = UnusedPose5DFrames,
        [SamusPoseId.UnusedPose5F] = UnusedPose5DFrames,
        [SamusPoseId.UnusedPose60] = UnusedPose5DFrames,
        [SamusPoseId.UnusedPose61] = UnusedPose5DFrames,
        [SamusPoseId.GrappleSwingRightPose] = UnusedPose5DFrames,
        [SamusPoseId.UnusedPose62] = UnusedPose62Frames,
        [SamusPoseId.GrappleSwingLeftPose] = UnusedPose62Frames,
        [SamusPoseId.UnusedPose63] = UnusedPose63Frames,
        [SamusPoseId.UnusedPose64] = UnusedPose64Frames,
        [SamusPoseId.UnusedPose65] = UnusedPose65Frames,
        [SamusPoseId.UnusedPose66] = UnusedPose66Frames,
        [SamusPoseId.FallingGunExtendedRightPose] = FallingGunExtendedRightFrames,
        [SamusPoseId.FallingGunExtendedLeftPose] = FallingGunExtendedLeftFrames,
        [SamusPoseId.NormalJumpAimDiagonalUpRightPose] = NormalJumpAimDiagonalUpRightFrames,
        [SamusPoseId.NormalJumpAimDiagonalUpLeftPose] = NormalJumpAimDiagonalUpLeftFrames,
        [SamusPoseId.NormalJumpAimDiagonalDownRightPose] = NormalJumpAimDiagonalDownRightFrames,
        [SamusPoseId.UnusedPoseB0] = NormalJumpAimDiagonalDownRightFrames,
        [SamusPoseId.NormalJumpAimDiagonalDownLeftPose] = NormalJumpAimDiagonalDownLeftFrames,
        [SamusPoseId.UnusedPoseB1] = NormalJumpAimDiagonalDownLeftFrames,
        [SamusPoseId.FallingAimDiagonalUpRightPose] = FallingAimDiagonalUpRightFrames,
        [SamusPoseId.FallingAimDiagonalUpLeftPose] = FallingAimDiagonalUpLeftFrames,
        [SamusPoseId.FallingAimDiagonalDownRightPose] = FallingAimDiagonalDownRightFrames,
        [SamusPoseId.FallingAimDiagonalDownLeftPose] = FallingAimDiagonalDownLeftFrames,
        [SamusPoseId.CrouchingAimDiagonalUpRightPose] = CrouchingAimDiagonalUpRightFrames,
        [SamusPoseId.CrouchingAimDiagonalUpLeftPose] = CrouchingAimDiagonalUpLeftFrames,
        [SamusPoseId.CrouchingAimDiagonalDownRightPose] = CrouchingAimDiagonalDownRightFrames,
        [SamusPoseId.GrappleCrouchingDownRightPose] = CrouchingAimDiagonalDownRightFrames,
        [SamusPoseId.CrouchingAimDiagonalDownLeftPose] = CrouchingAimDiagonalDownLeftFrames,
        [SamusPoseId.GrappleCrouchingDownLeftPose] = CrouchingAimDiagonalDownLeftFrames,
        [SamusPoseId.MoonwalkAimUpLeftPose] = MoonwalkAimUpLeftFrames,
        [SamusPoseId.MoonwalkAimUpRightPose] = MoonwalkAimUpRightFrames,
        [SamusPoseId.MoonwalkAimDownLeftPose] = MoonwalkAimDownLeftFrames,
        [SamusPoseId.MoonwalkAimDownRightPose] = MoonwalkAimDownRightFrames,
        [SamusPoseId.SpringBallGroundRightPose] = SpringBallGroundRightFrames,
        [SamusPoseId.SpringBallMovingRightPose] = SpringBallGroundRightFrames,
        [SamusPoseId.SpringBallFallingRightPose] = SpringBallGroundRightFrames,
        [SamusPoseId.SpringBallJumpRightPose] = SpringBallGroundRightFrames,
        [SamusPoseId.SpringBallGroundLeftPose] = SpringBallGroundLeftFrames,
        [SamusPoseId.SpringBallMovingLeftPose] = SpringBallGroundLeftFrames,
        [SamusPoseId.SpringBallFallingLeftPose] = SpringBallGroundLeftFrames,
        [SamusPoseId.SpringBallJumpLeftPose] = SpringBallGroundLeftFrames,
        [SamusPoseId.ScrewAttackRightPose] = ScrewAttackRightFrames,
        [SamusPoseId.ScrewAttackLeftPose] = ScrewAttackLeftFrames,
        [SamusPoseId.WallJumpRightPose] = WallJumpRightFrames,
        [SamusPoseId.WallJumpLeftPose] = WallJumpLeftFrames,
        [SamusPoseId.CrouchingAimUpRightPose] = CrouchingAimUpRightFrames,
        [SamusPoseId.CrouchingAimUpLeftPose] = CrouchingAimUpLeftFrames,
        [SamusPoseId.TurningRightToLeftAimUpPose] = TurningRightToLeftAimUpFrames,
        [SamusPoseId.TurningRightToLeftAimDiagonalUpPose] = TurningRightToLeftAimUpFrames,
        [SamusPoseId.MoonwalkTurnJumpAimUpLeftPose] = TurningRightToLeftAimUpFrames,
        [SamusPoseId.TurningLeftToRightAimUpPose] = TurningLeftToRightAimUpFrames,
        [SamusPoseId.TurningLeftToRightAimDiagonalUpPose] = TurningLeftToRightAimUpFrames,
        [SamusPoseId.MoonwalkTurnJumpAimUpRightPose] = TurningLeftToRightAimUpFrames,
        [SamusPoseId.TurningRightToLeftAimDiagonalDownPose] = TurningRightToLeftAimDiagonalDownFrames,
        [SamusPoseId.MoonwalkTurnJumpAimDownLeftPose] = TurningRightToLeftAimDiagonalDownFrames,
        [SamusPoseId.TurningLeftToRightAimDiagonalDownPose] = TurningLeftToRightAimDiagonalDownFrames,
        [SamusPoseId.MoonwalkTurnJumpAimDownRightPose] = TurningLeftToRightAimDiagonalDownFrames,
        [SamusPoseId.TurningRightToLeftJumpAimUpPose] = TurningRightToLeftJumpAimUpFrames,
        [SamusPoseId.TurningRightToLeftFallingAimUpPose] = TurningRightToLeftJumpAimUpFrames,
        [SamusPoseId.TurningRightToLeftCrouchingAimUpPose] = TurningRightToLeftJumpAimUpFrames,
        [SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose] = TurningRightToLeftJumpAimUpFrames,
        [SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose] = TurningRightToLeftJumpAimUpFrames,
        [SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose] = TurningRightToLeftJumpAimUpFrames,
        [SamusPoseId.TurningLeftToRightJumpAimUpPose] = TurningLeftToRightJumpAimUpFrames,
        [SamusPoseId.TurningLeftToRightFallingAimUpPose] = TurningLeftToRightJumpAimUpFrames,
        [SamusPoseId.TurningLeftToRightCrouchingAimUpPose] = TurningLeftToRightJumpAimUpFrames,
        [SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose] = TurningLeftToRightJumpAimUpFrames,
        [SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose] = TurningLeftToRightJumpAimUpFrames,
        [SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose] = TurningLeftToRightJumpAimUpFrames,
        [SamusPoseId.TurningRightToLeftJumpAimDownPose] = TurningRightToLeftJumpAimDownFrames,
        [SamusPoseId.TurningRightToLeftFallingAimDownPose] = TurningRightToLeftJumpAimDownFrames,
        [SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose] = TurningRightToLeftJumpAimDownFrames,
        [SamusPoseId.TurningLeftToRightJumpAimDownPose] = TurningLeftToRightJumpAimDownFrames,
        [SamusPoseId.TurningLeftToRightFallingAimDownPose] = TurningLeftToRightJumpAimDownFrames,
        [SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose] = TurningLeftToRightJumpAimDownFrames,
        [SamusPoseId.ForwardFacingSuitedPose] = ForwardFacingSuitedFrames,
        [SamusPoseId.NormalLandingRightPose] = NormalLandingRightFrames,
        [SamusPoseId.NormalLandingLeftPose] = NormalLandingLeftFrames,
        [SamusPoseId.SpinLandingRightPose] = SpinLandingRightFrames,
        [SamusPoseId.SpinLandingLeftPose] = SpinLandingLeftFrames,
        [SamusPoseId.DraygonGrabbedNeutralLeftPose] = DraygonGrabbedNeutralLeftFrames,
        [SamusPoseId.DraygonGrabbedAimUpLeftPose] = DraygonGrabbedAimUpLeftFrames,
        [SamusPoseId.DraygonGrabbedFiringLeftPose] = DraygonGrabbedFiringLeftFrames,
        [SamusPoseId.DraygonGrabbedAimDownLeftPose] = DraygonGrabbedAimDownLeftFrames,
        [SamusPoseId.DraygonGrabbedMovingLeftPose] = DraygonGrabbedMovingLeftFrames,
        [SamusPoseId.ShinesparkHorizontalRightPose] = ShinesparkHorizontalRightFrames,
        [SamusPoseId.ShinesparkHorizontalLeftPose] = ShinesparkHorizontalLeftFrames,
        [SamusPoseId.ShinesparkVerticalRightPose] = ShinesparkVerticalRightFrames,
        [SamusPoseId.ShinesparkVerticalLeftPose] = ShinesparkVerticalLeftFrames,
        [SamusPoseId.ShinesparkDiagonalRightPose] = ShinesparkDiagonalRightFrames,
        [SamusPoseId.ShinesparkDiagonalLeftPose] = ShinesparkDiagonalLeftFrames,
        [SamusPoseId.CrystalFlashRightPose] = CrystalFlashRightFrames,
        [SamusPoseId.CrystalFlashLeftPose] = CrystalFlashLeftFrames,
        [SamusPoseId.XrayingStandingRightPose] = XrayingStandingRightFrames,
        [SamusPoseId.XrayingStandingLeftPose] = XrayingStandingLeftFrames,
        [SamusPoseId.DeathSequenceRightPose] = DeathSequenceRightFrames,
        [SamusPoseId.DeathSequenceLeftPose] = DeathSequenceLeftFrames,
        [SamusPoseId.XrayingCrouchingRightPose] = XrayingCrouchingRightFrames,
        [SamusPoseId.XrayingCrouchingLeftPose] = XrayingCrouchingLeftFrames,
        [SamusPoseId.UnusedPoseDb] = UnusedPoseDbFrames,
        [SamusPoseId.UnusedPoseDc] = UnusedPoseDcFrames,
        [SamusPoseId.UnusedPoseDd] = UnusedPoseDdFrames,
        [SamusPoseId.UnusedPoseDe] = UnusedPoseDeFrames,
        [SamusPoseId.LandingAimUpRightPose] = LandingAimUpRightFrames,
        [SamusPoseId.LandingAimUpLeftPose] = LandingAimUpLeftFrames,
        [SamusPoseId.LandingAimDiagonalUpRightPose] = LandingAimDiagonalUpRightFrames,
        [SamusPoseId.LandingAimDiagonalUpLeftPose] = LandingAimDiagonalUpLeftFrames,
        [SamusPoseId.LandingAimDiagonalDownRightPose] = LandingAimDiagonalDownRightFrames,
        [SamusPoseId.LandingAimDiagonalDownLeftPose] = LandingAimDiagonalDownLeftFrames,
        [SamusPoseId.FiringLandingRightPose] = FiringLandingRightFrames,
        [SamusPoseId.FiringLandingLeftPose] = FiringLandingLeftFrames,
        [SamusPoseId.DrainedCrouchingRightPose] = DrainedCrouchingRightFrames,
        [SamusPoseId.DrainedCrouchingLeftPose] = DrainedCrouchingLeftFrames,
        [SamusPoseId.DrainedStandingRightPose] = DrainedStandingRightFrames,
        [SamusPoseId.DrainedStandingLeftPose] = DrainedStandingLeftFrames,
        [SamusPoseId.DraygonGrabbedNeutralRightPose] = DraygonGrabbedNeutralRightFrames,
        [SamusPoseId.DraygonGrabbedAimUpRightPose] = DraygonGrabbedAimUpRightFrames,
        [SamusPoseId.DraygonGrabbedFiringRightPose] = DraygonGrabbedFiringRightFrames,
        [SamusPoseId.DraygonGrabbedAimDownRightPose] = DraygonGrabbedAimDownRightFrames,
        [SamusPoseId.DraygonGrabbedMovingRightPose] = DraygonGrabbedMovingRightFrames,
    };
}
