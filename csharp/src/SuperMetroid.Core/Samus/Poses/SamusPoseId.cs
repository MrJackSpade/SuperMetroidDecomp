namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Exclusive cartridge pose identifiers. Undefined byte values remain representable through an explicit cast for corrupt-state diagnostics.
/// </summary>
public enum SamusPoseId
{
    /// <summary>
    /// Pose `$00`: the power-suit body viewed from the front during the intro, elevators,
    /// save-station appearance, and other controller-locked sequences.
    /// </summary>
    ForwardFacingPowerSuitPose = 0,

    /// <summary>
    /// Pose `$9B`: the Varia/Gravity-suit counterpart of pose `$00`. Both use movement type
    /// zero, but only the power-suit record needs bank `$90`'s extra chest-cover OBJ.
    /// </summary>
    ForwardFacingSuitedPose = 155,

    /// <summary>Pose $01 is “facing right - normal” in the cartridge table.</summary>
    FacingRightNormalPose = 1,

    /// <summary>Pose $02 is “facing left - normal” in the cartridge table.</summary>
    FacingLeftNormalPose = 2,

    /// <summary>Pose $09 is “moving right - not aiming” in the cartridge table.</summary>
    MovingRightNormalPose = 9,

    /// <summary>Pose $0A is “moving left - not aiming” in the cartridge table.</summary>
    MovingLeftNormalPose = 10,

    /// <summary>
    /// Pose `$0B` is the right-moving horizontal-fire body. It is a genuine movement-type-one
    /// record, shares the ordinary ten-frame running delay stream, and is selected when Shot
    /// remains held with Right after the projectile direction has been established.
    /// </summary>
    MovingRightGunExtendedPose = 11,

    /// <summary>Pose `$0C` is the left-moving mirror of <see cref="MovingRightGunExtendedPose"/>.</summary>
    MovingLeftGunExtendedPose = 12,

    /// <summary>Unused movement-type-$0D right-facing pose definition `$65`.</summary>
    UnusedPose65 = 101,

    /// <summary>Unused movement-type-$0D left-facing pose definition `$66`.</summary>
    UnusedPose66 = 102,

    /// <summary>
    /// Pose $49 visually faces left while moonwalking right. Its pose-X byte is deliberately
    /// eight: native horizontal movement follows travel direction, not the artwork's facing.
    /// </summary>
    MoonwalkFacingLeftPose = 73,

    /// <summary>Pose $4A visually faces right while moonwalking left.</summary>
    MoonwalkFacingRightPose = 74,

    /// <summary>Pose `$75`: left-facing/right-moving moonwalk aimed diagonally up-left.</summary>
    MoonwalkAimUpLeftPose = 117,

    /// <summary>Pose `$76`: right-facing/left-moving mirror of pose `$75`.</summary>
    MoonwalkAimUpRightPose = 118,

    /// <summary>Pose `$77`: left-facing/right-moving moonwalk aimed diagonally down-left.</summary>
    MoonwalkAimDownLeftPose = 119,

    /// <summary>Pose `$78`: right-facing/left-moving mirror of pose `$77`.</summary>
    MoonwalkAimDownRightPose = 120,

    /// <summary>
    /// Pose `$BF`: right-facing moonwalk art turning/jumping toward the left. Despite the
    /// name in the disassembly, this is still grounded movement type `$0E` until its input
    /// table or terminal `$F8,$1A` animation command installs the actual spin jump.
    /// </summary>
    MoonwalkTurnJumpLeftPose = 191,

    /// <summary>Pose `$C0`: left-facing mirror of <see cref="MoonwalkTurnJumpLeftPose"/>.</summary>
    MoonwalkTurnJumpRightPose = 192,

    /// <summary>Pose `$C1`: aimed-up moonwalk turn whose terminal jump faces left.</summary>
    MoonwalkTurnJumpAimUpLeftPose = 193,

    /// <summary>Pose `$C2`: aimed-up moonwalk turn whose terminal jump faces right.</summary>
    MoonwalkTurnJumpAimUpRightPose = 194,

    /// <summary>Pose `$C3`: aimed-down moonwalk turn whose terminal jump faces left.</summary>
    MoonwalkTurnJumpAimDownLeftPose = 195,

    /// <summary>Pose `$C4`: aimed-down moonwalk turn whose terminal jump faces right.</summary>
    MoonwalkTurnJumpAimDownRightPose = 196,

    /// <summary>Pose `$C7`: right-facing stored-shine windup body.</summary>
    ShinesparkWindupRightPose = 199,

    /// <summary>Pose `$C8`: left-facing stored-shine windup body.</summary>
    ShinesparkWindupLeftPose = 200,

    /// <summary>Pose `$C9`: horizontal shinespark travelling right.</summary>
    ShinesparkHorizontalRightPose = 201,

    /// <summary>Pose `$CA`: horizontal shinespark travelling left.</summary>
    ShinesparkHorizontalLeftPose = 202,

    /// <summary>Pose `$CB`: vertical shinespark using right-facing metadata.</summary>
    ShinesparkVerticalRightPose = 203,

    /// <summary>Pose `$CC`: vertical shinespark using left-facing metadata.</summary>
    ShinesparkVerticalLeftPose = 204,

