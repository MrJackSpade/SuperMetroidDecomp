namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Samus's poses. Their values and their order are arbitrary.
/// </summary>
public enum SamusPoseId
{
    /// <summary>
    /// The power-suit body viewed from the front during the intro, elevators,
    /// save-station appearance, and other controller-locked sequences.
    /// </summary>
    ForwardFacingPowerSuitPose,

    /// <summary>
    /// The Varia/Gravity-suit counterpart of <see cref="SamusPoseId.ForwardFacingPowerSuitPose"/>. Both use movement type
    /// zero, but only the power-suit record needs the draw's extra chest-cover sprite.
    /// </summary>
    ForwardFacingSuitedPose,

    /// <summary>This pose is “facing right - normal” in the game table.</summary>
    FacingRightNormalPose,

    /// <summary>This pose is “facing left - normal” in the game table.</summary>
    FacingLeftNormalPose,

    /// <summary>This pose is “moving right - not aiming” in the game table.</summary>
    MovingRightNormalPose,

    /// <summary>This pose is “moving left - not aiming” in the game table.</summary>
    MovingLeftNormalPose,

    /// <summary>
    /// This pose is the right-moving horizontal-fire body. It is a genuine movement-type-one
    /// record, shares the ordinary ten-frame running delay stream, and is selected when Shot
    /// remains held with Right after the projectile direction has been established.
    /// </summary>
    MovingRightGunExtendedPose,

    /// <summary>This pose is the left-moving mirror of <see cref="MovingRightGunExtendedPose"/>.</summary>
    MovingLeftGunExtendedPose,

    /// <summary>A right-facing compact airborne pose whose animation repeats; with no input it falls facing right.</summary>
    CompactAirborneFallingRightPose,

    /// <summary>The left-facing partner of <see cref="CompactAirborneFallingRightPose"/>; with no input it falls facing left.</summary>
    CompactAirborneFallingLeftPose,

    /// <summary>
    /// This pose visually faces left while moonwalking right. Its pose X direction is deliberately
    /// eight: horizontal movement follows travel direction, not the artwork's facing.
    /// </summary>
    MoonwalkFacingLeftPose,

    /// <summary>This pose visually faces right while moonwalking left.</summary>
    MoonwalkFacingRightPose,

    /// <summary>Left-facing/right-moving moonwalk aimed diagonally up-left.</summary>
    MoonwalkAimUpLeftPose,

    /// <summary>Right-facing/left-moving mirror of <see cref="SamusPoseId.MoonwalkAimUpLeftPose"/>.</summary>
    MoonwalkAimUpRightPose,

    /// <summary>Left-facing/right-moving moonwalk aimed diagonally down-left.</summary>
    MoonwalkAimDownLeftPose,

    /// <summary>Right-facing/left-moving mirror of <see cref="SamusPoseId.MoonwalkAimDownLeftPose"/>.</summary>
    MoonwalkAimDownRightPose,

    /// <summary>
    /// Right-facing moonwalk art turning/jumping toward the left. Despite its traditional
    /// name, this is still grounded turning movement until its input table or its
    /// animation's final step installs the actual spin jump.
    /// </summary>
    MoonwalkTurnJumpLeftPose,

    /// <summary>Left-facing mirror of <see cref="MoonwalkTurnJumpLeftPose"/>.</summary>
    MoonwalkTurnJumpRightPose,

    /// <summary>Aimed-up moonwalk turn whose terminal jump faces left.</summary>
    MoonwalkTurnJumpAimUpLeftPose,

    /// <summary>Aimed-up moonwalk turn whose terminal jump faces right.</summary>
    MoonwalkTurnJumpAimUpRightPose,

    /// <summary>Aimed-down moonwalk turn whose terminal jump faces left.</summary>
    MoonwalkTurnJumpAimDownLeftPose,

    /// <summary>Aimed-down moonwalk turn whose terminal jump faces right.</summary>
    MoonwalkTurnJumpAimDownRightPose,

    /// <summary>Right-facing stored-shine windup body.</summary>
    ShinesparkWindupRightPose,

    /// <summary>Left-facing stored-shine windup body.</summary>
    ShinesparkWindupLeftPose,

