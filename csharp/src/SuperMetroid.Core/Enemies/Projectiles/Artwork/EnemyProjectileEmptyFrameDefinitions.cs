namespace SuperMetroid.Core.Enemies.Projectiles.Artwork;

/// <summary>
/// The animation frames that show the empty projectile frame (#1513): an enemy's death explosion while it waits to
/// respawn or scatters its effect actors, the falling spark's and Phantoon's flames' blinks, the Torizos' drool delays,
/// Golden Torizo's eye beam floor impact loop, one pose of the Tourian statues' projectiles, and Botwoon's segments
/// hidden in the wall. Like
/// <see cref="EnemyProjectileFrameId.Blank"/>, they draw nothing and are on no sheet.
/// </summary>
internal static class EnemyProjectileEmptyFrameDefinitions
{
    /// <summary>The animation frames that show the empty projectile frame.</summary>
    internal static readonly HashSet<EnemyProjectileFrameId> EmptyFrames =
    [
        EnemyProjectileFrameId.EnemyDeathBlank,
        EnemyProjectileFrameId.EnemyDeathBlankTwin,
        EnemyProjectileFrameId.EnemyDeathBlankTwinB,
        EnemyProjectileFrameId.FallingSparkBlank,
        EnemyProjectileFrameId.FallingSparkBlankTwin,
        EnemyProjectileFrameId.FallingSparkBlankTwinB,
        EnemyProjectileFrameId.FallingSparkBlankTwinC,
        EnemyProjectileFrameId.FallingSparkBlankTwinD,
        EnemyProjectileFrameId.PhantoonFlameBlank,
        EnemyProjectileFrameId.PhantoonFlameBlankTwin,
        EnemyProjectileFrameId.PhantoonFlameBlankTwinB,
        EnemyProjectileFrameId.TorizoDroolBlank,
        EnemyProjectileFrameId.TorizoDroolBlankTwin,
        EnemyProjectileFrameId.GoldenTorizoEyeBeamBlank,
        EnemyProjectileFrameId.TourianStatueEyeGlowBlank,
        EnemyProjectileFrameId.BotwoonBlank,
    ];
}