    /// <summary>Pose `$CD`: diagonal up-right shinespark.</summary>
    ShinesparkDiagonalRightPose = 205,

    /// <summary>Pose `$CE`: diagonal up-left shinespark.</summary>
    ShinesparkDiagonalLeftPose = 206,

    /// <summary>Pose `$D3`: right-facing Crystal Flash body and delay program.</summary>
    CrystalFlashRightPose = 211,

    /// <summary>Pose `$D4`: left-facing Crystal Flash mirror.</summary>
    CrystalFlashLeftPose = 212,

    /// <summary>Pose `$D5`: right-facing standing X-ray body.</summary>
    XrayingStandingRightPose = 213,

    /// <summary>Pose `$D6`: left-facing standing X-ray body.</summary>
    XrayingStandingLeftPose = 214,

    /// <summary>Pose `$D7`: right-facing fatal-damage / crystal-flash-ending body.</summary>
    DeathSequenceRightPose = 215,

    /// <summary>Pose `$D8`: left-facing fatal-damage / crystal-flash-ending mirror.</summary>
    DeathSequenceLeftPose = 216,

    /// <summary>Pose `$D9`: right-facing crouching X-ray body.</summary>
    XrayingCrouchingRightPose = 217,

    /// <summary>Pose `$DA`: left-facing crouching X-ray body.</summary>
    XrayingCrouchingLeftPose = 218,

    /// <summary>Unused compact right-facing transition pose definition `$DB`.</summary>
    UnusedPoseDb = 219,

    /// <summary>Unused compact left-facing transition pose definition `$DC`.</summary>
    UnusedPoseDc = 220,

    /// <summary>Unused standing-radius right-facing transition pose definition `$DD`.</summary>
    UnusedPoseDd = 221,

    /// <summary>Unused standing-radius left-facing transition pose definition `$DE`.</summary>
    UnusedPoseDe = 222,

    /// <summary>Unused Draygon-owned compact pose definition `$DF`.</summary>
    UnusedPoseDf = 223,

    /// <summary>Pose `$E8`: right-facing drained crouch/fall animation.</summary>
    DrainedCrouchingRightPose = 232,

    /// <summary>Pose `$E9`: left-facing drained crouch/fall animation.</summary>
    DrainedCrouchingLeftPose = 233,

    /// <summary>Pose `$EA`: right-facing drained standing animation.</summary>
    DrainedStandingRightPose = 234,

    /// <summary>Pose `$EB`: left-facing drained standing animation.</summary>
    DrainedStandingLeftPose = 235,

    /// <summary>Pose `$89`: facing right after forward running collides with a wall.</summary>
    RanIntoWallRightPose = 137,

    /// <summary>Pose `$8A`: facing-left mirror of pose `$89`.</summary>
    RanIntoWallLeftPose = 138,

    /// <summary>Pose `$CF`: right-facing wall-stop art aimed diagonally up-right.</summary>
    RanIntoWallAimUpRightPose = 207,

    /// <summary>Pose `$D0`: left-facing wall-stop art aimed diagonally up-left.</summary>
    RanIntoWallAimUpLeftPose = 208,

    /// <summary>Pose `$D1`: right-facing wall-stop art aimed diagonally down-right.</summary>
    RanIntoWallAimDownRightPose = 209,

    /// <summary>Pose `$D2`: left-facing wall-stop art aimed diagonally down-left.</summary>
    RanIntoWallAimDownLeftPose = 210,

    /// <summary>Pose $25 turns a right-facing grounded Samus toward the left.</summary>
    TurningRightToLeftPose = 37,

    /// <summary>Pose $26 turns a left-facing grounded Samus toward the right.</summary>
    TurningLeftToRightPose = 38,

    /// <summary>Pose $19 is the ordinary right-facing spin jump.</summary>
    SpinJumpRightPose = 25,

    /// <summary>Pose $1A is the ordinary left-facing spin jump.</summary>
    SpinJumpLeftPose = 26,

    /// <summary>
    /// Pose `$1B` is the right-facing Space Jump body. The equipment-aware movement-type
    /// initializer at `$91:F624` substitutes this for the transition table's ordinary
    /// `$19` target when Space Jump is equipped and Screw Attack is not.
    /// </summary>
    SpaceJumpRightPose = 27,

    /// <summary>Pose `$1C` is the left-facing mirror of <see cref="SpaceJumpRightPose"/>.</summary>
    SpaceJumpLeftPose = 28,

    /// <summary>
    /// Pose `$81` is the right-facing Screw Attack body. Screw Attack bit `$0008` has
    /// priority over Space Jump bit `$0200` in the native spin-pose selector.
    /// </summary>
    ScrewAttackRightPose = 129,

    /// <summary>Pose `$82` is the left-facing mirror of <see cref="ScrewAttackRightPose"/>.</summary>
    ScrewAttackLeftPose = 130,

    /// <summary>Pose $2F turns a right-facing normal jump toward the left.</summary>
    TurningRightToLeftJumpPose = 47,

    /// <summary>Pose $30 turns a left-facing normal jump toward the right.</summary>
    TurningLeftToRightJumpPose = 48,

    /// <summary>Pose $83 is the right-facing wall-jump launch animation.</summary>
    WallJumpRightPose = 131,

