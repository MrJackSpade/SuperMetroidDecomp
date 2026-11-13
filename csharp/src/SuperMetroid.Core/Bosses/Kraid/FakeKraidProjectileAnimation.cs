using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>A Fake Kraid spit or spike's typed animation: it shows its pose and holds it for good, as the script's sleep does.</summary>
/// <param name="pose">The pose to show.</param>
internal sealed class FakeKraidProjectileAnimation(FakeKraidProjectilePose pose)
    : EnemyProjectileTypedAnimation<FakeKraidProjectilePose, FakeKraidProjectilePose, NoAnimationCue>(PoseAnimations[pose])
{

    /// <inheritdoc/>
    private protected override AnimationSegments<FakeKraidProjectilePose, FakeKraidProjectilePose, NoAnimationCue> Segments => PoseSegments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<FakeKraidProjectilePose, Animation<FakeKraidProjectilePose>> StartingAt => PoseAnimations;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(FakeKraidProjectilePose frame) =>
        ShownProjectileFrames[frame];

    /// <summary>
    /// Each pose's segment: the spit and the left and right spikes each hold their pose for good (#1627). The original's
    /// lists show it for the longest frame a list gives and then sleep on it; nothing reads the projectile's frame timer,
    /// so the pose simply stays until a shot deletes it.
    /// </summary>
    private static AnimationSegments<FakeKraidProjectilePose, FakeKraidProjectilePose, NoAnimationCue> PoseSegments { get; } = new(
        Enum.GetValues<FakeKraidProjectilePose>().ToDictionary(pose => pose, pose => new AnimationBeat<FakeKraidProjectilePose, NoAnimationCue>[]
        {
            new AnimationBeat<FakeKraidProjectilePose, NoAnimationCue>.HeldFrame(pose),
        }));

    /// <summary>Each pose's animation: its segment once, then a hold, as the script's sleep does.</summary>
    private static Dictionary<FakeKraidProjectilePose, Animation<FakeKraidProjectilePose>> PoseAnimations { get; } =
        Enum.GetValues<FakeKraidProjectilePose>().ToDictionary(pose => pose,
            pose => new Animation<FakeKraidProjectilePose>([new(pose, 0, 1, 1)], AnimationEnd.Holds));

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<FakeKraidProjectilePose, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<FakeKraidProjectilePose, EnemyProjectileFrameId>
        {
            [FakeKraidProjectilePose.Spit] = EnemyProjectileFrameId.FakeKraidSpit,
            [FakeKraidProjectilePose.SpikeLeft] = EnemyProjectileFrameId.FakeKraidSpikeLeft,
            [FakeKraidProjectilePose.SpikeRight] = EnemyProjectileFrameId.FakeKraidSpikeRight,
        };
}
