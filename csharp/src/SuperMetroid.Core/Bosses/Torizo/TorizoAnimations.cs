using System.Collections.Frozen;
using SuperMetroid.Core.Animation;
using A = SuperMetroid.Core.Bosses.Torizo.TorizoAnimation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's body animations (#641, #1600, #1788): each animation's steps, each frame showing a
/// <see cref="EnemyFrameId"/>, each cue a <see cref="TorizoCue"/> with the values it acts on, each artwork change naming
/// the <see cref="TorizoArtworkLook"/> it shows; the phases it plays them in, each a run of its steps played a number of
/// times; and whether it then stops, loops or holds. A cue that pauses for a fixed time is followed by a wait of that
/// length. The runs several animations play alike are one array of steps each reuses (<see cref="TorizoSharedSteps"/>).
/// </summary>
internal static class TorizoAnimations
{
    /// <summary>Every animation's steps.</summary>
    internal static AnimationSegments<TorizoAnimation, EnemyFrameId, TorizoCue> Segments { get; } = new(
        new Dictionary<TorizoAnimation, AnimationBeat<EnemyFrameId, TorizoCue>[]>(
            [.. TorizoEncounterAnimations.Segments, .. TorizoFacingLeftAnimations.Segments, .. TorizoFacingRightAnimations.Segments,
             .. GoldenTorizoAnimations.Segments]));

