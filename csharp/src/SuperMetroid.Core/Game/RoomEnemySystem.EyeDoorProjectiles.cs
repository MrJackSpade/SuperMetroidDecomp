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
            EyeDoorProjectileDefinition.Projectile =>
                RoomEnemyProjectileKind.EyeDoorProjectile,
            EyeDoorProjectileDefinition.Sweat =>
                RoomEnemyProjectileKind.EyeDoorSweat,
            EyeDoorProjectileDefinition.Smoke =>
                RoomEnemyProjectileKind.EyeDoorSmoke,
            _ => throw new InvalidOperationException(
                $"Undefined {nameof(EyeDoorProjectileDefinition)} {(int)request.DefinitionPointer:X4}."),
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
        if ((request.Parameter & 1) != 0 || offsetWord + 1 >= EyeDoorEnemyProjectileRomData.ProjectileOriginWordCount)
        {
            throw new InvalidDataException(
                $"Eye-door projectile parameter ${request.Parameter:X4} is outside " +
                "the cartridge origin-pair table.");
        }

        projectile.Variable1 = request.DoorBit;
        projectile.XPosition = unchecked((ushort)(
            blockX * EyeDoorEnemyProjectileRomData.PixelsPerRoomBlock + 8 +
            EyeDoorEnemyProjectileRomData.ProjectileOriginWord(offsetWord)));
        projectile.YPosition = unchecked((ushort)(
            blockY * EyeDoorEnemyProjectileRomData.PixelsPerRoomBlock +
            EyeDoorEnemyProjectileRomData.ProjectileOriginWord(offsetWord + 1)));
    }

    private static void InitializeEyeDoorSweat(
        RoomEnemyProjectileSlot projectile,
        ushort parameter,
        int blockX,
        int blockY)
    {
        int velocityWord = parameter >> 1;
        if ((parameter & 1) != 0 || velocityWord + 1 >= EyeDoorEnemyProjectileRomData.SweatVelocityWordCount)
        {
            throw new InvalidDataException(
                $"Eye-door sweat parameter ${parameter:X4} is outside " +
                "the cartridge velocity-pair table.");
        }

        projectile.XPosition = unchecked((ushort)(
            blockX * EyeDoorEnemyProjectileRomData.PixelsPerRoomBlock - 8));
        projectile.YPosition = unchecked((ushort)(
            (blockY + 1) * EyeDoorEnemyProjectileRomData.PixelsPerRoomBlock));
        projectile.XVelocity = unchecked((ushort)EyeDoorEnemyProjectileRomData.SweatVelocityWord(velocityWord));
        projectile.YVelocity = unchecked((ushort)EyeDoorEnemyProjectileRomData.SweatVelocityWord(velocityWord + 1));
    }

    private void InitializeEyeDoorSmoke(
        RoomEnemyProjectileSlot projectile,
        ushort parameter,
        int blockX,
        int blockY)
    {
        byte listOffset = unchecked((byte)parameter);
        ushort instructionList = MiscDustProjectileDefinitions.InstructionList(listOffset);
        MiscDustPlacementDefinition placement =
            MiscDustProjectileDefinitions.SmokePlacement((ushort)(parameter >> 8));

        ushort random = (_readRandomNumber ?? throw new InvalidOperationException(
            "Eye-door smoke initialization requires the current cartridge RNG word."))();
        projectile.InstructionPointer = instructionList;

        projectile.XPosition = unchecked((ushort)(
            blockX * EyeDoorEnemyProjectileRomData.PixelsPerRoomBlock + 8 +
            placement.XBase + (random & placement.XMask)));
        projectile.YPosition = unchecked((ushort)(
            blockY * EyeDoorEnemyProjectileRomData.PixelsPerRoomBlock + 8 +
            placement.YBase + ((random >> 8) & placement.YMask)));

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
                EyeDoorProjectileInstructionProgramDefinitions.Impact);
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
                EyeDoorProjectileInstructionProgramDefinitions.Impact);
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
                EyeDoorSweatInstructionProgramDefinitions.Impact);
            return;
        }

        projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + 12));
    }

    private static short ReadEyeDoorAcceleration(int tableIndex)
    {
        short sample = EnemyTrigonometryTables.SignedSine(unchecked((byte)tableIndex));
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
            projectile.XPosition = SnapEnemyProjectileAxis(projectile.XPosition, edge, projectile.XRadius,
                movingBack: unchecked((short)projectile.XVelocity) < 0);
            return true;
        }

        projectile.XPosition = position;
        projectile.XSubposition = subposition;
        return false;
    }

    /// <summary>
    /// The collision snap of $86:894F/$86:8A0D: against the blocking block's face, but only
    /// toward the move. Moving forward it never pulls the projectile back, and moving back it
    /// never pushes it forward, so a projectile that starts inside a block keeps its position.
    /// </summary>
    private static ushort SnapEnemyProjectileAxis(ushort current, int edge, ushort radius, bool movingBack)
    {
        if (movingBack)
        {
            ushort face = unchecked((ushort)((edge | 0x000f) + 1 + radius));
            return face <= current ? face : current;
        }
        ushort near = unchecked((ushort)((edge & 0xfff0) - radius));
        return near >= current ? near : current;
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
            projectile.YPosition = SnapEnemyProjectileAxis(projectile.YPosition, edge, projectile.YRadius,
                movingBack: unchecked((short)projectile.YVelocity) < 0);
            return true;
        }

        projectile.YPosition = position;
        projectile.YSubposition = subposition;
        return false;
    }

    /// <remarks>
    /// $86:88B6/$86:897B count the blocks to test as ((end) - (start &amp; $FFF0)) &gt;&gt; 4 in 16 bits
    /// and test one more than that, stepping the level index by a row (horizontal edge) or a
    /// block (vertical edge). A zero radius on a block boundary makes the count underflow to
    /// $0FFF, so the scan runs on through the room until it meets a solid block.
    /// </remarks>
    private static bool EyeDoorEffectEdgeIsSolid(
        RoomLevelData level,
        int fixedAxisPixel,
        int spanStartPixel,
        int spanEndPixel,
        bool verticalEdge)
    {
        int fixedBlock = unchecked((ushort)fixedAxisPixel) >> 4;
        int startBlock = unchecked((ushort)spanStartPixel) >> 4;
        int extraBlocks = unchecked((ushort)(spanEndPixel - (spanStartPixel & 0xfff0))) >> 4;
        int width = level.WidthInBlocks;
        int linear = verticalEdge ? fixedBlock * width + startBlock : startBlock * width + fixedBlock;
        int step = verticalEdge ? 1 : width;
        for (int tested = 0; tested <= extraBlocks; tested++, linear += step)
        {
            if (tested > 0 && linear >= width * level.HeightInBlocks)
            {
                throw new InvalidDataException(
                    "Enemy projectile block scan ran past the room's level data into other WRAM.");
            }
            int blockX = tested == 0 ? (verticalEdge ? startBlock : fixedBlock) : linear % width;
            int blockY = tested == 0 ? (verticalEdge ? fixedBlock : startBlock) : linear / width;
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
