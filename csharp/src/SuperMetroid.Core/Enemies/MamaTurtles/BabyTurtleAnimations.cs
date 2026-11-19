using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.MamaTurtles.BabyTurtleCueKind;
using P = SuperMetroid.Core.Enemies.MamaTurtles.BabyTurtleSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The baby turtles' crawl, hiding, shell and spin animations (#641, #2342). Each frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="BabyTurtleCue"/> naming
/// what it does.
/// </summary>
internal static class BabyTurtleAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BabyTurtleSegment, EnemyFrameId, BabyTurtleCue> Segments { get; } = new(
        new Dictionary<BabyTurtleSegment, AnimationBeat<EnemyFrameId, BabyTurtleCue>[]>
    {
        [P.CrawlingLeft] =
        [
            Cue(new BabyTurtleCue(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTucked, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReaching, 10),
            Cue(new BabyTurtleCue(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsPlanted, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsPushing, 10),
        ],
        [P.CrawlingLeftLegsTrailing] =
        [
            Cue(new BabyTurtleCue(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTucked, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReaching, 10),
            Cue(new BabyTurtleCue(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTrailing, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsLifting, 10),
            Cue(new BabyTurtleCue(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTucked, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReturning, 10),
            Cue(new BabyTurtleCue(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTrailing, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsLifting, 10),
            Cue(new BabyTurtleCue(K.LoopOrTurnAround)),
        ],
        [P.Spinning] =
        [
            Show(EnemyFrameId.TurtleBabyShellTurned0, 1),
            Cue(new BabyTurtleCue(K.PlaySpinningSound)),
            Show(EnemyFrameId.TurtleBabyShellTurned0, 4),
            Show(EnemyFrameId.TurtleBabyShellTurned90, 5),
            Show(EnemyFrameId.TurtleBabyShellTurned180, 5),
            Show(EnemyFrameId.TurtleBabyShellTurned270, 5),
            Cue(new BabyTurtleCue(K.SetSpinningStoppable)),
        ],
        [P.HidingLeft] =
        [
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReturning, 5),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTrailing, 5),
            Show(EnemyFrameId.TurtleBabyShellTurned0, 64),
            Cue(new BabyTurtleCue(K.LeaveShell)),
            Hold(EnemyFrameId.TurtleBabyShellTurned0),
        ],
        [P.LeaveShell] =
        [
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTrailing, 5),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReturning, 47),
            Cue(new BabyTurtleCue(K.LeftShell)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReturning, 47),
        ],
        [P.CrawlingRight] =
        [
            Cue(new BabyTurtleCue(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsTucked, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsReaching, 10),
            Cue(new BabyTurtleCue(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsPlanted, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsPushing, 10),
        ],
        [P.CrawlingRightLegsTrailing] =
        [
            Cue(new BabyTurtleCue(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsTucked, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsReaching, 10),
            Cue(new BabyTurtleCue(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsTrailing, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsLifting, 10),
            Cue(new BabyTurtleCue(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsTucked, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsReturning, 10),
            Cue(new BabyTurtleCue(K.Crawl)),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsTrailing, 10),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsLifting, 10),
            Cue(new BabyTurtleCue(K.LoopOrTurnAround)),
        ],
        [P.HidingRight] =
        [
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsReturning, 5),
            Show(EnemyFrameId.TurtleBabyCrawlingRightLegsTrailing, 5),
            Show(EnemyFrameId.TurtleBabyShellTurned180, 64),
            Cue(new BabyTurtleCue(K.LeaveShell)),
            Hold(EnemyFrameId.TurtleBabyShellTurned180),
        ],
    });

    /// <summary>The strides a crawl takes with its legs planting and pushing before its strides with them trailing (#2439).</summary>
    private const int PushingStrides = 2;

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. A crawl strides
    /// <see cref="PushingStrides"/> times with its legs planting and pushing, then with them trailing, and ends at the check
    /// that crawls on or turns the baby round.
    /// </summary>
    private static IReadOnlyDictionary<BabyTurtleSegment, Animation<BabyTurtleSegment>> StartingAt { get; } = new Dictionary<BabyTurtleSegment, Animation<BabyTurtleSegment>>
    {
        [P.CrawlingLeft] = new([Segments.Repeated(P.CrawlingLeft, PushingStrides), Segments.Whole(P.CrawlingLeftLegsTrailing)], AnimationEnd.Holds),
        [P.Spinning] = Animation<BabyTurtleSegment>.Looping([Segments.Whole(P.Spinning)]),
        [P.HidingLeft] = new([Segments.Whole(P.HidingLeft)], AnimationEnd.Holds),
        [P.LeaveShell] = new([Segments.Whole(P.LeaveShell)], AnimationEnd.Holds),
        [P.CrawlingRight] = new([Segments.Repeated(P.CrawlingRight, PushingStrides), Segments.Whole(P.CrawlingRightLegsTrailing)], AnimationEnd.Holds),
        [P.HidingRight] = new([Segments.Whole(P.HidingRight)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BabyTurtleSegment> StartingWith(BabyTurtleSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, BabyTurtleCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, BabyTurtleCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: the animation rests on it until its owner plays another (#1607).</summary>
    private static AnimationBeat<EnemyFrameId, BabyTurtleCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, BabyTurtleCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, BabyTurtleCue> Cue(BabyTurtleCue cue) => new AnimationBeat<EnemyFrameId, BabyTurtleCue>.Event(cue);
}
