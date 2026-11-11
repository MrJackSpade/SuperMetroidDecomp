using P = SuperMetroid.Core.Enemies.Puyo.PuyoProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// Puyo's grounded loops and airborne hop poses as typed programs (#641). Each
/// frame shows a <see cref="PuyoFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class PuyoAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PuyoProgram, PuyoFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<PuyoProgram, AnimationBeat<PuyoFrame, NoAnimationCue>[]>
    {
        [P.GroundedFast] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(1, 5),
        ],
        [P.GroundedMedium] =
        [
            Show(0, 8),
            Show(1, 8),
            Show(2, 8),
            Show(1, 8),
        ],
        [P.GroundedSlow] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(1, 10),
        ],
        [P.RightFrame0LeftFrame4] =
        [
            Show(7, 1),
        ],
        [P.RightFrame1LeftFrame3] =
        [
            Show(6, 1),
        ],
        [P.Frame2] =
        [
            Show(5, 1),
        ],
        [P.RightFrame3LeftFrame1] =
        [
            Show(4, 1),
        ],
        [P.RightFrame4LeftFrame0] =
        [
            Show(3, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PuyoProgram, Animation<PuyoProgram>> StartingAt { get; } = new Dictionary<PuyoProgram, Animation<PuyoProgram>>
    {
        [P.GroundedFast] = new([new(P.GroundedFast, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GroundedMedium] = new([new(P.GroundedMedium, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GroundedSlow] = new([new(P.GroundedSlow, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RightFrame0LeftFrame4] = new([new(P.RightFrame0LeftFrame4, 0, 1, 1)], AnimationEnd.Holds),
        [P.RightFrame1LeftFrame3] = new([new(P.RightFrame1LeftFrame3, 0, 1, 1)], AnimationEnd.Holds),
        [P.Frame2] = new([new(P.Frame2, 0, 1, 1)], AnimationEnd.Holds),
        [P.RightFrame3LeftFrame1] = new([new(P.RightFrame3LeftFrame1, 0, 1, 1)], AnimationEnd.Holds),
        [P.RightFrame4LeftFrame0] = new([new(P.RightFrame4LeftFrame0, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PuyoProgram> StartingWith(PuyoProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<PuyoFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<PuyoFrame, NoAnimationCue>.Frame(new(pose), duration);
}
