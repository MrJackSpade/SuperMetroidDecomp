using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Fx;

/// <summary>One quicksand reaction: the setup its contact runs and the terrain change its room object then makes.</summary>
/// <param name="Setup">The setup Samus's contact with the sand runs.</param>
/// <param name="Change">The reaction object's change, which is the whole of its spawn: it leaves on its first update.</param>
internal readonly record struct QuicksandReactionDefinition(
    SpecialAirReactionSetup Setup,
    TerrainChange Change);
