using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Stoke.StokeProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// The Stoke's projectile program (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class StokeProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<StokeProjectileProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<StokeProjectileProgram, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Initial] =
        [
            Show(EnemyProjectileFrameId.StokeInitialFirst, 16),
            Show(EnemyProjectileFrameId.StokeInitialSecond, 16),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<StokeProjectileProgram, Animation<StokeProjectileProgram>> StartingAt { get; } = new Dictionary<StokeProjectileProgram, Animation<StokeProjectileProgram>>
    {
        [P.Initial] = new([new(P.Initial, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<StokeProjectileProgram> StartingWith(StokeProjectileProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);
}