    /// <summary>Horizontal shinespark travelling right.</summary>
    ShinesparkHorizontalRightPose,

    /// <summary>Horizontal shinespark travelling left.</summary>
    ShinesparkHorizontalLeftPose,

    /// <summary>Vertical shinespark using right-facing metadata.</summary>
    ShinesparkVerticalRightPose,

    /// <summary>Vertical shinespark using left-facing metadata.</summary>
    ShinesparkVerticalLeftPose,

    /// <summary>Diagonal up-right shinespark.</summary>
    ShinesparkDiagonalRightPose,

    /// <summary>Diagonal up-left shinespark.</summary>
    ShinesparkDiagonalLeftPose,

    /// <summary>Right-facing Crystal Flash body and delay animation.</summary>
    CrystalFlashRightPose,

    /// <summary>Left-facing Crystal Flash mirror.</summary>
    CrystalFlashLeftPose,

    /// <summary>Right-facing standing X-ray body.</summary>
    XrayingStandingRightPose,

    /// <summary>Left-facing standing X-ray body.</summary>
    XrayingStandingLeftPose,

    /// <summary>Right-facing fatal-damage / crystal-flash-ending body.</summary>
    DeathSequenceRightPose,

    /// <summary>Left-facing fatal-damage / crystal-flash-ending mirror.</summary>
    DeathSequenceLeftPose,

    /// <summary>Right-facing crouching X-ray body.</summary>
    XrayingCrouchingRightPose,

    /// <summary>Left-facing crouching X-ray body.</summary>
    XrayingCrouchingLeftPose,

    /// <summary>A right-facing compact posture transition that morphs into the ball, or the Spring Ball, on the ground or falling.</summary>
    CompactMorphingRightPose,

    /// <summary>A left-facing compact posture transition that ends in <see cref="DraygonHeldBallLeftPose"/>.</summary>
    DraygonHeldMorphingLeftPose,

    /// <summary>A right-facing posture transition at standing height that ends standing facing right.</summary>
    CompactStandingUpRightPose,

    /// <summary>A left-facing posture transition at standing height that ends grabbed by Draygon facing left.</summary>
    DraygonHeldStandingUpLeftPose,

    /// <summary>A left-facing compact ball Draygon holds; Up stands it up through <see cref="DraygonHeldStandingUpLeftPose"/>.</summary>
    DraygonHeldBallLeftPose,

    /// <summary>Right-facing drained crouch/fall animation.</summary>
    DrainedCrouchingRightPose,

    /// <summary>Left-facing drained crouch/fall animation.</summary>
    DrainedCrouchingLeftPose,

    /// <summary>Right-facing drained standing animation.</summary>
    DrainedStandingRightPose,

    /// <summary>Left-facing drained standing animation.</summary>
    DrainedStandingLeftPose,

    /// <summary>Facing right after forward running collides with a wall.</summary>
    RanIntoWallRightPose,

    /// <summary>Facing-left mirror of <see cref="SamusPoseId.RanIntoWallRightPose"/>.</summary>
    RanIntoWallLeftPose,

    /// <summary>Right-facing wall-stop art aimed diagonally up-right.</summary>
    RanIntoWallAimUpRightPose,

    /// <summary>Left-facing wall-stop art aimed diagonally up-left.</summary>
    RanIntoWallAimUpLeftPose,

    /// <summary>Right-facing wall-stop art aimed diagonally down-right.</summary>
    RanIntoWallAimDownRightPose,

    /// <summary>Left-facing wall-stop art aimed diagonally down-left.</summary>
    RanIntoWallAimDownLeftPose,

    /// <summary>This pose turns a right-facing grounded Samus toward the left.</summary>
    TurningRightToLeftPose,

    /// <summary>This pose turns a left-facing grounded Samus toward the right.</summary>
    TurningLeftToRightPose,

    /// <summary>This pose is the ordinary right-facing spin jump.</summary>
    SpinJumpRightPose,

    /// <summary>This pose is the ordinary left-facing spin jump.</summary>
    SpinJumpLeftPose,