    /// <summary>Pose $84 is the left-facing wall-jump launch animation.</summary>
    WallJumpLeftPose = 132,

    /// <summary>Pose $B2 is the right-facing airborne grapple-swing body.</summary>
    GrappleSwingRightPose = 178,

    /// <summary>Pose $B3 is the left-facing airborne grapple-swing body.</summary>
    GrappleSwingLeftPose = 179,

    /// <summary>Pose $A8 is the standing right-facing horizontal grapple lock.</summary>
    GrappleStandingRightPose = 168,

    /// <summary>Pose $A9 is the standing left-facing horizontal grapple lock.</summary>
    GrappleStandingLeftPose = 169,

    /// <summary>Pose $AA is the standing right-facing down-right grapple lock.</summary>
    GrappleStandingDownRightPose = 170,

    /// <summary>Pose $AB is the standing left-facing down-left grapple lock.</summary>
    GrappleStandingDownLeftPose = 171,

    /// <summary>Pose $B4 is the crouched right-facing horizontal grapple lock.</summary>
    GrappleCrouchingRightPose = 180,

    /// <summary>Pose $B5 is the crouched left-facing horizontal grapple lock.</summary>
    GrappleCrouchingLeftPose = 181,

    /// <summary>Pose $B6 is the crouched right-facing down-right grapple lock.</summary>
    GrappleCrouchingDownRightPose = 182,

    /// <summary>Pose $B7 is the crouched left-facing down-left grapple lock.</summary>
    GrappleCrouchingDownLeftPose = 183,

    /// <summary>Pose $B8 is the left-wall grapple-jump contact body.</summary>
    GrappleWallContactLeftPose = 184,

    /// <summary>Pose $B9 is the right-wall grapple-jump contact body.</summary>
    GrappleWallContactRightPose = 185,

    /// <summary>Pose `$BA`: left-facing neutral body held in Draygon's claws.</summary>
    DraygonGrabbedNeutralLeftPose = 186,

    /// <summary>Pose `$BB`: left-facing Draygon-held body aiming diagonally upward.</summary>
    DraygonGrabbedAimUpLeftPose = 187,

    /// <summary>Pose `$BC`: left-facing Draygon-held firing body.</summary>
    DraygonGrabbedFiringLeftPose = 188,

    /// <summary>Pose `$BD`: left-facing Draygon-held body aiming diagonally downward.</summary>
    DraygonGrabbedAimDownLeftPose = 189,

    /// <summary>Pose `$BE`: left-facing six-frame struggling animation while held by Draygon.</summary>
    DraygonGrabbedMovingLeftPose = 190,

    /// <summary>Pose `$EC`: right-facing neutral body held in Draygon's claws.</summary>
    DraygonGrabbedNeutralRightPose = 236,

    /// <summary>Pose `$ED`: right-facing Draygon-held body aiming diagonally upward.</summary>
    DraygonGrabbedAimUpRightPose = 237,

    /// <summary>Pose `$EE`: right-facing Draygon-held firing body.</summary>
    DraygonGrabbedFiringRightPose = 238,

    /// <summary>Pose `$EF`: right-facing Draygon-held body aiming diagonally downward.</summary>
    DraygonGrabbedAimDownRightPose = 239,

    /// <summary>Pose `$F0`: right-facing six-frame struggling animation while held by Draygon.</summary>
    DraygonGrabbedMovingRightPose = 240,

    /// <summary>
    /// Pose $4F is the visually left-facing damage boost; its X-direction byte is the
    /// deliberately reversed value eight that drives the boost to the right.
    /// </summary>
    DamageBoostLeftPose = 79,

    /// <summary>
    /// Pose $50 is the visually right-facing damage boost; its X-direction byte is the
    /// deliberately reversed value four that drives the boost to the left.
    /// </summary>
    DamageBoostRightPose = 80,

    /// <summary>Pose $53 is the right-facing normal knockback body.</summary>
    KnockbackRightPose = 83,

    /// <summary>Pose $54 is the left-facing normal knockback body.</summary>
    KnockbackLeftPose = 84,

    /// <summary>Pose $87 turns a right-facing fall toward the left.</summary>
    TurningRightToLeftFallingPose = 135,

    /// <summary>Pose $88 turns a left-facing fall toward the right.</summary>
    TurningLeftToRightFallingPose = 136,

