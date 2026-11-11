using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Polyp.PolypRockProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// The Polyp's lava-rock program (#641). Each
/// frame shows a <see cref="PolypRockFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class PolypRockAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PolypRockProgram, PolypRockFrame, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<PolypRockProgram, AnimationBeat<PolypRockFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Initial] =
        [
            Show(0, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PolypRockProgram, Animation<PolypRockProgram>> StartingAt { get; } = new Dictionary<PolypRockProgram, Animation<PolypRockProgram>>
    {
        [P.Initial] = new([new(P.Initial, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PolypRockProgram> StartingWith(PolypRockProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<PolypRockFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => new AnimationBeat<PolypRockFrame, EnemyProjectileCommonCue>.Frame(new(pose), duration);
}
