using P = SuperMetroid.Core.Enemies.PipeBugs.PipeBugSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// The Zeb, Zebbo, Gamet and Geega Pipe Bugs' rising, shooting, flying and arcing loops (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class PipeBugAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PipeBugSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<PipeBugSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.ZebRisingLeft] =
        [
            Show(EnemyFrameId.ZebFacingLeftWingsUp, 2),
            Show(EnemyFrameId.ZebFacingLeftWingsRaised, 2),
            Show(EnemyFrameId.ZebFacingLeftWingsLevel, 2),
            Show(EnemyFrameId.ZebFacingLeftWingsLowered, 2),
            Show(EnemyFrameId.ZebFacingLeftWingsDown, 2),
            Show(EnemyFrameId.ZebFacingLeftWingsLowered, 2),
            Show(EnemyFrameId.ZebFacingLeftWingsLevel, 2),
            Show(EnemyFrameId.ZebFacingLeftWingsRaised, 2),
        ],
        [P.ZebShootingLeft] =
        [
            Show(EnemyFrameId.ZebFacingLeftWingsUp, 1),
            Show(EnemyFrameId.ZebFacingLeftWingsRaised, 1),
            Show(EnemyFrameId.ZebFacingLeftWingsLowered, 1),
            Show(EnemyFrameId.ZebFacingLeftWingsDown, 1),
            Show(EnemyFrameId.ZebFacingLeftWingsLowered, 1),
            Show(EnemyFrameId.ZebFacingLeftWingsRaised, 1),
        ],
        [P.ZebRisingRight] =
        [
            Show(EnemyFrameId.ZebFacingRightWingsUp, 2),
            Show(EnemyFrameId.ZebFacingRightWingsRaised, 2),
            Show(EnemyFrameId.ZebFacingRightWingsLevel, 2),
            Show(EnemyFrameId.ZebFacingRightWingsLowered, 2),
            Show(EnemyFrameId.ZebFacingRightWingsDown, 2),
            Show(EnemyFrameId.ZebFacingRightWingsLowered, 2),
            Show(EnemyFrameId.ZebFacingRightWingsLevel, 2),
            Show(EnemyFrameId.ZebFacingRightWingsRaised, 2),
        ],
        [P.ZebShootingRight] =
        [
            Show(EnemyFrameId.ZebFacingRightWingsUp, 1),
            Show(EnemyFrameId.ZebFacingRightWingsRaised, 1),
            Show(EnemyFrameId.ZebFacingRightWingsLowered, 1),
            Show(EnemyFrameId.ZebFacingRightWingsDown, 1),
            Show(EnemyFrameId.ZebFacingRightWingsLowered, 1),
            Show(EnemyFrameId.ZebFacingRightWingsRaised, 1),
        ],
        [P.ZebboRisingLeft] =
        [
            Show(EnemyFrameId.ZebboRisingFacingLeftWingsUp, 2),
            Show(EnemyFrameId.ZebboRisingFacingLeftWingsLevel, 1),
            Show(EnemyFrameId.ZebboRisingFacingLeftWingsDown, 2),
            Show(EnemyFrameId.ZebboRisingFacingLeftWingsLevel, 1),
        ],
        [P.ZebboShootingLeft] =
        [
            Show(EnemyFrameId.ZebboDartingFacingLeftWingsUp, 3),
            Show(EnemyFrameId.ZebboDartingFacingLeftWingsLevel, 3),
            Show(EnemyFrameId.ZebboDartingFacingLeftWingsDown, 3),
            Show(EnemyFrameId.ZebboDartingFacingLeftWingsLevel, 3),
        ],
        [P.ZebboRisingRight] =
        [
            Show(EnemyFrameId.ZebboRisingFacingRightWingsUp, 2),
            Show(EnemyFrameId.ZebboRisingFacingRightWingsLevel, 1),
            Show(EnemyFrameId.ZebboRisingFacingRightWingsDown, 2),
            Show(EnemyFrameId.ZebboRisingFacingRightWingsLevel, 1),
        ],
        [P.ZebboShootingRight] =
        [
            Show(EnemyFrameId.ZebboDartingFacingRightWingsUp, 3),
            Show(EnemyFrameId.ZebboDartingFacingRightWingsLevel, 3),
            Show(EnemyFrameId.ZebboDartingFacingRightWingsDown, 3),
            Show(EnemyFrameId.ZebboDartingFacingRightWingsLevel, 3),
        ],
        [P.GametRisingLeft] =
        [
            Show(EnemyFrameId.GametFacingLeftWingsUp, 2),
            Show(EnemyFrameId.GametFacingLeftWingsRaised, 2),
            Show(EnemyFrameId.GametFacingLeftWingsLevel, 2),
            Show(EnemyFrameId.GametFacingLeftWingsLowered, 2),
            Show(EnemyFrameId.GametFacingLeftWingsDown, 2),
            Show(EnemyFrameId.GametFacingLeftWingsLowered, 2),
            Show(EnemyFrameId.GametFacingLeftWingsLevel, 2),
            Show(EnemyFrameId.GametFacingLeftWingsRaised, 2),
        ],
        [P.GametFlyingLeft] =
        [
            Show(EnemyFrameId.GametFacingLeftWingsUp, 1),
            Show(EnemyFrameId.GametFacingLeftWingsRaised, 1),
            Show(EnemyFrameId.GametFacingLeftWingsLowered, 1),
            Show(EnemyFrameId.GametFacingLeftWingsDown, 1),
            Show(EnemyFrameId.GametFacingLeftWingsLowered, 1),
            Show(EnemyFrameId.GametFacingLeftWingsRaised, 1),
        ],
        [P.GametRisingRight] =
        [
            Show(EnemyFrameId.GametFacingRightWingsUp, 2),
            Show(EnemyFrameId.GametFacingRightWingsRaised, 2),
            Show(EnemyFrameId.GametFacingRightWingsLevel, 2),
            Show(EnemyFrameId.GametFacingRightWingsLowered, 2),
            Show(EnemyFrameId.GametFacingRightWingsDown, 2),
            Show(EnemyFrameId.GametFacingRightWingsLowered, 2),
            Show(EnemyFrameId.GametFacingRightWingsLevel, 2),
            Show(EnemyFrameId.GametFacingRightWingsRaised, 2),
        ],
        [P.GametFlyingRight] =
        [
            Show(EnemyFrameId.GametFacingRightWingsUp, 1),
            Show(EnemyFrameId.GametFacingRightWingsRaised, 1),
            Show(EnemyFrameId.GametFacingRightWingsLowered, 1),
            Show(EnemyFrameId.GametFacingRightWingsDown, 1),
            Show(EnemyFrameId.GametFacingRightWingsLowered, 1),
            Show(EnemyFrameId.GametFacingRightWingsRaised, 1),
        ],
        [P.GeegaFlyingLeft] =
        [
            Show(EnemyFrameId.GeegaLevelFacingLeftWingsUp, 4),
            Show(EnemyFrameId.GeegaLevelFacingLeftWingsLevel, 4),
            Show(EnemyFrameId.GeegaLevelFacingLeftWingsDown, 4),
            Show(EnemyFrameId.GeegaLevelFacingLeftWingsLevel, 4),
        ],
        [P.GeegaArcingLeft] =
        [
            Show(EnemyFrameId.GeegaTiltedFacingLeftWingsUp, 1),
            Show(EnemyFrameId.GeegaTiltedFacingLeftWingsLevel, 1),
            Show(EnemyFrameId.GeegaTiltedFacingLeftWingsDown, 1),
            Show(EnemyFrameId.GeegaTiltedFacingLeftWingsLevel, 1),
        ],
        [P.GeegaFlyingRight] =
        [
            Show(EnemyFrameId.GeegaLevelFacingRightWingsUp, 4),
            Show(EnemyFrameId.GeegaLevelFacingRightWingsLevel, 4),
            Show(EnemyFrameId.GeegaLevelFacingRightWingsDown, 4),
            Show(EnemyFrameId.GeegaLevelFacingRightWingsLevel, 4),
        ],
        [P.GeegaArcingRight] =
        [
            Show(EnemyFrameId.GeegaTiltedFacingRightWingsUp, 1),
            Show(EnemyFrameId.GeegaTiltedFacingRightWingsLevel, 1),
            Show(EnemyFrameId.GeegaTiltedFacingRightWingsDown, 1),
            Show(EnemyFrameId.GeegaTiltedFacingRightWingsLevel, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PipeBugSegment, Animation<PipeBugSegment>> StartingAt { get; } = new Dictionary<PipeBugSegment, Animation<PipeBugSegment>>
    {
        [P.ZebRisingLeft] = new([Segments.Whole(P.ZebRisingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.ZebShootingLeft] = new([Segments.Whole(P.ZebShootingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.ZebRisingRight] = new([Segments.Whole(P.ZebRisingRight)], AnimationEnd.LoopsFrom(0)),
        [P.ZebShootingRight] = new([Segments.Whole(P.ZebShootingRight)], AnimationEnd.LoopsFrom(0)),
        [P.ZebboRisingLeft] = new([Segments.Whole(P.ZebboRisingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.ZebboShootingLeft] = new([Segments.Whole(P.ZebboShootingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.ZebboRisingRight] = new([Segments.Whole(P.ZebboRisingRight)], AnimationEnd.LoopsFrom(0)),
        [P.ZebboShootingRight] = new([Segments.Whole(P.ZebboShootingRight)], AnimationEnd.LoopsFrom(0)),
        [P.GametRisingLeft] = new([Segments.Whole(P.GametRisingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.GametFlyingLeft] = new([Segments.Whole(P.GametFlyingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.GametRisingRight] = new([Segments.Whole(P.GametRisingRight)], AnimationEnd.LoopsFrom(0)),
        [P.GametFlyingRight] = new([Segments.Whole(P.GametFlyingRight)], AnimationEnd.LoopsFrom(0)),
        [P.GeegaFlyingLeft] = new([Segments.Whole(P.GeegaFlyingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.GeegaArcingLeft] = new([Segments.Whole(P.GeegaArcingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.GeegaFlyingRight] = new([Segments.Whole(P.GeegaFlyingRight)], AnimationEnd.LoopsFrom(0)),
        [P.GeegaArcingRight] = new([Segments.Whole(P.GeegaArcingRight)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PipeBugSegment> StartingWith(PipeBugSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
