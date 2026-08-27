using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// First breakpoint-friendly slice of normal gameplay Samus state and rendering.
/// </summary>
/// <remarks>
/// This class owns the state shared by the exact standing/right-running render, animation,
/// and grounded movement slices. It does not claim to port damage, equipment, most poses,
/// or the Landing Site cinematic actor. Keeping those boundaries blunt is preferable to
/// filling unported bank-$90 behavior with plausible-looking host code.
/// </remarks>
public sealed class SamusState
{
    private const int PoseDefinitions = 0x91b629;
    private const int AnimationDelayPointerTable = 0x91b010;
    private const int TopSpritemapBaseIndexTable = 0x929263;
    private const int BottomSpritemapBaseIndexTable = 0x92945d;
    private const int PowerSuitPalette = 0x9b9400;

    /// <summary>
    /// Pose `$00`: the power-suit body viewed from the front during the intro, elevators,
    /// save-station appearance, and other controller-locked sequences.
    /// </summary>
    public const byte ForwardFacingPowerSuitPose = 0x00;

    /// <summary>
    /// Pose `$9B`: the Varia/Gravity-suit counterpart of pose `$00`. Both use movement type
    /// zero, but only the power-suit record needs bank `$90`'s extra chest-cover OBJ.
    /// </summary>
    public const byte ForwardFacingSuitedPose = 0x9b;

    /// <summary>Pose $01 is “facing right - normal” in the cartridge table.</summary>
    public const byte FacingRightNormalPose = 0x01;

    /// <summary>Pose $02 is “facing left - normal” in the cartridge table.</summary>
    public const byte FacingLeftNormalPose = 0x02;

    /// <summary>Pose $09 is “moving right - not aiming” in the cartridge table.</summary>
    public const byte MovingRightNormalPose = 0x09;

    /// <summary>Pose $0A is “moving left - not aiming” in the cartridge table.</summary>
    public const byte MovingLeftNormalPose = 0x0a;

    /// <summary>
    /// Pose `$0B` is the right-moving horizontal-fire body. It is a genuine movement-type-one
    /// record, shares the ordinary ten-frame running delay stream, and is selected when Shot
    /// remains held with Right after the projectile direction has been established.
    /// </summary>
    public const byte MovingRightGunExtendedPose = 0x0b;

    /// <summary>Pose `$0C` is the left-moving mirror of <see cref="MovingRightGunExtendedPose"/>.</summary>
    public const byte MovingLeftGunExtendedPose = 0x0c;

    /// <summary>
    /// Pose $49 visually faces left while moonwalking right. Its pose-X byte is deliberately
    /// eight: native horizontal movement follows travel direction, not the artwork's facing.
    /// </summary>
    public const byte MoonwalkFacingLeftPose = 0x49;

    /// <summary>Pose $4A visually faces right while moonwalking left.</summary>
    public const byte MoonwalkFacingRightPose = 0x4a;

    /// <summary>Pose `$75`: left-facing/right-moving moonwalk aimed diagonally up-left.</summary>
    public const byte MoonwalkAimUpLeftPose = 0x75;

    /// <summary>Pose `$76`: right-facing/left-moving mirror of pose `$75`.</summary>
    public const byte MoonwalkAimUpRightPose = 0x76;

    /// <summary>Pose `$77`: left-facing/right-moving moonwalk aimed diagonally down-left.</summary>
    public const byte MoonwalkAimDownLeftPose = 0x77;

    /// <summary>Pose `$78`: right-facing/left-moving mirror of pose `$77`.</summary>
    public const byte MoonwalkAimDownRightPose = 0x78;

    /// <summary>
    /// Pose `$BF`: right-facing moonwalk art turning/jumping toward the left. Despite the
    /// name in the disassembly, this is still grounded movement type `$0E` until its input
    /// table or terminal `$F8,$1A` animation command installs the actual spin jump.
    /// </summary>
    public const byte MoonwalkTurnJumpLeftPose = 0xbf;

    /// <summary>Pose `$C0`: left-facing mirror of <see cref="MoonwalkTurnJumpLeftPose"/>.</summary>
    public const byte MoonwalkTurnJumpRightPose = 0xc0;

    /// <summary>Pose `$C1`: aimed-up moonwalk turn whose terminal jump faces left.</summary>
    public const byte MoonwalkTurnJumpAimUpLeftPose = 0xc1;

    /// <summary>Pose `$C2`: aimed-up moonwalk turn whose terminal jump faces right.</summary>
    public const byte MoonwalkTurnJumpAimUpRightPose = 0xc2;

    /// <summary>Pose `$C3`: aimed-down moonwalk turn whose terminal jump faces left.</summary>
    public const byte MoonwalkTurnJumpAimDownLeftPose = 0xc3;

    /// <summary>Pose `$C4`: aimed-down moonwalk turn whose terminal jump faces right.</summary>
    public const byte MoonwalkTurnJumpAimDownRightPose = 0xc4;

    /// <summary>Pose `$C7`: right-facing stored-shine windup body.</summary>
    public const byte ShinesparkWindupRightPose = 0xc7;

    /// <summary>Pose `$C8`: left-facing stored-shine windup body.</summary>
    public const byte ShinesparkWindupLeftPose = 0xc8;

    /// <summary>Pose `$C9`: horizontal shinespark travelling right.</summary>
    public const byte ShinesparkHorizontalRightPose = 0xc9;

    /// <summary>Pose `$CA`: horizontal shinespark travelling left.</summary>
    public const byte ShinesparkHorizontalLeftPose = 0xca;

    /// <summary>Pose `$CB`: vertical shinespark using right-facing metadata.</summary>
    public const byte ShinesparkVerticalRightPose = 0xcb;

    /// <summary>Pose `$CC`: vertical shinespark using left-facing metadata.</summary>
    public const byte ShinesparkVerticalLeftPose = 0xcc;

    /// <summary>Pose `$CD`: diagonal up-right shinespark.</summary>
    public const byte ShinesparkDiagonalRightPose = 0xcd;

    /// <summary>Pose `$CE`: diagonal up-left shinespark.</summary>
    public const byte ShinesparkDiagonalLeftPose = 0xce;

    /// <summary>Pose `$D3`: right-facing Crystal Flash body and delay program.</summary>
    public const byte CrystalFlashRightPose = 0xd3;

    /// <summary>Pose `$D4`: left-facing Crystal Flash mirror.</summary>
    public const byte CrystalFlashLeftPose = 0xd4;

    /// <summary>Pose `$D5`: right-facing standing X-ray body.</summary>
    public const byte XrayingStandingRightPose = 0xd5;

    /// <summary>Pose `$D6`: left-facing standing X-ray body.</summary>
    public const byte XrayingStandingLeftPose = 0xd6;

    /// <summary>Pose `$D7`: right-facing fatal-damage / crystal-flash-ending body.</summary>
    public const byte DeathSequenceRightPose = 0xd7;

    /// <summary>Pose `$D8`: left-facing fatal-damage / crystal-flash-ending mirror.</summary>
    public const byte DeathSequenceLeftPose = 0xd8;

    /// <summary>Pose `$D9`: right-facing crouching X-ray body.</summary>
    public const byte XrayingCrouchingRightPose = 0xd9;

    /// <summary>Pose `$DA`: left-facing crouching X-ray body.</summary>
    public const byte XrayingCrouchingLeftPose = 0xda;

    /// <summary>Pose `$E8`: right-facing drained crouch/fall animation.</summary>
    public const byte DrainedCrouchingRightPose = 0xe8;

    /// <summary>Pose `$E9`: left-facing drained crouch/fall animation.</summary>
    public const byte DrainedCrouchingLeftPose = 0xe9;

    /// <summary>Pose `$EA`: right-facing drained standing animation.</summary>
    public const byte DrainedStandingRightPose = 0xea;

    /// <summary>Pose `$EB`: left-facing drained standing animation.</summary>
    public const byte DrainedStandingLeftPose = 0xeb;

    /// <summary>Pose `$89`: facing right after forward running collides with a wall.</summary>
    public const byte RanIntoWallRightPose = 0x89;

    /// <summary>Pose `$8A`: facing-left mirror of pose `$89`.</summary>
    public const byte RanIntoWallLeftPose = 0x8a;

    /// <summary>Pose `$CF`: right-facing wall-stop art aimed diagonally up-right.</summary>
    public const byte RanIntoWallAimUpRightPose = 0xcf;

    /// <summary>Pose `$D0`: left-facing wall-stop art aimed diagonally up-left.</summary>
    public const byte RanIntoWallAimUpLeftPose = 0xd0;

    /// <summary>Pose `$D1`: right-facing wall-stop art aimed diagonally down-right.</summary>
    public const byte RanIntoWallAimDownRightPose = 0xd1;

    /// <summary>Pose `$D2`: left-facing wall-stop art aimed diagonally down-left.</summary>
    public const byte RanIntoWallAimDownLeftPose = 0xd2;

    /// <summary>Pose $25 turns a right-facing grounded Samus toward the left.</summary>
    public const byte TurningRightToLeftPose = 0x25;

    /// <summary>Pose $26 turns a left-facing grounded Samus toward the right.</summary>
    public const byte TurningLeftToRightPose = 0x26;

    /// <summary>Pose $19 is the ordinary right-facing spin jump.</summary>
    public const byte SpinJumpRightPose = 0x19;

    /// <summary>Pose $1A is the ordinary left-facing spin jump.</summary>
    public const byte SpinJumpLeftPose = 0x1a;

    /// <summary>
    /// Pose `$1B` is the right-facing Space Jump body. The equipment-aware movement-type
    /// initializer at `$91:F624` substitutes this for the transition table's ordinary
    /// `$19` target when Space Jump is equipped and Screw Attack is not.
    /// </summary>
    public const byte SpaceJumpRightPose = 0x1b;

    /// <summary>Pose `$1C` is the left-facing mirror of <see cref="SpaceJumpRightPose"/>.</summary>
    public const byte SpaceJumpLeftPose = 0x1c;

    /// <summary>
    /// Pose `$81` is the right-facing Screw Attack body. Screw Attack bit `$0008` has
    /// priority over Space Jump bit `$0200` in the native spin-pose selector.
    /// </summary>
    public const byte ScrewAttackRightPose = 0x81;

    /// <summary>Pose `$82` is the left-facing mirror of <see cref="ScrewAttackRightPose"/>.</summary>
    public const byte ScrewAttackLeftPose = 0x82;

    /// <summary>Pose $2F turns a right-facing normal jump toward the left.</summary>
    public const byte TurningRightToLeftJumpPose = 0x2f;

    /// <summary>Pose $30 turns a left-facing normal jump toward the right.</summary>
    public const byte TurningLeftToRightJumpPose = 0x30;

    /// <summary>Pose $83 is the right-facing wall-jump launch animation.</summary>
    public const byte WallJumpRightPose = 0x83;

    /// <summary>Pose $84 is the left-facing wall-jump launch animation.</summary>
    public const byte WallJumpLeftPose = 0x84;

    /// <summary>Pose $B2 is the right-facing airborne grapple-swing body.</summary>
    public const byte GrappleSwingRightPose = 0xb2;

    /// <summary>Pose $B3 is the left-facing airborne grapple-swing body.</summary>
    public const byte GrappleSwingLeftPose = 0xb3;

    /// <summary>Pose $A8 is the standing right-facing horizontal grapple lock.</summary>
    public const byte GrappleStandingRightPose = 0xa8;

    /// <summary>Pose $A9 is the standing left-facing horizontal grapple lock.</summary>
    public const byte GrappleStandingLeftPose = 0xa9;

    /// <summary>Pose $AA is the standing right-facing down-right grapple lock.</summary>
    public const byte GrappleStandingDownRightPose = 0xaa;

    /// <summary>Pose $AB is the standing left-facing down-left grapple lock.</summary>
    public const byte GrappleStandingDownLeftPose = 0xab;

    /// <summary>Pose $B4 is the crouched right-facing horizontal grapple lock.</summary>
    public const byte GrappleCrouchingRightPose = 0xb4;

    /// <summary>Pose $B5 is the crouched left-facing horizontal grapple lock.</summary>
    public const byte GrappleCrouchingLeftPose = 0xb5;

    /// <summary>Pose $B6 is the crouched right-facing down-right grapple lock.</summary>
    public const byte GrappleCrouchingDownRightPose = 0xb6;

    /// <summary>Pose $B7 is the crouched left-facing down-left grapple lock.</summary>
    public const byte GrappleCrouchingDownLeftPose = 0xb7;

    /// <summary>Pose $B8 is the left-wall grapple-jump contact body.</summary>
    public const byte GrappleWallContactLeftPose = 0xb8;

    /// <summary>Pose $B9 is the right-wall grapple-jump contact body.</summary>
    public const byte GrappleWallContactRightPose = 0xb9;

    /// <summary>Pose `$BA`: left-facing neutral body held in Draygon's claws.</summary>
    public const byte DraygonGrabbedNeutralLeftPose = 0xba;

    /// <summary>Pose `$BB`: left-facing Draygon-held body aiming diagonally upward.</summary>
    public const byte DraygonGrabbedAimUpLeftPose = 0xbb;

    /// <summary>Pose `$BC`: left-facing Draygon-held firing body.</summary>
    public const byte DraygonGrabbedFiringLeftPose = 0xbc;

    /// <summary>Pose `$BD`: left-facing Draygon-held body aiming diagonally downward.</summary>
    public const byte DraygonGrabbedAimDownLeftPose = 0xbd;

    /// <summary>Pose `$BE`: left-facing six-frame struggling animation while held by Draygon.</summary>
    public const byte DraygonGrabbedMovingLeftPose = 0xbe;

    /// <summary>Pose `$EC`: right-facing neutral body held in Draygon's claws.</summary>
    public const byte DraygonGrabbedNeutralRightPose = 0xec;

    /// <summary>Pose `$ED`: right-facing Draygon-held body aiming diagonally upward.</summary>
    public const byte DraygonGrabbedAimUpRightPose = 0xed;

    /// <summary>Pose `$EE`: right-facing Draygon-held firing body.</summary>
    public const byte DraygonGrabbedFiringRightPose = 0xee;

    /// <summary>Pose `$EF`: right-facing Draygon-held body aiming diagonally downward.</summary>
    public const byte DraygonGrabbedAimDownRightPose = 0xef;

    /// <summary>Pose `$F0`: right-facing six-frame struggling animation while held by Draygon.</summary>
    public const byte DraygonGrabbedMovingRightPose = 0xf0;

    /// <summary>
    /// Pose $4F is the visually left-facing damage boost; its X-direction byte is the
    /// deliberately reversed value eight that drives the boost to the right.
    /// </summary>
    public const byte DamageBoostLeftPose = 0x4f;

    /// <summary>
    /// Pose $50 is the visually right-facing damage boost; its X-direction byte is the
    /// deliberately reversed value four that drives the boost to the left.
    /// </summary>
    public const byte DamageBoostRightPose = 0x50;

    /// <summary>Pose $53 is the right-facing normal knockback body.</summary>
    public const byte KnockbackRightPose = 0x53;

    /// <summary>Pose $54 is the left-facing normal knockback body.</summary>
    public const byte KnockbackLeftPose = 0x54;

    /// <summary>Pose $87 turns a right-facing fall toward the left.</summary>
    public const byte TurningRightToLeftFallingPose = 0x87;

    /// <summary>Pose $88 turns a left-facing fall toward the right.</summary>
    public const byte TurningLeftToRightFallingPose = 0x88;

    public const byte TurningRightToLeftJumpAimUpPose = 0x8f;
    public const byte TurningLeftToRightJumpAimUpPose = 0x90;
    public const byte TurningRightToLeftJumpAimDownPose = 0x91;
    public const byte TurningLeftToRightJumpAimDownPose = 0x92;
    public const byte TurningRightToLeftFallingAimUpPose = 0x93;
    public const byte TurningLeftToRightFallingAimUpPose = 0x94;
    public const byte TurningRightToLeftFallingAimDownPose = 0x95;
    public const byte TurningLeftToRightFallingAimDownPose = 0x96;
    public const byte TurningRightToLeftJumpAimDiagonalUpPose = 0x9e;
    public const byte TurningLeftToRightJumpAimDiagonalUpPose = 0x9f;
    public const byte TurningRightToLeftFallingAimDiagonalUpPose = 0xa0;
    public const byte TurningLeftToRightFallingAimDiagonalUpPose = 0xa1;

    /// <summary>Pose $1D is the stationary right-facing ordinary morph ball on the ground.</summary>
    public const byte MorphBallGroundRightPose = 0x1d;

    /// <summary>Pose $1E is the ordinary morph ball rolling to the right on the ground.</summary>
    public const byte MorphBallMovingRightPose = 0x1e;

    /// <summary>Pose $1F is the ordinary morph ball rolling to the left on the ground.</summary>
    public const byte MorphBallMovingLeftPose = 0x1f;

    /// <summary>Pose $31 is the right-facing ordinary morph ball in the air.</summary>
    public const byte MorphBallFallingRightPose = 0x31;

    /// <summary>Pose $32 is the left-facing ordinary morph ball in the air.</summary>
    public const byte MorphBallFallingLeftPose = 0x32;

    /// <summary>Pose $37 is the right-facing crouch-to-morph transition.</summary>
    public const byte MorphingTransitionRightPose = 0x37;

    /// <summary>Pose $38 is the left-facing crouch-to-morph transition.</summary>
    public const byte MorphingTransitionLeftPose = 0x38;

    /// <summary>Pose $3D is the right-facing morph-to-crouch transition.</summary>
    public const byte UnmorphingTransitionRightPose = 0x3d;

    /// <summary>Pose $3E is the left-facing morph-to-crouch transition.</summary>
    public const byte UnmorphingTransitionLeftPose = 0x3e;

    /// <summary>Pose $41 is the stationary left-facing ordinary morph ball on the ground.</summary>
    public const byte MorphBallGroundLeftPose = 0x41;

    /// <summary>Pose $79 is the stationary right-facing Spring Ball on the ground.</summary>
    public const byte SpringBallGroundRightPose = 0x79;

    /// <summary>Pose $7A is the stationary left-facing Spring Ball on the ground.</summary>
    public const byte SpringBallGroundLeftPose = 0x7a;

    /// <summary>Pose $7B is the Spring Ball moving right on the ground.</summary>
    public const byte SpringBallMovingRightPose = 0x7b;

    /// <summary>Pose $7C is the Spring Ball moving left on the ground.</summary>
    public const byte SpringBallMovingLeftPose = 0x7c;

    /// <summary>Pose $7D is the right-facing Spring Ball falling or bouncing.</summary>
    public const byte SpringBallFallingRightPose = 0x7d;

    /// <summary>Pose $7E is the left-facing Spring Ball falling or bouncing.</summary>
    public const byte SpringBallFallingLeftPose = 0x7e;

    /// <summary>Pose $7F is the right-facing Spring Ball in its powered jump.</summary>
    public const byte SpringBallJumpRightPose = 0x7f;

    /// <summary>Pose $80 is the left-facing Spring Ball in its powered jump.</summary>
    public const byte SpringBallJumpLeftPose = 0x80;

    /// <summary>Pose $29 is the unaimed right-facing falling pose.</summary>
    public const byte FallingRightPose = 0x29;

    /// <summary>Pose $2A is the unaimed left-facing falling pose.</summary>
    public const byte FallingLeftPose = 0x2a;

    /// <summary>
    /// Pose `$67` is the right-facing falling body with the cannon held horizontally after
    /// firing. Its `$91:B353` delay stream has the same terminal-velocity split as `$29`.
    /// </summary>
    public const byte FallingGunExtendedRightPose = 0x67;

    /// <summary>Pose `$68` is the left-facing mirror of <see cref="FallingGunExtendedRightPose"/>.</summary>
    public const byte FallingGunExtendedLeftPose = 0x68;

    /// <summary>Pose $03 is stationary, facing right, with the arm cannon aimed straight up.</summary>
    public const byte StandingAimUpRightPose = 0x03;

    /// <summary>Pose $04 is stationary, facing left, with the arm cannon aimed straight up.</summary>
    public const byte StandingAimUpLeftPose = 0x04;

    /// <summary>Pose $05 is stationary, facing right, and aiming diagonally up-right.</summary>
    public const byte StandingAimDiagonalUpRightPose = 0x05;

    /// <summary>Pose $06 is stationary, facing left, and aiming diagonally up-left.</summary>
    public const byte StandingAimDiagonalUpLeftPose = 0x06;

    /// <summary>Pose $07 is stationary, facing right, and aiming diagonally down-right.</summary>
    public const byte StandingAimDiagonalDownRightPose = 0x07;

    /// <summary>Pose $08 is stationary, facing left, and aiming diagonally down-left.</summary>
    public const byte StandingAimDiagonalDownLeftPose = 0x08;

    /// <summary>Pose $0D is the unused right-moving straight-up aim record.</summary>
    public const byte RunningAimUpRightPose = 0x0d;

    /// <summary>Pose $0E is the unused left-moving straight-up aim record.</summary>
    public const byte RunningAimUpLeftPose = 0x0e;

    /// <summary>Pose $0F moves right while aiming diagonally up-right.</summary>
    public const byte RunningAimDiagonalUpRightPose = 0x0f;

    /// <summary>Pose $10 moves left while aiming diagonally up-left.</summary>
    public const byte RunningAimDiagonalUpLeftPose = 0x10;

    /// <summary>Pose $11 moves right while aiming diagonally down-right.</summary>
    public const byte RunningAimDiagonalDownRightPose = 0x11;

    /// <summary>Pose $12 moves left while aiming diagonally down-left.</summary>
    public const byte RunningAimDiagonalDownLeftPose = 0x12;

    /// <summary>
    /// Pose `$13` is a stationary right-facing normal jump with the cannon extended. Unlike
    /// `$4D`, its pose table deliberately stores no no-input fallback, so the firing body can
    /// remain active until another input-table record or landing replaces it.
    /// </summary>
    public const byte NormalJumpGunExtendedRightPose = 0x13;

    /// <summary>Pose `$14` is the left-facing mirror of <see cref="NormalJumpGunExtendedRightPose"/>.</summary>
    public const byte NormalJumpGunExtendedLeftPose = 0x14;

    /// <summary>Pose $15 is a right-facing normal jump aimed straight up.</summary>
    public const byte NormalJumpAimUpRightPose = 0x15;

    /// <summary>Pose $16 is a left-facing normal jump aimed straight up.</summary>
    public const byte NormalJumpAimUpLeftPose = 0x16;

    /// <summary>Pose $17 is the compact right-facing normal jump aimed straight down.</summary>
    public const byte NormalJumpAimDownRightPose = 0x17;

    /// <summary>Pose $18 is the compact left-facing normal jump aimed straight down.</summary>
    public const byte NormalJumpAimDownLeftPose = 0x18;

    /// <summary>Pose $51 is a right-facing normal jump using the moving-forward art.</summary>
    public const byte NormalJumpForwardRightPose = 0x51;

    /// <summary>Pose $52 is a left-facing normal jump using the moving-forward art.</summary>
    public const byte NormalJumpForwardLeftPose = 0x52;

    /// <summary>Pose $55 is the right-facing jump transition aimed straight up.</summary>
    public const byte NormalJumpTransitionAimUpRightPose = 0x55;

    /// <summary>Pose $56 is the left-facing jump transition aimed straight up.</summary>
    public const byte NormalJumpTransitionAimUpLeftPose = 0x56;

    /// <summary>Pose $57 is the right-facing jump transition aimed diagonally up.</summary>
    public const byte NormalJumpTransitionAimDiagonalUpRightPose = 0x57;

    /// <summary>Pose $58 is the left-facing jump transition aimed diagonally up.</summary>
    public const byte NormalJumpTransitionAimDiagonalUpLeftPose = 0x58;

    /// <summary>Pose $59 is the right-facing jump transition aimed diagonally down.</summary>
    public const byte NormalJumpTransitionAimDiagonalDownRightPose = 0x59;

    /// <summary>Pose $5A is the left-facing jump transition aimed diagonally down.</summary>
    public const byte NormalJumpTransitionAimDiagonalDownLeftPose = 0x5a;

    /// <summary>Pose $69 is a right-facing normal jump aimed diagonally up.</summary>
    public const byte NormalJumpAimDiagonalUpRightPose = 0x69;

    /// <summary>Pose $6A is a left-facing normal jump aimed diagonally up.</summary>
    public const byte NormalJumpAimDiagonalUpLeftPose = 0x6a;

    /// <summary>Pose $6B is a right-facing normal jump aimed diagonally down.</summary>
    public const byte NormalJumpAimDiagonalDownRightPose = 0x6b;

    /// <summary>Pose $6C is a left-facing normal jump aimed diagonally down.</summary>
    public const byte NormalJumpAimDiagonalDownLeftPose = 0x6c;

    /// <summary>Pose $2B is right-facing falling aimed straight up.</summary>
    public const byte FallingAimUpRightPose = 0x2b;

    /// <summary>Pose $2C is left-facing falling aimed straight up.</summary>
    public const byte FallingAimUpLeftPose = 0x2c;

