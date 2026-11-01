namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Samus's poses. Their values are arbitrary; the game's number for each pose is <see cref="SamusPoseNumbering"/>'s.
/// </summary>
public enum SamusPoseId
{
    /// <summary>
    /// Pose `$00`: the power-suit body viewed from the front during the intro, elevators,
    /// save-station appearance, and other controller-locked sequences.
    /// </summary>
    ForwardFacingPowerSuitPose,

    /// <summary>
    /// Pose `$9B`: the Varia/Gravity-suit counterpart of pose `$00`. Both use movement type
    /// zero, but only the power-suit record needs the draw's extra chest-cover sprite.
    /// </summary>
    ForwardFacingSuitedPose,

    /// <summary>Pose $01 is “facing right - normal” in the game table.</summary>
    FacingRightNormalPose,

    /// <summary>Pose $02 is “facing left - normal” in the game table.</summary>
    FacingLeftNormalPose,

    /// <summary>Pose $09 is “moving right - not aiming” in the game table.</summary>
    MovingRightNormalPose,

    /// <summary>Pose $0A is “moving left - not aiming” in the game table.</summary>
    MovingLeftNormalPose,

    /// <summary>
    /// Pose `$0B` is the right-moving horizontal-fire body. It is a genuine movement-type-one
    /// record, shares the ordinary ten-frame running delay stream, and is selected when Shot
    /// remains held with Right after the projectile direction has been established.
    /// </summary>
    MovingRightGunExtendedPose,

    /// <summary>Pose `$0C` is the left-moving mirror of <see cref="MovingRightGunExtendedPose"/>.</summary>
    MovingLeftGunExtendedPose,

    /// <summary>Unused movement-type-$0D right-facing pose definition `$65`.</summary>
    UnusedPose65,

    /// <summary>Unused movement-type-$0D left-facing pose definition `$66`.</summary>
    UnusedPose66,

    /// <summary>
    /// Pose $49 visually faces left while moonwalking right. Its pose-X byte is deliberately
    /// eight: horizontal movement follows travel direction, not the artwork's facing.
    /// </summary>
    MoonwalkFacingLeftPose,

    /// <summary>Pose $4A visually faces right while moonwalking left.</summary>
    MoonwalkFacingRightPose,

    /// <summary>Pose `$75`: left-facing/right-moving moonwalk aimed diagonally up-left.</summary>
    MoonwalkAimUpLeftPose,

    /// <summary>Pose `$76`: right-facing/left-moving mirror of pose `$75`.</summary>
    MoonwalkAimUpRightPose,

    /// <summary>Pose `$77`: left-facing/right-moving moonwalk aimed diagonally down-left.</summary>
    MoonwalkAimDownLeftPose,

    /// <summary>Pose `$78`: right-facing/left-moving mirror of pose `$77`.</summary>
    MoonwalkAimDownRightPose,

    /// <summary>
    /// Right-facing moonwalk art turning/jumping toward the left. Despite its traditional
    /// name, this is still grounded turning movement until its input table or its
    /// animation's final step installs the actual spin jump.
    /// </summary>
    MoonwalkTurnJumpLeftPose,

    /// <summary>Pose `$C0`: left-facing mirror of <see cref="MoonwalkTurnJumpLeftPose"/>.</summary>
    MoonwalkTurnJumpRightPose,

    /// <summary>Pose `$C1`: aimed-up moonwalk turn whose terminal jump faces left.</summary>
    MoonwalkTurnJumpAimUpLeftPose,

    /// <summary>Pose `$C2`: aimed-up moonwalk turn whose terminal jump faces right.</summary>
    MoonwalkTurnJumpAimUpRightPose,

    /// <summary>Pose `$C3`: aimed-down moonwalk turn whose terminal jump faces left.</summary>
    MoonwalkTurnJumpAimDownLeftPose,

    /// <summary>Pose `$C4`: aimed-down moonwalk turn whose terminal jump faces right.</summary>
    MoonwalkTurnJumpAimDownRightPose,

    /// <summary>Pose `$C7`: right-facing stored-shine windup body.</summary>
    ShinesparkWindupRightPose,

    /// <summary>Pose `$C8`: left-facing stored-shine windup body.</summary>
    ShinesparkWindupLeftPose,

    /// <summary>Pose `$C9`: horizontal shinespark travelling right.</summary>
    ShinesparkHorizontalRightPose,

    /// <summary>Pose `$CA`: horizontal shinespark travelling left.</summary>
    ShinesparkHorizontalLeftPose,

    /// <summary>Pose `$CB`: vertical shinespark using right-facing metadata.</summary>
    ShinesparkVerticalRightPose,

