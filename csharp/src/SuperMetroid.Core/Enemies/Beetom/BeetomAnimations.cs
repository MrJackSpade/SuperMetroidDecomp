using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Beetom.BeetomCueKind;
using P = SuperMetroid.Core.Enemies.Beetom.BeetomProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The Beetom's crawl, hop and drain animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="BeetomCue"/> with its typed operands.
/// </summary>
internal static class BeetomAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BeetomProgram, EnemyFrameId, BeetomCue> Segments { get; } = new(
        new Dictionary<BeetomProgram, AnimationBeat<EnemyFrameId, BeetomCue>[]>
    {
        [P.CrawlingLeft] =
        [
            Cue(new(K.StopProcessingOffScreen)),
        ],
        [P.CrawlingLeftLoop] =
        [
            Show(EnemyFrameId.BeetomLeftCrawl0, 10),
            Show(EnemyFrameId.BeetomLeftCrawl1, 10),
            Show(EnemyFrameId.BeetomLeftCrawl2, 10),
            Show(EnemyFrameId.BeetomLeftCrawl1, 10),
        ],
        [P.HopLeft] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.BeetomLeftHop0, 4),
            Show(EnemyFrameId.BeetomLeftHop1, 8),
            Show(EnemyFrameId.BeetomLeftHop0, 4),
            Show(EnemyFrameId.BeetomLeftCrawl0, 1),
        ],
        [P.DrainingLeft] =
        [
            Show(EnemyFrameId.BeetomLeftDrain0, 5),
            Show(EnemyFrameId.BeetomLeftDrain1, 5),
            Show(EnemyFrameId.BeetomLeftDrain2, 5),
            Show(EnemyFrameId.BeetomLeftDrain1, 48),
            Cue(new(K.Nothing)),
        ],
        [P.DrainingLeftLoop] =
        [
            Show(EnemyFrameId.BeetomLeftDrain3, 5),
            Show(EnemyFrameId.BeetomLeftDrain4, 5),
            Show(EnemyFrameId.BeetomLeftDrain5, 5),
            Show(EnemyFrameId.BeetomLeftDrain4, 5),
        ],
        [P.CrawlingRight] =
        [
            Cue(new(K.StopProcessingOffScreen)),
        ],
        [P.CrawlingRightLoop] =
        [
            Show(EnemyFrameId.BeetomRightCrawl0, 10),
            Show(EnemyFrameId.BeetomRightCrawl1, 10),
            Show(EnemyFrameId.BeetomRightCrawl2, 10),
            Show(EnemyFrameId.BeetomRightCrawl1, 10),
        ],
        [P.HopRight] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.BeetomRightHop0, 4),
            Show(EnemyFrameId.BeetomRightHop1, 8),
            Show(EnemyFrameId.BeetomRightHop0, 4),
            Show(EnemyFrameId.BeetomRightCrawl0, 1),
        ],
        [P.DrainingRight] =
        [
            Show(EnemyFrameId.BeetomRightDrain0, 5),
            Show(EnemyFrameId.BeetomRightDrain1, 5),
            Show(EnemyFrameId.BeetomRightDrain2, 5),
            Show(EnemyFrameId.BeetomRightDrain1, 48),
            Cue(new(K.Nothing)),
        ],
        [P.DrainingRightLoop] =
        [
            Show(EnemyFrameId.BeetomRightDrain3, 5),
            Show(EnemyFrameId.BeetomRightDrain4, 5),
            Show(EnemyFrameId.BeetomRightDrain5, 5),
            Show(EnemyFrameId.BeetomRightDrain4, 5),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, BeetomCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, BeetomCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, BeetomCue> Cue(BeetomCue cue) => new AnimationBeat<EnemyFrameId, BeetomCue>.Event(cue);
}
