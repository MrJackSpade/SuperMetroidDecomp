using P = SuperMetroid.Core.Enemies.YappingMaws.YappingMawBodyProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's body-link programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="YappingMawBodyProjectileFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class YappingMawBodyProjectileAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<YappingMawBodyProjectileProgram, YappingMawBodyProjectileFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<YappingMawBodyProjectileProgram, AnimationProgramStep<YappingMawBodyProjectileProgram, YappingMawBodyProjectileFrame, EnemyProjectileCommonCue>[]>
    {
        // $86:EC56
        [P.FacingDown] =
        [
            Show(0, 1),
            Hold,
        ],
        // $86:EC5C
        [P.FacingUp] =
        [
            Show(1, 1),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<YappingMawBodyProjectileProgram, YappingMawBodyProjectileFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<YappingMawBodyProjectileProgram, YappingMawBodyProjectileFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<YappingMawBodyProjectileProgram, YappingMawBodyProjectileFrame, EnemyProjectileCommonCue> Hold => AnimationProgramStep<YappingMawBodyProjectileProgram, YappingMawBodyProjectileFrame, EnemyProjectileCommonCue>.Hold;
}
