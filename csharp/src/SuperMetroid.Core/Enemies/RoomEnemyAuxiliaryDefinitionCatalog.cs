using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy definitions used by runtime-created actors but not selected by any retail room population or the
/// enemy artwork rooms load. They hold the same immutable gameplay fields as ordinary room definitions.
/// </summary>
internal static class RoomEnemyAuxiliaryDefinitionCatalog
{
    /// <summary>The respawn placeholder's, cutscene actors', Torizo orbs' and Spore Spawn stalk's definitions.</summary>
    /// <remarks>
    /// The respawn placeholder is selected when a killed actor vacates its slot. The Baby Metroid and falling tubes are
    /// Mother Brain's cutscene actors. The two Chozo-orb definitions are Bomb Torizo's and Golden Torizo's drops. The
    /// Spore Spawn stalk definition's drop chances pass for every spore impact.
    /// </remarks>
    internal static readonly Dictionary<EnemyDefinitionId, RoomEnemyDefinition> Definitions =
        new Dictionary<EnemyDefinitionId, RoomEnemyDefinition>
    {
        [EnemyDefinitionId.Respawn] = new(0, 0, new(8, 8), 4, EnemySound.RidleyRoar, null, EnemyAiRoutine.Inert, EnemyAiRoutine.Inert, GrappleAiRoutine.Cancel, EnemyAiRoutine.Inert, EnemyDeathExplosion.Small, null, 0, EnemyReaction.Ignore, EnemyReaction.Ignore, EnemyHitboxScan.Withheld, EnemyHitboxScan.Withheld, null, null),
        [EnemyDefinitionId.BabyMetroidCutscene] = new(3200, 40, new(36, 36), 4, null, null, EnemyAiRoutine.SpeciesRoutine, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.Inert, EnemyDeathExplosion.Small, null, 0, EnemyReaction.SpeciesOwn, EnemyReaction.Ignore, EnemyHitboxScan.Scanned, EnemyHitboxScan.Withheld, new EnemyDropChances(0, 0, 0, 255, 0, 0), EnemyVulnerabilityDefinitions.Standard),
        [EnemyDefinitionId.MotherBrainTubes] = new(1000, 0, new(0, 0), 4, null, null, EnemyAiRoutine.SpeciesRoutine, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.Inert, EnemyDeathExplosion.Small, null, 0, EnemyReaction.Ignore, EnemyReaction.Ignore, EnemyHitboxScan.Withheld, EnemyHitboxScan.Withheld, null, EnemyVulnerabilityDefinitions.Standard),
        [EnemyDefinitionId.BombTorizoOrb] = new(800, 8, new(18, 48), 8, EnemySound.ShotTorizo, AreaBoss.Torizo, EnemyAiRoutine.SpeciesRoutine, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.SpeciesRoutine, EnemyDeathExplosion.Small, null, 0, EnemyReaction.BombTorizoTouch, EnemyReaction.BombTorizoShot, EnemyHitboxScan.Scanned, EnemyHitboxScan.Scanned, new EnemyDropChances(116, 20, 65, 54, 0, 0), EnemyVulnerabilityDefinitions.BombTorizo),
        [EnemyDefinitionId.GoldenTorizoOrb] = new(13500, 160, new(18, 48), 8, EnemySound.ShotTorizo, AreaBoss.Torizo, EnemyAiRoutine.SpeciesRoutine, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.SpeciesRoutine, EnemyDeathExplosion.Small, null, 0, EnemyReaction.BombTorizoTouch, EnemyReaction.GoldenTorizoShot, EnemyHitboxScan.Scanned, EnemyHitboxScan.Scanned, new EnemyDropChances(30, 30, 50, 60, 85, 0), EnemyVulnerabilityDefinitions.GoldenTorizo),
        [EnemyDefinitionId.SporeSpawnStalk] = new(960, 12, new(19, 19), 8, null, AreaBoss.SporeSpawn, EnemyAiRoutine.SpeciesRoutine, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.Inert, EnemyDeathExplosion.Small, null, 0, EnemyReaction.SporeSpawnTouch, EnemyReaction.SporeSpawnShot, EnemyHitboxScan.Scanned, EnemyHitboxScan.Scanned, new EnemyDropChances(10, 20, 200, 25, 0, 0), EnemyVulnerabilityDefinitions.SporeSpawn),
    };

    /// <summary>Looks up one dynamic actor's definition.</summary>
    internal static bool TryGet(EnemyDefinitionId definitionId, out RoomEnemyDefinition definition) =>
        Definitions.TryGetValue(definitionId, out definition);
}
