using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The eye door species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class EyeDoorBehavior
{
    /// <summary>Provides the loaded room and random seed for smoke placement.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates eye-door projectile, sweat, and smoke slots.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds eye-door effects to a room and its enemy projectile pool.</summary>
    internal EyeDoorBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>Door-bit owner captured when a live eye projectile is spawned.</summary>
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
        _room.EnsureLoaded();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomWidthInBlocks);
        Ensure.NotNull(system);

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

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return; // Native SpawnEproj returns carry set when all eighteen slots are live.

        _eyeDoorProjectileSystem = system;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: 0);

        var block = new Position16<RoomBlocks>(
            checked((ushort)(request.PlmBlockIndex % roomWidthInBlocks)),
            checked((ushort)(request.PlmBlockIndex / roomWidthInBlocks)));
        switch (kind)
        {
            case RoomEnemyProjectileKind.EyeDoorProjectile:
                InitializeEyeDoorProjectile(projectile, request, block);
                return;

            case RoomEnemyProjectileKind.EyeDoorSweat:
                InitializeEyeDoorSweat(projectile, request.Parameter, block);
                return;

            case RoomEnemyProjectileKind.EyeDoorSmoke:
                InitializeEyeDoorSmoke(projectile, request.Parameter, block);
                return;

            default:
                throw new InvalidOperationException(
                    $"Eye-door projectile kind {kind} escaped its initializer dispatcher.");
        }
    }

    /// <summary>Positions the eye shot from its native block-relative origin pair and retains its door bit.</summary>
    private static void InitializeEyeDoorProjectile(
        RoomEnemyProjectileSlot projectile,
        EyeDoorProjectileRequest request,
        Position16<RoomBlocks> block)
    {
        int offsetWord = request.Parameter >> 1;
        if ((request.Parameter & 1) != 0 || offsetWord + 1 >= EyeDoorEnemyProjectileRomData.ProjectileOriginWordCount)
        {
            throw new InvalidDataException(
                $"Eye-door projectile parameter ${request.Parameter:X4} is outside " +
                "the cartridge origin-pair table.");
        }

        projectile.Variable1 = request.DoorBit;
        projectile.Position = projectile.Position.WithWhole(block.ToRoomPixels() + new Offset16<RoomPixels>(
            unchecked((short)(8 + EyeDoorEnemyProjectileRomData.ProjectileOriginWord(offsetWord))),
            EyeDoorEnemyProjectileRomData.ProjectileOriginWord(offsetWord + 1)));
    }

    /// <summary>Places sweat below the eye block and loads its native initial velocity pair.</summary>
    private static void InitializeEyeDoorSweat(
        RoomEnemyProjectileSlot projectile,
        ushort parameter,
        Position16<RoomBlocks> block)
    {
        int velocityWord = parameter >> 1;
        if ((parameter & 1) != 0 || velocityWord + 1 >= EyeDoorEnemyProjectileRomData.SweatVelocityWordCount)
        {
            throw new InvalidDataException(
                $"Eye-door sweat parameter ${parameter:X4} is outside " +
                "the cartridge velocity-pair table.");
        }

        // The sweat starts eight pixels left of the eye block, at the top of the block below it.
        projectile.Position = projectile.Position.WithWhole(
            block.ToRoomPixels() + new Offset16<RoomPixels>(-8, EyeDoorEnemyProjectileRomData.PixelsPerRoomBlock));
        projectile.Velocity = new EightEightVector<RoomPixels>(
            EyeDoorEnemyProjectileRomData.SweatVelocityWord(velocityWord),
            EyeDoorEnemyProjectileRomData.SweatVelocityWord(velocityWord + 1));
    }

    /// <summary>Uses one shared random sample for both smoke offsets, then advances the seed.</summary>
    internal void InitializeEyeDoorSmoke(
        RoomEnemyProjectileSlot projectile,
        ushort parameter,
        Position16<RoomBlocks> block)
    {
        byte listOffset = unchecked((byte)parameter);
        ushort instructionList = MiscDustProjectileDefinitions.InstructionList(listOffset);
        MiscDustPlacementDefinition placement =
            MiscDustProjectileDefinitions.SmokePlacement((ushort)(SnesLittleEndian.HighByte(parameter)));

        ushort random = _room.Random.Read();
        projectile.InstructionPointer = instructionList;

        projectile.Position = projectile.Position.WithWhole(block.ToRoomPixels() + new Offset16<RoomPixels>(
            unchecked((short)(8 + placement.Base.X + (random & placement.RandomMask.X))),
            unchecked((short)(8 + placement.Base.Y + ((SnesLittleEndian.HighByte(random)) & placement.RandomMask.Y)))));

        // $E4FA advances the global seed after sampling it for both coordinates.
        _room.Random.Generate();
    }

    /// <summary>Moves the eye shot, accelerates along its angle, and switches to impact on collision or door opening.</summary>
    internal void RunEyeDoorProjectilePreInstruction(
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
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.X.RawValue +
            ReadEyeDoorAcceleration(angle)))));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue +
            ReadEyeDoorAcceleration(angle - 64)))));

        Bank80SystemState system = _eyeDoorProjectileSystem ??
            throw new InvalidOperationException("A live eye-door projectile has no door-bit owner.");
        if (system.HasOpenedDoorBit(projectile.Variable1))
        {
            SetEyeDoorEffectImpactList(
                projectile,
                EyeDoorProjectileInstructionProgramDefinitions.Impact);
        }
    }

    /// <summary>Applies sweat gravity and enters the impact list on a downward vertical collision.</summary>
    internal static void RunEyeDoorSweatPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        MoveEyeDoorEffectHorizontally(projectile, level);
        bool collidedVertically = MoveEyeDoorEffectVertically(projectile, level);
        if (unchecked((short)projectile.Velocity.Y.RawValue) >= 0 && collidedVertically)
        {
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole - 4)));
            SetEyeDoorEffectImpactList(
                projectile,
                EyeDoorSweatInstructionProgramDefinitions.Impact);
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 12))));
    }

    /// <summary>Scales one signed sine-table sample to the eye shot's per-frame acceleration.</summary>
    internal static short ReadEyeDoorAcceleration(int tableIndex)
    {
        short sample = EnemyTrigonometryTables.SignedSine(unchecked((byte)tableIndex));
        return unchecked((short)(sample >> 4));
    }

    /// <summary>Stops projectile movement and restarts its impact animation at the first instruction.</summary>
    private static void SetEyeDoorEffectImpactList(
        RoomEnemyProjectileSlot projectile,
        ushort instructionList)
    {
        projectile.PreInstruction = (ushort)EnemyProjectilePreInstruction.EyeDoorSmokeInertPreInstruction;
        projectile.InstructionPointer = instructionList;
        projectile.InstructionTimer = 1;
    }

    /// <summary>Advances horizontal subpixels or snaps to the first solid leading edge.</summary>
    private static bool MoveEyeDoorEffectHorizontally(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        SnesFixedPosition moved = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta()).X;
        int edge = unchecked((short)projectile.Velocity.X.RawValue) < 0
            ? unchecked((ushort)(moved.Whole - projectile.Radius.X))
            : unchecked((ushort)(moved.Whole + projectile.Radius.X - 1));
        if (EyeDoorEffectEdgeIsSolid(
            level,
            edge,
            unchecked((ushort)(projectile.Position.Y.Whole - projectile.Radius.Y)),
            unchecked((ushort)(projectile.Position.Y.Whole + projectile.Radius.Y - 1)),
            verticalEdge: false))
        {
            projectile.Position = projectile.Position.WithFractionX(0);
            projectile.Position = projectile.Position.WithWholeX(SnapEnemyProjectileAxis(projectile.Position.X.Whole, edge, projectile.Radius.X,
                movingBack: unchecked((short)projectile.Velocity.X.RawValue) < 0));
            return true;
        }

        projectile.Position = projectile.Position.WithX(moved);
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

    /// <summary>Advances vertical subpixels or snaps to the first solid leading edge.</summary>
    private static bool MoveEyeDoorEffectVertically(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        SnesFixedPosition moved = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta()).Y;
        int edge = unchecked((short)projectile.Velocity.Y.RawValue) < 0
            ? unchecked((ushort)(moved.Whole - projectile.Radius.Y))
            : unchecked((ushort)(moved.Whole + projectile.Radius.Y - 1));
        if (EyeDoorEffectEdgeIsSolid(
            level,
            edge,
            unchecked((ushort)(projectile.Position.X.Whole - projectile.Radius.X)),
            unchecked((ushort)(projectile.Position.X.Whole + projectile.Radius.X - 1)),
            verticalEdge: true))
        {
            projectile.Position = projectile.Position.WithFractionY(0);
            projectile.Position = projectile.Position.WithWholeY(SnapEnemyProjectileAxis(projectile.Position.Y.Whole, edge, projectile.Radius.Y,
                movingBack: unchecked((short)projectile.Velocity.Y.RawValue) < 0));
            return true;
        }

        projectile.Position = projectile.Position.WithY(moved);
        return false;
    }

    /// <summary>Scans every touched room block along one projectile collision edge.</summary>
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
        int width = level.Size.X;
        int linear = verticalEdge ? fixedBlock * width + startBlock : startBlock * width + fixedBlock;
        int step = verticalEdge ? 1 : width;
        for (int tested = 0; tested <= extraBlocks; tested++, linear += step)
        {
            if (tested > 0 && linear >= width * level.Size.Y)
            {
                throw new InvalidDataException(
                    "Enemy projectile block scan ran past the room's level data into other WRAM.");
            }
            int blockX = tested == 0 ? (verticalEdge ? startBlock : fixedBlock) : linear % width;
            int blockY = tested == 0 ? (verticalEdge ? fixedBlock : startBlock) : linear / width;
            int blockIndex = EnemyTerrain.ResolveEnemyCollisionBlockIndex(level, new(unchecked((ushort)(blockX)), unchecked((ushort)(blockY))));
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

    /// <summary>Routes live eye shots and sweat through their distinct movement routines.</summary>
    internal void RegisterEyeDoorProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunEyeDoorProjectilePreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.EyeDoorProjectilePreInstruction);

        table.RegisterPreInstruction(context =>
        {
            RunEyeDoorSweatPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.EyeDoorSweatPreInstruction);
    }
}
