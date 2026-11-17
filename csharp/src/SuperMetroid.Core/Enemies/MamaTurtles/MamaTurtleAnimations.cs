using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleCueKind;
using P = SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The Mama Turtle's and baby turtles' crawl, spin, shell and dormant animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="MamaTurtleCue"/> naming what it does.
/// </summary>
internal static class MamaTurtleAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MamaTurtleSegment, EnemyFrameId, MamaTurtleCue> Segments { get; } = new(
        new Dictionary<MamaTurtleSegment, AnimationBeat<EnemyFrameId, MamaTurtleCue>[]>
    {
        [P.BabyCrawlingLeft] =
        [
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTucked, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReaching, 10),
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsPlanted, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsPushing, 10),
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTucked, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReaching, 10),
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsPlanted, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsPushing, 10),
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTucked, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReaching, 10),
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTrailing, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsLifting, 10),
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTucked, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReturning, 10),
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTrailing, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsLifting, 10),
            Cue(new MamaTurtleCue.Signal(K.LoopOrTurnAround)),
        ],
        [P.BabySpinning] =
        [
            Show(EnemyFrameId.TurtleBabyShellTurned0, 1),
            Cue(new MamaTurtleCue.Signal(K.PlaySpinningSound)),
            Show(EnemyFrameId.TurtleBabyShellTurned0, 4),
            Show(EnemyFrameId.TurtleBabyShellTurned90, 5),
            Show(EnemyFrameId.TurtleBabyShellTurned180, 5),
            Show(EnemyFrameId.TurtleBabyShellTurned270, 5),
            Cue(new MamaTurtleCue.Signal(K.SetSpinningStoppable)),
        ],
        [P.MamaSpinning] =
        [
            Show(EnemyFrameId.TurtleMamaInShellFacingLeft, 1),
            Cue(new MamaTurtleCue.Signal(K.PlaySpinningSound)),
            Show(EnemyFrameId.TurtleMamaInShellFacingLeft, 4),
            Show(EnemyFrameId.TurtleMamaInShellFacingViewer, 5),
            Show(EnemyFrameId.TurtleMamaInShellFacingRight, 5),
            Show(EnemyFrameId.TurtleMamaInShellFacingViewer, 5),
        ],
        [P.MamaEnterShellLeft] =
        [
            Show(EnemyFrameId.TurtleMamaOutOfShellFacingLeft, 32),
            Show(EnemyFrameId.TurtleMamaWithdrawingFacingLeft, 5),
            Show(EnemyFrameId.TurtleMamaWithdrawnFacingLeft, 5),
            Cue(new MamaTurtleCue.RiseToHover(HorizontalDirection.Left)),
            Hold(EnemyFrameId.TurtleMamaInShellFacingLeft),
        ],
        [P.BabyHidingLeft] =
        [
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReturning, 5),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTrailing, 5),
            Show(EnemyFrameId.TurtleBabyShellTurned0, 64),
            Cue(new MamaTurtleCue.Signal(K.LeaveShell)),
            Hold(EnemyFrameId.TurtleBabyShellTurned0),
        ],
        [P.MamaAsleep] =
        [
            Hold(EnemyFrameId.TurtleMamaInShellFacingLeft),
        ],
        [P.MamaLeaveShellLeft] =
        [
            Show(EnemyFrameId.TurtleMamaInShellFacingLeft, 16),
            Show(EnemyFrameId.TurtleMamaWithdrawnFacingLeft, 5),
            Show(EnemyFrameId.TurtleMamaWithdrawingFacingLeft, 5),
            Show(EnemyFrameId.TurtleMamaOutOfShellFacingLeft, 96),
            Cue(new MamaTurtleCue.Signal(K.EnterShell)),
            Hold(EnemyFrameId.TurtleMamaOutOfShellFacingLeft),
        ],
        [P.BabyLeaveShellLeft] =
        [
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTrailing, 5),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReturning, 47),
            Cue(new MamaTurtleCue.Signal(K.LeftShell)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReturning, 47),
        ],
        [P.BabyCrawlingRight] =
        [
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsTucked, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsReaching, 10),
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsPlanted, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsPushing, 10),
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsTucked, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsReaching, 10),
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsPlanted, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsPushing, 10),
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsTucked, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsReaching, 10),
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsTrailing, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsLifting, 10),
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsTucked, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsReturning, 10),
            Cue(new MamaTurtleCue.Signal(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsTrailing, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsLifting, 10),
            Cue(new MamaTurtleCue.Signal(K.LoopOrTurnAround)),
        ],
        [P.MamaEnterShellRight] =
        [
            Show(EnemyFrameId.TurtleMamaOutOfShellFacingRight, 1),
            Show(EnemyFrameId.TurtleMamaWithdrawingFacingRight, 5),
            Show(EnemyFrameId.TurtleMamaWithdrawnFacingRight, 5),
            Cue(new MamaTurtleCue.RiseToHover(HorizontalDirection.Right)),
            Hold(EnemyFrameId.TurtleMamaInShellFacingRight),
        ],
        [P.BabyHidingRight] =
        [
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsReturning, 5),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsTrailing, 5),
            Show(EnemyFrameId.TurtleBabyShellTurned180, 64),
            Cue(new MamaTurtleCue.Signal(K.LeaveShell)),
            Hold(EnemyFrameId.TurtleBabyShellTurned180),
        ],
        [P.MamaLeaveShellRight] =
        [
            Show(EnemyFrameId.TurtleMamaInShellFacingRight, 16),
            Show(EnemyFrameId.TurtleMamaWithdrawnFacingRight, 5),
            Show(EnemyFrameId.TurtleMamaWithdrawingFacingRight, 5),
            Show(EnemyFrameId.TurtleMamaOutOfShellFacingRight, 96),
            Cue(new MamaTurtleCue.Signal(K.EnterShell)),
            Hold(EnemyFrameId.TurtleMamaOutOfShellFacingRight),
        ],
        [P.BabyLeaveShellRight] =
        [
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTrailing, 5),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReturning, 47),
            Cue(new MamaTurtleCue.Signal(K.LeftShell)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReturning, 47),
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