    /// <summary>Pose $2D is the compact right-facing fall aimed straight down.</summary>
    public const byte FallingAimDownRightPose = 0x2d;

    /// <summary>Pose $2E is the compact left-facing fall aimed straight down.</summary>
    public const byte FallingAimDownLeftPose = 0x2e;

    /// <summary>Pose $6D is right-facing falling aimed diagonally up.</summary>
    public const byte FallingAimDiagonalUpRightPose = 0x6d;

    /// <summary>Pose $6E is left-facing falling aimed diagonally up.</summary>
    public const byte FallingAimDiagonalUpLeftPose = 0x6e;

    /// <summary>Pose $6F is right-facing falling aimed diagonally down.</summary>
    public const byte FallingAimDiagonalDownRightPose = 0x6f;

    /// <summary>Pose $70 is left-facing falling aimed diagonally down.</summary>
    public const byte FallingAimDiagonalDownLeftPose = 0x70;

    /// <summary>Pose $71 is right-facing crouching aimed diagonally up.</summary>
    public const byte CrouchingAimDiagonalUpRightPose = 0x71;

    /// <summary>Pose $72 is left-facing crouching aimed diagonally up.</summary>
    public const byte CrouchingAimDiagonalUpLeftPose = 0x72;

    /// <summary>Pose $73 is right-facing crouching aimed diagonally down.</summary>
    public const byte CrouchingAimDiagonalDownRightPose = 0x73;

    /// <summary>Pose $74 is left-facing crouching aimed diagonally down.</summary>
    public const byte CrouchingAimDiagonalDownLeftPose = 0x74;

    /// <summary>Pose $85 is right-facing crouching aimed straight up.</summary>
    public const byte CrouchingAimUpRightPose = 0x85;

    /// <summary>Pose $86 is left-facing crouching aimed straight up.</summary>
    public const byte CrouchingAimUpLeftPose = 0x86;

    /// <summary>Pose $43 turns right-to-left while crouched without retaining aim.</summary>
    public const byte TurningRightToLeftCrouchingPose = 0x43;

    /// <summary>Pose $44 turns left-to-right while crouched without retaining aim.</summary>
    public const byte TurningLeftToRightCrouchingPose = 0x44;

    /// <summary>Pose $8B turns right-to-left on ground while preserving straight-up aim.</summary>
    public const byte TurningRightToLeftAimUpPose = 0x8b;

    /// <summary>Pose $8C turns left-to-right on ground while preserving straight-up aim.</summary>
    public const byte TurningLeftToRightAimUpPose = 0x8c;

    /// <summary>Pose $8D turns right-to-left on ground while preserving diagonal-down aim.</summary>
    public const byte TurningRightToLeftAimDiagonalDownPose = 0x8d;

    /// <summary>Pose $8E turns left-to-right on ground while preserving diagonal-down aim.</summary>
    public const byte TurningLeftToRightAimDiagonalDownPose = 0x8e;

    /// <summary>Pose $9C turns right-to-left on ground while preserving diagonal-up aim.</summary>
    public const byte TurningRightToLeftAimDiagonalUpPose = 0x9c;

    /// <summary>Pose $9D turns left-to-right on ground while preserving diagonal-up aim.</summary>
    public const byte TurningLeftToRightAimDiagonalUpPose = 0x9d;

    /// <summary>Pose $97 turns right-to-left while crouched and preserving straight-up aim.</summary>
    public const byte TurningRightToLeftCrouchingAimUpPose = 0x97;

    /// <summary>Pose $98 turns left-to-right while crouched and preserving straight-up aim.</summary>
    public const byte TurningLeftToRightCrouchingAimUpPose = 0x98;

    /// <summary>Pose $99 turns right-to-left while crouched and preserving diagonal-down aim.</summary>
    public const byte TurningRightToLeftCrouchingAimDiagonalDownPose = 0x99;

    /// <summary>Pose $9A turns left-to-right while crouched and preserving diagonal-down aim.</summary>
    public const byte TurningLeftToRightCrouchingAimDiagonalDownPose = 0x9a;

    /// <summary>Pose $A2 turns right-to-left while crouched and preserving diagonal-up aim.</summary>
    public const byte TurningRightToLeftCrouchingAimDiagonalUpPose = 0xa2;

    /// <summary>Pose $A3 turns left-to-right while crouched and preserving diagonal-up aim.</summary>
    public const byte TurningLeftToRightCrouchingAimDiagonalUpPose = 0xa3;

    /// <summary>Pose $E0 is a right-facing normal-jump landing aimed straight up.</summary>
    public const byte LandingAimUpRightPose = 0xe0;

    /// <summary>Pose $E1 is a left-facing normal-jump landing aimed straight up.</summary>
    public const byte LandingAimUpLeftPose = 0xe1;

    /// <summary>Pose $E2 is a right-facing normal-jump landing aimed diagonally up.</summary>
    public const byte LandingAimDiagonalUpRightPose = 0xe2;

    /// <summary>Pose $E3 is a left-facing normal-jump landing aimed diagonally up.</summary>
    public const byte LandingAimDiagonalUpLeftPose = 0xe3;

    /// <summary>Pose $E4 is a right-facing normal-jump landing aimed diagonally down.</summary>
    public const byte LandingAimDiagonalDownRightPose = 0xe4;

    /// <summary>Pose $E5 is a left-facing normal-jump landing aimed diagonally down.</summary>
    public const byte LandingAimDiagonalDownLeftPose = 0xe5;

    /// <summary>
    /// Pose `$E6` is the right-facing horizontal-fire landing selected by `$91:E95D` only
    /// when the current shot direction is two and the Shot binding is still held at impact.
    /// Its terminal `$F8,$01` command returns to ordinary standing after the landing frames.
    /// </summary>
    public const byte FiringLandingRightPose = 0xe6;

    /// <summary>Pose `$E7` is the left-facing mirror of <see cref="FiringLandingRightPose"/>.</summary>
    public const byte FiringLandingLeftPose = 0xe7;

    /// <summary>Pose $F1 transitions right-facing standing to crouching while aiming up.</summary>
    public const byte CrouchingTransitionAimUpRightPose = 0xf1;

    /// <summary>Pose $F2 transitions left-facing standing to crouching while aiming up.</summary>
    public const byte CrouchingTransitionAimUpLeftPose = 0xf2;

    /// <summary>Pose $F3 transitions right-facing standing to crouching while aiming diagonally up.</summary>
    public const byte CrouchingTransitionAimDiagonalUpRightPose = 0xf3;

    /// <summary>Pose $F4 transitions left-facing standing to crouching while aiming diagonally up.</summary>
    public const byte CrouchingTransitionAimDiagonalUpLeftPose = 0xf4;

    /// <summary>Pose $F5 transitions right-facing standing to crouching while aiming diagonally down.</summary>
    public const byte CrouchingTransitionAimDiagonalDownRightPose = 0xf5;

    /// <summary>Pose $F6 transitions left-facing standing to crouching while aiming diagonally down.</summary>
    public const byte CrouchingTransitionAimDiagonalDownLeftPose = 0xf6;

    /// <summary>Pose $F7 transitions right-facing crouching to standing while aiming up.</summary>
    public const byte StandingTransitionAimUpRightPose = 0xf7;

    /// <summary>Pose $F8 transitions left-facing crouching to standing while aiming up.</summary>
    public const byte StandingTransitionAimUpLeftPose = 0xf8;

    /// <summary>Pose $F9 transitions right-facing crouching to standing while aiming diagonally up.</summary>
    public const byte StandingTransitionAimDiagonalUpRightPose = 0xf9;

    /// <summary>Pose $FA transitions left-facing crouching to standing while aiming diagonally up.</summary>
    public const byte StandingTransitionAimDiagonalUpLeftPose = 0xfa;

    /// <summary>Pose $FB transitions right-facing crouching to standing while aiming diagonally down.</summary>
    public const byte StandingTransitionAimDiagonalDownRightPose = 0xfb;

    /// <summary>Pose $FC transitions left-facing crouching to standing while aiming diagonally down.</summary>
    public const byte StandingTransitionAimDiagonalDownLeftPose = 0xfc;

    /// <summary>Pose $27 is ordinary right-facing crouching.</summary>
    public const byte CrouchingRightPose = 0x27;

    /// <summary>Pose $28 is ordinary left-facing crouching.</summary>
    public const byte CrouchingLeftPose = 0x28;

    /// <summary>Pose $35 is the right-facing standing-to-crouch transition.</summary>
    public const byte CrouchingTransitionRightPose = 0x35;

    /// <summary>Pose $36 is the left-facing standing-to-crouch transition.</summary>
    public const byte CrouchingTransitionLeftPose = 0x36;

    /// <summary>Pose $3B is the right-facing crouch-to-standing transition.</summary>
    public const byte StandingTransitionRightPose = 0x3b;

    /// <summary>Pose $3C is the left-facing crouch-to-standing transition.</summary>
    public const byte StandingTransitionLeftPose = 0x3c;

    /// <summary>Pose $4B is the one-frame right neutral-jump transition.</summary>
    public const byte NeutralJumpTransitionRightPose = 0x4b;

    /// <summary>Pose $4C is the one-frame left neutral-jump transition.</summary>
    public const byte NeutralJumpTransitionLeftPose = 0x4c;

    /// <summary>Pose $4D is the ordinary right-facing neutral jump.</summary>
    public const byte NeutralJumpRightPose = 0x4d;

    /// <summary>Pose $4E is the ordinary left-facing neutral jump.</summary>
    public const byte NeutralJumpLeftPose = 0x4e;

    /// <summary>Pose $A4 lands facing right after a non-spinning jump or fall.</summary>
    public const byte NormalLandingRightPose = 0xa4;

    /// <summary>Pose $A5 lands facing left after a non-spinning jump or fall.</summary>
    public const byte NormalLandingLeftPose = 0xa5;

    /// <summary>Pose $A6 lands facing right after a spin or wall jump.</summary>
    public const byte SpinLandingRightPose = 0xa6;

    /// <summary>Pose $A7 lands facing left after a spin or wall jump.</summary>
    public const byte SpinLandingLeftPose = 0xa7;

    /// <summary>Current one-byte pose index, corresponding to WRAM <c>$0A1C</c>.</summary>
    public byte Pose { get; set; } = FacingRightNormalPose;

    /// <summary>Current animation-frame index, corresponding to WRAM <c>$0A96</c>.</summary>
    public ushort AnimationFrame { get; set; }

    /// <summary>
    /// Countdown at WRAM <c>$0A94</c>. Zero and negative-after-decrement both advance,
    /// matching the original routine's BEQ/BPL pair.
    /// </summary>
    public ushort AnimationFrameTimer { get; private set; }

    /// <summary>
    /// Delay added for speed/liquid physics at WRAM <c>$0A9A</c>. The translated standing
    /// dry-room slice keeps this equal to <see cref="XSpeedDivisor"/>.
    /// </summary>
    public ushort AnimationFrameBuffer { get; internal set; }

    /// <summary>
    /// Exact bank-$90 fixed-point horizontal-speed registers used by ordinary grounded
    /// right-running movement and exposed independently for debugger inspection.
    /// </summary>
    public SamusHorizontalSpeedState HorizontalSpeed { get; } = new();

    /// <summary>
    /// Live room-FX surface words and remembered liquid medium. Physics, animation, and
    /// pose initialization intentionally share this object because the cartridge shares
    /// WRAM `$195E/$1962/$197E/$0AD2` across all three phases.
    /// </summary>
    public SamusLiquidPhysicsState LiquidPhysics { get; } = new();

    /// <summary>
    /// Stored-shine palette countdown and the special shinespark movement-handler state.
    /// Its fields map to the reused WRAM words documented on
    /// <see cref="SamusShinesparkState"/> and remain individually debugger-visible.
    /// </summary>
    public SamusShinesparkState Shinespark { get; } = new();

    /// <summary>
    /// Crystal Flash initiation, ammo cadence, energy restoration, and installed handler
    /// state. It is deliberately separate from <see cref="Shinespark"/> even though the
    /// original aliases a few WRAM words between the mutually exclusive effects.
    /// </summary>
    public SamusCrystalFlashState CrystalFlash { get; } = new();

    /// <summary>
    /// X-ray admission, dedicated pose input/movement, beam-angle state, visor palette, and
    /// teardown. Revealed-block tilemaps and window HDMA remain presentation-owned outputs.
    /// </summary>
    public SamusXrayState Xray { get; } = new();

    /// <summary>
    /// Fatal-damage `$D7/$D8` ownership, death tiles/palettes, whiteout, and suit explosion.
    /// The outer music wait and post-explosion room fade remain explicit game-state seams.
    /// </summary>
    public SamusDeathSequenceState DeathSequence { get; } = new();

    /// <summary>
    /// Mother Brain/Baby Metroid drain poses, controller calls, and installed falling
    /// movement handler. Enemy AI remains a separate producer of these literal commands.
    /// </summary>
    public SamusDrainedState Drained { get; } = new();

    /// <summary>
    /// Bank-$90's grabbed-pose hack handler and the narrow bank-$A5 owner-position seam.
    /// Draygon's enemy AI owns its own flight path; this child owns every Samus-side word.
    /// </summary>
    public SamusDraygonGrabbedState DraygonGrabbed { get; } = new();

    /// <summary>
    /// Compatibility/debugger view of <c>samus_x_speed_divisor</c> at WRAM <c>$0A66</c>.
    /// The backing word belongs to <see cref="HorizontalSpeed"/>, just as the native word
    /// is both a movement-speed divisor and the no-FX animation delay buffer.
    /// </summary>
    public ushort XSpeedDivisor
    {
        get => HorizontalSpeed.SpeedDivisor;
        set => HorizontalSpeed.SpeedDivisor = value;
    }

    /// <summary>Current energy at WRAM `$09C2`, also used by animation command `$F6`.</summary>
    public ushort Health { get; set; } = 99;

    /// <summary>
    /// Fractional energy word consumed by `$90:E9CE` before the whole-energy subtraction.
    /// Ordinary HUD/debug output shows only <see cref="Health"/>, but lava/acid damage uses
    /// this word's borrow so sub-energy cannot be rounded independently on every frame.
    /// </summary>
    public ushort SubunitHealth { get; set; }

    /// <summary>Maximum normal energy at WRAM `$09C4`; defaults to the initial 99.</summary>
    public ushort MaxHealth { get; set; } = 99;

    /// <summary>Current reserve energy at WRAM `$09D6`.</summary>
    public ushort ReserveEnergy { get; set; }

    /// <summary>Maximum reserve energy at WRAM `$09D4`.</summary>
    public ushort MaxReserveEnergy { get; set; }

    /// <summary>Reserve-tank mode at WRAM `$09C0`: zero off, one auto, two manual.</summary>
    public ushort ReserveTankMode { get; set; }

    /// <summary>Current missiles at WRAM `$09C6`.</summary>
    public ushort Missiles { get; set; }

    /// <summary>Current super missiles at WRAM `$09CA`.</summary>
    public ushort SuperMissiles { get; set; }

    /// <summary>Current power bombs at WRAM `$09CE`.</summary>
    public ushort PowerBombs { get; set; }

    /// <summary>Selected HUD item at WRAM <c>$09D2</c>: 0 none, 1 missiles, 2 supers, 3 bombs.</summary>
    public ushort SelectedHudItem { get; set; }

    /// <summary>Native HUD auto-cancel index at WRAM <c>$0A04</c>.</summary>
    public ushort AutoCancelHudItemIndex { get; set; }

    /// <summary>
    /// Debugger-readable identity of the normal-versus-locked Samus state-handler pair.
    /// Mother Brain command five/<c>$18</c> locks input; command one unlocks it after the
    /// rainbow beam has narrowed. Movement type alone cannot represent this independent word.
    /// </summary>
    public bool InputLocked { get; set; }

    /// <summary>
    /// Equipped beam bitfield at WRAM `$09A6`. Drained-controller function three replaces
    /// this with `$1009`, the exact charge/wave/plasma plus hyper-beam configuration.
    /// </summary>
    public ushort EquippedBeams { get; set; }

    /// <summary>
    /// WRAM <c>SamusProjectile_FlareCounter</c>. Values at least $003C mean the charge
    /// beam is fully charged; spin and wall-jump movement use that producer-owned word to
    /// publish contact-damage index four without owning the projectile charge lifecycle.
    /// </summary>
    public ushort ProjectileFlareCounter { get; set; }

    /// <summary>
    /// WRAM <c>$0B5E</c>, <c>PoseTransitionShotDirection</c>. A pose initializer can
    /// publish one shot direction after the current frame's projectile handler has already
    /// run. The following frame's bank-$90 producer consumes the low byte, then the normal
    /// current-state epilogue clears the complete word whether or not allocation succeeded.
    /// </summary>
    /// <remarks>
    /// The high byte is meaningful provenance, not part of the direction: `$8000` marks a
    /// fresh-shot normal-jump transition and `$0100` marks a moonwalk turn. Keeping the raw
    /// word visible makes both native producers and the single consumer debugger-auditable.
    /// </remarks>
    public ushort PoseTransitionShotDirection { get; private set; }

    /// <summary>WRAM `$0A76`; `$8000` marks Mother Brain's hyper beam as enabled.</summary>
    public ushort HyperBeam { get; set; }

    /// <summary>
    /// Equipped-item bitfield corresponding to WRAM <c>$09A2</c>. Bit two ($0004) is Morph
    /// Ball, bit one ($0002) is Spring Ball, and bit twelve ($1000) is Bombs. Animation
    /// command $F9 and the Morph-Ball projectile handler read this word directly.
    /// </summary>
    public ushort EquippedItems { get; set; }

    /// <summary>
    /// Morph-ball bounce state at WRAM <c>$0B20</c>: zero is not bouncing, one is the first
    /// rebound, and two is the second rebound. Spring Ball also uses the high byte, but that
    /// separate movement family is intentionally not folded into the ordinary-ball state.
    /// </summary>
    public ushort MorphBallBounceState { get; set; }

    /// <summary>
    /// WRAM `$0A56`: low byte 1/2/3 is left/straight/right; bit `$0800` marks the bank-$91
    /// special prospective command as accepted and prevents the same explosion rearming it.
    /// </summary>
    public ushort BombJumpDirection { get; set; }

    /// <summary>True for `$90:E025`'s one-frame velocity-initialization handler.</summary>
    public bool BombJumpStarting { get; set; }

    /// <summary>True while `$90:E032` owns the rising bomb-jump arc.</summary>
    public bool BombJumpActive { get; set; }

    /// <summary>
    /// WRAM `$0A52`: zero means no knockback, one/two mean up-left/up-right, and four/five
    /// mean down-left/down-right. Value three is the retail routine's unused straight-up
    /// table entry and is intentionally rejected by the translated live path.
    /// </summary>
    public ushort KnockbackDirection { get; set; }

    /// <summary>WRAM `$0A54`: zero moves knockback left; one moves it right.</summary>
    public ushort KnockbackXDirection { get; set; }

    /// <summary>
    /// WRAM <c>$0A48</c>, the shared hurt-flash palette counter. Ordinary knockback starts
    /// it at one, while an active shinespark repeatedly publishes eight and its crash
    /// handler clears it. Bank <c>$91:D8A5</c> is the sole ordinary consumer: values one
    /// through six alternate hurt and suit palettes, value forty restores interrupted
    /// movement audio, and value sixty ends the lifetime.
    /// </summary>
    /// <remarks>
    /// This word belongs to Samus rather than the shinespark child state. Keeping it here
    /// preserves the cartridge's intentional sharing between unrelated damage, palette,
    /// and special-movement producers and makes the complete effect debugger-watchable.
    /// </remarks>
    public ushort HurtFlashCounter { get; set; }

    /// <summary>
    /// WRAM <c>ResumeChargingBeamSFXFlag</c>. Hurt-flash recovery sets one at counter forty
    /// when a non-spinning Samus is still holding a sufficiently charged beam; the native
    /// post-draw sound handler consumes and clears it later in the same gameplay pass.
    /// </summary>
    public ushort ResumeChargingBeamSoundFlag { get; internal set; }

    /// <summary>
    /// WRAM `$18A8`, the general Samus invincibility timer tested by enemy, projectile,
    /// ordinary spike, spike-air, and grapple-swing damage producers. Bank `$A0:9169`
    /// decrements it once near the end of every gameplay frame, after all those producers
    /// have had an opportunity to reject or publish damage.
    /// </summary>
    public ushort InvincibilityTimer { get; set; }

    /// <summary>
    /// WRAM `$18AA`, initialized to five by bank `$A0` damage collision and decremented once
    /// per enemy-processing pass. The translated special handler owns movement only while
    /// this counter remains nonzero.
    /// </summary>
    public ushort KnockbackTimer { get; set; }

    /// <summary>
    /// Ports the two Samus-owned words from <c>DecrementSamusHurtTimers</c> at
    /// <c>$A0:9169-$9178</c>. This deliberately lives outside every individual movement
    /// handler: grapple spikes can start the timers without installing knockback movement,
    /// and the native gameplay tail ages them even while time is frozen.
    /// </summary>
    public void DecrementHurtTimers()
    {
        if (InvincibilityTimer != 0)
            InvincibilityTimer--;
        if (KnockbackTimer != 0)
            KnockbackTimer--;
    }

    /// <summary>
    /// Bank-$9B's grapple-beam state. Keeping this as a named child object makes the original
    /// WRAM words individually watchable without polluting ordinary Samus kinematics.
    /// </summary>
    public SamusGrappleState Grapple { get; } = new();

    /// <summary>
    /// The independent opening cover, one-OBJ draw, and tile-$1F DMA used by aimed weapon
    /// poses. Its state is driven by HUD selection rather than by the body animation timer.
    /// </summary>
    public SamusArmCannonState ArmCannon { get; } = new();

    /// <summary>
    /// Packed five-call visor color cycle used only by backdrop-color-math rooms.
    /// </summary>
    public SamusVisorPaletteState VisorPalette { get; } = new();

    /// <summary>
    /// Native WRAM <c>EnemyIndexToShake</c>, written when an ordinary or grapple wall jump
    /// launches from a solid/frozen enemy instead of room terrain.
    /// </summary>
    /// <remarks>
    /// Enemy AI owns consumption of this word. Retaining the slot index now makes the
    /// movement result complete even before the general live-enemy update loop is translated.
    /// </remarks>
    public ushort EnemyIndexToShake { get; set; } = 0xffff;

    /// <summary>
    /// Host-visible equivalent of movement-handler pointer `$90:DF38`. It is separate from
    /// movement type `$0A` because the cartridge also uses that type for crystal-flash art.
    /// </summary>
    public bool KnockbackActive { get; set; }

    /// <summary>Bank-$91 address of the active pose's byte-oriented delay program.</summary>
    public int AnimationDelayListAddress { get; private set; }

    /// <summary>
    /// Most recent high-bit delay command, or null when the last advance selected a plain
    /// delay. This is a host-only debugger watch rather than original WRAM.
    /// </summary>
    public byte? LastAnimationDelayCommand { get; private set; }

    /// <summary>
    /// Operand published by animation command $F8 through the native “super-special
    /// prospective pose” seam. Null means the animation has not requested that transition.
    /// </summary>
    /// <remarks>
    /// This is host storage for WRAM $0A2A/$0A2C's command-three route. The grounded slice
    /// consumes it after animation, at the same pose-transition point used by the cartridge.
    /// Keeping it explicit makes the turn's final facing change visible to a debugger.
    /// </remarks>
    public byte? PendingTransitionalPose { get; private set; }

    /// <summary>Exact fixed-point position/radius words consumed by bank-$94 collision.</summary>
    public SamusKinematicsState Kinematics { get; } = new();

    /// <summary>
    /// WRAM <c>SamusSolidVerticalCollisionResult</c>. Most translated movement methods
    /// return collision results directly; movement type `$1A` has no displacement and its
    /// sole bank-$90 side effect is explicitly clearing this otherwise-stale word.
    /// </summary>
    public ushort SolidVerticalCollisionResult { get; set; }

    /// <summary>Debugger/rendering view of the whole-pixel world X position.</summary>
    public ushort XPosition
    {
        get => Kinematics.XPosition;
        set => Kinematics.XPosition = value;
    }

    /// <summary>Debugger/rendering view of the whole-pixel world Y position.</summary>
    public ushort YPosition
    {
        get => Kinematics.YPosition;
        set => Kinematics.YPosition = value;
    }

    /// <summary>
    /// Ports <c>Samus_SetRadius</c> at <c>$90:EC22</c>: every pose is five pixels wide and
    /// reads its vertical radius from pose-definition byte six.
    /// </summary>
    public void RefreshCollisionRadii(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int poseDefinition = AddWithinBank(PoseDefinitions, Pose * 8);
        Kinematics.XRadius = 5;
        Kinematics.YRadius = bus.ReadByte(AddWithinBank(poseDefinition, 6));
    }

