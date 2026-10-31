namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Screen-relative bounds used by enemy projectile deletion routines.</summary>
public static class EnemyProjectileOffScreenDefinitions
{
    /// <summary>
    /// <c>CMP #$0120</c>: pixels below the camera's top row at which
    /// <c>Delete_EnemyProjectile_IfVerticallyOffScreen</c> deletes a projectile.
    /// </summary>
    public const int VerticalDeletionDepth = 288;
}