    /// <summary>How each animation plays its own steps.</summary>
    internal static IReadOnlyDictionary<TorizoAnimation, Animation<TorizoAnimation>> StartingAt { get; } = new Dictionary<TorizoAnimation, Animation<TorizoAnimation>>
    {
        [A.BombTorizoGutBlowingUp] = new([new(A.BombTorizoGutBlowingUp, 0, 12, 1)], AnimationEnd.Holds),
        [A.BombTorizoFaceBlowingUp] = new([new(A.BombTorizoFaceBlowingUp, 0, 18, 1)], AnimationEnd.Holds),
        [A.Dying] = new([new(A.Dying, 0, 2, 1), new(A.Dying, 2, 3, 8), new(A.Dying, 5, 1, 1), new(A.Dying, 6, 21, 14), new(A.Dying, 27, 2, 1)], AnimationEnd.Stops),
        [A.BombTorizoDormant] = new([new(A.BombTorizoDormant, 0, 4, 1)], AnimationEnd.Holds),
        [A.BombTorizoAwakening] = new([new(A.BombTorizoAwakening, 0, 10, 1), new(A.BombTorizoAwakening, 10, 8, 2), new(A.BombTorizoAwakening, 18, 13, 1), new(A.BombTorizoAwakening, 31, 2, 16), new(A.BombTorizoAwakening, 33, 3, 1), new(A.BombTorizoAwakening, 36, 20, 1), new(A.BombTorizoAwakening, 56, 20, 1)], AnimationEnd.LoopsFrom(5)),
        [A.Stunned] = new([new(A.Stunned, 0, 3, 1), new(A.Stunned, 3, 8, 2), new(A.Stunned, 11, 5, 1)], AnimationEnd.Holds),
        [A.TurningLeft] = new([new(A.TurningLeft, 0, 3, 1), new(A.TurningLeft, 3, 20, 1), new(A.TurningLeft, 23, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacingLeftSpewingChozoOrbsRightFootForward] = new([new(A.FacingLeftSpewingChozoOrbsRightFootForward, 0, 8, 1), new(A.FacingLeftSpewingChozoOrbsRightFootForward, 8, 2, 3), new(A.FacingLeftSpewingChozoOrbsRightFootForward, 10, 6, 1)], AnimationEnd.Holds),
        [A.FacingLeftSpewingChozoOrbsLeftFootForward] = new([new(A.FacingLeftSpewingChozoOrbsLeftFootForward, 0, 8, 1), new(A.FacingLeftSpewingChozoOrbsLeftFootForward, 8, 2, 3), new(A.FacingLeftSpewingChozoOrbsLeftFootForward, 10, 6, 1)], AnimationEnd.Holds),
        [A.FacingLeftSonicBoomsRightFootForward] = new([new(A.FacingLeftSonicBoomsRightFootForward, 0, 1, 1), new(A.FacingLeftSonicBoomsRightFootForward, 1, 22, 4), new(A.FacingLeftSonicBoomsRightFootForward, 23, 2, 1)], AnimationEnd.Holds),
        [A.FacingLeftSonicBoomsLeftFootForward] = new([new(A.FacingLeftSonicBoomsLeftFootForward, 0, 1, 1), new(A.FacingLeftSonicBoomsLeftFootForward, 1, 22, 4), new(A.FacingLeftSonicBoomsLeftFootForward, 23, 2, 1)], AnimationEnd.Holds),
        [A.FacingLeftExplosiveSwipeRightFootForward] = new([new(A.FacingLeftExplosiveSwipeRightFootForward, 0, 33, 1)], AnimationEnd.Holds),
        [A.FacingLeftExplosiveSwipeLeftFootForward] = new([new(A.FacingLeftExplosiveSwipeLeftFootForward, 0, 33, 1)], AnimationEnd.Holds),
        [A.FacingLeftJumpingForward] = new([new(A.FacingLeftJumpingForward, 0, 4, 1), new(A.FacingLeftJumpingForward, 4, 2, 1), new(A.FacingLeftJumpingForward, 6, 2, 1), new(A.FacingLeftJumpingForward, 8, 1, 1)], AnimationEnd.LoopsFrom(3)),
        [A.FacingLeftFalling] = new([new(A.FacingLeftFalling, 0, 2, 1), new(A.FacingLeftFalling, 2, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacingLeftLanding] = new([new(A.FacingLeftLanding, 0, 3, 1), new(A.FacingLeftLanding, 3, 20, 1), new(A.FacingLeftLanding, 23, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacingLeftJumpingBackwardToLeftFoot] = new([new(A.FacingLeftJumpingBackwardToLeftFoot, 0, 4, 1), new(A.FacingLeftJumpingBackwardToLeftFoot, 4, 2, 1), new(A.FacingLeftJumpingBackwardToLeftFoot, 6, 2, 1), new(A.FacingLeftJumpingBackwardToLeftFoot, 8, 1, 1)], AnimationEnd.LoopsFrom(3)),
        [A.FacingLeftFallingBackwardToLeftFoot] = new([new(A.FacingLeftFallingBackwardToLeftFoot, 0, 2, 1), new(A.FacingLeftFallingBackwardToLeftFoot, 2, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacingLeftLandingBackwardOnLeftFoot] = new([new(A.FacingLeftLandingBackwardOnLeftFoot, 0, 4, 1), new(A.FacingLeftLandingBackwardOnLeftFoot, 4, 20, 1), new(A.FacingLeftLandingBackwardOnLeftFoot, 24, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacingLeftJumpingBackwardToRightFoot] = new([new(A.FacingLeftJumpingBackwardToRightFoot, 0, 4, 1), new(A.FacingLeftJumpingBackwardToRightFoot, 4, 2, 1), new(A.FacingLeftJumpingBackwardToRightFoot, 6, 2, 1), new(A.FacingLeftJumpingBackwardToRightFoot, 8, 1, 1)], AnimationEnd.LoopsFrom(3)),
        [A.FacingLeftFallingBackwardToRightFoot] = new([new(A.FacingLeftFallingBackwardToRightFoot, 0, 2, 1), new(A.FacingLeftFallingBackwardToRightFoot, 2, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacingLeftLandingBackwardOnRightFoot] = new([new(A.FacingLeftLandingBackwardOnRightFoot, 0, 4, 1), new(A.FacingLeftLandingBackwardOnRightFoot, 4, 20, 1), new(A.FacingLeftLandingBackwardOnRightFoot, 24, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacelessTurningLeft] = new([new(A.FacelessTurningLeft, 0, 3, 1), new(A.FacelessTurningLeft, 3, 16, 1), new(A.FacelessTurningLeft, 19, 16, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacingLeftFacelessWalkingRightLegMoving] = new([new(A.FacingLeftFacelessWalkingRightLegMoving, 0, 16, 1), new(A.FacingLeftFacelessWalkingRightLegMoving, 16, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [A.FacingLeftFacelessWalkingLeftLegMoving] = new([new(A.FacingLeftFacelessWalkingLeftLegMoving, 0, 16, 1), new(A.FacingLeftFacelessWalkingLeftLegMoving, 16, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [A.BombTorizoDormantFacingRight] = new([new(A.BombTorizoDormantFacingRight, 0, 3, 1)], AnimationEnd.Holds),
        [A.BombTorizoAwakeningFacingRight] = new([new(A.BombTorizoAwakeningFacingRight, 0, 14, 1), new(A.BombTorizoAwakeningFacingRight, 14, 20, 1), new(A.BombTorizoAwakeningFacingRight, 34, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [A.TurningRight] = new([new(A.TurningRight, 0, 3, 1), new(A.TurningRight, 3, 21, 1), new(A.TurningRight, 24, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacingRightSpewingChozoOrbsLeftFootForward] = new([new(A.FacingRightSpewingChozoOrbsLeftFootForward, 0, 8, 1), new(A.FacingRightSpewingChozoOrbsLeftFootForward, 8, 2, 3), new(A.FacingRightSpewingChozoOrbsLeftFootForward, 10, 6, 1)], AnimationEnd.Holds),
        [A.FacingRightSpewingChozoOrbsRightFootForward] = new([new(A.FacingRightSpewingChozoOrbsRightFootForward, 0, 8, 1), new(A.FacingRightSpewingChozoOrbsRightFootForward, 8, 2, 3), new(A.FacingRightSpewingChozoOrbsRightFootForward, 10, 6, 1)], AnimationEnd.Holds),
        [A.FacingRightSonicBoomsLeftFootForward] = new([new(A.FacingRightSonicBoomsLeftFootForward, 0, 1, 1), new(A.FacingRightSonicBoomsLeftFootForward, 1, 22, 4), new(A.FacingRightSonicBoomsLeftFootForward, 23, 2, 1)], AnimationEnd.Holds),
        [A.FacingRightSonicBoomsRightFootForward] = new([new(A.FacingRightSonicBoomsRightFootForward, 0, 1, 1), new(A.FacingRightSonicBoomsRightFootForward, 1, 22, 4), new(A.FacingRightSonicBoomsRightFootForward, 23, 2, 1)], AnimationEnd.Holds),
        [A.FacingRightExplosiveSwipeLeftFootForward] = new([new(A.FacingRightExplosiveSwipeLeftFootForward, 0, 33, 1)], AnimationEnd.Holds),
        [A.FacingRightExplosiveSwipeRightFootForward] = new([new(A.FacingRightExplosiveSwipeRightFootForward, 0, 33, 1)], AnimationEnd.Holds),
        [A.FacingRightJumpingForward] = new([new(A.FacingRightJumpingForward, 0, 4, 1), new(A.FacingRightJumpingForward, 4, 2, 1), new(A.FacingRightJumpingForward, 6, 2, 1), new(A.FacingRightJumpingForward, 8, 1, 1)], AnimationEnd.LoopsFrom(3)),
        [A.FacingRightFalling] = new([new(A.FacingRightFalling, 0, 2, 1), new(A.FacingRightFalling, 2, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacingRightLanding] = new([new(A.FacingRightLanding, 0, 3, 1), new(A.FacingRightLanding, 3, 20, 1), new(A.FacingRightLanding, 23, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacingRightJumpingBackwardToRightFoot] = new([new(A.FacingRightJumpingBackwardToRightFoot, 0, 4, 1), new(A.FacingRightJumpingBackwardToRightFoot, 4, 2, 1), new(A.FacingRightJumpingBackwardToRightFoot, 6, 2, 1), new(A.FacingRightJumpingBackwardToRightFoot, 8, 1, 1)], AnimationEnd.LoopsFrom(3)),
        [A.FacingRightFallingBackwardToRightFoot] = new([new(A.FacingRightFallingBackwardToRightFoot, 0, 2, 1), new(A.FacingRightFallingBackwardToRightFoot, 2, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacingRightLandingBackwardOnRightFoot] = new([new(A.FacingRightLandingBackwardOnRightFoot, 0, 4, 1), new(A.FacingRightLandingBackwardOnRightFoot, 4, 21, 1), new(A.FacingRightLandingBackwardOnRightFoot, 25, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacingRightJumpingBackwardToLeftFoot] = new([new(A.FacingRightJumpingBackwardToLeftFoot, 0, 4, 1), new(A.FacingRightJumpingBackwardToLeftFoot, 4, 2, 1), new(A.FacingRightJumpingBackwardToLeftFoot, 6, 2, 1), new(A.FacingRightJumpingBackwardToLeftFoot, 8, 1, 1)], AnimationEnd.LoopsFrom(3)),
        [A.FacingRightFallingBackwardToLeftFoot] = new([new(A.FacingRightFallingBackwardToLeftFoot, 0, 2, 1), new(A.FacingRightFallingBackwardToLeftFoot, 2, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacingRightLandingBackwardOnLeftFoot] = new([new(A.FacingRightLandingBackwardOnLeftFoot, 0, 4, 1), new(A.FacingRightLandingBackwardOnLeftFoot, 4, 20, 1), new(A.FacingRightLandingBackwardOnLeftFoot, 24, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacelessTurningRight] = new([new(A.FacelessTurningRight, 0, 3, 1), new(A.FacelessTurningRight, 3, 16, 1), new(A.FacelessTurningRight, 19, 16, 1)], AnimationEnd.LoopsFrom(1)),
        [A.FacingRightFacelessWalkingLeftLegMoving] = new([new(A.FacingRightFacelessWalkingLeftLegMoving, 0, 16, 1), new(A.FacingRightFacelessWalkingLeftLegMoving, 16, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [A.FacingRightFacelessWalkingRightLegMoving] = new([new(A.FacingRightFacelessWalkingRightLegMoving, 0, 16, 1), new(A.FacingRightFacelessWalkingRightLegMoving, 16, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [A.GoldenTorizoDormant] = new([new(A.GoldenTorizoDormant, 0, 5, 1)], AnimationEnd.Holds),
        [A.GoldenTorizoAwakening] = new([new(A.GoldenTorizoAwakening, 0, 1, 1), new(A.GoldenTorizoAwakening, 1, 19, 1), new(A.GoldenTorizoAwakening, 20, 8, 2), new(A.GoldenTorizoAwakening, 28, 13, 1), new(A.GoldenTorizoAwakening, 41, 2, 16), new(A.GoldenTorizoAwakening, 43, 3, 1), new(A.GoldenTorizoAwakening, 46, 21, 1), new(A.GoldenTorizoAwakening, 67, 20, 1)], AnimationEnd.LoopsFrom(6)),
        [A.GoldenFacingLeftSpewingChozoOrbsRightFootForward] = new([new(A.GoldenFacingLeftSpewingChozoOrbsRightFootForward, 0, 7, 1), new(A.GoldenFacingLeftSpewingChozoOrbsRightFootForward, 7, 3, 6), new(A.GoldenFacingLeftSpewingChozoOrbsRightFootForward, 10, 6, 1)], AnimationEnd.Holds),
        [A.GoldenFacingLeftSpewingChozoOrbsLeftFootForward] = new([new(A.GoldenFacingLeftSpewingChozoOrbsLeftFootForward, 0, 7, 1), new(A.GoldenFacingLeftSpewingChozoOrbsLeftFootForward, 7, 3, 6), new(A.GoldenFacingLeftSpewingChozoOrbsLeftFootForward, 10, 6, 1)], AnimationEnd.Holds),
        [A.GoldenFacingLeftSonicBoomsRightFootForward] = new([new(A.GoldenFacingLeftSonicBoomsRightFootForward, 0, 1, 1), new(A.GoldenFacingLeftSonicBoomsRightFootForward, 1, 22, 4), new(A.GoldenFacingLeftSonicBoomsRightFootForward, 23, 2, 1)], AnimationEnd.Holds),
        [A.GoldenFacingLeftSonicBoomsLeftFootForward] = new([new(A.GoldenFacingLeftSonicBoomsLeftFootForward, 0, 1, 1), new(A.GoldenFacingLeftSonicBoomsLeftFootForward, 1, 22, 4), new(A.GoldenFacingLeftSonicBoomsLeftFootForward, 23, 2, 1)], AnimationEnd.Holds),
        [A.GoldenFacingRightSpewingChozoOrbsLeftFootForward] = new([new(A.GoldenFacingRightSpewingChozoOrbsLeftFootForward, 0, 7, 1), new(A.GoldenFacingRightSpewingChozoOrbsLeftFootForward, 7, 3, 6), new(A.GoldenFacingRightSpewingChozoOrbsLeftFootForward, 10, 6, 1)], AnimationEnd.Holds),
        [A.GoldenFacingRightSpewingChozoOrbsRightFootForward] = new([new(A.GoldenFacingRightSpewingChozoOrbsRightFootForward, 0, 7, 1), new(A.GoldenFacingRightSpewingChozoOrbsRightFootForward, 7, 3, 6), new(A.GoldenFacingRightSpewingChozoOrbsRightFootForward, 10, 6, 1)], AnimationEnd.Holds),
        [A.GoldenFacingRightSonicBoomsLeftFootForward] = new([new(A.GoldenFacingRightSonicBoomsLeftFootForward, 0, 1, 1), new(A.GoldenFacingRightSonicBoomsLeftFootForward, 1, 22, 4), new(A.GoldenFacingRightSonicBoomsLeftFootForward, 23, 2, 1)], AnimationEnd.Holds),
        [A.GoldenFacingRightSonicBoomsRightFootForward] = new([new(A.GoldenFacingRightSonicBoomsRightFootForward, 0, 1, 1), new(A.GoldenFacingRightSonicBoomsRightFootForward, 1, 22, 4), new(A.GoldenFacingRightSonicBoomsRightFootForward, 23, 2, 1)], AnimationEnd.Holds),
        [A.GoldenFacingLeftLandedBackwardOnLeftFoot] = new([new(A.GoldenFacingLeftLandedBackwardOnLeftFoot, 0, 1, 1), new(A.GoldenFacingLeftLandedBackwardOnLeftFoot, 1, 20, 1), new(A.GoldenFacingLeftLandedBackwardOnLeftFoot, 21, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [A.GoldenFacingLeftLandedBackwardOnRightFoot] = new([new(A.GoldenFacingLeftLandedBackwardOnRightFoot, 0, 1, 1), new(A.GoldenFacingLeftLandedBackwardOnRightFoot, 1, 21, 1), new(A.GoldenFacingLeftLandedBackwardOnRightFoot, 22, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [A.GoldenFacingRightLandedBackwardOnRightFoot] = new([new(A.GoldenFacingRightLandedBackwardOnRightFoot, 0, 1, 1), new(A.GoldenFacingRightLandedBackwardOnRightFoot, 1, 20, 1), new(A.GoldenFacingRightLandedBackwardOnRightFoot, 21, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [A.GoldenFacingRightLandedBackwardOnLeftFoot] = new([new(A.GoldenFacingRightLandedBackwardOnLeftFoot, 0, 1, 1), new(A.GoldenFacingRightLandedBackwardOnLeftFoot, 1, 21, 1), new(A.GoldenFacingRightLandedBackwardOnLeftFoot, 22, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [A.GoldenFacingLeftCatchingSuperMissileLeftLegForward] = new([new(A.GoldenFacingLeftCatchingSuperMissileLeftLegForward, 0, 25, 1), new(A.GoldenFacingLeftCatchingSuperMissileLeftLegForward, 25, 20, 1), new(A.GoldenFacingLeftCatchingSuperMissileLeftLegForward, 45, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [A.GoldenFacingLeftCatchingSuperMissileRightLegForward] = new([new(A.GoldenFacingLeftCatchingSuperMissileRightLegForward, 0, 25, 1), new(A.GoldenFacingLeftCatchingSuperMissileRightLegForward, 25, 21, 1), new(A.GoldenFacingLeftCatchingSuperMissileRightLegForward, 46, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [A.GoldenFacingRightCatchingSuperMissileRightLegForward] = new([new(A.GoldenFacingRightCatchingSuperMissileRightLegForward, 0, 23, 1), new(A.GoldenFacingRightCatchingSuperMissileRightLegForward, 23, 20, 1), new(A.GoldenFacingRightCatchingSuperMissileRightLegForward, 43, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [A.GoldenFacingRightCatchingSuperMissileLeftLegForward] = new([new(A.GoldenFacingRightCatchingSuperMissileLeftLegForward, 0, 23, 1), new(A.GoldenFacingRightCatchingSuperMissileLeftLegForward, 23, 21, 1), new(A.GoldenFacingRightCatchingSuperMissileLeftLegForward, 44, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [A.GoldenFacingLeftSittingDownAttack] = new([new(A.GoldenFacingLeftSittingDownAttack, 0, 26, 1), new(A.GoldenFacingLeftSittingDownAttack, 26, 21, 1), new(A.GoldenFacingLeftSittingDownAttack, 47, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [A.GoldenFacingRightSittingDownAttack] = new([new(A.GoldenFacingRightSittingDownAttack, 0, 26, 1), new(A.GoldenFacingRightSittingDownAttack, 26, 21, 1), new(A.GoldenFacingRightSittingDownAttack, 47, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [A.GoldenReleasingEggs] = new([new(A.GoldenReleasingEggs, 0, 10, 1), new(A.GoldenReleasingEggs, 10, 3, 6), new(A.GoldenReleasingEggs, 13, 15, 1)], AnimationEnd.Holds),
        [A.GoldenEyeBeamAttack] = new([new(A.GoldenEyeBeamAttack, 0, 4, 1), new(A.GoldenEyeBeamAttack, 4, 1, 1), new(A.GoldenEyeBeamAttack, 5, 2, 4), new(A.GoldenEyeBeamAttack, 7, 1, 1), new(A.GoldenEyeBeamAttack, 8, 8, 2), new(A.GoldenEyeBeamAttack, 16, 6, 1)], AnimationEnd.Holds),
        [A.GoldenDodgeTurningLeft] = new([new(A.GoldenDodgeTurningLeft, 0, 5, 1), new(A.GoldenDodgeTurningLeft, 5, 20, 1), new(A.GoldenDodgeTurningLeft, 25, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [A.GoldenTurningLeft] = new([new(A.GoldenTurningLeft, 0, 3, 1), new(A.GoldenTurningLeft, 3, 20, 1), new(A.GoldenTurningLeft, 23, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [A.GoldenWalkingLeftLeftLegMoving] = new([new(A.GoldenWalkingLeftLeftLegMoving, 0, 21, 1), new(A.GoldenWalkingLeftLeftLegMoving, 21, 20, 1)], AnimationEnd.LoopsFrom(0)),
        [A.GoldenDodgeTurningRight] = new([new(A.GoldenDodgeTurningRight, 0, 5, 1), new(A.GoldenDodgeTurningRight, 5, 20, 1), new(A.GoldenDodgeTurningRight, 25, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [A.GoldenTurningRight] = new([new(A.GoldenTurningRight, 0, 3, 1), new(A.GoldenTurningRight, 3, 20, 1), new(A.GoldenTurningRight, 23, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [A.GoldenWalkingRightRightLegMoving] = new([new(A.GoldenWalkingRightRightLegMoving, 0, 21, 1), new(A.GoldenWalkingRightRightLegMoving, 21, 20, 1)], AnimationEnd.LoopsFrom(0)),
    }.ToFrozenDictionary();

    /// <summary>Each animation's identity, by the animation itself.</summary>
    private static readonly FrozenDictionary<Animation<TorizoAnimation>, TorizoAnimation> Identities =
        StartingAt.ToFrozenDictionary<KeyValuePair<A, Animation<A>>, Animation<A>, A>(pair => pair.Value, pair => pair.Key, ReferenceEqualityComparer.Instance);

    /// <summary>
    /// The animation each sleeping animation resumes into when the AI wakes it, as Bomb Torizo's wake and Golden Torizo's
    /// trigger do; no other animation sleeps.
    /// </summary>
    private static readonly FrozenDictionary<A, A> ResumesAfterSleep = new Dictionary<A, A>
    {
        [A.BombTorizoDormant] = A.BombTorizoAwakening,
        [A.BombTorizoDormantFacingRight] = A.BombTorizoAwakeningFacingRight,
        [A.GoldenTorizoDormant] = A.GoldenTorizoAwakening,
    }.ToFrozenDictionary();

    /// <summary>How <paramref name="animation"/> plays.</summary>
    internal static Animation<TorizoAnimation> StartingWith(TorizoAnimation animation) => StartingAt[animation];

    /// <summary>A playback of <paramref name="animation"/>, before its first play.</summary>
    internal static AnimationPlayback<TorizoAnimation> Start(TorizoAnimation animation) => new(StartingWith(animation), AnimationPlace.StartOfAnimation);

    /// <summary>A cue's continuation into <paramref name="animation"/>, from its start.</summary>
    internal static AnimationContinuation<TorizoAnimation> ContinueWith(TorizoAnimation animation) => new(StartingWith(animation), AnimationPlace.StartOfAnimation);

    /// <summary>A cue's continuation where <paramref name="playback"/> stood, as a return to a saved place continues there.</summary>
    internal static AnimationContinuation<TorizoAnimation> ContinueAt(AnimationPlayback<TorizoAnimation> playback) =>
        new(Ensure.NotNull(playback.Animation), playback.Place);

    /// <summary>
    /// A replay cue's continuation (<see cref="TorizoCue.Replay"/>): the start of the phase <paramref name="continuation"/>,
    /// the place just past the cue, stands in, so the run the cue is in plays again from its first step.
    /// </summary>
    internal static AnimationContinuation<TorizoAnimation> ReplayPhase(AnimationPlayback<TorizoAnimation> continuation) =>
        new(Ensure.NotNull(continuation.Animation), AnimationPlace.Before(continuation.Place.Phase));

    /// <summary>The animation <paramref name="playback"/> plays.</summary>
    internal static TorizoAnimation Playing(AnimationPlayback<TorizoAnimation> playback) => Identities[Ensure.NotNull(playback.Animation)];

    /// <summary>
    /// The animation a sleeping animation resumes into when the AI wakes it, as Bomb Torizo's wake and Golden Torizo's
    /// trigger do.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="animation"/> does not sleep.</exception>
    internal static TorizoAnimation AfterSleep(TorizoAnimation animation) =>
        ResumesAfterSleep.TryGetValue(animation, out A resumed)
            ? resumed
            : throw new ArgumentOutOfRangeException(nameof(animation), animation, "The animation does not sleep.");
}