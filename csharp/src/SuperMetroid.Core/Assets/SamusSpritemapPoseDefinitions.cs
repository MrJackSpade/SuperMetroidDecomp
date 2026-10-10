using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Semantic real-pose dispatch to native OAM frame-sequence identities; pointer and part payloads remain separately required.</summary>
internal static class SamusSpritemapPoseDefinitions
{
    /// <summary>$92:9263 selects top frame-sequence index$0002 for ForwardFacingPowerSuitPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopForwardFacingPowerSuitBase = 0x0002;
    /// <summary>$92:9265 selects top frame-sequence index$019A for FacingRightNormalPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFacingRightNormalBase = 0x019a;
    /// <summary>$92:9267 selects top frame-sequence index$01A3 for FacingLeftNormalPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFacingLeftNormalBase = 0x01a3;
    /// <summary>$92:9269 selects top frame-sequence index$01AD for StandingAimUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopStandingAimUpRightBase = 0x01ad;
    /// <summary>$92:926B selects top frame-sequence index$01AF for StandingAimUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopStandingAimUpLeftBase = 0x01af;
    /// <summary>$92:926D selects top frame-sequence index$01B1 for StandingAimDiagonalUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopStandingAimDiagonalUpRightBase = 0x01b1;
    /// <summary>$92:926F selects top frame-sequence index$01B3 for StandingAimDiagonalUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopStandingAimDiagonalUpLeftBase = 0x01b3;
    /// <summary>$92:9271 selects top frame-sequence index$01B5 for StandingAimDiagonalDownRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopStandingAimDiagonalDownRightBase = 0x01b5;
    /// <summary>$92:9273 selects top frame-sequence index$01B7 for StandingAimDiagonalDownLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopStandingAimDiagonalDownLeftBase = 0x01b7;
    /// <summary>$92:9275 selects top frame-sequence index$01F9 for MovingRightNormalPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMovingRightNormalBase = 0x01f9;
    /// <summary>$92:9277 selects top frame-sequence index$0203 for MovingLeftNormalPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMovingLeftNormalBase = 0x0203;
    /// <summary>$92:9279 selects top frame-sequence index$020D for MovingRightGunExtendedPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMovingRightGunExtendedBase = 0x020d;
    /// <summary>$92:927B selects top frame-sequence index$0217 for MovingLeftGunExtendedPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMovingLeftGunExtendedBase = 0x0217;
    /// <summary>$92:927D selects top frame-sequence index$0221 for RunningAimUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopRunningAimUpRightBase = 0x0221;
    /// <summary>$92:927F selects top frame-sequence index$022B for RunningAimUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopRunningAimUpLeftBase = 0x022b;
    /// <summary>$92:9281 selects top frame-sequence index$0235 for RunningAimDiagonalUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopRunningAimDiagonalUpRightBase = 0x0235;
    /// <summary>$92:9283 selects top frame-sequence index$023F for RunningAimDiagonalUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopRunningAimDiagonalUpLeftBase = 0x023f;
    /// <summary>$92:9285 selects top frame-sequence index$0249 for RunningAimDiagonalDownRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopRunningAimDiagonalDownRightBase = 0x0249;
    /// <summary>$92:9287 selects top frame-sequence index$0253 for RunningAimDiagonalDownLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopRunningAimDiagonalDownLeftBase = 0x0253;
    /// <summary>$92:9289 selects top frame-sequence index$0275 for NormalJumpGunExtendedRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalJumpGunExtendedRightBase = 0x0275;
    /// <summary>$92:928B selects top frame-sequence index$0277 for NormalJumpGunExtendedLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalJumpGunExtendedLeftBase = 0x0277;
    /// <summary>$92:928D selects top frame-sequence index$0279 for NormalJumpAimUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalJumpAimUpRightBase = 0x0279;
    /// <summary>$92:928F selects top frame-sequence index$027B for NormalJumpAimUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalJumpAimUpLeftBase = 0x027b;
    /// <summary>$92:9291 selects top frame-sequence index$0271 for NormalJumpAimDownRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalJumpAimDownRightBase = 0x0271;
    /// <summary>$92:9293 selects top frame-sequence index$0273 for NormalJumpAimDownLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalJumpAimDownLeftBase = 0x0273;
    /// <summary>$92:9295 selects top frame-sequence index$074C for SpinJumpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopSpinJumpRightBase = 0x074c;
    /// <summary>$92:9297 selects top frame-sequence index$0764 for SpinJumpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopSpinJumpLeftBase = 0x0764;
    /// <summary>$92:9299 selects top frame-sequence index$077C for SpaceJumpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopSpaceJumpRightBase = 0x077c;
    /// <summary>$92:929B selects top frame-sequence index$0794 for SpaceJumpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopSpaceJumpLeftBase = 0x0794;
    /// <summary>$92:929D selects top frame-sequence index$0710 for MorphBallGroundRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMorphBallGroundRightBase = 0x0710;
    /// <summary>$92:929F selects top frame-sequence index$0724 for MorphBallMovingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMorphBallMovingRightBase = 0x0724;
    /// <summary>$92:92A1 selects top frame-sequence index$072E for MorphBallMovingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMorphBallMovingLeftBase = 0x072e;
    /// <summary>$92:92AD selects top frame-sequence index$0419 for TurningRightToLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopTurningRightToLeftBase = 0x0419;
    /// <summary>$92:92AF selects top frame-sequence index$041C for TurningLeftToRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopTurningLeftToRightBase = 0x041c;
    /// <summary>$92:92B1 selects top frame-sequence index$03F1 for CrouchingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopCrouchingRightBase = 0x03f1;
    /// <summary>$92:92B3 selects top frame-sequence index$03FA for CrouchingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopCrouchingLeftBase = 0x03fa;
    /// <summary>$92:92B5 selects top frame-sequence index$03B7 for FallingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFallingRightBase = 0x03b7;
    /// <summary>$92:92B7 selects top frame-sequence index$03BE for FallingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFallingLeftBase = 0x03be;
    /// <summary>$92:92B9 selects top frame-sequence index$03D3 for FallingAimUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFallingAimUpRightBase = 0x03d3;
    /// <summary>$92:92BB selects top frame-sequence index$03D6 for FallingAimUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFallingAimUpLeftBase = 0x03d6;
    /// <summary>$92:92BD selects top frame-sequence index$03D9 for FallingAimDownRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFallingAimDownRightBase = 0x03d9;
    /// <summary>$92:92BF selects top frame-sequence index$03DB for FallingAimDownLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFallingAimDownLeftBase = 0x03db;
    /// <summary>$92:92CD selects top frame-sequence index$0403 for CrouchingTransitionRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopCrouchingTransitionRightBase = 0x0403;
    /// <summary>$92:92CF selects top frame-sequence index$0404 for CrouchingTransitionLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopCrouchingTransitionLeftBase = 0x0404;
    /// <summary>$92:92D1 selects top frame-sequence index$0405 for MorphingTransitionRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMorphingTransitionRightBase = 0x0405;
    /// <summary>$92:92D3 selects top frame-sequence index$0407 for MorphingTransitionLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMorphingTransitionLeftBase = 0x0407;
    /// <summary>$92:92DD selects top frame-sequence index$0409 for UnmorphingTransitionRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnmorphingTransitionRightBase = 0x0409;
    /// <summary>$92:92DF selects top frame-sequence index$040B for UnmorphingTransitionLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnmorphingTransitionLeftBase = 0x040b;
    /// <summary>$92:92E5 selects top frame-sequence index$071A for MorphBallGroundLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMorphBallGroundLeftBase = 0x071a;
    /// <summary>$92:92ED selects top frame-sequence index$025D for UnusedPose45 and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPose45Base = 0x025d;
    /// <summary>$92:92EF selects top frame-sequence index$0267 for UnusedPose46 and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPose46Base = 0x0267;
    /// <summary>$92:92F5 selects top frame-sequence index$01D5 for MoonwalkFacingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMoonwalkFacingLeftBase = 0x01d5;
    /// <summary>$92:92F7 selects top frame-sequence index$01DB for MoonwalkFacingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMoonwalkFacingRightBase = 0x01db;
    /// <summary>$92:92F9 selects top frame-sequence index$0289 for NeutralJumpTransitionRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNeutralJumpTransitionRightBase = 0x0289;
    /// <summary>$92:92FB selects top frame-sequence index$028A for NeutralJumpTransitionLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNeutralJumpTransitionLeftBase = 0x028a;
    /// <summary>$92:92FD selects top frame-sequence index$028B for NeutralJumpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNeutralJumpRightBase = 0x028b;
    /// <summary>$92:92FF selects top frame-sequence index$0291 for NeutralJumpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNeutralJumpLeftBase = 0x0291;
    /// <summary>$92:9301 selects top frame-sequence index$0297 for DamageBoostLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDamageBoostLeftBase = 0x0297;
    /// <summary>$92:9303 selects top frame-sequence index$02A1 for DamageBoostRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDamageBoostRightBase = 0x02a1;
    /// <summary>$92:9305 selects top frame-sequence index$0285 for NormalJumpForwardRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalJumpForwardRightBase = 0x0285;
    /// <summary>$92:9307 selects top frame-sequence index$0287 for NormalJumpForwardLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalJumpForwardLeftBase = 0x0287;
    /// <summary>$92:9309 selects top frame-sequence index$031F for KnockbackRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopKnockbackRightBase = 0x031f;
    /// <summary>$92:930B selects top frame-sequence index$0321 for KnockbackLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopKnockbackLeftBase = 0x0321;
    /// <summary>$92:930D selects top frame-sequence index$01CD for NormalJumpTransitionAimUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalJumpTransitionAimUpRightBase = 0x01cd;
    /// <summary>$92:930F selects top frame-sequence index$01CF for NormalJumpTransitionAimUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalJumpTransitionAimUpLeftBase = 0x01cf;
    /// <summary>$92:9319 selects top frame-sequence index$032F for UnusedPose5B and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPose5BBase = 0x032f;
    /// <summary>$92:931B selects top frame-sequence index$0330 for UnusedPose5C and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPose5CBase = 0x0330;
    /// <summary>$92:931D selects top frame-sequence index$0331 for UnusedPose5D and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPose5DBase = 0x0331;
    /// <summary>$92:9323 selects top frame-sequence index$0332 for UnusedPose60 and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPose60Base = 0x0332;
    /// <summary>$92:9325 selects top frame-sequence index$0333 for UnusedPose61 and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPose61Base = 0x0333;
    /// <summary>$92:9327 selects top frame-sequence index$0375 for UnusedPose62 and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPose62Base = 0x0375;
    /// <summary>$92:9329 selects top frame-sequence index$02AB for UnusedPose63 and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPose63Base = 0x02ab;
    /// <summary>$92:932B selects top frame-sequence index$02AD for UnusedPose64 and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPose64Base = 0x02ad;
    /// <summary>$92:932D selects top frame-sequence index$02AF for UnusedPose65 and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPose65Base = 0x02af;
    /// <summary>$92:932F selects top frame-sequence index$02B8 for UnusedPose66 and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPose66Base = 0x02b8;
    /// <summary>$92:9331 selects top frame-sequence index$03C5 for FallingGunExtendedRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFallingGunExtendedRightBase = 0x03c5;
    /// <summary>$92:9333 selects top frame-sequence index$03CC for FallingGunExtendedLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFallingGunExtendedLeftBase = 0x03cc;
    /// <summary>$92:9335 selects top frame-sequence index$027D for NormalJumpAimDiagonalUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalJumpAimDiagonalUpRightBase = 0x027d;
    /// <summary>$92:9337 selects top frame-sequence index$027F for NormalJumpAimDiagonalUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalJumpAimDiagonalUpLeftBase = 0x027f;
    /// <summary>$92:9339 selects top frame-sequence index$0281 for NormalJumpAimDiagonalDownRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalJumpAimDiagonalDownRightBase = 0x0281;
    /// <summary>$92:933B selects top frame-sequence index$0283 for NormalJumpAimDiagonalDownLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalJumpAimDiagonalDownLeftBase = 0x0283;
    /// <summary>$92:933D selects top frame-sequence index$03DD for FallingAimDiagonalUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFallingAimDiagonalUpRightBase = 0x03dd;
    /// <summary>$92:933F selects top frame-sequence index$03E0 for FallingAimDiagonalUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFallingAimDiagonalUpLeftBase = 0x03e0;
    /// <summary>$92:9341 selects top frame-sequence index$03E3 for FallingAimDiagonalDownRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFallingAimDiagonalDownRightBase = 0x03e3;
    /// <summary>$92:9343 selects top frame-sequence index$03E6 for FallingAimDiagonalDownLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFallingAimDiagonalDownLeftBase = 0x03e6;
    /// <summary>$92:9345 selects top frame-sequence index$03E9 for CrouchingAimDiagonalUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopCrouchingAimDiagonalUpRightBase = 0x03e9;
    /// <summary>$92:9347 selects top frame-sequence index$03EA for CrouchingAimDiagonalUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopCrouchingAimDiagonalUpLeftBase = 0x03ea;
    /// <summary>$92:9349 selects top frame-sequence index$03EB for CrouchingAimDiagonalDownRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopCrouchingAimDiagonalDownRightBase = 0x03eb;
    /// <summary>$92:934B selects top frame-sequence index$03EC for CrouchingAimDiagonalDownLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopCrouchingAimDiagonalDownLeftBase = 0x03ec;
    /// <summary>$92:934D selects top frame-sequence index$01E1 for MoonwalkAimUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMoonwalkAimUpLeftBase = 0x01e1;
    /// <summary>$92:934F selects top frame-sequence index$01E7 for MoonwalkAimUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMoonwalkAimUpRightBase = 0x01e7;
    /// <summary>$92:9351 selects top frame-sequence index$01ED for MoonwalkAimDownLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMoonwalkAimDownLeftBase = 0x01ed;
    /// <summary>$92:9353 selects top frame-sequence index$01F3 for MoonwalkAimDownRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopMoonwalkAimDownRightBase = 0x01f3;
    /// <summary>$92:9355 selects top frame-sequence index$0738 for SpringBallGroundRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopSpringBallGroundRightBase = 0x0738;
    /// <summary>$92:9357 selects top frame-sequence index$0742 for SpringBallGroundLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopSpringBallGroundLeftBase = 0x0742;
    /// <summary>$92:9365 selects top frame-sequence index$07AC for ScrewAttackRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopScrewAttackRightBase = 0x07ac;
    /// <summary>$92:9367 selects top frame-sequence index$07E4 for ScrewAttackLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopScrewAttackLeftBase = 0x07e4;
    /// <summary>$92:9369 selects top frame-sequence index$02C1 for WallJumpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopWallJumpRightBase = 0x02c1;
    /// <summary>$92:936B selects top frame-sequence index$02F0 for WallJumpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopWallJumpLeftBase = 0x02f0;
    /// <summary>$92:936D selects top frame-sequence index$03ED for CrouchingAimUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopCrouchingAimUpRightBase = 0x03ed;
    /// <summary>$92:936F selects top frame-sequence index$03EF for CrouchingAimUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopCrouchingAimUpLeftBase = 0x03ef;
    /// <summary>$92:9379 selects top frame-sequence index$041F for TurningRightToLeftAimUpPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopTurningRightToLeftAimUpBase = 0x041f;
    /// <summary>$92:937B selects top frame-sequence index$0422 for TurningLeftToRightAimUpPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopTurningLeftToRightAimUpBase = 0x0422;
    /// <summary>$92:937D selects top frame-sequence index$0425 for TurningRightToLeftAimDiagonalDownPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopTurningRightToLeftAimDiagonalDownBase = 0x0425;
    /// <summary>$92:937F selects top frame-sequence index$0428 for TurningLeftToRightAimDiagonalDownPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopTurningLeftToRightAimDiagonalDownBase = 0x0428;
    /// <summary>$92:9399 selects top frame-sequence index$00C2 for ForwardFacingSuitedPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopForwardFacingSuitedBase = 0x00c2;
    /// <summary>$92:93AB selects top frame-sequence index$01B9 for NormalLandingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalLandingRightBase = 0x01b9;
    /// <summary>$92:93AD selects top frame-sequence index$01BB for NormalLandingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopNormalLandingLeftBase = 0x01bb;
    /// <summary>$92:93AF selects top frame-sequence index$01BD for SpinLandingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopSpinLandingRightBase = 0x01bd;
    /// <summary>$92:93B1 selects top frame-sequence index$01C0 for SpinLandingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopSpinLandingLeftBase = 0x01c0;
    /// <summary>$92:93D7 selects top frame-sequence index$042F for DraygonGrabbedNeutralLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDraygonGrabbedNeutralLeftBase = 0x042f;
    /// <summary>$92:93D9 selects top frame-sequence index$0430 for DraygonGrabbedAimUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDraygonGrabbedAimUpLeftBase = 0x0430;
    /// <summary>$92:93DB selects top frame-sequence index$0431 for DraygonGrabbedFiringLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDraygonGrabbedFiringLeftBase = 0x0431;
    /// <summary>$92:93DD selects top frame-sequence index$0432 for DraygonGrabbedAimDownLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDraygonGrabbedAimDownLeftBase = 0x0432;
    /// <summary>$92:93DF selects top frame-sequence index$0439 for DraygonGrabbedMovingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDraygonGrabbedMovingLeftBase = 0x0439;
    /// <summary>$92:93F5 selects top frame-sequence index$043F for ShinesparkHorizontalRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopShinesparkHorizontalRightBase = 0x043f;
    /// <summary>$92:93F7 selects top frame-sequence index$0440 for ShinesparkHorizontalLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopShinesparkHorizontalLeftBase = 0x0440;
    /// <summary>$92:93F9 selects top frame-sequence index$082E for ShinesparkVerticalRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopShinesparkVerticalRightBase = 0x082e;
    /// <summary>$92:93FB selects top frame-sequence index$082F for ShinesparkVerticalLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopShinesparkVerticalLeftBase = 0x082f;
    /// <summary>$92:93FD selects top frame-sequence index$0441 for ShinesparkDiagonalRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopShinesparkDiagonalRightBase = 0x0441;
    /// <summary>$92:93FF selects top frame-sequence index$0442 for ShinesparkDiagonalLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopShinesparkDiagonalLeftBase = 0x0442;
    /// <summary>$92:9409 selects top frame-sequence index$0443 for CrystalFlashRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopCrystalFlashRightBase = 0x0443;
    /// <summary>$92:940B selects top frame-sequence index$0452 for CrystalFlashLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopCrystalFlashLeftBase = 0x0452;
    /// <summary>$92:940D selects top frame-sequence index$01C3 for XrayingStandingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopXrayingStandingRightBase = 0x01c3;
    /// <summary>$92:940F selects top frame-sequence index$01C8 for XrayingStandingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopXrayingStandingLeftBase = 0x01c8;
    /// <summary>$92:9411 selects top frame-sequence index$0461 for DeathSequenceRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDeathSequenceRightBase = 0x0461;
    /// <summary>$92:9413 selects top frame-sequence index$0467 for DeathSequenceLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDeathSequenceLeftBase = 0x0467;
    /// <summary>$92:9419 selects top frame-sequence index$040D for UnusedPoseDb and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPoseDbBase = 0x040d;
    /// <summary>$92:941B selects top frame-sequence index$0410 for UnusedPoseDc and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPoseDcBase = 0x0410;
    /// <summary>$92:941D selects top frame-sequence index$0413 for UnusedPoseDd and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPoseDdBase = 0x0413;
    /// <summary>$92:941F selects top frame-sequence index$0416 for UnusedPoseDe and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopUnusedPoseDeBase = 0x0416;
    /// <summary>$92:942F selects top frame-sequence index$01D1 for FiringLandingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFiringLandingRightBase = 0x01d1;
    /// <summary>$92:9431 selects top frame-sequence index$01D3 for FiringLandingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopFiringLandingLeftBase = 0x01d3;
    /// <summary>$92:9433 selects top frame-sequence index$046D for DrainedCrouchingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDrainedCrouchingRightBase = 0x046d;
    /// <summary>$92:9435 selects top frame-sequence index$047C for DrainedCrouchingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDrainedCrouchingLeftBase = 0x047c;
    /// <summary>$92:9437 selects top frame-sequence index$049C for DrainedStandingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDrainedStandingRightBase = 0x049c;
    /// <summary>$92:9439 selects top frame-sequence index$04A2 for DrainedStandingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDrainedStandingLeftBase = 0x04a2;
    /// <summary>$92:943B selects top frame-sequence index$042B for DraygonGrabbedNeutralRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDraygonGrabbedNeutralRightBase = 0x042b;
    /// <summary>$92:943D selects top frame-sequence index$042C for DraygonGrabbedAimUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDraygonGrabbedAimUpRightBase = 0x042c;
    /// <summary>$92:943F selects top frame-sequence index$042D for DraygonGrabbedFiringRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDraygonGrabbedFiringRightBase = 0x042d;
    /// <summary>$92:9441 selects top frame-sequence index$042E for DraygonGrabbedAimDownRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDraygonGrabbedAimDownRightBase = 0x042e;
    /// <summary>$92:9443 selects top frame-sequence index$0433 for DraygonGrabbedMovingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort TopDraygonGrabbedMovingRightBase = 0x0433;

