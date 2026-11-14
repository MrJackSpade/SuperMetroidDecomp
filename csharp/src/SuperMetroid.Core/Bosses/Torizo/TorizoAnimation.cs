namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's body animations (#641, #1788): one for each animation the encounter's AI, its movement, its
/// reactions or another animation starts, named for what it shows. Each is its steps, the phases it plays them in and how
/// it ends (<see cref="TorizoAnimations"/>).
/// </summary>
internal enum TorizoAnimation
{
    /// <summary>Bomb Torizo's gut blows up: the low-health explosions, then the gut's looks; started by its movement once its health falls below the gut's threshold, it resumes the animation it interrupted.</summary>
    BombTorizoGutBlowingUp,
    /// <summary>Bomb Torizo's face blows up: the low-health explosions, then the face's looks; started by its movement once its health falls below the face's threshold, it returns to the faceless turn its movement saved.</summary>
    BombTorizoFaceBlowingUp,
    /// <summary>Either Torizo's death: the death explosions while it flickers and fades to black, then the end of the fight.</summary>
    Dying,
    /// <summary>Bomb Torizo sits dormant, facing left, until the statue's hand is gone; its AI then wakes it into <see cref="BombTorizoAwakening"/>.</summary>
    BombTorizoDormant,
    /// <summary>Bomb Torizo wakes facing left: its eyes light up, it stands, its fight starts, and it walks left.</summary>
    BombTorizoAwakening,
    /// <summary>Golden Torizo, stunned, falls and its eyes light up again; it then returns to the walk that called it.</summary>
    Stunned,
    /// <summary>Bomb Torizo turns to face left, then walks left, right leg first.</summary>
    TurningLeft,
    /// <summary>Bomb Torizo, facing left with its right foot forward, spews Chozo orbs, then returns to the landing that called it.</summary>
    FacingLeftSpewingChozoOrbsRightFootForward,
    /// <summary>Bomb Torizo, facing left with its left foot forward, spews Chozo orbs, then returns to the landing that called it.</summary>
    FacingLeftSpewingChozoOrbsLeftFootForward,
    /// <summary>Bomb Torizo, facing left with its right foot forward, throws two sonic booms, then returns to the landing that called it.</summary>
    FacingLeftSonicBoomsRightFootForward,
    /// <summary>Bomb Torizo, facing left with its left foot forward, throws two sonic booms, then returns to the landing that called it.</summary>
    FacingLeftSonicBoomsLeftFootForward,
    /// <summary>Torizo, facing left with its right foot forward, swipes behind it, then returns to the walk that called it.</summary>
    FacingLeftExplosiveSwipeRightFootForward,
    /// <summary>Torizo, facing left with its left foot forward, swipes behind it, then returns to the walk that called it.</summary>
    FacingLeftExplosiveSwipeLeftFootForward,
    /// <summary>Torizo jumps forward facing left: it rises while it can, then falls until its jump lands it in <see cref="FacingLeftLanding"/>.</summary>
    FacingLeftJumpingForward,
    /// <summary>Torizo falls facing left until its jump lands it in <see cref="FacingLeftLanding"/>.</summary>
    FacingLeftFalling,
    /// <summary>Torizo lands facing left, then walks left on: faceless or Golden Torizo in its own walk, Bomb Torizo left leg first.</summary>
    FacingLeftLanding,
    /// <summary>Torizo jumps backward facing left: it rises while it can, then falls in <see cref="FacingLeftFallingBackwardToLeftFoot"/>.</summary>
    FacingLeftJumpingBackwardToLeftFoot,
    /// <summary>Torizo falls from its backward jump facing left until it lands in <see cref="FacingLeftLandingBackwardOnLeftFoot"/>.</summary>
    FacingLeftFallingBackwardToLeftFoot,
    /// <summary>Torizo lands from its backward jump facing left with its left foot forward: faceless or Golden Torizo walks on, Bomb Torizo attacks, then walks left.</summary>
    FacingLeftLandingBackwardOnLeftFoot,
    /// <summary>Torizo jumps backward facing left: it rises while it can, then falls in <see cref="FacingLeftFallingBackwardToRightFoot"/>.</summary>
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
    /// <summary>A dormant Bomb Torizo facing right, which no room starts; its AI would wake it into <see cref="BombTorizoAwakeningFacingRight"/>.</summary>
    BombTorizoDormantFacingRight,
    /// <summary>Bomb Torizo waking facing right, which no room starts: it stands and walks right.</summary>
    BombTorizoAwakeningFacingRight,
    /// <summary>Bomb Torizo turns to face right, then walks right, left leg first.</summary>
    TurningRight,
    /// <summary>Bomb Torizo, facing right with its left foot forward, spews Chozo orbs, then returns to the landing that called it.</summary>
    FacingRightSpewingChozoOrbsLeftFootForward,
    /// <summary>Bomb Torizo, facing right with its right foot forward, spews Chozo orbs, then returns to the landing that called it.</summary>
    FacingRightSpewingChozoOrbsRightFootForward,
    /// <summary>Bomb Torizo, facing right with its left foot forward, throws two sonic booms, then returns to the landing that called it.</summary>
    FacingRightSonicBoomsLeftFootForward,
    /// <summary>Bomb Torizo, facing right with its right foot forward, throws two sonic booms, then returns to the landing that called it.</summary>
    FacingRightSonicBoomsRightFootForward,
    /// <summary>Torizo, facing right with its left foot forward, swipes behind it, then returns to the walk that called it.</summary>
    FacingRightExplosiveSwipeLeftFootForward,
    /// <summary>Torizo, facing right with its right foot forward, swipes behind it, then returns to the walk that called it.</summary>
    FacingRightExplosiveSwipeRightFootForward,
    /// <summary>Torizo jumps forward facing right: it rises while it can, then falls until its jump lands it in <see cref="FacingRightLanding"/>.</summary>
    FacingRightJumpingForward,
    /// <summary>Torizo falls facing right until its jump lands it in <see cref="FacingRightLanding"/>.</summary>
    FacingRightFalling,
    /// <summary>Torizo lands facing right, then walks right on: faceless or Golden Torizo in its own walk, Bomb Torizo right leg first.</summary>
    FacingRightLanding,
    /// <summary>Torizo jumps backward facing right: it rises while it can, then falls in <see cref="FacingRightFallingBackwardToRightFoot"/>.</summary>
    FacingRightJumpingBackwardToRightFoot,
    /// <summary>Torizo falls from its backward jump facing right until it lands in <see cref="FacingRightLandingBackwardOnRightFoot"/>.</summary>
    FacingRightFallingBackwardToRightFoot,
    /// <summary>Torizo lands from its backward jump facing right with its right foot forward: faceless or Golden Torizo walks on, Bomb Torizo attacks, then walks right.</summary>
    FacingRightLandingBackwardOnRightFoot,
    /// <summary>Torizo jumps backward facing right: it rises while it can, then falls in <see cref="FacingRightFallingBackwardToLeftFoot"/>.</summary>
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
    /// <summary>Golden Torizo wakes: it drops to the floor, its eyes light up, it stands, its fight starts, and it walks left.</summary>
    GoldenTorizoAwakening,
    /// <summary>Golden Torizo, facing left with its right foot forward, spews Chozo orbs, then returns to the landing that called it.</summary>
    GoldenFacingLeftSpewingChozoOrbsRightFootForward,
    /// <summary>Golden Torizo, facing left with its left foot forward, spews Chozo orbs, then returns to the landing that called it.</summary>
    GoldenFacingLeftSpewingChozoOrbsLeftFootForward,
    /// <summary>Golden Torizo, facing left with its right foot forward, throws two sonic booms, then returns to the landing that called it.</summary>
    GoldenFacingLeftSonicBoomsRightFootForward,
    /// <summary>Golden Torizo, facing left with its left foot forward, throws two sonic booms, then returns to the landing that called it.</summary>
    GoldenFacingLeftSonicBoomsLeftFootForward,
    /// <summary>Golden Torizo, facing right with its left foot forward, spews Chozo orbs, then returns to the landing that called it.</summary>
    GoldenFacingRightSpewingChozoOrbsLeftFootForward,
    /// <summary>Golden Torizo, facing right with its right foot forward, spews Chozo orbs, then returns to the landing that called it.</summary>
    GoldenFacingRightSpewingChozoOrbsRightFootForward,
    /// <summary>Golden Torizo, facing right with its left foot forward, throws two sonic booms, then returns to the landing that called it.</summary>
    GoldenFacingRightSonicBoomsLeftFootForward,
    /// <summary>Golden Torizo, facing right with its right foot forward, throws two sonic booms, then returns to the landing that called it.</summary>
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
    /// <summary>Golden Torizo opens its chest and releases its eggs, holds it open while any egg remains, then returns to the walk that called it.</summary>
    GoldenReleasingEggs,
    /// <summary>Golden Torizo fires its eye beam, then returns to the walk that called it.</summary>
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
}
