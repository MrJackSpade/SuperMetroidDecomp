namespace SuperMetroid.Core.Game;

/// <summary>
/// Exclusive cartridge pose identifiers. Undefined byte values remain representable through an explicit cast for corrupt-state diagnostics.
/// </summary>
public enum SamusPoseId : byte
{
    /// <summary>
    /// Pose `$00`: the power-suit body viewed from the front during the intro, elevators,
    /// save-station appearance, and other controller-locked sequences.
    /// </summary>
    ForwardFacingPowerSuitPose = 0x00,

    /// <summary>
    /// Pose `$9B`: the Varia/Gravity-suit counterpart of pose `$00`. Both use movement type
    /// zero, but only the power-suit record needs bank `$90`'s extra chest-cover OBJ.
    /// </summary>
    ForwardFacingSuitedPose = 0x9b,

    /// <summary>Pose $01 is “facing right - normal” in the cartridge table.</summary>
    FacingRightNormalPose = 0x01,

    /// <summary>Pose $02 is “facing left - normal” in the cartridge table.</summary>
    FacingLeftNormalPose = 0x02,

    /// <summary>Pose $09 is “moving right - not aiming” in the cartridge table.</summary>
    MovingRightNormalPose = 0x09,

    /// <summary>Pose $0A is “moving left - not aiming” in the cartridge table.</summary>
    MovingLeftNormalPose = 0x0a,

    /// <summary>
    /// Pose `$0B` is the right-moving horizontal-fire body. It is a genuine movement-type-one
    /// record, shares the ordinary ten-frame running delay stream, and is selected when Shot
    /// remains held with Right after the projectile direction has been established.
    /// </summary>
    MovingRightGunExtendedPose = 0x0b,

    /// <summary>Pose `$0C` is the left-moving mirror of <see cref="MovingRightGunExtendedPose"/>.</summary>
    MovingLeftGunExtendedPose = 0x0c,

    /// <summary>Unused movement-type-$0D right-facing pose definition `$65`.</summary>
    UnusedPose65 = 0x65,

    /// <summary>Unused movement-type-$0D left-facing pose definition `$66`.</summary>
    UnusedPose66 = 0x66,

    /// <summary>
    /// Pose $49 visually faces left while moonwalking right. Its pose-X byte is deliberately
    /// eight: native horizontal movement follows travel direction, not the artwork's facing.
    /// </summary>
    MoonwalkFacingLeftPose = 0x49,

    /// <summary>Pose $4A visually faces right while moonwalking left.</summary>
    MoonwalkFacingRightPose = 0x4a,

    /// <summary>Pose `$75`: left-facing/right-moving moonwalk aimed diagonally up-left.</summary>
    MoonwalkAimUpLeftPose = 0x75,

    /// <summary>Pose `$76`: right-facing/left-moving mirror of pose `$75`.</summary>
    MoonwalkAimUpRightPose = 0x76,

    /// <summary>Pose `$77`: left-facing/right-moving moonwalk aimed diagonally down-left.</summary>
    MoonwalkAimDownLeftPose = 0x77,

    /// <summary>Pose `$78`: right-facing/left-moving mirror of pose `$77`.</summary>
    MoonwalkAimDownRightPose = 0x78,

    /// <summary>
    /// Pose `$BF`: right-facing moonwalk art turning/jumping toward the left. Despite the
    /// name in the disassembly, this is still grounded movement type `$0E` until its input
    /// table or terminal `$F8,$1A` animation command installs the actual spin jump.
    /// </summary>
    MoonwalkTurnJumpLeftPose = 0xbf,

    /// <summary>Pose `$C0`: left-facing mirror of <see cref="MoonwalkTurnJumpLeftPose"/>.</summary>
    MoonwalkTurnJumpRightPose = 0xc0,

    /// <summary>Pose `$C1`: aimed-up moonwalk turn whose terminal jump faces left.</summary>
    MoonwalkTurnJumpAimUpLeftPose = 0xc1,

    /// <summary>Pose `$C2`: aimed-up moonwalk turn whose terminal jump faces right.</summary>
    MoonwalkTurnJumpAimUpRightPose = 0xc2,

    /// <summary>Pose `$C3`: aimed-down moonwalk turn whose terminal jump faces left.</summary>
    MoonwalkTurnJumpAimDownLeftPose = 0xc3,

    /// <summary>Pose `$C4`: aimed-down moonwalk turn whose terminal jump faces right.</summary>
    MoonwalkTurnJumpAimDownRightPose = 0xc4,

    /// <summary>Pose `$C7`: right-facing stored-shine windup body.</summary>
    ShinesparkWindupRightPose = 0xc7,

    /// <summary>Pose `$C8`: left-facing stored-shine windup body.</summary>
    ShinesparkWindupLeftPose = 0xc8,

    /// <summary>Pose `$C9`: horizontal shinespark travelling right.</summary>
    ShinesparkHorizontalRightPose = 0xc9,

    /// <summary>Pose `$CA`: horizontal shinespark travelling left.</summary>
    ShinesparkHorizontalLeftPose = 0xca,

    /// <summary>Pose `$CB`: vertical shinespark using right-facing metadata.</summary>
    ShinesparkVerticalRightPose = 0xcb,

    /// <summary>Pose `$CC`: vertical shinespark using left-facing metadata.</summary>
    ShinesparkVerticalLeftPose = 0xcc,

    /// <summary>Pose `$CD`: diagonal up-right shinespark.</summary>
    ShinesparkDiagonalRightPose = 0xcd,

    /// <summary>Pose `$CE`: diagonal up-left shinespark.</summary>
    ShinesparkDiagonalLeftPose = 0xce,

    /// <summary>Pose `$D3`: right-facing Crystal Flash body and delay program.</summary>
    CrystalFlashRightPose = 0xd3,

    /// <summary>Pose `$D4`: left-facing Crystal Flash mirror.</summary>
    CrystalFlashLeftPose = 0xd4,

    /// <summary>Pose `$D5`: right-facing standing X-ray body.</summary>
    XrayingStandingRightPose = 0xd5,

    /// <summary>Pose `$D6`: left-facing standing X-ray body.</summary>
    XrayingStandingLeftPose = 0xd6,

    /// <summary>Pose `$D7`: right-facing fatal-damage / crystal-flash-ending body.</summary>
    DeathSequenceRightPose = 0xd7,

    /// <summary>Pose `$D8`: left-facing fatal-damage / crystal-flash-ending mirror.</summary>
    DeathSequenceLeftPose = 0xd8,

    /// <summary>Pose `$D9`: right-facing crouching X-ray body.</summary>
    XrayingCrouchingRightPose = 0xd9,

    /// <summary>Pose `$DA`: left-facing crouching X-ray body.</summary>
    XrayingCrouchingLeftPose = 0xda,

    /// <summary>Unused compact right-facing transition pose definition `$DB`.</summary>
    UnusedPoseDb = 0xdb,

    /// <summary>Unused compact left-facing transition pose definition `$DC`.</summary>
    UnusedPoseDc = 0xdc,

    /// <summary>Unused standing-radius right-facing transition pose definition `$DD`.</summary>
    UnusedPoseDd = 0xdd,

    /// <summary>Unused standing-radius left-facing transition pose definition `$DE`.</summary>
    UnusedPoseDe = 0xde,

    /// <summary>Unused Draygon-owned compact pose definition `$DF`.</summary>
    UnusedPoseDf = 0xdf,

    /// <summary>Pose `$E8`: right-facing drained crouch/fall animation.</summary>
    DrainedCrouchingRightPose = 0xe8,

    /// <summary>Pose `$E9`: left-facing drained crouch/fall animation.</summary>
    DrainedCrouchingLeftPose = 0xe9,