    /// <summary>Pose `$CC`: vertical shinespark using left-facing metadata.</summary>
    ShinesparkVerticalLeftPose,

    /// <summary>Pose `$CD`: diagonal up-right shinespark.</summary>
    ShinesparkDiagonalRightPose,

    /// <summary>Pose `$CE`: diagonal up-left shinespark.</summary>
    ShinesparkDiagonalLeftPose,

    /// <summary>Pose `$D3`: right-facing Crystal Flash body and delay program.</summary>
    CrystalFlashRightPose,

    /// <summary>Pose `$D4`: left-facing Crystal Flash mirror.</summary>
    CrystalFlashLeftPose,

    /// <summary>Pose `$D5`: right-facing standing X-ray body.</summary>
    XrayingStandingRightPose,

    /// <summary>Pose `$D6`: left-facing standing X-ray body.</summary>
    XrayingStandingLeftPose,

    /// <summary>Pose `$D7`: right-facing fatal-damage / crystal-flash-ending body.</summary>
    DeathSequenceRightPose,

    /// <summary>Pose `$D8`: left-facing fatal-damage / crystal-flash-ending mirror.</summary>
    DeathSequenceLeftPose,

    /// <summary>Pose `$D9`: right-facing crouching X-ray body.</summary>
    XrayingCrouchingRightPose,

    /// <summary>Pose `$DA`: left-facing crouching X-ray body.</summary>
    XrayingCrouchingLeftPose,

    /// <summary>Unused compact right-facing transition pose definition `$DB`.</summary>
    UnusedPoseDb,

    /// <summary>Unused compact left-facing transition pose definition `$DC`.</summary>
    UnusedPoseDc,

    /// <summary>Unused standing-radius right-facing transition pose definition `$DD`.</summary>
    UnusedPoseDd,

    /// <summary>Unused standing-radius left-facing transition pose definition `$DE`.</summary>
    UnusedPoseDe,

    /// <summary>Unused Draygon-owned compact pose definition `$DF`.</summary>
    UnusedPoseDf,

    /// <summary>Pose `$E8`: right-facing drained crouch/fall animation.</summary>
    DrainedCrouchingRightPose,

    /// <summary>Pose `$E9`: left-facing drained crouch/fall animation.</summary>
    DrainedCrouchingLeftPose,

    /// <summary>Pose `$EA`: right-facing drained standing animation.</summary>
    DrainedStandingRightPose,

    /// <summary>Pose `$EB`: left-facing drained standing animation.</summary>
    DrainedStandingLeftPose,

    /// <summary>Pose `$89`: facing right after forward running collides with a wall.</summary>
    RanIntoWallRightPose,

    /// <summary>Pose `$8A`: facing-left mirror of pose `$89`.</summary>
    RanIntoWallLeftPose,

    /// <summary>Pose `$CF`: right-facing wall-stop art aimed diagonally up-right.</summary>
    RanIntoWallAimUpRightPose,

    /// <summary>Pose `$D0`: left-facing wall-stop art aimed diagonally up-left.</summary>
    RanIntoWallAimUpLeftPose,

    /// <summary>Pose `$D1`: right-facing wall-stop art aimed diagonally down-right.</summary>
    RanIntoWallAimDownRightPose,

    /// <summary>Pose `$D2`: left-facing wall-stop art aimed diagonally down-left.</summary>
    RanIntoWallAimDownLeftPose,

    /// <summary>Pose $25 turns a right-facing grounded Samus toward the left.</summary>
    TurningRightToLeftPose,

    /// <summary>Pose $26 turns a left-facing grounded Samus toward the right.</summary>
    TurningLeftToRightPose,

    /// <summary>Pose $19 is the ordinary right-facing spin jump.</summary>
    SpinJumpRightPose,

    /// <summary>Pose $1A is the ordinary left-facing spin jump.</summary>
    SpinJumpLeftPose,

    /// <summary>
    /// Pose `$1B` is the right-facing Space Jump body. The equipment-aware movement-type
    /// initializer substitutes this for the transition table's ordinary
    /// `$19` target when Space Jump is equipped and Screw Attack is not.
    /// </summary>
    SpaceJumpRightPose,

    /// <summary>Pose `$1C` is the left-facing mirror of <see cref="SpaceJumpRightPose"/>.</summary>
    SpaceJumpLeftPose,

    /// <summary>
    /// Pose `$81` is the right-facing Screw Attack body. The Screw Attack bit has
    /// priority over the Space Jump bit in the spin-pose selector.
    /// </summary>
    ScrewAttackRightPose,

