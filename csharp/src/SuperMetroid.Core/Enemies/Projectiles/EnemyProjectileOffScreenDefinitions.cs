namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Screen-relative bounds used by enemy projectile deletion checks.</summary>
internal static partial class EnemyProjectileOffScreenDefinitions
{
    /// <summary>
    /// The distance below the camera's top row at which
    /// the vertical off-screen check deletes a projectile.
    /// </summary>
    internal const int VerticalDeletionDepth = 288;
}
