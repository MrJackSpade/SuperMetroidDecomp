using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies;

/// <summary>Enemy definitions checks bind where the definition's own values are not what they exercise.</summary>
internal static class EnemyDefinitionFixtures
{
    /// <summary>A touch reaction of the blank definition's own, which no species and no shared reaction answers.</summary>
    internal static EnemyReaction UnansweredTouch { get; } = EnemyReaction.OfSpecies("Blank.Touch");

    /// <summary>A shot reaction of the blank definition's own, which no species and no shared reaction answers.</summary>
    internal static EnemyReaction UnansweredShot { get; } = EnemyReaction.OfSpecies("Blank.Shot");

    /// <summary>
    /// A definition with no health, damage or size of its own that names every role a definition has (#2558): a touch and
    /// a shot of its own that no species answers (<see cref="UnansweredTouch"/>, <see cref="UnansweredShot"/>), the
    /// common Power Bomb damage, no grapple reaction, no drop and the standard vulnerabilities. A check changes the values
    /// it exercises with <c>with</c>; a check of a species' contact names the species' own reactions.
    /// </summary>
    internal static RoomEnemyDefinition Blank { get; } = new(
        Health: 0,
        Damage: 0,
        Radius: default,
        HurtFlashFrames: 0,
        HurtSoundEffect: null,
        Boss: null,
        GrappleResponse: GrappleEnemyReaction.NamesNoReaction,
        HurtResponse: default,
        DeathExplosion: default,
        PowerBombReaction: EnemyReaction.CommonPowerBomb,
        Touch: UnansweredTouch,
        Shot: UnansweredShot,
        TouchHitboxScan: default,
        ShotHitboxScan: default,
        Drops: EnemyDrops.Nothing,
        Vulnerability: EnemyVulnerabilityDefinitions.Standard);
}
