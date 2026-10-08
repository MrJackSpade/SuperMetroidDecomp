namespace SuperMetroid.Core.Game;

/// <summary>Screen-relative bounds used by bank-$86 enemy projectile deletion routines.</summary>
public static class EnemyProjectileOffScreenDefinitions
{
    /// <summary>
    /// <c>CMP #$0120</c> at $86:B5C2: pixels below the camera's top row at which
    /// <c>Delete_EnemyProjectile_IfVerticallyOffScreen</c> deletes a projectile.
    /// </summary>
    public const ushort VerticalDeletionDepth = 0x0120;
}