    /// <summary>Pose `$EA`: right-facing drained standing animation.</summary>
    DrainedStandingRightPose = 0xea,

    /// <summary>Pose `$EB`: left-facing drained standing animation.</summary>
    DrainedStandingLeftPose = 0xeb,

    /// <summary>Pose `$89`: facing right after forward running collides with a wall.</summary>
    RanIntoWallRightPose = 0x89,

    /// <summary>Pose `$8A`: facing-left mirror of pose `$89`.</summary>
    RanIntoWallLeftPose = 0x8a,

    /// <summary>Pose `$CF`: right-facing wall-stop art aimed diagonally up-right.</summary>
    RanIntoWallAimUpRightPose = 0xcf,

    /// <summary>Pose `$D0`: left-facing wall-stop art aimed diagonally up-left.</summary>
    RanIntoWallAimUpLeftPose = 0xd0,

    /// <summary>Pose `$D1`: right-facing wall-stop art aimed diagonally down-right.</summary>
    RanIntoWallAimDownRightPose = 0xd1,

    /// <summary>Pose `$D2`: left-facing wall-stop art aimed diagonally down-left.</summary>
    RanIntoWallAimDownLeftPose = 0xd2,

    /// <summary>Pose $25 turns a right-facing grounded Samus toward the left.</summary>
    TurningRightToLeftPose = 0x25,

    /// <summary>Pose $26 turns a left-facing grounded Samus toward the right.</summary>
    TurningLeftToRightPose = 0x26,

    /// <summary>Pose $19 is the ordinary right-facing spin jump.</summary>
    SpinJumpRightPose = 0x19,

    /// <summary>Pose $1A is the ordinary left-facing spin jump.</summary>
    SpinJumpLeftPose = 0x1a,

    /// <summary>
    /// Pose `$1B` is the right-facing Space Jump body. The equipment-aware movement-type
    /// initializer at `$91:F624` substitutes this for the transition table's ordinary
    /// `$19` target when Space Jump is equipped and Screw Attack is not.
    /// </summary>
    SpaceJumpRightPose = 0x1b,

    /// <summary>Pose `$1C` is the left-facing mirror of <see cref="SpaceJumpRightPose"/>.</summary>
    SpaceJumpLeftPose = 0x1c,

    /// <summary>
    /// Pose `$81` is the right-facing Screw Attack body. Screw Attack bit `$0008` has
    /// priority over Space Jump bit `$0200` in the native spin-pose selector.
    /// </summary>
    ScrewAttackRightPose = 0x81,

    /// <summary>Pose `$82` is the left-facing mirror of <see cref="ScrewAttackRightPose"/>.</summary>
    ScrewAttackLeftPose = 0x82,

    /// <summary>Pose $2F turns a right-facing normal jump toward the left.</summary>
    TurningRightToLeftJumpPose = 0x2f,

    /// <summary>Pose $30 turns a left-facing normal jump toward the right.</summary>
    TurningLeftToRightJumpPose = 0x30,

    /// <summary>Pose $83 is the right-facing wall-jump launch animation.</summary>
    WallJumpRightPose = 0x83,

    /// <summary>Pose $84 is the left-facing wall-jump launch animation.</summary>
    WallJumpLeftPose = 0x84,

    /// <summary>Pose $B2 is the right-facing airborne grapple-swing body.</summary>
    GrappleSwingRightPose = 0xb2,

    /// <summary>Pose $B3 is the left-facing airborne grapple-swing body.</summary>
    GrappleSwingLeftPose = 0xb3,

    /// <summary>Pose $A8 is the standing right-facing horizontal grapple lock.</summary>
    GrappleStandingRightPose = 0xa8,

    /// <summary>Pose $A9 is the standing left-facing horizontal grapple lock.</summary>
    GrappleStandingLeftPose = 0xa9,

    /// <summary>Pose $AA is the standing right-facing down-right grapple lock.</summary>
    GrappleStandingDownRightPose = 0xaa,

    /// <summary>Pose $AB is the standing left-facing down-left grapple lock.</summary>
    GrappleStandingDownLeftPose = 0xab,

    /// <summary>Pose $B4 is the crouched right-facing horizontal grapple lock.</summary>
    GrappleCrouchingRightPose = 0xb4,

    /// <summary>Pose $B5 is the crouched left-facing horizontal grapple lock.</summary>
    GrappleCrouchingLeftPose = 0xb5,

    /// <summary>Pose $B6 is the crouched right-facing down-right grapple lock.</summary>
    GrappleCrouchingDownRightPose = 0xb6,

    /// <summary>Pose $B7 is the crouched left-facing down-left grapple lock.</summary>
    GrappleCrouchingDownLeftPose = 0xb7,

    /// <summary>Pose $B8 is the left-wall grapple-jump contact body.</summary>
    GrappleWallContactLeftPose = 0xb8,

    /// <summary>Pose $B9 is the right-wall grapple-jump contact body.</summary>
    GrappleWallContactRightPose = 0xb9,

    /// <summary>Pose `$BA`: left-facing neutral body held in Draygon's claws.</summary>
    DraygonGrabbedNeutralLeftPose = 0xba,

    /// <summary>Pose `$BB`: left-facing Draygon-held body aiming diagonally upward.</summary>
    DraygonGrabbedAimUpLeftPose = 0xbb,

    /// <summary>Pose `$BC`: left-facing Draygon-held firing body.</summary>
    DraygonGrabbedFiringLeftPose = 0xbc,

    /// <summary>Pose `$BD`: left-facing Draygon-held body aiming diagonally downward.</summary>
    DraygonGrabbedAimDownLeftPose = 0xbd,

    /// <summary>Pose `$BE`: left-facing six-frame struggling animation while held by Draygon.</summary>
    DraygonGrabbedMovingLeftPose = 0xbe,

    /// <summary>Pose `$EC`: right-facing neutral body held in Draygon's claws.</summary>
    DraygonGrabbedNeutralRightPose = 0xec,

    /// <summary>Pose `$ED`: right-facing Draygon-held body aiming diagonally upward.</summary>
    DraygonGrabbedAimUpRightPose = 0xed,

    /// <summary>Pose `$EE`: right-facing Draygon-held firing body.</summary>
    DraygonGrabbedFiringRightPose = 0xee,

    /// <summary>Pose `$EF`: right-facing Draygon-held body aiming diagonally downward.</summary>
    DraygonGrabbedAimDownRightPose = 0xef,

    /// <summary>Pose `$F0`: right-facing six-frame struggling animation while held by Draygon.</summary>
    DraygonGrabbedMovingRightPose = 0xf0,

    /// <summary>
    /// Pose $4F is the visually left-facing damage boost; its X-direction byte is the
    /// deliberately reversed value eight that drives the boost to the right.
    /// </summary>
    DamageBoostLeftPose = 0x4f,

    /// <summary>
    /// Pose $50 is the visually right-facing damage boost; its X-direction byte is the
    /// deliberately reversed value four that drives the boost to the left.
    /// </summary>
    DamageBoostRightPose = 0x50,

    /// <summary>Pose $53 is the right-facing normal knockback body.</summary>
    KnockbackRightPose = 0x53,

    /// <summary>Pose $54 is the left-facing normal knockback body.</summary>
    KnockbackLeftPose = 0x54,

    /// <summary>Pose $87 turns a right-facing fall toward the left.</summary>
    TurningRightToLeftFallingPose = 0x87,

    /// <summary>Pose $88 turns a left-facing fall toward the right.</summary>
    TurningLeftToRightFallingPose = 0x88,

