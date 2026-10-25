using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The typed programs Mother Brain projectiles start at spawn (#1395): the family's spawners initialize a projectile from its
/// definition and then start the program its kind names, as bank $86 starts the definition's list.
/// </summary>
internal static class MotherBrainProjectileStarts
{
    /// <summary>Each kind's initial program.</summary>
    private static readonly Dictionary<RoomEnemyProjectileKind, Func<EnemyProjectileAnimation>> InitialPrograms = new()
    {
        [RoomEnemyProjectileKind.MotherBrainTopRightTube] = () => new MotherBrainTopTubeAnimation(MotherBrainTopTubeProgram.TopRight),
        [RoomEnemyProjectileKind.MotherBrainTopLeftTube] = () => new MotherBrainTopTubeAnimation(MotherBrainTopTubeProgram.TopLeft),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube] = () => new MotherBrainTopTubeAnimation(MotherBrainTopTubeProgram.TopMiddleLeft),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube] = () => new MotherBrainTopTubeAnimation(MotherBrainTopTubeProgram.TopMiddleRight),
        [RoomEnemyProjectileKind.MotherBrainOnionRing] = () => new MotherBrainEffectAnimation(MotherBrainEffectProgram.OnionRing),
        [RoomEnemyProjectileKind.MotherBrainBomb] = () => new MotherBrainEffectAnimation(MotherBrainEffectProgram.Bomb),
        [RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging] = () => new MotherBrainEffectAnimation(MotherBrainEffectProgram.RainbowBeamCharging),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathBig] = () => new MotherBrainEffectAnimation(MotherBrainEffectProgram.PurpleBreathBig),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall] = () => new MotherBrainEffectAnimation(MotherBrainEffectProgram.PurpleBreathSmall),
        [RoomEnemyProjectileKind.MotherBrainDrool] = () => new MotherBrainEffectAnimation(MotherBrainEffectProgram.Drool),
        [RoomEnemyProjectileKind.MotherBrainDyingDrool] = () => new MotherBrainEffectAnimation(MotherBrainEffectProgram.Drool),
        [RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment] = () => new MotherBrainEffectAnimation(MotherBrainEffectProgram.EscapeDoorParticle),
        [RoomEnemyProjectileKind.MotherBrainEscapeSubtitle] = () => new MotherBrainEffectAnimation(MotherBrainEffectProgram.EscapeSubtitle),
    };

    /// <summary>The typed program a <paramref name="kind"/> projectile starts, or null when its spawner starts none.</summary>
    internal static EnemyProjectileAnimation? ForKind(RoomEnemyProjectileKind kind) =>
        InitialPrograms.TryGetValue(kind, out Func<EnemyProjectileAnimation>? start) ? start() : null;

    /// <summary>Initializes <paramref name="projectile"/> from its definition and starts the typed program its kind names.</summary>
    internal static void Initialize(RoomEnemyProjectileSlot projectile, RoomEnemyProjectileKind kind, int graphicsIndex)
    {
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, checked((ushort)graphicsIndex));
        if (ForKind(kind) is { } animation)
            projectile.StartAnimation(animation);
    }
}
