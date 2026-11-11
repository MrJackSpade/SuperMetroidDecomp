using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Dragon.DragonFireballProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Dragon's rising and falling fireball programs (#641). Each
/// frame shows a <see cref="DragonFireballFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class DragonFireballAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DragonFireballProgram, DragonFireballFrame, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<DragonFireballProgram, AnimationBeat<DragonFireballFrame, EnemyProjectileCommonCue>[]>
    {
        [P.RisingLeft] =
        [
            Show(0, 5),
            Show(1, 5),
        ],
        [P.RisingRight] =
        [
            Show(2, 5),
            Show(3, 5),
        ],
        [P.FallingLeft] =
        [
            Show(4, 5),
            Show(5, 5),
        ],
        [P.FallingRight] =
        [
            Show(6, 5),
            Show(7, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DragonFireballProgram, Animation<DragonFireballProgram>> StartingAt { get; } = new Dictionary<DragonFireballProgram, Animation<DragonFireballProgram>>
    {
        [P.RisingLeft] = new([new(P.RisingLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RisingRight] = new([new(P.RisingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FallingLeft] = new([new(P.FallingLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FallingRight] = new([new(P.FallingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DragonFireballProgram> StartingWith(DragonFireballProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<DragonFireballFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => new AnimationBeat<DragonFireballFrame, EnemyProjectileCommonCue>.Frame(new(pose), duration);
}
