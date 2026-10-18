using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>A Fake Kraid spit or spike's typed program: it shows its pose, then holds as the native sleep does.</summary>
internal sealed class FakeKraidProjectileAnimation : EnemyProjectileTimelineAnimation<FakeKraidProjectilePose, NoAnimationCue>
{
    /// <summary>Plays <paramref name="pose"/>'s program.</summary>
    internal FakeKraidProjectileAnimation(FakeKraidProjectilePose pose) => Pose = pose;

    /// <summary>The pose shown.</summary>
    internal FakeKraidProjectilePose Pose { get; }

    /// <inheritdoc/>
    private protected override AnimationTimeline<FakeKraidProjectilePose, NoAnimationCue> Timeline => Pose switch
    {
        FakeKraidProjectilePose.Spit => SpitProgram,
        FakeKraidProjectilePose.SpikeLeft => SpikeLeftProgram,
        FakeKraidProjectilePose.SpikeRight => SpikeRightProgram,
        _ => throw new InvalidOperationException($"Undefined {nameof(FakeKraidProjectilePose)} {Pose}."),
    };

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(FakeKraidProjectilePose frame) =>
        ShownProjectileFrames[frame];

    /// <summary><c>InstList_EnemyProjectile_MiniKraidSpit</c>: the spit pose, then sleep.</summary>
    private static AnimationTimeline<FakeKraidProjectilePose, NoAnimationCue> SpitProgram { get; } = Held(FakeKraidProjectilePose.Spit);

    /// <summary><c>InstList_EnemyProjectile_MiniKraidSpikes_Left</c>: the left spike pose, then sleep.</summary>
    private static AnimationTimeline<FakeKraidProjectilePose, NoAnimationCue> SpikeLeftProgram { get; } = Held(FakeKraidProjectilePose.SpikeLeft);

    /// <summary><c>InstList_EnemyProjectile_MiniKraidSpikes_Right</c>: the right spike pose, then sleep.</summary>
    private static AnimationTimeline<FakeKraidProjectilePose, NoAnimationCue> SpikeRightProgram { get; } = Held(FakeKraidProjectilePose.SpikeRight);

    /// <summary>Shows <paramref name="pose"/> for the authored duration, then holds.</summary>
    private static AnimationTimeline<FakeKraidProjectilePose, NoAnimationCue> Held(FakeKraidProjectilePose pose) => new(
        AnimationStep<FakeKraidProjectilePose, NoAnimationCue>.Show(pose, FakeKraidProjectileInstructionProgramDefinitions.PoseDuration),
        AnimationStep<FakeKraidProjectilePose, NoAnimationCue>.Hold);

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<FakeKraidProjectilePose, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<FakeKraidProjectilePose, EnemyProjectileFrameId>
        {
            [FakeKraidProjectilePose.Spit] = EnemyProjectileFrameId.FakeKraidFrame00,
            [FakeKraidProjectilePose.SpikeLeft] = EnemyProjectileFrameId.FakeKraidFrame01,
            [FakeKraidProjectilePose.SpikeRight] = EnemyProjectileFrameId.FakeKraidFrame02,
        };
}