    /// <summary>Pose `$82` is the left-facing mirror of <see cref="ScrewAttackRightPose"/>.</summary>
    ScrewAttackLeftPose,

    /// <summary>Pose $2F turns a right-facing normal jump toward the left.</summary>
    TurningRightToLeftJumpPose,

    /// <summary>Pose $30 turns a left-facing normal jump toward the right.</summary>
    TurningLeftToRightJumpPose,

    /// <summary>Pose $83 is the right-facing wall-jump launch animation.</summary>
    WallJumpRightPose,

    /// <summary>Pose $84 is the left-facing wall-jump launch animation.</summary>
    WallJumpLeftPose,

    /// <summary>Pose $B2 is the right-facing airborne grapple-swing body.</summary>
    GrappleSwingRightPose,

    /// <summary>Pose $B3 is the left-facing airborne grapple-swing body.</summary>
    GrappleSwingLeftPose,

    /// <summary>Pose $A8 is the standing right-facing horizontal grapple lock.</summary>
    GrappleStandingRightPose,

    /// <summary>Pose $A9 is the standing left-facing horizontal grapple lock.</summary>
    GrappleStandingLeftPose,

    /// <summary>Pose $AA is the standing right-facing down-right grapple lock.</summary>
    GrappleStandingDownRightPose,

    /// <summary>Pose $AB is the standing left-facing down-left grapple lock.</summary>
    GrappleStandingDownLeftPose,

    /// <summary>Pose $B4 is the crouched right-facing horizontal grapple lock.</summary>
    GrappleCrouchingRightPose,

    /// <summary>Pose $B5 is the crouched left-facing horizontal grapple lock.</summary>
    GrappleCrouchingLeftPose,

    /// <summary>Pose $B6 is the crouched right-facing down-right grapple lock.</summary>
    GrappleCrouchingDownRightPose,

    /// <summary>Pose $B7 is the crouched left-facing down-left grapple lock.</summary>
    GrappleCrouchingDownLeftPose,

    /// <summary>Pose $B8 is the left-wall grapple-jump contact body.</summary>
    GrappleWallContactLeftPose,

    /// <summary>Pose $B9 is the right-wall grapple-jump contact body.</summary>
    GrappleWallContactRightPose,

    /// <summary>Pose `$BA`: left-facing neutral body held in Draygon's claws.</summary>
    DraygonGrabbedNeutralLeftPose,

    /// <summary>Pose `$BB`: left-facing Draygon-held body aiming diagonally upward.</summary>
    DraygonGrabbedAimUpLeftPose,

    /// <summary>Pose `$BC`: left-facing Draygon-held firing body.</summary>
    DraygonGrabbedFiringLeftPose,

    /// <summary>Pose `$BD`: left-facing Draygon-held body aiming diagonally downward.</summary>
    DraygonGrabbedAimDownLeftPose,

    /// <summary>Pose `$BE`: left-facing six-frame struggling animation while held by Draygon.</summary>
    DraygonGrabbedMovingLeftPose,

    /// <summary>Pose `$EC`: right-facing neutral body held in Draygon's claws.</summary>
    DraygonGrabbedNeutralRightPose,

    /// <summary>Pose `$ED`: right-facing Draygon-held body aiming diagonally upward.</summary>
    DraygonGrabbedAimUpRightPose,

    /// <summary>Pose `$EE`: right-facing Draygon-held firing body.</summary>
    DraygonGrabbedFiringRightPose,

    /// <summary>Pose `$EF`: right-facing Draygon-held body aiming diagonally downward.</summary>
    DraygonGrabbedAimDownRightPose,

    /// <summary>Pose `$F0`: right-facing six-frame struggling animation while held by Draygon.</summary>
    DraygonGrabbedMovingRightPose,

    /// <summary>
    /// Pose $4F is the visually left-facing damage boost; its X-direction byte is the
    /// deliberately reversed value eight that drives the boost to the right.
    /// </summary>
    DamageBoostLeftPose,

    /// <summary>
    /// Pose $50 is the visually right-facing damage boost; its X-direction byte is the
    /// deliberately reversed value four that drives the boost to the left.
    /// </summary>
    DamageBoostRightPose,

    /// <summary>Pose $53 is the right-facing normal knockback body.</summary>
    KnockbackRightPose,

    /// <summary>Pose $54 is the left-facing normal knockback body.</summary>
    KnockbackLeftPose,

    /// <summary>Pose $87 turns a right-facing fall toward the left.</summary>
    TurningRightToLeftFallingPose,

    /// <summary>Pose $88 turns a left-facing fall toward the right.</summary>
    TurningLeftToRightFallingPose,

