using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>A family of Samus's poses that gameplay or the import reads as one (<see cref="SamusPoseFamilyDefinitions.Members"/>).</summary>
internal enum SamusPoseFamily
{
    /// <summary>
    /// The four right-facing standing poses that move as <see cref="SamusMovementType.Standing"/>.
    /// </summary>
    RightFacingStanding,

    /// <summary>
    /// The two front-view poses selected when Samus faces forward. They face neither way and must never be
    /// guessed as left or right.
    /// </summary>
    ForwardFacing,

    /// <summary>
    /// The six ordinary-play bodies whose names explicitly say “gun extended.” The firing landings are kept
    /// separate because they are short transition art, not stable horizontal-fire bodies selected directly by an
    /// input transition.
    /// </summary>
    GunExtended,

    /// <summary>The three right-facing moonwalk poses that travel left.</summary>
    MoonwalkingFacingRight,

    /// <summary>All five right-facing Draygon-held poses.</summary>
    RightFacingDraygonGrabbed,

    /// <summary>
    /// The moonwalk turn jumps, the six grounded turn frames selected only when Jump begins a direction
    /// reversal from moonwalking. The predicates below group them by the facing they turn to.
    /// </summary>
    MoonwalkTurnToSpinJump,

    /// <summary>The moonwalk turn-jump family whose input transitions lead to right.</summary>
    MoonwalkTurnToSpinJumpRight,

    /// <summary>The standing, running, crouching, landing and ran-into-wall poses that carry aim metadata.</summary>
    GroundedAim,

    /// <summary>
    /// right-facing aimed normal-jump landing <see cref="SamusPoseId.LandingAimUpRightPose"/>, <see
    /// cref="SamusPoseId.LandingAimDiagonalUpRightPose"/> and <see
    /// cref="SamusPoseId.LandingAimDiagonalDownRightPose"/>.
    /// </summary>
    RightFacingAimedLanding,

    /// <summary>
    /// The complete right-facing landing set that shares the ordinary standing input transitions: normal,
    /// spin, aimed up, diagonally up and diagonally down, and firing. Every pose change that tests for a landing
    /// reads this set, so each of these landings keeps standing's crouch, turn, run and wall-probe exits.
    /// </summary>
    RightFacingLanding,

    /// <summary>
    /// The complete right-facing <see cref="SamusMovementType.Crouching"/> family. All its poses use the
    /// crouching radius and the shared input transitions.
    /// </summary>
    RightFacingCrouching,

    /// <summary>
    /// The grounded turning poses that began facing right and turn left. Each already faces left, the facing
    /// it turns to, so preserving Samus's old rightward momentum must use this grouping.
    /// </summary>
    RightToLeftGroundTurn,

    /// <summary>
    /// The six aimed crouched-turn poses moved by the turning-while-jumping movement type. The unaimed
    /// crouching turns are intentionally absent because the pose definitions assign those otherwise similar-looking
    /// turns to the grounded turning movement type.
    /// </summary>
    AimedCrouchingTurn,

    /// <summary>every turning-while-jumping normal-jump turn pose.</summary>
    JumpingTurn,

    /// <summary>every turning-while-falling falling-turn pose.</summary>
    FallingTurn,

    /// <summary>
    /// The eight jumping/falling turn poses that change from right to left, including their up, down, and
    /// diagonal-up aim variants. <param name="pose">Prospective or active turn pose; direction describes the facing
    /// change, not horizontal velocity.</param> <returns>Whether the pose is a right-to-left aerial turn admitted by
    /// the transition selector.</returns>
    /// </summary>
    RightToLeftAerialTurn,

    /// <summary>The four drained crouching and standing bodies that Mother Brain's attack puts her in.</summary>
    Drained,

    /// <summary>The two poses a hit knocks Samus back in, facing right and left.</summary>
    Knockback,

    /// <summary>
    /// The jump transitions, neutral and aimed up, diagonally up or diagonally down, facing either way: the
    /// normal-jumping poses a jump begins in before its launch body. They move Samus only by external displacement.
    /// </summary>
    JumpTransition,

    /// <summary>
    /// The jumps a stored shine starts a shinespark from: the neutral jumps and the normal jumps aimed up or
    /// diagonally up.
    /// </summary>
    StoredShineShinesparkStart,

    /// <summary>The two Screw Attack animation poses.</summary>
    ScrewAttack,

    /// <summary>The two Space Jump animation poses.</summary>
    SpaceJump,

    /// <summary>
    /// The crouching transitions, unaimed and aimed up, diagonally up or diagonally down, facing either way.
    /// </summary>
    CrouchingTransition,

    /// <summary>
    /// The standing transitions, unaimed and aimed up, diagonally up or diagonally down, facing either way.
    /// </summary>
    StandingTransition,

    /// <summary>The four ordinary, non-Spring-Ball grounded morph poses.</summary>
    GroundedMorphBall,

    /// <summary>The two ordinary, non-Spring-Ball airborne morph poses.</summary>
    AirborneMorphBall,