    /// <summary>Pose $8F turns right-to-left in a normal jump while retaining straight-up aim.</summary>
    TurningRightToLeftJumpAimUpPose = 143,
    /// <summary>Pose $90 turns left-to-right in a normal jump while retaining straight-up aim.</summary>
    TurningLeftToRightJumpAimUpPose = 144,
    /// <summary>Pose $91 turns right-to-left in a normal jump while retaining downward or down-right aim.</summary>
    TurningRightToLeftJumpAimDownPose = 145,
    /// <summary>Pose $92 turns left-to-right in a normal jump while retaining downward or down-left aim.</summary>
    TurningLeftToRightJumpAimDownPose = 146,
    /// <summary>Pose $93 turns right-to-left during a fall while retaining straight-up aim.</summary>
    TurningRightToLeftFallingAimUpPose = 147,
    /// <summary>Pose $94 turns left-to-right during a fall while retaining straight-up aim.</summary>
    TurningLeftToRightFallingAimUpPose = 148,
    /// <summary>Pose $95 turns right-to-left during a fall while retaining downward or down-right aim.</summary>
    TurningRightToLeftFallingAimDownPose = 149,
    /// <summary>Pose $96 turns left-to-right during a fall while retaining downward or down-left aim.</summary>
    TurningLeftToRightFallingAimDownPose = 150,
    /// <summary>Pose $9E turns right-to-left in a normal jump while retaining diagonal-up aim.</summary>
    TurningRightToLeftJumpAimDiagonalUpPose = 158,
    /// <summary>Pose $9F turns left-to-right in a normal jump while retaining diagonal-up aim.</summary>
    TurningLeftToRightJumpAimDiagonalUpPose = 159,
    /// <summary>Pose $A0 turns right-to-left during a fall while retaining diagonal-up aim.</summary>
    TurningRightToLeftFallingAimDiagonalUpPose = 160,
    /// <summary>Pose $A1 turns left-to-right during a fall while retaining diagonal-up aim.</summary>
    TurningLeftToRightFallingAimDiagonalUpPose = 161,

    /// <summary>Pose $1D is the stationary right-facing ordinary morph ball on the ground.</summary>
    MorphBallGroundRightPose = 29,

    /// <summary>Pose $1E is the ordinary morph ball rolling to the right on the ground.</summary>
    MorphBallMovingRightPose = 30,

    /// <summary>Pose $1F is the ordinary morph ball rolling to the left on the ground.</summary>
    MorphBallMovingLeftPose = 31,

    /// <summary>Pose $31 is the right-facing ordinary morph ball in the air.</summary>
    MorphBallFallingRightPose = 49,

    /// <summary>Pose $32 is the left-facing ordinary morph ball in the air.</summary>
    MorphBallFallingLeftPose = 50,

    /// <summary>
    /// Pose `$33` is the unused right-facing body selected only by movement type `$07`'s
    /// otherwise-unused knockback transition at `$90:DF1D`. Naming it keeps the exhaustive
    /// native pointer table executable without pretending the retail game reaches it.
    /// </summary>
    UnusedKnockbackRightPose = 51,

    /// <summary>Left-facing partner of <see cref="UnusedKnockbackRightPose"/> at pose `$34`.</summary>
    UnusedKnockbackLeftPose = 52,

    /// <summary>Pose $37 is the right-facing crouch-to-morph transition.</summary>
    MorphingTransitionRightPose = 55,

    /// <summary>Pose $38 is the left-facing crouch-to-morph transition.</summary>
    MorphingTransitionLeftPose = 56,

    /// <summary>Pose $3D is the right-facing morph-to-crouch transition.</summary>
    UnmorphingTransitionRightPose = 61,

    /// <summary>Pose $3E is the left-facing morph-to-crouch transition.</summary>
    UnmorphingTransitionLeftPose = 62,

    /// <summary>Pose $41 is the stationary left-facing ordinary morph ball on the ground.</summary>
    MorphBallGroundLeftPose = 65,

    /// <summary>Pose $79 is the stationary right-facing Spring Ball on the ground.</summary>
    SpringBallGroundRightPose = 121,

    /// <summary>Pose $7A is the stationary left-facing Spring Ball on the ground.</summary>
    SpringBallGroundLeftPose = 122,

    /// <summary>Pose $7B is the Spring Ball moving right on the ground.</summary>
    SpringBallMovingRightPose = 123,

    /// <summary>Pose $7C is the Spring Ball moving left on the ground.</summary>
    SpringBallMovingLeftPose = 124,

    /// <summary>Pose $7D is the right-facing Spring Ball falling or bouncing.</summary>
    SpringBallFallingRightPose = 125,

    /// <summary>Pose $7E is the left-facing Spring Ball falling or bouncing.</summary>
    SpringBallFallingLeftPose = 126,

    /// <summary>Pose $7F is the right-facing Spring Ball in its powered jump.</summary>
    SpringBallJumpRightPose = 127,

    /// <summary>Pose $80 is the left-facing Spring Ball in its powered jump.</summary>
    SpringBallJumpLeftPose = 128,

    /// <summary>Pose $29 is the unaimed right-facing falling pose.</summary>
    FallingRightPose = 41,

    /// <summary>Pose $2A is the unaimed left-facing falling pose.</summary>
    FallingLeftPose = 42,

    /// <summary>
    /// Pose `$67` is the right-facing falling body with the cannon held horizontally after
    /// firing. Its `$91:B353` delay stream has the same terminal-velocity split as `$29`.
    /// </summary>
    FallingGunExtendedRightPose = 103,

    /// <summary>Pose `$68` is the left-facing mirror of <see cref="FallingGunExtendedRightPose"/>.</summary>
    FallingGunExtendedLeftPose = 104,

    /// <summary>Pose $03 is stationary, facing right, with the arm cannon aimed straight up.</summary>
    StandingAimUpRightPose = 3,

    /// <summary>Pose $04 is stationary, facing left, with the arm cannon aimed straight up.</summary>
    StandingAimUpLeftPose = 4,

