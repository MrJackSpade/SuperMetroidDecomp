using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleCueKind;
using P = SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleProgram;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The Mama Turtle's and baby turtles' crawl, spin, shell and sleep animations as typed programs (#641). Each
/// frame shows a <see cref="MamaTurtleFrame"/> pose and each callback becomes a <see cref="MamaTurtleCue"/> with its typed operands.
/// </summary>
internal static class MamaTurtleAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MamaTurtleProgram, MamaTurtleFrame, MamaTurtleCue> Segments { get; } = new(
        new Dictionary<MamaTurtleProgram, AnimationBeat<MamaTurtleFrame, MamaTurtleCue>[]>
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
        ],
        [P.MamaSpinning] =
        [
            Show(11, 1),
            Cue(new(K.PlaySpinningSound)),
            Show(11, 4),
            Show(16, 5),
            Show(15, 5),
            Show(16, 5),
        ],
        [P.MamaEnterShellLeft] =
        [
            Show(14, 32),
            Show(13, 5),
            Show(12, 5),
            Cue(new(K.RiseToHoverRightwards)),
            Show(11, 32767),
        ],
        [P.BabyHidingLeft] =
        [
            Show(4, 5),
            Show(5, 5),
            Show(7, 64),
            Cue(new(K.LeaveShell)),
            Show(7, 32767),
        ],
        [P.MamaAsleep] =
        [
            Show(11, 32767),
        ],
        [P.MamaLeaveShellLeft] =
        [
            Show(11, 16),
            Show(12, 5),
            Show(13, 5),
            Show(14, 96),
            Cue(new(K.EnterShell)),
            Show(14, 32767),
        ],
        [P.BabyLeaveShellLeft] =
        [
            Show(5, 5),
            Show(4, 47),
            Cue(new(K.LeftShell)),
            Show(4, 47),
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
        ],
        [P.MamaEnterShellRight] =
        [
            Show(28, 1),
            Show(27, 5),
            Show(26, 5),
            Cue(new(K.RiseToHoverLeftwards)),
            Show(25, 32767),
        ],
        [P.BabyHidingRight] =
        [
            Show(21, 5),
            Show(22, 5),
            Show(24, 64),
            Cue(new(K.LeaveShell)),
            Show(24, 32767),
        ],
        [P.MamaLeaveShellRight] =
        [
            Show(25, 16),
            Show(26, 5),
            Show(27, 5),
            Show(28, 96),
            Cue(new(K.EnterShell)),
            Show(28, 32767),
        ],
        [P.BabyLeaveShellRight] =
        [
            Show(5, 5),
            Show(4, 47),
            Cue(new(K.LeftShell)),
            Show(4, 47),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MamaTurtleProgram, Animation<MamaTurtleProgram>> StartingAt { get; } = new Dictionary<MamaTurtleProgram, Animation<MamaTurtleProgram>>
    {
        [P.BabyCrawlingLeft] = new([new(P.BabyCrawlingLeft, 0, 25, 1)], AnimationEnd.Holds),
        [P.BabySpinning] = new([new(P.BabySpinning, 0, 7, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MamaSpinning] = new([new(P.MamaSpinning, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MamaEnterShellLeft] = new([new(P.MamaEnterShellLeft, 0, 5, 1)], AnimationEnd.Holds),
        [P.BabyHidingLeft] = new([new(P.BabyHidingLeft, 0, 5, 1)], AnimationEnd.Holds),
        [P.MamaAsleep] = new([new(P.MamaAsleep, 0, 1, 1)], AnimationEnd.Holds),
        [P.MamaLeaveShellLeft] = new([new(P.MamaLeaveShellLeft, 0, 6, 1)], AnimationEnd.Holds),
        [P.BabyLeaveShellLeft] = new([new(P.BabyLeaveShellLeft, 0, 4, 1)], AnimationEnd.Holds),
        [P.BabyCrawlingRight] = new([new(P.BabyCrawlingRight, 0, 25, 1)], AnimationEnd.Holds),
        [P.MamaEnterShellRight] = new([new(P.MamaEnterShellRight, 0, 5, 1)], AnimationEnd.Holds),
        [P.BabyHidingRight] = new([new(P.BabyHidingRight, 0, 5, 1)], AnimationEnd.Holds),
        [P.MamaLeaveShellRight] = new([new(P.MamaLeaveShellRight, 0, 6, 1)], AnimationEnd.Holds),
        [P.BabyLeaveShellRight] = new([new(P.BabyLeaveShellRight, 0, 4, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MamaTurtleProgram> StartingWith(MamaTurtleProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<MamaTurtleFrame, MamaTurtleCue> Show(int pose, int duration) => new AnimationBeat<MamaTurtleFrame, MamaTurtleCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<MamaTurtleFrame, MamaTurtleCue> Cue(MamaTurtleCue cue) => new AnimationBeat<MamaTurtleFrame, MamaTurtleCue>.Event(cue);
}