    /// <summary>Pose $8F turns right-to-left in a normal jump while retaining straight-up aim.</summary>
    TurningRightToLeftJumpAimUpPose,
    /// <summary>Pose $90 turns left-to-right in a normal jump while retaining straight-up aim.</summary>
    TurningLeftToRightJumpAimUpPose,
    /// <summary>Pose $91 turns right-to-left in a normal jump while retaining downward or down-right aim.</summary>
    TurningRightToLeftJumpAimDownPose,
    /// <summary>Pose $92 turns left-to-right in a normal jump while retaining downward or down-left aim.</summary>
    TurningLeftToRightJumpAimDownPose,
    /// <summary>Pose $93 turns right-to-left during a fall while retaining straight-up aim.</summary>
    TurningRightToLeftFallingAimUpPose,
    /// <summary>Pose $94 turns left-to-right during a fall while retaining straight-up aim.</summary>
    TurningLeftToRightFallingAimUpPose,
    /// <summary>Pose $95 turns right-to-left during a fall while retaining downward or down-right aim.</summary>
    TurningRightToLeftFallingAimDownPose,
    /// <summary>Pose $96 turns left-to-right during a fall while retaining downward or down-left aim.</summary>
    TurningLeftToRightFallingAimDownPose,
    /// <summary>Pose $9E turns right-to-left in a normal jump while retaining diagonal-up aim.</summary>
    TurningRightToLeftJumpAimDiagonalUpPose,
    /// <summary>Pose $9F turns left-to-right in a normal jump while retaining diagonal-up aim.</summary>
    TurningLeftToRightJumpAimDiagonalUpPose,
    /// <summary>Pose $A0 turns right-to-left during a fall while retaining diagonal-up aim.</summary>
    TurningRightToLeftFallingAimDiagonalUpPose,
    /// <summary>Pose $A1 turns left-to-right during a fall while retaining diagonal-up aim.</summary>
    TurningLeftToRightFallingAimDiagonalUpPose,

    /// <summary>Pose $1D is the stationary right-facing ordinary morph ball on the ground.</summary>
    MorphBallGroundRightPose,

    /// <summary>Pose $1E is the ordinary morph ball rolling to the right on the ground.</summary>
    MorphBallMovingRightPose,

    /// <summary>Pose $1F is the ordinary morph ball rolling to the left on the ground.</summary>
    MorphBallMovingLeftPose,

    /// <summary>Pose $31 is the right-facing ordinary morph ball in the air.</summary>
    MorphBallFallingRightPose,

    /// <summary>Pose $32 is the left-facing ordinary morph ball in the air.</summary>
    MorphBallFallingLeftPose,

    /// <summary>
    /// Pose `$33` is the unused right-facing body selected only by movement type `$07`'s
    /// otherwise-unused knockback transition. Naming it keeps the exhaustive
    /// pose table executable without pretending the retail game reaches it.
    /// </summary>
    UnusedKnockbackRightPose,

    /// <summary>Left-facing partner of <see cref="UnusedKnockbackRightPose"/> at pose `$34`.</summary>
    UnusedKnockbackLeftPose,

    /// <summary>Pose $37 is the right-facing crouch-to-morph transition.</summary>
    MorphingTransitionRightPose,

    /// <summary>Pose $38 is the left-facing crouch-to-morph transition.</summary>
    MorphingTransitionLeftPose,

    /// <summary>Pose $3D is the right-facing morph-to-crouch transition.</summary>
    UnmorphingTransitionRightPose,

    /// <summary>Pose $3E is the left-facing morph-to-crouch transition.</summary>
    UnmorphingTransitionLeftPose,

    /// <summary>Pose $41 is the stationary left-facing ordinary morph ball on the ground.</summary>
    MorphBallGroundLeftPose,

    /// <summary>Pose $79 is the stationary right-facing Spring Ball on the ground.</summary>
    SpringBallGroundRightPose,

    /// <summary>Pose $7A is the stationary left-facing Spring Ball on the ground.</summary>
    SpringBallGroundLeftPose,

    /// <summary>Pose $7B is the Spring Ball moving right on the ground.</summary>
    SpringBallMovingRightPose,

    /// <summary>Pose $7C is the Spring Ball moving left on the ground.</summary>
    SpringBallMovingLeftPose,

    /// <summary>Pose $7D is the right-facing Spring Ball falling or bouncing.</summary>
    SpringBallFallingRightPose,

    /// <summary>Pose $7E is the left-facing Spring Ball falling or bouncing.</summary>
    SpringBallFallingLeftPose,