    /// <summary>
    /// This pose is the right-facing Space Jump body. The equipment-aware movement-type
    /// initializer substitutes this for the transition table's ordinary
    /// spin-jump target when Space Jump is equipped and Screw Attack is not.
    /// </summary>
    SpaceJumpRightPose,

    /// <summary>This pose is the left-facing mirror of <see cref="SpaceJumpRightPose"/>.</summary>
    SpaceJumpLeftPose,

    /// <summary>
    /// This pose is the right-facing Screw Attack body. The Screw Attack bit has
    /// priority over the Space Jump bit in the spin-pose selector.
    /// </summary>
    ScrewAttackRightPose,

    /// <summary>This pose is the left-facing mirror of <see cref="ScrewAttackRightPose"/>.</summary>
    ScrewAttackLeftPose,

    /// <summary>This pose turns a right-facing normal jump toward the left.</summary>
    TurningRightToLeftJumpPose,

    /// <summary>This pose turns a left-facing normal jump toward the right.</summary>
    TurningLeftToRightJumpPose,

    /// <summary>This pose is the right-facing wall-jump launch animation.</summary>
    WallJumpRightPose,

    /// <summary>This pose is the left-facing wall-jump launch animation.</summary>
    WallJumpLeftPose,

    /// <summary>This pose is the right-facing airborne grapple-swing body.</summary>
    GrappleSwingRightPose,

    /// <summary>This pose is the left-facing airborne grapple-swing body.</summary>
    GrappleSwingLeftPose,

    /// <summary>This pose is the standing right-facing horizontal grapple lock.</summary>
    GrappleStandingRightPose,

    /// <summary>This pose is the standing left-facing horizontal grapple lock.</summary>
    GrappleStandingLeftPose,

    /// <summary>This pose is the standing right-facing down-right grapple lock.</summary>
    GrappleStandingDownRightPose,

    /// <summary>This pose is the standing left-facing down-left grapple lock.</summary>
    GrappleStandingDownLeftPose,

    /// <summary>This pose is the crouched right-facing horizontal grapple lock.</summary>
    GrappleCrouchingRightPose,

    /// <summary>This pose is the crouched left-facing horizontal grapple lock.</summary>
    GrappleCrouchingLeftPose,

    /// <summary>This pose is the crouched right-facing down-right grapple lock.</summary>
    GrappleCrouchingDownRightPose,

    /// <summary>This pose is the crouched left-facing down-left grapple lock.</summary>
    GrappleCrouchingDownLeftPose,

    /// <summary>This pose is the left-wall grapple-jump contact body.</summary>
    GrappleWallContactLeftPose,

    /// <summary>This pose is the right-wall grapple-jump contact body.</summary>
    GrappleWallContactRightPose,

    /// <summary>Left-facing neutral body held in Draygon's claws.</summary>
    DraygonGrabbedNeutralLeftPose,

    /// <summary>Left-facing Draygon-held body aiming diagonally upward.</summary>
    DraygonGrabbedAimUpLeftPose,

    /// <summary>Left-facing Draygon-held firing body.</summary>
    DraygonGrabbedFiringLeftPose,

    /// <summary>Left-facing Draygon-held body aiming diagonally downward.</summary>
    DraygonGrabbedAimDownLeftPose,

    /// <summary>Left-facing six-frame struggling animation while held by Draygon.</summary>
    DraygonGrabbedMovingLeftPose,

    /// <summary>Right-facing neutral body held in Draygon's claws.</summary>
    DraygonGrabbedNeutralRightPose,

    /// <summary>Right-facing Draygon-held body aiming diagonally upward.</summary>
    DraygonGrabbedAimUpRightPose,

    /// <summary>Right-facing Draygon-held firing body.</summary>
    DraygonGrabbedFiringRightPose,

    /// <summary>Right-facing Draygon-held body aiming diagonally downward.</summary>
    DraygonGrabbedAimDownRightPose,

    /// <summary>Right-facing six-frame struggling animation while held by Draygon.</summary>
    DraygonGrabbedMovingRightPose,

    /// <summary>
    /// This pose is the visually left-facing damage boost; its facing is the deliberately
    /// reversed right, which drives the boost to the right.
    /// </summary>
    DamageBoostLeftPose,

