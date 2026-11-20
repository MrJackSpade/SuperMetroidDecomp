using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>A Fake Kraid spit or spike's typed animation: it shows its pose and holds it for good.</summary>
/// <param name="pose">The pose to show.</param>
internal sealed class FakeKraidProjectileAnimation(FakeKraidProjectilePose pose)
    : EnemyProjectileSilentAnimation<FakeKraidProjectilePose, FakeKraidProjectilePose>(PoseAnimations[pose],
        static (_, _, cue, _) => EnemyProjectileCuelessAnimations.Undefined<FakeKraidProjectilePose>(cue))
{

    /// <inheritdoc/>
    private protected override AnimationSegments<FakeKraidProjectilePose, FakeKraidProjectilePose, NoAnimationCue> Segments => PoseSegments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<FakeKraidProjectilePose, Animation<FakeKraidProjectilePose>> StartingAt => PoseAnimations;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(FakeKraidProjectilePose frame) =>
        ShownProjectileFrames[frame];

    /// <summary>
    /// Each pose's segment: the spit and the left and right spikes each hold their pose for good (#1627), until a shot
    /// deletes the projectile.
    /// </summary>
    private static AnimationSegments<FakeKraidProjectilePose, FakeKraidProjectilePose, NoAnimationCue> PoseSegments { get; } = new(
        Enum.GetValues<FakeKraidProjectilePose>().ToDictionary(pose => pose, pose => new AnimationBeat<FakeKraidProjectilePose, NoAnimationCue>[]
        {
            new AnimationBeat<FakeKraidProjectilePose, NoAnimationCue>.HeldFrame(pose),
        }));

    /// <summary>Each pose's animation: its segment once, then a hold.</summary>
    private static Dictionary<FakeKraidProjectilePose, Animation<FakeKraidProjectilePose>> PoseAnimations { get; } =
        Enum.GetValues<FakeKraidProjectilePose>().ToDictionary(pose => pose,
            pose => new Animation<FakeKraidProjectilePose>([PoseSegments.Whole(pose)], AnimationEnd.Holds));

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<FakeKraidProjectilePose, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<FakeKraidProjectilePose, EnemyProjectileFrameId>
        {
            [FakeKraidProjectilePose.Spit] = EnemyProjectileFrameId.FakeKraidSpit,
            [FakeKraidProjectilePose.SpikeLeft] = EnemyProjectileFrameId.FakeKraidSpikeLeft,
            [FakeKraidProjectilePose.SpikeRight] = EnemyProjectileFrameId.FakeKraidSpikeRight,
        };
}
