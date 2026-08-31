namespace SuperMetroid.Core.Game;

/// <summary>
/// Named cartridge pose identifiers grouped independently from mutable Samus state.
/// </summary>
public sealed partial class SamusState
{
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

    /// <summary>
    /// Pose `$33` is the unused right-facing body selected only by movement type `$07`'s
    /// otherwise-unused knockback transition at `$90:DF1D`. Naming it keeps the exhaustive
    /// native pointer table executable without pretending the retail game reaches it.
    /// </summary>
    public const byte UnusedKnockbackRightPose = 0x33;

    /// <summary>Left-facing partner of <see cref="UnusedKnockbackRightPose"/> at pose `$34`.</summary>
    public const byte UnusedKnockbackLeftPose = 0x34;

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
}