    TurningRightToLeftJumpAimUpPose = 0x8f,
    TurningLeftToRightJumpAimUpPose = 0x90,
    TurningRightToLeftJumpAimDownPose = 0x91,
    TurningLeftToRightJumpAimDownPose = 0x92,
    TurningRightToLeftFallingAimUpPose = 0x93,
    TurningLeftToRightFallingAimUpPose = 0x94,
    TurningRightToLeftFallingAimDownPose = 0x95,
    TurningLeftToRightFallingAimDownPose = 0x96,
    TurningRightToLeftJumpAimDiagonalUpPose = 0x9e,
    TurningLeftToRightJumpAimDiagonalUpPose = 0x9f,
    TurningRightToLeftFallingAimDiagonalUpPose = 0xa0,
    TurningLeftToRightFallingAimDiagonalUpPose = 0xa1,

    /// <summary>Pose $1D is the stationary right-facing ordinary morph ball on the ground.</summary>
    MorphBallGroundRightPose = 0x1d,

    /// <summary>Pose $1E is the ordinary morph ball rolling to the right on the ground.</summary>
    MorphBallMovingRightPose = 0x1e,

    /// <summary>Pose $1F is the ordinary morph ball rolling to the left on the ground.</summary>
    MorphBallMovingLeftPose = 0x1f,

    /// <summary>Pose $31 is the right-facing ordinary morph ball in the air.</summary>
    MorphBallFallingRightPose = 0x31,

    /// <summary>Pose $32 is the left-facing ordinary morph ball in the air.</summary>
    MorphBallFallingLeftPose = 0x32,

    /// <summary>
    /// Pose `$33` is the unused right-facing body selected only by movement type `$07`'s
    /// otherwise-unused knockback transition at `$90:DF1D`. Naming it keeps the exhaustive
    /// native pointer table executable without pretending the retail game reaches it.
    /// </summary>
    UnusedKnockbackRightPose = 0x33,

    /// <summary>Left-facing partner of <see cref="UnusedKnockbackRightPose"/> at pose `$34`.</summary>
    UnusedKnockbackLeftPose = 0x34,

    /// <summary>Pose $37 is the right-facing crouch-to-morph transition.</summary>
    MorphingTransitionRightPose = 0x37,

    /// <summary>Pose $38 is the left-facing crouch-to-morph transition.</summary>
    MorphingTransitionLeftPose = 0x38,

    /// <summary>Pose $3D is the right-facing morph-to-crouch transition.</summary>
    UnmorphingTransitionRightPose = 0x3d,

    /// <summary>Pose $3E is the left-facing morph-to-crouch transition.</summary>
    UnmorphingTransitionLeftPose = 0x3e,

    /// <summary>Pose $41 is the stationary left-facing ordinary morph ball on the ground.</summary>
    MorphBallGroundLeftPose = 0x41,

    /// <summary>Pose $79 is the stationary right-facing Spring Ball on the ground.</summary>
    SpringBallGroundRightPose = 0x79,

    /// <summary>Pose $7A is the stationary left-facing Spring Ball on the ground.</summary>
    SpringBallGroundLeftPose = 0x7a,

    /// <summary>Pose $7B is the Spring Ball moving right on the ground.</summary>
    SpringBallMovingRightPose = 0x7b,

    /// <summary>Pose $7C is the Spring Ball moving left on the ground.</summary>
    SpringBallMovingLeftPose = 0x7c,

    /// <summary>Pose $7D is the right-facing Spring Ball falling or bouncing.</summary>
    SpringBallFallingRightPose = 0x7d,

    /// <summary>Pose $7E is the left-facing Spring Ball falling or bouncing.</summary>
    SpringBallFallingLeftPose = 0x7e,

    /// <summary>Pose $7F is the right-facing Spring Ball in its powered jump.</summary>
    SpringBallJumpRightPose = 0x7f,

    /// <summary>Pose $80 is the left-facing Spring Ball in its powered jump.</summary>
    SpringBallJumpLeftPose = 0x80,

    /// <summary>Pose $29 is the unaimed right-facing falling pose.</summary>
    FallingRightPose = 0x29,

    /// <summary>Pose $2A is the unaimed left-facing falling pose.</summary>
    FallingLeftPose = 0x2a,

    /// <summary>
    /// Pose `$67` is the right-facing falling body with the cannon held horizontally after
    /// firing. Its `$91:B353` delay stream has the same terminal-velocity split as `$29`.
    /// </summary>
    FallingGunExtendedRightPose = 0x67,

    /// <summary>Pose `$68` is the left-facing mirror of <see cref="FallingGunExtendedRightPose"/>.</summary>
    FallingGunExtendedLeftPose = 0x68,

    /// <summary>Pose $03 is stationary, facing right, with the arm cannon aimed straight up.</summary>
    StandingAimUpRightPose = 0x03,

    /// <summary>Pose $04 is stationary, facing left, with the arm cannon aimed straight up.</summary>
    StandingAimUpLeftPose = 0x04,

    /// <summary>Pose $05 is stationary, facing right, and aiming diagonally up-right.</summary>
    StandingAimDiagonalUpRightPose = 0x05,

    /// <summary>Pose $06 is stationary, facing left, and aiming diagonally up-left.</summary>
    StandingAimDiagonalUpLeftPose = 0x06,

    /// <summary>Pose $07 is stationary, facing right, and aiming diagonally down-right.</summary>
    StandingAimDiagonalDownRightPose = 0x07,

    /// <summary>Pose $08 is stationary, facing left, and aiming diagonally down-left.</summary>
    StandingAimDiagonalDownLeftPose = 0x08,

    /// <summary>Pose $0D is the unused right-moving straight-up aim record.</summary>
    RunningAimUpRightPose = 0x0d,

    /// <summary>Pose $0E is the unused left-moving straight-up aim record.</summary>
    RunningAimUpLeftPose = 0x0e,

    /// <summary>Pose $0F moves right while aiming diagonally up-right.</summary>
    RunningAimDiagonalUpRightPose = 0x0f,

    /// <summary>Pose $10 moves left while aiming diagonally up-left.</summary>
    RunningAimDiagonalUpLeftPose = 0x10,

    /// <summary>Pose $11 moves right while aiming diagonally down-right.</summary>
    RunningAimDiagonalDownRightPose = 0x11,

    /// <summary>Pose $12 moves left while aiming diagonally down-left.</summary>
    RunningAimDiagonalDownLeftPose = 0x12,

    /// <summary>
    /// Pose `$13` is a stationary right-facing normal jump with the cannon extended. Unlike
    /// `$4D`, its pose table deliberately stores no no-input fallback, so the firing body can
    /// remain active until another input-table record or landing replaces it.
    /// </summary>
    NormalJumpGunExtendedRightPose = 0x13,

    /// <summary>Pose `$14` is the left-facing mirror of <see cref="NormalJumpGunExtendedRightPose"/>.</summary>
    NormalJumpGunExtendedLeftPose = 0x14,

    /// <summary>Pose $15 is a right-facing normal jump aimed straight up.</summary>
    NormalJumpAimUpRightPose = 0x15,

    /// <summary>Pose $16 is a left-facing normal jump aimed straight up.</summary>
    NormalJumpAimUpLeftPose = 0x16,

    /// <summary>Pose $17 is the compact right-facing normal jump aimed straight down.</summary>
    NormalJumpAimDownRightPose = 0x17,

    /// <summary>Pose $18 is the compact left-facing normal jump aimed straight down.</summary>
    NormalJumpAimDownLeftPose = 0x18,

    /// <summary>Pose $51 is a right-facing normal jump using the moving-forward art.</summary>
    NormalJumpForwardRightPose = 0x51,

