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
            Show(EnemyFrameId.ZebRisingLeftFirst, 2),
            Show(EnemyFrameId.ZebRisingLeftSecond, 2),
            Show(EnemyFrameId.ZebRisingLeftThird, 2),
            Show(EnemyFrameId.ZebRisingLeftFourth, 2),
            Show(EnemyFrameId.ZebRisingLeftFifth, 2),
            Show(EnemyFrameId.ZebRisingLeftFourth, 2),
            Show(EnemyFrameId.ZebRisingLeftThird, 2),
            Show(EnemyFrameId.ZebRisingLeftSecond, 2),
        ],
        [P.ZebShootingLeft] =
        [
            Show(EnemyFrameId.ZebRisingLeftFirst, 1),
            Show(EnemyFrameId.ZebRisingLeftSecond, 1),
            Show(EnemyFrameId.ZebRisingLeftFourth, 1),
            Show(EnemyFrameId.ZebRisingLeftFifth, 1),
            Show(EnemyFrameId.ZebRisingLeftFourth, 1),
            Show(EnemyFrameId.ZebRisingLeftSecond, 1),
        ],
        [P.ZebRisingRight] =
        [
            Show(EnemyFrameId.ZebRisingRightFirst, 2),
            Show(EnemyFrameId.ZebRisingRightSecond, 2),
            Show(EnemyFrameId.ZebRisingRightThird, 2),
            Show(EnemyFrameId.ZebRisingRightFourth, 2),
            Show(EnemyFrameId.ZebRisingRightFifth, 2),
            Show(EnemyFrameId.ZebRisingRightFourth, 2),
            Show(EnemyFrameId.ZebRisingRightThird, 2),
            Show(EnemyFrameId.ZebRisingRightSecond, 2),
        ],
        [P.ZebShootingRight] =
        [
            Show(EnemyFrameId.ZebRisingRightFirst, 1),
            Show(EnemyFrameId.ZebRisingRightSecond, 1),
            Show(EnemyFrameId.ZebRisingRightFourth, 1),
            Show(EnemyFrameId.ZebRisingRightFifth, 1),
            Show(EnemyFrameId.ZebRisingRightFourth, 1),
            Show(EnemyFrameId.ZebRisingRightSecond, 1),
        ],
        [P.ZebboRisingLeft] =
        [
            Show(EnemyFrameId.ZebboRisingLeftFirst, 2),
            Show(EnemyFrameId.ZebboRisingLeftSecond, 1),
            Show(EnemyFrameId.ZebboRisingLeftThird, 2),
            Show(EnemyFrameId.ZebboRisingLeftSecond, 1),
        ],
        [P.ZebboShootingLeft] =
        [
            Show(EnemyFrameId.ZebboShootingLeftFirst, 3),
            Show(EnemyFrameId.ZebboShootingLeftSecond, 3),
            Show(EnemyFrameId.ZebboShootingLeftThird, 3),
            Show(EnemyFrameId.ZebboShootingLeftSecond, 3),
        ],
        [P.ZebboRisingRight] =
        [
            Show(EnemyFrameId.ZebboRisingRightFirst, 2),
            Show(EnemyFrameId.ZebboRisingRightSecond, 1),
            Show(EnemyFrameId.ZebboRisingRightThird, 2),
            Show(EnemyFrameId.ZebboRisingRightSecond, 1),
        ],
        [P.ZebboShootingRight] =
        [
            Show(EnemyFrameId.ZebboShootingRightFirst, 3),
            Show(EnemyFrameId.ZebboShootingRightSecond, 3),
            Show(EnemyFrameId.ZebboShootingRightThird, 3),
            Show(EnemyFrameId.ZebboShootingRightSecond, 3),
        ],
        [P.GametRisingLeft] =
        [
            Show(EnemyFrameId.GametRisingLeftFirst, 2),
            Show(EnemyFrameId.GametRisingLeftSecond, 2),
            Show(EnemyFrameId.GametRisingLeftThird, 2),
            Show(EnemyFrameId.GametRisingLeftFourth, 2),
            Show(EnemyFrameId.GametRisingLeftFifth, 2),
            Show(EnemyFrameId.GametRisingLeftFourth, 2),
            Show(EnemyFrameId.GametRisingLeftThird, 2),
            Show(EnemyFrameId.GametRisingLeftSecond, 2),
        ],
        [P.GametFlyingLeft] =
        [
            Show(EnemyFrameId.GametRisingLeftFirst, 1),
            Show(EnemyFrameId.GametRisingLeftSecond, 1),
            Show(EnemyFrameId.GametRisingLeftFourth, 1),
            Show(EnemyFrameId.GametRisingLeftFifth, 1),
            Show(EnemyFrameId.GametRisingLeftFourth, 1),
            Show(EnemyFrameId.GametRisingLeftSecond, 1),
        ],
        [P.GametRisingRight] =
        [
            Show(EnemyFrameId.GametRisingRightFirst, 2),
            Show(EnemyFrameId.GametRisingRightSecond, 2),
            Show(EnemyFrameId.GametRisingRightThird, 2),
            Show(EnemyFrameId.GametRisingRightFourth, 2),
            Show(EnemyFrameId.GametRisingRightFifth, 2),
            Show(EnemyFrameId.GametRisingRightFourth, 2),
            Show(EnemyFrameId.GametRisingRightThird, 2),
            Show(EnemyFrameId.GametRisingRightSecond, 2),
        ],
        [P.GametFlyingRight] =
        [
            Show(EnemyFrameId.GametRisingRightFirst, 1),
            Show(EnemyFrameId.GametRisingRightSecond, 1),
            Show(EnemyFrameId.GametRisingRightFourth, 1),
            Show(EnemyFrameId.GametRisingRightFifth, 1),
            Show(EnemyFrameId.GametRisingRightFourth, 1),
            Show(EnemyFrameId.GametRisingRightSecond, 1),
        ],
        [P.GeegaFlyingLeft] =
        [
            Show(EnemyFrameId.GeegaFlyingLeftFirst, 4),
            Show(EnemyFrameId.GeegaFlyingLeftSecond, 4),
            Show(EnemyFrameId.GeegaFlyingLeftThird, 4),
            Show(EnemyFrameId.GeegaFlyingLeftSecond, 4),
        ],
        [P.GeegaArcingLeft] =
        [
            Show(EnemyFrameId.GeegaArcingLeftFirst, 1),
            Show(EnemyFrameId.GeegaArcingLeftSecond, 1),
            Show(EnemyFrameId.GeegaArcingLeftThird, 1),
            Show(EnemyFrameId.GeegaArcingLeftSecond, 1),
        ],
        [P.GeegaFlyingRight] =
        [
            Show(EnemyFrameId.GeegaFlyingRightFirst, 4),
            Show(EnemyFrameId.GeegaFlyingRightSecond, 4),
            Show(EnemyFrameId.GeegaFlyingRightThird, 4),
            Show(EnemyFrameId.GeegaFlyingRightSecond, 4),
        ],
        [P.GeegaArcingRight] =
        [
            Show(EnemyFrameId.GeegaArcingRightFirst, 1),
            Show(EnemyFrameId.GeegaArcingRightSecond, 1),
            Show(EnemyFrameId.GeegaArcingRightThird, 1),
            Show(EnemyFrameId.GeegaArcingRightSecond, 1),
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
