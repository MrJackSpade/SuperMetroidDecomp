using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleCueKind;
using P = SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The Mama Turtle's dormant, shell and spin animations (#641, #2342). Each frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="MamaTurtleCue"/> naming
/// what it does.
/// </summary>
internal static partial class MamaTurtleAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MamaTurtleSegment, EnemyFrameId, MamaTurtleCue> Segments { get; } = new(
        new Dictionary<MamaTurtleSegment, AnimationBeat<EnemyFrameId, MamaTurtleCue>[]>
    {
        [P.Spinning] =
        [
            Show(EnemyFrameId.TurtleMamaInShellFacingLeft, 1),
            Cue(new MamaTurtleCue.Signal(K.PlaySpinningSound)),
            Show(EnemyFrameId.TurtleMamaInShellFacingLeft, 4),
            Show(EnemyFrameId.TurtleMamaInShellFacingViewer, 5),
            Show(EnemyFrameId.TurtleMamaInShellFacingRight, 5),
            Show(EnemyFrameId.TurtleMamaInShellFacingViewer, 5),
        ],
        [P.EnterShellLeft] = EnterShell(EnemyFrameId.TurtleMamaOutOfShellFacingLeft, 32, EnemyFrameId.TurtleMamaWithdrawingFacingLeft, EnemyFrameId.TurtleMamaWithdrawnFacingLeft, HorizontalDirection.Left, EnemyFrameId.TurtleMamaInShellFacingLeft),
        [P.Asleep] =
        [
            Hold(EnemyFrameId.TurtleMamaInShellFacingLeft),
        ],
        [P.LeaveShellLeft] = LeaveShell(EnemyFrameId.TurtleMamaInShellFacingLeft, EnemyFrameId.TurtleMamaWithdrawnFacingLeft, EnemyFrameId.TurtleMamaWithdrawingFacingLeft, EnemyFrameId.TurtleMamaOutOfShellFacingLeft),
        [P.EnterShellRight] = EnterShell(EnemyFrameId.TurtleMamaOutOfShellFacingRight, 1, EnemyFrameId.TurtleMamaWithdrawingFacingRight, EnemyFrameId.TurtleMamaWithdrawnFacingRight, HorizontalDirection.Right, EnemyFrameId.TurtleMamaInShellFacingRight),
        [P.LeaveShellRight] = LeaveShell(EnemyFrameId.TurtleMamaInShellFacingRight, EnemyFrameId.TurtleMamaWithdrawnFacingRight, EnemyFrameId.TurtleMamaWithdrawingFacingRight, EnemyFrameId.TurtleMamaOutOfShellFacingRight),
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static Dictionary<MamaTurtleSegment, Animation<MamaTurtleSegment>> StartingAt { get; } = new Dictionary<MamaTurtleSegment, Animation<MamaTurtleSegment>>
    {
        [P.Spinning] = Animation<MamaTurtleSegment>.Looping([Segments.Whole(P.Spinning)]),
        [P.EnterShellLeft] = new([Segments.Whole(P.EnterShellLeft)], AnimationEnd.Holds),
        [P.Asleep] = new([Segments.Whole(P.Asleep)], AnimationEnd.Holds),
        [P.LeaveShellLeft] = new([Segments.Whole(P.LeaveShellLeft)], AnimationEnd.Holds),
        [P.EnterShellRight] = new([Segments.Whole(P.EnterShellRight)], AnimationEnd.Holds),
        [P.LeaveShellRight] = new([Segments.Whole(P.LeaveShellRight)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MamaTurtleSegment> StartingWith(MamaTurtleSegment segment) => StartingAt[segment];

    /// <summary>The mama turtle entering her shell over a facing's poses, <paramref name="mamaOutOfShell"/>, <paramref name="outOfShellUpdates"/>, <paramref name="mamaWithdrawing"/>, <paramref name="mamaWithdrawn"/>, <paramref name="facing"/>, <paramref name="mamaInShell"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MamaTurtleCue>[] EnterShell(EnemyFrameId mamaOutOfShell, int outOfShellUpdates, EnemyFrameId mamaWithdrawing, EnemyFrameId mamaWithdrawn, HorizontalDirection facing, EnemyFrameId mamaInShell) =>
    [
        Show(mamaOutOfShell, outOfShellUpdates),
        Show(mamaWithdrawing, 5),
        Show(mamaWithdrawn, 5),
        Cue(new MamaTurtleCue.RiseToHover(facing)),
        Hold(mamaInShell),
    ];

    /// <summary>The mama turtle leaving her shell over a facing's poses, <paramref name="mamaInShell"/>, <paramref name="mamaWithdrawn"/>, <paramref name="mamaWithdrawing"/>, <paramref name="mamaOutOfShell"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MamaTurtleCue>[] LeaveShell(EnemyFrameId mamaInShell, EnemyFrameId mamaWithdrawn, EnemyFrameId mamaWithdrawing, EnemyFrameId mamaOutOfShell) =>
    [
        Show(mamaInShell, 16),
        Show(mamaWithdrawn, 5),
        Show(mamaWithdrawing, 5),
        Show(mamaOutOfShell, 96),
        Cue(new MamaTurtleCue.Signal(K.EnterShell)),
        Hold(mamaOutOfShell),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MamaTurtleCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, MamaTurtleCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: the animation rests on it until its owner plays another (#1607).</summary>
    private static AnimationBeat<EnemyFrameId, MamaTurtleCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, MamaTurtleCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MamaTurtleCue> Cue(MamaTurtleCue cue) => new AnimationBeat<EnemyFrameId, MamaTurtleCue>.Event(cue);
}