    /// <summary>Pose $52 is a left-facing normal jump using the moving-forward art.</summary>
    NormalJumpForwardLeftPose = 0x52,

    /// <summary>Pose $55 is the right-facing jump transition aimed straight up.</summary>
    NormalJumpTransitionAimUpRightPose = 0x55,

    /// <summary>Pose $56 is the left-facing jump transition aimed straight up.</summary>
    NormalJumpTransitionAimUpLeftPose = 0x56,

    /// <summary>Pose $57 is the right-facing jump transition aimed diagonally up.</summary>
    NormalJumpTransitionAimDiagonalUpRightPose = 0x57,

    /// <summary>Pose $58 is the left-facing jump transition aimed diagonally up.</summary>
    NormalJumpTransitionAimDiagonalUpLeftPose = 0x58,

    /// <summary>Pose $59 is the right-facing jump transition aimed diagonally down.</summary>
    NormalJumpTransitionAimDiagonalDownRightPose = 0x59,

    /// <summary>Pose $5A is the left-facing jump transition aimed diagonally down.</summary>
    NormalJumpTransitionAimDiagonalDownLeftPose = 0x5a,

    /// <summary>Pose $69 is a right-facing normal jump aimed diagonally up.</summary>
    NormalJumpAimDiagonalUpRightPose = 0x69,

    /// <summary>Pose $6A is a left-facing normal jump aimed diagonally up.</summary>
    NormalJumpAimDiagonalUpLeftPose = 0x6a,

    /// <summary>Pose $6B is a right-facing normal jump aimed diagonally down.</summary>
    NormalJumpAimDiagonalDownRightPose = 0x6b,

    /// <summary>Pose $6C is a left-facing normal jump aimed diagonally down.</summary>
    NormalJumpAimDiagonalDownLeftPose = 0x6c,

    /// <summary>Pose $2B is right-facing falling aimed straight up.</summary>
    FallingAimUpRightPose = 0x2b,

    /// <summary>Pose $2C is left-facing falling aimed straight up.</summary>
    FallingAimUpLeftPose = 0x2c,

    /// <summary>Pose $2D is the compact right-facing fall aimed straight down.</summary>
    FallingAimDownRightPose = 0x2d,

    /// <summary>Pose $2E is the compact left-facing fall aimed straight down.</summary>
    FallingAimDownLeftPose = 0x2e,

    /// <summary>Pose $6D is right-facing falling aimed diagonally up.</summary>
    FallingAimDiagonalUpRightPose = 0x6d,

    /// <summary>Pose $6E is left-facing falling aimed diagonally up.</summary>
    FallingAimDiagonalUpLeftPose = 0x6e,

    /// <summary>Pose $6F is right-facing falling aimed diagonally down.</summary>
    FallingAimDiagonalDownRightPose = 0x6f,

    /// <summary>Pose $70 is left-facing falling aimed diagonally down.</summary>
    FallingAimDiagonalDownLeftPose = 0x70,

    /// <summary>Pose $71 is right-facing crouching aimed diagonally up.</summary>
    CrouchingAimDiagonalUpRightPose = 0x71,

    /// <summary>Pose $72 is left-facing crouching aimed diagonally up.</summary>
    CrouchingAimDiagonalUpLeftPose = 0x72,

    /// <summary>Pose $73 is right-facing crouching aimed diagonally down.</summary>
    CrouchingAimDiagonalDownRightPose = 0x73,

    /// <summary>Pose $74 is left-facing crouching aimed diagonally down.</summary>
    CrouchingAimDiagonalDownLeftPose = 0x74,

    /// <summary>Pose $85 is right-facing crouching aimed straight up.</summary>
    CrouchingAimUpRightPose = 0x85,

    /// <summary>Pose $86 is left-facing crouching aimed straight up.</summary>
    CrouchingAimUpLeftPose = 0x86,

    /// <summary>Pose $43 turns right-to-left while crouched without retaining aim.</summary>
    TurningRightToLeftCrouchingPose = 0x43,

    /// <summary>Pose $44 turns left-to-right while crouched without retaining aim.</summary>
    TurningLeftToRightCrouchingPose = 0x44,

    /// <summary>Pose $8B turns right-to-left on ground while preserving straight-up aim.</summary>
    TurningRightToLeftAimUpPose = 0x8b,

    /// <summary>Pose $8C turns left-to-right on ground while preserving straight-up aim.</summary>
    TurningLeftToRightAimUpPose = 0x8c,

    /// <summary>Pose $8D turns right-to-left on ground while preserving diagonal-down aim.</summary>
    TurningRightToLeftAimDiagonalDownPose = 0x8d,

    /// <summary>Pose $8E turns left-to-right on ground while preserving diagonal-down aim.</summary>
    TurningLeftToRightAimDiagonalDownPose = 0x8e,

    /// <summary>Pose $9C turns right-to-left on ground while preserving diagonal-up aim.</summary>
    TurningRightToLeftAimDiagonalUpPose = 0x9c,

    /// <summary>Pose $9D turns left-to-right on ground while preserving diagonal-up aim.</summary>
    TurningLeftToRightAimDiagonalUpPose = 0x9d,

    /// <summary>Pose $97 turns right-to-left while crouched and preserving straight-up aim.</summary>
    TurningRightToLeftCrouchingAimUpPose = 0x97,

    /// <summary>Pose $98 turns left-to-right while crouched and preserving straight-up aim.</summary>
    TurningLeftToRightCrouchingAimUpPose = 0x98,

    /// <summary>Pose $99 turns right-to-left while crouched and preserving diagonal-down aim.</summary>
    TurningRightToLeftCrouchingAimDiagonalDownPose = 0x99,

    /// <summary>Pose $9A turns left-to-right while crouched and preserving diagonal-down aim.</summary>
    TurningLeftToRightCrouchingAimDiagonalDownPose = 0x9a,

    /// <summary>Pose $A2 turns right-to-left while crouched and preserving diagonal-up aim.</summary>
    TurningRightToLeftCrouchingAimDiagonalUpPose = 0xa2,

    /// <summary>Pose $A3 turns left-to-right while crouched and preserving diagonal-up aim.</summary>
    TurningLeftToRightCrouchingAimDiagonalUpPose = 0xa3,

    /// <summary>Pose $E0 is a right-facing normal-jump landing aimed straight up.</summary>
    LandingAimUpRightPose = 0xe0,

    /// <summary>Pose $E1 is a left-facing normal-jump landing aimed straight up.</summary>
    LandingAimUpLeftPose = 0xe1,

    /// <summary>Pose $E2 is a right-facing normal-jump landing aimed diagonally up.</summary>
    LandingAimDiagonalUpRightPose = 0xe2,

    /// <summary>Pose $E3 is a left-facing normal-jump landing aimed diagonally up.</summary>
    LandingAimDiagonalUpLeftPose = 0xe3,

    /// <summary>Pose $E4 is a right-facing normal-jump landing aimed diagonally down.</summary>
    LandingAimDiagonalDownRightPose = 0xe4,

    /// <summary>Pose $E5 is a left-facing normal-jump landing aimed diagonally down.</summary>
    LandingAimDiagonalDownLeftPose = 0xe5,

    /// <summary>
    /// Pose `$E6` is the right-facing horizontal-fire landing selected by `$91:E95D` only
    /// when the current shot direction is two and the Shot binding is still held at impact.
    /// Its terminal `$F8,$01` command returns to ordinary standing after the landing frames.
    /// </summary>
    FiringLandingRightPose = 0xe6,

    /// <summary>Pose `$E7` is the left-facing mirror of <see cref="FiringLandingRightPose"/>.</summary>
    FiringLandingLeftPose = 0xe7,

