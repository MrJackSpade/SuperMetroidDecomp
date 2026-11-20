using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Enemies.SpacePirates.SpacePirateLaserSegment;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The Pirate/Mother Brain laser's animations (#641, #2554).</summary>
internal static class SpacePirateLaserAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SpacePirateLaserSegment, EnemyProjectileFrameId, SpacePirateLaserCue> Segments { get; } = new(
        new Dictionary<SpacePirateLaserSegment, AnimationBeat<EnemyProjectileFrameId, SpacePirateLaserCue>[]>
    {
        [P.LaserLeft] =
        [
            Show(EnemyProjectileFrameId.SpacePirateLaserFlareGlint, 2),
            Show(EnemyProjectileFrameId.SpacePirateLaserFlareGlow, 2),
            Show(EnemyProjectileFrameId.SpacePirateLaserFlareBall, 2),
            Cue(HorizontalDirection.Left),
            Show(EnemyProjectileFrameId.SpacePirateLaserEmergingBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserEmergingDim, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserExtendingBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserExtendingDim, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLengtheningBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLengtheningDim, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthDim, 1),
        ],
        [P.LaserLeftFlicker] =
        [
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthDim, 1),
        ],
        [P.LaserRight] =
        [
            Show(EnemyProjectileFrameId.SpacePirateLaserFlareGlint, 2),
            Show(EnemyProjectileFrameId.SpacePirateLaserFlareGlow, 2),
            Show(EnemyProjectileFrameId.SpacePirateLaserFlareBall, 2),
            Cue(HorizontalDirection.Right),
            Show(EnemyProjectileFrameId.SpacePirateLaserEmergingBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserEmergingDim, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserExtendingBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserExtendingDim, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLengtheningBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserLengtheningDim, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthDim, 1),
        ],
        [P.LaserRightFlicker] =
        [
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthBright, 1),
            Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthDim, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start: a laser's muzzle and flight, each played whole, then its flicker looping.</summary>
    internal static IReadOnlyDictionary<SpacePirateLaserSegment, Animation<SpacePirateLaserSegment>> StartingAt { get; } = new Dictionary<SpacePirateLaserSegment, Animation<SpacePirateLaserSegment>>
    {
        [P.LaserLeft] = Animation<SpacePirateLaserSegment>.LeadingInto([Segments.Whole(P.LaserLeft)], [Segments.Whole(P.LaserLeftFlicker)]),
        [P.LaserRight] = Animation<SpacePirateLaserSegment>.LeadingInto([Segments.Whole(P.LaserRight)], [Segments.Whole(P.LaserRightFlicker)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SpacePirateLaserSegment> StartingWith(SpacePirateLaserSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SpacePirateLaserCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, SpacePirateLaserCue>.Frame(frame, duration);

    /// <summary>Publishes the cue that fires the laser toward <paramref name="direction"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SpacePirateLaserCue> Cue(HorizontalDirection direction) =>
        new AnimationBeat<EnemyProjectileFrameId, SpacePirateLaserCue>.Event(new(direction));
}
