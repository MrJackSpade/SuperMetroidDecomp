using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies;

/// <summary>Enemy definitions checks bind where the definition's own values are not what they exercise.</summary>
internal static class EnemyDefinitionFixtures
{
    /// <summary>
    /// A definition with no health, damage or size of its own that names every role a definition has (#2558): the
    /// species' own touch and shot, the common Power Bomb damage, no grapple reaction, no drop and the standard
    /// vulnerabilities. A check changes the values it exercises with <c>with</c>.
    /// </summary>
    internal static RoomEnemyDefinition Blank { get; } = new(
        Health: 0,
        Damage: 0,
        Radius: default,
        HurtFlashFrames: 0,
        HurtSoundEffect: null,
        Boss: null,
        GrappleResponse: GrappleEnemyReaction.Unreactive,
        HurtResponse: default,
        DeathExplosion: default,
        PowerBombReaction: EnemyReaction.CommonPowerBomb,
        Touch: EnemyReaction.SpeciesOwn,
        Shot: EnemyReaction.SpeciesOwn,
        TouchHitboxScan: default,
        ShotHitboxScan: default,
        Drops: EnemyDrops.Nothing,
        Vulnerability: EnemyVulnerabilityDefinitions.Standard);
}
