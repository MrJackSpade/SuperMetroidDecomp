using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheFireballProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// Fune and Namihe's fireball programs (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class FuneNamiheFireballAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FuneNamiheFireballProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<FuneNamiheFireballProgram, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Left] =
        [
            Show(EnemyProjectileFrameId.FuneNamiheFireballLeftFirst, 5),
            Show(EnemyProjectileFrameId.FuneNamiheFireballLeftSecond, 5),
            Show(EnemyProjectileFrameId.FuneNamiheFireballLeftThird, 5),
        ],
        [P.Right] =
        [
            Show(EnemyProjectileFrameId.FuneNamiheFireballRightFirst, 5),
            Show(EnemyProjectileFrameId.FuneNamiheFireballRightSecond, 5),
            Show(EnemyProjectileFrameId.FuneNamiheFireballRightThird, 5),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);
}
