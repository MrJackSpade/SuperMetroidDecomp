using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleCueKind;
using P = SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The Mama Turtle's and baby turtles' crawl, spin, shell and sleep animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="MamaTurtleCue"/> with its typed operands.
/// </summary>
internal static class MamaTurtleAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MamaTurtleSegment, EnemyFrameId, MamaTurtleCue> Segments { get; } = new(
        new Dictionary<MamaTurtleSegment, AnimationBeat<EnemyFrameId, MamaTurtleCue>[]>
    {
        [P.BabyCrawlingLeft] =
        [
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftFirst, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftSecond, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftThird, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftFourth, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftFirst, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftSecond, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftThird, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftFourth, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftFirst, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftSecond, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftFifth, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftSixth, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftFirst, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftSeventh, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftFifth, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftSixth, 10),
            Cue(new(K.LoopOrTurnAround)),
        ],
        [P.BabySpinning] =
        [
            Show(EnemyFrameId.TurtleBabySpinningFirst, 1),
            Cue(new(K.PlaySpinningSound)),
            Show(EnemyFrameId.TurtleBabySpinningFirst, 4),
            Show(EnemyFrameId.TurtleBabySpinningSecond, 5),
            Show(EnemyFrameId.TurtleBabySpinningThird, 5),
            Show(EnemyFrameId.TurtleBabySpinningFourth, 5),
            Cue(new(K.SetSpinningStoppable)),
        ],
        [P.MamaSpinning] =
        [
            Show(EnemyFrameId.TurtleMamaSpinningFirst, 1),
            Cue(new(K.PlaySpinningSound)),
            Show(EnemyFrameId.TurtleMamaSpinningFirst, 4),
            Show(EnemyFrameId.TurtleMamaSpinningSecond, 5),
            Show(EnemyFrameId.TurtleMamaSpinningThird, 5),
            Show(EnemyFrameId.TurtleMamaSpinningSecond, 5),
        ],
        [P.MamaEnterShellLeft] =
        [
            Show(EnemyFrameId.TurtleMamaEnterShellLeftFirst, 32),
            Show(EnemyFrameId.TurtleMamaEnterShellLeftSecond, 5),
            Show(EnemyFrameId.TurtleMamaEnterShellLeftThird, 5),
            Cue(new(K.RiseToHoverRightwards)),
            Hold(EnemyFrameId.TurtleMamaSpinningFirst),
        ],
        [P.BabyHidingLeft] =
        [
            Show(EnemyFrameId.TurtleBabyCrawlingLeftSeventh, 5),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftFifth, 5),
            Show(EnemyFrameId.TurtleBabySpinningFirst, 64),
            Cue(new(K.LeaveShell)),
            Hold(EnemyFrameId.TurtleBabySpinningFirst),
        ],
        [P.MamaAsleep] =
        [
            Hold(EnemyFrameId.TurtleMamaSpinningFirst),
        ],
        [P.MamaLeaveShellLeft] =
        [
            Show(EnemyFrameId.TurtleMamaSpinningFirst, 16),
            Show(EnemyFrameId.TurtleMamaEnterShellLeftThird, 5),
            Show(EnemyFrameId.TurtleMamaEnterShellLeftSecond, 5),
            Show(EnemyFrameId.TurtleMamaEnterShellLeftFirst, 96),
            Cue(new(K.EnterShell)),
            Hold(EnemyFrameId.TurtleMamaEnterShellLeftFirst),
        ],
        [P.BabyLeaveShellLeft] =
        [
            Show(EnemyFrameId.TurtleBabyCrawlingLeftFifth, 5),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftSeventh, 47),
            Cue(new(K.LeftShell)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftSeventh, 47),
        ],
        [P.BabyCrawlingRight] =
        [
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightFirst, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightSecond, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightThird, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightFourth, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightFirst, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightSecond, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightThird, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightFourth, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightFirst, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightSecond, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightFifth, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightSixth, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightFirst, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightSeventh, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightFifth, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightSixth, 10),
            Cue(new(K.LoopOrTurnAround)),
        ],
        [P.MamaEnterShellRight] =
        [
            Show(EnemyFrameId.TurtleMamaEnterShellRightFirst, 1),
            Show(EnemyFrameId.TurtleMamaEnterShellRightSecond, 5),
            Show(EnemyFrameId.TurtleMamaEnterShellRightThird, 5),
            Cue(new(K.RiseToHoverLeftwards)),
            Hold(EnemyFrameId.TurtleMamaEnterShellRightFourth),
        ],
        [P.BabyHidingRight] =
        [
            Show(EnemyFrameId.TurtleBabyCrawlingRightSeventh, 5),
            Show(EnemyFrameId.TurtleBabyCrawlingRightFifth, 5),
            Show(EnemyFrameId.TurtleBabyHidingRightThird, 64),
            Cue(new(K.LeaveShell)),
            Hold(EnemyFrameId.TurtleBabyHidingRightThird),
        ],
        [P.MamaLeaveShellRight] =
        [
            Show(EnemyFrameId.TurtleMamaEnterShellRightFourth, 16),
            Show(EnemyFrameId.TurtleMamaEnterShellRightThird, 5),
            Show(EnemyFrameId.TurtleMamaEnterShellRightSecond, 5),
            Show(EnemyFrameId.TurtleMamaEnterShellRightFirst, 96),
            Cue(new(K.EnterShell)),
            Hold(EnemyFrameId.TurtleMamaEnterShellRightFirst),
        ],
        [P.BabyLeaveShellRight] =
        [
            Show(EnemyFrameId.TurtleBabyCrawlingLeftFifth, 5),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftSeventh, 47),
            Cue(new(K.LeftShell)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftSeventh, 47),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MamaTurtleSegment, Animation<MamaTurtleSegment>> StartingAt { get; } = new Dictionary<MamaTurtleSegment, Animation<MamaTurtleSegment>>
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
    internal static Animation<MamaTurtleSegment> StartingWith(MamaTurtleSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MamaTurtleCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, MamaTurtleCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: the animation rests on it until its owner plays another (#1607).</summary>
    private static AnimationBeat<EnemyFrameId, MamaTurtleCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, MamaTurtleCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MamaTurtleCue> Cue(MamaTurtleCue cue) => new AnimationBeat<EnemyFrameId, MamaTurtleCue>.Event(cue);
}