    /// <summary>Reads pose-definition byte zero, the direction consumed by camera tracking.</summary>
    public byte ReadPoseXDirection(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return ReadPoseXDirection(bus, Pose);
    }

    /// <summary>
    /// Reads pose-definition byte zero for an arbitrary pose. Native transition
    /// initializers compare the old and prospective records before publishing the new
    /// pose, so making that distinction explicit avoids temporarily corrupting live state.
    /// </summary>
    public static byte ReadPoseXDirection(ISnesAddressSpace bus, byte pose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return bus.ReadByte(AddWithinBank(PoseDefinitions, pose * 8));
    }

    /// <summary>Reads pose-definition byte one, the movement-type dispatcher index.</summary>
    public byte ReadMovementType(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return ReadMovementType(bus, Pose);
    }

    /// <summary>Reads pose-definition byte one for a prospective pose without mutating Samus.</summary>
    public static byte ReadMovementType(ISnesAddressSpace bus, byte pose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return bus.ReadByte(AddWithinBank(PoseDefinitions, pose * 8 + 1));
    }

    /// <summary>
    /// Reads pose-definition byte two, the pose selected by <c>$91:82D9</c> when no
    /// controller bits are held and the transition table therefore is not consulted.
    /// </summary>
    public byte ReadNoInputFallbackPose(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return bus.ReadByte(AddWithinBank(PoseDefinitions, Pose * 8 + 2));
    }

    /// <summary>
    /// Reads pose-definition byte three, the ten-way arm-cannon direction consumed by
    /// the native normal-jump landing selector at <c>$91:E974</c>.
    /// </summary>
    public byte ReadShotDirection(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return ReadShotDirection(bus, Pose);
    }

    /// <summary>Reads pose-definition byte three for a current or prospective pose.</summary>
    public static byte ReadShotDirection(ISnesAddressSpace bus, byte pose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return bus.ReadByte(AddWithinBank(PoseDefinitions, pose * 8 + 3));
    }

    /// <summary>
    /// Reads signed pose-definition byte four. Grapple firing uses this graphics-origin
    /// correction before applying the direction-specific hand offset at <c>$9B:C51E</c>.
    /// Keeping the byte behind a named accessor prevents the grapple port from duplicating
    /// the bank-$91 pose-table address or silently treating a negative offset as unsigned.
    /// </summary>
    public sbyte ReadGraphicsYOffset(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return unchecked((sbyte)bus.ReadByte(AddWithinBank(PoseDefinitions, Pose * 8 + 4)));
    }

    /// <summary>True for the four movement-type-zero, right-facing standing poses.</summary>
    public static bool IsRightFacingStandingPose(byte pose) => pose is
        FacingRightNormalPose or
        StandingAimUpRightPose or
        StandingAimDiagonalUpRightPose or
        StandingAimDiagonalDownRightPose;

    /// <summary>True for the four movement-type-zero, left-facing standing poses.</summary>
    public static bool IsLeftFacingStandingPose(byte pose) => pose is
        FacingLeftNormalPose or
        StandingAimUpLeftPose or
        StandingAimDiagonalUpLeftPose or
        StandingAimDiagonalDownLeftPose;

    /// <summary>
    /// True for the two front-view records selected by <c>MakeSamusFaceForward</c> at
    /// `$91:E3F6`. Their pose-X direction is zero and must never be guessed as left/right.
    /// </summary>
    public static bool IsForwardFacingPose(byte pose) => pose is
        ForwardFacingPowerSuitPose or ForwardFacingSuitedPose;

    /// <summary>
    /// Applies the movement/animation subset of <c>MakeSamusFaceForward</c> at
    /// `$91:E3F6-$91:E4A5`.
    /// </summary>
    /// <remarks>
    /// The native routine also locks the global current/new-state handlers, kills grapple,
    /// clears beam-flare presentation words, and reloads the suit palette. Those owners do
    /// not live in this state object. This method deliberately covers only the state it can
    /// own exactly: equipment-selected pose, ROM collision radius/delay list, and all motion
    /// words cleared by the routine. Runtime/debug callers remain responsible for input lock,
    /// palette, grapple, and priming the first graphics DMA before their first visible NMI.
    /// </remarks>
    public void ApplyForwardFacingPoseSetup(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);

        // `$91:E3FD-$91:E415` gives Gravity bit `$0020` and Varia bit `$0001` equal
        // precedence: either selects the shared suited front-view pose `$9B`; neither
        // selects power-suit pose `$00`.
        Pose = (EquippedItems & 0x0021) != 0
            ? ForwardFacingSuitedPose
            : ForwardFacingPowerSuitPose;
        AnimationFrame = 0;
        RefreshCollisionRadii(bus);
        InitializeAnimation(bus, initialFrame: 0);