    /// <summary>$92:9263 pose table: exactly253 named real-pose top-half identities.</summary>
    internal static ushort TopBase(SamusPoseId pose) => (SamusPoseId)pose switch
    {
        SamusPoseId.ForwardFacingPowerSuitPose => TopForwardFacingPowerSuitBase,
        SamusPoseId.FacingRightNormalPose or
        SamusPoseId.UnusedPose47 or
        SamusPoseId.RanIntoWallRightPose or
        SamusPoseId.GrappleStandingRightPose => TopFacingRightNormalBase,
        SamusPoseId.FacingLeftNormalPose or
        SamusPoseId.UnusedPose48 or
        SamusPoseId.RanIntoWallLeftPose or
        SamusPoseId.GrappleStandingLeftPose => TopFacingLeftNormalBase,
        SamusPoseId.StandingAimUpRightPose => TopStandingAimUpRightBase,
        SamusPoseId.StandingAimUpLeftPose => TopStandingAimUpLeftBase,
        SamusPoseId.StandingAimDiagonalUpRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
        SamusPoseId.RanIntoWallAimUpRightPose or
        SamusPoseId.LandingAimDiagonalUpRightPose or
        SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
        SamusPoseId.StandingTransitionAimDiagonalUpRightPose => TopStandingAimDiagonalUpRightBase,
        SamusPoseId.StandingAimDiagonalUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
        SamusPoseId.RanIntoWallAimUpLeftPose or
        SamusPoseId.LandingAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalUpLeftPose => TopStandingAimDiagonalUpLeftBase,
        SamusPoseId.StandingAimDiagonalDownRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
        SamusPoseId.GrappleStandingDownRightPose or
        SamusPoseId.RanIntoWallAimDownRightPose or
        SamusPoseId.LandingAimDiagonalDownRightPose or
        SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
        SamusPoseId.StandingTransitionAimDiagonalDownRightPose => TopStandingAimDiagonalDownRightBase,
        SamusPoseId.StandingAimDiagonalDownLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
        SamusPoseId.GrappleStandingDownLeftPose or
        SamusPoseId.RanIntoWallAimDownLeftPose or
        SamusPoseId.LandingAimDiagonalDownLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => TopStandingAimDiagonalDownLeftBase,
        SamusPoseId.MovingRightNormalPose => TopMovingRightNormalBase,
        SamusPoseId.MovingLeftNormalPose => TopMovingLeftNormalBase,
        SamusPoseId.MovingRightGunExtendedPose => TopMovingRightGunExtendedBase,
        SamusPoseId.MovingLeftGunExtendedPose => TopMovingLeftGunExtendedBase,
        SamusPoseId.RunningAimUpRightPose => TopRunningAimUpRightBase,
        SamusPoseId.RunningAimUpLeftPose => TopRunningAimUpLeftBase,
        SamusPoseId.RunningAimDiagonalUpRightPose => TopRunningAimDiagonalUpRightBase,
        SamusPoseId.RunningAimDiagonalUpLeftPose => TopRunningAimDiagonalUpLeftBase,
        SamusPoseId.RunningAimDiagonalDownRightPose => TopRunningAimDiagonalDownRightBase,
        SamusPoseId.RunningAimDiagonalDownLeftPose => TopRunningAimDiagonalDownLeftBase,
        SamusPoseId.NormalJumpGunExtendedRightPose or
        SamusPoseId.UnusedPoseAC => TopNormalJumpGunExtendedRightBase,
        SamusPoseId.NormalJumpGunExtendedLeftPose or
        SamusPoseId.UnusedPoseAD => TopNormalJumpGunExtendedLeftBase,
        SamusPoseId.NormalJumpAimUpRightPose => TopNormalJumpAimUpRightBase,
        SamusPoseId.NormalJumpAimUpLeftPose => TopNormalJumpAimUpLeftBase,
        SamusPoseId.NormalJumpAimDownRightPose or
        SamusPoseId.UnusedPoseAE => TopNormalJumpAimDownRightBase,
        SamusPoseId.NormalJumpAimDownLeftPose or
        SamusPoseId.UnusedPoseAF => TopNormalJumpAimDownLeftBase,
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
        SamusPoseId.UnusedPose42 => TopSpinJumpRightBase,
        SamusPoseId.SpinJumpLeftPose => TopSpinJumpLeftBase,
        SamusPoseId.SpaceJumpRightPose => TopSpaceJumpRightBase,
        SamusPoseId.SpaceJumpLeftPose => TopSpaceJumpLeftBase,
        SamusPoseId.MorphBallGroundRightPose or
        SamusPoseId.MorphBallFallingRightPose or
        SamusPoseId.MorphBallFallingLeftPose or
        SamusPoseId.UnusedPose3F or
        SamusPoseId.UnusedPose40 => TopMorphBallGroundRightBase,
        SamusPoseId.MorphBallMovingRightPose => TopMorphBallMovingRightBase,
        SamusPoseId.MorphBallMovingLeftPose => TopMorphBallMovingLeftBase,
        SamusPoseId.TurningRightToLeftPose or
        SamusPoseId.TurningRightToLeftJumpPose or
        SamusPoseId.TurningRightToLeftCrouchingPose or
        SamusPoseId.TurningRightToLeftFallingPose or
        SamusPoseId.MoonwalkTurnJumpLeftPose or
        SamusPoseId.UnusedPoseC6 => TopTurningRightToLeftBase,
        SamusPoseId.TurningLeftToRightPose or
        SamusPoseId.TurningLeftToRightJumpPose or
        SamusPoseId.TurningLeftToRightCrouchingPose or
        SamusPoseId.TurningLeftToRightFallingPose or
        SamusPoseId.MoonwalkTurnJumpRightPose => TopTurningLeftToRightBase,
        SamusPoseId.CrouchingRightPose or
        SamusPoseId.GrappleCrouchingRightPose => TopCrouchingRightBase,
        SamusPoseId.CrouchingLeftPose or
        SamusPoseId.GrappleCrouchingLeftPose => TopCrouchingLeftBase,
        SamusPoseId.FallingRightPose => TopFallingRightBase,
        SamusPoseId.FallingLeftPose => TopFallingLeftBase,
        SamusPoseId.FallingAimUpRightPose => TopFallingAimUpRightBase,
        SamusPoseId.FallingAimUpLeftPose => TopFallingAimUpLeftBase,
        SamusPoseId.FallingAimDownRightPose => TopFallingAimDownRightBase,
        SamusPoseId.FallingAimDownLeftPose => TopFallingAimDownLeftBase,
        SamusPoseId.CrouchingTransitionRightPose or
        SamusPoseId.StandingTransitionRightPose => TopCrouchingTransitionRightBase,
        SamusPoseId.CrouchingTransitionLeftPose or
        SamusPoseId.StandingTransitionLeftPose => TopCrouchingTransitionLeftBase,
        SamusPoseId.MorphingTransitionRightPose => TopMorphingTransitionRightBase,
        SamusPoseId.MorphingTransitionLeftPose => TopMorphingTransitionLeftBase,
        SamusPoseId.UnmorphingTransitionRightPose => TopUnmorphingTransitionRightBase,
        SamusPoseId.UnmorphingTransitionLeftPose => TopUnmorphingTransitionLeftBase,
        SamusPoseId.MorphBallGroundLeftPose or
        SamusPoseId.UnusedPoseC5 or
        SamusPoseId.UnusedPoseDf => TopMorphBallGroundLeftBase,
        SamusPoseId.UnusedPose45 => TopUnusedPose45Base,
        SamusPoseId.UnusedPose46 => TopUnusedPose46Base,
        SamusPoseId.MoonwalkFacingLeftPose => TopMoonwalkFacingLeftBase,
        SamusPoseId.MoonwalkFacingRightPose => TopMoonwalkFacingRightBase,
        SamusPoseId.NeutralJumpTransitionRightPose => TopNeutralJumpTransitionRightBase,
        SamusPoseId.NeutralJumpTransitionLeftPose => TopNeutralJumpTransitionLeftBase,
        SamusPoseId.NeutralJumpRightPose or
        SamusPoseId.ShinesparkWindupRightPose => TopNeutralJumpRightBase,
        SamusPoseId.NeutralJumpLeftPose or
        SamusPoseId.ShinesparkWindupLeftPose => TopNeutralJumpLeftBase,
        SamusPoseId.DamageBoostLeftPose => TopDamageBoostLeftBase,
        SamusPoseId.DamageBoostRightPose => TopDamageBoostRightBase,
        SamusPoseId.NormalJumpForwardRightPose => TopNormalJumpForwardRightBase,
        SamusPoseId.NormalJumpForwardLeftPose => TopNormalJumpForwardLeftBase,
        SamusPoseId.KnockbackRightPose => TopKnockbackRightBase,
        SamusPoseId.KnockbackLeftPose => TopKnockbackLeftBase,
        SamusPoseId.NormalJumpTransitionAimUpRightPose or
        SamusPoseId.LandingAimUpRightPose or
        SamusPoseId.CrouchingTransitionAimUpRightPose or
        SamusPoseId.StandingTransitionAimUpRightPose => TopNormalJumpTransitionAimUpRightBase,
        SamusPoseId.NormalJumpTransitionAimUpLeftPose or
        SamusPoseId.LandingAimUpLeftPose or
        SamusPoseId.CrouchingTransitionAimUpLeftPose or
        SamusPoseId.StandingTransitionAimUpLeftPose => TopNormalJumpTransitionAimUpLeftBase,
        SamusPoseId.UnusedPose5B or
        SamusPoseId.GrappleWallContactLeftPose => TopUnusedPose5BBase,
        SamusPoseId.UnusedPose5C or
        SamusPoseId.GrappleWallContactRightPose => TopUnusedPose5CBase,
        SamusPoseId.UnusedPose5D or
        SamusPoseId.UnusedPose5E or
        SamusPoseId.UnusedPose5F => TopUnusedPose5DBase,
        SamusPoseId.UnusedPose60 => TopUnusedPose60Base,
        SamusPoseId.UnusedPose61 or
        SamusPoseId.GrappleSwingRightPose => TopUnusedPose61Base,
        SamusPoseId.UnusedPose62 or
        SamusPoseId.GrappleSwingLeftPose => TopUnusedPose62Base,
        SamusPoseId.UnusedPose63 => TopUnusedPose63Base,
        SamusPoseId.UnusedPose64 => TopUnusedPose64Base,
        SamusPoseId.UnusedPose65 => TopUnusedPose65Base,
        SamusPoseId.UnusedPose66 => TopUnusedPose66Base,
        SamusPoseId.FallingGunExtendedRightPose => TopFallingGunExtendedRightBase,
        SamusPoseId.FallingGunExtendedLeftPose => TopFallingGunExtendedLeftBase,
        SamusPoseId.NormalJumpAimDiagonalUpRightPose => TopNormalJumpAimDiagonalUpRightBase,
        SamusPoseId.NormalJumpAimDiagonalUpLeftPose => TopNormalJumpAimDiagonalUpLeftBase,
        SamusPoseId.NormalJumpAimDiagonalDownRightPose or
        SamusPoseId.UnusedPoseB0 => TopNormalJumpAimDiagonalDownRightBase,
        SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
        SamusPoseId.UnusedPoseB1 => TopNormalJumpAimDiagonalDownLeftBase,
        SamusPoseId.FallingAimDiagonalUpRightPose => TopFallingAimDiagonalUpRightBase,
        SamusPoseId.FallingAimDiagonalUpLeftPose => TopFallingAimDiagonalUpLeftBase,
        SamusPoseId.FallingAimDiagonalDownRightPose => TopFallingAimDiagonalDownRightBase,
        SamusPoseId.FallingAimDiagonalDownLeftPose => TopFallingAimDiagonalDownLeftBase,
        SamusPoseId.CrouchingAimDiagonalUpRightPose => TopCrouchingAimDiagonalUpRightBase,
        SamusPoseId.CrouchingAimDiagonalUpLeftPose => TopCrouchingAimDiagonalUpLeftBase,
        SamusPoseId.CrouchingAimDiagonalDownRightPose or
        SamusPoseId.GrappleCrouchingDownRightPose => TopCrouchingAimDiagonalDownRightBase,
        SamusPoseId.CrouchingAimDiagonalDownLeftPose or
        SamusPoseId.GrappleCrouchingDownLeftPose => TopCrouchingAimDiagonalDownLeftBase,
        SamusPoseId.MoonwalkAimUpLeftPose => TopMoonwalkAimUpLeftBase,
        SamusPoseId.MoonwalkAimUpRightPose => TopMoonwalkAimUpRightBase,
        SamusPoseId.MoonwalkAimDownLeftPose => TopMoonwalkAimDownLeftBase,
        SamusPoseId.MoonwalkAimDownRightPose => TopMoonwalkAimDownRightBase,
        SamusPoseId.SpringBallGroundRightPose or
        SamusPoseId.SpringBallMovingRightPose or
        SamusPoseId.SpringBallFallingRightPose or
        SamusPoseId.SpringBallJumpRightPose => TopSpringBallGroundRightBase,
        SamusPoseId.SpringBallGroundLeftPose or
        SamusPoseId.SpringBallMovingLeftPose or
        SamusPoseId.SpringBallFallingLeftPose or
        SamusPoseId.SpringBallJumpLeftPose => TopSpringBallGroundLeftBase,
        SamusPoseId.ScrewAttackRightPose => TopScrewAttackRightBase,
        SamusPoseId.ScrewAttackLeftPose => TopScrewAttackLeftBase,
        SamusPoseId.WallJumpRightPose => TopWallJumpRightBase,
        SamusPoseId.WallJumpLeftPose => TopWallJumpLeftBase,
        SamusPoseId.CrouchingAimUpRightPose => TopCrouchingAimUpRightBase,
        SamusPoseId.CrouchingAimUpLeftPose => TopCrouchingAimUpLeftBase,
        SamusPoseId.TurningRightToLeftAimUpPose or
        SamusPoseId.TurningRightToLeftJumpAimUpPose or
        SamusPoseId.TurningRightToLeftFallingAimUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
        SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or
        SamusPoseId.MoonwalkTurnJumpAimUpLeftPose => TopTurningRightToLeftAimUpBase,
        SamusPoseId.TurningLeftToRightAimUpPose or
        SamusPoseId.TurningLeftToRightJumpAimUpPose or
        SamusPoseId.TurningLeftToRightFallingAimUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
        SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or
        SamusPoseId.MoonwalkTurnJumpAimUpRightPose => TopTurningLeftToRightAimUpBase,
        SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
        SamusPoseId.TurningRightToLeftJumpAimDownPose or
        SamusPoseId.TurningRightToLeftFallingAimDownPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or
        SamusPoseId.MoonwalkTurnJumpAimDownLeftPose => TopTurningRightToLeftAimDiagonalDownBase,
        SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
        SamusPoseId.TurningLeftToRightJumpAimDownPose or
        SamusPoseId.TurningLeftToRightFallingAimDownPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or
        SamusPoseId.MoonwalkTurnJumpAimDownRightPose => TopTurningLeftToRightAimDiagonalDownBase,
        SamusPoseId.ForwardFacingSuitedPose => TopForwardFacingSuitedBase,
        SamusPoseId.NormalLandingRightPose => TopNormalLandingRightBase,
        SamusPoseId.NormalLandingLeftPose => TopNormalLandingLeftBase,
        SamusPoseId.SpinLandingRightPose => TopSpinLandingRightBase,
        SamusPoseId.SpinLandingLeftPose => TopSpinLandingLeftBase,
        SamusPoseId.DraygonGrabbedNeutralLeftPose => TopDraygonGrabbedNeutralLeftBase,
        SamusPoseId.DraygonGrabbedAimUpLeftPose => TopDraygonGrabbedAimUpLeftBase,
        SamusPoseId.DraygonGrabbedFiringLeftPose => TopDraygonGrabbedFiringLeftBase,
        SamusPoseId.DraygonGrabbedAimDownLeftPose => TopDraygonGrabbedAimDownLeftBase,
        SamusPoseId.DraygonGrabbedMovingLeftPose => TopDraygonGrabbedMovingLeftBase,
        SamusPoseId.ShinesparkHorizontalRightPose => TopShinesparkHorizontalRightBase,
        SamusPoseId.ShinesparkHorizontalLeftPose => TopShinesparkHorizontalLeftBase,
        SamusPoseId.ShinesparkVerticalRightPose => TopShinesparkVerticalRightBase,
        SamusPoseId.ShinesparkVerticalLeftPose => TopShinesparkVerticalLeftBase,
        SamusPoseId.ShinesparkDiagonalRightPose => TopShinesparkDiagonalRightBase,
        SamusPoseId.ShinesparkDiagonalLeftPose => TopShinesparkDiagonalLeftBase,
        SamusPoseId.CrystalFlashRightPose => TopCrystalFlashRightBase,
        SamusPoseId.CrystalFlashLeftPose => TopCrystalFlashLeftBase,
        SamusPoseId.XrayingStandingRightPose or
        SamusPoseId.XrayingCrouchingRightPose => TopXrayingStandingRightBase,
        SamusPoseId.XrayingStandingLeftPose or
        SamusPoseId.XrayingCrouchingLeftPose => TopXrayingStandingLeftBase,
        SamusPoseId.DeathSequenceRightPose => TopDeathSequenceRightBase,
        SamusPoseId.DeathSequenceLeftPose => TopDeathSequenceLeftBase,
        SamusPoseId.UnusedPoseDb => TopUnusedPoseDbBase,
        SamusPoseId.UnusedPoseDc => TopUnusedPoseDcBase,
        SamusPoseId.UnusedPoseDd => TopUnusedPoseDdBase,
        SamusPoseId.UnusedPoseDe => TopUnusedPoseDeBase,
        SamusPoseId.FiringLandingRightPose => TopFiringLandingRightBase,
        SamusPoseId.FiringLandingLeftPose => TopFiringLandingLeftBase,
        SamusPoseId.DrainedCrouchingRightPose => TopDrainedCrouchingRightBase,
        SamusPoseId.DrainedCrouchingLeftPose => TopDrainedCrouchingLeftBase,
        SamusPoseId.DrainedStandingRightPose => TopDrainedStandingRightBase,
        SamusPoseId.DrainedStandingLeftPose => TopDrainedStandingLeftBase,
        SamusPoseId.DraygonGrabbedNeutralRightPose => TopDraygonGrabbedNeutralRightBase,
        SamusPoseId.DraygonGrabbedAimUpRightPose => TopDraygonGrabbedAimUpRightBase,
        SamusPoseId.DraygonGrabbedFiringRightPose => TopDraygonGrabbedFiringRightBase,
        SamusPoseId.DraygonGrabbedAimDownRightPose => TopDraygonGrabbedAimDownRightBase,
        SamusPoseId.DraygonGrabbedMovingRightPose => TopDraygonGrabbedMovingRightBase,
        _ => throw new ArgumentOutOfRangeException(nameof(pose)),
    };

