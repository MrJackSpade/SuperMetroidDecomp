using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Bosses.Kraid.FakeKraidSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Fake Kraid's action choice, step and spit animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="FakeKraidCue"/> saying what it does.
/// </summary>
internal static partial class FakeKraidAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FakeKraidSegment, EnemyFrameId, FakeKraidCue> Segments { get; } = new(
        new Dictionary<FakeKraidSegment, AnimationBeat<EnemyFrameId, FakeKraidCue>[]>
    {
        [P.ChooseAction] =
        [
            Cue(new FakeKraidCue.ChooseAction()),
        ],
        [P.StepForwardsFacingLeft] = StepForwards(FakeKraidPoses.Left),
        [P.StepBackwardsFacingLeft] = StepBackwards(FakeKraidPoses.Left),
        [P.FireSpitFacingLeft] = FireSpit(FakeKraidPoses.Left),
        [P.StepForwardsFacingRight] = StepForwards(FakeKraidPoses.Right),
        [P.StepBackwardsFacingRight] = StepBackwards(FakeKraidPoses.Right),
        [P.FireSpitFacingRight] = FireSpit(FakeKraidPoses.Right),
    });

    /// <summary>
    /// The animation that starts at each step and spit (#2214): it plays, then chooses the next action, which always
    /// continues at the step or spit it picks, so no animation plays past its choice and none starts at the choice.
    /// </summary>
    internal static Dictionary<FakeKraidSegment, Animation<FakeKraidSegment>> StartingAt { get; } =
        new[]
        {
            P.StepForwardsFacingLeft, P.StepBackwardsFacingLeft, P.FireSpitFacingLeft,
            P.StepForwardsFacingRight, P.StepBackwardsFacingRight, P.FireSpitFacingRight,
        }.ToDictionary(segment => segment, segment => new Animation<FakeKraidSegment>(
            [Segments.Whole(segment), Segments.Whole(P.ChooseAction)], AnimationEnd.Holds));

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FakeKraidSegment> StartingWith(FakeKraidSegment segment) => StartingAt[segment];

    /// <summary>His step forwards over a facing's poses (#2557): feet together, the front foot lifting and forward, then planted, moving as it plants.</summary>
    private static AnimationBeat<EnemyFrameId, FakeKraidCue>[] StepForwards(FakeKraidPoses poses) =>
    [
        Show(poses.SteppingFeetTogether, 16),
        Show(poses.SteppingFrontFootLifting, 12),
        Show(poses.SteppingFrontFootForward, 8),
        Show(poses.SteppingFeetPlanted, 12),
        Cue(new FakeKraidCue.Move()),
    ];

    /// <summary>His step backwards over a facing's poses (#2557): feet together, moving, then planted and the front foot forward and lifting.</summary>
    private static AnimationBeat<EnemyFrameId, FakeKraidCue>[] StepBackwards(FakeKraidPoses poses) =>
    [
        Show(poses.SteppingFeetTogether, 16),
        Cue(new FakeKraidCue.Move()),
        Show(poses.SteppingFeetPlanted, 12),
        Show(poses.SteppingFrontFootForward, 8),
        Show(poses.SteppingFrontFootLifting, 12),
    ];

    /// <summary>His spit over a facing's poses (#2557): his mouth closed, his cry, opening, spitting the way he faces, wide and closing.</summary>
    private static AnimationBeat<EnemyFrameId, FakeKraidCue>[] FireSpit(FakeKraidPoses poses) =>
    [
        Show(poses.MouthClosed, 16),
        Cue(new FakeKraidCue.PlayCrySound()),
        Show(poses.MouthOpening, 8),
        Cue(new FakeKraidCue.Spit(poses.Facing)),
        Show(poses.MouthWide, 16),
        Show(poses.MouthOpening, 8),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, FakeKraidCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, FakeKraidCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, FakeKraidCue> Cue(FakeKraidCue cue) => new AnimationBeat<EnemyFrameId, FakeKraidCue>.Event(cue);
}