    /// <summary>The four grounded Spring Ball poses.</summary>
    GroundedSpringBall,

    /// <summary>Spring Ball jump/fall poses across the in-air and falling Spring Ball movement types.</summary>
    AirborneSpringBall,

    /// <summary>The four morph entry/exit poses handled by the posture-transition movement type.</summary>
    MorphTransition,

    /// <summary>The aimed normal-jump/falling poses, including compact Down aim.</summary>
    AimedAerial,

    /// <summary>
    /// The forward jumps, the two normal-jumping bodies used while Samus has a same-facing horizontal
    /// direction held. These are ordinary aerial poses, not aimed or spinning poses; naming the pair prevents their
    /// transition semantics from being hidden inside another aim-specific condition.
    /// </summary>
    ForwardMovingNormalJump,

    /// <summary>The compact straight-down aerial bodies.</summary>
    CompactAerial,

    /// <summary>The drained crouches, facing right and left, that the Baby holds Samus in.</summary>
    DrainedCrouch,

    /// <summary>The drained standing poses, facing right and left, the drained Samus stands up into.</summary>
    DrainedStanding,

    /// <summary>The death sequence's poses, facing right and left.</summary>
    DeathSequence,

    /// <summary>The vertical shinespark flights, facing right and left.</summary>
    ShinesparkVertical,

    /// <summary>The shinespark windups, facing right and left, which stop Samus's momentum.</summary>
    ShinesparkWindup,

    /// <summary>
    /// The two generic spin jumps, facing right and left, that Screw Attack and Space Jump replace when equipped.
    /// </summary>
    GenericSpinJump,

    /// <summary>The neutral jumps, facing right and left, whose rise holds its frame while Samus rises.</summary>
    NeutralJump,

    /// <summary>The neutral standing poses, facing right and left.</summary>
    NeutralStanding,

    /// <summary>
    /// The falls with the arm cannon forward or extended, facing right and left, whose animation has a fast-fall run.
    /// </summary>
    PlainFalling,

    /// <summary>The Grapple swings, facing right and left, which drop Samus standing.</summary>
    GrappleSwing,

    /// <summary>The poses Draygon's hold moves Samus in, which aim and fire Grapple by the hold's own rules.</summary>
    DraygonGrabbedMoving,

    /// <summary>
    /// The poses that show the chest cover between the body's halves: the unsuited Samus's front view, whose Power Suit's
    /// chest the cover hides.
    /// </summary>
    ChestCovered,

    /// <summary>
    /// The poses whose frames the angle, the Crystal Flash's stages and the Zebes start select directly, from outside their
    /// animation: the X-ray poses, the Grapple swings, the Crystal Flash and the front views.
    /// </summary>
    FramesSelectedDirectly,

    /// <summary>The spring ball's jumps, facing right and left.</summary>
    SpringBallJump,

    /// <summary>The crouches with the arm cannon forward, facing right and left, which a crouch jump lifts from.</summary>
    PlainCrouching,

    /// <summary>The poses Grapple's wall contact holds Samus in, either side.</summary>
    GrappleWallContact,

    /// <summary>The moonwalks aiming up, which fire from the running muzzle origins.</summary>
    MoonwalkAimUp,

    /// <summary>The still grounded balls, morph and spring, facing right and left.</summary>
    StationaryBall,

    /// <summary>The grounded balls a Chozo statue's hand admits: the morph ball facing right and the spring balls, but the morph ball facing left.</summary>
    ChozoHandBall,

    /// <summary>The jumps a left-facing damage boost turns into.</summary>
    DamageBoostJumpLeft,

    /// <summary>The jumps a right-facing damage boost turns into.</summary>
    DamageBoostJumpRight,

    /// <summary>The falls with the arm cannon forward, facing right and left.</summary>
    PlainFallingOnly,

    /// <summary>
    /// The grounded balls a Torizo behind Samus sits down for: the grounded morph and spring balls, but the left-facing
    /// still morph ball, which the cartridge's list leaves out, so a Torizo never sits for it.
    /// </summary>
    TorizoSitDownBall,

    /// <summary>
    /// The jumps a turn's pose input can launch, which survive the turn's final frame: the neutral jump's transitions and the
    /// generic spin jumps.
    /// </summary>
    JumpLaunch,

    /// <summary>The neutral jump's transitions, facing right and left.</summary>
    NeutralJumpTransition,

    /// <summary>The normal runs, facing right and left.</summary>
    NormalRun,

    /// <summary>The standing aims up a ran-into-wall route admits as its wall stop, facing right and left.</summary>
    AimUpWallStop,

    /// <summary>The standing ground turns with the arm cannon forward, either way.</summary>
    PlainGroundTurn,

    /// <summary>The crouching ground turns with the arm cannon forward, either way.</summary>
    PlainCrouchingTurn,

    /// <summary>The posture transitions that hold Samus where her installation put her rather than moving her: Draygon's stand-up.</summary>
    HeldInPlaceStandUp,
}