    /// <summary>
    /// This pose is the visually right-facing damage boost; its facing is the deliberately
    /// reversed left, which drives the boost to the left.
    /// </summary>
    DamageBoostRightPose,

    /// <summary>This pose is the right-facing normal knockback body.</summary>
    KnockbackRightPose,

    /// <summary>This pose is the left-facing normal knockback body.</summary>
    KnockbackLeftPose,

    /// <summary>This pose turns a right-facing fall toward the left.</summary>
    TurningRightToLeftFallingPose,

    /// <summary>This pose turns a left-facing fall toward the right.</summary>
    TurningLeftToRightFallingPose,

    /// <summary>This pose turns right-to-left in a normal jump while retaining straight-up aim.</summary>
    TurningRightToLeftJumpAimUpPose,
    /// <summary>This pose turns left-to-right in a normal jump while retaining straight-up aim.</summary>
    TurningLeftToRightJumpAimUpPose,
    /// <summary>This pose turns right-to-left in a normal jump while retaining downward or down-right aim.</summary>
    TurningRightToLeftJumpAimDownPose,
    /// <summary>This pose turns left-to-right in a normal jump while retaining downward or down-left aim.</summary>
    TurningLeftToRightJumpAimDownPose,
    /// <summary>This pose turns right-to-left during a fall while retaining straight-up aim.</summary>
    TurningRightToLeftFallingAimUpPose,
    /// <summary>This pose turns left-to-right during a fall while retaining straight-up aim.</summary>
    TurningLeftToRightFallingAimUpPose,
    /// <summary>This pose turns right-to-left during a fall while retaining downward or down-right aim.</summary>
    TurningRightToLeftFallingAimDownPose,
    /// <summary>This pose turns left-to-right during a fall while retaining downward or down-left aim.</summary>
    TurningLeftToRightFallingAimDownPose,
    /// <summary>This pose turns right-to-left in a normal jump while retaining diagonal-up aim.</summary>
    TurningRightToLeftJumpAimDiagonalUpPose,
    /// <summary>This pose turns left-to-right in a normal jump while retaining diagonal-up aim.</summary>
    TurningLeftToRightJumpAimDiagonalUpPose,
    /// <summary>This pose turns right-to-left during a fall while retaining diagonal-up aim.</summary>
    TurningRightToLeftFallingAimDiagonalUpPose,
    /// <summary>This pose turns left-to-right during a fall while retaining diagonal-up aim.</summary>
    TurningLeftToRightFallingAimDiagonalUpPose,

    /// <summary>This pose is the stationary right-facing ordinary morph ball on the ground.</summary>
    MorphBallGroundRightPose,

    /// <summary>This pose is the ordinary morph ball rolling to the right on the ground.</summary>
    MorphBallMovingRightPose,

    /// <summary>This pose is the ordinary morph ball rolling to the left on the ground.</summary>
    MorphBallMovingLeftPose,

    /// <summary>This pose is the right-facing ordinary morph ball in the air.</summary>
    MorphBallFallingRightPose,

    /// <summary>This pose is the left-facing ordinary morph ball in the air.</summary>
    MorphBallFallingLeftPose,

    /// <summary>
    /// This pose is the unused right-facing body selected only by the unused glitch-ball movement type's
    /// otherwise-unused knockback transition. Naming it keeps the exhaustive
    /// pose table executable without pretending the retail game reaches it.
    /// </summary>
    UnusedKnockbackRightPose,

    /// <summary>Left-facing partner of <see cref="UnusedKnockbackRightPose"/>.</summary>
    UnusedKnockbackLeftPose,

    /// <summary>This pose is the right-facing crouch-to-morph transition.</summary>
    MorphingTransitionRightPose,

    /// <summary>This pose is the left-facing crouch-to-morph transition.</summary>
    MorphingTransitionLeftPose,

    /// <summary>This pose is the right-facing morph-to-crouch transition.</summary>
    UnmorphingTransitionRightPose,

    /// <summary>This pose is the left-facing morph-to-crouch transition.</summary>
    UnmorphingTransitionLeftPose,

    /// <summary>This pose is the stationary left-facing ordinary morph ball on the ground.</summary>
    MorphBallGroundLeftPose,

