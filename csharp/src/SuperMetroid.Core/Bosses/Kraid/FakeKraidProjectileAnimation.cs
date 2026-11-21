using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>A Fake Kraid spit or spike's typed animation: it shows its pose and holds it for good.</summary>
internal sealed partial class FakeKraidProjectileAnimation
    : EnemyProjectileSilentAnimation<FakeKraidProjectilePose, FakeKraidProjectilePose>
{
    /// <summary>Creates a <see cref="FakeKraidProjectileAnimation"/>.</summary>
    /// <param name="pose">The pose to show.</param>
    public FakeKraidProjectileAnimation(FakeKraidProjectilePose pose)
        : base(pose, PoseAnimations[pose])
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<FakeKraidProjectilePose, FakeKraidProjectilePose, NoAnimationCue> Segments => PoseSegments;

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
