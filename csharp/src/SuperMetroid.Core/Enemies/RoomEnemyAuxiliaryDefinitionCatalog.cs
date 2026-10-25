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
    /// Spawn stalk header's drop table passes for every spore impact.
    /// </remarks>
    internal static readonly Dictionary<EnemyDefinitionId, RoomEnemyDefinition> Definitions =
        new Dictionary<EnemyDefinitionId, RoomEnemyDefinition>
    {
        [EnemyDefinitionId.Respawn] = new(0x0000, EnemyTilePlacement.FollowingPrevious, 0x0000, 0x0000, new(0x0008, 0x0008), 0x00, Library2Sound.RidleyRoar, 0x0000, EnemyAiRoutine.CommonNoOp, 0x0001, EnemyAiRoutine.CommonNoOp, GrappleAiRoutine.Cancel, EnemyAiRoutine.CommonNoOp, 0x0000, null, EnemyCallbackBody.None, 0x0000, EnemyInteractionCallback.NoOp, EnemyCallbackBody.ImmediateReturn, EnemyInteractionCallback.NoOp, EnemyCallbackBody.ImmediateReturn, EnemyCallbackBody.ImmediateReturn, 0x00, null, new(0)),
        [EnemyDefinitionId.BabyMetroidCutscene] = new(0x0000, EnemyTilePlacement.FollowingPrevious, 0x0c80, 0x0028, new(0x0024, 0x0024), 0x00, null, 0x0000, EnemyAiRoutine.SpeciesRoutine, 0x0001, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.CommonNoOp, 0x0000, null, EnemyCallbackBody.None, 0x0000, null, EnemyCallbackBody.Routine, EnemyInteractionCallback.NoOp, EnemyCallbackBody.ImmediateReturn, EnemyCallbackBody.ImmediateReturn, 0x02, new(114), new(0)),
        [EnemyDefinitionId.MotherBrainTubes] = new(0x0000, EnemyTilePlacement.FollowingPrevious, 0x03e8, 0x0000, new(0x0000, 0x0000), 0x00, null, 0x0000, EnemyAiRoutine.SpeciesRoutine, 0x0001, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.CommonNoOp, 0x0000, null, EnemyCallbackBody.None, 0x0000, EnemyInteractionCallback.NoOp, EnemyCallbackBody.ImmediateReturn, EnemyInteractionCallback.NoOp, EnemyCallbackBody.ImmediateReturn, EnemyCallbackBody.ImmediateReturn, 0x05, null, new(0)),
        [EnemyDefinitionId.BombTorizoOrb] = new(0x2000, EnemyTilePlacement.FollowingPrevious, 0x0320, 0x0008, new(0x0012, 0x0030), 0x08, Library2Sound.ShotTorizo, 0x0002, EnemyAiRoutine.SpeciesRoutine, 0x0001, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.SpeciesRoutine, 0x0000, null, EnemyCallbackBody.None, 0x0000, EnemyInteractionCallback.BombTorizoTouch, EnemyCallbackBody.Routine, EnemyInteractionCallback.BombTorizoShot, EnemyCallbackBody.Routine, EnemyCallbackBody.ImmediateReturn, 0x05, new(87), new(54)),
        [EnemyDefinitionId.GoldenTorizoOrb] = new(0x2000, EnemyTilePlacement.FollowingPrevious, 0x34bc, 0x00a0, new(0x0012, 0x0030), 0x08, Library2Sound.ShotTorizo, 0x0002, EnemyAiRoutine.SpeciesRoutine, 0x0001, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.SpeciesRoutine, 0x0000, null, EnemyCallbackBody.None, 0x0000, EnemyInteractionCallback.BombTorizoTouch, EnemyCallbackBody.Routine, EnemyInteractionCallback.GoldenTorizoShot, EnemyCallbackBody.Routine, EnemyCallbackBody.ImmediateReturn, 0x05, new(89), new(55)),
        [EnemyDefinitionId.SporeSpawnStalk] = new(0x0e00, EnemyTilePlacement.FollowingPrevious, 0x03c0, 0x000c, new(0x0013, 0x0013), 0x08, null, 0x0004, EnemyAiRoutine.SpeciesRoutine, 0x0001, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.CommonNoOp, 0x0000, null, EnemyCallbackBody.None, 0x0000, EnemyInteractionCallback.SporeSpawnTouch, EnemyCallbackBody.Routine, EnemyInteractionCallback.SporeSpawnShot, EnemyCallbackBody.Routine, EnemyCallbackBody.ImmediateReturn, 0x05, new(91), new(52)),
    };

    /// <summary>Looks up one dynamic actor's definition.</summary>
    internal static bool TryGet(EnemyDefinitionId definitionId, out RoomEnemyDefinition definition) =>
        Definitions.TryGetValue(definitionId, out definition);
}
