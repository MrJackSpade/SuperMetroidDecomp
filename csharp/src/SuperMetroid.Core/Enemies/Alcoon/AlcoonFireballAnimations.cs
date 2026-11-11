using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Alcoon.AlcoonFireballProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// Alcoon's fireball program (#641). Each
/// frame shows a <see cref="AlcoonFireballFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class AlcoonFireballAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<AlcoonFireballProgram, AlcoonFireballFrame, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<AlcoonFireballProgram, AnimationBeat<AlcoonFireballFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Initial] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(3, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<AlcoonFireballProgram, Animation<AlcoonFireballProgram>> StartingAt { get; } = new Dictionary<AlcoonFireballProgram, Animation<AlcoonFireballProgram>>
    {
        [P.Initial] = new([new(P.Initial, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<AlcoonFireballProgram> StartingWith(AlcoonFireballProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<AlcoonFireballFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => new AnimationBeat<AlcoonFireballFrame, EnemyProjectileCommonCue>.Frame(new(pose), duration);
}
