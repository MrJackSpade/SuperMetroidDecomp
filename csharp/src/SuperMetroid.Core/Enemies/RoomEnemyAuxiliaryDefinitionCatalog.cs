using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy definitions used by runtime-created actors but not selected by any retail room population or the
/// enemy artwork rooms load. They hold the same immutable gameplay fields as ordinary room definitions.
/// </summary>
internal static class RoomEnemyAuxiliaryDefinitionCatalog
{
    /// <summary>The respawn placeholder, selected when a killed actor vacates its slot.</summary>
    internal static readonly RoomEnemyDefinition Respawn = new(
        Health: 0,
        Damage: 0,
        Radius: new(8, 8),
        HurtFlashFrames: 4,
        HurtSoundEffect: EnemySound.RidleyRoar,
        Boss: null,
        GrappleResponse: EnemyGrappleResponse.Cancel,
        HurtResponse: EnemyHurtResponse.Inert,
        DeathExplosion: EnemyDeathExplosion.Small,
        PowerBombReaction: null,
        Touch: EnemyReaction.Ignore,
        Shot: EnemyReaction.Ignore,
        TouchHitboxScan: EnemyHitboxScan.Withheld,
        ShotHitboxScan: EnemyHitboxScan.Withheld,
        DropChances: null,
        Vulnerability: null);

    /// <summary>The Baby Metroid of Mother Brain's cutscene.</summary>
    internal static readonly RoomEnemyDefinition BabyMetroidCutscene = new(
        Health: 3200,
        Damage: 40,
        Radius: new(36, 36),
        HurtFlashFrames: 4,
        HurtSoundEffect: null,
        Boss: null,
        GrappleResponse: EnemyGrappleResponse.Cancel,
        HurtResponse: EnemyHurtResponse.Inert,
        DeathExplosion: EnemyDeathExplosion.Small,
        PowerBombReaction: null,
        Touch: EnemyReaction.SpeciesOwn,
        Shot: EnemyReaction.Ignore,
        TouchHitboxScan: EnemyHitboxScan.Scanned,
        ShotHitboxScan: EnemyHitboxScan.Withheld,
        DropChances: new EnemyDropChances(0, 0, 0, 255, 0, 0),
        Vulnerability: EnemyVulnerabilityDefinitions.Standard);

    /// <summary>The falling tubes of Mother Brain's cutscene.</summary>
    internal static readonly RoomEnemyDefinition MotherBrainTubes = new(
        Health: 1000,
        Damage: 0,
        Radius: new(0, 0),
        HurtFlashFrames: 4,
        HurtSoundEffect: null,
        Boss: null,
        GrappleResponse: EnemyGrappleResponse.Cancel,
        HurtResponse: EnemyHurtResponse.Inert,
        DeathExplosion: EnemyDeathExplosion.Small,
        PowerBombReaction: null,
        Touch: EnemyReaction.Ignore,
        Shot: EnemyReaction.Ignore,
        TouchHitboxScan: EnemyHitboxScan.Withheld,
        ShotHitboxScan: EnemyHitboxScan.Withheld,
        DropChances: null,
        Vulnerability: EnemyVulnerabilityDefinitions.Standard);

    /// <summary>The Chozo orb Bomb Torizo drops.</summary>
    internal static readonly RoomEnemyDefinition BombTorizoOrb = new(
        Health: 800,
        Damage: 8,
        Radius: new(18, 48),
        HurtFlashFrames: 8,
        HurtSoundEffect: EnemySound.ShotTorizo,
        Boss: AreaBoss.Torizo,
        GrappleResponse: EnemyGrappleResponse.Cancel,
        HurtResponse: EnemyHurtResponse.SpeciesOwn,
        DeathExplosion: EnemyDeathExplosion.Small,
        PowerBombReaction: null,
        Touch: EnemyReaction.BombTorizoTouch,
        Shot: EnemyReaction.BombTorizoShot,
        TouchHitboxScan: EnemyHitboxScan.Scanned,
        ShotHitboxScan: EnemyHitboxScan.Scanned,
        DropChances: new EnemyDropChances(116, 20, 65, 54, 0, 0),
        Vulnerability: EnemyVulnerabilityDefinitions.BombTorizo);

    /// <summary>The Chozo orb Golden Torizo drops.</summary>
    internal static readonly RoomEnemyDefinition GoldenTorizoOrb = new(
        Health: 13500,
        Damage: 160,
        Radius: new(18, 48),
        HurtFlashFrames: 8,
        HurtSoundEffect: EnemySound.ShotTorizo,
        Boss: AreaBoss.Torizo,
        GrappleResponse: EnemyGrappleResponse.Cancel,
        HurtResponse: EnemyHurtResponse.SpeciesOwn,
        DeathExplosion: EnemyDeathExplosion.Small,
        PowerBombReaction: null,
        Touch: EnemyReaction.BombTorizoTouch,
        Shot: EnemyReaction.GoldenTorizoShot,
        TouchHitboxScan: EnemyHitboxScan.Scanned,
        ShotHitboxScan: EnemyHitboxScan.Scanned,
        DropChances: new EnemyDropChances(30, 30, 50, 60, 85, 0),
        Vulnerability: EnemyVulnerabilityDefinitions.GoldenTorizo);

    /// <summary>Spore Spawn's stalk, whose drop chances pass for every spore impact.</summary>
    internal static readonly RoomEnemyDefinition SporeSpawnStalk = new(
        Health: 960,
        Damage: 12,
        Radius: new(19, 19),
        HurtFlashFrames: 8,
        HurtSoundEffect: null,
        Boss: AreaBoss.SporeSpawn,
        GrappleResponse: EnemyGrappleResponse.Cancel,
        HurtResponse: EnemyHurtResponse.Inert,
        DeathExplosion: EnemyDeathExplosion.Small,
        PowerBombReaction: null,
        Touch: EnemyReaction.SporeSpawnTouch,
        Shot: EnemyReaction.SporeSpawnShot,
        TouchHitboxScan: EnemyHitboxScan.Scanned,
        ShotHitboxScan: EnemyHitboxScan.Scanned,
        DropChances: new EnemyDropChances(10, 20, 200, 25, 0, 0),
        Vulnerability: EnemyVulnerabilityDefinitions.SporeSpawn);

    /// <summary>The respawn placeholder's, cutscene actors', Torizo orbs' and Spore Spawn stalk's definitions, by identity.</summary>
    internal static readonly Dictionary<EnemyDefinitionId, RoomEnemyDefinition> Definitions =
        new Dictionary<EnemyDefinitionId, RoomEnemyDefinition>
    {
        [EnemyDefinitionId.Respawn] = Respawn,
        [EnemyDefinitionId.BabyMetroidCutscene] = BabyMetroidCutscene,
        [EnemyDefinitionId.MotherBrainTubes] = MotherBrainTubes,
        [EnemyDefinitionId.BombTorizoOrb] = BombTorizoOrb,
        [EnemyDefinitionId.GoldenTorizoOrb] = GoldenTorizoOrb,
        [EnemyDefinitionId.SporeSpawnStalk] = SporeSpawnStalk,
    };

    /// <summary>Looks up one dynamic actor's definition.</summary>
    internal static bool TryGet(EnemyDefinitionId definitionId, out RoomEnemyDefinition definition) =>
        Definitions.TryGetValue(definitionId, out definition);
}
