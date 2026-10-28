using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>
/// The particles Metaree bursts into. Their motion is Skree's, but each selects Metaree's own spritemap
/// (<c>$86:8AC5</c>) rather than Skree's visually different debris.
/// </summary>
internal static class MetareeParticleDefinitions
{
    /// <summary>Metaree's four-particle burst.</summary>
    internal static MetalParticleBurst Burst { get; } = new(
        RoomEnemyProjectileKind.MetareeParticleDownRight, RoomEnemyProjectileKind.MetareeParticleUpRight,
        RoomEnemyProjectileKind.MetareeParticleDownLeft, RoomEnemyProjectileKind.MetareeParticleUpLeft);
}
