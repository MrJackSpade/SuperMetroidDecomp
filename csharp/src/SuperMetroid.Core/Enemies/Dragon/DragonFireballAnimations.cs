using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Dragon.DragonFireballSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Dragon's rising and falling fireball animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static class DragonFireballAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DragonFireballSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<DragonFireballSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.RisingLeft] =
        [
            Show(EnemyProjectileFrameId.DragonFireballRisingLeftFlaring, 5),
            Show(EnemyProjectileFrameId.DragonFireballRisingLeftCompact, 5),
        ],
        [P.RisingRight] =
        [
            Show(EnemyProjectileFrameId.DragonFireballRisingRightFlaring, 5),
            Show(EnemyProjectileFrameId.DragonFireballRisingRightCompact, 5),
        ],
        [P.FallingLeft] =
        [
            Show(EnemyProjectileFrameId.DragonFireballFallingLeftFlaring, 5),
            Show(EnemyProjectileFrameId.DragonFireballFallingLeftCompact, 5),
        ],
        [P.FallingRight] =
        [
            Show(EnemyProjectileFrameId.DragonFireballFallingRightFlaring, 5),
            Show(EnemyProjectileFrameId.DragonFireballFallingRightCompact, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DragonFireballSegment, Animation<DragonFireballSegment>> StartingAt { get; } = new Dictionary<DragonFireballSegment, Animation<DragonFireballSegment>>
    {
        [P.RisingLeft] = new([Segments.Whole(P.RisingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.RisingRight] = new([Segments.Whole(P.RisingRight)], AnimationEnd.LoopsFrom(0)),
        [P.FallingLeft] = new([Segments.Whole(P.FallingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.FallingRight] = new([Segments.Whole(P.FallingRight)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DragonFireballSegment> StartingWith(DragonFireballSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);
}
