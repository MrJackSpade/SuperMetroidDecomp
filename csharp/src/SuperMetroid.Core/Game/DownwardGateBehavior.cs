using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The downward gate species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class DownwardGateBehavior
{
    /// <summary>Enforces that a room is loaded before gate requests are applied.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Shared finite projectile pool holding moving and closed gate actors.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds gate PLM requests to the current room's projectile owner.</summary>
    internal DownwardGateBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>Applies one bank-$84 gate request to the shared eighteen-slot projectile pool.</summary>
    public void ApplyDownwardGateProjectileRequest(
        DownwardGateProjectileRequest request,
        int roomWidthInBlocks)
    {
        _room.EnsureLoaded();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomWidthInBlocks);
        Ensure.AtLeastZero(request.PlmBlockIndex);

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

    /// <summary>Allocates a gate actor at its PLM block and records the native byte-index association used for waking.</summary>
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

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return; // SpawnEprojInner's deliberate finite-pool failure.

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: 0);
        int blockX = request.PlmBlockIndex % roomWidthInBlocks;
        int blockY = request.PlmBlockIndex / roomWidthInBlocks;
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(
            blockX * DownwardGateEnemyProjectileRomData.PixelsPerRoomBlock)), unchecked((ushort)(
            blockY * DownwardGateEnemyProjectileRomData.PixelsPerRoomBlock +
            (kind == RoomEnemyProjectileKind.DownwardGateClosed
                ? DownwardGateEnemyProjectileRomData.ClosedPositionOffsetPixels
                : 0)))));

        // Native eproj_E stores PLM_BlockIndices[x], whose unit is bytes. This association
        // is later searched by $84:BBF0 when the gate needs to open again.
        projectile.Variable0 = checked((ushort)(
            request.PlmBlockIndex * DownwardGateEnemyProjectileRomData.NativeBytesPerRoomBlock));
    }

    /// <summary>Finds the matching gate actor from the pool's end and advances its sleeping instruction.</summary>
    private void WakeDownwardGateProjectile(int plmBlockIndex)
    {
        ushort nativeBlockIndex = checked((ushort)(
            plmBlockIndex * DownwardGateEnemyProjectileRomData.NativeBytesPerRoomBlock));
        RoomEnemyProjectileSlot? projectile = null;
        for (int index = _projectiles.EnemyProjectiles.Count - 1; index >= 0; index--)
        {
            RoomEnemyProjectileSlot candidate = _projectiles.EnemyProjectiles[index];
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

        projectile.InstructionTimer = NativeCountdown.Start(1);
        projectile.InstructionPointer = unchecked((ushort)(projectile.InstructionPointer + 2));
    }

    /// <summary>Ports <c>$86:E605</c>'s one-block-at-a-time signed 8.8 motion.</summary>
    internal static void RunDownwardGateProjectileMovement(RoomEnemyProjectileSlot projectile)
    {
        int speedMagnitude = Math.Abs(unchecked((short)projectile.Velocity.Y.RawValue));
        int accumulated = projectile.GeneralTimer + speedMagnitude;
        if (accumulated >= DownwardGateEnemyProjectileRomData.OneBlockDistance)
        {
            // The sleeping opcode is the current instruction pointer. Advancing two bytes
            // wakes the next visual frame on this same projectile-handler pass.
            projectile.InstructionTimer = NativeCountdown.Start(1);
            projectile.InstructionPointer = unchecked((ushort)(projectile.InstructionPointer + 2));
            accumulated = 0;
        }
        projectile.GeneralTimer = unchecked((ushort)accumulated);
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
    }

    /// <summary>Connects the bank-$86 moving-gate pre-instruction to the projectile handler table.</summary>
    internal static void RegisterDownwardGateProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunDownwardGateProjectileMovement(context.Projectile);
        }, EnemyProjectilePreInstruction.DownwardGateMovementPreInstruction);
    }
}
