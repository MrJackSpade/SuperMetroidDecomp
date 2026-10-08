namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Spawns enemy projectile $86:B4B1 for PLM pre-instruction $84:B927. Its initializer
    /// ($86:B49D) ignores the spawn parameter and places the explosion on the middle of the
    /// breaking wall, saving that point in Var0/Var1.
    /// </summary>
    public void SpawnOldTourianEscapeShaftWallExplosion()
    {
        EnsureLoaded();
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return; // Native SpawnEproj returns carry set when all eighteen slots are live.

        // SpawnEnemyProjectileY_ParameterA_RoomGraphics uses graphics index zero.
        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion,
            graphicsIndex: 0);
        projectile.XPosition = OldTourianEscapeShaftWallExplosionDefinitions.XPosition;
        projectile.Variable0 = OldTourianEscapeShaftWallExplosionDefinitions.XPosition;
        projectile.YPosition = OldTourianEscapeShaftWallExplosionDefinitions.YPosition;
        projectile.Variable1 = OldTourianEscapeShaftWallExplosionDefinitions.YPosition;
    }
}
