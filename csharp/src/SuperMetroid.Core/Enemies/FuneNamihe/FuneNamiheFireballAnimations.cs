using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheFireballProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// Fune and Namihe's fireball programs (#641). Each
/// frame shows a <see cref="FuneNamiheFireballFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class FuneNamiheFireballAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FuneNamiheFireballProgram, FuneNamiheFireballFrame, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<FuneNamiheFireballProgram, AnimationBeat<FuneNamiheFireballFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Left] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
        ],
        [P.Right] =
        [
            Show(3, 5),
            Show(4, 5),
            Show(5, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FuneNamiheFireballProgram, Animation<FuneNamiheFireballProgram>> StartingAt { get; } = new Dictionary<FuneNamiheFireballProgram, Animation<FuneNamiheFireballProgram>>
    {
        [P.Left] = new([new(P.Left, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Right] = new([new(P.Right, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FuneNamiheFireballProgram> StartingWith(FuneNamiheFireballProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<FuneNamiheFireballFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => new AnimationBeat<FuneNamiheFireballFrame, EnemyProjectileCommonCue>.Frame(new(pose), duration);
}
