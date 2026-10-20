using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Dragon.DragonFireballProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Dragon's rising and falling fireball programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="DragonFireballFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class DragonFireballAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<DragonFireballProgram, DragonFireballFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<DragonFireballProgram, AnimationProgramStep<DragonFireballProgram, DragonFireballFrame, EnemyProjectileCommonCue>[]>
    {
        [P.RisingLeft] =
        [
            Show(0, 5),
            Show(1, 5),
            GoTo(P.RisingLeft),
        ],
        [P.RisingRight] =
        [
            Show(2, 5),
            Show(3, 5),
            GoTo(P.RisingRight),
        ],
        [P.FallingLeft] =
        [
            Show(4, 5),
            Show(5, 5),
            GoTo(P.FallingLeft),
        ],
        [P.FallingRight] =
        [
            Show(6, 5),
            Show(7, 5),
            GoTo(P.FallingRight),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<DragonFireballProgram, DragonFireballFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<DragonFireballProgram, DragonFireballFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<DragonFireballProgram, DragonFireballFrame, EnemyProjectileCommonCue> GoTo(DragonFireballProgram program) => AnimationProgramStep<DragonFireballProgram, DragonFireballFrame, EnemyProjectileCommonCue>.GoTo(program);
}
