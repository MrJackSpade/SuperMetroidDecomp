using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>Ports front-circle pre-instruction <c>$86:BE03</c>.</summary>
    private void RunShaktoolFrontCirclePreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true) ||
            MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Clear();
        }
    }

    /// <summary>Ports middle/back linked-circle pre-instruction <c>$86:BE12</c>.</summary>
    private void RunShaktoolLinkedCirclePreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        ushort ownerNativeIndex = projectile.Variable0;
        if ((ownerNativeIndex & 1) != 0 ||
            ownerNativeIndex >= _enemyProjectiles.Length * 2 ||
            !_enemyProjectiles[ownerNativeIndex >> 1].IsActive)
        {
            projectile.Clear();
            return;
        }

        _ = MoveProjectileAxis(projectile, level, horizontal: true);
        _ = MoveProjectileAxis(projectile, level, horizontal: false);
    }
}