    /// <summary>Pose $05 is stationary, facing right, and aiming diagonally up-right.</summary>
    StandingAimDiagonalUpRightPose = 5,

    /// <summary>Pose $06 is stationary, facing left, and aiming diagonally up-left.</summary>
    StandingAimDiagonalUpLeftPose = 6,

    /// <summary>Pose $07 is stationary, facing right, and aiming diagonally down-right.</summary>
    StandingAimDiagonalDownRightPose = 7,

    /// <summary>Pose $08 is stationary, facing left, and aiming diagonally down-left.</summary>
    StandingAimDiagonalDownLeftPose = 8,

    /// <summary>Pose $0D is the unused right-moving straight-up aim record.</summary>
    RunningAimUpRightPose = 13,

    /// <summary>Pose $0E is the unused left-moving straight-up aim record.</summary>
    RunningAimUpLeftPose = 14,

    /// <summary>Pose $0F moves right while aiming diagonally up-right.</summary>
    RunningAimDiagonalUpRightPose = 15,

    /// <summary>Pose $10 moves left while aiming diagonally up-left.</summary>
    RunningAimDiagonalUpLeftPose = 16,

    /// <summary>Pose $11 moves right while aiming diagonally down-right.</summary>
    RunningAimDiagonalDownRightPose = 17,

    /// <summary>Pose $12 moves left while aiming diagonally down-left.</summary>
    RunningAimDiagonalDownLeftPose = 18,

    /// <summary>
    /// Pose `$13` is a stationary right-facing normal jump with the cannon extended. Unlike
    /// `$4D`, its pose table deliberately stores no no-input fallback, so the firing body can
    /// remain active until another input-table record or landing replaces it.
    /// </summary>
    NormalJumpGunExtendedRightPose = 19,

    /// <summary>Pose `$14` is the left-facing mirror of <see cref="NormalJumpGunExtendedRightPose"/>.</summary>
    NormalJumpGunExtendedLeftPose = 20,

    /// <summary>Pose $15 is a right-facing normal jump aimed straight up.</summary>
    NormalJumpAimUpRightPose = 21,

    /// <summary>Pose $16 is a left-facing normal jump aimed straight up.</summary>
    NormalJumpAimUpLeftPose = 22,

    /// <summary>Pose $17 is the compact right-facing normal jump aimed straight down.</summary>
    NormalJumpAimDownRightPose = 23,

    /// <summary>Pose $18 is the compact left-facing normal jump aimed straight down.</summary>
    NormalJumpAimDownLeftPose = 24,

    /// <summary>Pose $51 is a right-facing normal jump using the moving-forward art.</summary>
    NormalJumpForwardRightPose = 81,

    /// <summary>Pose $52 is a left-facing normal jump using the moving-forward art.</summary>
    NormalJumpForwardLeftPose = 82,

    /// <summary>Pose $55 is the right-facing jump transition aimed straight up.</summary>
    NormalJumpTransitionAimUpRightPose = 85,

    /// <summary>Pose $56 is the left-facing jump transition aimed straight up.</summary>
    NormalJumpTransitionAimUpLeftPose = 86,

    /// <summary>Pose $57 is the right-facing jump transition aimed diagonally up.</summary>
    NormalJumpTransitionAimDiagonalUpRightPose = 87,

    /// <summary>Pose $58 is the left-facing jump transition aimed diagonally up.</summary>
    NormalJumpTransitionAimDiagonalUpLeftPose = 88,

    /// <summary>Pose $59 is the right-facing jump transition aimed diagonally down.</summary>
    NormalJumpTransitionAimDiagonalDownRightPose = 89,

    /// <summary>Pose $5A is the left-facing jump transition aimed diagonally down.</summary>
    NormalJumpTransitionAimDiagonalDownLeftPose = 90,

    /// <summary>Pose $69 is a right-facing normal jump aimed diagonally up.</summary>
    NormalJumpAimDiagonalUpRightPose = 105,

    /// <summary>Pose $6A is a left-facing normal jump aimed diagonally up.</summary>
    NormalJumpAimDiagonalUpLeftPose = 106,

    /// <summary>Pose $6B is a right-facing normal jump aimed diagonally down.</summary>
    NormalJumpAimDiagonalDownRightPose = 107,

    /// <summary>Pose $6C is a left-facing normal jump aimed diagonally down.</summary>
    NormalJumpAimDiagonalDownLeftPose = 108,

    /// <summary>Pose $2B is right-facing falling aimed straight up.</summary>
    FallingAimUpRightPose = 43,

    /// <summary>Pose $2C is left-facing falling aimed straight up.</summary>
    FallingAimUpLeftPose = 44,

    /// <summary>Pose $2D is the compact right-facing fall aimed straight down.</summary>
    FallingAimDownRightPose = 45,

    /// <summary>Pose $2E is the compact left-facing fall aimed straight down.</summary>
    FallingAimDownLeftPose = 46,

    /// <summary>Pose $6D is right-facing falling aimed diagonally up.</summary>
    FallingAimDiagonalUpRightPose = 109,

    /// <summary>Pose $6E is left-facing falling aimed diagonally up.</summary>
    FallingAimDiagonalUpLeftPose = 110,

