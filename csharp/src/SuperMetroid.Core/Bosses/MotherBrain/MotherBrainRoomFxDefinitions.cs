using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The changes Mother Brain's fight makes to her room's acid: room effects of their own beside her room's effect selection, which selects only
/// the resting acid, so her fight is the one thing that reaches them (#1546).
/// </summary>
internal static class MotherBrainRoomFxDefinitions
{
    /// <summary>
    /// Her room's acid rises from Y 232 to Y 184 after a short delay: requested by her body's initialization and applied
    /// as the room finishes loading.
    /// </summary>
    internal static RoomEffectDefinition AcidRises => RoomFxDefinitions.MotherBrainAcidRises;

    /// <summary>Her room's acid drains from Y 184 back down to Y 232: requested when her fake death starts the room flash.</summary>
    internal static RoomEffectDefinition AcidDrains => RoomFxDefinitions.MotherBrainAcidDrains;
}
