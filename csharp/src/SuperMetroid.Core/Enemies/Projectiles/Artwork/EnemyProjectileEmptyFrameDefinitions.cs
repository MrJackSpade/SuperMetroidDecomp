namespace SuperMetroid.Core.Enemies.Projectiles.Artwork;

/// <summary>
/// The animation frames that show the empty projectile frame (#1513): an enemy's death explosion while it waits to
/// respawn or scatters its sprite objects, the falling spark's and Phantoon's flames' blinks, the Torizos' drool delays,
/// Golden Torizo's eye beam floor impact loop, one pose of the Tourian statues' projectiles, and Botwoon's segments
/// hidden in the wall. Like
/// <see cref="EnemyProjectileFrameId.Blank"/>, they draw nothing and are on no sheet.
/// </summary>
internal static class EnemyProjectileEmptyFrameDefinitions
{
    /// <summary>The animation frames that show the empty projectile frame.</summary>
    internal static readonly HashSet<EnemyProjectileFrameId> EmptyFrames =
    [
        EnemyProjectileFrameId.EnemyDeathRespawnTail,
        EnemyProjectileFrameId.EnemyDeathBigExplosion,
        EnemyProjectileFrameId.EnemyDeathMiniKraidExplosion,
        EnemyProjectileFrameId.FallingSparkHitFloorSecond,
        EnemyProjectileFrameId.FallingSparkHitFloorFourth,
        EnemyProjectileFrameId.FallingSparkHitFloorSixth,
        EnemyProjectileFrameId.FallingSparkHitFloorEighth,
        EnemyProjectileFrameId.FallingSparkHitFloorTenth,
        EnemyProjectileFrameId.PhantoonRainImpactFifth,
        EnemyProjectileFrameId.PhantoonCasualFallingFourth,
        EnemyProjectileFrameId.PhantoonCasualFallingSixth,
        EnemyProjectileFrameId.TorizoDroolFourFrameDelay,
        EnemyProjectileFrameId.TorizoDroolTwoFrameDelay,
        EnemyProjectileFrameId.GoldenTorizoEyeBeamFloorImpactLoopFirst,
        EnemyProjectileFrameId.TourianStatueEyeGlowTenth,
        EnemyProjectileFrameId.BotwoonHiddenUp,
    ];
}
