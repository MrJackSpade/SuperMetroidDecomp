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

    /// <summary>The poses in list order, the order their installed frames are named in.</summary>
    internal static IReadOnlyList<FakeKraidProjectilePose> Poses { get; } =
        [FakeKraidProjectilePose.Spit, FakeKraidProjectilePose.SpikeLeft, FakeKraidProjectilePose.SpikeRight];

    /// <summary>The installed artwork key of <paramref name="pose"/>: its native visual operand.</summary>
    internal static ushort PresentationKey(FakeKraidProjectilePose pose) => pose switch
    {
        FakeKraidProjectilePose.Spit => FakeKraidProjectileInstructionProgramDefinitions.SpitOperand,
        FakeKraidProjectilePose.SpikeLeft => FakeKraidProjectileInstructionProgramDefinitions.SpikeLeftOperand,
        FakeKraidProjectilePose.SpikeRight => FakeKraidProjectileInstructionProgramDefinitions.SpikeRightOperand,
        _ => throw new ArgumentOutOfRangeException(nameof(pose), pose, "Undefined Fake Kraid projectile pose."),
    };

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(FakeKraidProjectilePose frame) =>
        EnemyProjectileFramePresentation.Installed(PresentationKey(frame));

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
}