    /// <summary>Pose $6F is right-facing falling aimed diagonally down.</summary>
    FallingAimDiagonalDownRightPose = 111,

    /// <summary>Pose $70 is left-facing falling aimed diagonally down.</summary>
    FallingAimDiagonalDownLeftPose = 112,

    /// <summary>Pose $71 is right-facing crouching aimed diagonally up.</summary>
    CrouchingAimDiagonalUpRightPose = 113,

    /// <summary>Pose $72 is left-facing crouching aimed diagonally up.</summary>
    CrouchingAimDiagonalUpLeftPose = 114,

    /// <summary>Pose $73 is right-facing crouching aimed diagonally down.</summary>
    CrouchingAimDiagonalDownRightPose = 115,

    /// <summary>Pose $74 is left-facing crouching aimed diagonally down.</summary>
    CrouchingAimDiagonalDownLeftPose = 116,

    /// <summary>Pose $85 is right-facing crouching aimed straight up.</summary>
    CrouchingAimUpRightPose = 133,

    /// <summary>Pose $86 is left-facing crouching aimed straight up.</summary>
    CrouchingAimUpLeftPose = 134,

    /// <summary>Pose $43 turns right-to-left while crouched without retaining aim.</summary>
    TurningRightToLeftCrouchingPose = 67,

    /// <summary>Pose $44 turns left-to-right while crouched without retaining aim.</summary>
    TurningLeftToRightCrouchingPose = 68,

    /// <summary>Pose $8B turns right-to-left on ground while preserving straight-up aim.</summary>
    TurningRightToLeftAimUpPose = 139,

    /// <summary>Pose $8C turns left-to-right on ground while preserving straight-up aim.</summary>
    TurningLeftToRightAimUpPose = 140,

    /// <summary>Pose $8D turns right-to-left on ground while preserving diagonal-down aim.</summary>
    TurningRightToLeftAimDiagonalDownPose = 141,

    /// <summary>Pose $8E turns left-to-right on ground while preserving diagonal-down aim.</summary>
    TurningLeftToRightAimDiagonalDownPose = 142,

    /// <summary>Pose $9C turns right-to-left on ground while preserving diagonal-up aim.</summary>
    TurningRightToLeftAimDiagonalUpPose = 156,

    /// <summary>Pose $9D turns left-to-right on ground while preserving diagonal-up aim.</summary>
    TurningLeftToRightAimDiagonalUpPose = 157,

    /// <summary>Pose $97 turns right-to-left while crouched and preserving straight-up aim.</summary>
    TurningRightToLeftCrouchingAimUpPose = 151,

    /// <summary>Pose $98 turns left-to-right while crouched and preserving straight-up aim.</summary>
    TurningLeftToRightCrouchingAimUpPose = 152,

    /// <summary>Pose $99 turns right-to-left while crouched and preserving diagonal-down aim.</summary>
    TurningRightToLeftCrouchingAimDiagonalDownPose = 153,

    /// <summary>Pose $9A turns left-to-right while crouched and preserving diagonal-down aim.</summary>
    TurningLeftToRightCrouchingAimDiagonalDownPose = 154,

    /// <summary>Pose $A2 turns right-to-left while crouched and preserving diagonal-up aim.</summary>
    TurningRightToLeftCrouchingAimDiagonalUpPose = 162,

    /// <summary>Pose $A3 turns left-to-right while crouched and preserving diagonal-up aim.</summary>
    TurningLeftToRightCrouchingAimDiagonalUpPose = 163,

    /// <summary>Pose $E0 is a right-facing normal-jump landing aimed straight up.</summary>
    LandingAimUpRightPose = 224,

    /// <summary>Pose $E1 is a left-facing normal-jump landing aimed straight up.</summary>
    LandingAimUpLeftPose = 225,

    /// <summary>Pose $E2 is a right-facing normal-jump landing aimed diagonally up.</summary>
    LandingAimDiagonalUpRightPose = 226,

    /// <summary>Pose $E3 is a left-facing normal-jump landing aimed diagonally up.</summary>
    LandingAimDiagonalUpLeftPose = 227,

    /// <summary>Pose $E4 is a right-facing normal-jump landing aimed diagonally down.</summary>
    LandingAimDiagonalDownRightPose = 228,

    /// <summary>Pose $E5 is a left-facing normal-jump landing aimed diagonally down.</summary>
    LandingAimDiagonalDownLeftPose = 229,

    /// <summary>
    /// Pose `$E6` is the right-facing horizontal-fire landing selected by `$91:E95D` only
    /// when the current shot direction is two and the Shot binding is still held at impact.
    /// Its terminal `$F8,$01` command returns to ordinary standing after the landing frames.
    /// </summary>
    FiringLandingRightPose = 230,

    /// <summary>Pose `$E7` is the left-facing mirror of <see cref="FiringLandingRightPose"/>.</summary>
    FiringLandingLeftPose = 231,

    /// <summary>Pose $F1 transitions right-facing standing to crouching while aiming up.</summary>
    CrouchingTransitionAimUpRightPose = 241,

    /// <summary>Pose $F2 transitions left-facing standing to crouching while aiming up.</summary>
    CrouchingTransitionAimUpLeftPose = 242,

