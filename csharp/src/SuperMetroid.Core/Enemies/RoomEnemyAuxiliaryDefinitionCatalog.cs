using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy definitions used by runtime-created actors but not selected by any retail room population or graphics-set
/// list. They hold the same immutable gameplay fields as ordinary room definitions.
/// </summary>
internal static class RoomEnemyAuxiliaryDefinitionCatalog
{
    /// <summary>The respawn placeholder's, cutscene actors', Torizo orbs' and Spore Spawn stalk's definitions.</summary>
    /// <remarks>
    /// The respawn placeholder is selected when a killed actor vacates its slot. The Baby Metroid and falling tubes are
    /// Mother Brain's cutscene actors. The two Chozo-orb headers are Bomb Torizo's and Golden Torizo's drops. The Spore
    /// Spawn stalk header's drop chances pass for every spore impact.
    /// </remarks>
    internal static readonly Dictionary<EnemyDefinitionId, RoomEnemyDefinition> Definitions =
        new Dictionary<EnemyDefinitionId, RoomEnemyDefinition>
    {
        [EnemyDefinitionId.Respawn] = new(0x0000, EnemyTilePlacement.FollowingPrevious, 0x0000, 0x0000, new(0x0008, 0x0008), 4, Library2Sound.RidleyRoar, null, EnemyAiRoutine.CommonNoOp, EnemyAiRoutine.CommonNoOp, GrappleAiRoutine.Cancel, EnemyAiRoutine.CommonNoOp, EnemyDeathExplosion.Small, null, EnemyCallbackBody.None, 0x0000, EnemyInteractionCallback.NoOp, EnemyCallbackBody.ImmediateReturn, EnemyInteractionCallback.NoOp, EnemyCallbackBody.ImmediateReturn, EnemyCallbackBody.ImmediateReturn, 0x00, null, null),
        [EnemyDefinitionId.BabyMetroidCutscene] = new(0x0000, EnemyTilePlacement.FollowingPrevious, 0x0c80, 0x0028, new(0x0024, 0x0024), 4, null, null, EnemyAiRoutine.SpeciesRoutine, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.CommonNoOp, EnemyDeathExplosion.Small, null, EnemyCallbackBody.None, 0x0000, null, EnemyCallbackBody.Routine, EnemyInteractionCallback.NoOp, EnemyCallbackBody.ImmediateReturn, EnemyCallbackBody.ImmediateReturn, 0x02, new EnemyDropChances(0, 0, 0, 255, 0, 0), EnemyVulnerabilityDefinitions.Standard),
        [EnemyDefinitionId.MotherBrainTubes] = new(0x0000, EnemyTilePlacement.FollowingPrevious, 0x03e8, 0x0000, new(0x0000, 0x0000), 4, null, null, EnemyAiRoutine.SpeciesRoutine, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.CommonNoOp, EnemyDeathExplosion.Small, null, EnemyCallbackBody.None, 0x0000, EnemyInteractionCallback.NoOp, EnemyCallbackBody.ImmediateReturn, EnemyInteractionCallback.NoOp, EnemyCallbackBody.ImmediateReturn, EnemyCallbackBody.ImmediateReturn, 0x05, null, EnemyVulnerabilityDefinitions.Standard),
        [EnemyDefinitionId.BombTorizoOrb] = new(0x2000, EnemyTilePlacement.FollowingPrevious, 0x0320, 0x0008, new(0x0012, 0x0030), 8, Library2Sound.ShotTorizo, AreaBoss.Torizo, EnemyAiRoutine.SpeciesRoutine, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.SpeciesRoutine, EnemyDeathExplosion.Small, null, EnemyCallbackBody.None, 0x0000, EnemyInteractionCallback.BombTorizoTouch, EnemyCallbackBody.Routine, EnemyInteractionCallback.BombTorizoShot, EnemyCallbackBody.Routine, EnemyCallbackBody.ImmediateReturn, 0x05, new EnemyDropChances(116, 20, 65, 54, 0, 0), EnemyVulnerabilityDefinitions.BombTorizo),
        [EnemyDefinitionId.GoldenTorizoOrb] = new(0x2000, EnemyTilePlacement.FollowingPrevious, 0x34bc, 0x00a0, new(0x0012, 0x0030), 8, Library2Sound.ShotTorizo, AreaBoss.Torizo, EnemyAiRoutine.SpeciesRoutine, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.SpeciesRoutine, EnemyDeathExplosion.Small, null, EnemyCallbackBody.None, 0x0000, EnemyInteractionCallback.BombTorizoTouch, EnemyCallbackBody.Routine, EnemyInteractionCallback.GoldenTorizoShot, EnemyCallbackBody.Routine, EnemyCallbackBody.ImmediateReturn, 0x05, new EnemyDropChances(30, 30, 50, 60, 85, 0), EnemyVulnerabilityDefinitions.GoldenTorizo),
        [EnemyDefinitionId.SporeSpawnStalk] = new(0x0e00, EnemyTilePlacement.FollowingPrevious, 0x03c0, 0x000c, new(0x0013, 0x0013), 8, null, AreaBoss.SporeSpawn, EnemyAiRoutine.SpeciesRoutine, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.CommonNoOp, EnemyDeathExplosion.Small, null, EnemyCallbackBody.None, 0x0000, EnemyInteractionCallback.SporeSpawnTouch, EnemyCallbackBody.Routine, EnemyInteractionCallback.SporeSpawnShot, EnemyCallbackBody.Routine, EnemyCallbackBody.ImmediateReturn, 0x05, new EnemyDropChances(10, 20, 200, 25, 0, 0), EnemyVulnerabilityDefinitions.SporeSpawn),
    };

    /// <summary>Looks up one dynamic actor's definition.</summary>
    internal static bool TryGet(EnemyDefinitionId definitionId, out RoomEnemyDefinition definition) =>
        Definitions.TryGetValue(definitionId, out definition);
}