    /// <summary>This pose is the stationary right-facing Spring Ball on the ground.</summary>
    SpringBallGroundRightPose,

    /// <summary>This pose is the stationary left-facing Spring Ball on the ground.</summary>
    SpringBallGroundLeftPose,

    /// <summary>This pose is the Spring Ball moving right on the ground.</summary>
    SpringBallMovingRightPose,

    /// <summary>This pose is the Spring Ball moving left on the ground.</summary>
    SpringBallMovingLeftPose,

    /// <summary>This pose is the right-facing Spring Ball falling or bouncing.</summary>
    SpringBallFallingRightPose,

    /// <summary>This pose is the left-facing Spring Ball falling or bouncing.</summary>
    SpringBallFallingLeftPose,

    /// <summary>This pose is the right-facing Spring Ball in its powered jump.</summary>
    SpringBallJumpRightPose,

    /// <summary>This pose is the left-facing Spring Ball in its powered jump.</summary>
    SpringBallJumpLeftPose,

    /// <summary>This pose is the unaimed right-facing falling pose.</summary>
    FallingRightPose,

    /// <summary>This pose is the unaimed left-facing falling pose.</summary>
    FallingLeftPose,

    /// <summary>
    /// This pose is the right-facing falling body with the cannon held horizontally after
    /// firing. Its delay stream has the same terminal-velocity split as <see cref="FallingRightPose"/>.
    /// </summary>
    FallingGunExtendedRightPose,

    /// <summary>This pose is the left-facing mirror of <see cref="FallingGunExtendedRightPose"/>.</summary>
    FallingGunExtendedLeftPose,

    /// <summary>This pose is stationary, facing right, with the arm cannon aimed straight up.</summary>
    StandingAimUpRightPose,

    /// <summary>This pose is stationary, facing left, with the arm cannon aimed straight up.</summary>
    StandingAimUpLeftPose,

    /// <summary>This pose is stationary, facing right, and aiming diagonally up-right.</summary>
    StandingAimDiagonalUpRightPose,

    /// <summary>This pose is stationary, facing left, and aiming diagonally up-left.</summary>
    StandingAimDiagonalUpLeftPose,

    /// <summary>This pose is stationary, facing right, and aiming diagonally down-right.</summary>
    StandingAimDiagonalDownRightPose,

    /// <summary>This pose is stationary, facing left, and aiming diagonally down-left.</summary>
    StandingAimDiagonalDownLeftPose,

    /// <summary>This pose is the unused right-moving straight-up aim record.</summary>
    RunningAimUpRightPose,

    /// <summary>This pose is the unused left-moving straight-up aim record.</summary>
    RunningAimUpLeftPose,

    /// <summary>This pose moves right while aiming diagonally up-right.</summary>
    RunningAimDiagonalUpRightPose,

    /// <summary>This pose moves left while aiming diagonally up-left.</summary>
    RunningAimDiagonalUpLeftPose,

    /// <summary>This pose moves right while aiming diagonally down-right.</summary>
    RunningAimDiagonalDownRightPose,

    /// <summary>This pose moves left while aiming diagonally down-left.</summary>
    RunningAimDiagonalDownLeftPose,

    /// <summary>
    /// This pose is a stationary right-facing normal jump with the cannon extended. Unlike
    /// <see cref="NeutralJumpRightPose"/>, its pose table deliberately stores no no-input fallback, so the firing body can
    /// remain active until another input-table record or landing replaces it.
    /// </summary>
    NormalJumpGunExtendedRightPose,

    /// <summary>This pose is the left-facing mirror of <see cref="NormalJumpGunExtendedRightPose"/>.</summary>
    NormalJumpGunExtendedLeftPose,

    /// <summary>This pose is a right-facing normal jump aimed straight up.</summary>
    NormalJumpAimUpRightPose,

    /// <summary>This pose is a left-facing normal jump aimed straight up.</summary>
    NormalJumpAimUpLeftPose,

    /// <summary>This pose is the compact right-facing normal jump aimed straight down.</summary>
    NormalJumpAimDownRightPose,

    /// <summary>This pose is the compact left-facing normal jump aimed straight down.</summary>
    NormalJumpAimDownLeftPose,

