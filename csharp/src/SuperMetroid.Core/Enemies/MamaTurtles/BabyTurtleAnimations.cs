using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.MamaTurtles.BabyTurtleCueKind;
using P = SuperMetroid.Core.Enemies.MamaTurtles.BabyTurtleSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The baby turtles' crawl, hiding, shell and spin animations (#641, #2342). Each frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="BabyTurtleCue"/> naming
/// what it does.
/// </summary>
internal static partial class BabyTurtleAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BabyTurtleSegment, EnemyFrameId, BabyTurtleCue> Segments { get; } = new(
        new Dictionary<BabyTurtleSegment, AnimationBeat<EnemyFrameId, BabyTurtleCue>[]>
    {
        [P.CrawlingLeft] = Crawl(EnemyFrameId.TurtleBabyCrawlingLeftLegsTucked, EnemyFrameId.TurtleBabyCrawlingLeftLegsReaching, EnemyFrameId.TurtleBabyCrawlingLeftLegsPlanted, EnemyFrameId.TurtleBabyCrawlingLeftLegsPushing),
        [P.CrawlingLeftLegsTrailing] = CrawlLegsTrailing(EnemyFrameId.TurtleBabyCrawlingLeftLegsTucked, EnemyFrameId.TurtleBabyCrawlingLeftLegsReaching, EnemyFrameId.TurtleBabyCrawlingLeftLegsTrailing, EnemyFrameId.TurtleBabyCrawlingLeftLegsLifting, EnemyFrameId.TurtleBabyCrawlingLeftLegsReturning),
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
        [P.HidingLeft] = Hiding(EnemyFrameId.TurtleBabyCrawlingLeftLegsReturning, EnemyFrameId.TurtleBabyCrawlingLeftLegsTrailing, EnemyFrameId.TurtleBabyShellTurned0),
        [P.LeaveShell] =
        [
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsTrailing, 5),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReturning, 47),
            Cue(new BabyTurtleCue(K.LeftShell)),
            Show(EnemyFrameId.TurtleBabyCrawlingLeftLegsReturning, 47),
        ],
        [P.CrawlingRight] = Crawl(EnemyFrameId.TurtleBabyCrawlingRightLegsTucked, EnemyFrameId.TurtleBabyCrawlingRightLegsReaching, EnemyFrameId.TurtleBabyCrawlingRightLegsPlanted, EnemyFrameId.TurtleBabyCrawlingRightLegsPushing),
        [P.CrawlingRightLegsTrailing] = CrawlLegsTrailing(EnemyFrameId.TurtleBabyCrawlingRightLegsTucked, EnemyFrameId.TurtleBabyCrawlingRightLegsReaching, EnemyFrameId.TurtleBabyCrawlingRightLegsTrailing, EnemyFrameId.TurtleBabyCrawlingRightLegsLifting, EnemyFrameId.TurtleBabyCrawlingRightLegsReturning),
        [P.HidingRight] = Hiding(EnemyFrameId.TurtleBabyCrawlingRightLegsReturning, EnemyFrameId.TurtleBabyCrawlingRightLegsTrailing, EnemyFrameId.TurtleBabyShellTurned180),
    });

    /// <summary>The strides a crawl takes with its legs planting and pushing before its strides with them trailing (#2439).</summary>
    private const int PushingStrides = 2;

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. A crawl strides
    /// <see cref="PushingStrides"/> times with its legs planting and pushing, then with them trailing, and ends at the check
    /// that crawls on or turns the baby round.
    /// </summary>
    internal static Dictionary<BabyTurtleSegment, Animation<BabyTurtleSegment>> StartingAt { get; } = new Dictionary<BabyTurtleSegment, Animation<BabyTurtleSegment>>
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

    /// <summary>A baby turtle's crawl over a facing's leg poses, <paramref name="legsTucked"/>, <paramref name="legsReaching"/>, <paramref name="legsPlanted"/>, <paramref name="legsPushing"/>.</summary>
    private static AnimationBeat<EnemyFrameId, BabyTurtleCue>[] Crawl(EnemyFrameId legsTucked, EnemyFrameId legsReaching, EnemyFrameId legsPlanted, EnemyFrameId legsPushing) =>
    [
        Cue(new BabyTurtleCue(K.Crawl)),
        Show(legsTucked, 10),
        Show(legsReaching, 10),
        Cue(new BabyTurtleCue(K.Crawl)),
        Show(legsPlanted, 10),
        Show(legsPushing, 10),
    ];

    /// <summary>A baby turtle's crawl with its legs trailing, over a facing's leg poses, <paramref name="legsTucked"/>, <paramref name="legsReaching"/>, <paramref name="legsTrailing"/>, <paramref name="legsLifting"/>, <paramref name="legsReturning"/>.</summary>
    private static AnimationBeat<EnemyFrameId, BabyTurtleCue>[] CrawlLegsTrailing(EnemyFrameId legsTucked, EnemyFrameId legsReaching, EnemyFrameId legsTrailing, EnemyFrameId legsLifting, EnemyFrameId legsReturning) =>
    [
        Cue(new BabyTurtleCue(K.Crawl)),
        Show(legsTucked, 10),
        Show(legsReaching, 10),
        Cue(new BabyTurtleCue(K.Crawl)),
        Show(legsTrailing, 10),
        Show(legsLifting, 10),
        Cue(new BabyTurtleCue(K.Crawl)),
        Show(legsTucked, 10),
        Show(legsReturning, 10),
        Cue(new BabyTurtleCue(K.Crawl)),
        Show(legsTrailing, 10),
        Show(legsLifting, 10),
        Cue(new BabyTurtleCue(K.LoopOrTurnAround)),
    ];

    /// <summary>A baby turtle hiding in its shell over a facing's poses, <paramref name="legsReturning"/>, <paramref name="legsTrailing"/>, <paramref name="turtleBabyShell"/>.</summary>
    private static AnimationBeat<EnemyFrameId, BabyTurtleCue>[] Hiding(EnemyFrameId legsReturning, EnemyFrameId legsTrailing, EnemyFrameId turtleBabyShell) =>
    [
        Show(legsReturning, 5),
        Show(legsTrailing, 5),
        Show(turtleBabyShell, 64),
        Cue(new BabyTurtleCue(K.LeaveShell)),
        Hold(turtleBabyShell),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, BabyTurtleCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, BabyTurtleCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: the animation rests on it until its owner plays another (#1607).</summary>
    private static AnimationBeat<EnemyFrameId, BabyTurtleCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, BabyTurtleCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, BabyTurtleCue> Cue(BabyTurtleCue cue) => new AnimationBeat<EnemyFrameId, BabyTurtleCue>.Event(cue);
}
