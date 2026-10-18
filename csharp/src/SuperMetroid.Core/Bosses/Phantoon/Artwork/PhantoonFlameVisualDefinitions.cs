using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// The presentation boundary of Phantoon's flames (#641): the projectile frame every typed
/// frame shows.
/// </summary>
internal static class PhantoonFlameVisualDefinitions
{
    /// <summary>Distinct frames, in installed-artwork order.</summary>
    internal const int ProjectileFrameCount = 31;

    /// <summary><c>InstList_EnemyProjectile_PhantoonDestroyableFlame_Idle</c> at $86:975C.</summary>
    internal const ushort DestroyableIdleProgram = 0x975c;
    /// <summary><c>InstList_EnemyProjectile_PhantoonStartingFlames</c> at $86:97E8.</summary>
    internal const ushort StartingFlameProgram = 0x97e8;

    /// <summary>Every frame in installed-artwork order.</summary>
    internal static IReadOnlyList<PhantoonFlameFrame> ProjectileFrames { get; } =
    [
        .. Frames(PhantoonFlameFrameKind.DestroyableIdle, PhantoonFlameAnimations.LoopPoseCount),
        .. Frames(PhantoonFlameFrameKind.CasualHitGround, 1),
        .. Frames(PhantoonFlameFrameKind.CasualBouncing, PhantoonFlameAnimations.LoopPoseCount),
        .. Frames(PhantoonFlameFrameKind.CasualResting, PhantoonFlameAnimations.RestingPoseCount),
        .. Frames(PhantoonFlameFrameKind.Dying, PhantoonFlameAnimations.DyingPoseCount),
        .. Frames(PhantoonFlameFrameKind.RainImpact, 1),
        .. Frames(PhantoonFlameFrameKind.CasualFalling, PhantoonFlameAnimations.FallingPhaseCount * PhantoonFlameAnimations.FallingPoseCount),
        .. Frames(PhantoonFlameFrameKind.StartingFlame, PhantoonFlameAnimations.LoopPoseCount),
        .. Frames(PhantoonFlameFrameKind.DestroyableShot, PhantoonFlameAnimations.ShotPoseCount),
    ];

    /// <summary>The <paramref name="count"/> frames of <paramref name="kind"/>.</summary>
    private static IEnumerable<PhantoonFlameFrame> Frames(PhantoonFlameFrameKind kind, int count) =>
        Enumerable.Range(0, count).Select(phase => new PhantoonFlameFrame(kind, phase));

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<PhantoonFlameFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<PhantoonFlameFrame, EnemyProjectileFrameId>
        {
            [new(PhantoonFlameFrameKind.DestroyableIdle, 0)] = EnemyProjectileFrameId.PhantoonFrame00,
            [new(PhantoonFlameFrameKind.DestroyableIdle, 1)] = EnemyProjectileFrameId.PhantoonFrame01,
            [new(PhantoonFlameFrameKind.DestroyableIdle, 2)] = EnemyProjectileFrameId.PhantoonFrame02,
            [new(PhantoonFlameFrameKind.DestroyableIdle, 4)] = EnemyProjectileFrameId.PhantoonFrame03,
            [new(PhantoonFlameFrameKind.CasualHitGround, 0)] = EnemyProjectileFrameId.PhantoonFrame03,
            [new(PhantoonFlameFrameKind.CasualBouncing, 0)] = EnemyProjectileFrameId.PhantoonFrame04,
            [new(PhantoonFlameFrameKind.CasualBouncing, 1)] = EnemyProjectileFrameId.PhantoonFrame05,
            [new(PhantoonFlameFrameKind.CasualBouncing, 2)] = EnemyProjectileFrameId.PhantoonFrame06,
            [new(PhantoonFlameFrameKind.CasualBouncing, 4)] = EnemyProjectileFrameId.PhantoonFrame07,
            [new(PhantoonFlameFrameKind.CasualBouncing, 5)] = EnemyProjectileFrameId.PhantoonFrame08,
            [new(PhantoonFlameFrameKind.CasualResting, 0)] = EnemyProjectileFrameId.PhantoonFrame07,
            [new(PhantoonFlameFrameKind.CasualResting, 1)] = EnemyProjectileFrameId.PhantoonFrame08,
            [new(PhantoonFlameFrameKind.CasualResting, 2)] = EnemyProjectileFrameId.PhantoonFrame09,
            [new(PhantoonFlameFrameKind.CasualResting, 3)] = EnemyProjectileFrameId.PhantoonFrame10,
            [new(PhantoonFlameFrameKind.CasualResting, 4)] = EnemyProjectileFrameId.PhantoonFrame11,
            [new(PhantoonFlameFrameKind.CasualResting, 5)] = EnemyProjectileFrameId.PhantoonFrame12,
            [new(PhantoonFlameFrameKind.Dying, 0)] = EnemyProjectileFrameId.PhantoonFrame13,
            [new(PhantoonFlameFrameKind.Dying, 1)] = EnemyProjectileFrameId.PhantoonFrame14,
            [new(PhantoonFlameFrameKind.Dying, 2)] = EnemyProjectileFrameId.PhantoonFrame15,
            [new(PhantoonFlameFrameKind.Dying, 3)] = EnemyProjectileFrameId.PhantoonFrame16,
            [new(PhantoonFlameFrameKind.RainImpact, 0)] = EnemyProjectileFrameId.PhantoonFrame17,
            [new(PhantoonFlameFrameKind.RainImpact, 3)] = EnemyProjectileFrameId.PhantoonFrame18,
            [new(PhantoonFlameFrameKind.RainImpact, 4)] = EnemyProjectileFrameId.PhantoonFrame19,
            [new(PhantoonFlameFrameKind.CasualFalling, 0)] = EnemyProjectileFrameId.PhantoonFrame18,
            [new(PhantoonFlameFrameKind.CasualFalling, 1)] = EnemyProjectileFrameId.PhantoonFrame19,
            [new(PhantoonFlameFrameKind.CasualFalling, 2)] = EnemyProjectileFrameId.PhantoonFrame20,
            [new(PhantoonFlameFrameKind.CasualFalling, 3)] = EnemyProjectileFrameId.PhantoonFrame21,
            [new(PhantoonFlameFrameKind.CasualFalling, 4)] = EnemyProjectileFrameId.PhantoonFrame22,
            [new(PhantoonFlameFrameKind.CasualFalling, 5)] = EnemyProjectileFrameId.PhantoonFrame23,
            [new(PhantoonFlameFrameKind.StartingFlame, 0)] = EnemyProjectileFrameId.PhantoonFrame24,
            [new(PhantoonFlameFrameKind.StartingFlame, 1)] = EnemyProjectileFrameId.PhantoonFrame25,
            [new(PhantoonFlameFrameKind.StartingFlame, 2)] = EnemyProjectileFrameId.PhantoonFrame26,
            [new(PhantoonFlameFrameKind.DestroyableShot, 0)] = EnemyProjectileFrameId.PhantoonFrame27,
            [new(PhantoonFlameFrameKind.DestroyableShot, 1)] = EnemyProjectileFrameId.PhantoonFrame28,
            [new(PhantoonFlameFrameKind.DestroyableShot, 2)] = EnemyProjectileFrameId.PhantoonFrame29,
            [new(PhantoonFlameFrameKind.DestroyableShot, 3)] = EnemyProjectileFrameId.PhantoonFrame30,
        };
}
