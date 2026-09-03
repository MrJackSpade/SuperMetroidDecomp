using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 actors spawned by the mirrored bank-$84 eye-door PLMs.</summary>
public sealed partial class RoomEnemySystem
{
    private Bank80SystemState? _eyeDoorProjectileSystem;

    /// <summary>
    /// Replays <c>SpawnEprojWithRoomGfx</c> and the definition-specific initializer selected
    /// by the eye-door PLM instruction. The PLM block index and room argument are retained
    /// because the cartridge initializers read both directly from their spawning PLM slot.
    /// </summary>
    public void SpawnEyeDoorProjectile(
        EyeDoorProjectileRequest request,
        int roomWidthInBlocks,
        Bank80SystemState system)
    {
        EnsureLoaded();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomWidthInBlocks);
        ArgumentNullException.ThrowIfNull(system);

        RoomEnemyProjectileKind kind = request.DefinitionPointer switch
        {
            EyeDoorEnemyProjectileRomData.ProjectileDefinition =>
                RoomEnemyProjectileKind.EyeDoorProjectile,
            EyeDoorEnemyProjectileRomData.SweatDefinition =>
                RoomEnemyProjectileKind.EyeDoorSweat,
            EyeDoorEnemyProjectileRomData.SmokeDefinition =>
                RoomEnemyProjectileKind.EyeDoorSmoke,
            _ => throw new InvalidDataException(
                $"Eye-door PLM requested unknown bank-$86 definition " +
                $"$86:{request.DefinitionPointer:X4}."),
        };

        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return; // Native SpawnEproj returns carry set when all eighteen slots are live.

