namespace SuperMetroid.Core.Enemies.Projectiles.Artwork;

/// <summary>
/// The program frames that show the empty projectile spritemap (#1513): an enemy's death explosion while it waits to
/// respawn or scatters its sprite objects, the falling spark's and Phantoon's flames' blinks, the Torizos' drool delays,
/// Golden Torizo's eye beam floor impact loop, one pose of the Tourian statues' projectiles, and Botwoon's segments
/// hidden in the wall. Like
/// <see cref="EnemyProjectileFrameId.Blank"/>, they draw nothing and are on no sheet.
/// </summary>
internal static class EnemyProjectileEmptyFrameDefinitions
{
    /// <summary>The program frames that show the empty projectile spritemap.</summary>
    internal static readonly HashSet<EnemyProjectileFrameId> EmptySpritemapFrames =
    [
        EnemyProjectileFrameId.EnemyDeathFrame00,
        EnemyProjectileFrameId.EnemyDeathFrame01,
        EnemyProjectileFrameId.EnemyDeathFrame02,
        EnemyProjectileFrameId.FallingSparkFrame04,
        EnemyProjectileFrameId.FallingSparkFrame06,
        EnemyProjectileFrameId.FallingSparkFrame08,
        EnemyProjectileFrameId.FallingSparkFrame10,
        EnemyProjectileFrameId.FallingSparkFrame12,
        EnemyProjectileFrameId.PhantoonFrame19,
        EnemyProjectileFrameId.PhantoonFrame21,
        EnemyProjectileFrameId.PhantoonFrame23,
        EnemyProjectileFrameId.TorizoDroolFrame00,
        EnemyProjectileFrameId.TorizoDroolFrame01,
        EnemyProjectileFrameId.GoldenTorizoEyeBeamFrame05,
        EnemyProjectileFrameId.TourianStatueFrame13,
        EnemyProjectileFrameId.BotwoonFrame40,
    ];
}
