using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoBodySegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's typed body animations (#641, #1600): each frame shows a <see cref="EnemyFrameId"/>,
/// each cue is a <see cref="TorizoCue"/> with the values it acts on, and each artwork swap names its
/// <see cref="TorizoArtworkSwap"/> entry. A cue that pauses for a fixed time is followed by a wait of that
/// length.
/// </summary>
internal static class TorizoAnimations
{
    /// <summary>Every body segment's beats.</summary>
    internal static AnimationSegments<TorizoBodySegment, EnemyFrameId, TorizoCue> Segments { get; } = new(
        new Dictionary<TorizoBodySegment, AnimationBeat<EnemyFrameId, TorizoCue>[]>(
            [.. TorizoEncounterAnimations.Segments, .. TorizoFacingLeftAnimations.Segments, .. TorizoFacingRightAnimations.Segments,
             .. GoldenTorizoAnimations.Segments]));

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<TorizoBodySegment, Animation<TorizoBodySegment>> StartingAt { get; } = new Dictionary<TorizoBodySegment, Animation<TorizoBodySegment>>
    {
        [P.SpecialCallableBlowUpBombTorizosGut] = new([new(P.SpecialCallableBlowUpBombTorizosGut, 0, 12, 1)], AnimationEnd.Holds),
        [P.CallableBlowUpBombTorizosFace] = new([new(P.CallableBlowUpBombTorizosFace, 0, 18, 1)], AnimationEnd.Holds),
        [P.DeathSequence0] = new([new(P.DeathSequence0, 0, 2, 1), new(P.DeathSequence1, 0, 3, 8), new(P.DeathSequence1, 3, 1, 1), new(P.DeathSequence2, 0, 21, 14), new(P.DeathSequence2, 21, 2, 1)], AnimationEnd.Stops),
        [P.BombTorizoInitial0] = new([new(P.BombTorizoInitial0, 0, 4, 1)], AnimationEnd.Holds),
        [P.BombTorizoInitial0Resumed] = new([new(P.BombTorizoInitial0Resumed, 0, 10, 1), new(P.BombTorizoInitial1, 0, 8, 2), new(P.BombTorizoInitial1, 8, 13, 1), new(P.BombTorizoInitial2, 0, 2, 16), new(P.BombTorizoInitial2, 2, 4, 1), new(P.FacingLeftWalkingLeftLegMoving, 0, 20, 1), new(P.FacingLeftWalkingRightLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(5)),
        [P.Stunned0] = new([new(P.Stunned0, 0, 3, 1), new(P.Stunned1, 0, 8, 2), new(P.Stunned1, 8, 5, 1)], AnimationEnd.Holds),
        [P.FacingLeftTurningLeft] = new([new(P.FacingLeftTurningLeft, 0, 3, 1), new(P.FacingLeftWalkingRightLegMoving, 0, 20, 1), new(P.FacingLeftWalkingLeftLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingLeftWalkingRightLegMoving] = new([new(P.FacingLeftWalkingRightLegMoving, 0, 20, 1), new(P.FacingLeftWalkingLeftLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingLeftWalkingLeftLegMoving] = new([new(P.FacingLeftWalkingLeftLegMoving, 0, 20, 1), new(P.FacingLeftWalkingRightLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingLeftSpewingChozoOrbsRightFootFwd0] = new([new(P.FacingLeftSpewingChozoOrbsRightFootFwd0, 0, 8, 1), new(P.FacingLeftSpewingChozoOrbsRightFootFwd1, 0, 2, 3), new(P.FacingLeftSpewingChozoOrbsRightFootFwd1, 2, 6, 1), new(P.FacingLeftSpewingChozoOrbsLeftFootFwd0, 0, 8, 1), new(P.FacingLeftSpewingChozoOrbsLeftFootFwd1, 0, 2, 3), new(P.FacingLeftSpewingChozoOrbsLeftFootFwd1, 2, 6, 1), new(P.FacingLeftSonicBoomsRightFootForward0, 0, 1, 1), new(P.FacingLeftSonicBoomsRightFootForward1, 0, 22, 4), new(P.FacingLeftSonicBoomsRightFootForward1, 22, 2, 1), new(P.FacingLeftSonicBoomsLeftFootForward0, 0, 1, 1), new(P.FacingLeftSonicBoomsLeftFootForward1, 0, 22, 4), new(P.FacingLeftSonicBoomsLeftFootForward1, 22, 2, 1), new(P.FacingLeftExplosiveSwipeRightFootForward, 0, 33, 1), new(P.FacingLeftExplosiveSwipeLeftFootForward, 0, 33, 1), new(P.FacingLeftJumpingForwards0, 0, 4, 1), new(P.FacingLeftJumpingForwards1, 0, 2, 1), new(P.FacingLeftFalling0, 0, 2, 1), new(P.FacingLeftFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(17)),
        [P.FacingLeftSpewingChozoOrbsLeftFootFwd0] = new([new(P.FacingLeftSpewingChozoOrbsLeftFootFwd0, 0, 8, 1), new(P.FacingLeftSpewingChozoOrbsLeftFootFwd1, 0, 2, 3), new(P.FacingLeftSpewingChozoOrbsLeftFootFwd1, 2, 6, 1), new(P.FacingLeftSonicBoomsRightFootForward0, 0, 1, 1), new(P.FacingLeftSonicBoomsRightFootForward1, 0, 22, 4), new(P.FacingLeftSonicBoomsRightFootForward1, 22, 2, 1), new(P.FacingLeftSonicBoomsLeftFootForward0, 0, 1, 1), new(P.FacingLeftSonicBoomsLeftFootForward1, 0, 22, 4), new(P.FacingLeftSonicBoomsLeftFootForward1, 22, 2, 1), new(P.FacingLeftExplosiveSwipeRightFootForward, 0, 33, 1), new(P.FacingLeftExplosiveSwipeLeftFootForward, 0, 33, 1), new(P.FacingLeftJumpingForwards0, 0, 4, 1), new(P.FacingLeftJumpingForwards1, 0, 2, 1), new(P.FacingLeftFalling0, 0, 2, 1), new(P.FacingLeftFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(14)),
        [P.FacingLeftSonicBoomsRightFootForward0] = new([new(P.FacingLeftSonicBoomsRightFootForward0, 0, 1, 1), new(P.FacingLeftSonicBoomsRightFootForward1, 0, 22, 4), new(P.FacingLeftSonicBoomsRightFootForward1, 22, 2, 1), new(P.FacingLeftSonicBoomsLeftFootForward0, 0, 1, 1), new(P.FacingLeftSonicBoomsLeftFootForward1, 0, 22, 4), new(P.FacingLeftSonicBoomsLeftFootForward1, 22, 2, 1), new(P.FacingLeftExplosiveSwipeRightFootForward, 0, 33, 1), new(P.FacingLeftExplosiveSwipeLeftFootForward, 0, 33, 1), new(P.FacingLeftJumpingForwards0, 0, 4, 1), new(P.FacingLeftJumpingForwards1, 0, 2, 1), new(P.FacingLeftFalling0, 0, 2, 1), new(P.FacingLeftFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(11)),
        [P.FacingLeftSonicBoomsLeftFootForward0] = new([new(P.FacingLeftSonicBoomsLeftFootForward0, 0, 1, 1), new(P.FacingLeftSonicBoomsLeftFootForward1, 0, 22, 4), new(P.FacingLeftSonicBoomsLeftFootForward1, 22, 2, 1), new(P.FacingLeftExplosiveSwipeRightFootForward, 0, 33, 1), new(P.FacingLeftExplosiveSwipeLeftFootForward, 0, 33, 1), new(P.FacingLeftJumpingForwards0, 0, 4, 1), new(P.FacingLeftJumpingForwards1, 0, 2, 1), new(P.FacingLeftFalling0, 0, 2, 1), new(P.FacingLeftFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(8)),
        [P.FacingLeftExplosiveSwipeRightFootForward] = new([new(P.FacingLeftExplosiveSwipeRightFootForward, 0, 33, 1), new(P.FacingLeftExplosiveSwipeLeftFootForward, 0, 33, 1), new(P.FacingLeftJumpingForwards0, 0, 4, 1), new(P.FacingLeftJumpingForwards1, 0, 2, 1), new(P.FacingLeftFalling0, 0, 2, 1), new(P.FacingLeftFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(5)),
        [P.FacingLeftExplosiveSwipeLeftFootForward] = new([new(P.FacingLeftExplosiveSwipeLeftFootForward, 0, 33, 1), new(P.FacingLeftJumpingForwards0, 0, 4, 1), new(P.FacingLeftJumpingForwards1, 0, 2, 1), new(P.FacingLeftFalling0, 0, 2, 1), new(P.FacingLeftFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(4)),
        [P.FacingLeftJumpingForwards0] = new([new(P.FacingLeftJumpingForwards0, 0, 4, 1), new(P.FacingLeftJumpingForwards1, 0, 2, 1), new(P.FacingLeftFalling0, 0, 2, 1), new(P.FacingLeftFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(3)),
        [P.FacingLeftJumpingForwards1] = new([new(P.FacingLeftJumpingForwards1, 0, 2, 1), new(P.FacingLeftFalling0, 0, 2, 1), new(P.FacingLeftFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(2)),
        [P.FacingLeftFalling0] = new([new(P.FacingLeftFalling0, 0, 2, 1), new(P.FacingLeftFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingLeftFalling1] = new([new(P.FacingLeftFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingLeftFalling2] = new([new(P.FacingLeftFalling2, 0, 3, 1), new(P.FacingLeftWalkingLeftLegMoving, 0, 20, 1), new(P.FacingLeftWalkingRightLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingLeftJumpingBackwardLandLeftFootFwd0] = new([new(P.FacingLeftJumpingBackwardLandLeftFootFwd0, 0, 4, 1), new(P.FacingLeftJumpingBackwardLandLeftFootFwd1, 0, 2, 1), new(P.FacingLeftJumpingBackwardLandLeftFootFwd2, 0, 2, 1), new(P.FacingLeftJumpingBackwardLandLeftFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(3)),
        [P.FacingLeftJumpingBackwardLandLeftFootFwd1] = new([new(P.FacingLeftJumpingBackwardLandLeftFootFwd1, 0, 2, 1), new(P.FacingLeftJumpingBackwardLandLeftFootFwd2, 0, 2, 1), new(P.FacingLeftJumpingBackwardLandLeftFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(2)),
        [P.FacingLeftJumpingBackwardLandLeftFootFwd2] = new([new(P.FacingLeftJumpingBackwardLandLeftFootFwd2, 0, 2, 1), new(P.FacingLeftJumpingBackwardLandLeftFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingLeftJumpingBackwardLandLeftFootFwd3] = new([new(P.FacingLeftJumpingBackwardLandLeftFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingLeftJumpingBackwardLandLeftFootFwd4] = new([new(P.FacingLeftJumpingBackwardLandLeftFootFwd4, 0, 4, 1), new(P.FacingLeftWalkingRightLegMoving, 0, 20, 1), new(P.FacingLeftWalkingLeftLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingLeftJumpingBackwardRightFootFwd0] = new([new(P.FacingLeftJumpingBackwardRightFootFwd0, 0, 4, 1), new(P.FacingLeftJumpingBackwardRightFootFwd1, 0, 2, 1), new(P.FacingLeftJumpingBackwardRightFootFwd2, 0, 2, 1), new(P.FacingLeftJumpingBackwardRightFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(3)),
        [P.FacingLeftJumpingBackwardRightFootFwd1] = new([new(P.FacingLeftJumpingBackwardRightFootFwd1, 0, 2, 1), new(P.FacingLeftJumpingBackwardRightFootFwd2, 0, 2, 1), new(P.FacingLeftJumpingBackwardRightFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(2)),
        [P.FacingLeftJumpingBackwardRightFootFwd2] = new([new(P.FacingLeftJumpingBackwardRightFootFwd2, 0, 2, 1), new(P.FacingLeftJumpingBackwardRightFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingLeftJumpingBackwardRightFootFwd3] = new([new(P.FacingLeftJumpingBackwardRightFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingLeftJumpingBackwardRightFootFwd4] = new([new(P.FacingLeftJumpingBackwardRightFootFwd4, 0, 4, 1), new(P.FacingLeftWalkingLeftLegMoving, 0, 20, 1), new(P.FacingLeftWalkingRightLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingLeftFacelessTurningLeft] = new([new(P.FacingLeftFacelessTurningLeft, 0, 3, 1), new(P.FacingLeftFacelessWalkingRightLegMoving, 0, 16, 1), new(P.FacingLeftFacelessWalkingLeftLegMoving, 0, 16, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingLeftFacelessWalkingRightLegMoving] = new([new(P.FacingLeftFacelessWalkingRightLegMoving, 0, 16, 1), new(P.FacingLeftFacelessWalkingLeftLegMoving, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingLeftFacelessWalkingLeftLegMoving] = new([new(P.FacingLeftFacelessWalkingLeftLegMoving, 0, 16, 1), new(P.FacingLeftFacelessWalkingRightLegMoving, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingLeftFacelessWalkingLeftLegMovingTrailing] = new([new(P.FacingLeftFacelessWalkingLeftLegMovingTrailing, 0, 3, 1)], AnimationEnd.Holds),
        [P.FacingLeftFacelessWalkingLeftLegMovingTrailingResumed] = new([new(P.FacingLeftFacelessWalkingLeftLegMovingTrailingResumed, 0, 14, 1), new(P.FacingRightWalkingRightLegMoving, 0, 20, 1), new(P.FacingRightWalkingLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingRightTurningRight] = new([new(P.FacingRightTurningRight, 0, 3, 1), new(P.FacingRightWalkingLeftLegMoving, 0, 21, 1), new(P.FacingRightWalkingRightLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingRightWalkingLeftLegMoving] = new([new(P.FacingRightWalkingLeftLegMoving, 0, 21, 1), new(P.FacingRightWalkingRightLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightWalkingRightLegMoving] = new([new(P.FacingRightWalkingRightLegMoving, 0, 20, 1), new(P.FacingRightWalkingLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightSpewingChozoOrbsLeftFootFwd0] = new([new(P.FacingRightSpewingChozoOrbsLeftFootFwd0, 0, 8, 1), new(P.FacingRightSpewingChozoOrbsLeftFootFwd1, 0, 2, 3), new(P.FacingRightSpewingChozoOrbsLeftFootFwd1, 2, 6, 1), new(P.FacingRightSpewingChozoOrbsRightFootFwd0, 0, 8, 1), new(P.FacingRightSpewingChozoOrbsRightFootFwd1, 0, 2, 3), new(P.FacingRightSpewingChozoOrbsRightFootFwd1, 2, 6, 1), new(P.FacingRightSonicBoomsLeftFootForward0, 0, 1, 1), new(P.FacingRightSonicBoomsLeftFootForward1, 0, 22, 4), new(P.FacingRightSonicBoomsLeftFootForward1, 22, 2, 1), new(P.FacingRightSonicBoomsRightFootForward0, 0, 1, 1), new(P.FacingRightSonicBoomsRightFootForward1, 0, 22, 4), new(P.FacingRightSonicBoomsRightFootForward1, 22, 2, 1), new(P.FacingRightExplosiveSwipeLeftFootForward, 0, 33, 1), new(P.FacingRightExplosiveSwipeRightFootForward, 0, 33, 1), new(P.FacingRightJumpingForwards0, 0, 4, 1), new(P.FacingRightJumpingForwards1, 0, 2, 1), new(P.FacingRightFalling0, 0, 2, 1), new(P.FacingRightFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(17)),
        [P.FacingRightSpewingChozoOrbsRightFootFwd0] = new([new(P.FacingRightSpewingChozoOrbsRightFootFwd0, 0, 8, 1), new(P.FacingRightSpewingChozoOrbsRightFootFwd1, 0, 2, 3), new(P.FacingRightSpewingChozoOrbsRightFootFwd1, 2, 6, 1), new(P.FacingRightSonicBoomsLeftFootForward0, 0, 1, 1), new(P.FacingRightSonicBoomsLeftFootForward1, 0, 22, 4), new(P.FacingRightSonicBoomsLeftFootForward1, 22, 2, 1), new(P.FacingRightSonicBoomsRightFootForward0, 0, 1, 1), new(P.FacingRightSonicBoomsRightFootForward1, 0, 22, 4), new(P.FacingRightSonicBoomsRightFootForward1, 22, 2, 1), new(P.FacingRightExplosiveSwipeLeftFootForward, 0, 33, 1), new(P.FacingRightExplosiveSwipeRightFootForward, 0, 33, 1), new(P.FacingRightJumpingForwards0, 0, 4, 1), new(P.FacingRightJumpingForwards1, 0, 2, 1), new(P.FacingRightFalling0, 0, 2, 1), new(P.FacingRightFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(14)),
        [P.FacingRightSonicBoomsLeftFootForward0] = new([new(P.FacingRightSonicBoomsLeftFootForward0, 0, 1, 1), new(P.FacingRightSonicBoomsLeftFootForward1, 0, 22, 4), new(P.FacingRightSonicBoomsLeftFootForward1, 22, 2, 1), new(P.FacingRightSonicBoomsRightFootForward0, 0, 1, 1), new(P.FacingRightSonicBoomsRightFootForward1, 0, 22, 4), new(P.FacingRightSonicBoomsRightFootForward1, 22, 2, 1), new(P.FacingRightExplosiveSwipeLeftFootForward, 0, 33, 1), new(P.FacingRightExplosiveSwipeRightFootForward, 0, 33, 1), new(P.FacingRightJumpingForwards0, 0, 4, 1), new(P.FacingRightJumpingForwards1, 0, 2, 1), new(P.FacingRightFalling0, 0, 2, 1), new(P.FacingRightFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(11)),
        [P.FacingRightSonicBoomsRightFootForward0] = new([new(P.FacingRightSonicBoomsRightFootForward0, 0, 1, 1), new(P.FacingRightSonicBoomsRightFootForward1, 0, 22, 4), new(P.FacingRightSonicBoomsRightFootForward1, 22, 2, 1), new(P.FacingRightExplosiveSwipeLeftFootForward, 0, 33, 1), new(P.FacingRightExplosiveSwipeRightFootForward, 0, 33, 1), new(P.FacingRightJumpingForwards0, 0, 4, 1), new(P.FacingRightJumpingForwards1, 0, 2, 1), new(P.FacingRightFalling0, 0, 2, 1), new(P.FacingRightFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(8)),
        [P.FacingRightExplosiveSwipeLeftFootForward] = new([new(P.FacingRightExplosiveSwipeLeftFootForward, 0, 33, 1), new(P.FacingRightExplosiveSwipeRightFootForward, 0, 33, 1), new(P.FacingRightJumpingForwards0, 0, 4, 1), new(P.FacingRightJumpingForwards1, 0, 2, 1), new(P.FacingRightFalling0, 0, 2, 1), new(P.FacingRightFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(5)),
        [P.FacingRightExplosiveSwipeRightFootForward] = new([new(P.FacingRightExplosiveSwipeRightFootForward, 0, 33, 1), new(P.FacingRightJumpingForwards0, 0, 4, 1), new(P.FacingRightJumpingForwards1, 0, 2, 1), new(P.FacingRightFalling0, 0, 2, 1), new(P.FacingRightFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(4)),
        [P.FacingRightJumpingForwards0] = new([new(P.FacingRightJumpingForwards0, 0, 4, 1), new(P.FacingRightJumpingForwards1, 0, 2, 1), new(P.FacingRightFalling0, 0, 2, 1), new(P.FacingRightFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(3)),
        [P.FacingRightJumpingForwards1] = new([new(P.FacingRightJumpingForwards1, 0, 2, 1), new(P.FacingRightFalling0, 0, 2, 1), new(P.FacingRightFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(2)),
        [P.FacingRightFalling0] = new([new(P.FacingRightFalling0, 0, 2, 1), new(P.FacingRightFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingRightFalling1] = new([new(P.FacingRightFalling1, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightFalling2] = new([new(P.FacingRightFalling2, 0, 3, 1), new(P.FacingRightWalkingRightLegMoving, 0, 20, 1), new(P.FacingRightWalkingLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingRightJumpBackwardLandRightFootFwd0] = new([new(P.FacingRightJumpBackwardLandRightFootFwd0, 0, 4, 1), new(P.FacingRightJumpBackwardLandRightFootFwd1, 0, 2, 1), new(P.FacingRightJumpBackwardLandRightFootFwd2, 0, 2, 1), new(P.FacingRightJumpBackwardLandRightFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(3)),
        [P.FacingRightJumpBackwardLandRightFootFwd1] = new([new(P.FacingRightJumpBackwardLandRightFootFwd1, 0, 2, 1), new(P.FacingRightJumpBackwardLandRightFootFwd2, 0, 2, 1), new(P.FacingRightJumpBackwardLandRightFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(2)),
        [P.FacingRightJumpBackwardLandRightFootFwd2] = new([new(P.FacingRightJumpBackwardLandRightFootFwd2, 0, 2, 1), new(P.FacingRightJumpBackwardLandRightFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingRightJumpBackwardLandRightFootFwd3] = new([new(P.FacingRightJumpBackwardLandRightFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightJumpBackwardLandRightFootFwd4] = new([new(P.FacingRightJumpBackwardLandRightFootFwd4, 0, 4, 1), new(P.FacingRightWalkingLeftLegMoving, 0, 21, 1), new(P.FacingRightWalkingRightLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingRightJumpBackwardsLandLeftFootFwd0] = new([new(P.FacingRightJumpBackwardsLandLeftFootFwd0, 0, 4, 1), new(P.FacingRightJumpBackwardsLandLeftFootFwd1, 0, 2, 1), new(P.FacingRightJumpBackwardsLandLeftFootFwd2, 0, 2, 1), new(P.FacingRightJumpBackwardsLandLeftFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(3)),
        [P.FacingRightJumpBackwardsLandLeftFootFwd1] = new([new(P.FacingRightJumpBackwardsLandLeftFootFwd1, 0, 2, 1), new(P.FacingRightJumpBackwardsLandLeftFootFwd2, 0, 2, 1), new(P.FacingRightJumpBackwardsLandLeftFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(2)),
        [P.FacingRightJumpBackwardsLandLeftFootFwd2] = new([new(P.FacingRightJumpBackwardsLandLeftFootFwd2, 0, 2, 1), new(P.FacingRightJumpBackwardsLandLeftFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingRightJumpBackwardsLandLeftFootFwd3] = new([new(P.FacingRightJumpBackwardsLandLeftFootFwd3, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightJumpBackwardsLandLeftFootFwd4] = new([new(P.FacingRightJumpBackwardsLandLeftFootFwd4, 0, 4, 1), new(P.FacingRightWalkingRightLegMoving, 0, 20, 1), new(P.FacingRightWalkingLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingRightFacelessTurningRight] = new([new(P.FacingRightFacelessTurningRight, 0, 3, 1), new(P.FacingRightFacelessWalkingLeftLegMoving, 0, 16, 1), new(P.FacingRightFacelessWalkingRightLegMoving, 0, 16, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FacingRightFacelessWalkingLeftLegMoving] = new([new(P.FacingRightFacelessWalkingLeftLegMoving, 0, 16, 1), new(P.FacingRightFacelessWalkingRightLegMoving, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightFacelessWalkingRightLegMoving] = new([new(P.FacingRightFacelessWalkingRightLegMoving, 0, 16, 1), new(P.FacingRightFacelessWalkingLeftLegMoving, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GoldenInitial0] = new([new(P.GoldenInitial0, 0, 5, 1)], AnimationEnd.Holds),
        [P.GoldenInitial0Resumed] = new([new(P.GoldenInitial0Resumed, 0, 1, 1), new(P.GoldenInitial1, 0, 19, 1), new(P.GoldenInitial2, 0, 8, 2), new(P.GoldenInitial2, 8, 13, 1), new(P.GoldenInitial3, 0, 2, 16), new(P.GoldenInitial3, 2, 4, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(6)),
        [P.GoldenInitial1] = new([new(P.GoldenInitial1, 0, 19, 1), new(P.GoldenInitial2, 0, 8, 2), new(P.GoldenInitial2, 8, 13, 1), new(P.GoldenInitial3, 0, 2, 16), new(P.GoldenInitial3, 2, 4, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(5)),
        [P.GoldenSpewChozoOrbsFaceLeftRightFootFwd0] = new([new(P.GoldenSpewChozoOrbsFaceLeftRightFootFwd0, 0, 7, 1), new(P.GoldenSpewChozoOrbsFaceLeftRightFootFwd1, 0, 3, 6), new(P.GoldenSpewChozoOrbsFaceLeftRightFootFwd1, 3, 6, 1), new(P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd0, 0, 7, 1), new(P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd1, 0, 3, 6), new(P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd1, 3, 6, 1), new(P.GoldenSonicBoomsFacingLeftRightFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingLeftRightFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingLeftRightFootFwd1, 22, 2, 1), new(P.GoldenSonicBoomsFacingLeftLeftFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingLeftLeftFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingLeftLeftFootFwd1, 22, 2, 1), new(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd0, 0, 7, 1), new(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1, 0, 3, 6), new(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1, 3, 6, 1), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd0, 0, 7, 1), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd1, 0, 3, 6), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd1, 3, 6, 1), new(P.GoldenSonicBoomsFacingRightLeftFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingRightLeftFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingRightLeftFootFwd1, 22, 2, 1), new(P.GoldenSonicBoomsFacingRightRightFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 22, 2, 1), new(P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd, 0, 1, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(25)),
        [P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd0] = new([new(P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd0, 0, 7, 1), new(P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd1, 0, 3, 6), new(P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd1, 3, 6, 1), new(P.GoldenSonicBoomsFacingLeftRightFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingLeftRightFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingLeftRightFootFwd1, 22, 2, 1), new(P.GoldenSonicBoomsFacingLeftLeftFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingLeftLeftFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingLeftLeftFootFwd1, 22, 2, 1), new(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd0, 0, 7, 1), new(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1, 0, 3, 6), new(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1, 3, 6, 1), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd0, 0, 7, 1), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd1, 0, 3, 6), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd1, 3, 6, 1), new(P.GoldenSonicBoomsFacingRightLeftFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingRightLeftFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingRightLeftFootFwd1, 22, 2, 1), new(P.GoldenSonicBoomsFacingRightRightFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 22, 2, 1), new(P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd, 0, 1, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(22)),
        [P.GoldenSonicBoomsFacingLeftRightFootFwd0] = new([new(P.GoldenSonicBoomsFacingLeftRightFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingLeftRightFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingLeftRightFootFwd1, 22, 2, 1), new(P.GoldenSonicBoomsFacingLeftLeftFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingLeftLeftFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingLeftLeftFootFwd1, 22, 2, 1), new(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd0, 0, 7, 1), new(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1, 0, 3, 6), new(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1, 3, 6, 1), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd0, 0, 7, 1), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd1, 0, 3, 6), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd1, 3, 6, 1), new(P.GoldenSonicBoomsFacingRightLeftFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingRightLeftFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingRightLeftFootFwd1, 22, 2, 1), new(P.GoldenSonicBoomsFacingRightRightFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 22, 2, 1), new(P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd, 0, 1, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(19)),
        [P.GoldenSonicBoomsFacingLeftLeftFootFwd0] = new([new(P.GoldenSonicBoomsFacingLeftLeftFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingLeftLeftFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingLeftLeftFootFwd1, 22, 2, 1), new(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd0, 0, 7, 1), new(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1, 0, 3, 6), new(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1, 3, 6, 1), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd0, 0, 7, 1), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd1, 0, 3, 6), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd1, 3, 6, 1), new(P.GoldenSonicBoomsFacingRightLeftFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingRightLeftFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingRightLeftFootFwd1, 22, 2, 1), new(P.GoldenSonicBoomsFacingRightRightFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 22, 2, 1), new(P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd, 0, 1, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(16)),
        [P.GoldenSpewChozoOrbFacingLeftLeftFootFwd0] = new([new(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd0, 0, 7, 1), new(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1, 0, 3, 6), new(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1, 3, 6, 1), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd0, 0, 7, 1), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd1, 0, 3, 6), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd1, 3, 6, 1), new(P.GoldenSonicBoomsFacingRightLeftFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingRightLeftFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingRightLeftFootFwd1, 22, 2, 1), new(P.GoldenSonicBoomsFacingRightRightFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 22, 2, 1), new(P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd, 0, 1, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(13)),
        [P.GoldenSpewChozoOrbFacingLeftRightFootFwd0] = new([new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd0, 0, 7, 1), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd1, 0, 3, 6), new(P.GoldenSpewChozoOrbFacingLeftRightFootFwd1, 3, 6, 1), new(P.GoldenSonicBoomsFacingRightLeftFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingRightLeftFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingRightLeftFootFwd1, 22, 2, 1), new(P.GoldenSonicBoomsFacingRightRightFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 22, 2, 1), new(P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd, 0, 1, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(10)),
        [P.GoldenSonicBoomsFacingRightLeftFootFwd0] = new([new(P.GoldenSonicBoomsFacingRightLeftFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingRightLeftFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingRightLeftFootFwd1, 22, 2, 1), new(P.GoldenSonicBoomsFacingRightRightFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 22, 2, 1), new(P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd, 0, 1, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(7)),
        [P.GoldenSonicBoomsFacingRightRightFootFwd0] = new([new(P.GoldenSonicBoomsFacingRightRightFootFwd0, 0, 1, 1), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 0, 22, 4), new(P.GoldenSonicBoomsFacingRightRightFootFwd1, 22, 2, 1), new(P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd, 0, 1, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(4)),
        [P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd] = new([new(P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd, 0, 1, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GoldenLandedFromBackwardsJumpFacingLeftRightFootFwd] = new([new(P.GoldenLandedFromBackwardsJumpFacingLeftRightFootFwd, 0, 1, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GoldenLandedFromBackwardsJumpFacingRightRightFootFwd] = new([new(P.GoldenLandedFromBackwardsJumpFacingRightRightFootFwd, 0, 1, 1), new(P.GoldenWalkingRightLeftLegMoving, 0, 20, 1), new(P.GoldenWalkingRightRightLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GoldenLandedFromBackwardsJumpFacingRightLeftFootFwd] = new([new(P.GoldenLandedFromBackwardsJumpFacingRightLeftFootFwd, 0, 1, 1), new(P.GoldenWalkingRightRightLegMoving, 0, 21, 1), new(P.GoldenWalkingRightLeftLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GoldenCaughtSuperFacingLeftLeftLegFwd] = new([new(P.GoldenCaughtSuperFacingLeftLeftLegFwd, 0, 25, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GoldenCaughtSuperFacingLeftRightLegFwd] = new([new(P.GoldenCaughtSuperFacingLeftRightLegFwd, 0, 25, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GoldenCaughtSuperFacingRightRightLegFwd] = new([new(P.GoldenCaughtSuperFacingRightRightLegFwd, 0, 23, 1), new(P.GoldenWalkingRightLeftLegMoving, 0, 20, 1), new(P.GoldenWalkingRightRightLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GoldenCaughtSuperFacingRightLeftLegFwd] = new([new(P.GoldenCaughtSuperFacingRightLeftLegFwd, 0, 23, 1), new(P.GoldenWalkingRightRightLegMoving, 0, 21, 1), new(P.GoldenWalkingRightLeftLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GoldenSitDownAttackFacingLeft] = new([new(P.GoldenSitDownAttackFacingLeft, 0, 26, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GoldenSitDownAttackFacingRight] = new([new(P.GoldenSitDownAttackFacingRight, 0, 26, 1), new(P.GoldenWalkingRightRightLegMoving, 0, 21, 1), new(P.GoldenWalkingRightLeftLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GoldenReleaseGoldenTorizoEggs0] = new([new(P.GoldenReleaseGoldenTorizoEggs0, 0, 10, 1), new(P.GoldenReleaseGoldenTorizoEggs1, 0, 3, 6), new(P.GoldenReleaseGoldenTorizoEggs2, 0, 15, 1)], AnimationEnd.Holds),
        [P.GoldenReleaseGoldenTorizoEggs2] = new([new(P.GoldenReleaseGoldenTorizoEggs2, 0, 15, 1)], AnimationEnd.Holds),
        [P.GoldenEyeBeamAttack0] = new([new(P.GoldenEyeBeamAttack0, 0, 4, 1), new(P.GoldenEyeBeamAttack0, 4, 1, 1), new(P.GoldenEyeBeamAttack1, 0, 2, 4), new(P.GoldenEyeBeamAttack1, 2, 1, 1), new(P.GoldenEyeBeamAttack2, 0, 8, 2), new(P.GoldenEyeBeamAttack2, 8, 6, 1)], AnimationEnd.Holds),
        [P.GoldenDodgeTurningLeft] = new([new(P.GoldenDodgeTurningLeft, 0, 5, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GoldenTurningLeft] = new([new(P.GoldenTurningLeft, 0, 3, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GoldenWalkingLeftRightLegMoving] = new([new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1), new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GoldenWalkingLeftLeftLegMoving] = new([new(P.GoldenWalkingLeftLeftLegMoving, 0, 21, 1), new(P.GoldenWalkingLeftRightLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GoldenDodgeTurningRight] = new([new(P.GoldenDodgeTurningRight, 0, 5, 1), new(P.GoldenWalkingRightLeftLegMoving, 0, 20, 1), new(P.GoldenWalkingRightRightLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GoldenTurningRight] = new([new(P.GoldenTurningRight, 0, 3, 1), new(P.GoldenWalkingRightLeftLegMoving, 0, 20, 1), new(P.GoldenWalkingRightRightLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GoldenWalkingRightLeftLegMoving] = new([new(P.GoldenWalkingRightLeftLegMoving, 0, 20, 1), new(P.GoldenWalkingRightRightLegMoving, 0, 21, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GoldenWalkingRightRightLegMoving] = new([new(P.GoldenWalkingRightRightLegMoving, 0, 21, 1), new(P.GoldenWalkingRightLeftLegMoving, 0, 20, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<TorizoBodySegment> StartingWith(TorizoBodySegment segment) => StartingAt[segment];

    /// <summary>A playback of the animation that starts at <paramref name="segment"/>, before its first play.</summary>
    internal static AnimationPlayback<TorizoBodySegment> Start(TorizoBodySegment segment) => new(StartingWith(segment), AnimationPlace.StartOfAnimation);

    /// <summary>A cue's continuation into the animation that starts at <paramref name="segment"/>, as a goto continues there.</summary>
    internal static AnimationContinuation<TorizoBodySegment> ContinueWith(TorizoBodySegment segment) => new(StartingWith(segment), AnimationPlace.StartOfAnimation);

    /// <summary>A cue's continuation where <paramref name="playback"/> stood, as a return to a saved place continues there.</summary>
    internal static AnimationContinuation<TorizoBodySegment> ContinueAt(AnimationPlayback<TorizoBodySegment> playback) =>
        new(Ensure.NotNull(playback.Animation), playback.Place);

    /// <summary>
    /// The segment <paramref name="playback"/> stands in: before its first play, the segment its animation starts at;
    /// otherwise the segment its playing phase plays, the one its next play shows a frame of, or rests in.
    /// </summary>
    internal static TorizoBodySegment SegmentOf(AnimationPlayback<TorizoBodySegment> playback) =>
        playback.Place == AnimationPlace.StartOfAnimation
            ? StartingAt.Single(pair => ReferenceEquals(pair.Value, playback.Animation)).Key
            : playback.Animation.Phases[playback.Place.Phase].Segment;

    /// <summary>
    /// The segment a sleeping animation resumes into when the AI wakes it, as Bomb Torizo's wake
    /// and Golden Torizo's trigger do.
    /// </summary>
    internal static TorizoBodySegment AfterSleep(TorizoBodySegment segment) => segment switch
    {
        P.BombTorizoInitial0
            or P.FacingLeftFacelessWalkingLeftLegMovingTrailing
            or P.GoldenInitial0 => EnumOrder.Next(segment),
        P.SpecialCallableBlowUpBombTorizosGut or
            P.CallableBlowUpBombTorizosFace or
            P.DeathSequence0 or
            P.DeathSequence1 or
            P.DeathSequence2 or
            P.BombTorizoInitial0Resumed or
            P.BombTorizoInitial1 or
            P.BombTorizoInitial2 or
            P.FacingLeftTurningLeft or
            P.FacingLeftWalkingRightLegMoving or
            P.FacingLeftWalkingLeftLegMoving or
            P.FacingLeftSpewingChozoOrbsRightFootFwd0 or
            P.FacingLeftSpewingChozoOrbsRightFootFwd1 or
            P.FacingLeftSpewingChozoOrbsLeftFootFwd0 or
            P.FacingLeftSpewingChozoOrbsLeftFootFwd1 or
            P.FacingLeftSonicBoomsRightFootForward0 or
            P.FacingLeftSonicBoomsRightFootForward1 or
            P.FacingLeftSonicBoomsLeftFootForward0 or
            P.FacingLeftSonicBoomsLeftFootForward1 or
            P.FacingLeftExplosiveSwipeRightFootForward or
            P.FacingLeftExplosiveSwipeLeftFootForward or
            P.FacingLeftJumpingForwards0 or
            P.FacingLeftJumpingForwards1 or
            P.FacingLeftFalling0 or
            P.FacingLeftFalling1 or
            P.FacingLeftFalling2 or
            P.FacingLeftJumpingBackwardLandLeftFootFwd0 or
            P.FacingLeftJumpingBackwardLandLeftFootFwd1 or
            P.FacingLeftJumpingBackwardLandLeftFootFwd2 or
            P.FacingLeftJumpingBackwardLandLeftFootFwd3 or
            P.FacingLeftJumpingBackwardLandLeftFootFwd4 or
            P.FacingLeftJumpingBackwardRightFootFwd0 or
            P.FacingLeftJumpingBackwardRightFootFwd1 or
            P.FacingLeftJumpingBackwardRightFootFwd2 or
            P.FacingLeftJumpingBackwardRightFootFwd3 or
            P.FacingLeftJumpingBackwardRightFootFwd4 or
            P.FacingLeftFacelessTurningLeft or
            P.FacingLeftFacelessWalkingRightLegMoving or
            P.FacingLeftFacelessWalkingLeftLegMoving or
            P.FacingLeftFacelessWalkingLeftLegMovingTrailingResumed or
            P.FacingRightTurningRight or
            P.FacingRightWalkingLeftLegMoving or
            P.FacingRightWalkingRightLegMoving or
            P.FacingRightSpewingChozoOrbsLeftFootFwd0 or
            P.FacingRightSpewingChozoOrbsLeftFootFwd1 or
            P.FacingRightSpewingChozoOrbsRightFootFwd0 or
            P.FacingRightSpewingChozoOrbsRightFootFwd1 or
            P.FacingRightSonicBoomsLeftFootForward0 or
            P.FacingRightSonicBoomsLeftFootForward1 or
            P.FacingRightSonicBoomsRightFootForward0 or
            P.FacingRightSonicBoomsRightFootForward1 or
            P.FacingRightExplosiveSwipeLeftFootForward or
            P.FacingRightExplosiveSwipeRightFootForward or
            P.FacingRightJumpingForwards0 or
            P.FacingRightJumpingForwards1 or
            P.FacingRightFalling0 or
            P.FacingRightFalling1 or
            P.FacingRightFalling2 or
            P.FacingRightJumpBackwardLandRightFootFwd0 or
            P.FacingRightJumpBackwardLandRightFootFwd1 or
            P.FacingRightJumpBackwardLandRightFootFwd2 or
            P.FacingRightJumpBackwardLandRightFootFwd3 or
            P.FacingRightJumpBackwardLandRightFootFwd4 or
            P.FacingRightJumpBackwardsLandLeftFootFwd0 or
            P.FacingRightJumpBackwardsLandLeftFootFwd1 or
            P.FacingRightJumpBackwardsLandLeftFootFwd2 or
            P.FacingRightJumpBackwardsLandLeftFootFwd3 or
            P.FacingRightJumpBackwardsLandLeftFootFwd4 or
            P.FacingRightFacelessTurningRight or
            P.FacingRightFacelessWalkingLeftLegMoving or
            P.FacingRightFacelessWalkingRightLegMoving or
            P.GoldenInitial0Resumed or
            P.GoldenInitial1 or
            P.GoldenInitial2 or
            P.GoldenInitial3 or
            P.GoldenSpewChozoOrbsFaceLeftRightFootFwd0 or
            P.GoldenSpewChozoOrbsFaceLeftRightFootFwd1 or
            P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd0 or
            P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd1 or
            P.GoldenSonicBoomsFacingLeftRightFootFwd0 or
            P.GoldenSonicBoomsFacingLeftRightFootFwd1 or
            P.GoldenSonicBoomsFacingLeftLeftFootFwd0 or
            P.GoldenSonicBoomsFacingLeftLeftFootFwd1 or
            P.GoldenSpewChozoOrbFacingLeftLeftFootFwd0 or
            P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1 or
            P.GoldenSpewChozoOrbFacingLeftRightFootFwd0 or
            P.GoldenSpewChozoOrbFacingLeftRightFootFwd1 or
            P.GoldenSonicBoomsFacingRightLeftFootFwd0 or
            P.GoldenSonicBoomsFacingRightLeftFootFwd1 or
            P.GoldenSonicBoomsFacingRightRightFootFwd0 or
            P.GoldenSonicBoomsFacingRightRightFootFwd1 or
            P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd or
            P.GoldenLandedFromBackwardsJumpFacingLeftRightFootFwd or
            P.GoldenLandedFromBackwardsJumpFacingRightRightFootFwd or
            P.GoldenLandedFromBackwardsJumpFacingRightLeftFootFwd or
            P.GoldenCaughtSuperFacingLeftLeftLegFwd or
            P.GoldenCaughtSuperFacingLeftRightLegFwd or
            P.GoldenCaughtSuperFacingRightRightLegFwd or
            P.GoldenCaughtSuperFacingRightLeftLegFwd or
            P.GoldenSitDownAttackFacingLeft or
            P.GoldenSitDownAttackFacingRight or
            P.GoldenReleaseGoldenTorizoEggs0 or
            P.GoldenReleaseGoldenTorizoEggs1 or
            P.GoldenReleaseGoldenTorizoEggs2 or
            P.GoldenEyeBeamAttack0 or
            P.GoldenEyeBeamAttack1 or
            P.GoldenEyeBeamAttack2 or
            P.Stunned0 or
            P.Stunned1 or
            P.GoldenDodgeTurningLeft or
            P.GoldenTurningLeft or
            P.GoldenWalkingLeftRightLegMoving or
            P.GoldenWalkingLeftLeftLegMoving or
            P.GoldenDodgeTurningRight or
            P.GoldenTurningRight or
            P.GoldenWalkingRightLeftLegMoving or
            P.GoldenWalkingRightRightLegMoving => throw new ArgumentOutOfRangeException(nameof(segment), segment, "The program does not sleep before authored code."),
        _ => throw new ArgumentOutOfRangeException(nameof(segment), segment, "The program does not sleep before authored code."),
    };
}
