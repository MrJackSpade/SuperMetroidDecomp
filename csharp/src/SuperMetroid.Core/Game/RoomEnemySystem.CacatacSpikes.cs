namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 projectile half of Cacatac's five-spike attack.</summary>
public sealed partial class RoomEnemySystem
{
    private const ushort CacatacSpikePreInstruction = 0xd9db;
    private const int CacatacSpikeInstructionListTable = 0x86d96a;
    private const ushort CacatacSpikeCardinalYVelocity = 0xfe00;
    private const ushort CacatacSpikeCardinalXVelocity = 0x0200;
    private const ushort CacatacSpikeDiagonalYVelocity = 0xfe80;
    private const ushort CacatacSpikeDiagonalXVelocity = 0x0180;

    /// <summary>
    /// Ports <c>SpawnEnemyProjectileY_ParameterA_XGraphics</c> and initializer $86:D992.
    /// The source actor supplies both origin subpixels and its combined palette/tile index.
    /// </summary>
    private void SpawnCacatacSpike(RoomEnemySlot source, ushort rawDirection)
    {
        if ((rawDirection & 1) != 0 || rawDirection > (ushort)CacatacSpikeDirection.DownRight)
        {
            throw new InvalidDataException(
                $"Cacatac spike direction ${rawDirection:X4} is outside ten even table offsets.");
        }

        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CacatacSpike,
            unchecked((ushort)(source.PaletteIndex | source.VramTilesIndex)));
        projectile.DirectionParameter = rawDirection;
        projectile.Variable0 = rawDirection;
        projectile.InstructionPointer = ReadWord(
            _bus!,
            CacatacSpikeInstructionListTable + rawDirection);
        projectile.XPosition = source.XPosition;
        projectile.XSubposition = source.XSubposition;
        projectile.YPosition = source.YPosition;
        projectile.YSubposition = source.YSubposition;
        projectile.YVelocity = rawDirection >= (ushort)CacatacSpikeDirection.UpLeft
            ? CacatacSpikeDiagonalYVelocity
            : CacatacSpikeCardinalYVelocity;
        projectile.XVelocity = rawDirection >= (ushort)CacatacSpikeDirection.UpLeft
            ? CacatacSpikeDiagonalXVelocity
            : CacatacSpikeCardinalXVelocity;
    }

    /// <summary>Ports pre-instruction $86:D9DB and all ten movement-table entries.</summary>
    private static void RunCacatacSpikePreInstruction(
        RoomEnemyProjectileSlot projectile,
        ushort cameraX,
        ushort cameraY)
    {
        CacatacSpikeDirection direction = (CacatacSpikeDirection)projectile.Variable0;
        switch (direction)
        {
            case CacatacSpikeDirection.LeftFacingUp:
            case CacatacSpikeDirection.LeftFacingDown:
                MoveCacatacSpikeX(projectile, projectile.YVelocity);
                break;
            case CacatacSpikeDirection.Up:
                MoveCacatacSpikeY(projectile, projectile.YVelocity);
                break;
            case CacatacSpikeDirection.RightFacingUp:
            case CacatacSpikeDirection.RightFacingDown:
                MoveCacatacSpikeX(projectile, projectile.XVelocity);
                break;
            case CacatacSpikeDirection.Down:
                MoveCacatacSpikeY(projectile, projectile.XVelocity);
                break;
            case CacatacSpikeDirection.UpLeft:
                MoveCacatacSpikeX(projectile, projectile.YVelocity);
                MoveCacatacSpikeY(projectile, projectile.YVelocity);
                break;
            case CacatacSpikeDirection.UpRight:
                MoveCacatacSpikeX(projectile, projectile.XVelocity);
                MoveCacatacSpikeY(projectile, projectile.YVelocity);
                break;
            case CacatacSpikeDirection.DownLeft:
                MoveCacatacSpikeX(projectile, projectile.YVelocity);
                MoveCacatacSpikeY(projectile, projectile.XVelocity);
                break;
            case CacatacSpikeDirection.DownRight:
                MoveCacatacSpikeX(projectile, projectile.XVelocity);
                MoveCacatacSpikeY(projectile, projectile.XVelocity);
                break;
            default:
                throw new NotSupportedException(
                    $"Cacatac spike direction ${projectile.Variable0:X4} is not translated.");
        }

        DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, cameraX, cameraY);
    }

    private static void MoveCacatacSpikeX(
        RoomEnemyProjectileSlot projectile,
        ushort velocity)
    {
        (projectile.XPosition, projectile.XSubposition) = AddEightBitVelocity(
            projectile.XPosition,
            projectile.XSubposition,
            velocity);
    }

    private static void MoveCacatacSpikeY(
        RoomEnemyProjectileSlot projectile,
        ushort velocity)
    {
        (projectile.YPosition, projectile.YSubposition) = AddEightBitVelocity(
            projectile.YPosition,
            projectile.YSubposition,
            velocity);
    }

    /// <summary>
    /// Shared clone of bank-$86's signed four-CMP viewport cull. Equality with the right or
    /// bottom edge remains alive; only strictly outside coordinates delete the projectile.
    /// </summary>
    private static void DeleteEnemyProjectileIfOutsideInclusiveViewport(
        RoomEnemyProjectileSlot projectile,
        ushort cameraX,
        ushort cameraY)
    {
        ushort right = unchecked((ushort)(cameraX + 0x0100));
        ushort bottom = unchecked((ushort)(cameraY + 0x0100));
        bool outside = unchecked((short)(projectile.XPosition - cameraX)) < 0 ||
            unchecked((short)(right - projectile.XPosition)) < 0 ||
            unchecked((short)(projectile.YPosition - cameraY)) < 0 ||
            unchecked((short)(bottom - projectile.YPosition)) < 0;
        if (outside)
            projectile.Clear();
    }
}
