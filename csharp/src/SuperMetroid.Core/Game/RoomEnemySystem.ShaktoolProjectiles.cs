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

        // The disabled native routine does not pass a made-up owner token. The first JSL
        // returns the physical bank-$86 slot index in A ($22, $20, ...), then PHA/PLA keeps
        // that value intact across the middle-circle allocation. Both delayed circles use
        // the saved front index as eproj_init_param_1, which initializer $BD9C copies into
        // variable E. Their moving pre-instruction later dereferences that exact slot.
        RoomEnemyProjectileSlot? front = SpawnUnusedShaktoolAttackCircle(
            segment,
            RoomEnemyProjectileKind.ShaktoolAttackFrontCircle,
            linkedFrontNativeIndex: null);
        if (front is null)
            return;

        ushort frontNativeIndex = unchecked((ushort)(front.SlotIndex * 2));
        SpawnUnusedShaktoolAttackCircle(
            segment,
            RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle,
            frontNativeIndex);
        SpawnUnusedShaktoolAttackCircle(
            segment,
            RoomEnemyProjectileKind.ShaktoolAttackBackCircle,
            frontNativeIndex);
    }

    private RoomEnemyProjectileSlot? SpawnUnusedShaktoolAttackCircle(
        RoomEnemySlot segment,
        RoomEnemyProjectileKind kind,
        ushort? linkedFrontNativeIndex)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return null;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            unchecked((ushort)(segment.VramTilesIndex | segment.PaletteIndex)));

        // Front initializer $BDA2 ignores the spawn parameter. Middle/back initializer
        // $BD9C stores it before falling through to the same position/velocity setup.
        if (linkedFrontNativeIndex is ushort ownerNativeIndex)
            projectile.Variable0 = ownerNativeIndex;

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

        // SpawnEnemyProjectileY_ParameterA_XGraphics returns this physical actor in A. The
        // caller converts it to a native index only by virtue of the pool's 2-byte stride;
        // returning the object keeps that allocation fact explicit in the C# translation.
        return projectile;
    }

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
