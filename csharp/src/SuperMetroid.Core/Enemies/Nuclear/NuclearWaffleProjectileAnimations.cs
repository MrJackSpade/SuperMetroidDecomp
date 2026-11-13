using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Nuclear.NuclearWaffleProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// The Nuclear Waffle's body program (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class NuclearWaffleProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NuclearWaffleProjectileProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<NuclearWaffleProjectileProgram, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Initial] =
        [
            Show(EnemyProjectileFrameId.NuclearWaffleInitialFirst, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialSecond, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialThird, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialFourth, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialFifth, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialSixth, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialSeventh, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialEighth, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialNinth, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialTenth, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialEleventh, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialTwelfth, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NuclearWaffleProjectileProgram, Animation<NuclearWaffleProjectileProgram>> StartingAt { get; } = new Dictionary<NuclearWaffleProjectileProgram, Animation<NuclearWaffleProjectileProgram>>
    {
        [P.Initial] = new([new(P.Initial, 0, 12, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NuclearWaffleProjectileProgram> StartingWith(NuclearWaffleProjectileProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);
}
