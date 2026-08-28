namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 projectile support coupled to Stoke's attack animation.</summary>
public sealed partial class RoomEnemySystem
{
    private const ushort StokeProjectilePreInstruction = 0xdb5b;
    private const ushort StokeProjectileMoveLeftFunction = 0xdb62;
    private const ushort StokeProjectileMoveRightFunction = 0xdb8c;

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
            ? StokeProjectileMoveLeftFunction
            : StokeProjectileMoveRightFunction;
        projectile.XPosition = stoke.XPosition;
        projectile.XSubposition = stoke.XSubposition;
        projectile.YPosition = unchecked((ushort)(stoke.YPosition + 2));
        projectile.YSubposition = stoke.YSubposition;
        projectile.YVelocity = 0xff00;
        projectile.XVelocity = 0x0100;
    }

    /// <summary>Ports pre-instruction <c>$86:DB5B</c> and its two indirect movers.</summary>
    private static void RunStokeProjectilePreInstruction(
        RoomEnemyProjectileSlot projectile,
        ushort cameraX,
        ushort cameraY)
    {
        ushort horizontalVelocity = projectile.Variable0 switch
        {
            // This is not a typo: $DB62 reads EnemyProjectile_YVelocity, producing -1 px.
            StokeProjectileMoveLeftFunction => projectile.YVelocity,
            StokeProjectileMoveRightFunction => projectile.XVelocity,
            _ => throw new NotSupportedException(
                $"Stoke projectile movement pointer $86:{projectile.Variable0:X4} is not translated."),
        };

        (projectile.XPosition, projectile.XSubposition) = AddEightBitVelocity(
            projectile.XPosition,
            projectile.XSubposition,
            horizontalVelocity);

        // Stoke's $DBB6 helper is another byte-for-byte clone of Cacatac's $DAC2 cull.
        DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, cameraX, cameraY);
    }
}
