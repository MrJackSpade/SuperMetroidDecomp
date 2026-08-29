using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    private static readonly short[] ShaktoolCircleXOffsets =
        [0, 12, 16, 12, 0, -12, -16, -12];
    private static readonly short[] ShaktoolCircleYOffsets =
        [-16, -12, 0, 12, 16, 12, 0, -12];

    /// <summary>
    /// Ports dead retail helper <c>Shaktool_Func_2</c> at <c>$AA:D9A0</c>. The routine has
    /// no caller in the shipped ROM, but retaining its three bank-$86 actors prevents an
    /// apparently complete Shaktool translation from silently discarding authored code.
    /// </summary>
    internal void SpawnUnusedShaktoolAttackCircles(RoomEnemySlot segment)
    {
        if (segment.EnemyDefinitionPointer != ShaktoolDefinition)
            throw new ArgumentException("Attack circles require a Shaktool segment.", nameof(segment));

        SpawnUnusedShaktoolAttackCircle(
            segment,
            RoomEnemyProjectileKind.ShaktoolAttackFrontCircle,
            storesOwnerProjectileIndex: false);
        SpawnUnusedShaktoolAttackCircle(
            segment,
            RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle,
            storesOwnerProjectileIndex: true);
        SpawnUnusedShaktoolAttackCircle(
            segment,
            RoomEnemyProjectileKind.ShaktoolAttackBackCircle,
            storesOwnerProjectileIndex: true);
    }

    private void SpawnUnusedShaktoolAttackCircle(
        RoomEnemySlot segment,
        RoomEnemyProjectileKind kind,
        bool storesOwnerProjectileIndex)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            unchecked((ushort)(segment.VramTilesIndex | segment.PaletteIndex)));

        // BD9C stores eproj_init_param_1 in variable E for the middle/back variants. The
        // only native caller passes literal zero. This looks suspicious with the descending
        // projectile allocator, but preserving that unreachable-code quirk is preferable to
        // inventing a link to the newly allocated front circle.
        if (storesOwnerProjectileIndex)
            projectile.Variable0 = 0;

        projectile.XPosition = segment.XPosition;
        projectile.YPosition = segment.YPosition;
        byte angle = unchecked((byte)segment.VariableD);
        projectile.XVelocity = ReadShaktoolCommonSineSample(angle + 64);
        projectile.YVelocity = ReadShaktoolCommonSineSample(angle);

        int offsetIndex = angle >> 5;
        projectile.XPosition = unchecked((ushort)(
            projectile.XPosition + ShaktoolCircleXOffsets[offsetIndex]));
        projectile.YPosition = unchecked((ushort)(
            projectile.YPosition + ShaktoolCircleYOffsets[offsetIndex]));
    }

    /// <summary>Ports front-circle pre-instruction <c>$86:BE03</c>.</summary>
    private static void RunShaktoolFrontCirclePreInstruction(
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
