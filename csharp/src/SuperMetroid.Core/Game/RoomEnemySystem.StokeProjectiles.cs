namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 projectile support coupled to Stoke's attack animation.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Ports <c>SpawnEnemyProjectileY_ParameterA_XGraphics</c> followed by Stoke projectile
    /// initializer <c>$86:DB18</c>. Direction zero selects the intentionally odd left branch,
    /// which reads the signed speed from Y velocity while still moving on the X axis.
    /// </summary>
    private void SpawnStokeProjectile(RoomEnemySlot stoke, ushort direction)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.StokeProjectile,
            unchecked((ushort)(stoke.PaletteIndex | stoke.VramTilesIndex)));
        projectile.DirectionParameter = direction;
        projectile.Variable0 = direction == 0
            ? (ushort)StokeProjectileFunction.MoveLeft
            : (ushort)StokeProjectileFunction.MoveRight;
        projectile.Position = new(new(stoke.Position.X.Whole, stoke.Position.X.Fraction), new(unchecked((ushort)(stoke.Position.Y.Whole + 2)), stoke.Position.Y.Fraction));
        projectile.Velocity = new(new SnesSignedEightEight(0x0100), new SnesSignedEightEight(0xff00));
    }

    /// <summary>Ports pre-instruction <c>$86:DB5B</c> and its two indirect movers.</summary>
    private static void RunStokeProjectilePreInstruction(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        ushort horizontalVelocity = ClosedNativeWords.Decode<StokeProjectileFunction>(projectile.Variable0, "Stoke projectile movement function") switch
        {
            // This is not a typo: $DB62 reads EnemyProjectile_YVelocity, producing -1 px.
            StokeProjectileFunction.MoveLeft => projectile.Velocity.Y.RawValue,
            StokeProjectileFunction.MoveRight => projectile.Velocity.X.RawValue,
            _ => throw new InvalidDataException(
                $"Stoke projectile movement pointer $86:{projectile.Variable0:X4} is not translated."),
        };

        projectile.Position = projectile.Position.AdvanceX(new SnesSignedEightEight(horizontalVelocity).ToSixteenSixteenDelta());

        // Stoke's $DBB6 helper is another byte-for-byte clone of Cacatac's $DAC2 cull.
        DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, camera);
    }
}

/// <summary>Stoke projectile movement functions stored in the native Variable0 slot.</summary>
internal enum StokeProjectileFunction : ushort
{
    /// <summary>$86:DB62.</summary>
    MoveLeft = 0xdb62,

    /// <summary>$86:DB8C.</summary>
    MoveRight = 0xdb8c,
}