    /// <summary>Pose $7F is the right-facing Spring Ball in its powered jump.</summary>
    SpringBallJumpRightPose,

    /// <summary>Pose $80 is the left-facing Spring Ball in its powered jump.</summary>
    SpringBallJumpLeftPose,

    /// <summary>Pose $29 is the unaimed right-facing falling pose.</summary>
    FallingRightPose,

    /// <summary>Pose $2A is the unaimed left-facing falling pose.</summary>
    FallingLeftPose,

    /// <summary>
    /// Pose `$67` is the right-facing falling body with the cannon held horizontally after
    /// firing. Its delay stream has the same terminal-velocity split as `$29`.
    /// </summary>
    FallingGunExtendedRightPose,

    /// <summary>Pose `$68` is the left-facing mirror of <see cref="FallingGunExtendedRightPose"/>.</summary>
    FallingGunExtendedLeftPose,

    /// <summary>Pose $03 is stationary, facing right, with the arm cannon aimed straight up.</summary>
    StandingAimUpRightPose,

    /// <summary>Pose $04 is stationary, facing left, with the arm cannon aimed straight up.</summary>
    StandingAimUpLeftPose,

    /// <summary>Pose $05 is stationary, facing right, and aiming diagonally up-right.</summary>
    StandingAimDiagonalUpRightPose,

    /// <summary>Pose $06 is stationary, facing left, and aiming diagonally up-left.</summary>
    StandingAimDiagonalUpLeftPose,

    /// <summary>Pose $07 is stationary, facing right, and aiming diagonally down-right.</summary>
    StandingAimDiagonalDownRightPose,

    /// <summary>Pose $08 is stationary, facing left, and aiming diagonally down-left.</summary>
    StandingAimDiagonalDownLeftPose,

    /// <summary>Pose $0D is the unused right-moving straight-up aim record.</summary>
    RunningAimUpRightPose,

    /// <summary>Pose $0E is the unused left-moving straight-up aim record.</summary>
    RunningAimUpLeftPose,

    /// <summary>Pose $0F moves right while aiming diagonally up-right.</summary>
    RunningAimDiagonalUpRightPose,

    /// <summary>Pose $10 moves left while aiming diagonally up-left.</summary>
    RunningAimDiagonalUpLeftPose,

    /// <summary>Pose $11 moves right while aiming diagonally down-right.</summary>
    RunningAimDiagonalDownRightPose,

    /// <summary>Pose $12 moves left while aiming diagonally down-left.</summary>
    RunningAimDiagonalDownLeftPose,

    /// <summary>
    /// Pose `$13` is a stationary right-facing normal jump with the cannon extended. Unlike
    /// `$4D`, its pose table deliberately stores no no-input fallback, so the firing body can
    /// remain active until another input-table record or landing replaces it.
    /// </summary>
    NormalJumpGunExtendedRightPose,

    /// <summary>Pose `$14` is the left-facing mirror of <see cref="NormalJumpGunExtendedRightPose"/>.</summary>
    NormalJumpGunExtendedLeftPose,

    /// <summary>Pose $15 is a right-facing normal jump aimed straight up.</summary>
    NormalJumpAimUpRightPose,

    /// <summary>Pose $16 is a left-facing normal jump aimed straight up.</summary>
    NormalJumpAimUpLeftPose,

    /// <summary>Pose $17 is the compact right-facing normal jump aimed straight down.</summary>
    NormalJumpAimDownRightPose,

    /// <summary>Pose $18 is the compact left-facing normal jump aimed straight down.</summary>
    NormalJumpAimDownLeftPose,

    /// <summary>Pose $51 is a right-facing normal jump using the moving-forward art.</summary>
    NormalJumpForwardRightPose,

    /// <summary>Pose $52 is a left-facing normal jump using the moving-forward art.</summary>
    NormalJumpForwardLeftPose,

    /// <summary>Pose $55 is the right-facing jump transition aimed straight up.</summary>
    NormalJumpTransitionAimUpRightPose,

    /// <summary>Pose $56 is the left-facing jump transition aimed straight up.</summary>
    NormalJumpTransitionAimUpLeftPose,

    /// <summary>Pose $57 is the right-facing jump transition aimed diagonally up.</summary>
    NormalJumpTransitionAimDiagonalUpRightPose,

    /// <summary>Pose $58 is the left-facing jump transition aimed diagonally up.</summary>
    NormalJumpTransitionAimDiagonalUpLeftPose,

    /// <summary>Pose $59 is the right-facing jump transition aimed diagonally down.</summary>
    NormalJumpTransitionAimDiagonalDownRightPose,

