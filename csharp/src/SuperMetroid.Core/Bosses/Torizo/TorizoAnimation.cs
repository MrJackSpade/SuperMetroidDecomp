namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's body animations, each named for what it shows (#641, #1788, #2235, #2411): the encounter's AI,
/// its movement, its reactions and its cues start them by name, and each plays the <see cref="TorizoBodySegment"/> runs
/// <see cref="TorizoAnimations.StartingAt"/> lists for it.
/// </summary>
internal enum TorizoAnimation
{
    /// <summary>Bomb Torizo's gut blows up; started by its movement once its health falls below the gut's threshold, it resumes the animation it interrupted.</summary>
    BombTorizoGutBlowingUp,
    /// <summary>Bomb Torizo's face blows up; started by its movement once its health falls below the face's threshold, it ends in the faceless turn away from the way it faces.</summary>
    BombTorizoFaceBlowingUp,
    /// <summary>Either Torizo's death: it goes idle and locks its animation, explodes eight times, fades to black while it flickers and explodes, and ends the fight.</summary>
    Dying,
    /// <summary>Bomb Torizo sits dormant, facing left, until the statue's hand is gone; its AI then wakes it into <see cref="BombTorizoAwakening"/>.</summary>
    BombTorizoDormant,
    /// <summary>Bomb Torizo wakes facing left: its eyes light up, flicker back on twice, it stands, its fight starts, and it walks left.</summary>
    BombTorizoAwakening,
    /// <summary>Golden Torizo, stunned, staggers; its eyes light up again twice, and it resumes the walk it interrupted.</summary>
    Stunned,
    /// <summary>Bomb Torizo turns to face left, then walks left, right leg first.</summary>
    TurningLeft,
    /// <summary>Bomb Torizo, facing left with its right foot forward, spews Chozo orbs three times, then walks left, left leg first.</summary>
    FacingLeftSpewingChozoOrbsRightFootForward,
    /// <summary>Bomb Torizo, facing left with its left foot forward, spews Chozo orbs three times, then walks left, right leg first.</summary>
    FacingLeftSpewingChozoOrbsLeftFootForward,
    /// <summary>Bomb Torizo, facing left with its right foot forward, throws sonic booms four times, then walks left, left leg first.</summary>
    FacingLeftSonicBoomsRightFootForward,
    /// <summary>Bomb Torizo, facing left with its left foot forward, throws sonic booms four times, then walks left, right leg first.</summary>
    FacingLeftSonicBoomsLeftFootForward,
    /// <summary>Torizo, facing left with its right foot forward, swipes behind it, then resumes the walk it interrupted.</summary>
    FacingLeftExplosiveSwipeRightFootForward,
    /// <summary>Torizo, facing left with its left foot forward, swipes behind it, then resumes the walk it interrupted.</summary>
    FacingLeftExplosiveSwipeLeftFootForward,
    /// <summary>Torizo jumps forward facing left and rises; once it no longer rises, it falls in <see cref="FacingLeftFalling"/>.</summary>
    FacingLeftJumpingForward,
    /// <summary>Torizo falls facing left, resting in its falling pose until its fall lands it in <see cref="FacingLeftLanding"/>.</summary>
    FacingLeftFalling,
    /// <summary>Torizo lands facing left, then walks left on: faceless or Golden Torizo in its own walk, Bomb Torizo left leg first.</summary>
    FacingLeftLanding,
    /// <summary>Torizo jumps backward facing left and rises; once it no longer rises, it falls in <see cref="FacingLeftFallingBackwardToLeftFoot"/>.</summary>
    FacingLeftJumpingBackwardToLeftFoot,
    /// <summary>Torizo falls from its backward jump facing left until it lands in <see cref="FacingLeftLandingBackwardOnLeftFoot"/>.</summary>
    FacingLeftFallingBackwardToLeftFoot,
    /// <summary>Torizo lands from its backward jump facing left with its left foot forward: faceless or Golden Torizo walks on, Bomb Torizo attacks, then walks left.</summary>
    FacingLeftLandingBackwardOnLeftFoot,
    /// <summary>Torizo jumps backward facing left and rises; once it no longer rises, it falls in <see cref="FacingLeftFallingBackwardToRightFoot"/>.</summary>
    FacingLeftJumpingBackwardToRightFoot,
    /// <summary>Torizo falls from its backward jump facing left until it lands in <see cref="FacingLeftLandingBackwardOnRightFoot"/>.</summary>
    FacingLeftFallingBackwardToRightFoot,
    /// <summary>Torizo lands from its backward jump facing left with its right foot forward: faceless or Golden Torizo walks on, Bomb Torizo attacks, then walks left.</summary>
    FacingLeftLandingBackwardOnRightFoot,
    /// <summary>Faceless Bomb Torizo turns to face left, then walks left, right leg first.</summary>
    FacelessTurningLeft,
    /// <summary>Faceless Bomb Torizo walks left, right leg first.</summary>
    FacingLeftFacelessWalkingRightLegMoving,
    /// <summary>Faceless Bomb Torizo walks left, left leg first.</summary>
    FacingLeftFacelessWalkingLeftLegMoving,
    /// <summary>Bomb Torizo turns to face right, then walks right, left leg first.</summary>
    TurningRight,
    /// <summary>Bomb Torizo, facing right with its left foot forward, spews Chozo orbs three times, then walks right, right leg first.</summary>
    FacingRightSpewingChozoOrbsLeftFootForward,
    /// <summary>Bomb Torizo, facing right with its right foot forward, spews Chozo orbs three times, then walks right, left leg first.</summary>
    FacingRightSpewingChozoOrbsRightFootForward,
    /// <summary>Bomb Torizo, facing right with its left foot forward, throws sonic booms four times, then walks right, right leg first.</summary>
    FacingRightSonicBoomsLeftFootForward,
    /// <summary>Bomb Torizo, facing right with its right foot forward, throws sonic booms four times, then walks right, left leg first.</summary>
    FacingRightSonicBoomsRightFootForward,
    /// <summary>Torizo, facing right with its left foot forward, swipes behind it, then resumes the walk it interrupted.</summary>
    FacingRightExplosiveSwipeLeftFootForward,
    /// <summary>Torizo, facing right with its right foot forward, swipes behind it, then resumes the walk it interrupted.</summary>
    FacingRightExplosiveSwipeRightFootForward,
    /// <summary>Torizo jumps forward facing right and rises; once it no longer rises, it falls in <see cref="FacingRightFalling"/>.</summary>
    FacingRightJumpingForward,
    /// <summary>Torizo falls facing right, resting in its falling pose until its fall lands it in <see cref="FacingRightLanding"/>.</summary>
    FacingRightFalling,
    /// <summary>Torizo lands facing right, then walks right on: faceless or Golden Torizo in its own walk, Bomb Torizo right leg first.</summary>
    FacingRightLanding,
    /// <summary>Torizo jumps backward facing right and rises; once it no longer rises, it falls in <see cref="FacingRightFallingBackwardToRightFoot"/>.</summary>
    FacingRightJumpingBackwardToRightFoot,
    /// <summary>Torizo falls from its backward jump facing right until it lands in <see cref="FacingRightLandingBackwardOnRightFoot"/>.</summary>
    FacingRightFallingBackwardToRightFoot,
    /// <summary>Torizo lands from its backward jump facing right with its right foot forward: faceless or Golden Torizo walks on, Bomb Torizo attacks, then walks right.</summary>
    FacingRightLandingBackwardOnRightFoot,
    /// <summary>Torizo jumps backward facing right and rises; once it no longer rises, it falls in <see cref="FacingRightFallingBackwardToLeftFoot"/>.</summary>
    FacingRightJumpingBackwardToLeftFoot,
    /// <summary>Torizo falls from its backward jump facing right until it lands in <see cref="FacingRightLandingBackwardOnLeftFoot"/>.</summary>
    FacingRightFallingBackwardToLeftFoot,
    /// <summary>Torizo lands from its backward jump facing right with its left foot forward: faceless or Golden Torizo walks on, Bomb Torizo attacks, then walks right.</summary>
    FacingRightLandingBackwardOnLeftFoot,
    /// <summary>Faceless Bomb Torizo turns to face right, then walks right, left leg first.</summary>
    FacelessTurningRight,
    /// <summary>Faceless Bomb Torizo walks right, left leg first.</summary>
    FacingRightFacelessWalkingLeftLegMoving,
    /// <summary>Faceless Bomb Torizo walks right, right leg first.</summary>
    FacingRightFacelessWalkingRightLegMoving,
    /// <summary>Golden Torizo sits dormant until Samus reaches its trigger; its AI then wakes it into <see cref="GoldenTorizoAwakening"/>.</summary>
    GoldenTorizoDormant,
    /// <summary>Golden Torizo wakes and drops, holding its half-risen pose; once it is on its floor, it settles in <see cref="GoldenTorizoSettling"/>.</summary>
    GoldenTorizoAwakening,
    /// <summary>A waking Golden Torizo lands on its floor and sits, its eyes light up and flicker back on twice, it stands, its fight starts, and it walks left.</summary>
    GoldenTorizoSettling,
    /// <summary>Golden Torizo, facing left with its right foot forward, spews Chozo orbs six times, then walks left, left leg first.</summary>
    GoldenFacingLeftSpewingChozoOrbsRightFootForward,
    /// <summary>Golden Torizo, facing left with its left foot forward, spews Chozo orbs six times, then walks left, right leg first.</summary>
    GoldenFacingLeftSpewingChozoOrbsLeftFootForward,
    /// <summary>Golden Torizo, facing left with its right foot forward, throws sonic booms four times, then walks left, left leg first.</summary>
    GoldenFacingLeftSonicBoomsRightFootForward,
    /// <summary>Golden Torizo, facing left with its left foot forward, throws sonic booms four times, then walks left, right leg first.</summary>
    GoldenFacingLeftSonicBoomsLeftFootForward,
    /// <summary>Golden Torizo, facing right with its left foot forward, spews Chozo orbs six times, then walks right, right leg first.</summary>
    GoldenFacingRightSpewingChozoOrbsLeftFootForward,
    /// <summary>Golden Torizo, facing right with its right foot forward, spews Chozo orbs six times, then walks right, left leg first.</summary>
    GoldenFacingRightSpewingChozoOrbsRightFootForward,
    /// <summary>Golden Torizo, facing right with its left foot forward, throws sonic booms four times, then walks right, right leg first.</summary>
    GoldenFacingRightSonicBoomsLeftFootForward,
    /// <summary>Golden Torizo, facing right with its right foot forward, throws sonic booms four times, then walks right, left leg first.</summary>
    GoldenFacingRightSonicBoomsRightFootForward,
    /// <summary>Golden Torizo, landed from a backward jump facing left with its left foot forward, attacks, then walks left.</summary>
    GoldenFacingLeftLandedBackwardOnLeftFoot,
    /// <summary>Golden Torizo, landed from a backward jump facing left with its right foot forward, attacks, then walks left.</summary>
    GoldenFacingLeftLandedBackwardOnRightFoot,
    /// <summary>Golden Torizo, landed from a backward jump facing right with its right foot forward, attacks, then walks right.</summary>
    GoldenFacingRightLandedBackwardOnRightFoot,
    /// <summary>Golden Torizo, landed from a backward jump facing right with its left foot forward, attacks, then walks right.</summary>
    GoldenFacingRightLandedBackwardOnLeftFoot,
    /// <summary>Golden Torizo, facing left with its left leg forward, catches a Super Missile and throws it back, then walks left.</summary>
    GoldenFacingLeftCatchingSuperMissileLeftLegForward,
    /// <summary>Golden Torizo, facing left with its right leg forward, catches a Super Missile and throws it back, then walks left.</summary>
    GoldenFacingLeftCatchingSuperMissileRightLegForward,
    /// <summary>Golden Torizo, facing right with its right leg forward, catches a Super Missile and throws it back, then walks right.</summary>
    GoldenFacingRightCatchingSuperMissileRightLegForward,
    /// <summary>Golden Torizo, facing right with its left leg forward, catches a Super Missile and throws it back, then walks right.</summary>
    GoldenFacingRightCatchingSuperMissileLeftLegForward,
    /// <summary>Golden Torizo, facing left, sits down on Samus morphed behind it, stands again, then walks left.</summary>
    GoldenFacingLeftSittingDownAttack,
    /// <summary>Golden Torizo, facing right, sits down on Samus morphed behind it, stands again, then walks right.</summary>
    GoldenFacingRightSittingDownAttack,
    /// <summary>Golden Torizo opens its chest, releases six eggs and holds its chest open; once its eggs are gone, it closes it in <see cref="GoldenChestClosing"/>.</summary>
    GoldenReleasingEggs,
    /// <summary>Golden Torizo closes its chest and resumes the walk its eggs interrupted.</summary>
    GoldenChestClosing,
    /// <summary>Golden Torizo readies its eye beam, fires it four times, its eyes glow twice, and it resumes the walk it interrupted.</summary>
    GoldenEyeBeamAttack,
    /// <summary>Golden Torizo, dodging, turns to face left, then walks left.</summary>
    GoldenDodgeTurningLeft,
    /// <summary>Golden Torizo turns to face left, then walks left.</summary>
    GoldenTurningLeft,
    /// <summary>Golden Torizo walks left, left leg first.</summary>
    GoldenWalkingLeftLeftLegMoving,
    /// <summary>Golden Torizo, dodging, turns to face right, then walks right.</summary>
    GoldenDodgeTurningRight,
    /// <summary>Golden Torizo turns to face right, then walks right.</summary>
    GoldenTurningRight,
    /// <summary>Golden Torizo walks right, right leg first.</summary>
    GoldenWalkingRightRightLegMoving,
    /// <summary>Bomb Torizo walks left, right leg first: the walk its attacks chosen on landing end in (#2028).</summary>
    FacingLeftWalkingRightLegMoving,
    /// <summary>Bomb Torizo walks left, left leg first: the walk its attacks chosen on landing end in (#2028).</summary>
    FacingLeftWalkingLeftLegMoving,
    /// <summary>Bomb Torizo walks right, left leg first: the walk its attacks chosen on landing end in (#2028).</summary>
    FacingRightWalkingLeftLegMoving,
    /// <summary>Bomb Torizo walks right, right leg first: the walk its attacks chosen on landing end in (#2028).</summary>
    FacingRightWalkingRightLegMoving,
    /// <summary>Golden Torizo walks left, right leg first: the walk its attacks chosen on landing end in (#2028).</summary>
    GoldenWalkingLeftRightLegMoving,
    /// <summary>Golden Torizo walks right, left leg first: the walk its attacks chosen on landing end in (#2028).</summary>
    GoldenWalkingRightLeftLegMoving,
}
