namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's body animation segments (#641, #1788, #2235, #2411): each a run of steps an animation plays
/// whole, once or a number of times. The animations the encounter's AI, its movement, its reactions and its cues start are
/// <see cref="TorizoAnimation"/>s, each playing the segments <see cref="TorizoAnimations"/> lists for it; a run
/// several animations play alike (a turn, a walk's steps, a fall's pose, a volley of orbs) is one segment each plays, and a
/// run the Torizo holds until something changes (a jump's rise, Golden Torizo's drop, its open chest) is a segment its
/// animation loops on.
/// </summary>
internal enum TorizoBodySegment
{
    /// <summary>Bomb Torizo's gut blows up: the low-health explosions, then the gut's looks, then the animation it interrupted resumes.</summary>
    BombTorizoGutBlowingUp,
    /// <summary>Bomb Torizo's face blows up: the low-health explosions, then the face's looks, then the faceless turn away from the way it faces.</summary>
    BombTorizoFaceBlowingUp,
    /// <summary>A dying Torizo goes idle and locks its animation.</summary>
    Dying,
    /// <summary>A dying Torizo's death explosion and the wait after it, the explosion's own and the next together.</summary>
    DyingExplosions,
    /// <summary>A dying Torizo starts its fade to black.</summary>
    DyingFadeToBlack,
    /// <summary>A dying Torizo explodes twice while it flickers four times, then its fade steps on.</summary>
    DyingFlickering,
    /// <summary>A dying Torizo's last wait, then the end of the fight.</summary>
    DyingFinish,
    /// <summary>Bomb Torizo sits dormant, facing left, its animation locked, until the statue's hand is gone.</summary>
    BombTorizoDormant,
    /// <summary>Bomb Torizo, seated facing left, falls idle and its eyes light up.</summary>
    BombTorizoAwakening,
    /// <summary>A seated Torizo's eyes go dark and light up through their looks again.</summary>
    SeatedEyesLightingAgain,
    /// <summary>Bomb Torizo stands up from its seat and starts its fade to its own colours.</summary>
    BombTorizoStandingUp,
    /// <summary>A Torizo standing up holds its wide stance while its colours fade a step in.</summary>
    StandingColourFade,
    /// <summary>Bomb Torizo unlocks its animation, starts its fight and holds its stance.</summary>
    BombTorizoFightStart,
    /// <summary>Golden Torizo, stunned, stands its ground with its animation locked and waits.</summary>
    Stunned,
    /// <summary>A stunned Golden Torizo's eyes go dark and light up through their looks again.</summary>
    StunnedEyesLightingAgain,
    /// <summary>A stunned Golden Torizo recovers: it unlocks its animation, starts walking and resumes the walk it interrupted.</summary>
    StunnedRecovery,
    /// <summary>Bomb Torizo turns around through facing the viewer, twenty-four updates.</summary>
    Turning,
    /// <summary>Bomb Torizo, facing left with its right foot forward, starts spewing: it lowers its head, opens its mouth, thrusts and draws back its arm.</summary>
    FacingLeftSpewingChozoOrbsRightFootForward,
    /// <summary>Bomb Torizo spits a burst of Chozo orbs and waits.</summary>
    BombChozoOrbBurst,
    /// <summary>Bomb Torizo, facing left with its right foot forward, lowers its arm after spewing and walks left on, left leg first.</summary>
    FacingLeftSpewingChozoOrbsRightFootForwardEnd,
    /// <summary>Bomb Torizo, facing left with its left foot forward, starts spewing: it lowers its head, opens its mouth, thrusts and draws back its arm.</summary>
    FacingLeftSpewingChozoOrbsLeftFootForward,
    /// <summary>Bomb Torizo, facing left with its left foot forward, lowers its arm after spewing and walks left on, right leg first.</summary>
    FacingLeftSpewingChozoOrbsLeftFootForwardEnd,
    /// <summary>Either Torizo stands its ground to throw its sonic booms.</summary>
    StandingToThrowSonicBooms,
    /// <summary>Bomb Torizo, facing left with its right foot forward, throws a sonic boom with each arm.</summary>
    FacingLeftSonicBoomsRightFootForwardThrowing,
    /// <summary>Bomb Torizo, facing left with its right foot forward, walks left on after its sonic booms, left leg first.</summary>
    FacingLeftSonicBoomsRightFootForwardEnd,
    /// <summary>Bomb Torizo, facing left with its left foot forward, throws a sonic boom with each arm.</summary>
    FacingLeftSonicBoomsLeftFootForwardThrowing,
    /// <summary>Bomb Torizo, facing left with its left foot forward, walks left on after its sonic booms, right leg first.</summary>
    FacingLeftSonicBoomsLeftFootForwardEnd,
    /// <summary>Torizo, facing left with its right foot forward, swipes behind it, then resumes the walk it interrupted.</summary>
    FacingLeftExplosiveSwipeRightFootForward,
    /// <summary>Torizo, facing left with its left foot forward, swipes behind it, then resumes the walk it interrupted.</summary>
    FacingLeftExplosiveSwipeLeftFootForward,
    /// <summary>Torizo crouches and springs forward facing left, naming the fall its rise ends in.</summary>
    FacingLeftJumpingForward,
    /// <summary>A jumping Torizo facing left shows its leaping pose, which it holds while it rises.</summary>
    FacingLeftRising,
    /// <summary>Torizo starts falling facing left, naming the forward landing its fall ends in.</summary>
    FacingLeftFalling,
    /// <summary>A Torizo falling facing left, the pose its fall rests in.</summary>
    FacingLeftFallingPose,
    /// <summary>Torizo lands facing left: faceless or Golden Torizo walks on in its own walk, Bomb Torizo runs on into its own.</summary>
    FacingLeftLanding,
    /// <summary>Torizo crouches and springs backward facing left, naming the fall onto its left foot its rise ends in.</summary>
    FacingLeftJumpingBackwardToLeftFoot,
    /// <summary>Torizo starts falling from its backward jump facing left, naming its landing on its left foot.</summary>
    FacingLeftFallingBackwardToLeftFoot,
    /// <summary>Torizo lands from its backward jump facing left with its left foot forward: faceless or Golden Torizo walks on, Bomb Torizo chooses its attack.</summary>
    FacingLeftLandingBackwardOnLeftFoot,
    /// <summary>Torizo crouches and springs backward facing left, naming the fall onto its right foot its rise ends in.</summary>
    FacingLeftJumpingBackwardToRightFoot,
    /// <summary>Torizo starts falling from its backward jump facing left, naming its landing on its right foot.</summary>
    FacingLeftFallingBackwardToRightFoot,
    /// <summary>Torizo lands from its backward jump facing left with its right foot forward: faceless or Golden Torizo walks on, Bomb Torizo chooses its attack.</summary>
    FacingLeftLandingBackwardOnRightFoot,
    /// <summary>Bomb Torizo's step left on its right leg, deciding at each pose whether to swipe or jump back.</summary>
    FacingLeftWalkingRightLegMoving,
    /// <summary>Bomb Torizo's step left on its left leg, deciding at each pose whether to swipe or jump back.</summary>
    FacingLeftWalkingLeftLegMoving,
    /// <summary>Faceless Bomb Torizo or Golden Torizo turns around through facing the viewer, eight updates.</summary>
    TurningQuickly,
    /// <summary>Faceless Bomb Torizo's step left on its right leg, deciding as it lifts its leg whether to swipe.</summary>
    FacingLeftFacelessWalkingRightLegMoving,
    /// <summary>Faceless Bomb Torizo's step left on its left leg, deciding as it lifts its leg whether to swipe.</summary>
    FacingLeftFacelessWalkingLeftLegMoving,
    /// <summary>Bomb Torizo, facing right with its left foot forward, starts spewing: it lowers its head, opens its mouth, thrusts and draws back its arm.</summary>
    FacingRightSpewingChozoOrbsLeftFootForward,
    /// <summary>Bomb Torizo, facing right with its left foot forward, lowers its arm after spewing and walks right on, right leg first.</summary>
    FacingRightSpewingChozoOrbsLeftFootForwardEnd,
    /// <summary>Bomb Torizo, facing right with its right foot forward, starts spewing: it lowers its head, opens its mouth, thrusts and draws back its arm.</summary>
    FacingRightSpewingChozoOrbsRightFootForward,
    /// <summary>Bomb Torizo, facing right with its right foot forward, lowers its arm after spewing and walks right on, left leg first.</summary>
    FacingRightSpewingChozoOrbsRightFootForwardEnd,
    /// <summary>Bomb Torizo, facing right with its left foot forward, throws a sonic boom with each arm.</summary>
    FacingRightSonicBoomsLeftFootForwardThrowing,
    /// <summary>Bomb Torizo, facing right with its left foot forward, walks right on after its sonic booms, right leg first.</summary>
    FacingRightSonicBoomsLeftFootForwardEnd,
    /// <summary>Bomb Torizo, facing right with its right foot forward, throws a sonic boom with each arm.</summary>
    FacingRightSonicBoomsRightFootForwardThrowing,
    /// <summary>Bomb Torizo, facing right with its right foot forward, walks right on after its sonic booms, left leg first.</summary>
    FacingRightSonicBoomsRightFootForwardEnd,
    /// <summary>Torizo, facing right with its left foot forward, swipes behind it, then resumes the walk it interrupted.</summary>
    FacingRightExplosiveSwipeLeftFootForward,
    /// <summary>Torizo, facing right with its right foot forward, swipes behind it, then resumes the walk it interrupted.</summary>
    FacingRightExplosiveSwipeRightFootForward,
    /// <summary>Torizo crouches and springs forward facing right, naming the fall its rise ends in.</summary>
    FacingRightJumpingForward,
    /// <summary>A jumping Torizo facing right shows its leaping pose, which it holds while it rises.</summary>
    FacingRightRising,
    /// <summary>Torizo starts falling facing right, naming the forward landing its fall ends in.</summary>
    FacingRightFalling,
    /// <summary>A Torizo falling facing right, the pose its fall rests in.</summary>
    FacingRightFallingPose,
    /// <summary>Torizo lands facing right: faceless or Golden Torizo walks on in its own walk, Bomb Torizo runs on into its own.</summary>
    FacingRightLanding,
    /// <summary>Torizo crouches and springs backward facing right, naming the fall onto its right foot its rise ends in.</summary>
    FacingRightJumpingBackwardToRightFoot,
    /// <summary>Torizo starts falling from its backward jump facing right, naming its landing on its right foot.</summary>
    FacingRightFallingBackwardToRightFoot,
    /// <summary>Torizo lands from its backward jump facing right with its right foot forward: faceless or Golden Torizo walks on, Bomb Torizo chooses its attack.</summary>
    FacingRightLandingBackwardOnRightFoot,
    /// <summary>Torizo crouches and springs backward facing right, naming the fall onto its left foot its rise ends in.</summary>
    FacingRightJumpingBackwardToLeftFoot,
    /// <summary>Torizo starts falling from its backward jump facing right, naming its landing on its left foot.</summary>
    FacingRightFallingBackwardToLeftFoot,
    /// <summary>Torizo lands from its backward jump facing right with its left foot forward: faceless or Golden Torizo walks on, Bomb Torizo chooses its attack.</summary>
    FacingRightLandingBackwardOnLeftFoot,
    /// <summary>Bomb Torizo's step right on its left leg, deciding at each pose whether to swipe or jump back.</summary>
    FacingRightWalkingLeftLegMoving,
    /// <summary>Bomb Torizo's step right on its right leg, deciding at each pose whether to swipe or jump back.</summary>
    FacingRightWalkingRightLegMoving,
    /// <summary>Faceless Bomb Torizo's step right on its left leg, deciding as it lifts its leg whether to swipe.</summary>
    FacingRightFacelessWalkingLeftLegMoving,
    /// <summary>Faceless Bomb Torizo's step right on its right leg, deciding as it lifts its leg whether to swipe.</summary>
    FacingRightFacelessWalkingRightLegMoving,
    /// <summary>Golden Torizo sits dormant, its animation locked, waiting for Samus to reach its trigger.</summary>
    GoldenTorizoDormant,
    /// <summary>Golden Torizo starts to fall as it wakes.</summary>
    GoldenTorizoAwakening,
    /// <summary>A waking Golden Torizo shows its half-risen pose, which it holds while it drops to its floor.</summary>
    GoldenTorizoDropping,
    /// <summary>A waking Golden Torizo lands on its floor, sits, falls idle and its eyes light up.</summary>
    GoldenTorizoSettling,
    /// <summary>Golden Torizo stands up from its seat and starts its fade to its golden colours.</summary>
    GoldenTorizoStandingUp,
    /// <summary>Golden Torizo unlocks its animation, starts its fight and holds its stance.</summary>
    GoldenTorizoFightStart,
    /// <summary>Golden Torizo, facing left with its right foot forward, starts spewing: it lowers its head, opens its mouth, thrusts and draws back its arm.</summary>
    GoldenFacingLeftSpewingChozoOrbsRightFootForward,
    /// <summary>Golden Torizo spits a burst of Chozo orbs with its shot's sound and waits.</summary>
    GoldenChozoOrbBurst,
    /// <summary>Golden Torizo, facing left with its right foot forward, lowers its arm after spewing and walks left on, left leg first.</summary>
    GoldenFacingLeftSpewingChozoOrbsRightFootForwardEnd,
    /// <summary>Golden Torizo, facing left with its left foot forward, starts spewing: it lowers its head, opens its mouth, thrusts and draws back its arm.</summary>
    GoldenFacingLeftSpewingChozoOrbsLeftFootForward,
    /// <summary>Golden Torizo, facing left with its left foot forward, lowers its arm after spewing and walks left on, right leg first.</summary>
    GoldenFacingLeftSpewingChozoOrbsLeftFootForwardEnd,
    /// <summary>Golden Torizo, facing left with its right foot forward, throws a sonic boom with each arm.</summary>
    GoldenFacingLeftSonicBoomsRightFootForwardThrowing,
    /// <summary>Golden Torizo, facing left with its right foot forward, walks left on after its sonic booms, left leg first.</summary>
    GoldenFacingLeftSonicBoomsRightFootForwardEnd,
    /// <summary>Golden Torizo, facing left with its left foot forward, throws a sonic boom with each arm.</summary>
    GoldenFacingLeftSonicBoomsLeftFootForwardThrowing,
    /// <summary>Golden Torizo, facing left with its left foot forward, walks left on after its sonic booms, right leg first.</summary>
    GoldenFacingLeftSonicBoomsLeftFootForwardEnd,
    /// <summary>Golden Torizo, facing right with its left foot forward, starts spewing: it lowers its head, opens its mouth, thrusts and draws back its arm.</summary>
    GoldenFacingRightSpewingChozoOrbsLeftFootForward,
    /// <summary>Golden Torizo, facing right with its left foot forward, lowers its arm after spewing and walks right on, right leg first.</summary>
    GoldenFacingRightSpewingChozoOrbsLeftFootForwardEnd,
    /// <summary>Golden Torizo, facing right with its right foot forward, starts spewing: it lowers its head, opens its mouth, thrusts and draws back its arm.</summary>
    GoldenFacingRightSpewingChozoOrbsRightFootForward,
    /// <summary>Golden Torizo, facing right with its right foot forward, lowers its arm after spewing and walks right on, left leg first.</summary>
    GoldenFacingRightSpewingChozoOrbsRightFootForwardEnd,
    /// <summary>Golden Torizo, facing right with its left foot forward, throws a sonic boom with each arm.</summary>
    GoldenFacingRightSonicBoomsLeftFootForwardThrowing,
    /// <summary>Golden Torizo, facing right with its left foot forward, walks right on after its sonic booms, right leg first.</summary>
    GoldenFacingRightSonicBoomsLeftFootForwardEnd,
    /// <summary>Golden Torizo, facing right with its right foot forward, throws a sonic boom with each arm.</summary>
    GoldenFacingRightSonicBoomsRightFootForwardThrowing,
    /// <summary>Golden Torizo, facing right with its right foot forward, walks right on after its sonic booms, left leg first.</summary>
    GoldenFacingRightSonicBoomsRightFootForwardEnd,
    /// <summary>Golden Torizo, landed from a backward jump facing left with its left foot forward, chooses its attack.</summary>
    GoldenFacingLeftLandedBackwardOnLeftFoot,
    /// <summary>Golden Torizo, landed from a backward jump facing left with its right foot forward, chooses its attack.</summary>
    GoldenFacingLeftLandedBackwardOnRightFoot,
    /// <summary>Golden Torizo, landed from a backward jump facing right with its right foot forward, chooses its attack.</summary>
    GoldenFacingRightLandedBackwardOnRightFoot,
    /// <summary>Golden Torizo, landed from a backward jump facing right with its left foot forward, chooses its attack.</summary>
    GoldenFacingRightLandedBackwardOnLeftFoot,
    /// <summary>Golden Torizo, facing left with its left leg forward, catches a Super Missile and throws it back.</summary>
    GoldenFacingLeftCatchingSuperMissileLeftLegForward,
    /// <summary>Golden Torizo, facing left with its right leg forward, catches a Super Missile and throws it back.</summary>
    GoldenFacingLeftCatchingSuperMissileRightLegForward,
    /// <summary>Golden Torizo, facing right with its right leg forward, catches a Super Missile and throws it back.</summary>
    GoldenFacingRightCatchingSuperMissileRightLegForward,
    /// <summary>Golden Torizo, facing right with its left leg forward, catches a Super Missile and throws it back.</summary>
    GoldenFacingRightCatchingSuperMissileLeftLegForward,
    /// <summary>Golden Torizo, facing left, sits down on Samus morphed behind it and stands again.</summary>
    GoldenFacingLeftSittingDownAttack,
    /// <summary>Golden Torizo, facing right, sits down on Samus morphed behind it and stands again.</summary>
    GoldenFacingRightSittingDownAttack,
    /// <summary>Golden Torizo stands its ground and opens its chest.</summary>
    GoldenReleasingEggs,
    /// <summary>Golden Torizo's open chest releases an egg with its sound, then waits.</summary>
    GoldenEggLaying,
    /// <summary>Golden Torizo holds its chest open a moment, which it does again while any egg remains.</summary>
    GoldenChestHeldOpen,
    /// <summary>Golden Torizo closes its chest and resumes the walk it interrupted.</summary>
    GoldenChestClosing,
    /// <summary>Golden Torizo falls still, locks its animation, waits and sounds its eye beam.</summary>
    GoldenEyeBeamAttack,
    /// <summary>Golden Torizo fires its eye beam and waits.</summary>
    GoldenEyeBeamFiring,
    /// <summary>Golden Torizo waits after its eye beam's shots.</summary>
    GoldenEyeBeamPause,
    /// <summary>Golden Torizo's eyes glow through their four looks.</summary>
    GoldenEyeBeamGlow,
    /// <summary>Golden Torizo's eye beam's explosions show for a while, then it unlocks its animation and resumes the walk it interrupted.</summary>
    GoldenEyeBeamEnd,
    /// <summary>Golden Torizo, dodging, turns around through facing the viewer with its animation locked.</summary>
    DodgeTurning,
    /// <summary>Golden Torizo's step left on its left leg, deciding as it lifts its leg whether to sit, leap, jump back, stagger or fire its eye beam, and as it reaches whether to lay eggs or jump back.</summary>
    GoldenWalkingLeftLeftLegMoving,
    /// <summary>Golden Torizo's step right on its right leg, deciding as it lifts its leg whether to sit, leap, jump back, stagger or fire its eye beam, and as it reaches whether to lay eggs or jump back.</summary>
    GoldenWalkingRightRightLegMoving,
    /// <summary>Golden Torizo's step left on its right leg, deciding as it lifts its leg whether to leap, jump back, stagger or fire its eye beam, and as it reaches whether to lay eggs or jump back.</summary>
    GoldenWalkingLeftRightLegMoving,
    /// <summary>Golden Torizo's step right on its left leg, deciding as it lifts its leg whether to leap, jump back, stagger or fire its eye beam, as it reaches whether to lay eggs, and as it plants whether to jump back.</summary>
    GoldenWalkingRightLeftLegMoving,
}