    /// <summary>$92:945D selects bottom frame-sequence index$0062 for ForwardFacingPowerSuitPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomForwardFacingPowerSuitBase = 0x0062;
    /// <summary>$92:945F selects bottom frame-sequence index$04AA for FacingRightNormalPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomFacingRightNormalBase = 0x04aa;
    /// <summary>$92:9461 selects bottom frame-sequence index$04B3 for FacingLeftNormalPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomFacingLeftNormalBase = 0x04b3;
    /// <summary>$92:9463 selects bottom frame-sequence index$04C0 for StandingAimUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomStandingAimUpRightBase = 0x04c0;
    /// <summary>$92:9465 selects bottom frame-sequence index$04C2 for StandingAimUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomStandingAimUpLeftBase = 0x04c2;
    /// <summary>$92:946F selects bottom frame-sequence index$04E3 for MovingRightNormalPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomMovingRightNormalBase = 0x04e3;
    /// <summary>$92:9471 selects bottom frame-sequence index$04ED for MovingLeftNormalPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomMovingLeftNormalBase = 0x04ed;
    /// <summary>$92:9483 selects bottom frame-sequence index$0507 for NormalJumpGunExtendedRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNormalJumpGunExtendedRightBase = 0x0507;
    /// <summary>$92:9485 selects bottom frame-sequence index$0509 for NormalJumpGunExtendedLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNormalJumpGunExtendedLeftBase = 0x0509;
    /// <summary>$92:9487 selects bottom frame-sequence index$050B for NormalJumpAimUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNormalJumpAimUpRightBase = 0x050b;
    /// <summary>$92:9489 selects bottom frame-sequence index$050D for NormalJumpAimUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNormalJumpAimUpLeftBase = 0x050d;
    /// <summary>$92:948B selects bottom frame-sequence index$0503 for NormalJumpAimDownRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNormalJumpAimDownRightBase = 0x0503;
    /// <summary>$92:948D selects bottom frame-sequence index$0505 for NormalJumpAimDownLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNormalJumpAimDownLeftBase = 0x0505;
    /// <summary>$92:948F selects bottom frame-sequence index$0758 for SpinJumpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomSpinJumpRightBase = 0x0758;
    /// <summary>$92:9491 selects bottom frame-sequence index$0770 for SpinJumpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomSpinJumpLeftBase = 0x0770;
    /// <summary>$92:9493 selects bottom frame-sequence index$0788 for SpaceJumpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomSpaceJumpRightBase = 0x0788;
    /// <summary>$92:9495 selects bottom frame-sequence index$07A0 for SpaceJumpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomSpaceJumpLeftBase = 0x07a0;
    /// <summary>$92:9497 selects bottom frame-sequence index$0710 for MorphBallGroundRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomMorphBallGroundRightBase = 0x0710;
    /// <summary>$92:9499 selects bottom frame-sequence index$0724 for MorphBallMovingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomMorphBallMovingRightBase = 0x0724;
    /// <summary>$92:949B selects bottom frame-sequence index$072E for MorphBallMovingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomMorphBallMovingLeftBase = 0x072e;
    /// <summary>$92:949D selects bottom frame-sequence index$074C for UnusedPose20 and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomUnusedPose20Base = 0x074c;
    /// <summary>$92:94A7 selects bottom frame-sequence index$0687 for TurningRightToLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomTurningRightToLeftBase = 0x0687;
    /// <summary>$92:94A9 selects bottom frame-sequence index$068A for TurningLeftToRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomTurningLeftToRightBase = 0x068a;
    /// <summary>$92:94AB selects bottom frame-sequence index$065F for CrouchingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomCrouchingRightBase = 0x065f;
    /// <summary>$92:94AD selects bottom frame-sequence index$0668 for CrouchingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomCrouchingLeftBase = 0x0668;
    /// <summary>$92:94AF selects bottom frame-sequence index$063B for FallingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomFallingRightBase = 0x063b;
    /// <summary>$92:94B1 selects bottom frame-sequence index$0642 for FallingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomFallingLeftBase = 0x0642;
    /// <summary>$92:94B3 selects bottom frame-sequence index$064D for FallingAimUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomFallingAimUpRightBase = 0x064d;
    /// <summary>$92:94B5 selects bottom frame-sequence index$0650 for FallingAimUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomFallingAimUpLeftBase = 0x0650;
    /// <summary>$92:94B7 selects bottom frame-sequence index$0649 for FallingAimDownRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomFallingAimDownRightBase = 0x0649;
    /// <summary>$92:94B9 selects bottom frame-sequence index$064B for FallingAimDownLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomFallingAimDownLeftBase = 0x064b;
    /// <summary>$92:94BB selects bottom frame-sequence index$068D for TurningRightToLeftJumpPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomTurningRightToLeftJumpBase = 0x068d;
    /// <summary>$92:94BD selects bottom frame-sequence index$0690 for TurningLeftToRightJumpPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomTurningLeftToRightJumpBase = 0x0690;
    /// <summary>$92:94C7 selects bottom frame-sequence index$0679 for CrouchingTransitionRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomCrouchingTransitionRightBase = 0x0679;
    /// <summary>$92:94C9 selects bottom frame-sequence index$067A for CrouchingTransitionLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomCrouchingTransitionLeftBase = 0x067a;
    /// <summary>$92:94CB selects bottom frame-sequence index$0405 for MorphingTransitionRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomMorphingTransitionRightBase = 0x0405;
    /// <summary>$92:94CD selects bottom frame-sequence index$0407 for MorphingTransitionLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomMorphingTransitionLeftBase = 0x0407;
    /// <summary>$92:94D7 selects bottom frame-sequence index$0409 for UnmorphingTransitionRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomUnmorphingTransitionRightBase = 0x0409;
    /// <summary>$92:94D9 selects bottom frame-sequence index$040B for UnmorphingTransitionLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomUnmorphingTransitionLeftBase = 0x040b;
    /// <summary>$92:94DF selects bottom frame-sequence index$071A for MorphBallGroundLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomMorphBallGroundLeftBase = 0x071a;
    /// <summary>$92:94EF selects bottom frame-sequence index$04F7 for MoonwalkFacingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomMoonwalkFacingLeftBase = 0x04f7;
    /// <summary>$92:94F1 selects bottom frame-sequence index$04FD for MoonwalkFacingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomMoonwalkFacingRightBase = 0x04fd;
    /// <summary>$92:94F3 selects bottom frame-sequence index$051B for NeutralJumpTransitionRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNeutralJumpTransitionRightBase = 0x051b;
    /// <summary>$92:94F5 selects bottom frame-sequence index$051C for NeutralJumpTransitionLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNeutralJumpTransitionLeftBase = 0x051c;
    /// <summary>$92:94F7 selects bottom frame-sequence index$051D for NeutralJumpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNeutralJumpRightBase = 0x051d;
    /// <summary>$92:94F9 selects bottom frame-sequence index$0523 for NeutralJumpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNeutralJumpLeftBase = 0x0523;
    /// <summary>$92:94FB selects bottom frame-sequence index$0529 for DamageBoostLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomDamageBoostLeftBase = 0x0529;
    /// <summary>$92:94FD selects bottom frame-sequence index$0533 for DamageBoostRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomDamageBoostRightBase = 0x0533;
    /// <summary>$92:94FF selects bottom frame-sequence index$050F for NormalJumpForwardRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNormalJumpForwardRightBase = 0x050f;
    /// <summary>$92:9501 selects bottom frame-sequence index$0511 for NormalJumpForwardLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNormalJumpForwardLeftBase = 0x0511;
    /// <summary>$92:9503 selects bottom frame-sequence index$05B1 for KnockbackRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomKnockbackRightBase = 0x05b1;
    /// <summary>$92:9505 selects bottom frame-sequence index$05B3 for KnockbackLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomKnockbackLeftBase = 0x05b3;
    /// <summary>$92:9513 selects bottom frame-sequence index$05B5 for UnusedPose5B and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomUnusedPose5BBase = 0x05b5;
    /// <summary>$92:9515 selects bottom frame-sequence index$05B6 for UnusedPose5C and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomUnusedPose5CBase = 0x05b6;
    /// <summary>$92:9517 selects bottom frame-sequence index$05B7 for UnusedPose5D and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomUnusedPose5DBase = 0x05b7;
    /// <summary>$92:9521 selects bottom frame-sequence index$05F9 for UnusedPose62 and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomUnusedPose62Base = 0x05f9;
    /// <summary>$92:9523 selects bottom frame-sequence index$053D for UnusedPose63 and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomUnusedPose63Base = 0x053d;
    /// <summary>$92:9525 selects bottom frame-sequence index$053F for UnusedPose64 and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomUnusedPose64Base = 0x053f;
    /// <summary>$92:9527 selects bottom frame-sequence index$0541 for UnusedPose65 and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomUnusedPose65Base = 0x0541;
    /// <summary>$92:9529 selects bottom frame-sequence index$054A for UnusedPose66 and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomUnusedPose66Base = 0x054a;
    /// <summary>$92:952F selects bottom frame-sequence index$0513 for NormalJumpAimDiagonalUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNormalJumpAimDiagonalUpRightBase = 0x0513;
    /// <summary>$92:9531 selects bottom frame-sequence index$0515 for NormalJumpAimDiagonalUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNormalJumpAimDiagonalUpLeftBase = 0x0515;
    /// <summary>$92:9533 selects bottom frame-sequence index$0517 for NormalJumpAimDiagonalDownRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNormalJumpAimDiagonalDownRightBase = 0x0517;
    /// <summary>$92:9535 selects bottom frame-sequence index$0519 for NormalJumpAimDiagonalDownLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNormalJumpAimDiagonalDownLeftBase = 0x0519;
    /// <summary>$92:9537 selects bottom frame-sequence index$0653 for FallingAimDiagonalUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomFallingAimDiagonalUpRightBase = 0x0653;
    /// <summary>$92:9539 selects bottom frame-sequence index$0656 for FallingAimDiagonalUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomFallingAimDiagonalUpLeftBase = 0x0656;
    /// <summary>$92:953B selects bottom frame-sequence index$0659 for FallingAimDiagonalDownRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomFallingAimDiagonalDownRightBase = 0x0659;
    /// <summary>$92:953D selects bottom frame-sequence index$065C for FallingAimDiagonalDownLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomFallingAimDiagonalDownLeftBase = 0x065c;
    /// <summary>$92:953F selects bottom frame-sequence index$0671 for CrouchingAimDiagonalUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomCrouchingAimDiagonalUpRightBase = 0x0671;
    /// <summary>$92:9541 selects bottom frame-sequence index$0672 for CrouchingAimDiagonalUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomCrouchingAimDiagonalUpLeftBase = 0x0672;
    /// <summary>$92:9543 selects bottom frame-sequence index$0673 for CrouchingAimDiagonalDownRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomCrouchingAimDiagonalDownRightBase = 0x0673;
    /// <summary>$92:9545 selects bottom frame-sequence index$0674 for CrouchingAimDiagonalDownLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomCrouchingAimDiagonalDownLeftBase = 0x0674;
    /// <summary>$92:954F selects bottom frame-sequence index$0738 for SpringBallGroundRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomSpringBallGroundRightBase = 0x0738;
    /// <summary>$92:9551 selects bottom frame-sequence index$0742 for SpringBallGroundLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomSpringBallGroundLeftBase = 0x0742;
    /// <summary>$92:955F selects bottom frame-sequence index$07C8 for ScrewAttackRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomScrewAttackRightBase = 0x07c8;
    /// <summary>$92:9561 selects bottom frame-sequence index$0800 for ScrewAttackLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomScrewAttackLeftBase = 0x0800;
    /// <summary>$92:9563 selects bottom frame-sequence index$0553 for WallJumpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomWallJumpRightBase = 0x0553;
    /// <summary>$92:9565 selects bottom frame-sequence index$0582 for WallJumpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomWallJumpLeftBase = 0x0582;
    /// <summary>$92:9567 selects bottom frame-sequence index$0675 for CrouchingAimUpRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomCrouchingAimUpRightBase = 0x0675;
    /// <summary>$92:9569 selects bottom frame-sequence index$0677 for CrouchingAimUpLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomCrouchingAimUpLeftBase = 0x0677;
    /// <summary>$92:956D selects bottom frame-sequence index$0696 for TurningLeftToRightFallingPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomTurningLeftToRightFallingBase = 0x0696;
    /// <summary>$92:9593 selects bottom frame-sequence index$0122 for ForwardFacingSuitedPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomForwardFacingSuitedBase = 0x0122;
    /// <summary>$92:95A5 selects bottom frame-sequence index$04C5 for NormalLandingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNormalLandingRightBase = 0x04c5;
    /// <summary>$92:95A7 selects bottom frame-sequence index$04C7 for NormalLandingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomNormalLandingLeftBase = 0x04c7;
    /// <summary>$92:95A9 selects bottom frame-sequence index$04C9 for SpinLandingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomSpinLandingRightBase = 0x04c9;
    /// <summary>$92:95AB selects bottom frame-sequence index$04CC for SpinLandingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomSpinLandingLeftBase = 0x04cc;
    /// <summary>$92:95D1 selects bottom frame-sequence index$069A for DraygonGrabbedNeutralLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomDraygonGrabbedNeutralLeftBase = 0x069a;
    /// <summary>$92:95D9 selects bottom frame-sequence index$06A1 for DraygonGrabbedMovingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomDraygonGrabbedMovingLeftBase = 0x06a1;
    /// <summary>$92:95EF selects bottom frame-sequence index$06A7 for ShinesparkHorizontalRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomShinesparkHorizontalRightBase = 0x06a7;
    /// <summary>$92:95F1 selects bottom frame-sequence index$06A8 for ShinesparkHorizontalLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomShinesparkHorizontalLeftBase = 0x06a8;
    /// <summary>$92:95F3 selects bottom frame-sequence index$082E for ShinesparkVerticalRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomShinesparkVerticalRightBase = 0x082e;
    /// <summary>$92:95F5 selects bottom frame-sequence index$082F for ShinesparkVerticalLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomShinesparkVerticalLeftBase = 0x082f;
    /// <summary>$92:95F7 selects bottom frame-sequence index$06A9 for ShinesparkDiagonalRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomShinesparkDiagonalRightBase = 0x06a9;
    /// <summary>$92:95F9 selects bottom frame-sequence index$06AA for ShinesparkDiagonalLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomShinesparkDiagonalLeftBase = 0x06aa;
    /// <summary>$92:9603 selects bottom frame-sequence index$06AB for CrystalFlashRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomCrystalFlashRightBase = 0x06ab;
    /// <summary>$92:9605 selects bottom frame-sequence index$06BA for CrystalFlashLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomCrystalFlashLeftBase = 0x06ba;
    /// <summary>$92:9607 selects bottom frame-sequence index$04CF for XrayingStandingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomXrayingStandingRightBase = 0x04cf;
    /// <summary>$92:9609 selects bottom frame-sequence index$04D4 for XrayingStandingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomXrayingStandingLeftBase = 0x04d4;
    /// <summary>$92:960B selects bottom frame-sequence index$06C9 for DeathSequenceRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomDeathSequenceRightBase = 0x06c9;
    /// <summary>$92:960D selects bottom frame-sequence index$06CF for DeathSequenceLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomDeathSequenceLeftBase = 0x06cf;
    /// <summary>$92:960F selects bottom frame-sequence index$04D9 for XrayingCrouchingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomXrayingCrouchingRightBase = 0x04d9;
    /// <summary>$92:9611 selects bottom frame-sequence index$04DE for XrayingCrouchingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomXrayingCrouchingLeftBase = 0x04de;
    /// <summary>$92:9613 selects bottom frame-sequence index$067D for UnusedPoseDb and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomUnusedPoseDbBase = 0x067d;
    /// <summary>$92:9615 selects bottom frame-sequence index$0680 for UnusedPoseDc and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomUnusedPoseDcBase = 0x0680;
    /// <summary>$92:9617 selects bottom frame-sequence index$0681 for UnusedPoseDd and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomUnusedPoseDdBase = 0x0681;
    /// <summary>$92:9619 selects bottom frame-sequence index$0684 for UnusedPoseDe and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomUnusedPoseDeBase = 0x0684;
    /// <summary>$92:962D selects bottom frame-sequence index$06D5 for DrainedCrouchingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomDrainedCrouchingRightBase = 0x06d5;
    /// <summary>$92:962F selects bottom frame-sequence index$06E4 for DrainedCrouchingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomDrainedCrouchingLeftBase = 0x06e4;
    /// <summary>$92:9631 selects bottom frame-sequence index$0704 for DrainedStandingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomDrainedStandingRightBase = 0x0704;
    /// <summary>$92:9633 selects bottom frame-sequence index$070A for DrainedStandingLeftPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomDrainedStandingLeftBase = 0x070a;
    /// <summary>$92:9635 selects bottom frame-sequence index$0699 for DraygonGrabbedNeutralRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomDraygonGrabbedNeutralRightBase = 0x0699;
    /// <summary>$92:963D selects bottom frame-sequence index$069B for DraygonGrabbedMovingRightPose and the aliases below; underlying pointer/OBJ payload remains REQUIRED.</summary>
    private const ushort BottomDraygonGrabbedMovingRightBase = 0x069b;

