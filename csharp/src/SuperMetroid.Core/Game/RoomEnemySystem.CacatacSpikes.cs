namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 projectile half of Cacatac's five-spike attack.</summary>
public sealed partial class RoomEnemySystem
{

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
        var direction = (CacatacSpikeDirection)rawDirection;
        projectile.InstructionPointer = CacatacProjectileDefinitions.InstructionList(direction);
        projectile.Position = source.Position;
        CacatacSpikeSpeedPair speeds = CacatacProjectileDefinitions.SpeedPair(direction);
        projectile.Velocity = new(new SnesSignedEightEight(speeds.Positive), new SnesSignedEightEight(speeds.Negative));
    }

    /// <summary>
    /// Ports pre-instruction $86:D9DB and the ten callback pointers at
    /// $86:D97E-$86:D991, selected by even direction bytes $00..$12.
    /// In selector order the native callbacks are $DA8E, $DA98, $DA93,
    /// $DA8E, $DA9D, $DA93, $DAA2, $DAB2, $DAAA, $DABA. The two left-facing
    /// and two right-facing variants share callbacks; diagonals compose one
    /// horizontal and one vertical signed-speed move. The native
    /// pre-instruction culls the projectile after its selected movement.
    /// These authored callback addresses do not form a uniform pointer stride.
    /// </summary>
    private static void RunCacatacSpikePreInstruction(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        CacatacSpikeDirection direction = (CacatacSpikeDirection)projectile.Variable0;
        switch (direction)
        {
            case CacatacSpikeDirection.LeftFacingUp:
            case CacatacSpikeDirection.LeftFacingDown:
                MoveCacatacSpikeX(projectile, projectile.Velocity.Y.RawValue);
                break;
            case CacatacSpikeDirection.Up:
                MoveCacatacSpikeY(projectile, projectile.Velocity.Y.RawValue);
                break;
            case CacatacSpikeDirection.RightFacingUp:
            case CacatacSpikeDirection.RightFacingDown:
                MoveCacatacSpikeX(projectile, projectile.Velocity.X.RawValue);
                break;
            case CacatacSpikeDirection.Down:
                MoveCacatacSpikeY(projectile, projectile.Velocity.X.RawValue);
                break;
            case CacatacSpikeDirection.UpLeft:
                MoveCacatacSpikeX(projectile, projectile.Velocity.Y.RawValue);
                MoveCacatacSpikeY(projectile, projectile.Velocity.Y.RawValue);
                break;
            case CacatacSpikeDirection.UpRight:
                MoveCacatacSpikeX(projectile, projectile.Velocity.X.RawValue);
                MoveCacatacSpikeY(projectile, projectile.Velocity.Y.RawValue);
                break;
            case CacatacSpikeDirection.DownLeft:
                MoveCacatacSpikeX(projectile, projectile.Velocity.Y.RawValue);
                MoveCacatacSpikeY(projectile, projectile.Velocity.X.RawValue);
                break;
            case CacatacSpikeDirection.DownRight:
                MoveCacatacSpikeX(projectile, projectile.Velocity.X.RawValue);
                MoveCacatacSpikeY(projectile, projectile.Velocity.X.RawValue);
                break;
            default:
                throw new InvalidDataException(
                    $"Cacatac spike direction ${projectile.Variable0:X4} is not translated.");
        }

        DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, camera);
    }

    private static void MoveCacatacSpikeX(
        RoomEnemyProjectileSlot projectile,
        ushort velocity)
    {
        projectile.Position = projectile.Position.AdvanceX(new SnesSignedEightEight(velocity).ToSixteenSixteenDelta());
    }

    private static void MoveCacatacSpikeY(
        RoomEnemyProjectileSlot projectile,
        ushort velocity)
    {
        projectile.Position = projectile.Position.AdvanceY(new SnesSignedEightEight(velocity).ToSixteenSixteenDelta());
    }

    /// <summary>
    /// Shared clone of bank-$86's signed four-CMP viewport cull. Equality with the right or
    /// bottom edge remains alive; only strictly outside coordinates delete the projectile.
    /// </summary>
    private static void DeleteEnemyProjectileIfOutsideInclusiveViewport(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        ushort right = unchecked((ushort)(camera.X + 0x0100));
        ushort bottom = unchecked((ushort)(camera.Y + 0x0100));
        bool outside = unchecked((short)(projectile.Position.X.Whole - camera.X)) < 0 ||
            unchecked((short)(right - projectile.Position.X.Whole)) < 0 ||
            unchecked((short)(projectile.Position.Y.Whole - camera.Y)) < 0 ||
            unchecked((short)(bottom - projectile.Position.Y.Whole)) < 0;
        if (outside)
            projectile.Clear();
    }
}
