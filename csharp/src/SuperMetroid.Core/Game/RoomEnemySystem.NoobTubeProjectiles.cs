using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 projectile initializers spawned by Maridia's n00b-tube PLM.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Executes the initializer selected by definitions $D904/$D912/$D920. The request
    /// retains the executing PLM block because native bank $86 reads the global PLM index
    /// while constructing each actor.
    /// </summary>
    public void SpawnNoobTubeProjectile(
        NoobTubeProjectileRequest request,
        int roomWidthInBlocks)
    {
        EnsureLoaded();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomWidthInBlocks);
        if (request.PlmBlockIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(request));

        RoomEnemyProjectileKind kind = request.DefinitionPointer switch
        {
            NoobTubePlmRomData.CrackProjectile => RoomEnemyProjectileKind.NoobTubeCrack,
            NoobTubePlmRomData.ShardProjectile => RoomEnemyProjectileKind.NoobTubeShard,
            NoobTubePlmRomData.ReleasedAirBubbleProjectile =>
                RoomEnemyProjectileKind.NoobTubeReleasedAirBubble,
            _ => throw new InvalidDataException(
                $"N00b tube requested unknown enemy projectile $86:{request.DefinitionPointer:X4}."),
        };

        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;
        InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: 0);

        int blockX = request.PlmBlockIndex % roomWidthInBlocks;
        int blockY = request.PlmBlockIndex / roomWidthInBlocks;
        switch (kind)
        {
            case RoomEnemyProjectileKind.NoobTubeCrack:
                if (request.Parameter != 0)
                    throw new InvalidDataException("N00b-tube crack parameter must be zero.");
                projectile.XPosition = unchecked((ushort)(
                    blockX * 16 + NoobTubeProjectileRomData.CrackXOffset));
                projectile.YPosition = unchecked((ushort)(
                    blockY * 16 + NoobTubeProjectileRomData.CrackYOffset));
                return;

            case RoomEnemyProjectileKind.NoobTubeShard:
            {
                int index = ValidateEvenNoobTubeParameter(
                    request.Parameter,
                    NoobTubeProjectileRomData.ShardXOffsets.Length,
                    "shard");
                projectile.Variable1 = unchecked((ushort)(
                    blockX * 16 + NoobTubeProjectileRomData.CrackXOffset +
                    NoobTubeProjectileRomData.ShardXOffsets[index]));
                projectile.Variable0 = 0;
                projectile.YPosition = unchecked((ushort)(
                    blockY * 16 + NoobTubeProjectileRomData.CrackYOffset +
                    NoobTubeProjectileRomData.ShardYOffsets[index]));
                projectile.InstructionPointer =
                    NoobTubeProjectileRomData.ShardInstructionLists[index];
                projectile.InstructionTimer = 1;
                projectile.XVelocity = unchecked((ushort)
                    NoobTubeProjectileRomData.ShardXVelocities[index]);
                projectile.YVelocity = unchecked((ushort)
                    NoobTubeProjectileRomData.ShardYVelocities[index]);
                return;
            }

            case RoomEnemyProjectileKind.NoobTubeReleasedAirBubble:
            {
                int index = ValidateEvenNoobTubeParameter(
                    request.Parameter,
                    NoobTubeProjectileRomData.BubbleXOffsets.Length,
                    "released-air bubble");
                projectile.Variable1 = unchecked((ushort)(
                    blockX * 16 + NoobTubeProjectileRomData.BubbleXOffsets[index]));
                projectile.Variable0 = 0;
                projectile.YPosition = unchecked((ushort)(
                    blockY * 16 + NoobTubeProjectileRomData.BubbleYOffsets[index]));
                projectile.YVelocity = NoobTubeProjectileRomData.BubbleInitialYVelocity;
                return;
            }

            default:
                throw new InvalidDataException(
                    $"N00b-tube initializer lost projectile kind {kind}.");
        }
    }

    private static int ValidateEvenNoobTubeParameter(
        ushort parameter,
        int entryCount,
        string family)
    {
        if ((parameter & 1) != 0 || parameter / 2 >= entryCount)
        {
            throw new InvalidDataException(
                $"N00b-tube {family} parameter ${parameter:X4} is outside its even table.");
        }
        return parameter / 2;
    }

    /// <summary>Runs the six movement callbacks referenced by the three projectile lists.</summary>
    private void RunNoobTubeProjectilePreInstruction(
        RoomEnemyProjectileSlot projectile,
        ushort cameraY)
    {
        switch (projectile.PreInstruction)
        {
            case EnemyProjectileCodePointers.PreInstruction_NoobTubeCrackFlickering:
                if (projectile.XPosition != NoobTubeProjectileRomData.HiddenXPosition)
                    projectile.Variable0 = projectile.XPosition;
                projectile.XPosition = (_currentEnemyProjectileFrame8 & 1) != 0
                    ? NoobTubeProjectileRomData.HiddenXPosition
                    : projectile.Variable0;
                return;

            case EnemyProjectileCodePointers.PreInstruction_NoobTubeCrackFalling:
                (projectile.YPosition, projectile.YSubposition) = AddEightBitVelocity(
                    projectile.YPosition,
                    projectile.YSubposition,
                    NoobTubeProjectileRomData.CrackFallVelocity);
                return;

            case EnemyProjectileCodePointers.PreInstruction_NoobTubeShardFlying:
                (projectile.Variable1, projectile.Variable0) = AddEightBitVelocity(
                    projectile.Variable1,
                    projectile.Variable0,
                    projectile.XVelocity);
                (projectile.YPosition, projectile.YSubposition) = AddEightBitVelocity(
                    projectile.YPosition,
                    projectile.YSubposition,
                    projectile.YVelocity);
                DeleteNoobTubeProjectileIfVerticallyOffscreen(projectile, cameraY);
                return;

            case EnemyProjectileCodePointers.PreInstruction_NoobTubeShardFalling:
                MoveNoobTubeProjectileHorizontallyAlongArc(projectile, angleStep: 2);
                (projectile.YPosition, projectile.YSubposition) = AddEightBitVelocity(
                    projectile.YPosition,
                    projectile.YSubposition,
                    projectile.YVelocity);
                DeleteNoobTubeProjectileIfVerticallyOffscreen(projectile, cameraY);
                return;

            case EnemyProjectileCodePointers.PreInstruction_NoobTubeBubbleFalling:
                // `$86:D89F` performs only the curved horizontal update before falling
                // through to `$D8DF`, which applies the bubble's vertical velocity once.
                MoveNoobTubeProjectileHorizontallyAlongArc(projectile, angleStep: 4);
                MoveNoobTubeBubbleVertically(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_NoobTubeBubbleFlying:
                MoveNoobTubeBubbleVertically(projectile);
                return;

            default:
                throw new InvalidDataException(
                    $"N00b-tube projectile has invalid pre-instruction $86:{projectile.PreInstruction:X4}.");
        }
    }

    private void MoveNoobTubeProjectileHorizontallyAlongArc(
        RoomEnemyProjectileSlot projectile,
        ushort angleStep)
    {
        ushort tableOffset = unchecked((ushort)((projectile.XVelocity & 0x01fe) | 0x0080));
        short sine = unchecked((short)ReadWord(
            _bus!,
            EnemyRomTablePointers.Common.SignedSineCosineWords + tableOffset));
        ushort horizontalVelocity = unchecked((ushort)(sine >> 2));
        (projectile.Variable1, projectile.Variable0) = AddEightBitVelocity(
            projectile.Variable1,
            projectile.Variable0,
            horizontalVelocity);
        projectile.XVelocity = unchecked((ushort)(projectile.XVelocity + angleStep));
    }

    private static void MoveNoobTubeBubbleVertically(RoomEnemyProjectileSlot projectile)
    {
        (projectile.YPosition, projectile.YSubposition) = AddEightBitVelocity(
            projectile.YPosition,
            projectile.YSubposition,
            projectile.YVelocity);
        projectile.XPosition = projectile.Variable1;
    }

    private static void DeleteNoobTubeProjectileIfVerticallyOffscreen(
        RoomEnemyProjectileSlot projectile,
        ushort cameraY)
    {
        if (unchecked((ushort)(projectile.YPosition - cameraY)) >= 256)
            projectile.Clear();
    }
}
