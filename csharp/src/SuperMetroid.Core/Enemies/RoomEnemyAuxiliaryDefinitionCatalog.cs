using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy definitions used by actors the game creates as it runs but not selected by any room's enemy population or the
/// enemy artwork rooms load. They hold the same immutable gameplay fields as ordinary room definitions.
/// </summary>
internal static class RoomEnemyAuxiliaryDefinitionCatalog
{
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
        Touch: null,
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
        Touch: null,
        Shot: null,
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
        Touch: null,
        Shot: null,
        TouchHitboxScan: EnemyHitboxScan.Scanned,
        ShotHitboxScan: EnemyHitboxScan.Scanned,
        DropChances: new EnemyDropChances(30, 30, 50, 60, 85, 0),
        Vulnerability: EnemyVulnerabilityDefinitions.GoldenTorizo);

    /// <summary>
    /// A wall of Ceres Ridley's getaway chamber, which his getaway places (#1989), with the same fields as every Ceres door
    /// and wall.
    /// </summary>
    internal static readonly RoomEnemyDefinition CeresGetawayChamberWall = new(
        Health: 40,
        Damage: 15,
        Radius: new(8, 32),
        HurtFlashFrames: 4,
        HurtSoundEffect: EnemySound.RidleyRoar,
        Boss: null,
        GrappleResponse: EnemyGrappleResponse.Cancel,
        HurtResponse: EnemyHurtResponse.Inert,
        DeathExplosion: EnemyDeathExplosion.Small,
        PowerBombReaction: null,
        Touch: EnemyReaction.Ignore,
        Shot: EnemyReaction.Ignore,
        TouchHitboxScan: EnemyHitboxScan.Scanned,
        ShotHitboxScan: EnemyHitboxScan.Scanned,
        DropChances: null,
        Vulnerability: null);

    /// <summary>The cutscene actors', Torizo orbs' and getaway chamber walls' definitions, by identity.</summary>
    internal static readonly Dictionary<EnemyDefinitionId, RoomEnemyDefinition> Definitions =
        new Dictionary<EnemyDefinitionId, RoomEnemyDefinition>
    {
        [EnemyDefinitionId.BabyMetroidCutscene] = BabyMetroidCutscene,
        [EnemyDefinitionId.MotherBrainTubes] = MotherBrainTubes,
        [EnemyDefinitionId.BombTorizoOrb] = BombTorizoOrb,
        [EnemyDefinitionId.GoldenTorizoOrb] = GoldenTorizoOrb,
        [EnemyDefinitionId.CeresGetawayChamberWall] = CeresGetawayChamberWall,
    };

    /// <summary>Looks up one dynamic actor's definition.</summary>
    internal static bool TryGet(EnemyDefinitionId definitionId, out RoomEnemyDefinition definition) =>
        Definitions.TryGetValue(definitionId, out definition);
}
