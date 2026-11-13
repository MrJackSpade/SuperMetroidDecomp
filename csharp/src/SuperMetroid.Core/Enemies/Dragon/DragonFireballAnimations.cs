using SuperMetroid.Core.Programs;
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
            Show(EnemyProjectileFrameId.DragonFireballRisingLeftFirst, 5),
            Show(EnemyProjectileFrameId.DragonFireballRisingLeftSecond, 5),
        ],
        [P.RisingRight] =
        [
            Show(EnemyProjectileFrameId.DragonFireballRisingRightFirst, 5),
            Show(EnemyProjectileFrameId.DragonFireballRisingRightSecond, 5),
        ],
        [P.FallingLeft] =
        [
            Show(EnemyProjectileFrameId.DragonFireballFallingLeftFirst, 5),
            Show(EnemyProjectileFrameId.DragonFireballFallingLeftSecond, 5),
        ],
        [P.FallingRight] =
        [
            Show(EnemyProjectileFrameId.DragonFireballFallingRightFirst, 5),
            Show(EnemyProjectileFrameId.DragonFireballFallingRightSecond, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DragonFireballSegment, Animation<DragonFireballSegment>> StartingAt { get; } = new Dictionary<DragonFireballSegment, Animation<DragonFireballSegment>>
    {
        [P.RisingLeft] = new([new(P.RisingLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RisingRight] = new([new(P.RisingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FallingLeft] = new([new(P.FallingLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FallingRight] = new([new(P.FallingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DragonFireballSegment> StartingWith(DragonFireballSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);
}
