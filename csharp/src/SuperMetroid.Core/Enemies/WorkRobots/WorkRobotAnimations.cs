using K = SuperMetroid.Core.Enemies.WorkRobots.WorkRobotCueKind;
using P = SuperMetroid.Core.Enemies.WorkRobots.WorkRobotProgram;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The Work Robot's unpowered poses and its walk, wall, recoil, laser and ledge animations as typed programs (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="WorkRobotFrame"/> pose and each callback becomes a <see cref="WorkRobotCue"/> with its typed operands.
/// </summary>
internal static class WorkRobotAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<WorkRobotProgram, WorkRobotFrame, WorkRobotCue> Programs { get; } = new(
        new Dictionary<WorkRobotProgram, AnimationProgramStep<WorkRobotProgram, WorkRobotFrame, WorkRobotCue>[]>
    {
        // $A8:C6D3
        [P.NoPowerNeutral] =
        [
            Show(25, 32767),
            Hold,
        ],
        // $A8:C6D9
        [P.NoPowerLeaningLeft] =
        [
            Show(24, 32767),
            Hold,
        ],
        // $A8:C6DF
        [P.NoPowerLeaningRight] =
        [
            Show(26, 32767),
            Hold,
        ],
        // $A8:C6E5
        [P.Initial] =
        [
            Show(1, 32),
            FallInto(P.FacingLeftWalkingForwards),
        ],
        // $A8:C6E9
        [P.FacingLeftWalkingForwards] =
        [
            Show(1, 1),
            FallInto(P.FacingLeftWalkingForwardsLoop),
        ],
        // $A8:C6ED
        [P.FacingLeftWalkingForwardsLoop] =
        [
            Show(1, 10),
            Show(2, 1),
            Cue(new(K.TryLaserLeft)),
            Show(2, 9),
            Show(3, 1),
            Cue(new(K.TryLaserUpLeft)),
            Show(3, 9),
            Show(4, 10),
            Cue(new(K.Footstep)),
            Cue(new(K.LeftForwardWatchingLedges)),
            Show(5, 10),
            Cue(new(K.LeftForwardWatchingLedges)),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(10, 10),
            Cue(new(K.LeftForwardWatchingLedges)),
            Cue(new(K.Footstep)),
            Show(11, 10),
            Cue(new(K.LeftForwardWatchingLedges)),
            Show(0, 1),
            Cue(new(K.TryLaserDownLeft)),
            Show(0, 9),
            GoTo(P.FacingLeftWalkingForwardsLoop),
        ],
        // $A8:C73F
        [P.FacingLeftHitWallMovingForwards] =
        [
            Show(11, 1),
            Show(11, 10),
            Cue(new(K.Footstep)),
            Cue(new(K.LeftBackwardWatchingLedges)),
            Show(10, 10),
            Show(9, 10),
            Show(8, 10),
            Show(7, 10),
            Show(6, 10),
            Cue(new(K.Footstep)),
            Cue(new(K.LeftBackwardWatchingLedges)),
            Show(5, 10),
            Cue(new(K.LeftBackwardWatchingLedges)),
            Show(4, 10),
            Show(3, 10),
            Show(2, 10),
            Show(1, 10),
            Show(0, 10),
            Cue(new(K.LeftBackwardWatchingLedges)),
            Cue(new(K.Footstep)),
            Show(11, 10),
            Cue(new(K.LeftBackwardWatchingLedges)),
            Show(10, 10),
            Show(9, 10),
            Show(8, 10),
            Show(7, 10),
            Show(6, 10),
            Cue(new(K.Footstep)),
            Cue(new(K.LeftBackwardWatchingLedges)),
            Show(5, 10),
            Cue(new(K.LeftBackwardWatchingLedges)),
            Show(4, 10),
            Show(3, 10),
            Show(2, 10),
            Show(1, 10),
            Show(0, 10),
            Cue(new(K.WalkRight)),
            FallInto(P.FacingLeftShotSamusAhead),
        ],
        // $A8:C7BB
        [P.FacingLeftShotSamusAhead] =
        [
            Show(11, 5),
            Cue(new(K.Footstep)),
            Cue(new(K.LeftBackward)),
            Show(10, 5),
            Show(9, 5),
            Show(8, 5),
            Show(7, 5),
            Show(6, 5),
            Cue(new(K.Footstep)),
            Cue(new(K.LeftBackward)),
            Show(5, 5),
            Cue(new(K.LeftBackward)),
            Show(4, 5),
            Show(3, 5),
            Show(2, 5),
            Show(1, 5),
            Show(0, 5),
            Cue(new(K.LeftBackward)),
            Cue(new(K.Footstep)),
            Show(11, 5),
            Cue(new(K.LeftBackward)),
            Show(10, 5),
            Show(9, 5),
            Show(8, 5),
            Show(7, 5),
            Show(6, 5),
            Cue(new(K.Footstep)),
            Cue(new(K.LeftBackward)),
            Show(5, 5),
            Cue(new(K.LeftBackward)),
            Show(4, 5),
            Show(3, 5),
            Show(2, 5),
            Show(1, 5),
            Show(0, 5),
            Cue(new(K.WalkRight)),
            FallInto(P.FacingLeftShotSamusBehind),
        ],
        // $A8:C833
        [P.FacingLeftShotSamusBehind] =
        [
            Cue(new(K.Footstep)),
            Cue(new(K.LeftForward)),
            Show(1, 5),
            Show(2, 5),
            Show(3, 5),
            Show(4, 5),
            Cue(new(K.Footstep)),
            Cue(new(K.LeftForward)),
            Show(5, 5),
            Cue(new(K.LeftForward)),
            Show(6, 5),
            Show(7, 5),
            Show(8, 5),
            Show(9, 5),
            Show(10, 5),
            Cue(new(K.LeftForward)),
            Cue(new(K.Footstep)),
            Show(11, 5),
            Cue(new(K.LeftForward)),
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(3, 5),
            Show(4, 5),
            Cue(new(K.Footstep)),
            Cue(new(K.LeftForward)),
            Show(5, 5),
            Cue(new(K.LeftForward)),
            Show(6, 5),
            Show(7, 5),
            Show(8, 5),
            Show(9, 5),
            Show(10, 5),
            Cue(new(K.LeftForward)),
            Cue(new(K.Footstep)),
            Show(11, 5),
            Cue(new(K.LeftForward)),
            Show(0, 5),
            Cue(new(K.WalkLeft)),
            FallInto(P.FacingLeftShotLaserDownLeft),
        ],
        // $A8:C8B1
        [P.FacingLeftShotLaserDownLeft] =
        [
            Show(0, 5),
            Show(11, 2),
            GoTo(P.FacingLeftLaserShotRecoil),
        ],
        // $A8:C8BD
        [P.FacingLeftShotLaserLeft] =
        [
            Show(1, 5),
            Show(0, 2),
            Cue(new(K.LeftBackwardWatchingLedges)),
            Cue(new(K.Footstep)),
            Show(11, 10),
            GoTo(P.FacingLeftLaserShotRecoil),
        ],
        // $A8:C8D1
        [P.FacingLeftShotLaserUpLeft] =
        [
            Show(3, 5),
            Show(2, 2),
            Show(1, 2),
            Show(0, 4),
            Cue(new(K.LeftBackwardWatchingLedges)),
            Cue(new(K.Footstep)),
            Show(11, 4),
            FallInto(P.FacingLeftLaserShotRecoil),
        ],
        // $A8:C8E9
        [P.FacingLeftLaserShotRecoil] =
        [
            Cue(new(K.LeftBackwardWatchingLedges)),
            Show(10, 16),
            Show(9, 5),
            Show(8, 5),
            Show(7, 5),
            Show(6, 5),
            Cue(new(K.Footstep)),
            Cue(new(K.LeftBackwardWatchingLedges)),
            Show(5, 10),
            Cue(new(K.LeftBackwardWatchingLedges)),
            Show(4, 10),
            Show(3, 10),
            Show(2, 96),
            Cue(new(K.CountDownLaserCooldown)),
            GoTo(P.FacingLeftWalkingForwards),
        ],
        // $A8:C91B
        [P.ApproachingFallRight] =
        [
            Show(12, 128),
            Show(13, 10),
            Show(14, 10),
            Cue(new(K.CountDownLaserCooldown)),
            GoTo(P.FacingLeftWalkingForwards),
        ],
        // $A8:C92D
        [P.FacingRightWalkingForwards] =
        [
            Show(12, 1),
            FallInto(P.FacingRightWalkingForwardsLoop),
        ],
        // $A8:C931
        [P.FacingRightWalkingForwardsLoop] =
        [
            Show(12, 1),
            Cue(new(K.TryLaserDownRight)),
            Show(12, 9),
            Cue(new(K.CountDownLaserCooldown)),
            Show(13, 10),
            Show(14, 1),
            Cue(new(K.TryLaserRight)),
            Show(14, 9),
            Show(15, 1),
            Cue(new(K.TryLaserUpRight)),
            Show(15, 9),
            Show(16, 10),
            Cue(new(K.Footstep)),
            Cue(new(K.RightForwardWatchingLedges)),
            Show(17, 10),
            Cue(new(K.RightForwardWatchingLedges)),
            Show(18, 10),
            Show(19, 10),
            Show(20, 10),
            Show(21, 10),
            Show(22, 10),
            Cue(new(K.RightForwardWatchingLedges)),
            Cue(new(K.Footstep)),
            Show(23, 10),
            Cue(new(K.RightForwardWatchingLedges)),
            GoTo(P.FacingRightWalkingForwardsLoop),
        ],
        // $A8:C985
        [P.FacingRightHitWallMovingForwards] =
        [
            Show(23, 1),
            Show(23, 10),
            Cue(new(K.Footstep)),
            Cue(new(K.RightBackwardWatchingLedges)),
            Show(22, 10),
            Show(21, 10),
            Show(20, 10),
            Show(19, 10),
            Show(18, 10),
            Cue(new(K.Footstep)),
            Cue(new(K.RightBackwardWatchingLedges)),
            Show(17, 10),
            Cue(new(K.RightBackwardWatchingLedges)),
            Show(16, 10),
            Show(15, 10),
            Show(14, 10),
            Show(13, 10),
            Show(12, 10),
            Cue(new(K.Footstep)),
            Cue(new(K.RightBackwardWatchingLedges)),
            Show(23, 10),
            Cue(new(K.RightBackwardWatchingLedges)),
            Show(22, 10),
            Show(21, 10),
            Show(20, 10),
            Show(19, 10),
            Show(18, 10),
            Cue(new(K.Footstep)),
            Cue(new(K.RightBackwardWatchingLedges)),
            Show(17, 10),
            Cue(new(K.RightBackwardWatchingLedges)),
            Show(16, 10),
            Show(15, 10),
            Show(14, 10),
            Show(13, 10),
            Show(12, 10),
            Cue(new(K.WalkLeft)),
            FallInto(P.FacingRightShotSamusAhead),
        ],
        // $A8:CA01
        [P.FacingRightShotSamusAhead] =
        [
            Show(23, 1),
            Show(23, 5),
            Cue(new(K.Footstep)),
            Cue(new(K.RightBackward)),
            Show(22, 5),
            Show(21, 5),
            Show(20, 5),
            Show(19, 5),
            Show(18, 5),
            Cue(new(K.Footstep)),
            Cue(new(K.RightBackward)),
            Show(17, 5),
            Cue(new(K.RightBackward)),
            Show(16, 5),
            Show(15, 5),
            Show(14, 5),
            Show(13, 5),
            Show(12, 5),
            Cue(new(K.RightBackward)),
            Cue(new(K.Footstep)),
            Show(23, 5),
            Cue(new(K.RightBackward)),
            Show(22, 5),
            Show(21, 5),
            Show(20, 5),
            Show(19, 5),
            Show(18, 5),
            Cue(new(K.Footstep)),
            Cue(new(K.RightBackward)),
            Show(17, 10),
            Cue(new(K.RightBackward)),
            Show(16, 5),
            Show(15, 5),
            Show(14, 5),
            Show(13, 5),
            Show(12, 5),
            Cue(new(K.WalkLeft)),
            FallInto(P.FacingRightShotSamusBehind),
        ],
        // $A8:CA7D
        [P.FacingRightShotSamusBehind] =
        [
            Cue(new(K.RightForward)),
            Cue(new(K.Footstep)),
            Show(12, 5),
            Show(13, 5),
            Show(14, 5),
            Show(15, 5),
            Show(16, 5),
            Cue(new(K.Footstep)),
            Cue(new(K.RightForward)),
            Show(17, 5),
            Cue(new(K.RightForward)),
            Cue(new(K.Footstep)),
            Show(18, 5),
            Show(19, 5),
            Show(20, 5),
            Show(21, 5),
            Show(22, 5),
            Cue(new(K.RightForward)),
            Cue(new(K.Footstep)),
            Show(23, 5),
            Cue(new(K.RightForward)),
            Show(12, 5),
            Show(13, 5),
            Show(14, 5),
            Show(15, 5),
            Show(16, 5),
            Cue(new(K.Footstep)),
            Cue(new(K.RightForward)),
            Show(17, 5),
            Cue(new(K.RightForward)),
            Show(18, 5),
            Show(19, 5),
            Show(20, 5),
            Show(21, 5),
            Show(22, 5),
            Cue(new(K.RightForward)),
            Cue(new(K.Footstep)),
            Show(23, 5),
            Cue(new(K.RightForward)),
            Cue(new(K.WalkRight)),
            FallInto(P.FacingRightShotLaserDownRight),
        ],
        // $A8:CAFD
        [P.FacingRightShotLaserDownRight] =
        [
            Show(12, 5),
            Show(23, 2),
            GoTo(P.FacingRightLaserShotRecoil),
        ],
        // $A8:CB09
        [P.FacingRightShotLaserRight] =
        [
            Show(13, 5),
            Show(12, 2),
            Cue(new(K.Footstep)),
            Cue(new(K.RightBackwardWatchingLedges)),
            Show(23, 10),
            GoTo(P.FacingRightLaserShotRecoil),
        ],
        // $A8:CB1D
        [P.FacingRightShotLaserUpRight] =
        [
            Show(15, 5),
            Show(14, 2),
            Show(13, 2),
            Show(12, 4),
            Cue(new(K.Footstep)),
            Cue(new(K.RightBackwardWatchingLedges)),
            Show(23, 4),
            FallInto(P.FacingRightLaserShotRecoil),
        ],
        // $A8:CB35
        [P.FacingRightLaserShotRecoil] =
        [
            Cue(new(K.RightBackwardWatchingLedges)),
            Show(22, 16),
            Show(21, 5),
            Show(20, 5),
            Show(19, 5),
            Show(18, 5),
            Cue(new(K.Footstep)),
            Cue(new(K.RightBackwardWatchingLedges)),
            Show(17, 10),
            Cue(new(K.RightBackwardWatchingLedges)),
            Show(16, 10),
            Show(15, 10),
            Show(14, 96),
            GoTo(P.FacingRightWalkingForwards),
        ],
        // $A8:CB65
        [P.ApproachingFallLeft] =
        [
            Show(0, 128),
            Show(1, 10),
            Show(2, 10),
            Cue(new(K.CountDownLaserCooldown)),
            GoTo(P.FacingRightWalkingForwards),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params WorkRobotProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => WorkRobotFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<WorkRobotProgram, WorkRobotFrame, WorkRobotCue> Show(int pose, int duration) => AnimationProgramStep<WorkRobotProgram, WorkRobotFrame, WorkRobotCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<WorkRobotProgram, WorkRobotFrame, WorkRobotCue> Cue(WorkRobotCue cue) => AnimationProgramStep<WorkRobotProgram, WorkRobotFrame, WorkRobotCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<WorkRobotProgram, WorkRobotFrame, WorkRobotCue> GoTo(WorkRobotProgram program) => AnimationProgramStep<WorkRobotProgram, WorkRobotFrame, WorkRobotCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<WorkRobotProgram, WorkRobotFrame, WorkRobotCue> FallInto(WorkRobotProgram program) => AnimationProgramStep<WorkRobotProgram, WorkRobotFrame, WorkRobotCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<WorkRobotProgram, WorkRobotFrame, WorkRobotCue> Hold => AnimationProgramStep<WorkRobotProgram, WorkRobotFrame, WorkRobotCue>.Hold;

}