    /// <summary>Pose $5A is the left-facing jump transition aimed diagonally down.</summary>
    NormalJumpTransitionAimDiagonalDownLeftPose,

    /// <summary>Pose $69 is a right-facing normal jump aimed diagonally up.</summary>
    NormalJumpAimDiagonalUpRightPose,

    /// <summary>Pose $6A is a left-facing normal jump aimed diagonally up.</summary>
    NormalJumpAimDiagonalUpLeftPose,

    /// <summary>Pose $6B is a right-facing normal jump aimed diagonally down.</summary>
    NormalJumpAimDiagonalDownRightPose,

    /// <summary>Pose $6C is a left-facing normal jump aimed diagonally down.</summary>
    NormalJumpAimDiagonalDownLeftPose,

    /// <summary>Pose $2B is right-facing falling aimed straight up.</summary>
    FallingAimUpRightPose,

    /// <summary>Pose $2C is left-facing falling aimed straight up.</summary>
    FallingAimUpLeftPose,

    /// <summary>Pose $2D is the compact right-facing fall aimed straight down.</summary>
    FallingAimDownRightPose,

    /// <summary>Pose $2E is the compact left-facing fall aimed straight down.</summary>
    FallingAimDownLeftPose,

    /// <summary>Pose $6D is right-facing falling aimed diagonally up.</summary>
    FallingAimDiagonalUpRightPose,

    /// <summary>Pose $6E is left-facing falling aimed diagonally up.</summary>
    FallingAimDiagonalUpLeftPose,

    /// <summary>Pose $6F is right-facing falling aimed diagonally down.</summary>
    FallingAimDiagonalDownRightPose,

    /// <summary>Pose $70 is left-facing falling aimed diagonally down.</summary>
    FallingAimDiagonalDownLeftPose,

    /// <summary>Pose $71 is right-facing crouching aimed diagonally up.</summary>
    CrouchingAimDiagonalUpRightPose,

    /// <summary>Pose $72 is left-facing crouching aimed diagonally up.</summary>
    CrouchingAimDiagonalUpLeftPose,

    /// <summary>Pose $73 is right-facing crouching aimed diagonally down.</summary>
    CrouchingAimDiagonalDownRightPose,

    /// <summary>Pose $74 is left-facing crouching aimed diagonally down.</summary>
    CrouchingAimDiagonalDownLeftPose,

    /// <summary>Pose $85 is right-facing crouching aimed straight up.</summary>
    CrouchingAimUpRightPose,

    /// <summary>Pose $86 is left-facing crouching aimed straight up.</summary>
    CrouchingAimUpLeftPose,

    /// <summary>Pose $43 turns right-to-left while crouched without retaining aim.</summary>
    TurningRightToLeftCrouchingPose,

    /// <summary>Pose $44 turns left-to-right while crouched without retaining aim.</summary>
    TurningLeftToRightCrouchingPose,

    /// <summary>Pose $8B turns right-to-left on ground while preserving straight-up aim.</summary>
    TurningRightToLeftAimUpPose,

    /// <summary>Pose $8C turns left-to-right on ground while preserving straight-up aim.</summary>
    TurningLeftToRightAimUpPose,

    /// <summary>Pose $8D turns right-to-left on ground while preserving diagonal-down aim.</summary>
    TurningRightToLeftAimDiagonalDownPose,

    /// <summary>Pose $8E turns left-to-right on ground while preserving diagonal-down aim.</summary>
    TurningLeftToRightAimDiagonalDownPose,

    /// <summary>Pose $9C turns right-to-left on ground while preserving diagonal-up aim.</summary>
    TurningRightToLeftAimDiagonalUpPose,

    /// <summary>Pose $9D turns left-to-right on ground while preserving diagonal-up aim.</summary>
    TurningLeftToRightAimDiagonalUpPose,

    /// <summary>Pose $97 turns right-to-left while crouched and preserving straight-up aim.</summary>
    TurningRightToLeftCrouchingAimUpPose,

    /// <summary>Pose $98 turns left-to-right while crouched and preserving straight-up aim.</summary>
    TurningLeftToRightCrouchingAimUpPose,

    /// <summary>Pose $99 turns right-to-left while crouched and preserving diagonal-down aim.</summary>
    TurningRightToLeftCrouchingAimDiagonalDownPose,

    /// <summary>Pose $9A turns left-to-right while crouched and preserving diagonal-down aim.</summary>
    TurningLeftToRightCrouchingAimDiagonalDownPose,

    /// <summary>Pose $A2 turns right-to-left while crouched and preserving diagonal-up aim.</summary>
    TurningRightToLeftCrouchingAimDiagonalUpPose,