    /// <summary>This pose is a right-facing normal jump using the moving-forward art.</summary>
    NormalJumpForwardRightPose,

    /// <summary>This pose is a left-facing normal jump using the moving-forward art.</summary>
    NormalJumpForwardLeftPose,

    /// <summary>This pose is the right-facing jump transition aimed straight up.</summary>
    NormalJumpTransitionAimUpRightPose,

    /// <summary>This pose is the left-facing jump transition aimed straight up.</summary>
    NormalJumpTransitionAimUpLeftPose,

    /// <summary>This pose is the right-facing jump transition aimed diagonally up.</summary>
    NormalJumpTransitionAimDiagonalUpRightPose,

    /// <summary>This pose is the left-facing jump transition aimed diagonally up.</summary>
    NormalJumpTransitionAimDiagonalUpLeftPose,

    /// <summary>This pose is the right-facing jump transition aimed diagonally down.</summary>
    NormalJumpTransitionAimDiagonalDownRightPose,

    /// <summary>This pose is the left-facing jump transition aimed diagonally down.</summary>
    NormalJumpTransitionAimDiagonalDownLeftPose,

    /// <summary>This pose is a right-facing normal jump aimed diagonally up.</summary>
    NormalJumpAimDiagonalUpRightPose,

    /// <summary>This pose is a left-facing normal jump aimed diagonally up.</summary>
    NormalJumpAimDiagonalUpLeftPose,

    /// <summary>This pose is a right-facing normal jump aimed diagonally down.</summary>
    NormalJumpAimDiagonalDownRightPose,

    /// <summary>This pose is a left-facing normal jump aimed diagonally down.</summary>
    NormalJumpAimDiagonalDownLeftPose,

    /// <summary>This pose is right-facing falling aimed straight up.</summary>
    FallingAimUpRightPose,

    /// <summary>This pose is left-facing falling aimed straight up.</summary>
    FallingAimUpLeftPose,

    /// <summary>This pose is the compact right-facing fall aimed straight down.</summary>
    FallingAimDownRightPose,

    /// <summary>This pose is the compact left-facing fall aimed straight down.</summary>
    FallingAimDownLeftPose,

    /// <summary>This pose is right-facing falling aimed diagonally up.</summary>
    FallingAimDiagonalUpRightPose,

    /// <summary>This pose is left-facing falling aimed diagonally up.</summary>
    FallingAimDiagonalUpLeftPose,

    /// <summary>This pose is right-facing falling aimed diagonally down.</summary>
    FallingAimDiagonalDownRightPose,

    /// <summary>This pose is left-facing falling aimed diagonally down.</summary>
    FallingAimDiagonalDownLeftPose,

    /// <summary>This pose is right-facing crouching aimed diagonally up.</summary>
    CrouchingAimDiagonalUpRightPose,

    /// <summary>This pose is left-facing crouching aimed diagonally up.</summary>
    CrouchingAimDiagonalUpLeftPose,

    /// <summary>This pose is right-facing crouching aimed diagonally down.</summary>
    CrouchingAimDiagonalDownRightPose,

    /// <summary>This pose is left-facing crouching aimed diagonally down.</summary>
    CrouchingAimDiagonalDownLeftPose,

    /// <summary>This pose is right-facing crouching aimed straight up.</summary>
    CrouchingAimUpRightPose,

    /// <summary>This pose is left-facing crouching aimed straight up.</summary>
    CrouchingAimUpLeftPose,

    /// <summary>This pose turns right-to-left while crouched without retaining aim.</summary>
    TurningRightToLeftCrouchingPose,

    /// <summary>This pose turns left-to-right while crouched without retaining aim.</summary>
    TurningLeftToRightCrouchingPose,

    /// <summary>This pose turns right-to-left on ground while preserving straight-up aim.</summary>
    TurningRightToLeftAimUpPose,

    /// <summary>This pose turns left-to-right on ground while preserving straight-up aim.</summary>
    TurningLeftToRightAimUpPose,

    /// <summary>This pose turns right-to-left on ground while preserving diagonal-down aim.</summary>
    TurningRightToLeftAimDiagonalDownPose,

