using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleCueKind;
using P = SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The Mama Turtle's and baby turtles' crawl, spin, shell and sleep animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="MamaTurtleCue"/> with its typed operands.
/// </summary>
internal static class MamaTurtleAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MamaTurtleProgram, EnemyFrameId, MamaTurtleCue> Segments { get; } = new(
        new Dictionary<MamaTurtleProgram, AnimationBeat<EnemyFrameId, MamaTurtleCue>[]>
    {
        [P.BabyCrawlingLeft] =
        [
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori00, 10),
            Show(EnemyFrameId.Tatori01, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori02, 10),
            Show(EnemyFrameId.Tatori03, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori00, 10),
            Show(EnemyFrameId.Tatori01, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori02, 10),
            Show(EnemyFrameId.Tatori03, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori00, 10),
            Show(EnemyFrameId.Tatori01, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori04, 10),
            Show(EnemyFrameId.Tatori05, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori00, 10),
            Show(EnemyFrameId.Tatori06, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori04, 10),
            Show(EnemyFrameId.Tatori05, 10),
            Cue(new(K.LoopOrTurnAround)),
        ],
        [P.BabySpinning] =
        [
            Show(EnemyFrameId.Tatori07, 1),
            Cue(new(K.PlaySpinningSound)),
            Show(EnemyFrameId.Tatori07, 4),
            Show(EnemyFrameId.Tatori08, 5),
            Show(EnemyFrameId.Tatori09, 5),
            Show(EnemyFrameId.Tatori10, 5),
            Cue(new(K.SetSpinningStoppable)),
        ],
        [P.MamaSpinning] =
        [
            Show(EnemyFrameId.Tatori11, 1),
            Cue(new(K.PlaySpinningSound)),
            Show(EnemyFrameId.Tatori11, 4),
            Show(EnemyFrameId.Tatori12, 5),
            Show(EnemyFrameId.Tatori13, 5),
            Show(EnemyFrameId.Tatori12, 5),
        ],
        [P.MamaEnterShellLeft] =
        [
            Show(EnemyFrameId.Tatori14, 32),
            Show(EnemyFrameId.Tatori15, 5),
            Show(EnemyFrameId.Tatori16, 5),
            Cue(new(K.RiseToHoverRightwards)),
            Hold(EnemyFrameId.Tatori11),
        ],
        [P.BabyHidingLeft] =
        [
            Show(EnemyFrameId.Tatori06, 5),
            Show(EnemyFrameId.Tatori04, 5),
            Show(EnemyFrameId.Tatori07, 64),
            Cue(new(K.LeaveShell)),
            Hold(EnemyFrameId.Tatori07),
        ],
        [P.MamaAsleep] =
        [
            Hold(EnemyFrameId.Tatori11),
        ],
        [P.MamaLeaveShellLeft] =
        [
            Show(EnemyFrameId.Tatori11, 16),
            Show(EnemyFrameId.Tatori16, 5),
            Show(EnemyFrameId.Tatori15, 5),
            Show(EnemyFrameId.Tatori14, 96),
            Cue(new(K.EnterShell)),
            Hold(EnemyFrameId.Tatori14),
        ],
        [P.BabyLeaveShellLeft] =
        [
            Show(EnemyFrameId.Tatori04, 5),
            Show(EnemyFrameId.Tatori06, 47),
            Cue(new(K.LeftShell)),
            Show(EnemyFrameId.Tatori06, 47),
        ],
        [P.BabyCrawlingRight] =
        [
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori17, 10),
            Show(EnemyFrameId.Tatori18, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori19, 10),
            Show(EnemyFrameId.Tatori20, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori17, 10),
            Show(EnemyFrameId.Tatori18, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori19, 10),
            Show(EnemyFrameId.Tatori20, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori17, 10),
            Show(EnemyFrameId.Tatori18, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori21, 10),
            Show(EnemyFrameId.Tatori22, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori17, 10),
            Show(EnemyFrameId.Tatori23, 10),
            Cue(new(K.Crawl)),
            Show(EnemyFrameId.Tatori21, 10),
            Show(EnemyFrameId.Tatori22, 10),
            Cue(new(K.LoopOrTurnAround)),
        ],
        [P.MamaEnterShellRight] =
        [
            Show(EnemyFrameId.Tatori24, 1),
            Show(EnemyFrameId.Tatori25, 5),
            Show(EnemyFrameId.Tatori26, 5),
            Cue(new(K.RiseToHoverLeftwards)),
            Hold(EnemyFrameId.Tatori27),
        ],
        [P.BabyHidingRight] =
        [
            Show(EnemyFrameId.Tatori23, 5),
            Show(EnemyFrameId.Tatori21, 5),
            Show(EnemyFrameId.Tatori28, 64),
            Cue(new(K.LeaveShell)),
            Hold(EnemyFrameId.Tatori28),
        ],
        [P.MamaLeaveShellRight] =
        [
            Show(EnemyFrameId.Tatori27, 16),
            Show(EnemyFrameId.Tatori26, 5),
            Show(EnemyFrameId.Tatori25, 5),
            Show(EnemyFrameId.Tatori24, 96),
            Cue(new(K.EnterShell)),
            Hold(EnemyFrameId.Tatori24),
        ],
        [P.BabyLeaveShellRight] =
        [
            Show(EnemyFrameId.Tatori04, 5),
            Show(EnemyFrameId.Tatori06, 47),
            Cue(new(K.LeftShell)),
            Show(EnemyFrameId.Tatori06, 47),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MamaTurtleCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, MamaTurtleCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: the animation rests on it until its owner plays another (#1607).</summary>
    private static AnimationBeat<EnemyFrameId, MamaTurtleCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, MamaTurtleCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MamaTurtleCue> Cue(MamaTurtleCue cue) => new AnimationBeat<EnemyFrameId, MamaTurtleCue>.Event(cue);
}
