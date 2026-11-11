using P = SuperMetroid.Core.Enemies.Ripper.RipperProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The GRipper, Ripper II and Ripper flying loops as typed programs (#641). Each
/// frame shows a <see cref="RipperFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class RipperAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<RipperProgram, RipperFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<RipperProgram, AnimationBeat<RipperFrame, NoAnimationCue>[]>
    {
        [P.GRipperMovingLeft] =
        [
            Show(0, 8),
            Show(1, 7),
            Show(0, 8),
            Show(2, 7),
        ],
        [P.GRipperMovingRight] =
        [
            Show(3, 8),
            Show(4, 7),
            Show(3, 8),
            Show(5, 7),
        ],
        [P.Ripper2MovingRight] =
        [
            Show(0, 8),
            Show(1, 7),
            Show(0, 8),
            Show(2, 7),
        ],
        [P.Ripper2MovingLeft] =
        [
            Show(3, 8),
            Show(4, 7),
            Show(3, 8),
            Show(5, 7),
        ],
        [P.RipperMovingRight] =
        [
            Show(9, 8),
            Show(10, 7),
            Show(9, 8),
            Show(11, 7),
        ],
        [P.RipperMovingLeft] =
        [
            Show(6, 8),
            Show(7, 7),
            Show(6, 8),
            Show(8, 7),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<RipperProgram, Animation<RipperProgram>> StartingAt { get; } = new Dictionary<RipperProgram, Animation<RipperProgram>>
    {
        [P.GRipperMovingLeft] = new([new(P.GRipperMovingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GRipperMovingRight] = new([new(P.GRipperMovingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Ripper2MovingRight] = new([new(P.Ripper2MovingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Ripper2MovingLeft] = new([new(P.Ripper2MovingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RipperMovingRight] = new([new(P.RipperMovingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RipperMovingLeft] = new([new(P.RipperMovingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RipperProgram> StartingWith(RipperProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<RipperFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<RipperFrame, NoAnimationCue>.Frame(new(pose), duration);
}
