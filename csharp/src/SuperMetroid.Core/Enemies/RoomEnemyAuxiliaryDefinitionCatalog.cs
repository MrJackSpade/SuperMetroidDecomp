using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Fixed bank-$A0 enemy headers used by runtime-created actors but not selected
/// by any retail room population or graphics-set list. They retain the same
/// immutable gameplay fields as ordinary room headers.
/// </summary>
internal static class RoomEnemyAuxiliaryDefinitionCatalog
{
    /// <summary>Maps the respawn placeholder, cutscene actors, Torizo orbs, and Spore Spawn stalk to pinned bank-$A0 headers.</summary>
    /// <remarks>
    /// The respawn placeholder is selected when a killed actor vacates its slot. The Baby Metroid and falling tubes are
    /// Mother Brain's cutscene actors. The two Chozo-orb headers are Bomb Torizo's and Golden Torizo's drops. The Spore
    /// Spawn stalk header's drop table passes for every spore impact.
    /// </remarks>
    internal static readonly Dictionary<EnemyDefinitionId, RoomEnemyDefinition> Definitions =
        new Dictionary<EnemyDefinitionId, RoomEnemyDefinition>
    {
        [EnemyDefinitionId.Respawn] = new(0x0000, 0x0000, 0x0000, new(0x0008, 0x0008), 0x00, 0x0059, 0x0000, EnemyAiRoutine.CommonNoOp, 0x0001, 0x0000, EnemyAiRoutine.CommonNoOp, GrappleAiRoutine.Cancel, EnemyAiRoutine.CommonNoOp, 0x0000, 0x0000, 0x0000, null, EnemyCallbackBody.None, 0x0000, 0x0000, 0x0000, EnemyInteractionCallback.NoOp, EnemyCallbackBody.ImmediateReturn, EnemyInteractionCallback.NoOp, EnemyCallbackBody.ImmediateReturn, EnemyCallbackBody.ImmediateReturn, 0xae9000, 0x00, null, new(0)),
        [EnemyDefinitionId.BabyMetroidCutscene] = new(0x0000, 0x0c80, 0x0028, new(0x0024, 0x0024), 0x00, 0x0000, 0x0000, EnemyAiRoutine.SpeciesRoutine, 0x0001, 0x0000, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.CommonNoOp, 0x0000, 0x0000, 0x0000, null, EnemyCallbackBody.None, 0x0000, 0x0000, 0x0000, null, EnemyCallbackBody.Routine, EnemyInteractionCallback.NoOp, EnemyCallbackBody.ImmediateReturn, EnemyCallbackBody.ImmediateReturn, 0xb18400, 0x02, new(114), new(0)),
        [EnemyDefinitionId.MotherBrainTubes] = new(0x0000, 0x03e8, 0x0000, new(0x0000, 0x0000), 0x00, 0x0000, 0x0000, EnemyAiRoutine.SpeciesRoutine, 0x0001, 0x0000, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.CommonNoOp, 0x0000, 0x0000, 0x0000, null, EnemyCallbackBody.None, 0x0000, 0x0000, 0x0000, EnemyInteractionCallback.NoOp, EnemyCallbackBody.ImmediateReturn, EnemyInteractionCallback.NoOp, EnemyCallbackBody.ImmediateReturn, EnemyCallbackBody.ImmediateReturn, 0xb78000, 0x05, null, new(0)),
        [EnemyDefinitionId.BombTorizoOrb] = new(0x2000, 0x0320, 0x0008, new(0x0012, 0x0030), 0x08, 0x0027, 0x0002, EnemyAiRoutine.SpeciesRoutine, 0x0001, 0x0000, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.SpeciesRoutine, 0x0000, 0x0000, 0x0000, null, EnemyCallbackBody.None, 0x0000, 0x0000, 0x0000, EnemyInteractionCallback.BombTorizoTouch, EnemyCallbackBody.Routine, EnemyInteractionCallback.BombTorizoShot, EnemyCallbackBody.Routine, EnemyCallbackBody.ImmediateReturn, 0xafc200, 0x05, new(87), new(54)),
        [EnemyDefinitionId.GoldenTorizoOrb] = new(0x2000, 0x34bc, 0x00a0, new(0x0012, 0x0030), 0x08, 0x0027, 0x0002, EnemyAiRoutine.SpeciesRoutine, 0x0001, 0x0000, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.SpeciesRoutine, 0x0000, 0x0000, 0x0000, null, EnemyCallbackBody.None, 0x0000, 0x0000, 0x0000, EnemyInteractionCallback.BombTorizoTouch, EnemyCallbackBody.Routine, EnemyInteractionCallback.GoldenTorizoShot, EnemyCallbackBody.Routine, EnemyCallbackBody.ImmediateReturn, 0xafc200, 0x05, new(89), new(55)),
        [EnemyDefinitionId.SporeSpawnStalk] = new(0x0e00, 0x03c0, 0x000c, new(0x0013, 0x0013), 0x08, 0x0000, 0x0004, EnemyAiRoutine.SpeciesRoutine, 0x0001, 0x0000, EnemyAiRoutine.SpeciesRoutine, GrappleAiRoutine.Cancel, EnemyAiRoutine.CommonNoOp, 0x0000, 0x0000, 0x0000, null, EnemyCallbackBody.None, 0x0000, 0x0000, 0x0000, EnemyInteractionCallback.SporeSpawnTouch, EnemyCallbackBody.Routine, EnemyInteractionCallback.SporeSpawnShot, EnemyCallbackBody.Routine, EnemyCallbackBody.ImmediateReturn, 0xac9c00, 0x05, new(91), new(52)),
    };

    /// <summary>Looks up one native dynamic-actor header without reading the cartridge.</summary>
    internal static bool TryGet(EnemyDefinitionId definitionId, out RoomEnemyDefinition definition) =>
        Definitions.TryGetValue(definitionId, out definition);
}
