using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The changes Mother Brain's fight makes to her room's acid. Her room's FX holds them after its resting acid; the room
/// only selects the resting acid itself, so her fight is the one thing that reaches them.
/// </summary>
internal static class MotherBrainRoomFxDefinitions
{
    /// <summary>
    /// Her room's acid rises from Y 232 to Y 184 after a short delay: requested by her body's initialization and applied
    /// as the room finishes loading.
    /// </summary>
    internal static RoomFxRecordDefinition AcidRises { get; } = RoomFxDefinitions.MotherBrainDefaultOrWhenMotherBrainGlassDestroyed.Records[1];

    /// <summary>Her room's acid drains from Y 184 back down to Y 232: requested when her fake death starts the room flash.</summary>
    internal static RoomFxRecordDefinition AcidDrains { get; } = RoomFxDefinitions.MotherBrainDefaultOrWhenMotherBrainGlassDestroyed.Records[2];
}
