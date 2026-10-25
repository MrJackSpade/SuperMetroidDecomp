using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The typed programs Tourian statue projectiles start at spawn (#1395): the family's spawners initialize a projectile from its
/// definition and then start the program its kind names, as bank $86 starts the definition's list.
/// </summary>
internal static class TourianStatueProjectileStarts
{
    /// <summary>Each kind's initial program.</summary>
    private static readonly Dictionary<RoomEnemyProjectileKind, Func<EnemyProjectileAnimation>> InitialPrograms = new()
    {
        [RoomEnemyProjectileKind.TourianStatueSplash] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileProgram.Splash),
        [RoomEnemyProjectileKind.TourianStatueEyeGlow] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileProgram.EyeGlow),
        [RoomEnemyProjectileKind.TourianStatueParticle] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileProgram.Particle),
        [RoomEnemyProjectileKind.TourianStatueTail] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileProgram.Tail),
        [RoomEnemyProjectileKind.TourianStatueSoul] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileProgram.Soul),
        [RoomEnemyProjectileKind.TourianStatueRidley] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileProgram.Ridley),
        [RoomEnemyProjectileKind.TourianStatuePhantoon] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileProgram.Phantoon),
        [RoomEnemyProjectileKind.TourianStatueBaseDecoration] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileProgram.BaseDecoration),
    };

    /// <summary>The typed program a <paramref name="kind"/> projectile starts, or null when its spawner starts none.</summary>
    internal static EnemyProjectileAnimation? ForKind(RoomEnemyProjectileKind kind) =>
        InitialPrograms.TryGetValue(kind, out Func<EnemyProjectileAnimation>? start) ? start() : null;

    /// <summary>Initializes <paramref name="projectile"/> from its definition and starts the typed program its kind names.</summary>
    internal static void Initialize(RoomEnemyProjectileSlot projectile, RoomEnemyProjectileKind kind, SpriteGraphics graphics)
    {
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphics);
        if (ForKind(kind) is { } animation)
            projectile.StartAnimation(animation);
    }
}