    /// <summary>Pose $A3 turns left-to-right while crouched and preserving diagonal-up aim.</summary>
    TurningLeftToRightCrouchingAimDiagonalUpPose,

    /// <summary>Pose $E0 is a right-facing normal-jump landing aimed straight up.</summary>
    LandingAimUpRightPose,

    /// <summary>Pose $E1 is a left-facing normal-jump landing aimed straight up.</summary>
    LandingAimUpLeftPose,

    /// <summary>Pose $E2 is a right-facing normal-jump landing aimed diagonally up.</summary>
    LandingAimDiagonalUpRightPose,

    /// <summary>Pose $E3 is a left-facing normal-jump landing aimed diagonally up.</summary>
    LandingAimDiagonalUpLeftPose,

    /// <summary>Pose $E4 is a right-facing normal-jump landing aimed diagonally down.</summary>
    LandingAimDiagonalDownRightPose,

    /// <summary>Pose $E5 is a left-facing normal-jump landing aimed diagonally down.</summary>
    LandingAimDiagonalDownLeftPose,

    /// <summary>
    /// Pose `$E6` is the right-facing horizontal-fire landing selected by the landing transition only
    /// when the current shot direction is two and the Shot binding is still held at impact.
    /// Its terminal `$F8,$01` command returns to ordinary standing after the landing frames.
    /// </summary>
    FiringLandingRightPose,

    /// <summary>Pose `$E7` is the left-facing mirror of <see cref="FiringLandingRightPose"/>.</summary>
    FiringLandingLeftPose,

    /// <summary>Pose $F1 transitions right-facing standing to crouching while aiming up.</summary>
    CrouchingTransitionAimUpRightPose,

    /// <summary>Pose $F2 transitions left-facing standing to crouching while aiming up.</summary>
    CrouchingTransitionAimUpLeftPose,

    /// <summary>Pose $F3 transitions right-facing standing to crouching while aiming diagonally up.</summary>
    CrouchingTransitionAimDiagonalUpRightPose,

    /// <summary>Pose $F4 transitions left-facing standing to crouching while aiming diagonally up.</summary>
    CrouchingTransitionAimDiagonalUpLeftPose,

    /// <summary>Pose $F5 transitions right-facing standing to crouching while aiming diagonally down.</summary>
    CrouchingTransitionAimDiagonalDownRightPose,

    /// <summary>Pose $F6 transitions left-facing standing to crouching while aiming diagonally down.</summary>
    CrouchingTransitionAimDiagonalDownLeftPose,

    /// <summary>Pose $F7 transitions right-facing crouching to standing while aiming up.</summary>
    StandingTransitionAimUpRightPose,

    /// <summary>Pose $F8 transitions left-facing crouching to standing while aiming up.</summary>
    StandingTransitionAimUpLeftPose,

    /// <summary>Pose $F9 transitions right-facing crouching to standing while aiming diagonally up.</summary>
    StandingTransitionAimDiagonalUpRightPose,

    /// <summary>Pose $FA transitions left-facing crouching to standing while aiming diagonally up.</summary>
    StandingTransitionAimDiagonalUpLeftPose,

    /// <summary>Pose $FB transitions right-facing crouching to standing while aiming diagonally down.</summary>
    StandingTransitionAimDiagonalDownRightPose,

    /// <summary>Pose $FC transitions left-facing crouching to standing while aiming diagonally down.</summary>
    StandingTransitionAimDiagonalDownLeftPose,

    /// <summary>Pose $27 is ordinary right-facing crouching.</summary>
    CrouchingRightPose,

    /// <summary>Pose $28 is ordinary left-facing crouching.</summary>
    CrouchingLeftPose,

    /// <summary>Pose $35 is the right-facing standing-to-crouch transition.</summary>
    CrouchingTransitionRightPose,

    /// <summary>Pose $36 is the left-facing standing-to-crouch transition.</summary>
    CrouchingTransitionLeftPose,

    /// <summary>Pose $3B is the right-facing crouch-to-standing transition.</summary>
    StandingTransitionRightPose,

    /// <summary>Pose $3C is the left-facing crouch-to-standing transition.</summary>
    StandingTransitionLeftPose,

    /// <summary>Pose $4B is the one-frame right neutral-jump transition.</summary>
    NeutralJumpTransitionRightPose,

    /// <summary>Pose $4C is the one-frame left neutral-jump transition.</summary>
    NeutralJumpTransitionLeftPose,

    /// <summary>Pose $4D is the ordinary right-facing neutral jump.</summary>
    NeutralJumpRightPose,