    /// <summary>$92:945D pose table: exactly253 named real-pose bottom-half identities.</summary>
    internal static ushort BottomBase(SamusPoseId pose) => (SamusPoseId)pose switch
    {
        SamusPoseId.ForwardFacingPowerSuitPose => BottomForwardFacingPowerSuitBase,
        SamusPoseId.FacingRightNormalPose or
        SamusPoseId.UnusedPose47 or
        SamusPoseId.RanIntoWallRightPose or
        SamusPoseId.GrappleStandingRightPose => BottomFacingRightNormalBase,
        SamusPoseId.FacingLeftNormalPose or
        SamusPoseId.UnusedPose48 or
        SamusPoseId.RanIntoWallLeftPose or
        SamusPoseId.GrappleStandingLeftPose => BottomFacingLeftNormalBase,
        SamusPoseId.StandingAimUpRightPose or
        SamusPoseId.StandingAimDiagonalUpRightPose or
        SamusPoseId.StandingAimDiagonalDownRightPose or
        SamusPoseId.GrappleStandingDownRightPose or
        SamusPoseId.RanIntoWallAimUpRightPose or
        SamusPoseId.RanIntoWallAimDownRightPose => BottomStandingAimUpRightBase,
        SamusPoseId.StandingAimUpLeftPose or
        SamusPoseId.StandingAimDiagonalUpLeftPose or
        SamusPoseId.StandingAimDiagonalDownLeftPose or
        SamusPoseId.GrappleStandingDownLeftPose or
        SamusPoseId.RanIntoWallAimUpLeftPose or
        SamusPoseId.RanIntoWallAimDownLeftPose => BottomStandingAimUpLeftBase,
        SamusPoseId.MovingRightNormalPose or
        SamusPoseId.MovingRightGunExtendedPose or
        SamusPoseId.RunningAimUpRightPose or
        SamusPoseId.RunningAimDiagonalUpRightPose or
        SamusPoseId.RunningAimDiagonalDownRightPose or
        SamusPoseId.UnusedPose45 => BottomMovingRightNormalBase,
        SamusPoseId.MovingLeftNormalPose or
        SamusPoseId.MovingLeftGunExtendedPose or
        SamusPoseId.RunningAimUpLeftPose or
        SamusPoseId.RunningAimDiagonalUpLeftPose or
        SamusPoseId.RunningAimDiagonalDownLeftPose or
        SamusPoseId.UnusedPose46 => BottomMovingLeftNormalBase,
        SamusPoseId.NormalJumpGunExtendedRightPose or
        SamusPoseId.UnusedPoseAC => BottomNormalJumpGunExtendedRightBase,
        SamusPoseId.NormalJumpGunExtendedLeftPose or
        SamusPoseId.UnusedPoseAD => BottomNormalJumpGunExtendedLeftBase,
        SamusPoseId.NormalJumpAimUpRightPose => BottomNormalJumpAimUpRightBase,
        SamusPoseId.NormalJumpAimUpLeftPose => BottomNormalJumpAimUpLeftBase,
        SamusPoseId.NormalJumpAimDownRightPose or
        SamusPoseId.UnusedPoseAE => BottomNormalJumpAimDownRightBase,
        SamusPoseId.NormalJumpAimDownLeftPose or
        SamusPoseId.UnusedPoseAF => BottomNormalJumpAimDownLeftBase,
        SamusPoseId.SpinJumpRightPose => BottomSpinJumpRightBase,
        SamusPoseId.SpinJumpLeftPose => BottomSpinJumpLeftBase,
        SamusPoseId.SpaceJumpRightPose => BottomSpaceJumpRightBase,
        SamusPoseId.SpaceJumpLeftPose => BottomSpaceJumpLeftBase,
        SamusPoseId.MorphBallGroundRightPose or
        SamusPoseId.MorphBallFallingRightPose or
        SamusPoseId.MorphBallFallingLeftPose or
        SamusPoseId.UnusedPose3F or
        SamusPoseId.UnusedPose40 => BottomMorphBallGroundRightBase,
        SamusPoseId.MorphBallMovingRightPose => BottomMorphBallMovingRightBase,
        SamusPoseId.MorphBallMovingLeftPose => BottomMorphBallMovingLeftBase,
        SamusPoseId.UnusedPose20 or
        SamusPoseId.UnusedPose21 or
        SamusPoseId.UnusedPose22 or
        SamusPoseId.UnusedPose23 or
        SamusPoseId.UnusedPose24 or
        SamusPoseId.UnusedKnockbackRightPose or
        SamusPoseId.UnusedKnockbackLeftPose or
        SamusPoseId.UnusedPose39 or
        SamusPoseId.UnusedPose3A or
        SamusPoseId.UnusedPose42 => BottomUnusedPose20Base,
        SamusPoseId.TurningRightToLeftPose or
        SamusPoseId.TurningRightToLeftAimUpPose or
        SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
        SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
        SamusPoseId.MoonwalkTurnJumpLeftPose or
        SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
        SamusPoseId.MoonwalkTurnJumpAimDownLeftPose or
        SamusPoseId.UnusedPoseC6 => BottomTurningRightToLeftBase,
        SamusPoseId.TurningLeftToRightPose or
        SamusPoseId.TurningLeftToRightAimUpPose or
        SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
        SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
        SamusPoseId.MoonwalkTurnJumpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimDownRightPose => BottomTurningLeftToRightBase,
        SamusPoseId.CrouchingRightPose or
        SamusPoseId.GrappleCrouchingRightPose => BottomCrouchingRightBase,
        SamusPoseId.CrouchingLeftPose or
        SamusPoseId.GrappleCrouchingLeftPose => BottomCrouchingLeftBase,
        SamusPoseId.FallingRightPose or
        SamusPoseId.FallingGunExtendedRightPose => BottomFallingRightBase,
        SamusPoseId.FallingLeftPose or
        SamusPoseId.FallingGunExtendedLeftPose => BottomFallingLeftBase,
        SamusPoseId.FallingAimUpRightPose => BottomFallingAimUpRightBase,
        SamusPoseId.FallingAimUpLeftPose => BottomFallingAimUpLeftBase,
        SamusPoseId.FallingAimDownRightPose => BottomFallingAimDownRightBase,
        SamusPoseId.FallingAimDownLeftPose => BottomFallingAimDownLeftBase,
        SamusPoseId.TurningRightToLeftJumpPose or
        SamusPoseId.TurningRightToLeftCrouchingPose or
        SamusPoseId.TurningRightToLeftFallingPose or
        SamusPoseId.TurningRightToLeftJumpAimUpPose or
        SamusPoseId.TurningRightToLeftJumpAimDownPose or
        SamusPoseId.TurningRightToLeftFallingAimUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDownPose or
        SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or
        SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose => BottomTurningRightToLeftJumpBase,
        SamusPoseId.TurningLeftToRightJumpPose or
        SamusPoseId.TurningLeftToRightCrouchingPose or
        SamusPoseId.TurningLeftToRightJumpAimUpPose or
        SamusPoseId.TurningLeftToRightJumpAimDownPose or
        SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or
        SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose => BottomTurningLeftToRightJumpBase,
        SamusPoseId.CrouchingTransitionRightPose or
        SamusPoseId.StandingTransitionRightPose or
        SamusPoseId.CrouchingTransitionAimUpRightPose or
        SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
        SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
        SamusPoseId.StandingTransitionAimUpRightPose or
        SamusPoseId.StandingTransitionAimDiagonalUpRightPose or
        SamusPoseId.StandingTransitionAimDiagonalDownRightPose => BottomCrouchingTransitionRightBase,
        SamusPoseId.CrouchingTransitionLeftPose or
        SamusPoseId.StandingTransitionLeftPose or
        SamusPoseId.CrouchingTransitionAimUpLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
        SamusPoseId.StandingTransitionAimUpLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalUpLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => BottomCrouchingTransitionLeftBase,
        SamusPoseId.MorphingTransitionRightPose => BottomMorphingTransitionRightBase,
        SamusPoseId.MorphingTransitionLeftPose => BottomMorphingTransitionLeftBase,
        SamusPoseId.UnmorphingTransitionRightPose => BottomUnmorphingTransitionRightBase,
        SamusPoseId.UnmorphingTransitionLeftPose => BottomUnmorphingTransitionLeftBase,
        SamusPoseId.MorphBallGroundLeftPose or
        SamusPoseId.UnusedPoseC5 or
        SamusPoseId.UnusedPoseDf => BottomMorphBallGroundLeftBase,
        SamusPoseId.MoonwalkFacingLeftPose or
        SamusPoseId.MoonwalkAimUpLeftPose or
        SamusPoseId.MoonwalkAimDownLeftPose => BottomMoonwalkFacingLeftBase,
        SamusPoseId.MoonwalkFacingRightPose or
        SamusPoseId.MoonwalkAimUpRightPose or
        SamusPoseId.MoonwalkAimDownRightPose => BottomMoonwalkFacingRightBase,
        SamusPoseId.NeutralJumpTransitionRightPose or
        SamusPoseId.NormalJumpTransitionAimUpRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose => BottomNeutralJumpTransitionRightBase,
        SamusPoseId.NeutralJumpTransitionLeftPose or
        SamusPoseId.NormalJumpTransitionAimUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose => BottomNeutralJumpTransitionLeftBase,
        SamusPoseId.NeutralJumpRightPose or
        SamusPoseId.ShinesparkWindupRightPose => BottomNeutralJumpRightBase,
        SamusPoseId.NeutralJumpLeftPose or
        SamusPoseId.ShinesparkWindupLeftPose => BottomNeutralJumpLeftBase,
        SamusPoseId.DamageBoostLeftPose => BottomDamageBoostLeftBase,
        SamusPoseId.DamageBoostRightPose => BottomDamageBoostRightBase,
        SamusPoseId.NormalJumpForwardRightPose => BottomNormalJumpForwardRightBase,
        SamusPoseId.NormalJumpForwardLeftPose => BottomNormalJumpForwardLeftBase,
        SamusPoseId.KnockbackRightPose => BottomKnockbackRightBase,
        SamusPoseId.KnockbackLeftPose => BottomKnockbackLeftBase,
        SamusPoseId.UnusedPose5B or
        SamusPoseId.GrappleWallContactLeftPose => BottomUnusedPose5BBase,
        SamusPoseId.UnusedPose5C or
        SamusPoseId.GrappleWallContactRightPose => BottomUnusedPose5CBase,
        SamusPoseId.UnusedPose5D or
        SamusPoseId.UnusedPose5E or
        SamusPoseId.UnusedPose5F or
        SamusPoseId.UnusedPose60 or
        SamusPoseId.UnusedPose61 or
        SamusPoseId.GrappleSwingRightPose => BottomUnusedPose5DBase,
        SamusPoseId.UnusedPose62 or
        SamusPoseId.GrappleSwingLeftPose => BottomUnusedPose62Base,
        SamusPoseId.UnusedPose63 => BottomUnusedPose63Base,
        SamusPoseId.UnusedPose64 => BottomUnusedPose64Base,
        SamusPoseId.UnusedPose65 => BottomUnusedPose65Base,
        SamusPoseId.UnusedPose66 => BottomUnusedPose66Base,
        SamusPoseId.NormalJumpAimDiagonalUpRightPose => BottomNormalJumpAimDiagonalUpRightBase,
        SamusPoseId.NormalJumpAimDiagonalUpLeftPose => BottomNormalJumpAimDiagonalUpLeftBase,
        SamusPoseId.NormalJumpAimDiagonalDownRightPose or
        SamusPoseId.UnusedPoseB0 => BottomNormalJumpAimDiagonalDownRightBase,
        SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
        SamusPoseId.UnusedPoseB1 => BottomNormalJumpAimDiagonalDownLeftBase,
        SamusPoseId.FallingAimDiagonalUpRightPose => BottomFallingAimDiagonalUpRightBase,
        SamusPoseId.FallingAimDiagonalUpLeftPose => BottomFallingAimDiagonalUpLeftBase,
        SamusPoseId.FallingAimDiagonalDownRightPose => BottomFallingAimDiagonalDownRightBase,
        SamusPoseId.FallingAimDiagonalDownLeftPose => BottomFallingAimDiagonalDownLeftBase,
        SamusPoseId.CrouchingAimDiagonalUpRightPose => BottomCrouchingAimDiagonalUpRightBase,
        SamusPoseId.CrouchingAimDiagonalUpLeftPose => BottomCrouchingAimDiagonalUpLeftBase,
        SamusPoseId.CrouchingAimDiagonalDownRightPose or
        SamusPoseId.GrappleCrouchingDownRightPose => BottomCrouchingAimDiagonalDownRightBase,
        SamusPoseId.CrouchingAimDiagonalDownLeftPose or
        SamusPoseId.GrappleCrouchingDownLeftPose => BottomCrouchingAimDiagonalDownLeftBase,
        SamusPoseId.SpringBallGroundRightPose or
        SamusPoseId.SpringBallMovingRightPose or
        SamusPoseId.SpringBallFallingRightPose or
        SamusPoseId.SpringBallJumpRightPose => BottomSpringBallGroundRightBase,
        SamusPoseId.SpringBallGroundLeftPose or
        SamusPoseId.SpringBallMovingLeftPose or
        SamusPoseId.SpringBallFallingLeftPose or
        SamusPoseId.SpringBallJumpLeftPose => BottomSpringBallGroundLeftBase,
        SamusPoseId.ScrewAttackRightPose => BottomScrewAttackRightBase,
        SamusPoseId.ScrewAttackLeftPose => BottomScrewAttackLeftBase,
        SamusPoseId.WallJumpRightPose => BottomWallJumpRightBase,
        SamusPoseId.WallJumpLeftPose => BottomWallJumpLeftBase,
        SamusPoseId.CrouchingAimUpRightPose => BottomCrouchingAimUpRightBase,
        SamusPoseId.CrouchingAimUpLeftPose => BottomCrouchingAimUpLeftBase,
        SamusPoseId.TurningLeftToRightFallingPose or
        SamusPoseId.TurningLeftToRightFallingAimUpPose or
        SamusPoseId.TurningLeftToRightFallingAimDownPose or
        SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose => BottomTurningLeftToRightFallingBase,
        SamusPoseId.ForwardFacingSuitedPose => BottomForwardFacingSuitedBase,
        SamusPoseId.NormalLandingRightPose or
        SamusPoseId.LandingAimUpRightPose or
        SamusPoseId.LandingAimDiagonalUpRightPose or
        SamusPoseId.LandingAimDiagonalDownRightPose or
        SamusPoseId.FiringLandingRightPose => BottomNormalLandingRightBase,
        SamusPoseId.NormalLandingLeftPose or
        SamusPoseId.LandingAimUpLeftPose or
        SamusPoseId.LandingAimDiagonalUpLeftPose or
        SamusPoseId.LandingAimDiagonalDownLeftPose or
        SamusPoseId.FiringLandingLeftPose => BottomNormalLandingLeftBase,
        SamusPoseId.SpinLandingRightPose => BottomSpinLandingRightBase,
        SamusPoseId.SpinLandingLeftPose => BottomSpinLandingLeftBase,
        SamusPoseId.DraygonGrabbedNeutralLeftPose or
        SamusPoseId.DraygonGrabbedAimUpLeftPose or
        SamusPoseId.DraygonGrabbedFiringLeftPose or
        SamusPoseId.DraygonGrabbedAimDownLeftPose => BottomDraygonGrabbedNeutralLeftBase,
        SamusPoseId.DraygonGrabbedMovingLeftPose => BottomDraygonGrabbedMovingLeftBase,
        SamusPoseId.ShinesparkHorizontalRightPose => BottomShinesparkHorizontalRightBase,
        SamusPoseId.ShinesparkHorizontalLeftPose => BottomShinesparkHorizontalLeftBase,
        SamusPoseId.ShinesparkVerticalRightPose => BottomShinesparkVerticalRightBase,
        SamusPoseId.ShinesparkVerticalLeftPose => BottomShinesparkVerticalLeftBase,
        SamusPoseId.ShinesparkDiagonalRightPose => BottomShinesparkDiagonalRightBase,
        SamusPoseId.ShinesparkDiagonalLeftPose => BottomShinesparkDiagonalLeftBase,
        SamusPoseId.CrystalFlashRightPose => BottomCrystalFlashRightBase,
        SamusPoseId.CrystalFlashLeftPose => BottomCrystalFlashLeftBase,
        SamusPoseId.XrayingStandingRightPose => BottomXrayingStandingRightBase,
        SamusPoseId.XrayingStandingLeftPose => BottomXrayingStandingLeftBase,
        SamusPoseId.DeathSequenceRightPose => BottomDeathSequenceRightBase,
        SamusPoseId.DeathSequenceLeftPose => BottomDeathSequenceLeftBase,
        SamusPoseId.XrayingCrouchingRightPose => BottomXrayingCrouchingRightBase,
        SamusPoseId.XrayingCrouchingLeftPose => BottomXrayingCrouchingLeftBase,
        SamusPoseId.UnusedPoseDb => BottomUnusedPoseDbBase,
        SamusPoseId.UnusedPoseDc => BottomUnusedPoseDcBase,
        SamusPoseId.UnusedPoseDd => BottomUnusedPoseDdBase,
        SamusPoseId.UnusedPoseDe => BottomUnusedPoseDeBase,
        SamusPoseId.DrainedCrouchingRightPose => BottomDrainedCrouchingRightBase,
        SamusPoseId.DrainedCrouchingLeftPose => BottomDrainedCrouchingLeftBase,
        SamusPoseId.DrainedStandingRightPose => BottomDrainedStandingRightBase,
        SamusPoseId.DrainedStandingLeftPose => BottomDrainedStandingLeftBase,
        SamusPoseId.DraygonGrabbedNeutralRightPose or
        SamusPoseId.DraygonGrabbedAimUpRightPose or
        SamusPoseId.DraygonGrabbedFiringRightPose or
        SamusPoseId.DraygonGrabbedAimDownRightPose => BottomDraygonGrabbedNeutralRightBase,
        SamusPoseId.DraygonGrabbedMovingRightPose => BottomDraygonGrabbedMovingRightBase,
        _ => throw new ArgumentOutOfRangeException(nameof(pose)),
    };

}
