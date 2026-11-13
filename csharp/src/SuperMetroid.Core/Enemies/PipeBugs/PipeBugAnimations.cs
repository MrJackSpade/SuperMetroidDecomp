using P = SuperMetroid.Core.Enemies.PipeBugs.PipeBugProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// The Zeb, Zebbo, Gamet and Geega Pipe Bugs' rising, shooting, flying and arcing loops as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class PipeBugAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PipeBugProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<PipeBugProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.ZebRisingLeft] =
        [
            Show(EnemyFrameId.PipeBrinstarNormalLeft0, 2),
            Show(EnemyFrameId.PipeBrinstarNormalLeft1, 2),
            Show(EnemyFrameId.PipeBrinstarNormalLeft2, 2),
            Show(EnemyFrameId.PipeBrinstarNormalLeft3, 2),
            Show(EnemyFrameId.PipeBrinstarNormalLeft4, 2),
            Show(EnemyFrameId.PipeBrinstarNormalLeft3, 2),
            Show(EnemyFrameId.PipeBrinstarNormalLeft2, 2),
            Show(EnemyFrameId.PipeBrinstarNormalLeft1, 2),
        ],
        [P.ZebShootingLeft] =
        [
            Show(EnemyFrameId.PipeBrinstarNormalLeft0, 1),
            Show(EnemyFrameId.PipeBrinstarNormalLeft1, 1),
            Show(EnemyFrameId.PipeBrinstarNormalLeft3, 1),
            Show(EnemyFrameId.PipeBrinstarNormalLeft4, 1),
            Show(EnemyFrameId.PipeBrinstarNormalLeft3, 1),
            Show(EnemyFrameId.PipeBrinstarNormalLeft1, 1),
        ],
        [P.ZebRisingRight] =
        [
            Show(EnemyFrameId.PipeBrinstarNormalRight0, 2),
            Show(EnemyFrameId.PipeBrinstarNormalRight1, 2),
            Show(EnemyFrameId.PipeBrinstarNormalRight2, 2),
            Show(EnemyFrameId.PipeBrinstarNormalRight3, 2),
            Show(EnemyFrameId.PipeBrinstarNormalRight4, 2),
            Show(EnemyFrameId.PipeBrinstarNormalRight3, 2),
            Show(EnemyFrameId.PipeBrinstarNormalRight2, 2),
            Show(EnemyFrameId.PipeBrinstarNormalRight1, 2),
        ],
        [P.ZebShootingRight] =
        [
            Show(EnemyFrameId.PipeBrinstarNormalRight0, 1),
            Show(EnemyFrameId.PipeBrinstarNormalRight1, 1),
            Show(EnemyFrameId.PipeBrinstarNormalRight3, 1),
            Show(EnemyFrameId.PipeBrinstarNormalRight4, 1),
            Show(EnemyFrameId.PipeBrinstarNormalRight3, 1),
            Show(EnemyFrameId.PipeBrinstarNormalRight1, 1),
        ],
        [P.ZebboRisingLeft] =
        [
            Show(EnemyFrameId.PipeBrinstarStrongRiseLeft0, 2),
            Show(EnemyFrameId.PipeBrinstarStrongRiseLeft1, 1),
            Show(EnemyFrameId.PipeBrinstarStrongRiseLeft2, 2),
            Show(EnemyFrameId.PipeBrinstarStrongRiseLeft1, 1),
        ],
        [P.ZebboShootingLeft] =
        [
            Show(EnemyFrameId.PipeBrinstarStrongShootLeft0, 3),
            Show(EnemyFrameId.PipeBrinstarStrongShootLeft1, 3),
            Show(EnemyFrameId.PipeBrinstarStrongShootLeft2, 3),
            Show(EnemyFrameId.PipeBrinstarStrongShootLeft1, 3),
        ],
        [P.ZebboRisingRight] =
        [
            Show(EnemyFrameId.PipeBrinstarStrongRiseRight0, 2),
            Show(EnemyFrameId.PipeBrinstarStrongRiseRight1, 1),
            Show(EnemyFrameId.PipeBrinstarStrongRiseRight2, 2),
            Show(EnemyFrameId.PipeBrinstarStrongRiseRight1, 1),
        ],
        [P.ZebboShootingRight] =
        [
            Show(EnemyFrameId.PipeBrinstarStrongShootRight0, 3),
            Show(EnemyFrameId.PipeBrinstarStrongShootRight1, 3),
            Show(EnemyFrameId.PipeBrinstarStrongShootRight2, 3),
            Show(EnemyFrameId.PipeBrinstarStrongShootRight1, 3),
        ],
        [P.GametRisingLeft] =
        [
            Show(EnemyFrameId.PipeNorfairLeft0, 2),
            Show(EnemyFrameId.PipeNorfairLeft1, 2),
            Show(EnemyFrameId.PipeNorfairLeft2, 2),
            Show(EnemyFrameId.PipeNorfairLeft3, 2),
            Show(EnemyFrameId.PipeNorfairLeft4, 2),
            Show(EnemyFrameId.PipeNorfairLeft3, 2),
            Show(EnemyFrameId.PipeNorfairLeft2, 2),
            Show(EnemyFrameId.PipeNorfairLeft1, 2),
        ],
        [P.GametFlyingLeft] =
        [
            Show(EnemyFrameId.PipeNorfairLeft0, 1),
            Show(EnemyFrameId.PipeNorfairLeft1, 1),
            Show(EnemyFrameId.PipeNorfairLeft3, 1),
            Show(EnemyFrameId.PipeNorfairLeft4, 1),
            Show(EnemyFrameId.PipeNorfairLeft3, 1),
            Show(EnemyFrameId.PipeNorfairLeft1, 1),
        ],
        [P.GametRisingRight] =
        [
            Show(EnemyFrameId.PipeNorfairRight0, 2),
            Show(EnemyFrameId.PipeNorfairRight1, 2),
            Show(EnemyFrameId.PipeNorfairRight2, 2),
            Show(EnemyFrameId.PipeNorfairRight3, 2),
            Show(EnemyFrameId.PipeNorfairRight4, 2),
            Show(EnemyFrameId.PipeNorfairRight3, 2),
            Show(EnemyFrameId.PipeNorfairRight2, 2),
            Show(EnemyFrameId.PipeNorfairRight1, 2),
        ],
        [P.GametFlyingRight] =
        [
            Show(EnemyFrameId.PipeNorfairRight0, 1),
            Show(EnemyFrameId.PipeNorfairRight1, 1),
            Show(EnemyFrameId.PipeNorfairRight3, 1),
            Show(EnemyFrameId.PipeNorfairRight4, 1),
            Show(EnemyFrameId.PipeNorfairRight3, 1),
            Show(EnemyFrameId.PipeNorfairRight1, 1),
        ],
        [P.GeegaFlyingLeft] =
        [
            Show(EnemyFrameId.PipeYellowFlyLeft0, 4),
            Show(EnemyFrameId.PipeYellowFlyLeft1, 4),
            Show(EnemyFrameId.PipeYellowFlyLeft2, 4),
            Show(EnemyFrameId.PipeYellowFlyLeft1, 4),
        ],
        [P.GeegaArcingLeft] =
        [
            Show(EnemyFrameId.PipeYellowArcLeft0, 1),
            Show(EnemyFrameId.PipeYellowArcLeft1, 1),
            Show(EnemyFrameId.PipeYellowArcLeft2, 1),
            Show(EnemyFrameId.PipeYellowArcLeft1, 1),
        ],
        [P.GeegaFlyingRight] =
        [
            Show(EnemyFrameId.PipeYellowFlyRight0, 4),
            Show(EnemyFrameId.PipeYellowFlyRight1, 4),
            Show(EnemyFrameId.PipeYellowFlyRight2, 4),
            Show(EnemyFrameId.PipeYellowFlyRight1, 4),
        ],
        [P.GeegaArcingRight] =
        [
            Show(EnemyFrameId.PipeYellowArcRight0, 1),
            Show(EnemyFrameId.PipeYellowArcRight1, 1),
            Show(EnemyFrameId.PipeYellowArcRight2, 1),
            Show(EnemyFrameId.PipeYellowArcRight1, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PipeBugProgram, Animation<PipeBugProgram>> StartingAt { get; } = new Dictionary<PipeBugProgram, Animation<PipeBugProgram>>
    {
        [P.ZebRisingLeft] = new([new(P.ZebRisingLeft, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebShootingLeft] = new([new(P.ZebShootingLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebRisingRight] = new([new(P.ZebRisingRight, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebShootingRight] = new([new(P.ZebShootingRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebboRisingLeft] = new([new(P.ZebboRisingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebboShootingLeft] = new([new(P.ZebboShootingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebboRisingRight] = new([new(P.ZebboRisingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebboShootingRight] = new([new(P.ZebboShootingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GametRisingLeft] = new([new(P.GametRisingLeft, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GametFlyingLeft] = new([new(P.GametFlyingLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GametRisingRight] = new([new(P.GametRisingRight, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GametFlyingRight] = new([new(P.GametFlyingRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GeegaFlyingLeft] = new([new(P.GeegaFlyingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GeegaArcingLeft] = new([new(P.GeegaArcingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GeegaFlyingRight] = new([new(P.GeegaFlyingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GeegaArcingRight] = new([new(P.GeegaArcingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PipeBugProgram> StartingWith(PipeBugProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