    /// <summary>Pose $4E is the ordinary left-facing neutral jump.</summary>
    NeutralJumpLeftPose,

    /// <summary>Pose $A4 lands facing right after a non-spinning jump or fall.</summary>
    NormalLandingRightPose,

    /// <summary>Pose $A5 lands facing left after a non-spinning jump or fall.</summary>
    NormalLandingLeftPose,

    /// <summary>Pose $A6 lands facing right after a spin or wall jump.</summary>
    SpinLandingRightPose,

    /// <summary>Pose $A7 lands facing left after a spin or wall jump.</summary>
    SpinLandingLeftPose,
    /// <summary>Unused pose $20; preserves its distinct dispatch record.</summary>
    UnusedPose20,

    /// <summary>Unused pose $21; preserves its distinct dispatch record.</summary>
    UnusedPose21,

    /// <summary>Unused pose $22; preserves its distinct dispatch record.</summary>
    UnusedPose22,

    /// <summary>Unused pose $23; preserves its distinct dispatch record.</summary>
    UnusedPose23,

    /// <summary>Unused pose $24; preserves its distinct dispatch record.</summary>
    UnusedPose24,

    /// <summary>Unused pose $39; preserves its distinct dispatch record.</summary>
    UnusedPose39,

    /// <summary>Unused pose $3A; preserves its distinct dispatch record.</summary>
    UnusedPose3A,

    /// <summary>Unused pose $3F; preserves its distinct dispatch record.</summary>
    UnusedPose3F,

    /// <summary>Unused pose $40; preserves its distinct dispatch record.</summary>
    UnusedPose40,

    /// <summary>Unused pose $42; preserves its distinct dispatch record.</summary>
    UnusedPose42,

    /// <summary>Unused pose $45; preserves its distinct dispatch record.</summary>
    UnusedPose45,

    /// <summary>Unused pose $46; preserves its distinct dispatch record.</summary>
    UnusedPose46,

    /// <summary>Unused pose $47; preserves its distinct dispatch record.</summary>
    UnusedPose47,

    /// <summary>Unused pose $48; preserves its distinct dispatch record.</summary>
    UnusedPose48,

    /// <summary>Unused pose $5B; preserves its distinct dispatch record.</summary>
    UnusedPose5B,

    /// <summary>Unused pose $5C; preserves its distinct dispatch record.</summary>
    UnusedPose5C,

    /// <summary>Unused pose $5D; preserves its distinct dispatch record.</summary>
    UnusedPose5D,

    /// <summary>Unused pose $5E; preserves its distinct dispatch record.</summary>
    UnusedPose5E,

    /// <summary>Unused pose $5F; preserves its distinct dispatch record.</summary>
    UnusedPose5F,

    /// <summary>Unused pose $60; preserves its distinct dispatch record.</summary>
    UnusedPose60,

    /// <summary>Unused pose $61; preserves its distinct dispatch record.</summary>
    UnusedPose61,

    /// <summary>Unused pose $62; preserves its distinct dispatch record.</summary>
    UnusedPose62,

    /// <summary>Unused pose $63; preserves its distinct dispatch record.</summary>
    UnusedPose63,

    /// <summary>Unused pose $64; preserves its distinct dispatch record.</summary>
    UnusedPose64,

    /// <summary>Unused pose $AC; preserves its distinct dispatch record.</summary>
    UnusedPoseAC,

    /// <summary>Unused pose $AD; preserves its distinct dispatch record.</summary>
    UnusedPoseAD,

    /// <summary>Unused pose $AE; preserves its distinct dispatch record.</summary>
    UnusedPoseAE,

    /// <summary>Unused pose $AF; preserves its distinct dispatch record.</summary>
    UnusedPoseAF,

    /// <summary>Unused pose $B0; preserves its distinct dispatch record.</summary>
    UnusedPoseB0,

    /// <summary>Unused pose $B1; preserves its distinct dispatch record.</summary>
    UnusedPoseB1,

    /// <summary>Unused pose $C5; preserves its distinct dispatch record.</summary>
    UnusedPoseC5,

    /// <summary>Unused pose $C6; preserves its distinct dispatch record.</summary>
    UnusedPoseC6,

    /// <summary>Pose byte $FD, past the definition table: its records read the adjacent code bytes.</summary>
    AdjacentCodePoseFD,

    /// <summary>Pose byte $FE, past the definition table: its records read the adjacent code bytes.</summary>
    AdjacentCodePoseFE,

    /// <summary>Pose byte $FF, past the definition table: its records read the adjacent code bytes.</summary>
    AdjacentCodePoseFF,
}
