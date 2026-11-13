using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The typed animations Mother Brain projectiles start at spawn (#1395): the family's spawners initialize a projectile from its
/// definition and then start the animation its kind names.
/// </summary>
internal static class MotherBrainProjectileStarts
{
    /// <summary>Each kind's initial animation.</summary>
    private static readonly Dictionary<RoomEnemyProjectileKind, Func<EnemyProjectileAnimation>> InitialAnimations = new()
    {
        [RoomEnemyProjectileKind.MotherBrainTopRightTube] = () => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopRight),
        [RoomEnemyProjectileKind.MotherBrainTopLeftTube] = () => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopLeft),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube] = () => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopMiddleLeft),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube] = () => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopMiddleRight),
        [RoomEnemyProjectileKind.MotherBrainOnionRing] = () => new MotherBrainEffectAnimation(MotherBrainEffectSegment.OnionRing),
        [RoomEnemyProjectileKind.MotherBrainBomb] = () => new MotherBrainEffectAnimation(MotherBrainEffectSegment.Bomb),
        [RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging] = () => new MotherBrainEffectAnimation(MotherBrainEffectSegment.RainbowBeamCharging),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathBig] = () => new MotherBrainEffectAnimation(MotherBrainEffectSegment.PurpleBreathBig),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall] = () => new MotherBrainEffectAnimation(MotherBrainEffectSegment.PurpleBreathSmall),
        [RoomEnemyProjectileKind.MotherBrainDrool] = () => new MotherBrainEffectAnimation(MotherBrainEffectSegment.Drool),
        [RoomEnemyProjectileKind.MotherBrainDyingDrool] = () => new MotherBrainEffectAnimation(MotherBrainEffectSegment.Drool),
        [RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment] = () => new MotherBrainEffectAnimation(MotherBrainEffectSegment.EscapeDoorParticle),
        [RoomEnemyProjectileKind.MotherBrainEscapeSubtitle] = () => new MotherBrainEffectAnimation(MotherBrainEffectSegment.EscapeSubtitle),
    };

    /// <summary>The typed animation a <paramref name="kind"/> projectile starts, or null when its spawner starts none.</summary>
    internal static EnemyProjectileAnimation? ForKind(RoomEnemyProjectileKind kind) =>
        InitialAnimations.TryGetValue(kind, out Func<EnemyProjectileAnimation>? start) ? start() : null;

    /// <summary>Initializes <paramref name="projectile"/> from its definition and starts the typed animation its kind names.</summary>
    internal static void Initialize(EnemyProjectile projectile, RoomEnemyProjectileKind kind, SpriteGraphics graphics)
    {
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphics);
        if (ForKind(kind) is { } animation)
            projectile.StartAnimation(animation);
    }
}
