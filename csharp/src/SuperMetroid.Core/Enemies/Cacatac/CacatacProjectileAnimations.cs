using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Cacatac.CacatacProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The Cacatac's spike programs, one per launch direction (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class CacatacProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CacatacProjectileProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<CacatacProjectileProgram, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.LeftFacingUp] =
        [
            Show(EnemyProjectileFrameId.CacatacSpikeFrame00, 1),
        ],
        [P.UpLeft] =
        [
            Show(EnemyProjectileFrameId.CacatacSpikeFrame01, 1),
        ],
        [P.Up] =
        [
            Show(EnemyProjectileFrameId.CacatacSpikeFrame02, 1),
        ],
        [P.UpRight] =
        [
            Show(EnemyProjectileFrameId.CacatacSpikeFrame03, 1),
        ],
        [P.RightFacingUp] =
        [
            Show(EnemyProjectileFrameId.CacatacSpikeFrame04, 1),
        ],
        [P.LeftFacingDown] =
        [
            Show(EnemyProjectileFrameId.CacatacSpikeFrame05, 1),
        ],
        [P.DownLeft] =
        [
            Show(EnemyProjectileFrameId.CacatacSpikeFrame06, 1),
        ],
        [P.Down] =
        [
            Show(EnemyProjectileFrameId.CacatacSpikeFrame07, 1),
        ],
        [P.DownRight] =
        [
            Show(EnemyProjectileFrameId.CacatacSpikeFrame08, 1),
        ],
        [P.RightFacingDown] =
        [
            Show(EnemyProjectileFrameId.CacatacSpikeFrame09, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CacatacProjectileProgram, Animation<CacatacProjectileProgram>> StartingAt { get; } = new Dictionary<CacatacProjectileProgram, Animation<CacatacProjectileProgram>>
    {
        [P.LeftFacingUp] = new([new(P.LeftFacingUp, 0, 1, 1)], AnimationEnd.Holds),
        [P.UpLeft] = new([new(P.UpLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.Up] = new([new(P.Up, 0, 1, 1)], AnimationEnd.Holds),
        [P.UpRight] = new([new(P.UpRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.RightFacingUp] = new([new(P.RightFacingUp, 0, 1, 1)], AnimationEnd.Holds),
        [P.LeftFacingDown] = new([new(P.LeftFacingDown, 0, 1, 1)], AnimationEnd.Holds),
        [P.DownLeft] = new([new(P.DownLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.Down] = new([new(P.Down, 0, 1, 1)], AnimationEnd.Holds),
        [P.DownRight] = new([new(P.DownRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.RightFacingDown] = new([new(P.RightFacingDown, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CacatacProjectileProgram> StartingWith(CacatacProjectileProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);
}
