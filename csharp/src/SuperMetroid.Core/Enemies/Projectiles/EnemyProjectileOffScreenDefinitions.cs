namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Screen-relative bounds used by enemy projectile deletion checks.</summary>
public static class EnemyProjectileOffScreenDefinitions
{
    /// <summary>
    /// The distance, 288 pixels below the camera's top row, at which
    /// the vertical off-screen check deletes a projectile.
    /// </summary>
    public const int VerticalDeletionDepth = 288;
}
