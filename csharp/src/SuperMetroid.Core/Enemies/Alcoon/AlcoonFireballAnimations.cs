using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Alcoon.AlcoonFireballProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// Alcoon's fireball program (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class AlcoonFireballAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<AlcoonFireballProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<AlcoonFireballProgram, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Initial] =
        [
            Show(EnemyProjectileFrameId.AlcoonFireballInitialFirst, 3),
            Show(EnemyProjectileFrameId.AlcoonFireballInitialSecond, 3),
            Show(EnemyProjectileFrameId.AlcoonFireballInitialThird, 3),
            Show(EnemyProjectileFrameId.AlcoonFireballInitialFourth, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<AlcoonFireballProgram, Animation<AlcoonFireballProgram>> StartingAt { get; } = new Dictionary<AlcoonFireballProgram, Animation<AlcoonFireballProgram>>
    {
        [P.Initial] = new([new(P.Initial, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<AlcoonFireballProgram> StartingWith(AlcoonFireballProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);
}
