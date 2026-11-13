using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The typed programs Tourian statue projectiles start at spawn (#1395): the family's spawners initialize a projectile from its
/// definition and then start the program its kind names.
/// </summary>
internal static class TourianStatueProjectileStarts
{
    /// <summary>Each kind's initial program.</summary>
    private static readonly Dictionary<RoomEnemyProjectileKind, Func<EnemyProjectileAnimation>> InitialAnimations = new()
    {
        [RoomEnemyProjectileKind.TourianStatueSplash] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.Splash),
        [RoomEnemyProjectileKind.TourianStatueEyeGlow] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.EyeGlow),
        [RoomEnemyProjectileKind.TourianStatueParticle] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.Particle),
        [RoomEnemyProjectileKind.TourianStatueTail] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.Tail),
        [RoomEnemyProjectileKind.TourianStatueSoul] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.Soul),
        [RoomEnemyProjectileKind.TourianStatueRidley] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.Ridley),
        [RoomEnemyProjectileKind.TourianStatuePhantoon] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.Phantoon),
        [RoomEnemyProjectileKind.TourianStatueBaseDecoration] = () => new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.BaseDecoration),
    };

    /// <summary>The typed program a <paramref name="kind"/> projectile starts, or null when its spawner starts none.</summary>
    internal static EnemyProjectileAnimation? ForKind(RoomEnemyProjectileKind kind) =>
        InitialAnimations.TryGetValue(kind, out Func<EnemyProjectileAnimation>? start) ? start() : null;

    /// <summary>
    /// Whether a <paramref name="kind"/> projectile is one of the statue scene's, drawn in the scene's look (#1512):
    /// every kind these spawners start, and the descent's dust.
    /// </summary>
    internal static bool ShowsStatueScene(RoomEnemyProjectileKind kind) =>
        InitialAnimations.ContainsKey(kind) || kind == RoomEnemyProjectileKind.TourianStatueDescentDust;

    /// <summary>Initializes <paramref name="projectile"/> from its definition and starts the typed program its kind names.</summary>
    internal static void Initialize(EnemyProjectile projectile, RoomEnemyProjectileKind kind, SpriteGraphics graphics)
    {
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphics);
        if (ForKind(kind) is { } animation)
            projectile.StartAnimation(animation);
    }
}
