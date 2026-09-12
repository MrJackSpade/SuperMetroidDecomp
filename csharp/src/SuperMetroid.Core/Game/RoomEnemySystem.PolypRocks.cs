namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 projectile half of Polyp's lava-rock attack.</summary>
public sealed partial class RoomEnemySystem
{
    private const ushort PolypRockPreInstruction =
        EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_PolypRock;
    private const ushort PolypRockInstructionList = 0xbbd5;
    private const ushort PolypRockRisingFunction = 0xbc16;
    private const ushort PolypRockFallingFunction = 0xbc8f;
    private const ushort PolypRockGravityStep = 2;
    private const ushort PolypRockTerminalSpeedIndex = 0x0040;

    /// <summary>
    /// Ports <c>SpawnEnemyProjectileY_ParameterA_XGraphics</c> plus initializer $86:BBDB.
    /// The actor's palette/tile word and all four position halves are copied at allocation.
    /// </summary>
    private void SpawnPolypRock(
        RoomEnemySlot source,
        ushort initialYSpeed,
        ushort xVelocity)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.PolypRock,
            unchecked((ushort)(source.PaletteIndex | source.VramTilesIndex)));
        projectile.InstructionPointer = PolypRockInstructionList;
        projectile.PreInstruction = PolypRockPreInstruction;
        projectile.Variable0 = PolypRockRisingFunction;
        projectile.YVelocity = initialYSpeed;
        projectile.XVelocity = xVelocity;
        projectile.XPosition = source.XPosition;
        projectile.XSubposition = source.XSubposition;
        projectile.YPosition = source.YPosition;
        projectile.YSubposition = source.YSubposition;
    }

    /// <summary>Ports pre-instruction $86:BC0F and its rising/falling dispatcher.</summary>
    private static void RunPolypRockPreInstruction(
        RoomEnemyProjectileSlot projectile,
        ushort cameraX,
        ushort cameraY)
    {
        switch (projectile.Variable0)
        {
            case PolypRockRisingFunction:
                StepRisingPolypRock(projectile);
                break;
            case PolypRockFallingFunction:
                StepFallingPolypRock(projectile);
                break;
            default:
                throw new InvalidDataException(
                    $"Polyp rock function $86:{projectile.Variable0:X4} is not translated.");
        }

        DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, cameraX, cameraY);
    }

    private static void StepRisingPolypRock(RoomEnemyProjectileSlot projectile)
    {
        projectile.YVelocity = unchecked((ushort)(
            projectile.YVelocity - PolypRockGravityStep));
        if (unchecked((short)projectile.YVelocity) < 0)
        {
            // Crossing below zero consumes an entire apex frame: retail installs falling
            // state and returns before either vertical or horizontal movement.
            projectile.YVelocity = 0;
            projectile.Variable0 = PolypRockFallingFunction;
            return;
        }

        // The native routine integrates two adjacent quadratic samples per frame, first
        // index velocity+1 and then velocity. This is why the table index changes by two
        // while the visual arc still advances at gameplay-frame rate.
        AddPolypRockQuadraticStep(
            projectile,
            unchecked((ushort)(projectile.YVelocity + 1)),
            negative: true);
        AddPolypRockQuadraticStep(projectile, projectile.YVelocity, negative: true);
        MovePolypRockHorizontally(projectile);
    }

    private static void StepFallingPolypRock(RoomEnemyProjectileSlot projectile)
    {
        projectile.YVelocity = unchecked((ushort)(
            projectile.YVelocity + PolypRockGravityStep));
        if (unchecked((short)(projectile.YVelocity - PolypRockTerminalSpeedIndex)) >= 0)
            projectile.YVelocity = PolypRockTerminalSpeedIndex;

        AddPolypRockQuadraticStep(
            projectile,
            unchecked((ushort)(projectile.YVelocity - 1)),
            negative: false);
        AddPolypRockQuadraticStep(projectile, projectile.YVelocity, negative: false);
        MovePolypRockHorizontally(projectile);
    }

    /// <summary>
    /// Adds one positive or cartridge-stored negative 16.16 record from $A0:CBC7. Variable
    /// one is real scratch WRAM: after each sample it contains the whole word because the
    /// assembly overwrites its earlier fractional copy before returning.
    /// </summary>
    private static void AddPolypRockQuadraticStep(
        RoomEnemyProjectileSlot projectile,
        ushort tableIndex,
        bool negative)
    {
        if (tableIndex >= EnemyQuadraticSpeedDefinitions.RecordCount)
        {
            throw new InvalidDataException(
                $"Polyp rock quadratic speed index ${tableIndex:X4} exceeds $005E.");
        }

        int record = tableIndex * EnemyQuadraticSpeedDefinitions.RecordSize +
            (negative ? 4 : 0);
        ushort fraction = EnemyQuadraticSpeedDefinitions.ReadWord(record);
        projectile.Variable1 = fraction;
        uint fractionalSum = (uint)projectile.YSubposition + fraction;
        projectile.YSubposition = unchecked((ushort)fractionalSum);
        if (fractionalSum > ushort.MaxValue)
            projectile.YPosition = unchecked((ushort)(projectile.YPosition + 1));

        ushort whole = EnemyQuadraticSpeedDefinitions.ReadWord(record + 2);
        projectile.Variable1 = whole;
        projectile.YPosition = unchecked((ushort)(projectile.YPosition + whole));
    }

    /// <summary>Ports $86:BCF4's split signed-8.8 horizontal addition.</summary>
    private static void MovePolypRockHorizontally(RoomEnemyProjectileSlot projectile)
    {
        (projectile.XPosition, projectile.XSubposition) = AddEightBitVelocity(
            projectile.XPosition,
            projectile.XSubposition,
            projectile.XVelocity);
    }
}