    /// <summary>This pose turns left-to-right on ground while preserving diagonal-down aim.</summary>
    TurningLeftToRightAimDiagonalDownPose,

    /// <summary>This pose turns right-to-left on ground while preserving diagonal-up aim.</summary>
    TurningRightToLeftAimDiagonalUpPose,

    /// <summary>This pose turns left-to-right on ground while preserving diagonal-up aim.</summary>
    TurningLeftToRightAimDiagonalUpPose,

    /// <summary>This pose turns right-to-left while crouched and preserving straight-up aim.</summary>
    TurningRightToLeftCrouchingAimUpPose,

    /// <summary>This pose turns left-to-right while crouched and preserving straight-up aim.</summary>
    TurningLeftToRightCrouchingAimUpPose,

    /// <summary>This pose turns right-to-left while crouched and preserving diagonal-down aim.</summary>
    TurningRightToLeftCrouchingAimDiagonalDownPose,

    /// <summary>This pose turns left-to-right while crouched and preserving diagonal-down aim.</summary>
    TurningLeftToRightCrouchingAimDiagonalDownPose,

    /// <summary>This pose turns right-to-left while crouched and preserving diagonal-up aim.</summary>
    TurningRightToLeftCrouchingAimDiagonalUpPose,

    /// <summary>This pose turns left-to-right while crouched and preserving diagonal-up aim.</summary>
    TurningLeftToRightCrouchingAimDiagonalUpPose,

    /// <summary>This pose is a right-facing normal-jump landing aimed straight up.</summary>
    LandingAimUpRightPose,

    /// <summary>This pose is a left-facing normal-jump landing aimed straight up.</summary>
    LandingAimUpLeftPose,

    /// <summary>This pose is a right-facing normal-jump landing aimed diagonally up.</summary>
    LandingAimDiagonalUpRightPose,

    /// <summary>This pose is a left-facing normal-jump landing aimed diagonally up.</summary>
    LandingAimDiagonalUpLeftPose,

    /// <summary>This pose is a right-facing normal-jump landing aimed diagonally down.</summary>
    LandingAimDiagonalDownRightPose,

    /// <summary>This pose is a left-facing normal-jump landing aimed diagonally down.</summary>
    LandingAimDiagonalDownLeftPose,

    /// <summary>
    /// This pose is the right-facing horizontal-fire landing selected by the landing transition only
    /// when the current shot direction is two and the Shot binding is still held at impact.
    /// Its terminal pose-transition command returns to ordinary standing after the landing frames.
    /// </summary>
    FiringLandingRightPose,

    /// <summary>This pose is the left-facing mirror of <see cref="FiringLandingRightPose"/>.</summary>
    FiringLandingLeftPose,

    /// <summary>This pose transitions right-facing standing to crouching while aiming up.</summary>
    CrouchingTransitionAimUpRightPose,

    /// <summary>This pose transitions left-facing standing to crouching while aiming up.</summary>
    CrouchingTransitionAimUpLeftPose,

    /// <summary>This pose transitions right-facing standing to crouching while aiming diagonally up.</summary>
    CrouchingTransitionAimDiagonalUpRightPose,

    /// <summary>This pose transitions left-facing standing to crouching while aiming diagonally up.</summary>
    CrouchingTransitionAimDiagonalUpLeftPose,

    /// <summary>This pose transitions right-facing standing to crouching while aiming diagonally down.</summary>
    CrouchingTransitionAimDiagonalDownRightPose,

    /// <summary>This pose transitions left-facing standing to crouching while aiming diagonally down.</summary>
    CrouchingTransitionAimDiagonalDownLeftPose,

    /// <summary>This pose transitions right-facing crouching to standing while aiming up.</summary>
    StandingTransitionAimUpRightPose,

    /// <summary>This pose transitions left-facing crouching to standing while aiming up.</summary>
    StandingTransitionAimUpLeftPose,

    /// <summary>This pose transitions right-facing crouching to standing while aiming diagonally up.</summary>
    StandingTransitionAimDiagonalUpRightPose,

    /// <summary>This pose transitions left-facing crouching to standing while aiming diagonally up.</summary>
    StandingTransitionAimDiagonalUpLeftPose,