    /// <summary>Pose $F3 transitions right-facing standing to crouching while aiming diagonally up.</summary>
    CrouchingTransitionAimDiagonalUpRightPose = 243,

    /// <summary>Pose $F4 transitions left-facing standing to crouching while aiming diagonally up.</summary>
    CrouchingTransitionAimDiagonalUpLeftPose = 244,

    /// <summary>Pose $F5 transitions right-facing standing to crouching while aiming diagonally down.</summary>
    CrouchingTransitionAimDiagonalDownRightPose = 245,

    /// <summary>Pose $F6 transitions left-facing standing to crouching while aiming diagonally down.</summary>
    CrouchingTransitionAimDiagonalDownLeftPose = 246,

    /// <summary>Pose $F7 transitions right-facing crouching to standing while aiming up.</summary>
    StandingTransitionAimUpRightPose = 247,

    /// <summary>Pose $F8 transitions left-facing crouching to standing while aiming up.</summary>
    StandingTransitionAimUpLeftPose = 248,

    /// <summary>Pose $F9 transitions right-facing crouching to standing while aiming diagonally up.</summary>
    StandingTransitionAimDiagonalUpRightPose = 249,

    /// <summary>Pose $FA transitions left-facing crouching to standing while aiming diagonally up.</summary>
    StandingTransitionAimDiagonalUpLeftPose = 250,

    /// <summary>Pose $FB transitions right-facing crouching to standing while aiming diagonally down.</summary>
    StandingTransitionAimDiagonalDownRightPose = 251,

    /// <summary>Pose $FC transitions left-facing crouching to standing while aiming diagonally down.</summary>
    StandingTransitionAimDiagonalDownLeftPose = 252,

    /// <summary>Pose $27 is ordinary right-facing crouching.</summary>
    CrouchingRightPose = 39,

    /// <summary>Pose $28 is ordinary left-facing crouching.</summary>
    CrouchingLeftPose = 40,

    /// <summary>Pose $35 is the right-facing standing-to-crouch transition.</summary>
    CrouchingTransitionRightPose = 53,

    /// <summary>Pose $36 is the left-facing standing-to-crouch transition.</summary>
    CrouchingTransitionLeftPose = 54,

    /// <summary>Pose $3B is the right-facing crouch-to-standing transition.</summary>
    StandingTransitionRightPose = 59,

    /// <summary>Pose $3C is the left-facing crouch-to-standing transition.</summary>
    StandingTransitionLeftPose = 60,

    /// <summary>Pose $4B is the one-frame right neutral-jump transition.</summary>
    NeutralJumpTransitionRightPose = 75,

    /// <summary>Pose $4C is the one-frame left neutral-jump transition.</summary>
    NeutralJumpTransitionLeftPose = 76,

    /// <summary>Pose $4D is the ordinary right-facing neutral jump.</summary>
    NeutralJumpRightPose = 77,

    /// <summary>Pose $4E is the ordinary left-facing neutral jump.</summary>
    NeutralJumpLeftPose = 78,

    /// <summary>Pose $A4 lands facing right after a non-spinning jump or fall.</summary>
    NormalLandingRightPose = 164,

    /// <summary>Pose $A5 lands facing left after a non-spinning jump or fall.</summary>
    NormalLandingLeftPose = 165,

    /// <summary>Pose $A6 lands facing right after a spin or wall jump.</summary>
    SpinLandingRightPose = 166,

    /// <summary>Pose $A7 lands facing left after a spin or wall jump.</summary>
    SpinLandingLeftPose = 167,
    /// <summary>Native unused pose $20, symbol UNUSED_PoseDefinitions_20_91B729 at $91:B729; preserves its distinct dispatch record.</summary>
    UnusedPose20 = 32,

    /// <summary>Native unused pose $21, symbol UNUSED_PoseDefinitions_21_91B731 at $91:B731; preserves its distinct dispatch record.</summary>
    UnusedPose21 = 33,

    /// <summary>Native unused pose $22, symbol UNUSED_PoseDefinitions_22_91B739 at $91:B739; preserves its distinct dispatch record.</summary>
    UnusedPose22 = 34,

    /// <summary>Native unused pose $23, symbol UNUSED_PoseDefinitions_23_91B741 at $91:B741; preserves its distinct dispatch record.</summary>
    UnusedPose23 = 35,

    /// <summary>Native unused pose $24, symbol UNUSED_PoseDefinitions_24_91B749 at $91:B749; preserves its distinct dispatch record.</summary>
    UnusedPose24 = 36,

    /// <summary>Native unused pose $39, symbol UNUSED_PoseDefinitions_39_91B7F1 at $91:B7F1; preserves its distinct dispatch record.</summary>
    UnusedPose39 = 57,

    /// <summary>Native unused pose $3A, symbol UNUSED_PoseDefinitions_3A_91B7F9 at $91:B7F9; preserves its distinct dispatch record.</summary>
    UnusedPose3A = 58,

    /// <summary>Native unused pose $3F, symbol UNUSED_PoseDefinitions_3F_91B821 at $91:B821; preserves its distinct dispatch record.</summary>
    UnusedPose3F = 63,

    /// <summary>Native unused pose $40, symbol UNUSED_PoseDefinitions_40_91B829 at $91:B829; preserves its distinct dispatch record.</summary>
    UnusedPose40 = 64,

