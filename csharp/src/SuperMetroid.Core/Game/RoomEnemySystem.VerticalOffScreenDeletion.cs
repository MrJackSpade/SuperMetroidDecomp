namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Ports <c>Delete_EnemyProjectile_IfVerticallyOffScreen</c> at $86:B5B9, shared by
    /// Dragon's fireball and both n00b-tube shard pre-instructions.
    /// </summary>
    /// <remarks>
    /// A position above the camera's top row (signed-negative difference) is kept; only a
    /// difference of <see cref="EnemyProjectileOffScreenDefinitions.VerticalDeletionDepth"/>
    /// or more below it deletes the projectile.
    /// </remarks>
    internal static void DeleteEnemyProjectileIfVerticallyOffScreen(
        RoomEnemyProjectileSlot projectile,
        ushort cameraY)
    {
        ushort screenY = unchecked((ushort)(projectile.YPosition - cameraY));
        if (unchecked((short)screenY) >= 0 &&
            screenY >= EnemyProjectileOffScreenDefinitions.VerticalDeletionDepth)
        {
            projectile.Clear();
        }
    }
}
