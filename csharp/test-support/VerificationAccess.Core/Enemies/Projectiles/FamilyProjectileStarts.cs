using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Verification's initialization of a projectile of any kind (#1395): the families whose spawners start their own typed
/// programs initialize through their own starts, and every other kind through the enemy projectile engine.
/// </summary>
internal static class FamilyProjectileStarts
{
    /// <summary>Initializes <paramref name="projectile"/> as its kind's production spawner does.</summary>
    internal static void Initialize(RoomEnemyProjectileSlot projectile, RoomEnemyProjectileKind kind, SpriteGraphics graphics)
    {
        if (TourianStatueProjectileStarts.ForKind(kind) is not null)
            TourianStatueProjectileStarts.Initialize(projectile, kind, graphics);
        else if (MotherBrainProjectileStarts.ForKind(kind) is not null)
            MotherBrainProjectileStarts.Initialize(projectile, kind, graphics);
        else
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphics);
    }
}
