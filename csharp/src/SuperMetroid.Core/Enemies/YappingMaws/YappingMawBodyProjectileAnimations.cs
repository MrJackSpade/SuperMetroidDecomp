using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.YappingMaws.YappingMawBodyProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's body-link programs (#641). Each
/// frame shows a <see cref="YappingMawBodyProjectileFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class YappingMawBodyProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<YappingMawBodyProjectileProgram, YappingMawBodyProjectileFrame, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<YappingMawBodyProjectileProgram, AnimationBeat<YappingMawBodyProjectileFrame, EnemyProjectileCommonCue>[]>
    {
        [P.FacingDown] =
        [
            Show(0, 1),
        ],
        [P.FacingUp] =
        [
            Show(1, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<YappingMawBodyProjectileProgram, Animation<YappingMawBodyProjectileProgram>> StartingAt { get; } = new Dictionary<YappingMawBodyProjectileProgram, Animation<YappingMawBodyProjectileProgram>>
    {
        [P.FacingDown] = new([new(P.FacingDown, 0, 1, 1)], AnimationEnd.Holds),
        [P.FacingUp] = new([new(P.FacingUp, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<YappingMawBodyProjectileProgram> StartingWith(YappingMawBodyProjectileProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<YappingMawBodyProjectileFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => new AnimationBeat<YappingMawBodyProjectileFrame, EnemyProjectileCommonCue>.Frame(new(pose), duration);
}
