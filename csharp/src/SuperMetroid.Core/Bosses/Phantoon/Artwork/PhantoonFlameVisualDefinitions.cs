using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// The presentation boundary of Phantoon's flames (#641): the projectile frame every typed
/// frame shows.
/// </summary>
internal static class PhantoonFlameVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<PhantoonFlameFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<PhantoonFlameFrame, EnemyProjectileFrameId>
        {
            [new(PhantoonFlameFrameKind.DestroyableIdle, 0)] = EnemyProjectileFrameId.PhantoonDestroyableIdleFirst,
            [new(PhantoonFlameFrameKind.DestroyableIdle, 1)] = EnemyProjectileFrameId.PhantoonDestroyableIdleSecond,
            [new(PhantoonFlameFrameKind.DestroyableIdle, 2)] = EnemyProjectileFrameId.PhantoonDestroyableIdleThird,
            [new(PhantoonFlameFrameKind.DestroyableIdle, 4)] = EnemyProjectileFrameId.PhantoonDestroyableIdleFifth,
            [new(PhantoonFlameFrameKind.CasualHitGround, 0)] = EnemyProjectileFrameId.PhantoonDestroyableIdleFifth,
            [new(PhantoonFlameFrameKind.CasualBouncing, 0)] = EnemyProjectileFrameId.PhantoonCasualBouncingFirst,
            [new(PhantoonFlameFrameKind.CasualBouncing, 1)] = EnemyProjectileFrameId.PhantoonCasualBouncingSecond,
            [new(PhantoonFlameFrameKind.CasualBouncing, 2)] = EnemyProjectileFrameId.PhantoonCasualBouncingThird,
            [new(PhantoonFlameFrameKind.CasualBouncing, 4)] = EnemyProjectileFrameId.PhantoonCasualBouncingFifth,
            [new(PhantoonFlameFrameKind.CasualBouncing, 5)] = EnemyProjectileFrameId.PhantoonCasualBouncingSixth,
            [new(PhantoonFlameFrameKind.CasualResting, 0)] = EnemyProjectileFrameId.PhantoonCasualBouncingFifth,
            [new(PhantoonFlameFrameKind.CasualResting, 1)] = EnemyProjectileFrameId.PhantoonCasualBouncingSixth,
            [new(PhantoonFlameFrameKind.CasualResting, 2)] = EnemyProjectileFrameId.PhantoonCasualRestingThird,
            [new(PhantoonFlameFrameKind.CasualResting, 3)] = EnemyProjectileFrameId.PhantoonCasualRestingFourth,
            [new(PhantoonFlameFrameKind.CasualResting, 4)] = EnemyProjectileFrameId.PhantoonCasualRestingFifth,
            [new(PhantoonFlameFrameKind.CasualResting, 5)] = EnemyProjectileFrameId.PhantoonCasualRestingSixth,
            [new(PhantoonFlameFrameKind.Dying, 0)] = EnemyProjectileFrameId.PhantoonDyingFirst,
            [new(PhantoonFlameFrameKind.Dying, 1)] = EnemyProjectileFrameId.PhantoonDyingSecond,
            [new(PhantoonFlameFrameKind.Dying, 2)] = EnemyProjectileFrameId.PhantoonDyingThird,
            [new(PhantoonFlameFrameKind.Dying, 3)] = EnemyProjectileFrameId.PhantoonDyingFourth,
            [new(PhantoonFlameFrameKind.RainImpact, 0)] = EnemyProjectileFrameId.PhantoonRainImpactFirst,
            [new(PhantoonFlameFrameKind.RainImpact, 3)] = EnemyProjectileFrameId.PhantoonRainImpactFourth,
            [new(PhantoonFlameFrameKind.RainImpact, 4)] = EnemyProjectileFrameId.PhantoonRainImpactFifth,
            [new(PhantoonFlameFrameKind.CasualFalling, 0)] = EnemyProjectileFrameId.PhantoonRainImpactFourth,
            [new(PhantoonFlameFrameKind.CasualFalling, 1)] = EnemyProjectileFrameId.PhantoonRainImpactFifth,
            [new(PhantoonFlameFrameKind.CasualFalling, 2)] = EnemyProjectileFrameId.PhantoonCasualFallingThird,
            [new(PhantoonFlameFrameKind.CasualFalling, 3)] = EnemyProjectileFrameId.PhantoonCasualFallingFourth,
            [new(PhantoonFlameFrameKind.CasualFalling, 4)] = EnemyProjectileFrameId.PhantoonCasualFallingFifth,
            [new(PhantoonFlameFrameKind.CasualFalling, 5)] = EnemyProjectileFrameId.PhantoonCasualFallingSixth,
            [new(PhantoonFlameFrameKind.StartingFlame, 0)] = EnemyProjectileFrameId.PhantoonStartingFlameFirst,
            [new(PhantoonFlameFrameKind.StartingFlame, 1)] = EnemyProjectileFrameId.PhantoonStartingFlameSecond,
            [new(PhantoonFlameFrameKind.StartingFlame, 2)] = EnemyProjectileFrameId.PhantoonStartingFlameThird,
            [new(PhantoonFlameFrameKind.DestroyableShot, 0)] = EnemyProjectileFrameId.PhantoonDestroyableShotFirst,
            [new(PhantoonFlameFrameKind.DestroyableShot, 1)] = EnemyProjectileFrameId.PhantoonDestroyableShotSecond,
            [new(PhantoonFlameFrameKind.DestroyableShot, 2)] = EnemyProjectileFrameId.PhantoonDestroyableShotThird,
            [new(PhantoonFlameFrameKind.DestroyableShot, 3)] = EnemyProjectileFrameId.PhantoonDestroyableShotFourth,
        };
}
