using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>A Fake Kraid spit or spike's typed program: it shows its pose, then holds as the native sleep does.</summary>
/// <param name="pose">The pose to show.</param>
internal sealed class FakeKraidProjectileAnimation(FakeKraidProjectilePose pose)
    : EnemyProjectileProgramAnimation<FakeKraidProjectilePose, FakeKraidProjectilePose, NoAnimationCue>(pose)
{

    /// <inheritdoc/>
    private protected override AnimationProgramSet<FakeKraidProjectilePose, FakeKraidProjectilePose, NoAnimationCue> Programs => PosePrograms;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(FakeKraidProjectilePose frame) =>
        ShownProjectileFrames[frame];

    /// <summary>
    /// Each pose's program: <c>InstList_EnemyProjectile_MiniKraidSpit</c> and <c>InstList_EnemyProjectile_MiniKraidSpikes_Left</c>
    /// and <c>_Right</c> each show their pose for the authored duration, then sleep.
    /// </summary>
    private static AnimationProgramSet<FakeKraidProjectilePose, FakeKraidProjectilePose, NoAnimationCue> PosePrograms { get; } = new(
        Enum.GetValues<FakeKraidProjectilePose>().ToDictionary(pose => pose, pose => new[]
        {
            AnimationProgramStep<FakeKraidProjectilePose, FakeKraidProjectilePose, NoAnimationCue>.Show(
                pose, FakeKraidProjectileInstructionProgramDefinitions.PoseDuration),
            AnimationProgramStep<FakeKraidProjectilePose, FakeKraidProjectilePose, NoAnimationCue>.Hold,
        }));

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<FakeKraidProjectilePose, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<FakeKraidProjectilePose, EnemyProjectileFrameId>
        {
            [FakeKraidProjectilePose.Spit] = EnemyProjectileFrameId.FakeKraidFrame00,
            [FakeKraidProjectilePose.SpikeLeft] = EnemyProjectileFrameId.FakeKraidFrame01,
            [FakeKraidProjectilePose.SpikeRight] = EnemyProjectileFrameId.FakeKraidFrame02,
        };
}