    /// <summary>This pose transitions right-facing crouching to standing while aiming diagonally down.</summary>
    StandingTransitionAimDiagonalDownRightPose,

    /// <summary>This pose transitions left-facing crouching to standing while aiming diagonally down.</summary>
    StandingTransitionAimDiagonalDownLeftPose,

    /// <summary>This pose is ordinary right-facing crouching.</summary>
    CrouchingRightPose,

    /// <summary>This pose is ordinary left-facing crouching.</summary>
    CrouchingLeftPose,

    /// <summary>This pose is the right-facing standing-to-crouch transition.</summary>
    CrouchingTransitionRightPose,

    /// <summary>This pose is the left-facing standing-to-crouch transition.</summary>
    CrouchingTransitionLeftPose,

    /// <summary>This pose is the right-facing crouch-to-standing transition.</summary>
    StandingTransitionRightPose,

    /// <summary>This pose is the left-facing crouch-to-standing transition.</summary>
    StandingTransitionLeftPose,

    /// <summary>This pose is the one-frame right neutral-jump transition.</summary>
    NeutralJumpTransitionRightPose,

    /// <summary>This pose is the one-frame left neutral-jump transition.</summary>
    NeutralJumpTransitionLeftPose,

    /// <summary>This pose is the ordinary right-facing neutral jump.</summary>
    NeutralJumpRightPose,

    /// <summary>This pose is the ordinary left-facing neutral jump.</summary>
    NeutralJumpLeftPose,

    /// <summary>This pose lands facing right after a non-spinning jump or fall.</summary>
    NormalLandingRightPose,

    /// <summary>This pose lands facing left after a non-spinning jump or fall.</summary>
    NormalLandingLeftPose,

    /// <summary>This pose lands facing right after a spin or wall jump.</summary>
    SpinLandingRightPose,

    /// <summary>This pose lands facing left after a spin or wall jump.</summary>
    SpinLandingLeftPose,
    /// <summary>A right-facing ball with the glitch-ball movement; it takes no input and keeps its pose without one.</summary>
    GlitchBallRightPose,

    /// <summary>A right-facing posture transition that ends at once in <see cref="GlitchBallRightPose"/>.</summary>
    GlitchBallMorphingRightPose,

    /// <summary>A left-facing posture transition that ends at once in <see cref="GlitchBallLeftPose"/>.</summary>
    GlitchBallMorphingLeftPose,

    /// <summary>A right-facing posture transition that morphs at once into the grounded ball, or the Spring Ball.</summary>
    InstantMorphingRightPose,

    /// <summary>The left-facing partner of <see cref="InstantMorphingRightPose"/>.</summary>
    InstantMorphingLeftPose,

    /// <summary>The left-facing partner of <see cref="GlitchBallRightPose"/>.</summary>
    GlitchBallLeftPose,

    /// <summary>A right-facing compact airborne pose whose animation repeats and which keeps its pose without input.</summary>
    CompactAirborneRightPose,

    /// <summary>The left-facing partner of <see cref="CompactAirborneRightPose"/>.</summary>
    CompactAirborneLeftPose,

    /// <summary>A right-facing grappling pose aimed right; with no input it falls facing right with the gun extended.</summary>
    GrappleAirborneGunExtendedRightPose,

    /// <summary>A right-facing grappling pose aimed down; with no input it falls facing right aimed down.</summary>
    GrappleAirborneAimDownRightPose,

    /// <summary>A right-facing grappling pose aimed diagonally down; with no input it falls aimed diagonally down.</summary>
    GrappleAirborneAimDiagonalDownRightPose,

    /// <summary>A left-facing Draygon-held pose whose animation ends grabbed by Draygon facing left.</summary>
    DraygonGrabbingLeftPose,

    /// <summary>The first pose number past the definition table: its records read whatever follows the table.</summary>
    FirstUnauthoredPose,

    /// <summary>The second pose number past the definition table: its records read whatever follows the table.</summary>
    SecondUnauthoredPose,

    /// <summary>The last pose number, past the definition table: its records read whatever follows the table.</summary>
    LastUnauthoredPose,
}