        // `$91:E438-$91:E44A` adjusts center Y upward three only if the initialized pose
        // did not produce radius 24. Retail `$00/$9B` both do, but retaining the branch
        // makes a corrupt/modified pose table observable rather than silently normalizing it.
        if (Kinematics.YRadius != 0x18)
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition - 3));

        HorizontalSpeed.ExtraRunSpeed = 0;
        HorizontalSpeed.ExtraRunSubspeed = 0;
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;
        HorizontalSpeed.AccelerationMode = 0;
        Kinematics.YSubspeed = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YDirection = 0;
        MorphBallBounceState = 0;
    }

    /// <summary>
    /// True for all five live movement-type-one, right-moving pose-table entries. `$0B`
    /// belongs here even though its name describes firing: bank `$90` dispatches its body
    /// through exactly the same running physics as `$09/$0D/$0F/$11`.
    /// </summary>
    public static bool IsRightFacingRunningPose(byte pose) => pose is
        MovingRightNormalPose or MovingRightGunExtendedPose or
        RunningAimUpRightPose or
        RunningAimDiagonalUpRightPose or
        RunningAimDiagonalDownRightPose;

    /// <summary>True for all five live movement-type-one, left-moving pose-table entries.</summary>
    public static bool IsLeftFacingRunningPose(byte pose) => pose is
        MovingLeftNormalPose or MovingLeftGunExtendedPose or
        RunningAimUpLeftPose or
        RunningAimDiagonalUpLeftPose or
        RunningAimDiagonalDownLeftPose;

    /// <summary>
    /// True for the six ordinary-play bodies whose names explicitly say “gun extended.”
    /// Firing landings `$E6/$E7` are kept separate because they are short transition art,
    /// not stable horizontal-fire bodies selected directly by an input table.
    /// </summary>
    public static bool IsGunExtendedPose(byte pose) => pose is
        MovingRightGunExtendedPose or MovingLeftGunExtendedPose or
        NormalJumpGunExtendedRightPose or NormalJumpGunExtendedLeftPose or
        FallingGunExtendedRightPose or FallingGunExtendedLeftPose;

    /// <summary>
    /// True for the three poses whose artwork faces left while the type-$10 body travels
    /// right. The naming is intentionally visual; <c>ReadPoseXDirection</c> returns eight.
    /// </summary>
    public static bool IsMoonwalkingFacingLeftPose(byte pose) => pose is
        MoonwalkFacingLeftPose or MoonwalkAimUpLeftPose or MoonwalkAimDownLeftPose;

    /// <summary>True for the three right-facing moonwalk poses that travel left.</summary>
    public static bool IsMoonwalkingFacingRightPose(byte pose) => pose is
        MoonwalkFacingRightPose or MoonwalkAimUpRightPose or MoonwalkAimDownRightPose;

    public static bool IsMoonwalkingPose(byte pose) =>
        IsMoonwalkingFacingLeftPose(pose) || IsMoonwalkingFacingRightPose(pose);

    /// <summary>True for all five left-facing movement-type-`$1A` Draygon poses.</summary>
    public static bool IsLeftFacingDraygonGrabbedPose(byte pose) => pose is
        DraygonGrabbedNeutralLeftPose or DraygonGrabbedAimUpLeftPose or
        DraygonGrabbedFiringLeftPose or DraygonGrabbedAimDownLeftPose or
        DraygonGrabbedMovingLeftPose;

    /// <summary>True for all five right-facing movement-type-`$1A` Draygon poses.</summary>
    public static bool IsRightFacingDraygonGrabbedPose(byte pose) => pose is
        DraygonGrabbedNeutralRightPose or DraygonGrabbedAimUpRightPose or
        DraygonGrabbedFiringRightPose or DraygonGrabbedAimDownRightPose or
        DraygonGrabbedMovingRightPose;

    /// <summary>True for the complete ten-pose grabbed-by-Draygon family.</summary>
    public static bool IsDraygonGrabbedPose(byte pose) =>
        IsLeftFacingDraygonGrabbedPose(pose) || IsRightFacingDraygonGrabbedPose(pose);

    /// <summary>
    /// Applies one ordinary `$91:AE18/$AE56` transition without inventing pose physics.
    /// Every admitted record stays within one facing, retains radius 21, and restarts the
    /// target's cartridge delay list at byte zero through the normal pose initializer.
    /// </summary>
    public void ApplyDraygonGrabbedPoseChange(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);

        bool leftFamily = IsLeftFacingDraygonGrabbedPose(Pose) &&
            IsLeftFacingDraygonGrabbedPose(targetPose);
        bool rightFamily = IsRightFacingDraygonGrabbedPose(Pose) &&
            IsRightFacingDraygonGrabbedPose(targetPose);
        if (!leftFamily && !rightFamily)
        {
            throw new NotSupportedException(
                $"Draygon-grabbed transition ${Pose:X2} -> ${targetPose:X2} crosses a native owner seam.");
        }

        ushort oldRadius = Kinematics.YRadius;
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        if (Kinematics.YRadius != oldRadius)
        {
            throw new InvalidDataException(
                $"Draygon pose ${targetPose:X2} unexpectedly changed radius {oldRadius} -> {Kinematics.YRadius}.");
        }

        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// True for `$BF-$C4`, the six grounded turn frames selected only when Jump begins a
    /// direction reversal from movement type `$10`. Odd/even descriptive names follow the
    /// literal pose records, while the predicates below group them by destination facing.
    /// </summary>
    public static bool IsMoonwalkTurnJumpPose(byte pose) => pose is
        MoonwalkTurnJumpLeftPose or MoonwalkTurnJumpRightPose or
        MoonwalkTurnJumpAimUpLeftPose or MoonwalkTurnJumpAimUpRightPose or
        MoonwalkTurnJumpAimDownLeftPose or MoonwalkTurnJumpAimDownRightPose;

    /// <summary>True for the `$BF/$C1/$C3` family whose transition tables lead to left.</summary>
    public static bool IsMoonwalkTurnJumpLeftPose(byte pose) => pose is
        MoonwalkTurnJumpLeftPose or MoonwalkTurnJumpAimUpLeftPose or
        MoonwalkTurnJumpAimDownLeftPose;

    /// <summary>True for the `$C0/$C2/$C4` family whose transition tables lead to right.</summary>
    public static bool IsMoonwalkTurnJumpRightPose(byte pose) => pose is
        MoonwalkTurnJumpRightPose or MoonwalkTurnJumpAimUpRightPose or
        MoonwalkTurnJumpAimDownRightPose;

    /// <summary>True for `$89/$CF/$D1`, the complete right-facing type-`$15` family.</summary>
    public static bool IsRightFacingRanIntoWallPose(byte pose) => pose is
        RanIntoWallRightPose or RanIntoWallAimUpRightPose or RanIntoWallAimDownRightPose;

    /// <summary>True for `$8A/$D0/$D2`, the complete left-facing type-`$15` family.</summary>
    public static bool IsLeftFacingRanIntoWallPose(byte pose) => pose is
        RanIntoWallLeftPose or RanIntoWallAimUpLeftPose or RanIntoWallAimDownLeftPose;

    public static bool IsRanIntoWallPose(byte pose) =>
        IsRightFacingRanIntoWallPose(pose) || IsLeftFacingRanIntoWallPose(pose);

    public static bool IsAimedRanIntoWallPose(byte pose) => pose is
        RanIntoWallAimUpRightPose or RanIntoWallAimUpLeftPose or
        RanIntoWallAimDownRightPose or RanIntoWallAimDownLeftPose;

    /// <summary>True when a supported standing, running, crouching, or landing pose carries aim metadata.</summary>
    public static bool IsGroundedAimPose(byte pose) => pose is
        StandingAimUpRightPose or StandingAimUpLeftPose or
        StandingAimDiagonalUpRightPose or StandingAimDiagonalUpLeftPose or
        StandingAimDiagonalDownRightPose or StandingAimDiagonalDownLeftPose or
        RunningAimUpRightPose or RunningAimUpLeftPose or
        RunningAimDiagonalUpRightPose or RunningAimDiagonalUpLeftPose or
        RunningAimDiagonalDownRightPose or RunningAimDiagonalDownLeftPose or
        CrouchingAimUpRightPose or CrouchingAimUpLeftPose or
        CrouchingAimDiagonalUpRightPose or CrouchingAimDiagonalUpLeftPose or
        CrouchingAimDiagonalDownRightPose or CrouchingAimDiagonalDownLeftPose or
        LandingAimUpRightPose or LandingAimUpLeftPose or
        LandingAimDiagonalUpRightPose or LandingAimDiagonalUpLeftPose or
        LandingAimDiagonalDownRightPose or LandingAimDiagonalDownLeftPose or
        RanIntoWallAimUpRightPose or RanIntoWallAimUpLeftPose or
        RanIntoWallAimDownRightPose or RanIntoWallAimDownLeftPose;

    /// <summary>True for right-facing aimed normal-jump landing poses `$E0/$E2/$E4`.</summary>
    public static bool IsRightFacingAimedLandingPose(byte pose) => pose is
        LandingAimUpRightPose or LandingAimDiagonalUpRightPose or
        LandingAimDiagonalDownRightPose;

    /// <summary>True for left-facing aimed normal-jump landing poses `$E1/$E3/$E5`.</summary>
    public static bool IsLeftFacingAimedLandingPose(byte pose) => pose is
        LandingAimUpLeftPose or LandingAimDiagonalUpLeftPose or
        LandingAimDiagonalDownLeftPose;

    /// <summary>
    /// True for the complete right-facing landing set that shares the ordinary standing
    /// transition table: normal `$A4`, spin `$A6`, aimed `$E0/$E2/$E4`, and firing `$E6`.
    /// Centralizing this prevents a newly translated landing from silently losing crouch,
    /// turn, run, or wall-probe exits in one of the several native pose-change seams.
    /// </summary>
    public static bool IsRightFacingLandingPose(byte pose) => pose is
        NormalLandingRightPose or SpinLandingRightPose or FiringLandingRightPose ||
        IsRightFacingAimedLandingPose(pose);

    /// <summary>True for the complete mirrored left-facing landing set.</summary>
    public static bool IsLeftFacingLandingPose(byte pose) => pose is
        NormalLandingLeftPose or SpinLandingLeftPose or FiringLandingLeftPose ||
        IsLeftFacingAimedLandingPose(pose);

    /// <summary>
    /// True for the complete right-facing movement-type-five crouch family. All four
    /// records use radius 16 and the shared transition table at <c>$91:A66C</c>.
    /// </summary>
    public static bool IsRightFacingCrouchingPose(byte pose) => pose is
        CrouchingRightPose or CrouchingAimUpRightPose or
        CrouchingAimDiagonalUpRightPose or CrouchingAimDiagonalDownRightPose;

    /// <summary>
    /// True for the complete left-facing movement-type-five crouch family. All four
    /// records use radius 16 and the mirrored table at <c>$91:A6BC</c>.
    /// </summary>
    public static bool IsLeftFacingCrouchingPose(byte pose) => pose is
        CrouchingLeftPose or CrouchingAimUpLeftPose or
        CrouchingAimDiagonalUpLeftPose or CrouchingAimDiagonalDownLeftPose;

    /// <summary>True for the six crouching poses carrying a real shot direction.</summary>
    public static bool IsAimedCrouchingPose(byte pose) => pose is
        CrouchingAimUpRightPose or CrouchingAimUpLeftPose or
        CrouchingAimDiagonalUpRightPose or CrouchingAimDiagonalUpLeftPose or
        CrouchingAimDiagonalDownRightPose or CrouchingAimDiagonalDownLeftPose;

    /// <summary>
    /// True for admitted grounded `$0E/$17` poses that began facing right and turn left.
    /// Their definition direction is already four (the destination facing), so callers
    /// must use this semantic grouping when preserving old rightward momentum.
    /// </summary>
    public static bool IsRightToLeftGroundTurnPose(byte pose) => pose is
        TurningRightToLeftPose or TurningRightToLeftAimUpPose or
        TurningRightToLeftAimDiagonalUpPose or TurningRightToLeftAimDiagonalDownPose or
        TurningRightToLeftCrouchingPose or TurningRightToLeftCrouchingAimUpPose or
        TurningRightToLeftCrouchingAimDiagonalUpPose or
        TurningRightToLeftCrouchingAimDiagonalDownPose;

    /// <summary>True for admitted grounded `$0E/$17` poses that began facing left and turn right.</summary>
    public static bool IsLeftToRightGroundTurnPose(byte pose) => pose is
        TurningLeftToRightPose or TurningLeftToRightAimUpPose or
        TurningLeftToRightAimDiagonalUpPose or TurningLeftToRightAimDiagonalDownPose or
        TurningLeftToRightCrouchingPose or TurningLeftToRightCrouchingAimUpPose or
        TurningLeftToRightCrouchingAimDiagonalUpPose or
        TurningLeftToRightCrouchingAimDiagonalDownPose;

    /// <summary>
    /// True for the six aimed crouched-turn records dispatched through movement type `$17`.
    /// `$43/$44` are intentionally absent because the retail pose definitions assign those
    /// otherwise similar-looking unaimed crouch turns to grounded movement type `$0E`.
    /// </summary>
    public static bool IsAimedCrouchingTurnPose(byte pose) => pose is
        TurningRightToLeftCrouchingAimUpPose or TurningLeftToRightCrouchingAimUpPose or
        TurningRightToLeftCrouchingAimDiagonalUpPose or TurningLeftToRightCrouchingAimDiagonalUpPose or
        TurningRightToLeftCrouchingAimDiagonalDownPose or TurningLeftToRightCrouchingAimDiagonalDownPose;

    /// <summary>True for every movement-type-$17 normal-jump turn record.</summary>
    public static bool IsJumpingTurnPose(byte pose) => pose is
        TurningRightToLeftJumpPose or TurningLeftToRightJumpPose or
        TurningRightToLeftJumpAimUpPose or TurningLeftToRightJumpAimUpPose or
        TurningRightToLeftJumpAimDownPose or TurningLeftToRightJumpAimDownPose or
        TurningRightToLeftJumpAimDiagonalUpPose or TurningLeftToRightJumpAimDiagonalUpPose;

    /// <summary>True for every movement-type-$18 falling-turn record.</summary>
    public static bool IsFallingTurnPose(byte pose) => pose is
        TurningRightToLeftFallingPose or TurningLeftToRightFallingPose or
        TurningRightToLeftFallingAimUpPose or TurningLeftToRightFallingAimUpPose or
        TurningRightToLeftFallingAimDownPose or TurningLeftToRightFallingAimDownPose or
        TurningRightToLeftFallingAimDiagonalUpPose or TurningLeftToRightFallingAimDiagonalUpPose;

    public static bool IsAerialTurnPose(byte pose) =>
        IsJumpingTurnPose(pose) || IsFallingTurnPose(pose);

    public static bool IsRightToLeftAerialTurnPose(byte pose) => pose is
        TurningRightToLeftJumpPose or TurningRightToLeftJumpAimUpPose or
        TurningRightToLeftJumpAimDownPose or TurningRightToLeftJumpAimDiagonalUpPose or
        TurningRightToLeftFallingPose or TurningRightToLeftFallingAimUpPose or
        TurningRightToLeftFallingAimDownPose or TurningRightToLeftFallingAimDiagonalUpPose;

    public static bool IsLeftToRightAerialTurnPose(byte pose) => pose is
        TurningLeftToRightJumpPose or TurningLeftToRightJumpAimUpPose or
        TurningLeftToRightJumpAimDownPose or TurningLeftToRightJumpAimDiagonalUpPose or
        TurningLeftToRightFallingPose or TurningLeftToRightFallingAimUpPose or
        TurningLeftToRightFallingAimDownPose or TurningLeftToRightFallingAimDiagonalUpPose;

    /// <summary>True for the two movement-type-$14 wall-jump launch records.</summary>
    public static bool IsWallJumpPose(byte pose) => pose is WallJumpRightPose or WallJumpLeftPose;

    /// <summary>
    /// True for all six stable movement-type-three spin bodies. The transition tables use
    /// `$19/$1A` as generic outputs; `$91:F624` can then substitute Space Jump or Screw
    /// Attack without changing the movement dispatcher.
    /// </summary>
    public static bool IsSpinJumpPose(byte pose) => pose is
        SpinJumpRightPose or SpinJumpLeftPose or
        SpaceJumpRightPose or SpaceJumpLeftPose or
        ScrewAttackRightPose or ScrewAttackLeftPose;

    /// <summary>True only for the two Screw Attack animation records `$81/$82`.</summary>
    public static bool IsScrewAttackPose(byte pose) => pose is
        ScrewAttackRightPose or ScrewAttackLeftPose;

    /// <summary>True only for the two Space Jump animation records `$1B/$1C`.</summary>
    public static bool IsSpaceJumpPose(byte pose) => pose is
        SpaceJumpRightPose or SpaceJumpLeftPose;

    /// <summary>
    /// True only for the admitted movement-type-$0F crouch/stand animation records.
    /// Morph/unmorph records share that dispatcher but are intentionally not hidden here.
    /// </summary>
    public static bool IsCrouchStandTransitionPose(byte pose) => pose is
        CrouchingTransitionRightPose or CrouchingTransitionLeftPose or
        StandingTransitionRightPose or StandingTransitionLeftPose or
        CrouchingTransitionAimUpRightPose or CrouchingTransitionAimUpLeftPose or
        CrouchingTransitionAimDiagonalUpRightPose or CrouchingTransitionAimDiagonalUpLeftPose or
        CrouchingTransitionAimDiagonalDownRightPose or CrouchingTransitionAimDiagonalDownLeftPose or
        StandingTransitionAimUpRightPose or StandingTransitionAimUpLeftPose or
        StandingTransitionAimDiagonalUpRightPose or StandingTransitionAimDiagonalUpLeftPose or
        StandingTransitionAimDiagonalDownRightPose or StandingTransitionAimDiagonalDownLeftPose;

    /// <summary>True for the four ordinary, non-Spring-Ball grounded morph poses.</summary>
    public static bool IsGroundedMorphBallPose(byte pose) => pose is
        MorphBallGroundRightPose or MorphBallGroundLeftPose or
        MorphBallMovingRightPose or MorphBallMovingLeftPose;

    /// <summary>True for the two ordinary, non-Spring-Ball airborne morph poses.</summary>
    public static bool IsAirborneMorphBallPose(byte pose) => pose is
        MorphBallFallingRightPose or MorphBallFallingLeftPose;

    /// <summary>True for movement type $11's four grounded Spring Ball poses.</summary>
    public static bool IsGroundedSpringBallPose(byte pose) => pose is
        SpringBallGroundRightPose or SpringBallGroundLeftPose or
        SpringBallMovingRightPose or SpringBallMovingLeftPose;

    /// <summary>True for Spring Ball jump/fall poses across movement types $12/$13.</summary>
    public static bool IsAirborneSpringBallPose(byte pose) => pose is
        SpringBallFallingRightPose or SpringBallFallingLeftPose or
        SpringBallJumpRightPose or SpringBallJumpLeftPose;

    /// <summary>True for every stable ordinary or Spring Ball pose using radius seven.</summary>
    public static bool IsStableBallPose(byte pose) =>
        IsGroundedMorphBallPose(pose) || IsAirborneMorphBallPose(pose) ||
        IsGroundedSpringBallPose(pose) || IsAirborneSpringBallPose(pose);

    /// <summary>True for the four entry/exit poses handled by movement type $0F.</summary>
    public static bool IsMorphTransitionPose(byte pose) => pose is
        MorphingTransitionRightPose or MorphingTransitionLeftPose or
        UnmorphingTransitionRightPose or UnmorphingTransitionLeftPose;

    /// <summary>
    /// Convenience debugger/test entry that performs both native phases: publishing the
    /// bank-$A0 timer-eight overlap direction, then consuming it through $90:DF99 and
    /// special command three $91:EE80. Live runtime code calls those phases on separate
    /// frames through <see cref="PublishBombJumpDirection"/> and
    /// <see cref="TrySetupPublishedMorphedBombJump"/>.
    /// </summary>
    public void RequestMorphedBombJump(byte direction)
    {
        PublishBombJumpDirection(direction);
        TrySetupPublishedMorphedBombJump();
    }

    /// <summary>
    /// Stores only bank-$A0's low-byte bomb direction. The gameplay loop does this after
    /// frame-handler alpha; setup consequently cannot consume it until the next frame.
    /// </summary>
    public void PublishBombJumpDirection(byte direction)
    {
        if (direction is < 1 or > 3)
        {
            throw new ArgumentOutOfRangeException(
                nameof(direction),
                direction,
                "Bomb-jump direction must be left, straight, or right.");
        }

        // $A0:984B writes the complete word, not just its low byte. A timer-eight overlap
        // therefore publishes exactly $0001/$0002/$0003.
        BombJumpDirection = direction;
    }

    /// <summary>
    /// Consumes a direction published by the previous frame's projectile collision using
    /// the morphed branch at $90:E010 and command-three branch at $91:EE80.
    /// </summary>
    /// <returns>True when a pending low-byte direction installed the start handler.</returns>
    public bool TrySetupPublishedMorphedBombJump()
    {
        if (BombJumpDirection == 0 || (BombJumpDirection & 0xff00) != 0)
            return false;

        if (!IsStableBallPose(Pose))
        {
            // Standing/running/falling bomb jumps deliberately select different poses at
            // $90:DFAD-$90:E00D. Silently preserving a non-ball pose would invent behavior.
            throw new NotSupportedException(
                $"Bomb-jump setup for non-ball pose ${Pose:X2} is not translated yet.");
        }

        // Morphed movement types `$04/$08/$11/$12/$13` preserve the current pose in
        // SpecialProspectivePose. Command three ORs `$0800` and installs start handler.
        BombJumpDirection |= 0x0800;
        BombJumpStarting = true;
        BombJumpActive = false;
        return true;
    }

    /// <summary>True for the admitted right-facing movement-type-two normal-jump poses.</summary>
    public static bool IsRightFacingNormalJumpPose(byte pose) => pose is
        NeutralJumpTransitionRightPose or NeutralJumpRightPose or
        NormalJumpGunExtendedRightPose or
        NormalJumpForwardRightPose or NormalJumpAimUpRightPose or
        NormalJumpTransitionAimUpRightPose or
        NormalJumpTransitionAimDiagonalUpRightPose or
        NormalJumpTransitionAimDiagonalDownRightPose or
        NormalJumpAimDiagonalUpRightPose or NormalJumpAimDiagonalDownRightPose or
        NormalJumpAimDownRightPose;

    /// <summary>True for the admitted left-facing movement-type-two normal-jump poses.</summary>
    public static bool IsLeftFacingNormalJumpPose(byte pose) => pose is
        NeutralJumpTransitionLeftPose or NeutralJumpLeftPose or
        NormalJumpGunExtendedLeftPose or
        NormalJumpForwardLeftPose or NormalJumpAimUpLeftPose or
        NormalJumpTransitionAimUpLeftPose or
        NormalJumpTransitionAimDiagonalUpLeftPose or
        NormalJumpTransitionAimDiagonalDownLeftPose or
        NormalJumpAimDiagonalUpLeftPose or NormalJumpAimDiagonalDownLeftPose or
        NormalJumpAimDownLeftPose;

    /// <summary>True for admitted right-facing movement-type-six falling poses.</summary>
    public static bool IsRightFacingFallingPose(byte pose) => pose is
        FallingRightPose or FallingGunExtendedRightPose or FallingAimUpRightPose or
        FallingAimDiagonalUpRightPose or FallingAimDiagonalDownRightPose or
        FallingAimDownRightPose;

    /// <summary>True for admitted left-facing movement-type-six falling poses.</summary>
    public static bool IsLeftFacingFallingPose(byte pose) => pose is
        FallingLeftPose or FallingGunExtendedLeftPose or FallingAimUpLeftPose or
        FallingAimDiagonalUpLeftPose or FallingAimDiagonalDownLeftPose or
        FallingAimDownLeftPose;

    /// <summary>True for the aimed normal-jump/falling poses, including compact Down aim.</summary>
    public static bool IsAimedAerialPose(byte pose) => pose is
        NormalJumpAimUpRightPose or NormalJumpAimUpLeftPose or
        NormalJumpTransitionAimUpRightPose or NormalJumpTransitionAimUpLeftPose or
        NormalJumpTransitionAimDiagonalUpRightPose or NormalJumpTransitionAimDiagonalUpLeftPose or
        NormalJumpTransitionAimDiagonalDownRightPose or NormalJumpTransitionAimDiagonalDownLeftPose or
        NormalJumpAimDiagonalUpRightPose or NormalJumpAimDiagonalUpLeftPose or
        NormalJumpAimDiagonalDownRightPose or NormalJumpAimDiagonalDownLeftPose or
        FallingAimUpRightPose or FallingAimUpLeftPose or
        FallingAimDiagonalUpRightPose or FallingAimDiagonalUpLeftPose or
        FallingAimDiagonalDownRightPose or FallingAimDiagonalDownLeftPose or
        NormalJumpAimDownRightPose or NormalJumpAimDownLeftPose or
        FallingAimDownRightPose or FallingAimDownLeftPose;

    /// <summary>True only for the four radius-ten straight-down aerial bodies.</summary>
    public static bool IsCompactAerialPose(byte pose) => pose is
        NormalJumpAimDownRightPose or NormalJumpAimDownLeftPose or
        FallingAimDownRightPose or FallingAimDownLeftPose;

    /// <summary>
    /// Applies a same-facing input/fallback transition within normal-jump type two or
    /// falling type six without replacing the live 16.16 velocity words. The horizontal
    /// gun-extended records `$13/$14/$67/$68` use this same native initialization seam;
    /// “aim” survives in the historical method name only because that was the first slice.
    /// </summary>
    public void ApplyAerialAimTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (IsCompactAerialPose(Pose) || IsCompactAerialPose(targetPose))
        {
            throw new NotSupportedException(
                "Radius-ten aerial transitions require TryApplyCompactAerialTransition and active room collision data.");
        }
        bool rightJump = IsRightFacingNormalJumpPose(Pose) &&
            IsRightFacingNormalJumpPose(targetPose);
        bool leftJump = IsLeftFacingNormalJumpPose(Pose) &&
            IsLeftFacingNormalJumpPose(targetPose);
        bool rightFall = IsRightFacingFallingPose(Pose) &&
            IsRightFacingFallingPose(targetPose);
        bool leftFall = IsLeftFacingFallingPose(Pose) &&
            IsLeftFacingFallingPose(targetPose);
        if (!rightJump && !leftJump && !rightFall && !leftFall)
        {
            throw new NotSupportedException(
                $"Aerial aim transition ${Pose:X2} -> ${targetPose:X2} crosses an untranslated family.");
        }
        if (!IsAimedAerialPose(Pose) && !IsAimedAerialPose(targetPose) &&
            !IsGunExtendedPose(Pose) && !IsGunExtendedPose(targetPose) &&
            targetPose is not (NormalJumpForwardRightPose or NormalJumpForwardLeftPose))
        {
            throw new InvalidOperationException(
                "Aerial arm transition requires aimed, gun-extended, or forward-jump metadata.");
        }

        ushort oldRadius = Kinematics.YRadius;
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        if (Kinematics.YRadius != oldRadius)
        {
            // The admitted family deliberately excludes compact straight-down `$17/$18`
            // and `$2D/$2E`; reaching a different radius proves a caller crossed that seam.
            throw new NotSupportedException(
                $"Aerial pose ${targetPose:X2} changes radius {oldRadius} -> {Kinematics.YRadius}; pose-change collision is not translated for it.");
        }
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Applies same-facing transitions where either endpoint is compact straight-down
    /// `$17/$18/$2D/$2E`, preserving live velocity while reproducing pose-change collision.
    /// </summary>
    /// <remarks>
    /// Entering radius ten from radius nineteen requires no collision work at `$91:FDAE`.
    /// Leaving it probes all nine newly occupied pixels above and below. If both initial
    /// directions are blocked, `$91:FFA7` selects ordinary crouch; if a compensating shift's
    /// second probe is blocked, `$91:FE82` retains the source. Neither case installs the target.
    /// </remarks>
    public bool TryApplyCompactAerialTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool rightJump = IsRightFacingNormalJumpPose(Pose) &&
            IsRightFacingNormalJumpPose(targetPose);
        bool leftJump = IsLeftFacingNormalJumpPose(Pose) &&
            IsLeftFacingNormalJumpPose(targetPose);
        bool rightFall = IsRightFacingFallingPose(Pose) &&
            IsRightFacingFallingPose(targetPose);
        bool leftFall = IsLeftFacingFallingPose(Pose) &&
            IsLeftFacingFallingPose(targetPose);
        if ((!rightJump && !leftJump && !rightFall && !leftFall) ||
            (!IsCompactAerialPose(Pose) && !IsCompactAerialPose(targetPose)))
        {
            throw new NotSupportedException(
                $"Compact aerial transition ${Pose:X2} -> ${targetPose:X2} is not a same-family ROM route.");
        }

        byte sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
                bus,
                level,
                targetPose,
                nmiFrameCounter,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies a same-facing arm-body transition within the movement-type-zero standing
    /// family and movement-type-one running family. Besides aimed bodies, this includes the
    /// real `$0B/$0C` horizontal-fire records.
    /// </summary>
    /// <remarks>
    /// All admitted records have radius 21. If both endpoints are running, `$91:F50C` writes
    /// `$8000` to the new-frame word and `$91:FB5C` preserves the current frame and timer;
    /// this matters when firing midway through the ten-frame run cycle. A transition between
    /// movement types initializes frame zero normally. Movement type changes take effect on
    /// the following frame because pose transitions occur after movement.
    /// </remarks>
    public void ApplyGroundedAimTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool sourceRight = IsRightFacingStandingPose(Pose) || IsRightFacingRunningPose(Pose) ||
            IsRightFacingRanIntoWallPose(Pose) ||
            IsRightFacingLandingPose(Pose);
        bool targetRight = IsRightFacingStandingPose(targetPose) || IsRightFacingRunningPose(targetPose) ||
            IsRightFacingRanIntoWallPose(targetPose);
        bool sourceLeft = IsLeftFacingStandingPose(Pose) || IsLeftFacingRunningPose(Pose) ||
            IsLeftFacingRanIntoWallPose(Pose) ||
            IsLeftFacingLandingPose(Pose);
        bool targetLeft = IsLeftFacingStandingPose(targetPose) || IsLeftFacingRunningPose(targetPose) ||
            IsLeftFacingRanIntoWallPose(targetPose);
        bool sameRightFamily = sourceRight && targetRight;
        bool sameLeftFamily = sourceLeft && targetLeft;
        bool sameRightCrouch = IsRightFacingCrouchingPose(Pose) &&
            IsRightFacingCrouchingPose(targetPose);
        bool sameLeftCrouch = IsLeftFacingCrouchingPose(Pose) &&
            IsLeftFacingCrouchingPose(targetPose);
        if (!sameRightFamily && !sameLeftFamily && !sameRightCrouch && !sameLeftCrouch)
        {
            throw new NotSupportedException(
                $"Grounded aim transition ${Pose:X2} -> ${targetPose:X2} crosses an untranslated pose family.");
        }
        if (!IsGroundedAimPose(Pose) && !IsGroundedAimPose(targetPose) &&
            !IsGunExtendedPose(Pose) && !IsGunExtendedPose(targetPose))
        {
            throw new InvalidOperationException(
                "Grounded arm transition requires an aimed or gun-extended source/target pose.");
        }

        byte sourcePose = Pose;
        bool preservesRunningAnimation =
            (IsRightFacingRunningPose(sourcePose) && IsRightFacingRunningPose(targetPose)) ||
            (IsLeftFacingRunningPose(sourcePose) && IsLeftFacingRunningPose(targetPose));
        if (!preservesRunningAnimation)
        {
            // This is `$91:F404/$91:FB08`'s ordinary target installation. Replacing Pose
            // directly would omit radius refresh, delay-list selection, and command cleanup.
            ApplySimpleGroundedPoseChange(bus, sourcePose, targetPose, "Grounded arm");
            return;
        }

        // `$91:F50C-$F51A` sees previous movement type one and publishes `$8000`. The BMI
        // at `$91:FB5C` exits before touching frame, timer, or delay-buffer state. Every live
        // running arm pose points to the same `$91:B20A` list, so the cached list address is
        // already exact and must not be rebound or reset here.
        ushort oldRadius = Kinematics.YRadius;
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        if (Kinematics.YRadius != oldRadius)
        {
            throw new InvalidDataException(
                $"Running arm pose ${targetPose:X2} unexpectedly changed radius {oldRadius} -> {Kinematics.YRadius}.");
        }
    }

    /// <summary>
    /// Implements `$91:EB56-$91:EB87`'s ten-entry shot-direction selector. The input pose
    /// may be the current running pose after a killed-X-speed collision or the prospective
    /// running pose tested by the one-pixel arm-pump probe.
    /// </summary>
    public static byte SelectRanIntoWallPose(ISnesAddressSpace bus, byte sourcePose)
    {
        byte shotDirection = ReadShotDirection(bus, sourcePose);
        return shotDirection switch
        {
            0 => StandingAimUpRightPose,
            1 => RanIntoWallAimUpRightPose,
            2 or 4 => RanIntoWallRightPose,
            3 => RanIntoWallAimDownRightPose,
            5 or 7 => RanIntoWallLeftPose,
            6 => RanIntoWallAimDownLeftPose,
            8 => RanIntoWallAimUpLeftPose,
            9 => StandingAimUpLeftPose,
            _ => throw new InvalidDataException(
                $"Pose ${sourcePose:X2} has invalid shot-direction byte ${shotDirection:X2}."),
        };
    }

    /// <summary>
    /// Ports the block-backed portion of <c>CheckIfProspectivePoseRunsIntoAWall</c> at
    /// <c>$91:EADE</c>. Solid-enemy collision is a separate earlier probe and remains an
    /// explicit boundary until the actor system exists.
    /// </summary>
    /// <remarks>
    /// A current type-one collision maps the current pose immediately. Otherwise, only a
    /// prospective type-one pose is interesting: native moves Samus one whole pixel in the
    /// CURRENT pose direction, retains that move when clear, and maps the prospective pose's
    /// shot direction when blocked. The retained move is the retail arm-pump bug.
    /// </remarks>
    public byte? CheckProspectiveRunningPoseForWall(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte? prospectivePose,
        bool currentXSpeedKilledByBlock,
        out BlockMoveResult? onePixelProbe)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        onePixelProbe = null;

        if (currentXSpeedKilledByBlock && ReadMovementType(bus) == 1)
            return SelectRanIntoWallPose(bus, Pose);

        if (prospectivePose is not { } target || ReadMovementType(bus, target) != 1)
            return null;

        int onePixelForward = ReadPoseXDirection(bus) == 4
            ? -0x00010000
            : 0x00010000;
        onePixelProbe = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            Kinematics,
            onePixelForward);
        return onePixelProbe.Value.Collided
            ? SelectRanIntoWallPose(bus, target)
            : null;
    }

    /// <summary>
    /// Installs the pose selected when `$91:EADE` finds a block wall. This deliberately
    /// accepts standing/running/landing sources as well as an existing wall-stop source:
    /// `$91:EADE` can test a prospective run before that run is installed, and changing aim
    /// while still pressing into the wall proposes another running pose for the same probe.
    /// </summary>
    public void ApplyRanIntoWallPoseChange(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool rightSource = IsRightFacingStandingPose(Pose) || IsRightFacingRunningPose(Pose) ||
            IsRightFacingRanIntoWallPose(Pose) || IsRightFacingLandingPose(Pose);
        bool leftSource = IsLeftFacingStandingPose(Pose) || IsLeftFacingRunningPose(Pose) ||
            IsLeftFacingRanIntoWallPose(Pose) || IsLeftFacingLandingPose(Pose);
        bool rightRoute = rightSource &&
            (IsRightFacingRanIntoWallPose(targetPose) || targetPose == StandingAimUpRightPose);
        bool leftRoute = leftSource &&
            (IsLeftFacingRanIntoWallPose(targetPose) || targetPose == StandingAimUpLeftPose);
        if (!rightRoute && !leftRoute)
        {
            throw new NotSupportedException(
                $"Ran-into-wall pose change ${Pose:X2} -> ${targetPose:X2} is not a block-backed retail route.");
        }

        ApplySimpleGroundedPoseChange(bus, Pose, targetPose, "Ran into wall");
    }

    /// <summary>Applies `$89/$8A/$CF-$D2`'s same-facing exit to a type-one running pose.</summary>
    public void ApplyRanIntoWallToRunning(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool rightRoute = IsRightFacingRanIntoWallPose(Pose) &&
            IsRightFacingRunningPose(targetPose);
        bool leftRoute = IsLeftFacingRanIntoWallPose(Pose) &&
            IsLeftFacingRunningPose(targetPose);
        if (!rightRoute && !leftRoute)
        {
            throw new NotSupportedException(
                $"Ran-into-wall running exit ${Pose:X2} -> ${targetPose:X2} is not a retail route.");
        }

        ApplySimpleGroundedPoseChange(bus, Pose, targetPose, "Ran into wall to running");
    }

    /// <summary>
    /// Applies the movement-type-$10 option gate and stable-pose changes surrounding
    /// <c>InitializeSamusPose_Moonwalking</c> at <c>$91:F88C</c>.
    /// </summary>
    /// <remarks>
    /// Standing transition tables always publish a moonwalk candidate when Shoot and the
    /// backward direction are held. The native settings word decides what that candidate
    /// means: enabled retains `$49/$4A/$75-$78`; disabled substitutes ordinary `$25/$26`
    /// turn art. Once active, aim changes and the direct return to forward running are
    /// ordinary radius-21 pose installations with no invented velocity adjustment.
    /// </remarks>
    public void ApplyMoonwalkPoseChange(
        ISnesAddressSpace bus,
        byte targetPose,
        bool moonwalkEnabled)
    {
        ArgumentNullException.ThrowIfNull(bus);

        bool sourceStandingRight = IsRightFacingStandingPose(Pose) ||
            IsRightFacingRanIntoWallPose(Pose);
        bool sourceStandingLeft = IsLeftFacingStandingPose(Pose) ||
            IsLeftFacingRanIntoWallPose(Pose);
        bool targetVisualRight = IsMoonwalkingFacingRightPose(targetPose);
        bool targetVisualLeft = IsMoonwalkingFacingLeftPose(targetPose);
        bool entering =
            (sourceStandingRight && targetVisualRight) ||
            (sourceStandingLeft && targetVisualLeft);

        if (entering && !moonwalkEnabled)
        {
            // `$91:F893-$F8A9` tests the candidate pose-X byte. `$4A/$76/$78` store four
            // and become right-to-left `$25`; `$49/$75/$77` store eight and become `$26`.
            ApplyGroundedTurn(
                bus,
                targetVisualRight ? TurningRightToLeftPose : TurningLeftToRightPose);
            return;
        }

        bool sameVisualFamily =
            (IsMoonwalkingFacingRightPose(Pose) && targetVisualRight) ||
            (IsMoonwalkingFacingLeftPose(Pose) && targetVisualLeft);
        bool exitsToForwardRun =
            (IsMoonwalkingFacingRightPose(Pose) && targetPose == MovingRightNormalPose) ||
            (IsMoonwalkingFacingLeftPose(Pose) && targetPose == MovingLeftNormalPose);
        bool exitsToStandingFallback =
            (IsMoonwalkingFacingRightPose(Pose) && IsRightFacingStandingPose(targetPose)) ||
            (IsMoonwalkingFacingLeftPose(Pose) && IsLeftFacingStandingPose(targetPose));
        if ((!entering || !moonwalkEnabled) && !sameVisualFamily &&
            !exitsToForwardRun && !exitsToStandingFallback)
        {
            throw new NotSupportedException(
                $"Moonwalk pose change ${Pose:X2} -> ${targetPose:X2} is not a stable retail route.");
        }

        ApplySimpleGroundedPoseChange(bus, Pose, targetPose, "Moonwalk");
    }

    /// <summary>
    /// Applies `$91:F8D3-$91:F950` when a stable moonwalk pose reverses while Jump is held.
    /// The six requested `$BF-$C4` records are not airborne yet: they retain the grounded
    /// turn movement handler while their three visible frames play.
    /// </summary>
    public void ApplyMoonwalkTurnJump(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);

        byte expectedTarget = ReadShotDirection(bus) switch
        {
            1 => MoonwalkTurnJumpAimUpLeftPose,
            2 => MoonwalkTurnJumpLeftPose,
            3 => MoonwalkTurnJumpAimDownLeftPose,
            6 => MoonwalkTurnJumpAimDownRightPose,
            7 => MoonwalkTurnJumpRightPose,
            8 => MoonwalkTurnJumpAimUpRightPose,
            _ => throw new NotSupportedException(
                $"Moonwalk shot direction ${ReadShotDirection(bus):X2} has no stable turn/jump route."),
        };
        if (!IsMoonwalkingPose(Pose) || targetPose != expectedTarget)
        {
            throw new NotSupportedException(
                $"Moonwalk turn/jump ${Pose:X2} -> ${targetPose:X2} is not a retail route.");
        }

        // `$91:F8F3-$F903` preserves the source moonwalk shot direction with bit eight set.
        // That word affects arm-cannon transition drawing, which is not independently
        // surfaced yet; the destination pose already contains the exact matching art.
        FoldExtraRunSpeedIntoBaseAndStartTurn();
        ApplySimpleGroundedPoseChange(bus, Pose, targetPose, "Moonwalk turn/jump");
    }

    /// <summary>
    /// Applies the verified ordinary transition from standing-right pose $01 to running-
    /// right pose $09 at the end-of-frame bank-$91 transition seam.
    /// </summary>
    /// <remarks>
    /// This is intentionally not a general pose setter. For this transition, momentum
    /// routine index zero is a no-op, both poses have radius 21, and pose $09 uses the
    /// already-supported default render/tile paths. Other transitions can change momentum,
    /// align radii, invoke collision, or run specialized handlers and remain rejected.
    /// </remarks>
    public void ApplyStandingRightToRunningRight(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (Pose != FacingRightNormalPose)
        {
            throw new InvalidOperationException(
                $"Standing-right to running-right transition requires pose $01, not ${Pose:X2}.");
        }

        Pose = MovingRightNormalPose;
        RefreshCollisionRadii(bus);

        // $91:F404 -> $91:FB08 resets animation frame zero and loads pose $09's first
        // delay byte after SamusFunc_F433 refreshes pose direction/movement metadata.
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Applies the verified no-button fallback from running-right pose $09 to standing-right
    /// pose $01 after ordinary running momentum has decelerated to zero.
    /// </summary>
    public void ApplyRunningRightToStandingRight(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (Pose != MovingRightNormalPose)
        {
            throw new InvalidOperationException(
                $"Running-right to standing-right transition requires pose $09, not ${Pose:X2}.");
        }

        Pose = FacingRightNormalPose;
        RefreshCollisionRadii(bus);

        // Pose $09's new-pose-unless-buttons byte is $01. Once Samus_Pose_Func2 selects
        // momentum index two, `$91:ECD0` cancels `$0B3C/$0B3E` but deliberately leaves the
        // numeric extra pair intact. The following standing movement pass consumes that
        // final no-base-speed displacement before clearing every X-motion word.
        HorizontalSpeed.CancelRunningMomentum(ReadPoseXDirection(bus));
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>Applies the ordinary grounded $02 -> $0A start-moving-left transition.</summary>
    public void ApplyStandingLeftToRunningLeft(ISnesAddressSpace bus) =>
        ApplySimpleGroundedPoseChange(
            bus,
            FacingLeftNormalPose,
            MovingLeftNormalPose,
            "Standing-left to running-left");

    /// <summary>Applies the no-button grounded $0A -> $02 fallback.</summary>
    public void ApplyRunningLeftToStandingLeft(ISnesAddressSpace bus)
    {
        ApplySimpleGroundedPoseChange(
            bus,
            MovingLeftNormalPose,
            FacingLeftNormalPose,
            "Running-left to standing-left");

        // This is the mirrored `$91:ECD0` momentum-index-two route used by `$09 -> $01`.
        // ExtraRunSpeed/Subspeed remain available to standing's ordered movement/clear pass.
        HorizontalSpeed.CancelRunningMomentum(ReadPoseXDirection(bus));
    }

    /// <summary>
    /// Applies the shared standing/landing transition-table route from any right-facing
    /// landing (including firing `$E6`) to running right `$09`, or from the mirrored left
    /// family (including `$E7`) to `$0A`. Landing movement has already cleared momentum in
    /// this frame; the new running pose begins accelerating on the next frame.
    /// </summary>
    public void ApplyLandingToRunning(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool right = IsRightFacingLandingPose(Pose) &&
            targetPose == MovingRightNormalPose;
        bool left = IsLeftFacingLandingPose(Pose) &&
            targetPose == MovingLeftNormalPose;
        if (!right && !left)
        {
            throw new InvalidOperationException(
                $"Landing-to-run transition ${Pose:X2} -> ${targetPose:X2} is not verified.");
        }

        ApplySimpleGroundedPoseChange(bus, Pose, targetPose, "Landing-to-run");
    }

    /// <summary>
    /// Applies bank-$91's grounded turn initialization at <c>$91:F8D3</c>. Input tables
    /// first publish generic `$25/$26`; the initializer indexes the previous pose's shot
    /// direction through `$91:F9C2` and may replace it with `$8B-$8E/$9C/$9D`.
    /// </summary>
    public void ApplyGroundedTurn(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);

        // Crouching transition tables publish `$43/$44` directly, whereas every admitted
        // standing/running/landing table publishes `$25/$26`. The initializer still uses
        // previous movement type five to choose the full crouched aim-preserving table.
        byte sourceMovementType = ReadMovementType(bus);
        bool wasCrouching = sourceMovementType == 5;
        bool wasMoonwalking = sourceMovementType == 0x10;

        // `$91:F8F3-$F903` reads the PREVIOUS moonwalk pose, not the selected turn pose.
        // Preserve the complete source byte now so the low-nibble projectile direction is
        // still available after ApplySimpleGroundedPoseChange replaces `Pose` below.
        byte moonwalkSourceShotDirection = wasMoonwalking
            ? ReadShotDirection(bus)
            : (byte)0;

        bool rightSource = IsRightFacingStandingPose(Pose) || IsRightFacingRunningPose(Pose) ||
            IsMoonwalkingFacingRightPose(Pose) ||
            IsRightFacingRanIntoWallPose(Pose) ||
            IsRightFacingCrouchingPose(Pose) ||
            IsRightFacingLandingPose(Pose);
        bool leftSource = IsLeftFacingStandingPose(Pose) || IsLeftFacingRunningPose(Pose) ||
            IsMoonwalkingFacingLeftPose(Pose) ||
            IsLeftFacingRanIntoWallPose(Pose) ||
            IsLeftFacingCrouchingPose(Pose) ||
            IsLeftFacingLandingPose(Pose);
        bool turnsLeft = targetPose == (wasCrouching
            ? TurningRightToLeftCrouchingPose
            : TurningRightToLeftPose) && rightSource;
        bool turnsRight = targetPose == (wasCrouching
            ? TurningLeftToRightCrouchingPose
            : TurningLeftToRightPose) && leftSource;
        if (!turnsLeft && !turnsRight)
        {
            throw new InvalidOperationException(
                $"Grounded turn ${Pose:X2} -> ${targetPose:X2} is not a verified transition.");
        }

        // `$91:F8D3` reads the PREVIOUS pose record before it installs the final turn art.
        // Previous movement type five selects `$91:F9CC`; every other admitted source uses
        // `$91:F9C2`. Both tables have ten entries, but shot directions four/five belong to
        // the compact straight-down family that remains untranslated.
        byte shotDirection = ReadShotDirection(bus);
        byte selectedTurnPose = wasCrouching
            ? shotDirection switch
            {
                0 => TurningRightToLeftCrouchingAimUpPose,
                1 => TurningRightToLeftCrouchingAimDiagonalUpPose,
                2 => TurningRightToLeftCrouchingPose,
                3 => TurningRightToLeftCrouchingAimDiagonalDownPose,
                6 => TurningLeftToRightCrouchingAimDiagonalDownPose,
                7 => TurningLeftToRightCrouchingPose,
                8 => TurningLeftToRightCrouchingAimDiagonalUpPose,
                9 => TurningLeftToRightCrouchingAimUpPose,
                _ => throw new NotSupportedException(
                    $"Crouched turn shot direction ${shotDirection:X2} is not translated."),
            }
            : shotDirection switch
            {
                0 => TurningRightToLeftAimUpPose,
                1 => TurningRightToLeftAimDiagonalUpPose,
                2 => TurningRightToLeftPose,
                3 => TurningRightToLeftAimDiagonalDownPose,
                6 => TurningLeftToRightAimDiagonalDownPose,
                7 => TurningLeftToRightPose,
                8 => TurningLeftToRightAimDiagonalUpPose,
                9 => TurningLeftToRightAimUpPose,
                _ => throw new NotSupportedException(
                    $"Grounded turn shot direction ${shotDirection:X2} is not translated."),
            };
        if ((turnsLeft && !IsRightToLeftGroundTurnPose(selectedTurnPose)) ||
            (turnsRight && !IsLeftToRightGroundTurnPose(selectedTurnPose)))
        {
            throw new InvalidOperationException(
                $"Grounded turn source ${Pose:X2} has direction metadata inconsistent with target ${targetPose:X2}.");
        }

        SamusHorizontalSpeedState speed = HorizontalSpeed;

        // $91:F931-$91:F941 folds the run-button/speed-booster component into base speed
        // with two independent 16-bit ADCs and their carry. Use one wrapping 16.16 add to
        // preserve exactly that pair of operations, including overflow at the high word.
        uint combinedSpeed = unchecked(speed.BaseFixed +
            ((uint)speed.ExtraRunSpeed << 16) + speed.ExtraRunSubspeed);
        speed.BaseSpeed = unchecked((ushort)(combinedSpeed >> 16));
        speed.BaseSubspeed = unchecked((ushort)combinedSpeed);

        // The native handler consumes the extra component and selects acceleration mode
        // one. $90:8EA9 interprets that mode as “move opposite the NEW facing direction”
        // while $90:9A7E decelerates, which is how reversal preserves old momentum.
        speed.ExtraRunSpeed = 0;
        speed.ExtraRunSubspeed = 0;
        speed.AccelerationMode = 1;

        ApplySimpleGroundedPoseChange(bus, Pose, selectedTurnPose, "Grounded turn");

        if (wasMoonwalking)
        {
            // The `$0100` tag makes HUD handler `$90:DD74` treat the one-frame turning
            // state as shooting. `$90:BA5F` later masks it away and consumes only this
            // source pose's direction byte when a beam slot can actually be initialized.
            PoseTransitionShotDirection = unchecked((ushort)(0x0100 | moonwalkSourceShotDirection));
        }
    }

    /// <summary>
    /// Ports the jumping/falling turn initializers at <c>$91:F952/$91:F98A</c>.
    /// The input tables publish only generic `$2F/$30/$87/$88`; the initializer reads the
    /// PREVIOUS pose's shot-direction byte and substitutes one of sixteen exact turn poses.
    /// </summary>
    /// <returns>
    /// True when the selected pose fits and is installed. False means the native larger-
    /// pose collision path retained the compact source or forced stable crouch.
    /// </returns>
    public bool TryApplyAerialTurn(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte genericTargetPose,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        byte sourcePose = Pose;
        byte sourceMovementType = ReadMovementType(bus);
        bool jumping = sourceMovementType == 2;
        bool falling = sourceMovementType == 6;
        bool turnsLeft = genericTargetPose == (jumping
            ? TurningRightToLeftJumpPose
            : TurningRightToLeftFallingPose);
        bool turnsRight = genericTargetPose == (jumping
            ? TurningLeftToRightJumpPose
            : TurningLeftToRightFallingPose);
        if ((!jumping && !falling) || (!turnsLeft && !turnsRight))
        {
            throw new InvalidOperationException(
                $"Aerial turn ${sourcePose:X2} -> ${genericTargetPose:X2} is not a verified type-2/type-6 transition.");
        }

        // These are literal copies of `$91:F9D6` and `$91:F9E0`. Directions four and five
        // are not typos: compact down-aim sources reuse `$91/$92` or `$95/$96` according to
        // facing. Retaining all ten entries is essential for a debugger to preserve aim.
        byte shotDirection = ReadShotDirection(bus);
        byte selectedTurnPose = jumping
            ? shotDirection switch
            {
                0 => TurningRightToLeftJumpAimUpPose,
                1 => TurningRightToLeftJumpAimDiagonalUpPose,
                2 => TurningRightToLeftJumpPose,
                3 or 4 => TurningRightToLeftJumpAimDownPose,
                5 or 6 => TurningLeftToRightJumpAimDownPose,
                7 => TurningLeftToRightJumpPose,
                8 => TurningLeftToRightJumpAimDiagonalUpPose,
                9 => TurningLeftToRightJumpAimUpPose,
                _ => throw new InvalidDataException($"Jump pose ${sourcePose:X2} has invalid shot direction ${shotDirection:X2}."),
            }
            : shotDirection switch
            {
                0 => TurningRightToLeftFallingAimUpPose,
                1 => TurningRightToLeftFallingAimDiagonalUpPose,
                2 => TurningRightToLeftFallingPose,
                3 or 4 => TurningRightToLeftFallingAimDownPose,
                5 or 6 => TurningLeftToRightFallingAimDownPose,
                7 => TurningLeftToRightFallingPose,
                8 => TurningLeftToRightFallingAimDiagonalUpPose,
                9 => TurningLeftToRightFallingAimUpPose,
                _ => throw new InvalidDataException($"Fall pose ${sourcePose:X2} has invalid shot direction ${shotDirection:X2}."),
            };

        if ((turnsLeft && !IsRightToLeftAerialTurnPose(selectedTurnPose)) ||
            (turnsRight && !IsLeftToRightAerialTurnPose(selectedTurnPose)))
        {
            throw new InvalidOperationException(
                $"Aerial turn source ${sourcePose:X2} has direction metadata inconsistent with ${genericTargetPose:X2}.");
        }

        // `$91:F952/$91:F98A` run this momentum conversion before the shared pose-change
        // collision resolver. Consequently even a cramped compact-pose rejection consumes
        // extra run speed; moving it after the probe would produce observably different WRAM.
        FoldExtraRunSpeedIntoBaseAndStartTurn();

        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
            bus,
            level,
            selectedTurnPose,
            nmiFrameCounter,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.RetainSource)
            return false;
        if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = selectedTurnPose;
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies `$91:F624` when spin/wall-jump input selects the opposite spin pose. A true
    /// direction reversal folds extra speed into base speed and selects mode one, preserving
    /// old-world direction while the newly facing pose decelerates.
    /// </summary>
    public void ApplySpinJumpDirectionTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!IsSpinJumpPose(targetPose) ||
            (!IsWallJumpPose(Pose) && !IsSpinJumpPose(Pose)))
        {
            throw new InvalidOperationException(
                $"Spin direction transition ${Pose:X2} -> ${targetPose:X2} is not verified.");
        }

        byte oldDirection = ReadPoseXDirection(bus);
        byte newDirection = ReadPoseXDirection(bus, targetPose);
        if (oldDirection != newDirection)
        {
            FoldExtraRunSpeedIntoBaseAndStartTurn();

            // Unlike bank-$91's grounded/aerial turn initializers, `$91:F624` calls
            // `Samus_CancelSpeedBoost` immediately after folding the extra pair. Waiting
            // for another movement frame would leave `$0B3C` observably stale.
            HorizontalSpeed.CancelRunningMomentum(oldDirection);
        }

        // Ordinary and wall-jump definition fallbacks publish generic `$19/$1A`, whereas
        // the dedicated `$81/$82` and `$1B/$1C` input tables can publish their already-
        // specialized opposite-facing record directly. Reduce either form back to the
        // generic direction before applying `$91:F624`'s live equipment priority. This is
        // observable when both bits are equipped: an `$81 -> $82` ROM record must remain
        // Screw art, while the same directional intent with Screw unequipped becomes Space
        // Jump art instead of trusting stale pose-table equipment state.
        byte genericTarget = newDirection == 4 ? SpinJumpLeftPose : SpinJumpRightPose;
        Pose = SelectEquippedSpinPose(genericTarget);
        RefreshCollisionRadii(bus);

        // InitializeSpinJump writes frame one, skipping the static first spin frame. This
        // is easy to miss because an ordinary ground jump initializes the pose elsewhere.
        InitializeAnimation(bus, initialFrame: 1);
    }

    /// <summary>
    /// Applies `$91:EABE`, solid-collision command five, and `$90:9949` after the block
    /// wall test succeeds. The launch values are read from the cartridge's dry-air tables.
    /// </summary>
    public void ApplyWallJumpTrigger(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!IsSpinJumpPose(Pose))
            throw new InvalidOperationException($"Wall-jump trigger requires spin pose, not ${Pose:X2}.");

        // `$91:F433` observes previous movement type three plus equipped Screw Attack and
        // immediately reloads the normal suit palette before `$83/$84` starts. The desktop
        // runtime performs palette writes later in the frame, so preserve that phase split.
        if ((EquippedItems & 0x0008) != 0)
            HorizontalSpeed.RequestNormalSuitPaletteRestore();

        Pose = ReadPoseXDirection(bus) == 4 ? WallJumpLeftPose : WallJumpRightPose;
        RefreshCollisionRadii(bus);

        // `$91:F2D3` clears acceleration and the ordinary base-speed pair before
        // `$90:9949` installs the launch. It deliberately does *not* touch the extra-run
        // pair at `$0B42/$0B44` or the running-momentum flag at `$0B3C`: a wall jump made
        // out of a Dash therefore carries that speed into movement type $14.
        HorizontalSpeed.AccelerationMode = 0;
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;

        // SolidVerticalCollision_WallJumpTriggered queues library-three sound five with a
        // six-entry threshold after clearing the base-speed words and before pose setup.
        LiquidPhysics.QueueMovementSound(library: 3, soundId: 0x05, maximumQueued: 6);

        SamusAerialMovement.InitializeWallJump(bus, this);
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Applies the pose/launch half of <c>$9B:C9CE</c> after the grapple wall-grace probe
    /// has accepted a fresh Jump edge. This route intentionally reverses the contact pose:
    /// `$B8` launches through left-facing `$84`, while `$B9` launches through `$83`.
    /// </summary>
    /// <remarks>
    /// The eventual seven-pixel horizontal push remains the ordinary `$FB` animation-command
    /// path already implemented by <see cref="AnimateNoFx"/>. This method performs only the
    /// immediate bank-$9B cleanup, prospective-pose command six, and environment-selected
    /// `$90:9949` vertical launch that occur when the wall-jump function runs.
    /// </remarks>
    public void ApplyGrappleWallJump(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (Pose is not (GrappleWallContactLeftPose or GrappleWallContactRightPose))
        {
            throw new InvalidOperationException(
                $"Grapple wall jump requires contact pose $B8/$B9, not ${Pose:X2}.");
        }

        // `$9B:C9D5-$C9EB` tests the contact pose's X-direction byte, not its descriptive
        // wall side. Direction eight selects `$84`; direction four selects `$83`.
        Pose = ReadPoseXDirection(bus) == 8 ? WallJumpLeftPose : WallJumpRightPose;
        RefreshCollisionRadii(bus);

        // `$9B:C9CE` clears the ordinary base-speed pair, then the normal pose-transition
        // machinery reaches `$90:9949`. Like the non-grapple route, it leaves both the
        // extra-run pair and `$0B3C` intact, so Dash momentum survives this wall launch.
        HorizontalSpeed.AccelerationMode = 0;
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;

        // `$9B:C9CE` uses generic QueueSound: library one, sound seven, maximum fifteen.
        // It also tears down the active beam flare so the wall-jump's charged-contact path
        // cannot inherit charge accumulated before grapple became active.
        LiquidPhysics.QueueMovementSound(library: 1, soundId: 0x07, maximumQueued: 15);
        ProjectileFlareCounter = 0;

        SamusAerialMovement.InitializeWallJump(bus, this);
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Commits the pose selected by <c>GrappleBeamFunction_Dropped</c> at `$9B:C8C5`.
    /// The caller supplies the exact cartridge-table target and this method runs the shared
    /// block-only pose-expansion collision before clearing grapple fall momentum.
    /// </summary>
    /// <returns>
    /// True when the requested target fits; false when native pose-change collision retains
    /// the source or substitutes stable crouch.
    /// </returns>
    public bool ApplyGrappleDropTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool supportedSource = Pose is
            GrappleSwingRightPose or GrappleSwingLeftPose or
            GrappleStandingRightPose or GrappleStandingLeftPose or
            GrappleStandingDownRightPose or GrappleStandingDownLeftPose or
            GrappleCrouchingRightPose or GrappleCrouchingLeftPose or
            GrappleCrouchingDownRightPose or GrappleCrouchingDownLeftPose or
            GrappleWallContactLeftPose or GrappleWallContactRightPose;
        bool supportedTarget = targetPose is
            FacingRightNormalPose or FacingLeftNormalPose or
            StandingAimUpRightPose or StandingAimUpLeftPose or
            StandingAimDiagonalUpRightPose or StandingAimDiagonalUpLeftPose or
            StandingAimDiagonalDownRightPose or StandingAimDiagonalDownLeftPose or
            CrouchingRightPose or CrouchingLeftPose or
            CrouchingAimUpRightPose or CrouchingAimUpLeftPose or
            CrouchingAimDiagonalUpRightPose or CrouchingAimDiagonalUpLeftPose or
            CrouchingAimDiagonalDownRightPose or CrouchingAimDiagonalDownLeftPose;
        if (!supportedSource || !supportedTarget)
        {
            throw new NotSupportedException(
                $"Grapple drop transition ${Pose:X2} -> ${targetPose:X2} is not a retail dropped-table route.");
        }

        byte sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
            bus,
            level,
            targetPose,
            nmiFrameCounter,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            Pose = targetPose;
            RefreshCollisionRadii(bus);
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
            InitializeAnimation(bus, initialFrame: 0);
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
        }

        // `$9B:C95E-$C967` clears the two base-X and two Y-speed words regardless of the
        // pose-collision result. Extra run speed is not touched by this routine; an ordinary
        // connected grapple never creates it, so preserving the words is the literal rule.
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        return collision == LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>
    /// Applies `$90:9DBF-$90:9DC8` when an early spin frame finds the wall-jump chord.
    /// This does not launch Samus: it rewinds the ordinary spin sequence to frame `$0A`
    /// with a one-tick timer so the visible wall-contact pose reaches eligibility naturally.
    /// </summary>
    public void ApplyWallContactAnimationRewind()
    {
        if (!IsSpinJumpPose(Pose))
            throw new InvalidOperationException($"Wall-contact rewind requires spin pose, not ${Pose:X2}.");
        AnimationFrameTimer = 1;

        // `$90:9D96-$90:9DA6` gives Screw Attack a longer pre-contact animation. Its first
        // eligible wall frame is 27, so contact rewinds to 26. Ordinary spin and Space Jump
        // both use the compact 10 -> 11 handoff.
        AnimationFrame = IsScrewAttackPose(Pose) ? (ushort)0x1a : (ushort)0x0a;
    }

    /// <summary>
    /// Exact 16.16 ADC equivalent shared by bank-$91's grounded, aerial, and spin-turn
    /// initializers. It deliberately permits word overflow, matching the 65816 registers.
    /// </summary>
    private void FoldExtraRunSpeedIntoBaseAndStartTurn()
    {
        SamusHorizontalSpeedState speed = HorizontalSpeed;
        uint combinedSpeed = unchecked(speed.BaseFixed +
            ((uint)speed.ExtraRunSpeed << 16) + speed.ExtraRunSubspeed);
        speed.BaseSpeed = unchecked((ushort)(combinedSpeed >> 16));
        speed.BaseSubspeed = unchecked((ushort)combinedSpeed);
        speed.ExtraRunSpeed = 0;
        speed.ExtraRunSubspeed = 0;
        speed.AccelerationMode = 1;
    }

    /// <summary>
    /// Applies the dry-room equipment half of <c>SamusFunc_F468_SpinJump</c> at
    /// <c>$91:F624</c> to a generic transition-table target `$19/$1A`.
    /// </summary>
    private byte SelectEquippedSpinPose(byte genericPose)
    {
        bool facingLeft = genericPose switch
        {
            SpinJumpRightPose => false,
            SpinJumpLeftPose => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(genericPose),
                genericPose,
                "Equipment spin selection requires generic pose $19 or $1A."),
        };

        // Native tests Screw Attack first. A save with both bits equipped therefore uses
        // `$81/$82`, not Space Jump art, while retaining Space Jump's repeat-jump physics.
        if ((EquippedItems & 0x0008) != 0)
            return facingLeft ? ScrewAttackLeftPose : ScrewAttackRightPose;
        if ((EquippedItems & 0x0200) != 0)
            return facingLeft ? SpaceJumpLeftPose : SpaceJumpRightPose;
        return genericPose;
    }

    /// <summary>
    /// Applies <c>SamusFunc_F468_NormalJump</c> at <c>$91:F543</c> after a normal-jump
    /// transition has selected one of the six cartridge-approved launch bodies.
    /// </summary>
    /// <remarks>
    /// This deliberately receives the already-selected target instead of testing the
    /// transition art `$4B/$4C/$55-$5A`. Native does not consume stored shine when Jump is
    /// first pressed; it waits until command `$F8/$FD` installs `$4D/$4E/$15/$16/$69/$6A`.
    /// That delay is visible both to animation and to the 180-frame palette countdown.
    /// </remarks>
    private bool TryBeginShinesparkWindup(
        ISnesAddressSpace bus,
        byte targetPose,
        byte previousMovementType)
    {
        bool right = targetPose is
            NeutralJumpRightPose or NormalJumpAimUpRightPose or
            NormalJumpAimDiagonalUpRightPose;
        bool left = targetPose is
            NeutralJumpLeftPose or NormalJumpAimUpLeftPose or
            NormalJumpAimDiagonalUpLeftPose;
        if ((!right && !left) || Shinespark.ShineTimer == 0 ||
            Shinespark.Phase != ShinesparkPhase.Stored)
        {
            return false;
        }

        Pose = right ? ShinesparkWindupRightPose : ShinesparkWindupLeftPose;
        RefreshCollisionRadii(bus);
        Shinespark.BeginWindup(this);

        // `$91:F56B-$F575` checks the PREVIOUS movement type and adjusts both current and
        // previous Y words by one. The host camera captures its previous point outside this
        // object, so only the live word is written here; the same-frame camera delta remains
        // one pixel and the following frame starts from the corrected coordinate.
        if (previousMovementType == 2)
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition - 1));

        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies a `$C7/$C8 -> $C9-$CE` input-table match and installs its exact special
    /// movement handler. No ordinary jump velocity or grounded momentum routine runs.
    /// </summary>
    public void ApplyShinesparkDirectionTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (Pose is not (ShinesparkWindupRightPose or ShinesparkWindupLeftPose))
        {
            throw new InvalidOperationException(
                $"Directional shinespark transition requires windup pose, not ${Pose:X2}.");
        }

        Shinespark.BeginDirectionalLaunch(bus, this, targetPose);
    }

    /// <summary>
    /// Applies the verified ordinary-input jump transitions selected from the cartridge's
    /// bank-$91 table, including <c>HandleJumpTransition</c>'s call to
    /// <c>Make_Samus_Jump</c>. Only the four no-equipment/no-aim routes admitted by the
    /// current runtime are accepted.
    /// </summary>
    public void ApplyOrdinaryJumpTransition(
        ISnesAddressSpace bus,
        byte targetPose,
        ushort controllerNewInput = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool verified = (Pose, targetPose) is
            (FacingRightNormalPose or StandingAimUpRightPose or
                StandingAimDiagonalUpRightPose or StandingAimDiagonalDownRightPose,
             NeutralJumpTransitionRightPose) or
            (FacingLeftNormalPose or StandingAimUpLeftPose or
                StandingAimDiagonalUpLeftPose or StandingAimDiagonalDownLeftPose,
             NeutralJumpTransitionLeftPose) or
            (FacingRightNormalPose or StandingAimUpRightPose or
                StandingAimDiagonalUpRightPose or StandingAimDiagonalDownRightPose,
             NormalJumpTransitionAimUpRightPose or
                NormalJumpTransitionAimDiagonalUpRightPose or
                NormalJumpTransitionAimDiagonalDownRightPose) or
            (FacingLeftNormalPose or StandingAimUpLeftPose or
                StandingAimDiagonalUpLeftPose or StandingAimDiagonalDownLeftPose,
             NormalJumpTransitionAimUpLeftPose or
                NormalJumpTransitionAimDiagonalUpLeftPose or
                NormalJumpTransitionAimDiagonalDownLeftPose) or
            (MovingRightNormalPose or MovingRightGunExtendedPose or RunningAimUpRightPose or
                RunningAimDiagonalUpRightPose or RunningAimDiagonalDownRightPose,
             SpinJumpRightPose) or
            (MovingLeftNormalPose or MovingLeftGunExtendedPose or RunningAimUpLeftPose or
                RunningAimDiagonalUpLeftPose or RunningAimDiagonalDownLeftPose,
             SpinJumpLeftPose) or
            // `$91:AF98-$AFFF` can leave the moonwalk turn art early while the backward
            // direction remains held, or select `$4B/$4C` on a fresh Jump edge. Both
            // routes call the same dry-air jump initializer after changing pose.
            (MoonwalkTurnJumpLeftPose or MoonwalkTurnJumpAimUpLeftPose or
                MoonwalkTurnJumpAimDownLeftPose,
             SpinJumpLeftPose or NeutralJumpTransitionLeftPose) or
            (MoonwalkTurnJumpRightPose or MoonwalkTurnJumpAimUpRightPose or
                MoonwalkTurnJumpAimDownRightPose,
             SpinJumpRightPose or NeutralJumpTransitionRightPose) or
            (RanIntoWallRightPose or RanIntoWallAimUpRightPose or RanIntoWallAimDownRightPose,
             NeutralJumpTransitionRightPose) or
            (RanIntoWallLeftPose or RanIntoWallAimUpLeftPose or RanIntoWallAimDownLeftPose,
             NeutralJumpTransitionLeftPose);
        if (!verified)
        {
            throw new NotSupportedException(
                $"Ordinary jump transition ${Pose:X2} -> ${targetPose:X2} is not translated.");
        }

        Pose = targetPose is SpinJumpRightPose or SpinJumpLeftPose
            ? SelectEquippedSpinPose(targetPose)
            : targetPose;
        RefreshCollisionRadii(bus);
        InitializeAnimation(bus, initialFrame: 0);
        SamusAerialMovement.InitializeJump(bus, this);

        if (ReadMovementType(bus) == 2 &&
            (controllerNewInput & (ushort)SnesButton.X) != 0)
        {
            // `$91:F5CF-$F5E6` runs only in the normal-jumping initializer. It reads the
            // newly installed pose's direction byte and adds `$8000`; spin-jump's separate
            // initializer never publishes this bridge even when Shoot and Jump share a frame.
            PoseTransitionShotDirection = unchecked((ushort)(0x8000 | ReadShotDirection(bus)));
        }
    }

    /// <summary>
    /// Ports `$90:EB20`, the unconditional current-state epilogue clear that follows the
    /// HUD/projectile handler. A failed cooldown/slot allocation must lose the bridge too;
    /// it is deliberately not retained until some later shot succeeds.
    /// </summary>
    public void ClearPoseTransitionShotDirection() => PoseTransitionShotDirection = 0;

    /// <summary>
    /// Applies the crouching table's direct `$27/$71/$73/$85 -> $01` and mirrored
    /// `$28/$72/$74/$86 -> $02` exits. These are real six-byte transition-table records;
    /// unlike the animated `$F7-$FC` stand-up family, they install the final standing pose
    /// immediately after pose-change collision has made room for its larger radius.
    /// </summary>
    /// <returns>
    /// False when the two-sided pose-change collision resolver falls back to stable
    /// crouch because both the floor and ceiling constrain the larger body.
    /// </returns>
    public bool TryApplyDirectCrouchToStandingTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool rightRoute = IsRightFacingCrouchingPose(Pose) &&
            targetPose == FacingRightNormalPose;
        bool leftRoute = IsLeftFacingCrouchingPose(Pose) &&
            targetPose == FacingLeftNormalPose;
        if (!rightRoute && !leftRoute)
        {
            throw new NotSupportedException(
                $"Direct crouch exit ${Pose:X2} -> ${targetPose:X2} is not a ROM-table route.");
        }

        byte sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
                bus,
                level,
                targetPose,
                nmiFrameCounter,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            // $91:FFA7 does not restore an aimed crouch when the attempted standing body
            // is boxed in. It selects the ordinary stable crouch from facing metadata.
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies the crouching table's `$4B/$4C` jump entry through the native ordering:
    /// enlarge radius 16 -> 19 with `$91:FDAE`, initialise movement type two, run the
    /// `$91:FC66` crouch-only Y adjustment, then call `Make_Samus_Jump`.
    /// </summary>
    /// <remarks>
    /// `$91:FC7D` tests the literal previous pose, not merely movement type five. Therefore
    /// only ordinary `$27/$28` subtract ten extra pixels; aimed `$71-$74/$85/$86` still
    /// jump, but retain only the collision resolver's bottom-alignment adjustment.
    /// </remarks>
    /// <returns>
    /// False when expansion is impossible. Simultaneous initial hits select ordinary stable
    /// crouch; rejection by a compensating opposite-side probe retains the source pose.
    /// </returns>
    public bool TryApplyCrouchJumpTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool rightRoute = IsRightFacingCrouchingPose(Pose) &&
            targetPose == NeutralJumpTransitionRightPose;
        bool leftRoute = IsLeftFacingCrouchingPose(Pose) &&
            targetPose == NeutralJumpTransitionLeftPose;
        if (!rightRoute && !leftRoute)
        {
            throw new NotSupportedException(
                $"Crouch jump ${Pose:X2} -> ${targetPose:X2} is not a ROM-table route.");
        }

        byte sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
                bus,
                level,
                targetPose,
                nmiFrameCounter,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));

        // HandleJumpTransition_NormalJumping at $91:FC7D performs this after pose
        // initialization/collision but before Make_Samus_Jump. It writes only current Y;
        // the desktop state has no separately exposed PreviousYPosition word to mirror.
        if (sourcePose is CrouchingRightPose or CrouchingLeftPose)
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition - 10));

        InitializeAnimation(bus, initialFrame: 0);
        SamusAerialMovement.InitializeJump(bus, this);
        return true;
    }

    /// <summary>
    /// Applies the ordinary or aimed crouch-start/stand-start transition records,
    /// including command seven's bottom alignment and the larger-radius collision branch
    /// from <c>HandlePoseChangeCollision</c>. The admitted target set is exactly
    /// `$35/$36/$3B/$3C/$F1-$FC`.
    /// </summary>
    /// <returns>
    /// False only when simultaneous floor and ceiling collision leave no verified room to
    /// expand from radius 16 to radius 21; native behavior then retains the crouching pose.
    /// </returns>
    public bool TryApplyPostureTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool startsCrouchingRight =
            (IsRightFacingStandingPose(Pose) || IsRightFacingRunningPose(Pose) ||
             IsMoonwalkingFacingRightPose(Pose) ||
             IsRightFacingRanIntoWallPose(Pose) ||
             IsRightFacingLandingPose(Pose)) &&
            targetPose is
                CrouchingTransitionRightPose or CrouchingTransitionAimUpRightPose or
                CrouchingTransitionAimDiagonalUpRightPose or
                CrouchingTransitionAimDiagonalDownRightPose;
        bool startsCrouchingLeft =
            (IsLeftFacingStandingPose(Pose) || IsLeftFacingRunningPose(Pose) ||
             IsMoonwalkingFacingLeftPose(Pose) ||
             IsLeftFacingRanIntoWallPose(Pose) ||
             IsLeftFacingLandingPose(Pose)) &&
            targetPose is
                CrouchingTransitionLeftPose or CrouchingTransitionAimUpLeftPose or
                CrouchingTransitionAimDiagonalUpLeftPose or
                CrouchingTransitionAimDiagonalDownLeftPose;
        bool startsCrouching = startsCrouchingRight || startsCrouchingLeft;
        bool startsStandingRight = IsRightFacingCrouchingPose(Pose) &&
            targetPose is
                StandingTransitionRightPose or StandingTransitionAimUpRightPose or
                StandingTransitionAimDiagonalUpRightPose or
                StandingTransitionAimDiagonalDownRightPose;
        bool startsStandingLeft = IsLeftFacingCrouchingPose(Pose) &&
            targetPose is
                StandingTransitionLeftPose or StandingTransitionAimUpLeftPose or
                StandingTransitionAimDiagonalUpLeftPose or
                StandingTransitionAimDiagonalDownLeftPose;
        bool startsStanding = startsStandingRight || startsStandingLeft;
        if (!startsCrouching && !startsStanding)
        {
            throw new NotSupportedException(
                $"Posture transition ${Pose:X2} -> ${targetPose:X2} is not translated.");
        }

        if (startsCrouching)
        {
            // `Samus_CrouchTrans` at `$91:F7B0` samples the high stage byte before later
            // posture movement can cancel running momentum. A stage-four crouch therefore
            // banks 180 palette-handler ticks even though the following stable crouch has
            // no horizontal Speed Booster state of its own.
            Shinespark.TryStoreFromSpeedBooster(HorizontalSpeed.SpeedBoostCounter);

            Pose = targetPose;
            RefreshCollisionRadii(bus);

            // Prospective command seven reads five from $91:ED36, installs the target's
            // radius 16, then moves center Y down five. Old radius 21 and new radius 16
            // therefore share exactly the same bottom collision boundary.
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + 5));
            InitializeAnimation(bus, initialFrame: 0);
            return true;
        }

        // Expanding from crouch radius 16 to standing-transition radius 21 invokes the
        // common pose-change collision routine before initialization. The shared helper
        // reads the target radius from this ROM rather than assuming the ordinary value 21.
        byte sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
                bus,
                level,
                targetPose,
                nmiFrameCounter,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies the ordinary Morph-Ball entry and exit records selected by the crouch and
    /// ball transition tables. This includes item gating, command-seven bottom alignment,
    /// and the block-only expansion collision used when unmorphing.
    /// </summary>
    /// <remarks>
    /// `$37/$38` shrink radius 16 to 7 and command seven moves center Y down nine pixels;
    /// this keeps the old crouching bottom boundary exactly fixed. `$3D/$3E` expand 7 to
    /// 16 through `$91:FDAE`; floor collision normally moves center up nine. If both sides
    /// constrain a radius-seven body, `$91:FFA7` rejects the target and keeps Samus morphed.
    /// </remarks>
    public bool TryApplyMorphTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool startsMorphingRight = IsRightFacingCrouchingPose(Pose) &&
            targetPose == MorphingTransitionRightPose;
        bool startsMorphingLeft = IsLeftFacingCrouchingPose(Pose) &&
            targetPose == MorphingTransitionLeftPose;
        bool startsMorphing = startsMorphingRight || startsMorphingLeft;
        bool startsUnmorphingRight =
            Pose is MorphBallGroundRightPose or MorphBallMovingRightPose or
                MorphBallFallingRightPose or SpringBallGroundRightPose or
                SpringBallMovingRightPose or SpringBallFallingRightPose or
                SpringBallJumpRightPose &&
            targetPose == UnmorphingTransitionRightPose;
        bool startsUnmorphingLeft =
            Pose is MorphBallGroundLeftPose or MorphBallMovingLeftPose or
                MorphBallFallingLeftPose or SpringBallGroundLeftPose or
                SpringBallMovingLeftPose or SpringBallFallingLeftPose or
                SpringBallJumpLeftPose &&
            targetPose == UnmorphingTransitionLeftPose;
        bool startsUnmorphing = startsUnmorphingRight || startsUnmorphingLeft;
        if (!startsMorphing && !startsUnmorphing)
        {
            throw new NotSupportedException(
                $"Morph transition ${Pose:X2} -> ${targetPose:X2} is not a ROM-table route.");
        }

        if (startsMorphing)
        {
            // InitializeSamusPose_MorphingTransition at $91:F7CE restores PreviousPose and
            // returns carry set when bit $0004 is absent. No radius, animation, or position
            // write from the rejected prospective pose survives that native frame.
            if ((EquippedItems & 0x0004) == 0)
                return false;

            Pose = targetPose;
            RefreshCollisionRadii(bus);
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + 9));

            // Prospective command seven also cancels an active bounce before starting the
            // transition. Ordinary crouch entry normally sees zero, but retaining the
            // literal writes makes externally stimulated debugger states deterministic.
            if (MorphBallBounceState != 0)
            {
                MorphBallBounceState = 0;
                Kinematics.YSubspeed = 0;
                Kinematics.YSpeed = 0;
                Kinematics.YDirection = 0;
            }
            InitializeAnimation(bus, initialFrame: 0);
            return true;
        }

        byte sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
            bus,
            level,
            targetPose,
            nmiFrameCounter,
            out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            // A morph source can only produce RetainSource here. Keep the defensive branch
            // explicit so a future caller cannot accidentally apply non-morph crouch fallback.
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Installs a stable ordinary Morph-Ball pose while preserving the shared eight-frame
    /// rolling animation exactly as <c>InitializeSamusPose_MorphBall</c> requests.
    /// </summary>
    public void ApplyMorphBallPoseChange(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool sourceSupported = IsStableBallPose(Pose);
        bool targetSupported = IsStableBallPose(targetPose);
        if (!sourceSupported || !targetSupported)
        {
            throw new NotSupportedException(
                $"Stable Morph-Ball transition ${Pose:X2} -> ${targetPose:X2} is not translated.");
        }

        byte previousDirection = ReadPoseXDirection(bus);
        int previousDelayList = AnimationDelayListAddress;
        Pose = targetPose;
        RefreshCollisionRadii(bus);

        // All ordinary ball poses point at `$91:B378`. Native writes $8000 to the new-frame
        // selector, causing `$91:FB5C` to return without changing frame OR timer. Assert the
        // table identity instead of depending on that retail-data fact silently.
        int targetDelayList = ResolveAnimationDelayList(bus);
        if (previousDelayList != targetDelayList)
        {
            throw new InvalidDataException(
                $"Morph-Ball transition selected mismatched animation lists ${previousDelayList:X6}/${targetDelayList:X6}.");
        }

        byte currentDirection = ReadPoseXDirection(bus);
        bool reversed = (previousDirection == 8 && currentDirection == 4) ||
            (previousDirection == 4 && currentDirection == 8);
        if (reversed)
        {
            // `$91:FA32-$91:FA52` folds run momentum into base speed with one 16-bit carry,
            // clears the extra words, and selects mode one so displacement initially keeps
            // travelling in the old direction while the ball decelerates through its turn.
            uint combined = unchecked(HorizontalSpeed.BaseFixed +
                ((uint)HorizontalSpeed.ExtraRunSpeed << 16) +
                HorizontalSpeed.ExtraRunSubspeed);
            HorizontalSpeed.BaseSpeed = unchecked((ushort)(combined >> 16));
            HorizontalSpeed.BaseSubspeed = unchecked((ushort)combined);
            // `$91:FA4C` invokes `Samus_CancelSpeedBoost` between the 16.16 fold and the
            // explicit extra-word clear. Preserve that ordering even for ordinary Dash.
            HorizontalSpeed.CancelRunningMomentum(currentDirection);
            HorizontalSpeed.ExtraRunSpeed = 0;
            HorizontalSpeed.ExtraRunSubspeed = 0;
            HorizontalSpeed.AccelerationMode = 1;
        }
    }

    /// <summary>
    /// Resolves an ordinary Morph-Ball downward collision through `$91:EA07` and
    /// `$91:F1FC`, including both automatic rebounds and the final grounded pose.
    /// </summary>
    /// <returns>True only when the collision ends grounded; false means another rebound.</returns>
    public bool ApplyMorphBallLanding(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!IsAirborneMorphBallPose(Pose) && !IsGroundedMorphBallPose(Pose))
            throw new InvalidOperationException($"Morph-Ball landing requires airborne pose $31/$32, not ${Pose:X2}.");

        if (MorphBallBounceState == 0 && Kinematics.YSpeed >= 3)
        {
            MorphBallBounceState = 1;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = ReadWord(bus, 0x909eb5);
            Kinematics.YSubspeed = ReadWord(bus, 0x909eb7);
            return false;
        }

        if (MorphBallBounceState == 1)
        {
            MorphBallBounceState = 2;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = unchecked((ushort)(ReadWord(bus, 0x909eb5) - 1));
            Kinematics.YSubspeed = ReadWord(bus, 0x909eb7);
            return false;
        }

        MorphBallBounceState = 0;
        Kinematics.YDirection = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        byte groundedPose = ReadPoseXDirection(bus) == 4
            ? MorphBallGroundLeftPose
            : MorphBallGroundRightPose;
        ApplyMorphBallPoseChange(bus, groundedPose);
        return true;
    }

    /// <summary>
    /// Resolves `$91:F25E` for Spring Ball. Holding Jump immediately calls the same
    /// cartridge-backed jump initializer as a grounded Spring Ball press; otherwise the
    /// low bounce byte advances through two rebounds while high byte `$0600` records the
    /// native Spring Ball bounce family.
    /// </summary>
    public bool ApplySpringBallLanding(ISnesAddressSpace bus, ushort controllerInput)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!IsAirborneSpringBallPose(Pose) && !IsGroundedSpringBallPose(Pose))
            throw new InvalidOperationException($"Spring-Ball landing requires pose $7D-$80, not ${Pose:X2}.");

        if ((controllerInput & (ushort)SnesButton.A) != 0)
        {
            MorphBallBounceState = 0;
            SamusAerialMovement.InitializeJump(bus, this);
            byte jumpPose = ReadPoseXDirection(bus) == 4
                ? SpringBallJumpLeftPose
                : SpringBallJumpRightPose;
            ApplyMorphBallPoseChange(bus, jumpPose);
            return false;
        }

        byte bounce = unchecked((byte)MorphBallBounceState);
        if (bounce == 0 && Kinematics.YSpeed >= 3)
        {
            MorphBallBounceState = 0x0601;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = ReadWord(bus, 0x909eb5);
            Kinematics.YSubspeed = ReadWord(bus, 0x909eb7);
            return false;
        }

        if (bounce == 1)
        {
            MorphBallBounceState = 0x0602;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = unchecked((ushort)(ReadWord(bus, 0x909eb5) - 1));
            Kinematics.YSubspeed = ReadWord(bus, 0x909eb7);
            return false;
        }

        MorphBallBounceState = 0;
        Kinematics.YDirection = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        byte groundPose = ReadPoseXDirection(bus) == 4
            ? SpringBallGroundLeftPose
            : SpringBallGroundRightPose;
        ApplyMorphBallPoseChange(bus, groundPose);
        return true;
    }

    /// <summary>
    /// Applies `$91:FC18` when grounded Spring Ball `$79/$7A` changes to `$7F/$80`.
    /// Moving-ground poses first pass through the same table-selected target, but the
    /// initializer only launches when the previous movement type was exactly `$11`.
    /// </summary>
    public void ApplySpringBallJump(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool validTarget = targetPose is SpringBallJumpRightPose or SpringBallJumpLeftPose;
        if (!IsGroundedSpringBallPose(Pose) || !validTarget)
            throw new NotSupportedException($"Spring-Ball jump ${Pose:X2} -> ${targetPose:X2} is not translated.");

        ApplyMorphBallPoseChange(bus, targetPose);
        MorphBallBounceState = 0;
        SamusAerialMovement.InitializeJump(bus, this);
    }

    /// <summary>
    /// Applies `$91:E8F2`'s type-four walk-off endpoint, retaining the rolling animation
    /// while changing to ordinary airborne pose `$31/$32` and starting dry-air gravity.
    /// </summary>
    public void ApplyMorphBallWalkOff(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool springBall = IsGroundedSpringBallPose(Pose);
        if (!IsGroundedMorphBallPose(Pose) && !springBall)
            throw new InvalidOperationException($"Morph-Ball walk-off requires grounded pose, not ${Pose:X2}.");

        byte fallingPose = ReadPoseXDirection(bus) == 4
            ? springBall ? SpringBallFallingLeftPose : MorphBallFallingLeftPose
            : springBall ? SpringBallFallingRightPose : MorphBallFallingRightPose;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        Kinematics.YDirection = 2;
        SamusAerialMovement.ConfigureEnvironmentGravity(bus, this);
        ApplyMorphBallPoseChange(bus, fallingPose);
    }

    /// <summary>
    /// Installs the ordinary or aimed falling pose selected by <c>$91:E8F2</c> when a
    /// grounded movement probe finds no floor. The collision command clears vertical
    /// speed and starts downward gravity before the pose is drawn.
    /// </summary>
    public void ApplyWalkedOffFloorTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool supportedSource =
            IsRightFacingStandingPose(Pose) || IsLeftFacingStandingPose(Pose) ||
            IsRightFacingRunningPose(Pose) || IsLeftFacingRunningPose(Pose) ||
            IsMoonwalkingPose(Pose) ||
            IsMoonwalkTurnJumpPose(Pose) ||
            IsRanIntoWallPose(Pose) ||
            IsRightFacingCrouchingPose(Pose) || IsLeftFacingCrouchingPose(Pose);
        byte expectedTarget = SelectFallingPoseForCurrentAim(bus);
        if (!supportedSource || targetPose != expectedTarget)
        {
            throw new NotSupportedException(
                $"Walk-off transition ${Pose:X2} -> ${targetPose:X2} is not translated.");
        }

        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        Kinematics.YDirection = 2;
        SamusAerialMovement.ConfigureEnvironmentGravity(bus, this);
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Ports the non-spinning direction lookup used by `$91:E8F2` for a grounded walk-off.
    /// </summary>
    public byte SelectFallingPoseForCurrentAim(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte shotDirection = ReadShotDirection(bus);
        return shotDirection switch
        {
            0 => FallingAimUpRightPose,
            1 => FallingAimDiagonalUpRightPose,
            2 => FallingRightPose,
            3 => FallingAimDiagonalDownRightPose,
            6 => FallingAimDiagonalDownLeftPose,
            7 => FallingLeftPose,
            8 => FallingAimDiagonalUpLeftPose,
            9 => FallingAimUpLeftPose,

            // Turn and crouch records store `$FB`/`$FF` rather than an arm direction.
            // Their facing byte still selects the ordinary unaimed falling pair.
            0xfb or 0xff => ReadPoseXDirection(bus) == 4
                ? FallingLeftPose
                : FallingRightPose,
            _ => throw new NotSupportedException(
                $"Walk-off shot direction ${shotDirection:X2} requires compact/downward falling collision handling."),
        };
    }

    /// <summary>
    /// Applies <c>$91:E95D</c>'s normal/spin/aimed landing choice and grounded collision cleanup
    /// at <c>$91:F010</c>. Expanding radius 19 to 21 moves Samus upward by two pixels so
    /// her feet stay on the same collision boundary, matching <c>$91:FF49</c>.
    /// </summary>
    public void ApplyAerialLanding(
        ISnesAddressSpace bus,
        bool wasSpinning,
        ushort controllerInput = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool leavingScrewAttack = IsScrewAttackPose(Pose);
        byte direction = ReadPoseXDirection(bus);
        bool facingLeft = direction == 4;
        byte targetPose;
        if (wasSpinning)
        {
            targetPose = facingLeft ? SpinLandingLeftPose : SpinLandingRightPose;
        }
        else
        {
            // `$91:E95D` checks `$FF` before indexing `$91:E9F3`; hurt/damage-boost art
            // deliberately stores that sentinel and lands through the ordinary facing pair.
            // Horizontal directions two/seven take `$91:E96E-$E98F`'s extra Shot-binding
            // test. Held Shot selects firing landing `$E6/$E7`; released Shot selects the
            // ordinary `$A4/$A5` pair. The six admitted aim directions select `$E0-$E5`.
            // Compact directions four/five intentionally remain in the collision-aware path.
            bool shotHeld = (controllerInput & (ushort)SnesButton.X) != 0;
            targetPose = ReadShotDirection(bus) switch
            {
                0 => LandingAimUpRightPose,
                1 => LandingAimDiagonalUpRightPose,
                2 => shotHeld ? FiringLandingRightPose : NormalLandingRightPose,
                3 => LandingAimDiagonalDownRightPose,
                6 => LandingAimDiagonalDownLeftPose,
                7 => shotHeld ? FiringLandingLeftPose : NormalLandingLeftPose,
                8 => LandingAimDiagonalUpLeftPose,
                9 => LandingAimUpLeftPose,
                0xff => facingLeft ? NormalLandingLeftPose : NormalLandingRightPose,
                byte shotDirection => throw new NotSupportedException(
                    $"Landing from shot direction ${shotDirection:X2} requires an untranslated compact/firing route."),
            };
        }

        ushort oldRadius = Kinematics.YRadius;
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        if (Kinematics.YRadius > oldRadius)
        {
            ushort difference = unchecked((ushort)(Kinematics.YRadius - oldRadius));
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition - difference));
        }

        HorizontalSpeed.AccelerationMode = 0;
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;

        // `$91:F433` reloads the normal suit palette whenever a pose change leaves spin or
        // wall-jump movement while Screw Attack is equipped. The host palette copy occurs
        // at the later `$91:D6F7` phase, so publish that same deferred work here.
        if (leavingScrewAttack)
            HorizontalSpeed.RequestNormalSuitPaletteRestore();
        Kinematics.YDirection = 0;
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Applies `$91:E9F3` directions four/five when radius-ten straight-down Samus lands.
    /// Both entries select ordinary `$A4/$A5`; the 10 -> 21 expansion still runs the full
    /// block pose-change collision resolver before collision command five clears motion.
    /// </summary>
    public bool TryApplyCompactAerialLanding(
        ISnesAddressSpace bus,
        RoomLevelData level,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        if (!IsCompactAerialPose(Pose))
            throw new InvalidOperationException($"Compact landing requires pose $17/$18/$2D/$2E, not ${Pose:X2}.");

        byte sourcePose = Pose;
        byte targetPose = ReadShotDirection(bus) switch
        {
            4 => NormalLandingRightPose,
            5 => NormalLandingLeftPose,
            byte shotDirection => throw new InvalidOperationException(
                $"Compact pose ${Pose:X2} has unexpected shot direction ${shotDirection:X2}."),
        };
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
            bus,
            level,
            targetPose,
            nmiFrameCounter,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            Pose = targetPose;
            RefreshCollisionRadii(bus);
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
            InitializeAnimation(bus, initialFrame: 0);
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
        }

        // `$91:F010` collision command five runs after pose selection even when the larger
        // body falls back to crouch, so no launch/fall residue survives the landing seam.
        HorizontalSpeed.AccelerationMode = 0;
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        Kinematics.YDirection = 0;
        return collision == LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>
    /// Consumes command $FD/$F8's command-three pose operand for every animation route in
    /// the current grounded/ordinary-air slice.
    /// </summary>
    public bool ApplyPendingVerifiedAnimationTransition(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (PendingTransitionalPose is not byte targetPose)
            return false;

        bool verified = (Pose, targetPose) is
            (TurningRightToLeftPose, FacingLeftNormalPose) or
            (TurningLeftToRightPose, FacingRightNormalPose) or
            (TurningRightToLeftAimUpPose, StandingAimUpLeftPose) or
            (TurningLeftToRightAimUpPose, StandingAimUpRightPose) or
            (TurningRightToLeftAimDiagonalDownPose, StandingAimDiagonalDownLeftPose) or
            (TurningLeftToRightAimDiagonalDownPose, StandingAimDiagonalDownRightPose) or
            (TurningRightToLeftAimDiagonalUpPose, StandingAimDiagonalUpLeftPose) or
            (TurningLeftToRightAimDiagonalUpPose, StandingAimDiagonalUpRightPose) or
            (TurningRightToLeftCrouchingPose, CrouchingLeftPose) or
            (TurningLeftToRightCrouchingPose, CrouchingRightPose) or
            (TurningRightToLeftCrouchingAimUpPose, CrouchingAimUpLeftPose) or
            (TurningLeftToRightCrouchingAimUpPose, CrouchingAimUpRightPose) or
            (TurningRightToLeftCrouchingAimDiagonalDownPose, CrouchingAimDiagonalDownLeftPose) or
            (TurningLeftToRightCrouchingAimDiagonalDownPose, CrouchingAimDiagonalDownRightPose) or
            (TurningRightToLeftCrouchingAimDiagonalUpPose, CrouchingAimDiagonalUpLeftPose) or
            (TurningLeftToRightCrouchingAimDiagonalUpPose, CrouchingAimDiagonalUpRightPose) or
            (NeutralJumpTransitionRightPose, NeutralJumpRightPose) or
            (NeutralJumpTransitionLeftPose, NeutralJumpLeftPose) or
            (NormalJumpTransitionAimUpRightPose, NormalJumpAimUpRightPose) or
            (NormalJumpTransitionAimUpLeftPose, NormalJumpAimUpLeftPose) or
            (NormalJumpTransitionAimDiagonalUpRightPose, NormalJumpAimDiagonalUpRightPose) or
            (NormalJumpTransitionAimDiagonalUpLeftPose, NormalJumpAimDiagonalUpLeftPose) or
            (NormalJumpTransitionAimDiagonalDownRightPose, NormalJumpAimDiagonalDownRightPose) or
            (NormalJumpTransitionAimDiagonalDownLeftPose, NormalJumpAimDiagonalDownLeftPose) or
            (CrouchingTransitionRightPose, CrouchingRightPose) or
            (CrouchingTransitionLeftPose, CrouchingLeftPose) or
            (StandingTransitionRightPose, FacingRightNormalPose) or
            (StandingTransitionLeftPose, FacingLeftNormalPose) or
            (CrouchingTransitionAimUpRightPose, CrouchingAimUpRightPose) or
            (CrouchingTransitionAimUpLeftPose, CrouchingAimUpLeftPose) or
            (CrouchingTransitionAimDiagonalUpRightPose, CrouchingAimDiagonalUpRightPose) or
            (CrouchingTransitionAimDiagonalUpLeftPose, CrouchingAimDiagonalUpLeftPose) or
            (CrouchingTransitionAimDiagonalDownRightPose, CrouchingAimDiagonalDownRightPose) or
            (CrouchingTransitionAimDiagonalDownLeftPose, CrouchingAimDiagonalDownLeftPose) or
            (StandingTransitionAimUpRightPose, StandingAimUpRightPose) or
            (StandingTransitionAimUpLeftPose, StandingAimUpLeftPose) or
            (StandingTransitionAimDiagonalUpRightPose, StandingAimDiagonalUpRightPose) or
            (StandingTransitionAimDiagonalUpLeftPose, StandingAimDiagonalUpLeftPose) or
            (StandingTransitionAimDiagonalDownRightPose, StandingAimDiagonalDownRightPose) or
            (StandingTransitionAimDiagonalDownLeftPose, StandingAimDiagonalDownLeftPose) or
            (MorphingTransitionRightPose, MorphBallGroundRightPose) or
            (MorphingTransitionRightPose, MorphBallFallingRightPose) or
            (MorphingTransitionRightPose, SpringBallGroundRightPose) or
            (MorphingTransitionRightPose, SpringBallFallingRightPose) or
            (MorphingTransitionLeftPose, MorphBallGroundLeftPose) or
            (MorphingTransitionLeftPose, MorphBallFallingLeftPose) or
            (MorphingTransitionLeftPose, SpringBallGroundLeftPose) or
            (MorphingTransitionLeftPose, SpringBallFallingLeftPose) or
            (UnmorphingTransitionRightPose, CrouchingRightPose) or
            (UnmorphingTransitionLeftPose, CrouchingLeftPose) or
            (NormalLandingRightPose, FacingRightNormalPose) or
            (NormalLandingLeftPose, FacingLeftNormalPose) or
            (SpinLandingRightPose, FacingRightNormalPose) or
            (SpinLandingLeftPose, FacingLeftNormalPose) or
            (LandingAimUpRightPose, StandingAimUpRightPose) or
            (LandingAimUpLeftPose, StandingAimUpLeftPose) or
            (LandingAimDiagonalUpRightPose, StandingAimDiagonalUpRightPose) or
            (LandingAimDiagonalUpLeftPose, StandingAimDiagonalUpLeftPose) or
            (LandingAimDiagonalDownRightPose, StandingAimDiagonalDownRightPose) or
            (LandingAimDiagonalDownLeftPose, StandingAimDiagonalDownLeftPose) or
            // `$91:B22D/$B231` is shared by ordinary and firing landings. `$E6/$E7`
            // therefore executes the literal same `$F8,$01/$02` terminal operands.
            (FiringLandingRightPose, FacingRightNormalPose) or
            (FiringLandingLeftPose, FacingLeftNormalPose) or
            // Every aerial-turn delay list ends in command `$F8 pp`. These pairs are the
            // literal operands from `$91:B3ED-$91:B490`, not inferred mirror poses.
            (TurningRightToLeftJumpPose, NormalJumpForwardLeftPose) or
            (TurningLeftToRightJumpPose, NormalJumpForwardRightPose) or
            (TurningRightToLeftFallingPose, FallingLeftPose) or
            (TurningLeftToRightFallingPose, FallingRightPose) or
            (TurningRightToLeftJumpAimUpPose, NormalJumpAimUpLeftPose) or
            (TurningLeftToRightJumpAimUpPose, NormalJumpAimUpRightPose) or
            (TurningRightToLeftJumpAimDownPose, NormalJumpAimDownLeftPose) or
            (TurningLeftToRightJumpAimDownPose, NormalJumpAimDownRightPose) or
            (TurningRightToLeftFallingAimUpPose, FallingAimUpLeftPose) or
            (TurningLeftToRightFallingAimUpPose, FallingAimUpRightPose) or
            (TurningRightToLeftFallingAimDownPose, FallingAimDownLeftPose) or
            (TurningLeftToRightFallingAimDownPose, FallingAimDownRightPose) or
            (TurningRightToLeftJumpAimDiagonalUpPose, NormalJumpAimDiagonalUpLeftPose) or
            (TurningLeftToRightJumpAimDiagonalUpPose, NormalJumpAimDiagonalUpRightPose) or
            (TurningRightToLeftFallingAimDiagonalUpPose, FallingAimDiagonalUpLeftPose) or
            (TurningLeftToRightFallingAimDiagonalUpPose, FallingAimDiagonalUpRightPose) or
            // `$91:B45B-$B478` ends every moonwalk turn/jump delay list with command
            // `$F8,$1A/$19`. Unlike an aerial turn, this is the first actual airborne pose,
            // so the special branch below also creates the dry-air jump velocity.
            (MoonwalkTurnJumpLeftPose or MoonwalkTurnJumpAimUpLeftPose or
                MoonwalkTurnJumpAimDownLeftPose, SpinJumpLeftPose) or
            (MoonwalkTurnJumpRightPose or MoonwalkTurnJumpAimUpRightPose or
                MoonwalkTurnJumpAimDownRightPose, SpinJumpRightPose) or
            // `$91:B545/$B556` terminate Crystal Flash finish art in `$FD,$01/$02`.
            // Its installed movement handler observes type zero on the following frame.
            (CrystalFlashRightPose, FacingRightNormalPose) or
            (CrystalFlashLeftPose, FacingLeftNormalPose) or
            // All four drained release streams eventually publish ordinary standing art.
            // These are literal `$FD` operands from `$91:B257-$B298`.
            (DrainedCrouchingRightPose or DrainedStandingRightPose, FacingRightNormalPose) or
            (DrainedCrouchingLeftPose or DrainedStandingLeftPose, FacingLeftNormalPose);
        if (!verified)
        {
            throw new NotSupportedException(
                $"Animation transition ${Pose:X2} -> ${targetPose:X2} is outside the translated routes.");
        }

        bool startsMoonwalkJump = IsMoonwalkTurnJumpPose(Pose) &&
            targetPose is SpinJumpRightPose or SpinJumpLeftPose;
        byte sourcePose = Pose;
        byte previousMovementType = ReadMovementType(bus, sourcePose);

        // `$91:F404` runs movement-type initialization after the animation command has
        // selected the new normal-jump pose but before it initializes that pose's frame.
        // A live stored shine replaces the target with `$C7/$C8`, so never briefly seed
        // `$4D/$4E/$15/$16/$69/$6A` animation state in this branch.
        bool beganShinespark = TryBeginShinesparkWindup(
            bus,
            targetPose,
            previousMovementType);
        if (!beganShinespark)
        {
            // Moonwalk turn lists end in literal `$F8,$19/$1A`. That operand is still fed
            // through movement-type initialization, so equipment substitution occurs here
            // just as it does for an input-table jump from ordinary running.
            byte installedPose = startsMoonwalkJump
                ? SelectEquippedSpinPose(targetPose)
                : targetPose;
            ApplySimpleGroundedPoseChange(bus, sourcePose, installedPose, "Animation command");
        }
        if (sourcePose is DrainedCrouchingRightPose or DrainedCrouchingLeftPose or
            DrainedStandingRightPose or DrainedStandingLeftPose)
        {
            // The actor-owned release command has now reached its ROM-authored normal pose.
            // Clear only the host handler marker; pose initialization above already owns the
            // same radius and animation writes as native `$91:F404/$91:FB08`.
            Drained.CompleteRelease();
        }
        if (startsMoonwalkJump)
            SamusAerialMovement.InitializeJump(bus, this);
        MorphBallBounceState = 0;
        return true;
    }

    /// <summary>
    /// Installs one already-validated grounded pose and runs the common $91:F404/$91:FB08
    /// metadata, radius, and frame-zero animation work modeled by this class.
    /// </summary>
    private void ApplySimpleGroundedPoseChange(
        ISnesAddressSpace bus,
        byte expectedPose,
        byte targetPose,
        string transitionName)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (Pose != expectedPose)
        {
            throw new InvalidOperationException(
                $"{transitionName} requires pose ${expectedPose:X2}, not ${Pose:X2}.");
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>Last top-half index passed to bank-$81's Samus spritemap routine.</summary>
    public ushort TopSpritemapIndex { get; private set; }

    /// <summary>Last bottom-half index passed to bank-$81's Samus spritemap routine.</summary>
    public ushort BottomSpritemapIndex { get; private set; }

    /// <summary>Last screen-space origin calculated from world position and layer-1 scroll.</summary>
    public ushort SpritemapXPosition { get; private set; }

    /// <summary>Last screen-space origin calculated from world position and layer-1 scroll.</summary>
    public ushort SpritemapYPosition { get; private set; }

    /// <summary>The exact pending bank-$92 definitions consumed by accepted NMI.</summary>
    public SamusTileTransferState TileTransfers { get; } = new();

    /// <summary>
    /// Copies <c>SamusPalettes_PowerSuit</c> at <c>$9B:9400</c> to palette-buffer/CGRAM
    /// entries 192–207, porting <c>Samus_LoadSuitPalette</c>'s no-suit branch.
    /// </summary>
    public void LoadPowerSuitPalette(ISnesAddressSpace bus, SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);

        // OBJ palettes begin at CGRAM 128. Spritemap palette 4 therefore resolves to 192,
        // exactly matching CopyToSamusSuitPalette's &palette_buffer[192] destination.
        cgram.LoadFromBus(bus, PowerSuitPalette, colorCount: 16, destinationIndex: 192);
    }

    /// <summary>
    /// Seeds pose/frame-zero graphics before the first NMI, as room/game setup must do
    /// before a visible normal-gameplay Samus can be constructed.
    /// </summary>
    public void PrimeGraphics(ISnesAddressSpace bus) =>
        TileTransfers.SelectForPoseFrame(bus, Pose, AnimationFrame);

    /// <summary>
    /// Seeds the animation frame timer as <c>Set_Samus_AnimationFrame_if_PoseChanged</c>
    /// at <c>$91:FB08</c> does, including its bottom-minus-one liquid boundary test.
    /// </summary>
    public void InitializeAnimation(ISnesAddressSpace bus, ushort initialFrame = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);

        AnimationFrame = initialFrame;
        // `$91:FB08` does not simply reuse the previous animation pass's `$0A9A`. It
        // recomputes water/lava delay from the NEW pose radius at Y + radius - 1, while
        // Gravity Suit takes the ordinary speed-divisor path.
        AnimationFrameBuffer = LiquidPhysics.DeterminePoseChangeAnimationBuffer(this);
        AnimationDelayListAddress = ResolveAnimationDelayList(bus);
        byte initialDelay = ReadAnimationByte(bus, AnimationFrame);
        if ((initialDelay & 0x80) != 0)
        {
            throw new InvalidOperationException(
                $"Samus pose ${Pose:X2} cannot initialize directly on delay command ${initialDelay:X2}.");
        }

        AnimationFrameTimer = unchecked((ushort)(AnimationFrameBuffer + initialDelay));
        LastAnimationDelayCommand = null;
        PendingTransitionalPose = null;
    }

    /// <summary>
    /// Rebinds the delay-list pointer after a bank-$91 scripted controller writes Pose and
    /// literal animation words without calling ordinary pose initialization.
    /// </summary>
    /// <remarks>
    /// Drained-controller functions one and four do exactly that at `$91:E571/$91:E60C`.
    /// The cartridge's animation pass resolves the pointer from Pose every call, whereas
    /// this C# port caches it as a consistency check. This narrow method updates only that
    /// cache plus the controller's explicit writes; it intentionally does not recompute the
    /// liquid animation buffer or silently refresh collision radius.
    /// </remarks>
    internal void SetPoseAndAnimationFromScriptedController(
        ISnesAddressSpace bus,
        byte pose,
        ushort frame,
        ushort timer,
        bool refreshRadius)
    {
        ArgumentNullException.ThrowIfNull(bus);
        Pose = pose;
        if (refreshRadius)
            RefreshCollisionRadii(bus);
        AnimationDelayListAddress = ResolveAnimationDelayList(bus);
        AnimationFrame = frame;
        AnimationFrameTimer = timer;
        LastAnimationDelayCommand = null;
        PendingTransitionalPose = null;
    }

    /// <summary>
    /// Publishes the angle-selected grapple art exactly as $9B:BD95 does before the normal
    /// bank-$91 animation pass. The latter will decrement this fifteen to fourteen later in
    /// the same gameplay frame; exposing this narrow writer prevents grapple code from
    /// acquiring a general-purpose escape hatch around animation invariants.
    /// </summary>
    public void SetGrappleSwingAnimationFrame(ushort frame)
    {
        if (Pose is not (GrappleSwingRightPose or GrappleSwingLeftPose))
            throw new InvalidOperationException($"Grapple swing art cannot be assigned to pose ${Pose:X2}.");
        AnimationFrame = frame;
        AnimationFrameTimer = 15;
    }

    /// <summary>
    /// Ports <c>AnimateSamus</c> at <c>$90:8000</c>, including movement-visible FX delay state.
    /// </summary>
    /// <remarks>
    /// The FX dispatcher publishes water/lava delay buffering, remembered medium, native
    /// atmospheric slots, sound requests, and periodic-damage words before the timer
    /// decrement. The historical name remains as a source-compatible debugger API.
    /// </remarks>
    public void AnimateNoFx(
        ISnesAddressSpace bus,
        ushort controllerInput = 0,
        ushort nmiFrameCounter = 0,
        Bank80SystemState? system = null,
        bool beginLiquidSoundRequestFrame = true)
    {
        ArgumentNullException.ThrowIfNull(bus);
        EnsureAnimationInitialized(bus);

        // `$90:8000` dispatches the active room-FX animation handler before touching the
        // frame timer. This also updates remembered `$0AD2`, which the next Space Jump gate
        // consumes independently of its current top-boundary submersion check.
        LiquidPhysics.PrepareAnimationFrame(
            bus,
            this,
            nmiFrameCounter,
            system,
            beginLiquidSoundRequestFrame);

        // $90:8032 keeps neutral-jump frame one alive in four-tick chunks while Samus is
        // still rising. This is intentionally tested before DEC and applies only when the
        // timer is exactly one; release/apex changes YDirection to two and lets it expire.
        if (Pose is NeutralJumpRightPose or NeutralJumpLeftPose &&
            Kinematics.YDirection != 2 &&
            AnimationFrame == 1 &&
            AnimationFrameTimer == 1)
        {
            AnimationFrameTimer = 4;
        }

        // DEC is 16-bit. A timer accidentally initialized to zero becomes $FFFF; BMI then
        // advances just like BEQ does for an ordinary 1 -> 0 expiration.
        AnimationFrameTimer = unchecked((ushort)(AnimationFrameTimer - 1));
        if (AnimationFrameTimer != 0 && (AnimationFrameTimer & 0x8000) == 0)
            return;

        AnimationFrame = unchecked((ushort)(AnimationFrame + 1));
        HandleAnimationDelay(bus, controllerInput);
    }

    /// <summary>
    /// Ports the animation-only core of <c>Draw_Samus_Starting_Death_Animation</c> at
    /// `$90:8976`. Unlike ordinary gameplay animation, this call deliberately skips liquid-
    /// FX delay buffering, neutral-jump hacks, and controller-dependent Dash interception.
    /// </summary>
    internal void AnimateDeathFrame(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        EnsureAnimationInitialized(bus);
        AnimationFrameTimer = unchecked((ushort)(AnimationFrameTimer - 1));
        if (AnimationFrameTimer != 0 && (AnimationFrameTimer & 0x8000) == 0)
            return;
        AnimationFrame = unchecked((ushort)(AnimationFrame + 1));
        HandleAnimationDelay(bus, controllerInput: 0);
    }

    /// <summary>
    /// Publishes an animation frame/timer pair written directly by an installed special
    /// movement handler instead of by the generic delay-program interpreter.
    /// </summary>
    /// <remarks>
    /// Crystal Flash writes `$0A94/$0A96` at `$90:D689` and `$90:D74E`. This narrow internal
    /// seam preserves the native ordering: movement writes the pair, then animation later
    /// in beta immediately decrements the newly written timer.
    /// </remarks>
    internal void SetAnimationFrameFromSpecialHandler(ushort frame, ushort timer)
    {
        AnimationFrame = frame;
        AnimationFrameTimer = timer;
    }

    private void HandleAnimationDelay(ISnesAddressSpace bus, ushort controllerInput)
    {
        byte delayOrCommand = ReadAnimationByte(bus, AnimationFrame);
        if ((delayOrCommand & 0x80) == 0)
        {
            LastAnimationDelayCommand = null;

            // `$90:8554-$8568` replaces an ordinary running pose's per-pose delay list
            // with the pointer stored at `$91:B5D1` whenever momentum flag `$0B3C` is set.
            // Notice that this branch does NOT test the Dash button: releasing B retains
            // the default running cadence until a transition/collision cancels momentum.
            byte runningOrPoseDelay = HorizontalSpeed.HasRunningMomentum && ReadMovementType(bus) == 1
                ? (EquippedItems & 0x2000) != 0
                    ? HorizontalSpeed.ReadSpeedBoosterAnimationByte(bus, AnimationFrame)
                    : ReadDefaultRunningAnimationByte(bus, AnimationFrame)
                : delayOrCommand;
            AnimationFrameTimer = unchecked((ushort)(AnimationFrameBuffer + runningOrPoseDelay));
            return;
        }

        bool activeDashAnimation = HorizontalSpeed.HasRunningMomentum &&
            ReadMovementType(bus) == 1 &&
            (controllerInput & (ushort)SnesButton.B) != 0;
        if (activeDashAnimation)
        {
            // `$90:852C-$8543` intercepts a command byte before the generic command table.
            // The no-Speed-Booster route restarts frame zero and uses byte zero from the
            // default running-delay stream. Returning here is important: native returns
            // command number zero, whose handler does no further timer selection.
            if ((EquippedItems & 0x2000) != 0)
            {
                ushort stagedFrame = AnimationFrame;
                if (HorizontalSpeed.TryAdvanceSpeedBoosterAnimationStage(
                    bus,
                    movementType: 1,
                    controllerInput,
                    AnimationFrameBuffer,
                    ref stagedFrame,
                    out ushort stagedTimer))
                {
                    AnimationFrame = stagedFrame;
                    AnimationFrameTimer = stagedTimer;
                    LastAnimationDelayCommand = delayOrCommand;
                    return;
                }

                // A nonzero low counter byte makes `$90:852C` return the original command,
                // so normal `$FE/$FF/...` dispatch below proceeds against the pose stream.
            }
            else
            {
                AnimationFrame = 0;
                LastAnimationDelayCommand = delayOrCommand;
                AnimationFrameTimer = unchecked((ushort)(
                    AnimationFrameBuffer + ReadDefaultRunningAnimationByte(bus, byteIndex: 0)));
                return;
            }
        }

        LastAnimationDelayCommand = delayOrCommand;
        switch (delayOrCommand & 0x0f)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                // `$90:8324-$8345` points all six instruction slots at the same CLC/RTS
                // handler. Carry clear tells the caller to return immediately: native does
                // not change the animation frame and, crucially, does not reload its timer.
                // The outer routine has already advanced onto this command after a 1 -> 0
                // expiration, so the timer remains zero for the rest of this gameplay frame.
                // On the next frame DEC wraps it to `$FFFF`; BMI then advances once more,
                // stepping over the no-op command and interpreting the following delay.
                // `$F0` is used by the aimed-falling sequences `$6D-$70`; accepting it as a
                // one-frame command is therefore live game behavior, not defensive parsing.
                return;

            case 6:
                // $90:8346, command $F6: healthy Samus loops to zero; below 30 energy she
                // advances past the command into the alternate breathing sequence.
                AnimationFrame = Health < 30
                    ? unchecked((ushort)(AnimationFrame + 1))
                    : (ushort)0;
                break;

            case 7:
                // `$90:8360`, command `$F7`: install `$90:94CB`, then increment once more
                // past the command byte. The outer animation routine already performed its
                // ordinary pre-dispatch increment, so this second increment is essential.
                Drained.InstallFallingMovementHandler(this);
                AnimationFrame = unchecked((ushort)(AnimationFrame + 1));
                break;

            case 8:
                // $90:8370 falls through to command $FD's one-byte pose operand. For the
                // grounded $25/$26 sequences this publishes $02/$01 through command three;
                // it does NOT select a new delay or advance the visible animation frame.
                // The runtime consumes this after AnimateNoFx, where Samus_HandleTransitions
                // runs in the native frame. `$BF-$C4` use the same command to start their
                // literal `$19/$1A` spin-jump operand after the grounded turn art finishes.
                PendingTransitionalPose = ReadAnimationByte(
                    bus,
                    unchecked((ushort)(AnimationFrame + 1)));
                return;

            case 9:
                // $90:839A, command $F9 eeee gg aa GG AA. The mask is a little-endian
                // item word; unequipped/equipped each choose a grounded or airborne target
                // according to BOTH halves of Y speed. `$37/$38` test Spring Ball bit $0002
                // and use this command to finish ordinary morph entry without guessing
                // whether the transition walked off a ledge.
                ushort itemMask = unchecked((ushort)(
                    ReadAnimationByte(bus, unchecked((ushort)(AnimationFrame + 1))) |
                    (ReadAnimationByte(bus, unchecked((ushort)(AnimationFrame + 2))) << 8)));
                bool itemEquipped = (EquippedItems & itemMask) != 0;
                bool movingVertically = Kinematics.YSpeed != 0 || Kinematics.YSubspeed != 0;
                ushort targetOffset = itemEquipped
                    ? movingVertically ? (ushort)6 : (ushort)5
                    : movingVertically ? (ushort)4 : (ushort)3;
                PendingTransitionalPose = ReadAnimationByte(
                    bus,
                    unchecked((ushort)(AnimationFrame + targetOffset)));
                return;

            case 10:
                // `$90:83F6`, unused command `$FA gg aa`: select one transitional pose
                // when both halves of Y speed are zero and the other when either half is
                // nonzero. Retail has no reachable active pose using this instruction, but
                // preserving it completes the actual sixteen-entry interpreter instead of
                // treating valid cartridge bytecode as corrupt data.
                bool faMovingVertically = Kinematics.YSpeed != 0 || Kinematics.YSubspeed != 0;
                PendingTransitionalPose = ReadAnimationByte(
                    bus,
                    unchecked((ushort)(AnimationFrame + (faMovingVertically ? 2 : 1))));
                return;

            case 11:
                // `$90:841D`, command `$FB`, first checks TOP-boundary submersion. A fully
                // submerged non-Gravity body is forced to the ordinary one-byte sequence;
                // otherwise Screw Attack has priority over Space Jump. This top-edge test
                // deliberately differs from jump launch/gravity's bottom-edge test.
                AnimationFrame = unchecked((ushort)(AnimationFrame + (
                    (EquippedItems & SamusLiquidPhysicsState.GravitySuitItem) == 0 &&
                    LiquidPhysics.IsTopBoundarySubmerged(this) ? 1 :
                    (EquippedItems & 0x0008) != 0 ? 0x15 :
                    (EquippedItems & 0x0200) != 0 ? 0x0b : 1)));
                break;

            case 12:
                // `$90:848B`, unused command `$FC eeee gg aa`: the little-endian item
                // mask is followed by unequipped/equipped pose bytes. Poses `$3F/$40` are
                // the only retail users (Spring Ball-aware Morph Ball selection), but the
                // command still publishes through the ordinary transitional-pose seam.
                ushort fcItemMask = unchecked((ushort)(
                    ReadAnimationByte(bus, unchecked((ushort)(AnimationFrame + 1))) |
                    (ReadAnimationByte(bus, unchecked((ushort)(AnimationFrame + 2))) << 8)));
                bool fcItemEquipped = (EquippedItems & fcItemMask) != 0;
                PendingTransitionalPose = ReadAnimationByte(
                    bus,
                    unchecked((ushort)(AnimationFrame + (fcItemEquipped ? 4 : 3))));
                return;

            case 13:
                // $90:83A0, command $FD pp: publish pose pp through the same command-three
                // seam as $F8. Unlike F8, FD has no auto-jump special case before it falls
                // through here. The one-frame $4B/$4C neutral-jump transitions use it.
                PendingTransitionalPose = ReadAnimationByte(
                    bus,
                    unchecked((ushort)(AnimationFrame + 1)));
                return;

            case 14:
                // $90:84C7, command $FE nn: move backward nn byte positions. The operand
                // remains in the same delay stream and is not itself an animation frame.
                byte backwardCount = ReadAnimationByte(bus, unchecked((ushort)(AnimationFrame + 1)));
                AnimationFrame = unchecked((ushort)(AnimationFrame - backwardCount));
                break;

            case 15:
                // $90:84DB, command $FF: unconditional loop to the start of the sequence.
                AnimationFrame = 0;
                break;

        }

        byte selectedDelay = ReadAnimationByte(bus, AnimationFrame);
        if ((selectedDelay & 0x80) != 0)
        {
            throw new InvalidDataException(
                $"Samus animation command ${delayOrCommand:X2} selected another command ${selectedDelay:X2}.");
        }
        AnimationFrameTimer = unchecked((ushort)(AnimationFrameBuffer + selectedDelay));
    }

    private void EnsureAnimationInitialized(ISnesAddressSpace bus)
    {
        int expectedList = ResolveAnimationDelayList(bus);
        if (AnimationDelayListAddress != expectedList)
        {
            throw new InvalidOperationException(
                "Samus animation was not initialized for the current pose. Call InitializeAnimation after changing Pose.");
        }
    }

    private int ResolveAnimationDelayList(ISnesAddressSpace bus)
    {
        ushort pointer = ReadWord(bus, AddWithinBank(AnimationDelayPointerTable, Pose * 2));
        return 0x910000 | pointer;
    }

    private byte ReadAnimationByte(ISnesAddressSpace bus, ushort byteIndex) =>
        bus.ReadByte(AddWithinBank(AnimationDelayListAddress, byteIndex));

    /// <summary>
    /// Reads the shared ordinary-Dash delay list selected indirectly through `$91:B5D1`.
    /// Keeping the pointer lookup live means the ROM, not a duplicated C# byte array,
    /// remains authoritative for both cadence and command position.
    /// </summary>
    private static byte ReadDefaultRunningAnimationByte(ISnesAddressSpace bus, ushort byteIndex)
    {
        ushort listPointer = ReadWord(bus, 0x91b5d1);
        return bus.ReadByte(AddWithinBank(0x910000 | listPointer, byteIndex));
    }

    /// <summary>
    /// Ports the standing and ordinary-running portions of <c>Samus_Draw</c> at
    /// <c>$90:85E2</c>.
    /// </summary>
    /// <remarks>
    /// Movement type zero uses the standing position selector. The admitted movement types
    /// `$01/$02/$04-$06/$08/$0E/$10/$15/$17/$1A` use the usual or explicitly table-backed
    /// transition position selector. Morph types `$04/$08` draw only their complete top
    /// spritemap; spin-jump `$03` retains its own conditional bottom rule. Draygon's ten
    /// type-`$1A` bodies have ordinary split top/bottom spritemaps and no draw-time offset.
    /// </remarks>
    public void Draw(ISnesAddressSpace bus, OamBuffer oam, ushort layer1X, ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);

        int poseDefinition = AddWithinBank(PoseDefinitions, Pose * 8);
        byte movementType = bus.ReadByte(AddWithinBank(poseDefinition, 1));
        if (movementType is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 8 or 0x0a or
            0x0e or 0x0f or 0x10 or 0x11 or 0x12 or 0x13 or 0x14 or 0x15 or 0x16 or
            0x17 or 0x18 or 0x19 or 0x1a or 0x1b))
        {
            throw new NotSupportedException(
                $"Samus pose ${Pose:X2} uses movement type ${movementType:X2}; its rendering selector is not translated.");
        }

        // $90:8C94 sign-extends the byte at pose-definition offset four. Pose $01 stores
        // +6, moving the art origin six pixels above Samus's world-space center.
        sbyte graphicsYOffset = unchecked((sbyte)bus.ReadByte(AddWithinBank(poseDefinition, 4)));
        SpritemapXPosition = unchecked((ushort)(XPosition - layer1X));
        if (movementType == 0x0f && Pose is
            CrouchingTransitionRightPose or CrouchingTransitionLeftPose or
            MorphingTransitionRightPose or MorphingTransitionLeftPose or
            StandingTransitionRightPose or StandingTransitionLeftPose or
            UnmorphingTransitionRightPose or UnmorphingTransitionLeftPose)
        {
            // $90:8D3C indexes a signed byte by 2*(pose-$35)+animation frame instead of
            // using pose-definition graphics offset. The retail table has exactly two
            // signed bytes per pose `$35-$40`; transition animation commands replace the
            // pose before a command/operand index can reach this draw path.
            int transitionOffset = Pose switch
            {
                CrouchingTransitionRightPose or CrouchingTransitionLeftPose =>
                    AnimationFrame == 0 ? -8 : 0,
                MorphingTransitionRightPose or MorphingTransitionLeftPose =>
                    AnimationFrame == 0 ? -4 : -2,
                StandingTransitionRightPose or StandingTransitionLeftPose =>
                    AnimationFrame == 0 ? -4 : 0,
                UnmorphingTransitionRightPose or UnmorphingTransitionLeftPose =>
                    AnimationFrame == 0 ? 5 : 4,
                _ => throw new NotSupportedException(
                    $"Transition pose ${Pose:X2} does not use the translated `$90:8D80` offset table."),
            };
            SpritemapYPosition = unchecked((ushort)(YPosition + transitionOffset - layer1Y));
        }
        else if (Pose is DrainedCrouchingRightPose or DrainedCrouchingLeftPose)
        {
            // `$90:8DC1` indexes the shared 32-byte table at `$90:8DEF` directly with the
            // animation byte index. Several indices intentionally name command operands,
            // because the external controller can publish those literal indices for a
            // visible frame. Reading ROM keeps that odd layout authoritative.
            sbyte drainedOffset = unchecked((sbyte)bus.ReadByte(
                AddWithinBank(0x908def, AnimationFrame)));
            SpritemapYPosition = unchecked((ushort)(YPosition + drainedOffset - layer1Y));
        }
        else if ((Pose is DrainedStandingRightPose or DrainedStandingLeftPose) &&
                 AnimationFrame >= 5)
        {
            // `$90:8DB1-$8DBC` replaces the usual pose graphics offset with -3 after
            // standing drained art reaches byte index five.
            SpritemapYPosition = unchecked((ushort)(YPosition - 3 - layer1Y));
        }
        else
        {
            SpritemapYPosition = unchecked((ushort)(YPosition - graphicsYOffset - layer1Y));
        }

        ushort topBase = ReadWord(bus, AddWithinBank(TopSpritemapBaseIndexTable, Pose * 2));
        TopSpritemapIndex = unchecked((ushort)(topBase + AnimationFrame));
        oam.AddSamusSpritemap(bus, TopSpritemapIndex, SpritemapXPosition, SpritemapYPosition);

        // `$90:868D-$90:86C4` writes one small OBJ directly between the top and bottom
        // spritemap calls when unsuited pose `$00` faces the screen. This is not a visor:
        // the disassembly identifies tile `$021` as a cover for the left side of the power-
        // suit chest. The suited `$9B` body has that shape in its ordinary spritemap and
        // deliberately skips this write. Coordinates use Samus's world center directly,
        // not SpritemapYPosition (which has already applied pose graphics offset `$08`).
        if (Pose == ForwardFacingPowerSuitPose)
        {
            ushort chestX = unchecked((ushort)(XPosition - 7 - layer1X));
            ushort chestY = unchecked((ushort)(YPosition - 0x11 - layer1Y));
            oam.AddRawSmallSprite(chestX, chestY, attributes: 0x3821);
        }

        // $90:8686 suppresses the ordinary spin-jump bottom half for art frames 1..A;
        // those frames' top spritemaps contain the complete curled body. Frame zero and
        // frames B+ draw the split bottom. The admitted Screw/Space Jump records use their
        // separate native rule and always draw the bottom half at every animation frame.
        bool ordinarySpinBottom = movementType != 3 ||
            Pose is SpaceJumpRightPose or SpaceJumpLeftPose or
                ScrewAttackRightPose or ScrewAttackLeftPose ||
            AnimationFrame == 0 || AnimationFrame >= 0x0b;
        bool wallJumpBottom = movementType != 0x14 ||
            AnimationFrame < 3 || AnimationFrame >= 0x0d;
        bool damageBoostBottom = movementType != 0x19 ||
            AnimationFrame < 2 || AnimationFrame >= 9;
        // `$90:8790` suppresses the lower half for vertical shinesparks and for drained
        // crouch/fall byte indices zero and one. Every other type-$1B record draws it.
        bool specialType1BBottom = movementType != 0x1b ||
            (Pose is not (ShinesparkVerticalRightPose or ShinesparkVerticalLeftPose) &&
             (Pose is not (DrainedCrouchingRightPose or DrainedCrouchingLeftPose) ||
              AnimationFrame >= 2));
        bool drawBottom = movementType is not (4 or 8 or 0x11 or 0x12 or 0x13) &&
            ordinarySpinBottom && wallJumpBottom && damageBoostBottom && specialType1BBottom;
        // The native bottom selector clears this word when a complete top-half frame does
        // not need a bottom. Clearing it here also prevents the following echo renderer
        // from reusing a bottom spritemap left by an earlier animation frame.
        BottomSpritemapIndex = 0;
        if (drawBottom)
        {
            ushort bottomBase = ReadWord(bus, AddWithinBank(BottomSpritemapBaseIndexTable, Pose * 2));
            BottomSpritemapIndex = unchecked((ushort)(bottomBase + AnimationFrame));
            oam.AddSamusSpritemap(bus, BottomSpritemapIndex, SpritemapXPosition, SpritemapYPosition);
        }

        // Native Samus_Draw always performs this selection after its conditional OAM work.
        // Those flags drive the following accepted NMI, so stepping exposes the authentic
        // one-main-loop/one-NMI producer-consumer relationship.
        TileTransfers.SelectForPoseFrame(bus, Pose, AnimationFrame);
    }

    /// <summary>
    /// Draws the two ordinary Speed-Booster echoes from the positions captured by
    /// <c>Samus_UpdateSpeedEchoPos</c> at <c>$90:EEE7</c>.
    /// </summary>
    /// <remarks>
    /// This preserves both branches of <c>Samus_DrawEchoes</c> at <c>$90:87BD</c>. A
    /// nonnegative index draws stationary trailing snapshots only at boost stage four.
    /// Cancellation changes the index to <c>$FFFF</c>; drawing then advances each body's
    /// native ±8 X / ±2 Y convergence before deciding whether it crossed the live Samus.
    /// </remarks>
    public void DrawSpeedBoosterEchoes(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort layer1X,
        ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);

        if ((HorizontalSpeed.SpeedEchoIndex & 0x8000) != 0)
        {
            // `$90:87D3` walks slot one before slot zero. Movement is a draw-handler side
            // effect in the retail game, so do not advance it earlier in StepFrame: frames
            // whose Samus draw handler is suppressed must also freeze these copies.
            if (HorizontalSpeed.AdvanceDepartingSpeedEcho(
                    slot: 1,
                    XPosition,
                    YPosition))
            {
                DrawActiveSpeedBoosterEcho(
                    bus,
                    oam,
                    HorizontalSpeed.SecondSpeedEchoXPosition,
                    HorizontalSpeed.SecondSpeedEchoYPosition,
                    layer1X,
                    layer1Y);
            }

            if (HorizontalSpeed.AdvanceDepartingSpeedEcho(
                    slot: 0,
                    XPosition,
                    YPosition))
            {
                DrawActiveSpeedBoosterEcho(
                    bus,
                    oam,
                    HorizontalSpeed.FirstSpeedEchoXPosition,
                    HorizontalSpeed.FirstSpeedEchoYPosition,
                    layer1X,
                    layer1Y);
            }

            HorizontalSpeed.FinishDepartingSpeedEchoFrame();
            return;
        }

        if ((HorizontalSpeed.SpeedBoostCounter & 0xff00) != 0x0400)
            return;

        // `$90:87C7` draws slot one before slot zero. OAM order is observable when their
        // opaque pixels overlap, so retain that otherwise-surprising reverse order.
        DrawActiveSpeedBoosterEcho(
            bus,
            oam,
            HorizontalSpeed.SecondSpeedEchoXPosition,
            HorizontalSpeed.SecondSpeedEchoYPosition,
            layer1X,
            layer1Y);
        DrawActiveSpeedBoosterEcho(
            bus,
            oam,
            HorizontalSpeed.FirstSpeedEchoXPosition,
            HorizontalSpeed.FirstSpeedEchoYPosition,
            layer1X,
            layer1Y);
    }

    /// <summary>
    /// Ports `$90:88BA/$90:EBF3`: on odd NMI frames, draw both crash-orbit copies after
    /// the real Samus body using the current pose/frame spritemaps.
    /// </summary>
    public void DrawShinesparkCrashEchoes(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort layer1X,
        ushort layer1Y,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);
        if ((nmiFrameCounter & 1) == 0 ||
            Shinespark.Phase is not (ShinesparkPhase.Crash or ShinesparkPhase.CrashEchoCircle))
        {
            return;
        }

        // Slot one precedes slot zero just like native's X=2,0 loop.
        DrawActiveSpeedBoosterEcho(
            bus, oam,
            HorizontalSpeed.SecondSpeedEchoXPosition,
            HorizontalSpeed.SecondSpeedEchoYPosition,
            layer1X, layer1Y);
        DrawActiveSpeedBoosterEcho(
            bus, oam,
            HorizontalSpeed.FirstSpeedEchoXPosition,
            HorizontalSpeed.FirstSpeedEchoYPosition,
            layer1X, layer1Y);
    }

    /// <summary>
    /// Ports <c>Samus_DrawShinesparkCrashEchoProjectiles</c> at <c>$90:8953</c>. These
    /// copies are ordinary projectile-phase visuals after the centered crash circle ends.
    /// </summary>
    public void DrawReleasedShinesparkCrashEchoes(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort layer1X,
        ushort layer1Y,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);
        if ((nmiFrameCounter & 1) == 0)
            return;

        // `$90:8953` tests/draws fixed slot four (speed echo index three) before fixed
        // slot three (index two). That ordering controls OAM priority when copies overlap.
        ShinesparkReleasedEcho second = Shinespark.SecondReleasedCrashEcho;
        if (second.Active)
        {
            DrawActiveSpeedBoosterEcho(
                bus, oam, second.XPosition, second.YPosition, layer1X, layer1Y);
        }

        ShinesparkReleasedEcho first = Shinespark.FirstReleasedCrashEcho;
        if (first.Active)
        {
            DrawActiveSpeedBoosterEcho(
                bus, oam, first.XPosition, first.YPosition, layer1X, layer1Y);
        }
    }

    private void DrawActiveSpeedBoosterEcho(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort echoX,
        ushort echoY,
        ushort layer1X,
        ushort layer1Y)
    {
        // Zero X is the cartridge's empty-slot sentinel, not merely an off-screen point.
        if (echoX == 0)
            return;

        int poseDefinition = AddWithinBank(PoseDefinitions, Pose * 8);
        sbyte graphicsYOffset = unchecked((sbyte)bus.ReadByte(AddWithinBank(poseDefinition, 4)));
        short screenY = unchecked((short)(echoY - graphicsYOffset - layer1Y));

        // The original accepts screen Y 0..247. Horizontal clipping remains OAM/PPU work,
        // exactly as it is for the current Samus body.
        if (screenY < 0 || screenY >= 248)
            return;

        ushort screenX = unchecked((ushort)(echoX - layer1X));
        oam.AddSamusSpritemap(bus, TopSpritemapIndex, screenX, unchecked((ushort)screenY));
        if (BottomSpritemapIndex != 0)
            oam.AddSamusSpritemap(bus, BottomSpritemapIndex, screenX, unchecked((ushort)screenY));
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | (bus.ReadByte(AddWithinBank(address, 1)) << 8));

    private static int AddWithinBank(int address, int byteCount) =>
        (address & 0xff0000) | ((address + byteCount) & 0xffff);

    private enum LargerPoseCollisionOutcome
    {
        Allowed,
        RetainSource,
        CrouchFallback,
    }

    /// <summary>
    /// Resolves the block-only portion of `HandlePoseChangeCollision` at `$91:FDAE` for
    /// a target whose Y radius is larger than the live body's radius.
    /// </summary>
    /// <remarks>
    /// The cartridge first probes the radius difference upward and downward while retaining
    /// the old radius. A one-sided hit calculates a compensating center shift, then probes
    /// that shift toward the opposite surface before committing it. Failure of that second
    /// probe restores the source pose; simultaneous initial hits select stable crouch. Solid-
    /// enemy collision is deliberately outside the current room slice, with no substitute.
    /// </remarks>
    private LargerPoseCollisionOutcome ResolveLargerPoseCollision(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter,
        out int centerAdjustment)
    {
        int targetDefinition = AddWithinBank(PoseDefinitions, targetPose * 8);
        ushort targetRadius = bus.ReadByte(AddWithinBank(targetDefinition, 6));
        if (targetRadius <= Kinematics.YRadius)
        {
            centerAdjustment = 0;
            return LargerPoseCollisionOutcome.Allowed;
        }

        int radiusDifference = targetRadius - Kinematics.YRadius;

        // The probes run against independent copies. Native stores both available-distance
        // results before choosing a branch, so neither probe may move the live body early.
        SamusKinematicsState upwardProbe = CopyKinematics(Kinematics);
        BlockMoveResult upward = SamusBlockCollision.MoveVertical(
            bus,
            level,
            upwardProbe,
            displacement: unchecked(-radiusDifference << 16),
            scanLeftToRight: (nmiFrameCounter & 1) == 0);
        SamusKinematicsState downwardProbe = CopyKinematics(Kinematics);
        BlockMoveResult downward = SamusBlockCollision.MoveVertical(
            bus,
            level,
            downwardProbe,
            displacement: radiusDifference << 16,
            scanLeftToRight: (nmiFrameCounter & 1) == 0);

        if (upward.Collided && downward.Collided)
        {
            centerAdjustment = 0;
            // `$91:FFA7` compares the OLD live radius with eight. Radius seven means
            // Morph Ball, and that branch restores PreviousPose instead of trying to fit
            // stable crouch. Every non-morph compact/standing body still uses crouch.
            return Kinematics.YRadius < 8
                ? LargerPoseCollisionOutcome.RetainSource
                : LargerPoseCollisionOutcome.CrouchFallback;
        }

        centerAdjustment = 0;
        if (downward.Collided)
        {
            // `$91:FF49`: move away from the floor by
            // radiusDifference-spaceToMoveDown, preserving the old bottom boundary.
            int freeWholePixels = Math.Max(0, downward.AcceptedDisplacement >> 16);
            centerAdjustment = -(radiusDifference - freeWholePixels);

            // `$91:FF58` checks the proposed upward correction against the opposite side.
            // A collision here takes `$91:FE82` and restores PreviousPose; it does not use
            // `$91:FFA7`'s crouch selector because the initial flags were not both set.
            SamusKinematicsState oppositeProbe = CopyKinematics(Kinematics);
            BlockMoveResult opposite = SamusBlockCollision.MoveVertical(
                bus,
                level,
                oppositeProbe,
                displacement: centerAdjustment << 16,
                scanLeftToRight: (nmiFrameCounter & 1) == 0);
            if (opposite.Collided)
                return LargerPoseCollisionOutcome.RetainSource;
        }
        else if (upward.Collided)
        {
            // `$91:FF20` is the mirror: move down only as far as necessary to clear the
            // ceiling while admitting the enlarged body.
            int acceptedWhole = unchecked((short)(upward.AcceptedDisplacement >> 16));
            int freeWholePixels = Math.Max(0, -acceptedWhole);
            centerAdjustment = radiusDifference - freeWholePixels;

            // `$91:FF2B` is the downward mirror of the opposite-side check above.
            SamusKinematicsState oppositeProbe = CopyKinematics(Kinematics);
            BlockMoveResult opposite = SamusBlockCollision.MoveVertical(
                bus,
                level,
                oppositeProbe,
                displacement: centerAdjustment << 16,
                scanLeftToRight: (nmiFrameCounter & 1) == 0);
            if (opposite.Collided)
                return LargerPoseCollisionOutcome.RetainSource;
        }

        return LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>
    /// Applies `$91:FFA7`'s non-morph fallback after simultaneous above/below collision.
    /// Direction metadata selects `$27/$28`; an aimed crouch consequently loses its aim.
    /// </summary>
    private void ApplyPoseChangeCollisionCrouchFallback(ISnesAddressSpace bus, byte sourcePose)
    {
        ushort oldRadius = Kinematics.YRadius;
        byte fallbackPose = ReadPoseXDirection(bus) == 4
            ? CrouchingLeftPose
            : CrouchingRightPose;
        if (sourcePose == fallbackPose)
            return;

        Pose = fallbackPose;
        RefreshCollisionRadii(bus);
        if (oldRadius < Kinematics.YRadius)
        {
            // `$91:FFD4-$91:FFE9` subtracts the extra crouch radius from center Y. This is
            // normally invisible for radius-16 aimed crouches, but compact radius ten must
            // move up six pixels when simultaneous initial probes force `$27/$28`.
            Kinematics.YPosition = unchecked((ushort)(
                Kinematics.YPosition - (Kinematics.YRadius - oldRadius)));
        }
        InitializeAnimation(bus, initialFrame: 0);
    }

    private static SamusKinematicsState CopyKinematics(SamusKinematicsState source) => new()
    {
        XPosition = source.XPosition,
        XSubposition = source.XSubposition,
        YPosition = source.YPosition,
        YSubposition = source.YSubposition,
        XRadius = source.XRadius,
        YRadius = source.YRadius,
        YSpeed = source.YSpeed,
        YSubspeed = source.YSubspeed,
        YDirection = source.YDirection,
        YAcceleration = source.YAcceleration,
        YSubacceleration = source.YSubacceleration,
        HorizontalSlopeCollisionEnable = source.HorizontalSlopeCollisionEnable,
        PositionAdjustedBySlope = source.PositionAdjustedBySlope,
        // Prospective-pose probes still call the native solid-enemy detector before blocks.
        // Share the immutable per-frame actor snapshots while copying only Samus's geometry.
        InteractiveEnemies = source.InteractiveEnemies,
    };
}
