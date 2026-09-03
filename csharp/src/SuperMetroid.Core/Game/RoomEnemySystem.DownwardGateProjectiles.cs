using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>Applies one bank-$84 gate request to the shared eighteen-slot projectile pool.</summary>
    public void ApplyDownwardGateProjectileRequest(
        DownwardGateProjectileRequest request,
        int roomWidthInBlocks)
    {
        EnsureLoaded();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomWidthInBlocks);
        ArgumentOutOfRangeException.ThrowIfNegative(request.PlmBlockIndex);

        switch (request.Operation)
        {
            case DownwardGateProjectileOperation.Spawn:
                SpawnDownwardGateProjectile(request, roomWidthInBlocks);
                return;
            case DownwardGateProjectileOperation.Wake:
                WakeDownwardGateProjectile(request.PlmBlockIndex);
                return;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(request), request.Operation, "Unknown downward gate projectile operation.");
        }
    }

    private void SpawnDownwardGateProjectile(
        DownwardGateProjectileRequest request,
        int roomWidthInBlocks)
    {
        RoomEnemyProjectileKind kind = (RoomEnemyProjectileKind)request.DefinitionPointer;
        if (kind is not RoomEnemyProjectileKind.DownwardGateMoving and
            not RoomEnemyProjectileKind.DownwardGateClosed)
        {
            throw new InvalidDataException(
                $"Downward gate requested unsupported projectile definition $86:{request.DefinitionPointer:X4}.");
        }

        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return; // SpawnEprojInner's deliberate finite-pool failure.

        InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: 0);
        int blockX = request.PlmBlockIndex % roomWidthInBlocks;
        int blockY = request.PlmBlockIndex / roomWidthInBlocks;
        projectile.XPosition = unchecked((ushort)(
            blockX * DownwardGateEnemyProjectileRomData.PixelsPerRoomBlock));
        projectile.YPosition = unchecked((ushort)(
            blockY * DownwardGateEnemyProjectileRomData.PixelsPerRoomBlock +
            (kind == RoomEnemyProjectileKind.DownwardGateClosed
                ? DownwardGateEnemyProjectileRomData.ClosedPositionOffsetPixels
                : 0)));

        // Native eproj_E stores PLM_BlockIndices[x], whose unit is bytes. This association
        // is later searched by $84:BBF0 when the gate needs to open again.
        projectile.Variable0 = checked((ushort)(
            request.PlmBlockIndex * DownwardGateEnemyProjectileRomData.NativeBytesPerRoomBlock));
    }

    private void WakeDownwardGateProjectile(int plmBlockIndex)
    {
        ushort nativeBlockIndex = checked((ushort)(
            plmBlockIndex * DownwardGateEnemyProjectileRomData.NativeBytesPerRoomBlock));
        RoomEnemyProjectileSlot? projectile = null;
        for (int index = _enemyProjectiles.Length - 1; index >= 0; index--)
        {
            RoomEnemyProjectileSlot candidate = _enemyProjectiles[index];
            if (candidate.IsActive &&
                candidate.Kind is (RoomEnemyProjectileKind.DownwardGateMoving or
                    RoomEnemyProjectileKind.DownwardGateClosed) &&
                candidate.Variable0 == nativeBlockIndex)
            {
                projectile = candidate;
                break;
            }
        }
        if (projectile is null)
        {
            throw new InvalidDataException(
                $"Downward gate PLM block {plmBlockIndex} has no associated bank-$86 actor.");
        }

        projectile.InstructionTimer = 1;
        projectile.InstructionPointer = unchecked((ushort)(projectile.InstructionPointer + 2));
    }

    /// <summary>Ports <c>$86:E605</c>'s one-block-at-a-time signed 8.8 motion.</summary>
    private static void RunDownwardGateProjectileMovement(RoomEnemyProjectileSlot projectile)
    {
        int speedMagnitude = Math.Abs(unchecked((short)projectile.YVelocity));
        int accumulated = projectile.GeneralTimer + speedMagnitude;
        if (accumulated >= DownwardGateEnemyProjectileRomData.OneBlockDistance)
        {
            // The sleeping opcode is the current instruction pointer. Advancing two bytes
            // wakes the next visual frame on this same projectile-handler pass.
            projectile.InstructionTimer = 1;
            projectile.InstructionPointer = unchecked((ushort)(projectile.InstructionPointer + 2));
            accumulated = 0;
        }
        projectile.GeneralTimer = unchecked((ushort)accumulated);
        (projectile.YPosition, projectile.YSubposition) = AddEightBitVelocity(
            projectile.YPosition,
            projectile.YSubposition,
            projectile.YVelocity);
    }
}
