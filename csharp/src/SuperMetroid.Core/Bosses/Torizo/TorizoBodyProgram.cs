namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's typed body programs (#641), one per list and per segment a jump, call,
/// link or resumed sleep enters, in authored order. Generated from the verified lists; see
/// <see cref="TorizoAnimations"/>.
/// </summary>
internal enum TorizoBodyProgram
{
    /// <summary>Torizo animation script: special callable, blow up Bomb torizos gut.</summary>
    SpecialCallableBlowUpBombTorizosGut,
    /// <summary>Torizo animation script: callable, blow up Bomb torizos face.</summary>
    CallableBlowUpBombTorizosFace,
    /// <summary>Torizo animation script: death sequence, part 0.</summary>
    DeathSequence0,
    /// <summary>Torizo animation script: death sequence, part 1.</summary>
    DeathSequence1,
    /// <summary>Torizo animation script: death sequence, part 2.</summary>
    DeathSequence2,
    /// <summary>Torizo animation script: Bomb Torizo, initial, part 0.</summary>
    BombTorizoInitial0,
    /// <summary>The code after the preceding list's sleep, which the AI resumes.</summary>
    BombTorizoInitial0Resumed,
    /// <summary>Torizo animation script: Bomb Torizo, initial, part 1.</summary>
    BombTorizoInitial1,
    /// <summary>Torizo animation script: Bomb Torizo, initial, part 2.</summary>
    BombTorizoInitial2,
    /// <summary>Torizo animation script: facing left, turning left.</summary>
    FacingLeftTurningLeft,
    /// <summary>Torizo animation script: facing left, walking, right leg moving.</summary>
    FacingLeftWalkingRightLegMoving,
    /// <summary>Torizo animation script: facing left, walking, left leg moving.</summary>
    FacingLeftWalkingLeftLegMoving,
    /// <summary>Torizo animation script: facing left, spewing Chozo orbs, right foot fwd, part 0.</summary>
    FacingLeftSpewingChozoOrbsRightFootFwd0,
    /// <summary>Torizo animation script: facing left, spewing Chozo orbs, right foot fwd, part 1.</summary>
    FacingLeftSpewingChozoOrbsRightFootFwd1,
    /// <summary>Torizo animation script: facing left, spewing Chozo orbs, left foot fwd, part 0.</summary>
    FacingLeftSpewingChozoOrbsLeftFootFwd0,
    /// <summary>Torizo animation script: facing left, spewing Chozo orbs, left foot fwd, part 1.</summary>
    FacingLeftSpewingChozoOrbsLeftFootFwd1,
    /// <summary>Torizo animation script: facing left, sonic booms, right foot forward, part 0.</summary>
    FacingLeftSonicBoomsRightFootForward0,
    /// <summary>Torizo animation script: facing left, sonic booms, right foot forward, part 1.</summary>
    FacingLeftSonicBoomsRightFootForward1,
    /// <summary>Torizo animation script: facing left, sonic booms, left foot forward, part 0.</summary>
    FacingLeftSonicBoomsLeftFootForward0,
    /// <summary>Torizo animation script: facing left, sonic booms, left foot forward, part 1.</summary>
    FacingLeftSonicBoomsLeftFootForward1,
    /// <summary>Torizo animation script: facing left, explosive swipe, right foot forward.</summary>
    FacingLeftExplosiveSwipeRightFootForward,
    /// <summary>Torizo animation script: facing left, explosive swipe, left foot forward.</summary>
    FacingLeftExplosiveSwipeLeftFootForward,
    /// <summary>Torizo animation script: facing left, jumping forwards, part 0.</summary>
    FacingLeftJumpingForwards0,
    /// <summary>Torizo animation script: facing left, jumping forwards, part 1.</summary>
    FacingLeftJumpingForwards1,
    /// <summary>Torizo animation script: facing left, falling, part 0.</summary>
    FacingLeftFalling0,
    /// <summary>Torizo animation script: facing left, falling, part 1.</summary>
    FacingLeftFalling1,
    /// <summary>Torizo animation script: facing left, falling, part 2.</summary>
    FacingLeftFalling2,
    /// <summary>Torizo animation script: facing left, jumping backward, land left foot fwd, part 0.</summary>
    FacingLeftJumpingBackwardLandLeftFootFwd0,
    /// <summary>Torizo animation script: facing left, jumping backward, land left foot fwd, part 1.</summary>
    FacingLeftJumpingBackwardLandLeftFootFwd1,
    /// <summary>Torizo animation script: facing left, jumping backward, land left foot fwd, part 2.</summary>
    FacingLeftJumpingBackwardLandLeftFootFwd2,
    /// <summary>Torizo animation script: facing left, jumping backward, land left foot fwd, part 3.</summary>
    FacingLeftJumpingBackwardLandLeftFootFwd3,
    /// <summary>Torizo animation script: facing left, jumping backward, land left foot fwd, part 4.</summary>
    FacingLeftJumpingBackwardLandLeftFootFwd4,
    /// <summary>Torizo animation script: facing left, jumping backward, right foot fwd, part 0.</summary>
    FacingLeftJumpingBackwardRightFootFwd0,
    /// <summary>Torizo animation script: facing left, jumping backward, right foot fwd, part 1.</summary>
    FacingLeftJumpingBackwardRightFootFwd1,
    /// <summary>Torizo animation script: facing left, jumping backward, right foot fwd, part 2.</summary>
    FacingLeftJumpingBackwardRightFootFwd2,
    /// <summary>Torizo animation script: facing left, jumping backward, right foot fwd, part 3.</summary>
    FacingLeftJumpingBackwardRightFootFwd3,
    /// <summary>Torizo animation script: facing left, jumping backward, right foot fwd, part 4.</summary>
    FacingLeftJumpingBackwardRightFootFwd4,
    /// <summary>Torizo animation script: facing left, faceless, turning left.</summary>
    FacingLeftFacelessTurningLeft,
    /// <summary>Torizo animation script: facing left, faceless, walking, right leg moving.</summary>
    FacingLeftFacelessWalkingRightLegMoving,
    /// <summary>Torizo animation script: facing left, faceless, walking, left leg moving.</summary>
    FacingLeftFacelessWalkingLeftLegMoving,
    /// <summary>An unnamed animation entry.</summary>
    FacingLeftFacelessWalkingLeftLegMovingTrailing,
    /// <summary>The code after the preceding list's sleep, which the AI resumes.</summary>
    FacingLeftFacelessWalkingLeftLegMovingTrailingResumed,
    /// <summary>Torizo animation script: facing right, turning right.</summary>
    FacingRightTurningRight,
    /// <summary>Torizo animation script: facing right, walking, left leg moving.</summary>
    FacingRightWalkingLeftLegMoving,
    /// <summary>Torizo animation script: facing right, walking, right leg moving.</summary>
    FacingRightWalkingRightLegMoving,
    /// <summary>Torizo animation script: facing right, spewing Chozo orbs, left foot fwd, part 0.</summary>
    FacingRightSpewingChozoOrbsLeftFootFwd0,
    /// <summary>Torizo animation script: facing right, spewing Chozo orbs, left foot fwd, part 1.</summary>
    FacingRightSpewingChozoOrbsLeftFootFwd1,
    /// <summary>Torizo animation script: facing right, spewing Chozo orbs, right foot fwd, part 0.</summary>
    FacingRightSpewingChozoOrbsRightFootFwd0,
    /// <summary>Torizo animation script: facing right, spewing Chozo orbs, right foot fwd, part 1.</summary>
    FacingRightSpewingChozoOrbsRightFootFwd1,
    /// <summary>Torizo animation script: facing right, sonic booms, left foot forward, part 0.</summary>
    FacingRightSonicBoomsLeftFootForward0,
    /// <summary>Torizo animation script: facing right, sonic booms, left foot forward, part 1.</summary>
    FacingRightSonicBoomsLeftFootForward1,
    /// <summary>Torizo animation script: facing right, sonic booms, right foot forward, part 0.</summary>
    FacingRightSonicBoomsRightFootForward0,
    /// <summary>Torizo animation script: facing right, sonic booms, right foot forward, part 1.</summary>
    FacingRightSonicBoomsRightFootForward1,
    /// <summary>Torizo animation script: facing right, explosive swipe, left foot forward.</summary>
    FacingRightExplosiveSwipeLeftFootForward,
    /// <summary>Torizo animation script: facing right, explosive swipe, right foot forward.</summary>
    FacingRightExplosiveSwipeRightFootForward,
    /// <summary>Torizo animation script: facing right, jumping forwards, part 0.</summary>
    FacingRightJumpingForwards0,
    /// <summary>Torizo animation script: facing right, jumping forwards, part 1.</summary>
    FacingRightJumpingForwards1,
    /// <summary>Torizo animation script: facing right, falling, part 0.</summary>
    FacingRightFalling0,
    /// <summary>Torizo animation script: facing right, falling, part 1.</summary>
    FacingRightFalling1,
    /// <summary>Torizo animation script: facing right, falling, part 2.</summary>
    FacingRightFalling2,
    /// <summary>Torizo animation script: facing right, jump backward, land right foot fwd, part 0.</summary>
    FacingRightJumpBackwardLandRightFootFwd0,
    /// <summary>Torizo animation script: facing right, jump backward, land right foot fwd, part 1.</summary>
    FacingRightJumpBackwardLandRightFootFwd1,
    /// <summary>Torizo animation script: facing right, jump backward, land right foot fwd, part 2.</summary>
    FacingRightJumpBackwardLandRightFootFwd2,
    /// <summary>Torizo animation script: facing right, jump backward, land right foot fwd, part 3.</summary>
    FacingRightJumpBackwardLandRightFootFwd3,
    /// <summary>Torizo animation script: facing right, jump backward, land right foot fwd, part 4.</summary>
    FacingRightJumpBackwardLandRightFootFwd4,
    /// <summary>Torizo animation script: facing right, jump backwards, land left foot fwd, part 0.</summary>
    FacingRightJumpBackwardsLandLeftFootFwd0,
    /// <summary>Torizo animation script: facing right, jump backwards, land left foot fwd, part 1.</summary>
    FacingRightJumpBackwardsLandLeftFootFwd1,
    /// <summary>Torizo animation script: facing right, jump backwards, land left foot fwd, part 2.</summary>
    FacingRightJumpBackwardsLandLeftFootFwd2,
    /// <summary>Torizo animation script: facing right, jump backwards, land left foot fwd, part 3.</summary>
    FacingRightJumpBackwardsLandLeftFootFwd3,
    /// <summary>Torizo animation script: facing right, jump backwards, land left foot fwd, part 4.</summary>
    FacingRightJumpBackwardsLandLeftFootFwd4,
    /// <summary>Torizo animation script: facing right, faceless, turning right.</summary>
    FacingRightFacelessTurningRight,
    /// <summary>Torizo animation script: facing right, faceless, walking, left leg moving.</summary>
    FacingRightFacelessWalkingLeftLegMoving,
    /// <summary>Torizo animation script: facing right, faceless, walking, right leg moving.</summary>
    FacingRightFacelessWalkingRightLegMoving,
    /// <summary>Golden Torizo animation script: initial, part 0.</summary>
    GoldenInitial0,
    /// <summary>The code after the preceding list's sleep, which the AI resumes.</summary>
    GoldenInitial0Resumed,
    /// <summary>Golden Torizo animation script: initial, part 1.</summary>
    GoldenInitial1,
    /// <summary>Golden Torizo animation script: initial, part 2.</summary>
    GoldenInitial2,
    /// <summary>Golden Torizo animation script: initial, part 3.</summary>
    GoldenInitial3,
    /// <summary>Golden Torizo animation script: spew Chozo orbs, face left, right foot fwd, part 0.</summary>
    GoldenSpewChozoOrbsFaceLeftRightFootFwd0,
    /// <summary>Golden Torizo animation script: spew Chozo orbs, face left, right foot fwd, part 1.</summary>
    GoldenSpewChozoOrbsFaceLeftRightFootFwd1,
    /// <summary>Golden Torizo animation script: spew Chozo orbs, facing left, left foot fwd, part 0.</summary>
    GoldenSpewChozoOrbsFacingLeftLeftFootFwd0,
    /// <summary>Golden Torizo animation script: spew Chozo orbs, facing left, left foot fwd, part 1.</summary>
    GoldenSpewChozoOrbsFacingLeftLeftFootFwd1,
    /// <summary>Golden Torizo animation script: sonic booms, facing left, right foot fwd, part 0.</summary>
    GoldenSonicBoomsFacingLeftRightFootFwd0,
    /// <summary>Golden Torizo animation script: sonic booms, facing left, right foot fwd, part 1.</summary>
    GoldenSonicBoomsFacingLeftRightFootFwd1,
    /// <summary>Golden Torizo animation script: sonic booms, facing left, left foot fwd, part 0.</summary>
    GoldenSonicBoomsFacingLeftLeftFootFwd0,
    /// <summary>Golden Torizo animation script: sonic booms, facing left, left foot fwd, part 1.</summary>
    GoldenSonicBoomsFacingLeftLeftFootFwd1,
    /// <summary>Golden Torizo animation script: spew Chozo orb, facing left, left foot fwd, part 0.</summary>
    GoldenSpewChozoOrbFacingLeftLeftFootFwd0,
    /// <summary>Golden Torizo animation script: spew Chozo orb, facing left, left foot fwd, part 1.</summary>
    GoldenSpewChozoOrbFacingLeftLeftFootFwd1,
    /// <summary>Golden Torizo animation script: spew Chozo orb, facing left, right foot fwd, part 0.</summary>
    GoldenSpewChozoOrbFacingLeftRightFootFwd0,
    /// <summary>Golden Torizo animation script: spew Chozo orb, facing left, right foot fwd, part 1.</summary>
    GoldenSpewChozoOrbFacingLeftRightFootFwd1,
    /// <summary>Golden Torizo animation script: sonic booms, facing right, left foot fwd, part 0.</summary>
    GoldenSonicBoomsFacingRightLeftFootFwd0,
    /// <summary>Golden Torizo animation script: sonic booms, facing right, left foot fwd, part 1.</summary>
    GoldenSonicBoomsFacingRightLeftFootFwd1,
    /// <summary>Golden Torizo animation script: sonic booms, facing right, right foot fwd, part 0.</summary>
    GoldenSonicBoomsFacingRightRightFootFwd0,
    /// <summary>Golden Torizo animation script: sonic booms, facing right, right foot fwd, part 1.</summary>
    GoldenSonicBoomsFacingRightRightFootFwd1,
    /// <summary>Golden Torizo animation script: landed from backwards jump, facing left, left foot fwd.</summary>
    GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd,
    /// <summary>Golden Torizo animation script: landed from backwards jump, facing left, right foot fwd.</summary>
    GoldenLandedFromBackwardsJumpFacingLeftRightFootFwd,
    /// <summary>Golden Torizo animation script: landed from backwards jump, facing right, right foot fwd.</summary>
    GoldenLandedFromBackwardsJumpFacingRightRightFootFwd,
    /// <summary>Golden Torizo animation script: landed from backwards jump, facing right, left foot fwd.</summary>
    GoldenLandedFromBackwardsJumpFacingRightLeftFootFwd,
    /// <summary>Golden Torizo animation script: caught super, facing left, left leg fwd.</summary>
    GoldenCaughtSuperFacingLeftLeftLegFwd,
    /// <summary>Golden Torizo animation script: caught super, facing left, right leg fwd.</summary>
    GoldenCaughtSuperFacingLeftRightLegFwd,
    /// <summary>Golden Torizo animation script: caught super, facing right, right leg fwd.</summary>
    GoldenCaughtSuperFacingRightRightLegFwd,
    /// <summary>Golden Torizo animation script: caught super, facing right, left leg fwd.</summary>
    GoldenCaughtSuperFacingRightLeftLegFwd,
    /// <summary>Golden Torizo animation script: sit down attack, facing left.</summary>
    GoldenSitDownAttackFacingLeft,
    /// <summary>Golden Torizo animation script: sit down attack, facing right.</summary>
    GoldenSitDownAttackFacingRight,
    /// <summary>Golden Torizo animation script: release Golden Torizo eggs, part 0.</summary>
    GoldenReleaseGoldenTorizoEggs0,
    /// <summary>Golden Torizo animation script: release Golden Torizo eggs, part 1.</summary>
    GoldenReleaseGoldenTorizoEggs1,
    /// <summary>Golden Torizo animation script: release Golden Torizo eggs, part 2.</summary>
    GoldenReleaseGoldenTorizoEggs2,
    /// <summary>Golden Torizo animation script: eye beam attack, part 0.</summary>
    GoldenEyeBeamAttack0,
    /// <summary>Golden Torizo animation script: eye beam attack, part 1.</summary>
    GoldenEyeBeamAttack1,
    /// <summary>Golden Torizo animation script: eye beam attack, part 2.</summary>
    GoldenEyeBeamAttack2,
    /// <summary>Torizo animation script: stunned, part 0.</summary>
    Stunned0,
    /// <summary>Torizo animation script: stunned, part 1.</summary>
    Stunned1,
    /// <summary>Golden Torizo animation script: dodge, turning left.</summary>
    GoldenDodgeTurningLeft,
    /// <summary>Golden Torizo animation script: turning left.</summary>
    GoldenTurningLeft,
    /// <summary>Golden Torizo animation script: walking left, right leg moving.</summary>
    GoldenWalkingLeftRightLegMoving,
    /// <summary>Golden Torizo animation script: walking left, left leg moving.</summary>
    GoldenWalkingLeftLeftLegMoving,
    /// <summary>Golden Torizo animation script: dodge, turning right.</summary>
    GoldenDodgeTurningRight,
    /// <summary>Golden Torizo animation script: turning right.</summary>
    GoldenTurningRight,
    /// <summary>Golden Torizo animation script: walking right, left leg moving.</summary>
    GoldenWalkingRightLeftLegMoving,
    /// <summary>Golden Torizo animation script: walking right, right leg moving.</summary>
    GoldenWalkingRightRightLegMoving,
}
