using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The changes Mother Brain's fight makes to her room's acid: room effects of their own beside her room's effect selection, which selects only
/// the resting acid, so her fight is the one thing that reaches them (#1546).
/// </summary>
internal static partial class MotherBrainRoomFxDefinitions
{
    /// <summary>
    /// Her room's acid rises from its low level to its high one after a short delay: requested by her body's initialization and applied
    /// as the room finishes loading.
    /// </summary>
    internal static RoomEffectDefinition AcidRises => RoomFxDefinitions.MotherBrainAcidRises;

    /// <summary>Her room's acid drains from its high level back down to its low one: requested when her fake death starts the room flash.</summary>
    internal static RoomEffectDefinition AcidDrains => RoomFxDefinitions.MotherBrainAcidDrains;
}
