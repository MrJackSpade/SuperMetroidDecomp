using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The four particles a Skree or Metaree bursts into as it burrows or dies (#1435), one per direction. Each enemy names its
/// own, so the debris it spawns is its own and draws from its own spawned-object sheet.
/// </summary>
/// <param name="OuterRight">The outer particle flying right.</param>
/// <param name="InnerRight">The inner particle flying right.</param>
/// <param name="OuterLeft">The outer particle flying left.</param>
/// <param name="InnerLeft">The inner particle flying left.</param>
/// <param name="Look">The enemy's own look, which the particles' animation shows.</param>
internal sealed record MetalParticleBurst(RoomEnemyProjectileKind OuterRight, RoomEnemyProjectileKind InnerRight,
    RoomEnemyProjectileKind OuterLeft, RoomEnemyProjectileKind InnerLeft, SkreeMetareeParticleSegment Look);
