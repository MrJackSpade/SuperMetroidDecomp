namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 projectile half of Polyp's lava-rock attack.</summary>
public sealed partial class RoomEnemySystem
{
    private const ushort PolypRockPreInstruction =
        (ushort)EnemyProjectilePreInstruction.PolypRock;
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
        projectile.InstructionPointer = PolypRockInstructionProgramDefinitions.Initial;
        projectile.PreInstruction = PolypRockPreInstruction;
        projectile.Variable0 = (ushort)PolypRockFunction.Rising;
        projectile.Velocity = new(new SnesSignedEightEight(xVelocity), new SnesSignedEightEight(initialYSpeed));
        projectile.Position = source.Position;
    }

    /// <summary>Ports pre-instruction $86:BC0F and its rising/falling dispatcher.</summary>
    private static void RunPolypRockPreInstruction(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        switch (ClosedNativeWords.Decode<PolypRockFunction>(projectile.Variable0, "Polyp rock function"))
        {
            case PolypRockFunction.Rising:
                StepRisingPolypRock(projectile);
                break;
            case PolypRockFunction.Falling:
                StepFallingPolypRock(projectile);
                break;
            default:
                throw new InvalidDataException(
                    $"Polyp rock function $86:{projectile.Variable0:X4} is not translated.");
        }

        DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, camera);
    }

    private static void StepRisingPolypRock(RoomEnemyProjectileSlot projectile)
    {
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
            projectile.Velocity.Y.RawValue - PolypRockGravityStep))));
        if (unchecked((short)projectile.Velocity.Y.RawValue) < 0)
        {
            // Crossing below zero consumes an entire apex frame: retail installs falling
            // state and returns before either vertical or horizontal movement.
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(0));
            projectile.Variable0 = (ushort)PolypRockFunction.Falling;
            return;
        }

        // The native routine integrates two adjacent quadratic samples per frame, first
        // index velocity+1 and then velocity. This is why the table index changes by two
        // while the visual arc still advances at gameplay-frame rate.
        AddPolypRockQuadraticStep(
            projectile,
            unchecked((ushort)(projectile.Velocity.Y.RawValue + 1)),
            negative: true);
        AddPolypRockQuadraticStep(projectile, projectile.Velocity.Y.RawValue, negative: true);
        MovePolypRockHorizontally(projectile);
    }

    private static void StepFallingPolypRock(RoomEnemyProjectileSlot projectile)
    {
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
            projectile.Velocity.Y.RawValue + PolypRockGravityStep))));
        if (unchecked((short)(projectile.Velocity.Y.RawValue - PolypRockTerminalSpeedIndex)) >= 0)
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(PolypRockTerminalSpeedIndex));

        AddPolypRockQuadraticStep(
            projectile,
            unchecked((ushort)(projectile.Velocity.Y.RawValue - 1)),
            negative: false);
        AddPolypRockQuadraticStep(projectile, projectile.Velocity.Y.RawValue, negative: false);
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
        uint fractionalSum = (uint)projectile.Position.Y.Fraction + fraction;
        projectile.Position = projectile.Position.WithFractionY(unchecked((ushort)fractionalSum));
        if (fractionalSum > ushort.MaxValue)
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole + 1)));

        ushort whole = EnemyQuadraticSpeedDefinitions.ReadWord(record + 2);
        projectile.Variable1 = whole;
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole + whole)));
    }

    /// <summary>Ports $86:BCF4's split signed-8.8 horizontal addition.</summary>
    private static void MovePolypRockHorizontally(RoomEnemyProjectileSlot projectile)
    {
        projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());
    }
}

/// <summary>Polyp rock functions stored in the native Variable0 slot.</summary>
internal enum PolypRockFunction : ushort
{
    /// <summary>$86:BC16.</summary>
    Rising = 0xbc16,

    /// <summary>$86:BC8F.</summary>
    Falling = 0xbc8f,
}
