using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleCueKind;
using P = SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleProgram;
using SuperMetroid.Core.Enemies.MamaTurtles.Artwork;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The Mama Turtle's and baby turtles' crawl, spin, shell and sleep animations as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="MamaTurtleFrame"/> pose and each callback becomes a <see cref="MamaTurtleCue"/> with its typed operands.
/// </summary>
internal static class MamaTurtleAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<MamaTurtleProgram, MamaTurtleFrame, MamaTurtleCue> Programs { get; } = new(
        new Dictionary<MamaTurtleProgram, AnimationProgramStep<MamaTurtleProgram, MamaTurtleFrame, MamaTurtleCue>[]>
    {
        [P.BabyCrawlingLeft] =
        [
            Cue(new(K.Crawl)),
            Show(0, 10),
            Show(1, 10),
            Cue(new(K.Crawl)),
            Show(2, 10),
            Show(3, 10),
            Cue(new(K.Crawl)),
            Show(0, 10),
            Show(1, 10),
            Cue(new(K.Crawl)),
            Show(2, 10),
            Show(3, 10),
            Cue(new(K.Crawl)),
            Show(0, 10),
            Show(1, 10),
            Cue(new(K.Crawl)),
            Show(5, 10),
            Show(6, 10),
            Cue(new(K.Crawl)),
            Show(0, 10),
            Show(4, 10),
            Cue(new(K.Crawl)),
            Show(5, 10),
            Show(6, 10),
            Cue(new(K.LoopOrTurnAround)),
            Hold,
        ],
        [P.BabySpinning] =
        [
            Show(7, 1),
            Cue(new(K.PlaySpinningSound)),
            Show(7, 4),
            Show(8, 5),
            Show(9, 5),
            Show(10, 5),
            Cue(new(K.SetSpinningStoppable)),
            GoTo(P.BabySpinning),
        ],
        [P.MamaSpinning] =
        [
            Show(11, 1),
            Cue(new(K.PlaySpinningSound)),
            Show(11, 4),
            Show(16, 5),
            Show(15, 5),
            Show(16, 5),
            GoTo(P.MamaSpinning),
        ],
        [P.MamaEnterShellLeft] =
        [
            Show(14, 32),
            Show(13, 5),
            Show(12, 5),
            Cue(new(K.RiseToHoverRightwards)),
            Show(11, 32767),
            Hold,
        ],
        [P.BabyHidingLeft] =
        [
            Show(4, 5),
            Show(5, 5),
            Show(7, 64),
            Cue(new(K.LeaveShell)),
            Show(7, 32767),
            Hold,
        ],
        [P.MamaAsleep] =
        [
            Show(11, 32767),
            Hold,
        ],
        [P.MamaLeaveShellLeft] =
        [
            Show(11, 16),
            Show(12, 5),
            Show(13, 5),
            Show(14, 96),
            Cue(new(K.EnterShell)),
            Show(14, 32767),
            Hold,
        ],
        [P.BabyLeaveShellLeft] =
        [
            Show(5, 5),
            Show(4, 47),
            Cue(new(K.LeftShell)),
            Show(4, 47),
            Hold,
        ],
        [P.BabyCrawlingRight] =
        [
            Cue(new(K.Crawl)),
            Show(17, 10),
            Show(18, 10),
            Cue(new(K.Crawl)),
            Show(19, 10),
            Show(20, 10),
            Cue(new(K.Crawl)),
            Show(17, 10),
            Show(18, 10),
            Cue(new(K.Crawl)),
            Show(19, 10),
            Show(20, 10),
            Cue(new(K.Crawl)),
            Show(17, 10),
            Show(18, 10),
            Cue(new(K.Crawl)),
            Show(22, 10),
            Show(23, 10),
            Cue(new(K.Crawl)),
            Show(17, 10),
            Show(21, 10),
            Cue(new(K.Crawl)),
            Show(22, 10),
            Show(23, 10),
            Cue(new(K.LoopOrTurnAround)),
            Hold,
        ],
        [P.MamaEnterShellRight] =
        [
            Show(28, 1),
            Show(27, 5),
            Show(26, 5),
            Cue(new(K.RiseToHoverLeftwards)),
            Show(25, 32767),
            Hold,
        ],
        [P.BabyHidingRight] =
        [
            Show(21, 5),
            Show(22, 5),
            Show(24, 64),
            Cue(new(K.LeaveShell)),
            Show(24, 32767),
            Hold,
        ],
        [P.MamaLeaveShellRight] =
        [
            Show(25, 16),
            Show(26, 5),
            Show(27, 5),
            Show(28, 96),
            Cue(new(K.EnterShell)),
            Show(28, 32767),
            Hold,
        ],
        [P.BabyLeaveShellRight] =
        [
            Show(5, 5),
            Show(4, 47),
            Cue(new(K.LeftShell)),
            Show(4, 47),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<MamaTurtleProgram, MamaTurtleFrame, MamaTurtleCue> Show(int pose, int duration) => AnimationProgramStep<MamaTurtleProgram, MamaTurtleFrame, MamaTurtleCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<MamaTurtleProgram, MamaTurtleFrame, MamaTurtleCue> Cue(MamaTurtleCue cue) => AnimationProgramStep<MamaTurtleProgram, MamaTurtleFrame, MamaTurtleCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<MamaTurtleProgram, MamaTurtleFrame, MamaTurtleCue> GoTo(MamaTurtleProgram program) => AnimationProgramStep<MamaTurtleProgram, MamaTurtleFrame, MamaTurtleCue>.GoTo(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<MamaTurtleProgram, MamaTurtleFrame, MamaTurtleCue> Hold => AnimationProgramStep<MamaTurtleProgram, MamaTurtleFrame, MamaTurtleCue>.Hold;

}
