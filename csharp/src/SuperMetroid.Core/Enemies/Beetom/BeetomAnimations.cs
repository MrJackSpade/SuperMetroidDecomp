using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Beetom.BeetomCueKind;
using P = SuperMetroid.Core.Enemies.Beetom.BeetomProgram;

namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The Beetom's crawl, hop and drain animations as typed programs (#641). Each
/// frame shows a <see cref="BeetomFrame"/> pose and each callback becomes a <see cref="BeetomCue"/> with its typed operands.
/// </summary>
internal static class BeetomAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BeetomProgram, BeetomFrame, BeetomCue> Segments { get; } = new(
        new Dictionary<BeetomProgram, AnimationBeat<BeetomFrame, BeetomCue>[]>
    {
        [P.CrawlingLeft] =
        [
            Cue(new(K.StopProcessingOffScreen)),
        ],
        [P.CrawlingLeftLoop] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(1, 10),
        ],
        [P.HopLeft] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(3, 4),
            Show(4, 8),
            Show(3, 4),
            Show(0, 1),
        ],
        [P.DrainingLeft] =
        [
            Show(5, 5),
            Show(6, 5),
            Show(7, 5),
            Show(6, 48),
            Cue(new(K.Nothing)),
        ],
        [P.DrainingLeftLoop] =
        [
            Show(8, 5),
            Show(9, 5),
            Show(10, 5),
            Show(9, 5),
        ],
        [P.CrawlingRight] =
        [
            Cue(new(K.StopProcessingOffScreen)),
        ],
        [P.CrawlingRightLoop] =
        [
            Show(11, 10),
            Show(12, 10),
            Show(13, 10),
            Show(12, 10),
        ],
        [P.HopRight] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(14, 4),
            Show(15, 8),
            Show(14, 4),
            Show(11, 1),
        ],
        [P.DrainingRight] =
        [
            Show(16, 5),
            Show(17, 5),
            Show(18, 5),
            Show(17, 48),
            Cue(new(K.Nothing)),
        ],
        [P.DrainingRightLoop] =
        [
            Show(19, 5),
            Show(20, 5),
            Show(21, 5),
            Show(20, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BeetomProgram, Animation<BeetomProgram>> StartingAt { get; } = new Dictionary<BeetomProgram, Animation<BeetomProgram>>
    {
        [P.CrawlingLeft] = new([new(P.CrawlingLeft, 0, 1, 1), new(P.CrawlingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingLeftLoop] = new([new(P.CrawlingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HopLeft] = new([new(P.HopLeft, 0, 5, 1)], AnimationEnd.Holds),
        [P.DrainingLeft] = new([new(P.DrainingLeft, 0, 5, 1), new(P.DrainingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.DrainingLeftLoop] = new([new(P.DrainingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CrawlingRight] = new([new(P.CrawlingRight, 0, 1, 1), new(P.CrawlingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingRightLoop] = new([new(P.CrawlingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HopRight] = new([new(P.HopRight, 0, 5, 1)], AnimationEnd.Holds),
        [P.DrainingRight] = new([new(P.DrainingRight, 0, 5, 1), new(P.DrainingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.DrainingRightLoop] = new([new(P.DrainingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BeetomProgram> StartingWith(BeetomProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<BeetomFrame, BeetomCue> Show(int pose, int duration) => new AnimationBeat<BeetomFrame, BeetomCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<BeetomFrame, BeetomCue> Cue(BeetomCue cue) => new AnimationBeat<BeetomFrame, BeetomCue>.Event(cue);
}
