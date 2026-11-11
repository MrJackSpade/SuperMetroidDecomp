using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Zoa.ZoaCueKind;
using P = SuperMetroid.Core.Enemies.Zoa.ZoaProgram;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// Zoa's shooting and rising animations as typed programs (#641). Each
/// frame shows a <see cref="ZoaFrame"/> pose and each callback becomes a <see cref="ZoaCue"/> with its typed operands.
/// </summary>
internal static class ZoaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ZoaProgram, ZoaFrame, ZoaCue> Segments { get; } = new(
        new Dictionary<ZoaProgram, AnimationBeat<ZoaFrame, ZoaCue>[]>
    {
        [P.FacingLeftShooting] =
        [
            Cue(new(K.SetSpeedStage1)),
            Show(0, 64),
            Cue(new(K.SetSpeedStage2)),
            Show(1, 8),
            Cue(new(K.SetSpeedStage3)),
            Show(2, 48),
        ],
        [P.FacingLeftRising] =
        [
            Show(4, 4),
            Show(3, 4),
            Show(5, 4),
        ],
        [P.FacingRightShooting] =
        [
            Cue(new(K.SetSpeedStage1)),
            Show(6, 64),
            Cue(new(K.SetSpeedStage2)),
            Show(7, 8),
            Cue(new(K.SetSpeedStage3)),
            Show(8, 48),
        ],
        [P.FacingRightRising] =
        [
            Show(10, 4),
            Show(9, 4),
            Show(11, 4),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ZoaProgram, Animation<ZoaProgram>> StartingAt { get; } = new Dictionary<ZoaProgram, Animation<ZoaProgram>>
    {
        [P.FacingLeftShooting] = new([new(P.FacingLeftShooting, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingLeftRising] = new([new(P.FacingLeftRising, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightShooting] = new([new(P.FacingRightShooting, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightRising] = new([new(P.FacingRightRising, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ZoaProgram> StartingWith(ZoaProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<ZoaFrame, ZoaCue> Show(int pose, int duration) => new AnimationBeat<ZoaFrame, ZoaCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<ZoaFrame, ZoaCue> Cue(ZoaCue cue) => new AnimationBeat<ZoaFrame, ZoaCue>.Event(cue);
}