        _eyeDoorProjectileSystem = system;
        InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: 0);

        int blockX = request.PlmBlockIndex % roomWidthInBlocks;
        int blockY = request.PlmBlockIndex / roomWidthInBlocks;
        switch (kind)
        {
            case RoomEnemyProjectileKind.EyeDoorProjectile:
                InitializeEyeDoorProjectile(projectile, request, blockX, blockY);
                return;

            case RoomEnemyProjectileKind.EyeDoorSweat:
                InitializeEyeDoorSweat(projectile, request.Parameter, blockX, blockY);
                return;

            case RoomEnemyProjectileKind.EyeDoorSmoke:
                InitializeEyeDoorSmoke(projectile, request.Parameter, blockX, blockY);
                return;

            default:
                throw new InvalidOperationException(
                    $"Eye-door projectile kind {kind} escaped its initializer dispatcher.");
        }
    }

    private static void InitializeEyeDoorProjectile(
        RoomEnemyProjectileSlot projectile,
        EyeDoorProjectileRequest request,
        int blockX,
        int blockY)
    {
        int offsetWord = request.Parameter >> 1;
        ReadOnlySpan<short> offsets = EyeDoorEnemyProjectileRomData.ProjectileOriginOffsets;
        if ((request.Parameter & 1) != 0 || offsetWord + 1 >= offsets.Length)
        {
            throw new InvalidDataException(
                $"Eye-door projectile parameter ${request.Parameter:X4} is outside " +
                "the cartridge origin-pair table.");
        }

        projectile.Variable1 = request.DoorBit;
        projectile.XPosition = unchecked((ushort)(
            blockX * EyeDoorEnemyProjectileRomData.PixelsPerRoomBlock + 8 +
            offsets[offsetWord]));
        projectile.YPosition = unchecked((ushort)(
            blockY * EyeDoorEnemyProjectileRomData.PixelsPerRoomBlock +
            offsets[offsetWord + 1]));
    }

    private static void InitializeEyeDoorSweat(
        RoomEnemyProjectileSlot projectile,
        ushort parameter,
        int blockX,
        int blockY)
    {
        int velocityWord = parameter >> 1;
        ReadOnlySpan<short> velocities = EyeDoorEnemyProjectileRomData.SweatVelocities;
        if ((parameter & 1) != 0 || velocityWord + 1 >= velocities.Length)
        {
            throw new InvalidDataException(
                $"Eye-door sweat parameter ${parameter:X4} is outside " +
                "the cartridge velocity-pair table.");
        }

        projectile.XPosition = unchecked((ushort)(
            blockX * EyeDoorEnemyProjectileRomData.PixelsPerRoomBlock - 8));
        projectile.YPosition = unchecked((ushort)(
            (blockY + 1) * EyeDoorEnemyProjectileRomData.PixelsPerRoomBlock));
        projectile.XVelocity = unchecked((ushort)velocities[velocityWord]);
        projectile.YVelocity = unchecked((ushort)velocities[velocityWord + 1]);
    }

    private void InitializeEyeDoorSmoke(
        RoomEnemyProjectileSlot projectile,
        ushort parameter,
        int blockX,
        int blockY)
    {
        byte listOffset = unchecked((byte)parameter);
        projectile.InstructionPointer = ReadWord(
            _bus!,
            EyeDoorEnemyProjectileRomData.BankBase | unchecked((ushort)(
                EyeDoorEnemyProjectileRomData.SmokeInstructionListTable + listOffset * 2)));

        ushort random = (_readRandomNumber ?? throw new InvalidOperationException(
            "Eye-door smoke initialization requires the current cartridge RNG word."))();
        int tableWord = (parameter >> 8) * 4;
        int tableAddress = EyeDoorEnemyProjectileRomData.BankBase | unchecked((ushort)(
            EyeDoorEnemyProjectileRomData.SmokeOffsetTable + tableWord * 2));
        ushort xMask = ReadWord(_bus!, tableAddress);
        ushort yMask = ReadWord(_bus!, tableAddress + 2);
        short xBase = unchecked((short)ReadWord(_bus!, tableAddress + 4));
        short yBase = unchecked((short)ReadWord(_bus!, tableAddress + 6));

        projectile.XPosition = unchecked((ushort)(
            blockX * EyeDoorEnemyProjectileRomData.PixelsPerRoomBlock + 8 +
            xBase + (random & xMask)));
        projectile.YPosition = unchecked((ushort)(
            blockY * EyeDoorEnemyProjectileRomData.PixelsPerRoomBlock + 8 +
            yBase + ((random >> 8) & yMask)));

        // $E4FA advances the global seed after sampling it for both coordinates.
        (_nextRandom ?? throw new InvalidOperationException(
            "Eye-door smoke initialization requires the cartridge RNG generator."))();
    }

    private void RunEyeDoorProjectilePreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveEyeDoorEffectHorizontally(projectile, level) ||
            MoveEyeDoorEffectVertically(projectile, level))
        {
            SetEyeDoorEffectImpactList(
                projectile,
                EyeDoorEnemyProjectileRomData.ProjectileImpactInstructionList);
            return;
        }

        int angle = projectile.Variable0 >> 1;
        projectile.XVelocity = unchecked((ushort)(projectile.XVelocity +
            ReadEyeDoorAcceleration(angle)));
        projectile.YVelocity = unchecked((ushort)(projectile.YVelocity +
            ReadEyeDoorAcceleration(angle - 64)));

        Bank80SystemState system = _eyeDoorProjectileSystem ??
            throw new InvalidOperationException("A live eye-door projectile has no door-bit owner.");
        if (system.HasOpenedDoorBit(projectile.Variable1))
        {
            SetEyeDoorEffectImpactList(
                projectile,
                EyeDoorEnemyProjectileRomData.ProjectileImpactInstructionList);
        }
    }

    private static void RunEyeDoorSweatPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        MoveEyeDoorEffectHorizontally(projectile, level);
        bool collidedVertically = MoveEyeDoorEffectVertically(projectile, level);
        if (unchecked((short)projectile.YVelocity) >= 0 && collidedVertically)
        {
            projectile.YPosition = unchecked((ushort)(projectile.YPosition - 4));
            SetEyeDoorEffectImpactList(
                projectile,
                EyeDoorEnemyProjectileRomData.SweatImpactInstructionList);
            return;
        }

        projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + 12));
    }

    private short ReadEyeDoorAcceleration(int tableIndex)
    {
        short sample = unchecked((short)ReadWord(
            _bus!,
            EnemyRomTablePointers.Common.SignedSineCosineWords + ((tableIndex & 0xff) * 2)));
        return unchecked((short)(sample >> 4));
    }

    private static void SetEyeDoorEffectImpactList(
        RoomEnemyProjectileSlot projectile,
        ushort instructionList)
    {
        projectile.PreInstruction = EyeDoorEnemyProjectileRomData.SmokeInertPreInstruction;
        projectile.InstructionPointer = instructionList;
        projectile.InstructionTimer = 1;
    }

    private static bool MoveEyeDoorEffectHorizontally(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        (ushort position, ushort subposition) = AddEightBitVelocity(
            projectile.XPosition,
            projectile.XSubposition,
            projectile.XVelocity);
        int edge = unchecked((short)projectile.XVelocity) < 0
            ? unchecked((ushort)(position - projectile.XRadius))
            : unchecked((ushort)(position + projectile.XRadius - 1));
        if (EyeDoorEffectEdgeIsSolid(
            level,
            edge,
            unchecked((ushort)(projectile.YPosition - projectile.YRadius)),
            unchecked((ushort)(projectile.YPosition + projectile.YRadius - 1)),
            verticalEdge: false))
        {
            projectile.XSubposition = 0;
            projectile.XPosition = unchecked((short)projectile.XVelocity) < 0
                ? unchecked((ushort)(projectile.XRadius + (edge | 0x000f) + 1))
                : unchecked((ushort)((edge & 0xfff0) - projectile.XRadius));
            return true;
        }

        projectile.XPosition = position;
        projectile.XSubposition = subposition;
        return false;
    }

    private static bool MoveEyeDoorEffectVertically(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        (ushort position, ushort subposition) = AddEightBitVelocity(
            projectile.YPosition,
            projectile.YSubposition,
            projectile.YVelocity);
        int edge = unchecked((short)projectile.YVelocity) < 0
            ? unchecked((ushort)(position - projectile.YRadius))
            : unchecked((ushort)(position + projectile.YRadius - 1));
        if (EyeDoorEffectEdgeIsSolid(
            level,
            edge,
            unchecked((ushort)(projectile.XPosition - projectile.XRadius)),
            unchecked((ushort)(projectile.XPosition + projectile.XRadius - 1)),
            verticalEdge: true))
        {
            projectile.YSubposition = 0;
            projectile.YPosition = unchecked((short)projectile.YVelocity) < 0
                ? unchecked((ushort)(projectile.YRadius + (edge | 0x000f) + 1))
                : unchecked((ushort)((edge & 0xfff0) - projectile.YRadius));
            return true;
        }

        projectile.YPosition = position;
        projectile.YSubposition = subposition;
        return false;
    }

    private static bool EyeDoorEffectEdgeIsSolid(
        RoomLevelData level,
        int fixedAxisPixel,
        int spanStartPixel,
        int spanEndPixel,
        bool verticalEdge)
    {
        int fixedBlock = unchecked((ushort)fixedAxisPixel) >> 4;
        int startBlock = unchecked((ushort)spanStartPixel) >> 4;
        int endBlock = unchecked((ushort)spanEndPixel) >> 4;
        for (int spanBlock = startBlock; spanBlock <= endBlock; spanBlock++)
        {
            int blockX = verticalEdge ? spanBlock : fixedBlock;
            int blockY = verticalEdge ? fixedBlock : spanBlock;
            int blockIndex = ResolveEnemyCollisionBlockIndex(level, blockX, blockY);
            if (blockIndex < 0)
                return true;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            if (block.CollisionType is not (
                RoomCollisionType.Air or
                RoomCollisionType.SpikeAir or
                RoomCollisionType.SpecialAir or
                RoomCollisionType.ShootableAir or
                RoomCollisionType.UnusedAir or
                RoomCollisionType.BombableAir or
                RoomCollisionType.HorizontalExtension or
                RoomCollisionType.VerticalExtension or
                RoomCollisionType.GrappleBlock))
            {
                return true;
            }
        }
        return false;
    }
}
