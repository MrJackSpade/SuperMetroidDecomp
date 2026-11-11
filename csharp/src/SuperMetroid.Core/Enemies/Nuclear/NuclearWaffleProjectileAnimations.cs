using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Nuclear.NuclearWaffleProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// The Nuclear Waffle's body program (#641). Each
/// frame shows a <see cref="NuclearWaffleProjectileFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class NuclearWaffleProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NuclearWaffleProjectileProgram, NuclearWaffleProjectileFrame, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<NuclearWaffleProjectileProgram, AnimationBeat<NuclearWaffleProjectileFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Initial] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(6, 3),
            Show(7, 3),
            Show(8, 3),
            Show(9, 3),
            Show(10, 3),
            Show(11, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NuclearWaffleProjectileProgram, Animation<NuclearWaffleProjectileProgram>> StartingAt { get; } = new Dictionary<NuclearWaffleProjectileProgram, Animation<NuclearWaffleProjectileProgram>>
    {
        [P.Initial] = new([new(P.Initial, 0, 12, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NuclearWaffleProjectileProgram> StartingWith(NuclearWaffleProjectileProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<NuclearWaffleProjectileFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => new AnimationBeat<NuclearWaffleProjectileFrame, EnemyProjectileCommonCue>.Frame(new(pose), duration);
}