    /// <summary>Native unused pose $42, symbol UNUSED_PoseDefinitions_42_91B839 at $91:B839; preserves its distinct dispatch record.</summary>
    UnusedPose42 = 66,

    /// <summary>Native unused pose $45, symbol UNUSED_PoseDefinitions_45_91B851 at $91:B851; preserves its distinct dispatch record.</summary>
    UnusedPose45 = 69,

    /// <summary>Native unused pose $46, symbol UNUSED_PoseDefinitions_46_91B859 at $91:B859; preserves its distinct dispatch record.</summary>
    UnusedPose46 = 70,

    /// <summary>Native unused pose $47, symbol UNUSED_PoseDefinitions_47_91B861 at $91:B861; preserves its distinct dispatch record.</summary>
    UnusedPose47 = 71,

    /// <summary>Native unused pose $48, symbol UNUSED_PoseDefinitions_48_91B869 at $91:B869; preserves its distinct dispatch record.</summary>
    UnusedPose48 = 72,

    /// <summary>Native unused pose $5B, symbol UNUSED_PoseDefinitions_5B_91B901 at $91:B901; preserves its distinct dispatch record.</summary>
    UnusedPose5B = 91,

    /// <summary>Native unused pose $5C, symbol UNUSED_PoseDefinitions_5C_91B909 at $91:B909; preserves its distinct dispatch record.</summary>
    UnusedPose5C = 92,

    /// <summary>Native unused pose $5D, symbol UNUSED_PoseDefinitions_5D_91B911 at $91:B911; preserves its distinct dispatch record.</summary>
    UnusedPose5D = 93,

    /// <summary>Native unused pose $5E, symbol UNUSED_PoseDefinitions_5E_91B919 at $91:B919; preserves its distinct dispatch record.</summary>
    UnusedPose5E = 94,

    /// <summary>Native unused pose $5F, symbol UNUSED_PoseDefinitions_5F_91B921 at $91:B921; preserves its distinct dispatch record.</summary>
    UnusedPose5F = 95,

    /// <summary>Native unused pose $60, symbol UNUSED_PoseDefinitions_60_91B929 at $91:B929; preserves its distinct dispatch record.</summary>
    UnusedPose60 = 96,

    /// <summary>Native unused pose $61, symbol UNUSED_PoseDefinitions_61_91B931 at $91:B931; preserves its distinct dispatch record.</summary>
    UnusedPose61 = 97,

    /// <summary>Native unused pose $62, symbol UNUSED_PoseDefinitions_62_91B939 at $91:B939; preserves its distinct dispatch record.</summary>
    UnusedPose62 = 98,

    /// <summary>Native unused pose $63, symbol UNUSED_PoseDefinitions_63_91B941 at $91:B941; preserves its distinct dispatch record.</summary>
    UnusedPose63 = 99,

    /// <summary>Native unused pose $64, symbol UNUSED_PoseDefinitions_64_91B949 at $91:B949; preserves its distinct dispatch record.</summary>
    UnusedPose64 = 100,

    /// <summary>Native unused pose $AC, symbol UNUSED_PoseDefinitions_AC_91BB89 at $91:BB89; preserves its distinct dispatch record.</summary>
    UnusedPoseAC = 172,

    /// <summary>Native unused pose $AD, symbol UNUSED_PoseDefinitions_AD_91BB91 at $91:BB91; preserves its distinct dispatch record.</summary>
    UnusedPoseAD = 173,

    /// <summary>Native unused pose $AE, symbol UNUSED_PoseDefinitions_AE_91BB99 at $91:BB99; preserves its distinct dispatch record.</summary>
    UnusedPoseAE = 174,

    /// <summary>Native unused pose $AF, symbol UNUSED_PoseDefinitions_AF_91BBA1 at $91:BBA1; preserves its distinct dispatch record.</summary>
    UnusedPoseAF = 175,

    /// <summary>Native unused pose $B0, symbol UNUSED_PoseDefinitions_B0_91BBA9 at $91:BBA9; preserves its distinct dispatch record.</summary>
    UnusedPoseB0 = 176,

    /// <summary>Native unused pose $B1, symbol UNUSED_PoseDefinitions_B1_91BBB1 at $91:BBB1; preserves its distinct dispatch record.</summary>
    UnusedPoseB1 = 177,

    /// <summary>Native unused pose $C5, symbol UNUSED_PoseDefinitions_C5_91BC51 at $91:BC51; preserves its distinct dispatch record.</summary>
    UnusedPoseC5 = 197,

    /// <summary>Native unused pose $C6, symbol UNUSED_PoseDefinitions_C6_91BC59 at $91:BC59; preserves its distinct dispatch record.</summary>
    UnusedPoseC6 = 198,

    /// <summary>Pose byte $FD, past the native definition table: its records read the adjacent code bytes.</summary>
    AdjacentCodePoseFD = 253,

    /// <summary>Pose byte $FE, past the native definition table: its records read the adjacent code bytes.</summary>
    AdjacentCodePoseFE = 254,

    /// <summary>Pose byte $FF, past the native definition table: its records read the adjacent code bytes.</summary>
    AdjacentCodePoseFF = 255,
}
