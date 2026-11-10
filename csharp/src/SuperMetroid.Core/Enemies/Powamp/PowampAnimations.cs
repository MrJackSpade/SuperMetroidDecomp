using P = SuperMetroid.Core.Enemies.Powamp.PowampProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Powamp's body pulses and balloon inflate and sink animations as typed programs (#641). Each
/// frame shows a <see cref="PowampFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class PowampAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PowampProgram, PowampFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<PowampProgram, AnimationBeat<PowampFrame, NoAnimationCue>[]>
    {
        [P.BodyFast] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
        ],
        [P.BodySlow] =
        [
            Show(0, 9),
            Show(1, 9),
            Show(2, 9),
        ],
        [P.BalloonInflate0] =
        [
            Show(3, 1),
        ],
        [P.BalloonInflate1] =
        [
            Show(4, 6),
            Show(5, 160),
        ],
        [P.BalloonStartSinking] =
        [
            Show(5, 1),
            Show(4, 6),
        ],
        [P.BalloonDeflated] =
        [
            Show(3, 160),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PowampProgram, Animation<PowampProgram>> StartingAt { get; } = new Dictionary<PowampProgram, Animation<PowampProgram>>
    {
        [P.BodyFast] = new([new(P.BodyFast, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BodySlow] = new([new(P.BodySlow, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BalloonInflate0] = new([new(P.BalloonInflate0, 0, 1, 1), new(P.BalloonInflate1, 0, 2, 1)], AnimationEnd.Holds),
        [P.BalloonInflate1] = new([new(P.BalloonInflate1, 0, 2, 1)], AnimationEnd.Holds),
        [P.BalloonStartSinking] = new([new(P.BalloonStartSinking, 0, 2, 1), new(P.BalloonDeflated, 0, 1, 1)], AnimationEnd.Holds),
        [P.BalloonDeflated] = new([new(P.BalloonDeflated, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PowampProgram> StartingWith(PowampProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<PowampFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<PowampFrame, NoAnimationCue>.Frame(new(pose), duration);
}
