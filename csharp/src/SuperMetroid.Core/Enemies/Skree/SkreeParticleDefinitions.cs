using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>The particles Skree bursts into.</summary>
internal static class SkreeParticleDefinitions
{
    /// <summary>Skree's four-particle burst.</summary>
    internal static MetalParticleBurst Burst { get; } = new(
        RoomEnemyProjectileKind.SkreeParticleDownRight, RoomEnemyProjectileKind.SkreeParticleUpRight,
        RoomEnemyProjectileKind.SkreeParticleDownLeft, RoomEnemyProjectileKind.SkreeParticleUpLeft);
}
