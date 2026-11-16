namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's body animation segments (#641, #1788, #2235): each a run of steps an animation plays whole, once
/// or a number of times. Each animation the encounter's AI, its movement, its reactions or another animation starts begins
/// at the segment named for what it shows (<see cref="TorizoAnimations.StartingAt"/>); the runs it repeats, loops on or
/// moves on to are segments of their own, named for what they show, and the runs several animations play alike (a walk's
/// steps, a fall's pose, a volley of orbs) are one segment each plays.
/// </summary>
internal enum TorizoAnimation
{
    /// <summary>Bomb Torizo's gut blows up: the low-health explosions, then the gut's looks; started by its movement once its health falls below the gut's threshold, it resumes the animation it interrupted.</summary>
    BombTorizoGutBlowingUp,
    /// <summary>Bomb Torizo's face blows up: the low-health explosions, then the face's looks; started by its movement once its health falls below the face's threshold, it ends in the faceless turn away from the way it faces.</summary>
    BombTorizoFaceBlowingUp,
    /// <summary>Either Torizo's death: it goes idle and locks its animation, then explodes (<see cref="DyingExplosions"/>).</summary>
    Dying,
    /// <summary>A dying Torizo's death explosion and the wait after it, the explosion's own and the next together, eight times.</summary>
    DyingExplosions,
    /// <summary>A dying Torizo starts its fade to black.</summary>
    DyingFadeToBlack,
    /// <summary>A dying Torizo explodes twice while it flickers and its fade steps on, fourteen times.</summary>
    DyingFlickering,
    /// <summary>A dying Torizo's last wait, then the end of the fight.</summary>
    DyingFinish,
    /// <summary>Bomb Torizo sits dormant, facing left, until the statue's hand is gone; its AI then wakes it into <see cref="BombTorizoAwakening"/>.</summary>
    BombTorizoDormant,
    /// <summary>Bomb Torizo wakes facing left: its eyes light up, flicker back on twice (<see cref="SeatedEyesLightingAgain"/>), it stands, its fight starts, and it walks left.</summary>
    BombTorizoAwakening,
    /// <summary>A seated Torizo's eyes flicker back on, twice as either wakes.</summary>
    SeatedEyesLightingAgain,
    /// <summary>Bomb Torizo stands up from its seat and starts its fade to its own colours.</summary>
    BombTorizoStandingUp,
    /// <summary>A Torizo standing up holds its wide stance while its colours fade in, sixteen steps.</summary>
    StandingColourFade,
    /// <summary>Bomb Torizo unlocks its animation, starts its fight and holds its stance before walking left.</summary>
    BombTorizoFightStart,
    /// <summary>Golden Torizo, stunned, staggers; its eyes light up again twice (<see cref="StunnedEyesLightingAgain"/>), and it resumes the walk it interrupted.</summary>
    Stunned,
    /// <summary>A stunned Golden Torizo's eyes light up again, twice.</summary>
    StunnedEyesLightingAgain,
    /// <summary>A stunned Golden Torizo recovers: it unlocks its animation, starts walking and resumes the walk it interrupted.</summary>
    StunnedRecovery,
    /// <summary>Bomb Torizo turns to face left, then walks left, right leg first.</summary>
    TurningLeft,
    /// <summary>Bomb Torizo, facing left with its right foot forward, spews Chozo orbs (<see cref="BombChozoOrbBurst"/>), then walks left, left leg first (<see cref="FacingLeftWalkingLeftLegMoving"/>).</summary>
    FacingLeftSpewingChozoOrbsRightFootForward,
    /// <summary>Bomb Torizo spits a burst of Chozo orbs and waits, three times a spew.</summary>
    BombChozoOrbBurst,
    /// <summary>Bomb Torizo, facing left with its right foot forward, lowers its arm after spewing and walks left on, left leg first.</summary>
    FacingLeftSpewingChozoOrbsRightFootForwardEnd,
    /// <summary>Bomb Torizo, facing left with its left foot forward, spews Chozo orbs, then walks left, right leg first (<see cref="FacingLeftWalkingRightLegMoving"/>).</summary>
    FacingLeftSpewingChozoOrbsLeftFootForward,
    /// <summary>Bomb Torizo, facing left with its left foot forward, lowers its arm after spewing and walks left on, right leg first.</summary>
    FacingLeftSpewingChozoOrbsLeftFootForwardEnd,
    /// <summary>Bomb Torizo, facing left with its right foot forward, throws sonic booms (<see cref="FacingLeftSonicBoomsRightFootForwardThrowing"/>), then walks left, left leg first (<see cref="FacingLeftWalkingLeftLegMoving"/>).</summary>
    FacingLeftSonicBoomsRightFootForward,
    /// <summary>Bomb Torizo, facing left with its right foot forward, throws a sonic boom with each arm, four times.</summary>
    FacingLeftSonicBoomsRightFootForwardThrowing,
    /// <summary>Bomb Torizo, facing left with its right foot forward, walks left on after its sonic booms, left leg first.</summary>
    FacingLeftSonicBoomsRightFootForwardEnd,
    /// <summary>Bomb Torizo, facing left with its left foot forward, throws sonic booms, then walks left, right leg first (<see cref="FacingLeftWalkingRightLegMoving"/>).</summary>
    FacingLeftSonicBoomsLeftFootForward,
    /// <summary>Bomb Torizo, facing left with its left foot forward, throws a sonic boom with each arm, four times.</summary>
    FacingLeftSonicBoomsLeftFootForwardThrowing,
    /// <summary>Bomb Torizo, facing left with its left foot forward, walks left on after its sonic booms, right leg first.</summary>
    FacingLeftSonicBoomsLeftFootForwardEnd,
    /// <summary>Torizo, facing left with its right foot forward, swipes behind it, then resumes the walk it interrupted.</summary>
    FacingLeftExplosiveSwipeRightFootForward,
    /// <summary>Torizo, facing left with its left foot forward, swipes behind it, then resumes the walk it interrupted.</summary>
    FacingLeftExplosiveSwipeLeftFootForward,
    /// <summary>Torizo jumps forward facing left: it rises while it can (<see cref="FacingLeftRising"/>), then falls until its jump lands it in <see cref="FacingLeftLanding"/>.</summary>
    FacingLeftJumpingForward,
    /// <summary>A jumping Torizo facing left leaps, again while it still rises.</summary>
    FacingLeftRising,
    /// <summary>Torizo falls facing left until its jump lands it in <see cref="FacingLeftLanding"/>, resting in <see cref="FacingLeftFallingPose"/>.</summary>
    FacingLeftFalling,
    /// <summary>A Torizo falling facing left, the pose its fall rests in.</summary>
    FacingLeftFallingPose,
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
    /// <summary>Bomb Torizo's step left on its right leg; the walk starting here steps on with its left (<see cref="FacingLeftWalkingLeftLegMoving"/>), as its attacks chosen on landing end (#2028).</summary>
    FacingLeftWalkingRightLegMoving,
    /// <summary>Bomb Torizo's step left on its left leg; the walk starting here steps on with its right (<see cref="FacingLeftWalkingRightLegMoving"/>), as its attacks chosen on landing end (#2028).</summary>
    FacingLeftWalkingLeftLegMoving,
    /// <summary>Faceless Bomb Torizo turns to face left, then walks left, right leg first.</summary>
    FacelessTurningLeft,
    /// <summary>Faceless Bomb Torizo's step left on its right leg; the walk starting here steps on with its left.</summary>
    FacingLeftFacelessWalkingRightLegMoving,
    /// <summary>Faceless Bomb Torizo's step left on its left leg; the walk starting here steps on with its right.</summary>
    FacingLeftFacelessWalkingLeftLegMoving,
    /// <summary>Bomb Torizo turns to face right, then walks right, left leg first.</summary>
    TurningRight,
    /// <summary>Bomb Torizo, facing right with its left foot forward, spews Chozo orbs (<see cref="BombChozoOrbBurst"/>), then walks right, right leg first (<see cref="FacingRightWalkingRightLegMoving"/>).</summary>
    FacingRightSpewingChozoOrbsLeftFootForward,
    /// <summary>Bomb Torizo, facing right with its left foot forward, lowers its arm after spewing and walks right on, right leg first.</summary>
    FacingRightSpewingChozoOrbsLeftFootForwardEnd,
    /// <summary>Bomb Torizo, facing right with its right foot forward, spews Chozo orbs, then walks right, left leg first (<see cref="FacingRightWalkingLeftLegMoving"/>).</summary>
    FacingRightSpewingChozoOrbsRightFootForward,
    /// <summary>Bomb Torizo, facing right with its right foot forward, lowers its arm after spewing and walks right on, left leg first.</summary>
    FacingRightSpewingChozoOrbsRightFootForwardEnd,
    /// <summary>Bomb Torizo, facing right with its left foot forward, throws sonic booms, then walks right, right leg first (<see cref="FacingRightWalkingRightLegMoving"/>).</summary>
    FacingRightSonicBoomsLeftFootForward,
    /// <summary>Bomb Torizo, facing right with its left foot forward, throws a sonic boom with each arm, four times.</summary>
    FacingRightSonicBoomsLeftFootForwardThrowing,
    /// <summary>Bomb Torizo, facing right with its left foot forward, walks right on after its sonic booms, right leg first.</summary>
    FacingRightSonicBoomsLeftFootForwardEnd,
    /// <summary>Bomb Torizo, facing right with its right foot forward, throws sonic booms, then walks right, left leg first (<see cref="FacingRightWalkingLeftLegMoving"/>).</summary>
    FacingRightSonicBoomsRightFootForward,
    /// <summary>Bomb Torizo, facing right with its right foot forward, throws a sonic boom with each arm, four times.</summary>
    FacingRightSonicBoomsRightFootForwardThrowing,
    /// <summary>Bomb Torizo, facing right with its right foot forward, walks right on after its sonic booms, left leg first.</summary>
    FacingRightSonicBoomsRightFootForwardEnd,
    /// <summary>Torizo, facing right with its left foot forward, swipes behind it, then resumes the walk it interrupted.</summary>
    FacingRightExplosiveSwipeLeftFootForward,
    /// <summary>Torizo, facing right with its right foot forward, swipes behind it, then resumes the walk it interrupted.</summary>
    FacingRightExplosiveSwipeRightFootForward,
    /// <summary>Torizo jumps forward facing right: it rises while it can (<see cref="FacingRightRising"/>), then falls until its jump lands it in <see cref="FacingRightLanding"/>.</summary>
    FacingRightJumpingForward,
    /// <summary>A jumping Torizo facing right leaps, again while it still rises.</summary>
    FacingRightRising,
    /// <summary>Torizo falls facing right until its jump lands it in <see cref="FacingRightLanding"/>, resting in <see cref="FacingRightFallingPose"/>.</summary>
    FacingRightFalling,
    /// <summary>A Torizo falling facing right, the pose its fall rests in.</summary>
    FacingRightFallingPose,
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
    /// <summary>Bomb Torizo's step right on its left leg; the walk starting here steps on with its right (<see cref="FacingRightWalkingRightLegMoving"/>), as its attacks chosen on landing end (#2028).</summary>
    FacingRightWalkingLeftLegMoving,
    /// <summary>Bomb Torizo's step right on its right leg; the walk starting here steps on with its left (<see cref="FacingRightWalkingLeftLegMoving"/>), as its attacks chosen on landing end (#2028).</summary>
    FacingRightWalkingRightLegMoving,
    /// <summary>Faceless Bomb Torizo turns to face right, then walks right, left leg first.</summary>
    FacelessTurningRight,
    /// <summary>Faceless Bomb Torizo's step right on its left leg; the walk starting here steps on with its right.</summary>
    FacingRightFacelessWalkingLeftLegMoving,
    /// <summary>Faceless Bomb Torizo's step right on its right leg; the walk starting here steps on with its left.</summary>
    FacingRightFacelessWalkingRightLegMoving,
    /// <summary>Golden Torizo sits dormant until Samus reaches its trigger; its AI then wakes it into <see cref="GoldenTorizoAwakening"/>.</summary>
    GoldenTorizoDormant,
    /// <summary>Golden Torizo wakes: it starts to fall (<see cref="GoldenTorizoSettling"/>), its eyes flicker back on twice, it stands, its fight starts, and it walks left.</summary>
    GoldenTorizoAwakening,
    /// <summary>A waking Golden Torizo drops to the floor while airborne, sits, and its eyes light up.</summary>
    GoldenTorizoSettling,
    /// <summary>Golden Torizo stands up from its seat and starts its fade to its golden colours.</summary>
    GoldenTorizoStandingUp,
    /// <summary>Golden Torizo unlocks its animation, starts its fight and holds its stance before walking left.</summary>
    GoldenTorizoFightStart,
    /// <summary>Golden Torizo, facing left with its right foot forward, spews Chozo orbs (<see cref="GoldenChozoOrbBurst"/>), then walks left, left leg first (<see cref="GoldenWalkingLeftLeftLegMoving"/>).</summary>
    GoldenFacingLeftSpewingChozoOrbsRightFootForward,
    /// <summary>Golden Torizo spits a burst of Chozo orbs with its shot's sound and waits, six times a spew.</summary>
    GoldenChozoOrbBurst,
    /// <summary>Golden Torizo, facing left with its right foot forward, lowers its arm after spewing and walks left on, left leg first.</summary>
    GoldenFacingLeftSpewingChozoOrbsRightFootForwardEnd,
    /// <summary>Golden Torizo, facing left with its left foot forward, spews Chozo orbs, then walks left, right leg first (<see cref="GoldenWalkingLeftRightLegMoving"/>).</summary>
    GoldenFacingLeftSpewingChozoOrbsLeftFootForward,
    /// <summary>Golden Torizo, facing left with its left foot forward, lowers its arm after spewing and walks left on, right leg first.</summary>
    GoldenFacingLeftSpewingChozoOrbsLeftFootForwardEnd,
    /// <summary>Golden Torizo, facing left with its right foot forward, throws sonic booms, then walks left, left leg first (<see cref="GoldenWalkingLeftLeftLegMoving"/>).</summary>
    GoldenFacingLeftSonicBoomsRightFootForward,
    /// <summary>Golden Torizo, facing left with its right foot forward, throws a sonic boom with each arm, four times.</summary>
    GoldenFacingLeftSonicBoomsRightFootForwardThrowing,
    /// <summary>Golden Torizo, facing left with its right foot forward, walks left on after its sonic booms, left leg first.</summary>
    GoldenFacingLeftSonicBoomsRightFootForwardEnd,
    /// <summary>Golden Torizo, facing left with its left foot forward, throws sonic booms, then walks left, right leg first (<see cref="GoldenWalkingLeftRightLegMoving"/>).</summary>
    GoldenFacingLeftSonicBoomsLeftFootForward,
    /// <summary>Golden Torizo, facing left with its left foot forward, throws a sonic boom with each arm, four times.</summary>
    GoldenFacingLeftSonicBoomsLeftFootForwardThrowing,
    /// <summary>Golden Torizo, facing left with its left foot forward, walks left on after its sonic booms, right leg first.</summary>
    GoldenFacingLeftSonicBoomsLeftFootForwardEnd,
    /// <summary>Golden Torizo, facing right with its left foot forward, spews Chozo orbs, then walks right, right leg first (<see cref="GoldenWalkingRightRightLegMoving"/>).</summary>
    GoldenFacingRightSpewingChozoOrbsLeftFootForward,
    /// <summary>Golden Torizo, facing right with its left foot forward, lowers its arm after spewing and walks right on, right leg first.</summary>
    GoldenFacingRightSpewingChozoOrbsLeftFootForwardEnd,
    /// <summary>Golden Torizo, facing right with its right foot forward, spews Chozo orbs, then walks right, left leg first (<see cref="GoldenWalkingRightLeftLegMoving"/>).</summary>
    GoldenFacingRightSpewingChozoOrbsRightFootForward,
    /// <summary>Golden Torizo, facing right with its right foot forward, lowers its arm after spewing and walks right on, left leg first.</summary>
    GoldenFacingRightSpewingChozoOrbsRightFootForwardEnd,
    /// <summary>Golden Torizo, facing right with its left foot forward, throws sonic booms, then walks right, right leg first (<see cref="GoldenWalkingRightRightLegMoving"/>).</summary>
    GoldenFacingRightSonicBoomsLeftFootForward,
    /// <summary>Golden Torizo, facing right with its left foot forward, throws a sonic boom with each arm, four times.</summary>
    GoldenFacingRightSonicBoomsLeftFootForwardThrowing,
    /// <summary>Golden Torizo, facing right with its left foot forward, walks right on after its sonic booms, right leg first.</summary>
    GoldenFacingRightSonicBoomsLeftFootForwardEnd,
    /// <summary>Golden Torizo, facing right with its right foot forward, throws sonic booms, then walks right, left leg first (<see cref="GoldenWalkingRightLeftLegMoving"/>).</summary>
    GoldenFacingRightSonicBoomsRightFootForward,
    /// <summary>Golden Torizo, facing right with its right foot forward, throws a sonic boom with each arm, four times.</summary>
    GoldenFacingRightSonicBoomsRightFootForwardThrowing,
    /// <summary>Golden Torizo, facing right with its right foot forward, walks right on after its sonic booms, left leg first.</summary>
    GoldenFacingRightSonicBoomsRightFootForwardEnd,
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
    /// <summary>Golden Torizo opens its chest (<see cref="GoldenEggLaying"/>), holds it open while any egg remains (<see cref="GoldenChestClosing"/>), then resumes the walk it interrupted.</summary>
    GoldenReleasingEggs,
    /// <summary>Golden Torizo's open chest releases an egg with its sound, then waits, six times.</summary>
    GoldenEggLaying,
    /// <summary>Golden Torizo holds its chest open again while any egg remains, then closes it and resumes the walk it interrupted.</summary>
    GoldenChestClosing,
    /// <summary>Golden Torizo readies its eye beam (<see cref="GoldenEyeBeamFiring"/>), its eyes glow, and it resumes the walk it interrupted.</summary>
    GoldenEyeBeamAttack,
    /// <summary>Golden Torizo fires its eye beam and waits, four times.</summary>
    GoldenEyeBeamFiring,
    /// <summary>Golden Torizo waits after its eye beam's shots.</summary>
    GoldenEyeBeamPause,
    /// <summary>Golden Torizo's eyes glow through their four looks, twice.</summary>
    GoldenEyeBeamGlow,
    /// <summary>Golden Torizo's eye beam's explosions show for a while, then it unlocks its animation and resumes the walk it interrupted.</summary>
    GoldenEyeBeamEnd,
    /// <summary>Golden Torizo, dodging, turns to face left, then walks left.</summary>
    GoldenDodgeTurningLeft,
    /// <summary>Golden Torizo turns to face left, then walks left.</summary>
    GoldenTurningLeft,
    /// <summary>Golden Torizo's step left on its right leg; the walk starting here steps on with its left, as its attacks chosen on landing end (#2028).</summary>
    GoldenWalkingLeftRightLegMoving,
    /// <summary>Golden Torizo's step left on its left leg; the walk starting here steps on with its right.</summary>
    GoldenWalkingLeftLeftLegMoving,
    /// <summary>Golden Torizo, dodging, turns to face right, then walks right.</summary>
    GoldenDodgeTurningRight,
    /// <summary>Golden Torizo turns to face right, then walks right.</summary>
    GoldenTurningRight,
    /// <summary>Golden Torizo's step right on its left leg; the walk starting here steps on with its right, as its attacks chosen on landing end (#2028).</summary>
    GoldenWalkingRightLeftLegMoving,
    /// <summary>Golden Torizo's step right on its right leg; the walk starting here steps on with its left.</summary>
    GoldenWalkingRightRightLegMoving,
}