    /// <summary>Pose $F1 transitions right-facing standing to crouching while aiming up.</summary>
    CrouchingTransitionAimUpRightPose = 0xf1,

    /// <summary>Pose $F2 transitions left-facing standing to crouching while aiming up.</summary>
    CrouchingTransitionAimUpLeftPose = 0xf2,

    /// <summary>Pose $F3 transitions right-facing standing to crouching while aiming diagonally up.</summary>
    CrouchingTransitionAimDiagonalUpRightPose = 0xf3,

    /// <summary>Pose $F4 transitions left-facing standing to crouching while aiming diagonally up.</summary>
    CrouchingTransitionAimDiagonalUpLeftPose = 0xf4,

    /// <summary>Pose $F5 transitions right-facing standing to crouching while aiming diagonally down.</summary>
    CrouchingTransitionAimDiagonalDownRightPose = 0xf5,

    /// <summary>Pose $F6 transitions left-facing standing to crouching while aiming diagonally down.</summary>
    CrouchingTransitionAimDiagonalDownLeftPose = 0xf6,

    /// <summary>Pose $F7 transitions right-facing crouching to standing while aiming up.</summary>
    StandingTransitionAimUpRightPose = 0xf7,

    /// <summary>Pose $F8 transitions left-facing crouching to standing while aiming up.</summary>
    StandingTransitionAimUpLeftPose = 0xf8,

    /// <summary>Pose $F9 transitions right-facing crouching to standing while aiming diagonally up.</summary>
    StandingTransitionAimDiagonalUpRightPose = 0xf9,

    /// <summary>Pose $FA transitions left-facing crouching to standing while aiming diagonally up.</summary>
    StandingTransitionAimDiagonalUpLeftPose = 0xfa,

    /// <summary>Pose $FB transitions right-facing crouching to standing while aiming diagonally down.</summary>
    StandingTransitionAimDiagonalDownRightPose = 0xfb,

    /// <summary>Pose $FC transitions left-facing crouching to standing while aiming diagonally down.</summary>
    StandingTransitionAimDiagonalDownLeftPose = 0xfc,

    /// <summary>Pose $27 is ordinary right-facing crouching.</summary>
    CrouchingRightPose = 0x27,

    /// <summary>Pose $28 is ordinary left-facing crouching.</summary>
    CrouchingLeftPose = 0x28,

    /// <summary>Pose $35 is the right-facing standing-to-crouch transition.</summary>
    CrouchingTransitionRightPose = 0x35,

    /// <summary>Pose $36 is the left-facing standing-to-crouch transition.</summary>
    CrouchingTransitionLeftPose = 0x36,

    /// <summary>Pose $3B is the right-facing crouch-to-standing transition.</summary>
    StandingTransitionRightPose = 0x3b,

    /// <summary>Pose $3C is the left-facing crouch-to-standing transition.</summary>
    StandingTransitionLeftPose = 0x3c,

    /// <summary>Pose $4B is the one-frame right neutral-jump transition.</summary>
    NeutralJumpTransitionRightPose = 0x4b,

    /// <summary>Pose $4C is the one-frame left neutral-jump transition.</summary>
    NeutralJumpTransitionLeftPose = 0x4c,

    /// <summary>Pose $4D is the ordinary right-facing neutral jump.</summary>
    NeutralJumpRightPose = 0x4d,

    /// <summary>Pose $4E is the ordinary left-facing neutral jump.</summary>
    NeutralJumpLeftPose = 0x4e,

    /// <summary>Pose $A4 lands facing right after a non-spinning jump or fall.</summary>
    NormalLandingRightPose = 0xa4,

    /// <summary>Pose $A5 lands facing left after a non-spinning jump or fall.</summary>
    NormalLandingLeftPose = 0xa5,

    /// <summary>Pose $A6 lands facing right after a spin or wall jump.</summary>
    SpinLandingRightPose = 0xa6,

    /// <summary>Pose $A7 lands facing left after a spin or wall jump.</summary>
    SpinLandingLeftPose = 0xa7,
}

