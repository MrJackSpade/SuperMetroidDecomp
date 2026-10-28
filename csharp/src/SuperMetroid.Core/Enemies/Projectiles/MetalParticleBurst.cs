namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The four particles a Skree or Metaree bursts into as it burrows or dies (#1435), one per direction. Each enemy names its
/// own, so the debris it spawns is its own and draws at its own tile base.
/// </summary>
/// <param name="DownRight">The outer particle flying right.</param>
/// <param name="UpRight">The inner particle flying right.</param>
/// <param name="DownLeft">The outer particle flying left.</param>
/// <param name="UpLeft">The inner particle flying left.</param>
internal sealed record MetalParticleBurst(RoomEnemyProjectileKind DownRight, RoomEnemyProjectileKind UpRight,
    RoomEnemyProjectileKind DownLeft, RoomEnemyProjectileKind UpLeft);
