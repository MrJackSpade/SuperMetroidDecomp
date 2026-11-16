namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>The particles Skree bursts into.</summary>
internal static class SkreeParticleDefinitions
{
    /// <summary>Skree's four-particle burst.</summary>
    internal static MetalParticleBurst Burst { get; } = new(
        RoomEnemyProjectileKind.SkreeParticleOuterRight, RoomEnemyProjectileKind.SkreeParticleInnerRight,
        RoomEnemyProjectileKind.SkreeParticleOuterLeft, RoomEnemyProjectileKind.SkreeParticleInnerLeft, SkreeMetareeParticleSegment.Skree);
}