/// <summary>
/// Byte-compatible pose constants for native tables and WRAM-facing APIs. New typed
/// state should use <see cref="SamusPoseId"/> directly; these aliases isolate legacy
/// byte boundaries without returning the definitions to functional classes.
/// </summary>
public static class SamusPoseIds
{
    public const byte ForwardFacingPowerSuitPose = (byte)SamusPoseId.ForwardFacingPowerSuitPose;
    public const byte ForwardFacingSuitedPose = (byte)SamusPoseId.ForwardFacingSuitedPose;
    public const byte FacingRightNormalPose = (byte)SamusPoseId.FacingRightNormalPose;
    public const byte FacingLeftNormalPose = (byte)SamusPoseId.FacingLeftNormalPose;
    public const byte MovingRightNormalPose = (byte)SamusPoseId.MovingRightNormalPose;
    public const byte MovingLeftNormalPose = (byte)SamusPoseId.MovingLeftNormalPose;
    public const byte MovingRightGunExtendedPose = (byte)SamusPoseId.MovingRightGunExtendedPose;
    public const byte MovingLeftGunExtendedPose = (byte)SamusPoseId.MovingLeftGunExtendedPose;
    public const byte UnusedPose65 = (byte)SamusPoseId.UnusedPose65;
    public const byte UnusedPose66 = (byte)SamusPoseId.UnusedPose66;
    public const byte MoonwalkFacingLeftPose = (byte)SamusPoseId.MoonwalkFacingLeftPose;
    public const byte MoonwalkFacingRightPose = (byte)SamusPoseId.MoonwalkFacingRightPose;
    public const byte MoonwalkAimUpLeftPose = (byte)SamusPoseId.MoonwalkAimUpLeftPose;
    public const byte MoonwalkAimUpRightPose = (byte)SamusPoseId.MoonwalkAimUpRightPose;
    public const byte MoonwalkAimDownLeftPose = (byte)SamusPoseId.MoonwalkAimDownLeftPose;
    public const byte MoonwalkAimDownRightPose = (byte)SamusPoseId.MoonwalkAimDownRightPose;
    public const byte MoonwalkTurnJumpLeftPose = (byte)SamusPoseId.MoonwalkTurnJumpLeftPose;
    public const byte MoonwalkTurnJumpRightPose = (byte)SamusPoseId.MoonwalkTurnJumpRightPose;
    public const byte MoonwalkTurnJumpAimUpLeftPose = (byte)SamusPoseId.MoonwalkTurnJumpAimUpLeftPose;
    public const byte MoonwalkTurnJumpAimUpRightPose = (byte)SamusPoseId.MoonwalkTurnJumpAimUpRightPose;
    public const byte MoonwalkTurnJumpAimDownLeftPose = (byte)SamusPoseId.MoonwalkTurnJumpAimDownLeftPose;
    public const byte MoonwalkTurnJumpAimDownRightPose = (byte)SamusPoseId.MoonwalkTurnJumpAimDownRightPose;
    public const byte ShinesparkWindupRightPose = (byte)SamusPoseId.ShinesparkWindupRightPose;
    public const byte ShinesparkWindupLeftPose = (byte)SamusPoseId.ShinesparkWindupLeftPose;
    public const byte ShinesparkHorizontalRightPose = (byte)SamusPoseId.ShinesparkHorizontalRightPose;
    public const byte ShinesparkHorizontalLeftPose = (byte)SamusPoseId.ShinesparkHorizontalLeftPose;
    public const byte ShinesparkVerticalRightPose = (byte)SamusPoseId.ShinesparkVerticalRightPose;
    public const byte ShinesparkVerticalLeftPose = (byte)SamusPoseId.ShinesparkVerticalLeftPose;
    public const byte ShinesparkDiagonalRightPose = (byte)SamusPoseId.ShinesparkDiagonalRightPose;
    public const byte ShinesparkDiagonalLeftPose = (byte)SamusPoseId.ShinesparkDiagonalLeftPose;
    public const byte CrystalFlashRightPose = (byte)SamusPoseId.CrystalFlashRightPose;
    public const byte CrystalFlashLeftPose = (byte)SamusPoseId.CrystalFlashLeftPose;
    public const byte XrayingStandingRightPose = (byte)SamusPoseId.XrayingStandingRightPose;
    public const byte XrayingStandingLeftPose = (byte)SamusPoseId.XrayingStandingLeftPose;
    public const byte DeathSequenceRightPose = (byte)SamusPoseId.DeathSequenceRightPose;
    public const byte DeathSequenceLeftPose = (byte)SamusPoseId.DeathSequenceLeftPose;
    public const byte XrayingCrouchingRightPose = (byte)SamusPoseId.XrayingCrouchingRightPose;
    public const byte XrayingCrouchingLeftPose = (byte)SamusPoseId.XrayingCrouchingLeftPose;
    public const byte UnusedPoseDb = (byte)SamusPoseId.UnusedPoseDb;
    public const byte UnusedPoseDc = (byte)SamusPoseId.UnusedPoseDc;
    public const byte UnusedPoseDd = (byte)SamusPoseId.UnusedPoseDd;
    public const byte UnusedPoseDe = (byte)SamusPoseId.UnusedPoseDe;
    public const byte UnusedPoseDf = (byte)SamusPoseId.UnusedPoseDf;
    public const byte DrainedCrouchingRightPose = (byte)SamusPoseId.DrainedCrouchingRightPose;
    public const byte DrainedCrouchingLeftPose = (byte)SamusPoseId.DrainedCrouchingLeftPose;
    public const byte DrainedStandingRightPose = (byte)SamusPoseId.DrainedStandingRightPose;
    public const byte DrainedStandingLeftPose = (byte)SamusPoseId.DrainedStandingLeftPose;
    public const byte RanIntoWallRightPose = (byte)SamusPoseId.RanIntoWallRightPose;
    public const byte RanIntoWallLeftPose = (byte)SamusPoseId.RanIntoWallLeftPose;
    public const byte RanIntoWallAimUpRightPose = (byte)SamusPoseId.RanIntoWallAimUpRightPose;
    public const byte RanIntoWallAimUpLeftPose = (byte)SamusPoseId.RanIntoWallAimUpLeftPose;
    public const byte RanIntoWallAimDownRightPose = (byte)SamusPoseId.RanIntoWallAimDownRightPose;
    public const byte RanIntoWallAimDownLeftPose = (byte)SamusPoseId.RanIntoWallAimDownLeftPose;
    public const byte TurningRightToLeftPose = (byte)SamusPoseId.TurningRightToLeftPose;
    public const byte TurningLeftToRightPose = (byte)SamusPoseId.TurningLeftToRightPose;
    public const byte SpinJumpRightPose = (byte)SamusPoseId.SpinJumpRightPose;
    public const byte SpinJumpLeftPose = (byte)SamusPoseId.SpinJumpLeftPose;
    public const byte SpaceJumpRightPose = (byte)SamusPoseId.SpaceJumpRightPose;
    public const byte SpaceJumpLeftPose = (byte)SamusPoseId.SpaceJumpLeftPose;
    public const byte ScrewAttackRightPose = (byte)SamusPoseId.ScrewAttackRightPose;
    public const byte ScrewAttackLeftPose = (byte)SamusPoseId.ScrewAttackLeftPose;
    public const byte TurningRightToLeftJumpPose = (byte)SamusPoseId.TurningRightToLeftJumpPose;
    public const byte TurningLeftToRightJumpPose = (byte)SamusPoseId.TurningLeftToRightJumpPose;
    public const byte WallJumpRightPose = (byte)SamusPoseId.WallJumpRightPose;
    public const byte WallJumpLeftPose = (byte)SamusPoseId.WallJumpLeftPose;
    public const byte GrappleSwingRightPose = (byte)SamusPoseId.GrappleSwingRightPose;
    public const byte GrappleSwingLeftPose = (byte)SamusPoseId.GrappleSwingLeftPose;
    public const byte GrappleStandingRightPose = (byte)SamusPoseId.GrappleStandingRightPose;
    public const byte GrappleStandingLeftPose = (byte)SamusPoseId.GrappleStandingLeftPose;
    public const byte GrappleStandingDownRightPose = (byte)SamusPoseId.GrappleStandingDownRightPose;
    public const byte GrappleStandingDownLeftPose = (byte)SamusPoseId.GrappleStandingDownLeftPose;
    public const byte GrappleCrouchingRightPose = (byte)SamusPoseId.GrappleCrouchingRightPose;
    public const byte GrappleCrouchingLeftPose = (byte)SamusPoseId.GrappleCrouchingLeftPose;
    public const byte GrappleCrouchingDownRightPose = (byte)SamusPoseId.GrappleCrouchingDownRightPose;
    public const byte GrappleCrouchingDownLeftPose = (byte)SamusPoseId.GrappleCrouchingDownLeftPose;
    public const byte GrappleWallContactLeftPose = (byte)SamusPoseId.GrappleWallContactLeftPose;
    public const byte GrappleWallContactRightPose = (byte)SamusPoseId.GrappleWallContactRightPose;
    public const byte DraygonGrabbedNeutralLeftPose = (byte)SamusPoseId.DraygonGrabbedNeutralLeftPose;
    public const byte DraygonGrabbedAimUpLeftPose = (byte)SamusPoseId.DraygonGrabbedAimUpLeftPose;
    public const byte DraygonGrabbedFiringLeftPose = (byte)SamusPoseId.DraygonGrabbedFiringLeftPose;
    public const byte DraygonGrabbedAimDownLeftPose = (byte)SamusPoseId.DraygonGrabbedAimDownLeftPose;
    public const byte DraygonGrabbedMovingLeftPose = (byte)SamusPoseId.DraygonGrabbedMovingLeftPose;
    public const byte DraygonGrabbedNeutralRightPose = (byte)SamusPoseId.DraygonGrabbedNeutralRightPose;
    public const byte DraygonGrabbedAimUpRightPose = (byte)SamusPoseId.DraygonGrabbedAimUpRightPose;
    public const byte DraygonGrabbedFiringRightPose = (byte)SamusPoseId.DraygonGrabbedFiringRightPose;
    public const byte DraygonGrabbedAimDownRightPose = (byte)SamusPoseId.DraygonGrabbedAimDownRightPose;
    public const byte DraygonGrabbedMovingRightPose = (byte)SamusPoseId.DraygonGrabbedMovingRightPose;
    public const byte DamageBoostLeftPose = (byte)SamusPoseId.DamageBoostLeftPose;
    public const byte DamageBoostRightPose = (byte)SamusPoseId.DamageBoostRightPose;
    public const byte KnockbackRightPose = (byte)SamusPoseId.KnockbackRightPose;
    public const byte KnockbackLeftPose = (byte)SamusPoseId.KnockbackLeftPose;
    public const byte TurningRightToLeftFallingPose = (byte)SamusPoseId.TurningRightToLeftFallingPose;
    public const byte TurningLeftToRightFallingPose = (byte)SamusPoseId.TurningLeftToRightFallingPose;
    public const byte TurningRightToLeftJumpAimUpPose = (byte)SamusPoseId.TurningRightToLeftJumpAimUpPose;
    public const byte TurningLeftToRightJumpAimUpPose = (byte)SamusPoseId.TurningLeftToRightJumpAimUpPose;
    public const byte TurningRightToLeftJumpAimDownPose = (byte)SamusPoseId.TurningRightToLeftJumpAimDownPose;
    public const byte TurningLeftToRightJumpAimDownPose = (byte)SamusPoseId.TurningLeftToRightJumpAimDownPose;
    public const byte TurningRightToLeftFallingAimUpPose = (byte)SamusPoseId.TurningRightToLeftFallingAimUpPose;
    public const byte TurningLeftToRightFallingAimUpPose = (byte)SamusPoseId.TurningLeftToRightFallingAimUpPose;
    public const byte TurningRightToLeftFallingAimDownPose = (byte)SamusPoseId.TurningRightToLeftFallingAimDownPose;
    public const byte TurningLeftToRightFallingAimDownPose = (byte)SamusPoseId.TurningLeftToRightFallingAimDownPose;
    public const byte TurningRightToLeftJumpAimDiagonalUpPose = (byte)SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose;
    public const byte TurningLeftToRightJumpAimDiagonalUpPose = (byte)SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose;
    public const byte TurningRightToLeftFallingAimDiagonalUpPose = (byte)SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose;
    public const byte TurningLeftToRightFallingAimDiagonalUpPose = (byte)SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose;
    public const byte MorphBallGroundRightPose = (byte)SamusPoseId.MorphBallGroundRightPose;
    public const byte MorphBallMovingRightPose = (byte)SamusPoseId.MorphBallMovingRightPose;
    public const byte MorphBallMovingLeftPose = (byte)SamusPoseId.MorphBallMovingLeftPose;
    public const byte MorphBallFallingRightPose = (byte)SamusPoseId.MorphBallFallingRightPose;
    public const byte MorphBallFallingLeftPose = (byte)SamusPoseId.MorphBallFallingLeftPose;
    public const byte UnusedKnockbackRightPose = (byte)SamusPoseId.UnusedKnockbackRightPose;
    public const byte UnusedKnockbackLeftPose = (byte)SamusPoseId.UnusedKnockbackLeftPose;
    public const byte MorphingTransitionRightPose = (byte)SamusPoseId.MorphingTransitionRightPose;
    public const byte MorphingTransitionLeftPose = (byte)SamusPoseId.MorphingTransitionLeftPose;
    public const byte UnmorphingTransitionRightPose = (byte)SamusPoseId.UnmorphingTransitionRightPose;
    public const byte UnmorphingTransitionLeftPose = (byte)SamusPoseId.UnmorphingTransitionLeftPose;
    public const byte MorphBallGroundLeftPose = (byte)SamusPoseId.MorphBallGroundLeftPose;
    public const byte SpringBallGroundRightPose = (byte)SamusPoseId.SpringBallGroundRightPose;
    public const byte SpringBallGroundLeftPose = (byte)SamusPoseId.SpringBallGroundLeftPose;
    public const byte SpringBallMovingRightPose = (byte)SamusPoseId.SpringBallMovingRightPose;
    public const byte SpringBallMovingLeftPose = (byte)SamusPoseId.SpringBallMovingLeftPose;
    public const byte SpringBallFallingRightPose = (byte)SamusPoseId.SpringBallFallingRightPose;
    public const byte SpringBallFallingLeftPose = (byte)SamusPoseId.SpringBallFallingLeftPose;
    public const byte SpringBallJumpRightPose = (byte)SamusPoseId.SpringBallJumpRightPose;
    public const byte SpringBallJumpLeftPose = (byte)SamusPoseId.SpringBallJumpLeftPose;
    public const byte FallingRightPose = (byte)SamusPoseId.FallingRightPose;
    public const byte FallingLeftPose = (byte)SamusPoseId.FallingLeftPose;
    public const byte FallingGunExtendedRightPose = (byte)SamusPoseId.FallingGunExtendedRightPose;
    public const byte FallingGunExtendedLeftPose = (byte)SamusPoseId.FallingGunExtendedLeftPose;
    public const byte StandingAimUpRightPose = (byte)SamusPoseId.StandingAimUpRightPose;
    public const byte StandingAimUpLeftPose = (byte)SamusPoseId.StandingAimUpLeftPose;
    public const byte StandingAimDiagonalUpRightPose = (byte)SamusPoseId.StandingAimDiagonalUpRightPose;
    public const byte StandingAimDiagonalUpLeftPose = (byte)SamusPoseId.StandingAimDiagonalUpLeftPose;
    public const byte StandingAimDiagonalDownRightPose = (byte)SamusPoseId.StandingAimDiagonalDownRightPose;
    public const byte StandingAimDiagonalDownLeftPose = (byte)SamusPoseId.StandingAimDiagonalDownLeftPose;
    public const byte RunningAimUpRightPose = (byte)SamusPoseId.RunningAimUpRightPose;
    public const byte RunningAimUpLeftPose = (byte)SamusPoseId.RunningAimUpLeftPose;
    public const byte RunningAimDiagonalUpRightPose = (byte)SamusPoseId.RunningAimDiagonalUpRightPose;
    public const byte RunningAimDiagonalUpLeftPose = (byte)SamusPoseId.RunningAimDiagonalUpLeftPose;
    public const byte RunningAimDiagonalDownRightPose = (byte)SamusPoseId.RunningAimDiagonalDownRightPose;
    public const byte RunningAimDiagonalDownLeftPose = (byte)SamusPoseId.RunningAimDiagonalDownLeftPose;
    public const byte NormalJumpGunExtendedRightPose = (byte)SamusPoseId.NormalJumpGunExtendedRightPose;
    public const byte NormalJumpGunExtendedLeftPose = (byte)SamusPoseId.NormalJumpGunExtendedLeftPose;
    public const byte NormalJumpAimUpRightPose = (byte)SamusPoseId.NormalJumpAimUpRightPose;
    public const byte NormalJumpAimUpLeftPose = (byte)SamusPoseId.NormalJumpAimUpLeftPose;
    public const byte NormalJumpAimDownRightPose = (byte)SamusPoseId.NormalJumpAimDownRightPose;
    public const byte NormalJumpAimDownLeftPose = (byte)SamusPoseId.NormalJumpAimDownLeftPose;
    public const byte NormalJumpForwardRightPose = (byte)SamusPoseId.NormalJumpForwardRightPose;
    public const byte NormalJumpForwardLeftPose = (byte)SamusPoseId.NormalJumpForwardLeftPose;
    public const byte NormalJumpTransitionAimUpRightPose = (byte)SamusPoseId.NormalJumpTransitionAimUpRightPose;
    public const byte NormalJumpTransitionAimUpLeftPose = (byte)SamusPoseId.NormalJumpTransitionAimUpLeftPose;
    public const byte NormalJumpTransitionAimDiagonalUpRightPose = (byte)SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose;
    public const byte NormalJumpTransitionAimDiagonalUpLeftPose = (byte)SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose;
    public const byte NormalJumpTransitionAimDiagonalDownRightPose = (byte)SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose;
    public const byte NormalJumpTransitionAimDiagonalDownLeftPose = (byte)SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose;
    public const byte NormalJumpAimDiagonalUpRightPose = (byte)SamusPoseId.NormalJumpAimDiagonalUpRightPose;
    public const byte NormalJumpAimDiagonalUpLeftPose = (byte)SamusPoseId.NormalJumpAimDiagonalUpLeftPose;
    public const byte NormalJumpAimDiagonalDownRightPose = (byte)SamusPoseId.NormalJumpAimDiagonalDownRightPose;
    public const byte NormalJumpAimDiagonalDownLeftPose = (byte)SamusPoseId.NormalJumpAimDiagonalDownLeftPose;
    public const byte FallingAimUpRightPose = (byte)SamusPoseId.FallingAimUpRightPose;
    public const byte FallingAimUpLeftPose = (byte)SamusPoseId.FallingAimUpLeftPose;
    public const byte FallingAimDownRightPose = (byte)SamusPoseId.FallingAimDownRightPose;
    public const byte FallingAimDownLeftPose = (byte)SamusPoseId.FallingAimDownLeftPose;
    public const byte FallingAimDiagonalUpRightPose = (byte)SamusPoseId.FallingAimDiagonalUpRightPose;
    public const byte FallingAimDiagonalUpLeftPose = (byte)SamusPoseId.FallingAimDiagonalUpLeftPose;
    public const byte FallingAimDiagonalDownRightPose = (byte)SamusPoseId.FallingAimDiagonalDownRightPose;
    public const byte FallingAimDiagonalDownLeftPose = (byte)SamusPoseId.FallingAimDiagonalDownLeftPose;
    public const byte CrouchingAimDiagonalUpRightPose = (byte)SamusPoseId.CrouchingAimDiagonalUpRightPose;
    public const byte CrouchingAimDiagonalUpLeftPose = (byte)SamusPoseId.CrouchingAimDiagonalUpLeftPose;
    public const byte CrouchingAimDiagonalDownRightPose = (byte)SamusPoseId.CrouchingAimDiagonalDownRightPose;
    public const byte CrouchingAimDiagonalDownLeftPose = (byte)SamusPoseId.CrouchingAimDiagonalDownLeftPose;
    public const byte CrouchingAimUpRightPose = (byte)SamusPoseId.CrouchingAimUpRightPose;
    public const byte CrouchingAimUpLeftPose = (byte)SamusPoseId.CrouchingAimUpLeftPose;
    public const byte TurningRightToLeftCrouchingPose = (byte)SamusPoseId.TurningRightToLeftCrouchingPose;
    public const byte TurningLeftToRightCrouchingPose = (byte)SamusPoseId.TurningLeftToRightCrouchingPose;
    public const byte TurningRightToLeftAimUpPose = (byte)SamusPoseId.TurningRightToLeftAimUpPose;
    public const byte TurningLeftToRightAimUpPose = (byte)SamusPoseId.TurningLeftToRightAimUpPose;
    public const byte TurningRightToLeftAimDiagonalDownPose = (byte)SamusPoseId.TurningRightToLeftAimDiagonalDownPose;
    public const byte TurningLeftToRightAimDiagonalDownPose = (byte)SamusPoseId.TurningLeftToRightAimDiagonalDownPose;
    public const byte TurningRightToLeftAimDiagonalUpPose = (byte)SamusPoseId.TurningRightToLeftAimDiagonalUpPose;
    public const byte TurningLeftToRightAimDiagonalUpPose = (byte)SamusPoseId.TurningLeftToRightAimDiagonalUpPose;
    public const byte TurningRightToLeftCrouchingAimUpPose = (byte)SamusPoseId.TurningRightToLeftCrouchingAimUpPose;
    public const byte TurningLeftToRightCrouchingAimUpPose = (byte)SamusPoseId.TurningLeftToRightCrouchingAimUpPose;
    public const byte TurningRightToLeftCrouchingAimDiagonalDownPose = (byte)SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose;
    public const byte TurningLeftToRightCrouchingAimDiagonalDownPose = (byte)SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose;
    public const byte TurningRightToLeftCrouchingAimDiagonalUpPose = (byte)SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose;
    public const byte TurningLeftToRightCrouchingAimDiagonalUpPose = (byte)SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose;
    public const byte LandingAimUpRightPose = (byte)SamusPoseId.LandingAimUpRightPose;
    public const byte LandingAimUpLeftPose = (byte)SamusPoseId.LandingAimUpLeftPose;
    public const byte LandingAimDiagonalUpRightPose = (byte)SamusPoseId.LandingAimDiagonalUpRightPose;
    public const byte LandingAimDiagonalUpLeftPose = (byte)SamusPoseId.LandingAimDiagonalUpLeftPose;
    public const byte LandingAimDiagonalDownRightPose = (byte)SamusPoseId.LandingAimDiagonalDownRightPose;
    public const byte LandingAimDiagonalDownLeftPose = (byte)SamusPoseId.LandingAimDiagonalDownLeftPose;
    public const byte FiringLandingRightPose = (byte)SamusPoseId.FiringLandingRightPose;
    public const byte FiringLandingLeftPose = (byte)SamusPoseId.FiringLandingLeftPose;
    public const byte CrouchingTransitionAimUpRightPose = (byte)SamusPoseId.CrouchingTransitionAimUpRightPose;
    public const byte CrouchingTransitionAimUpLeftPose = (byte)SamusPoseId.CrouchingTransitionAimUpLeftPose;
    public const byte CrouchingTransitionAimDiagonalUpRightPose = (byte)SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose;
    public const byte CrouchingTransitionAimDiagonalUpLeftPose = (byte)SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose;
    public const byte CrouchingTransitionAimDiagonalDownRightPose = (byte)SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose;
    public const byte CrouchingTransitionAimDiagonalDownLeftPose = (byte)SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose;
    public const byte StandingTransitionAimUpRightPose = (byte)SamusPoseId.StandingTransitionAimUpRightPose;
    public const byte StandingTransitionAimUpLeftPose = (byte)SamusPoseId.StandingTransitionAimUpLeftPose;
    public const byte StandingTransitionAimDiagonalUpRightPose = (byte)SamusPoseId.StandingTransitionAimDiagonalUpRightPose;
    public const byte StandingTransitionAimDiagonalUpLeftPose = (byte)SamusPoseId.StandingTransitionAimDiagonalUpLeftPose;
    public const byte StandingTransitionAimDiagonalDownRightPose = (byte)SamusPoseId.StandingTransitionAimDiagonalDownRightPose;
    public const byte StandingTransitionAimDiagonalDownLeftPose = (byte)SamusPoseId.StandingTransitionAimDiagonalDownLeftPose;
    public const byte CrouchingRightPose = (byte)SamusPoseId.CrouchingRightPose;
    public const byte CrouchingLeftPose = (byte)SamusPoseId.CrouchingLeftPose;
    public const byte CrouchingTransitionRightPose = (byte)SamusPoseId.CrouchingTransitionRightPose;
    public const byte CrouchingTransitionLeftPose = (byte)SamusPoseId.CrouchingTransitionLeftPose;
    public const byte StandingTransitionRightPose = (byte)SamusPoseId.StandingTransitionRightPose;
    public const byte StandingTransitionLeftPose = (byte)SamusPoseId.StandingTransitionLeftPose;
    public const byte NeutralJumpTransitionRightPose = (byte)SamusPoseId.NeutralJumpTransitionRightPose;
    public const byte NeutralJumpTransitionLeftPose = (byte)SamusPoseId.NeutralJumpTransitionLeftPose;
    public const byte NeutralJumpRightPose = (byte)SamusPoseId.NeutralJumpRightPose;
    public const byte NeutralJumpLeftPose = (byte)SamusPoseId.NeutralJumpLeftPose;
    public const byte NormalLandingRightPose = (byte)SamusPoseId.NormalLandingRightPose;
    public const byte NormalLandingLeftPose = (byte)SamusPoseId.NormalLandingLeftPose;
    public const byte SpinLandingRightPose = (byte)SamusPoseId.SpinLandingRightPose;
    public const byte SpinLandingLeftPose = (byte)SamusPoseId.SpinLandingLeftPose;
}
