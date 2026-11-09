using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One fixed special-air dispatch result paired with the setup callback its room-object
/// header names.
/// </summary>
public readonly record struct SpecialAirReactionDefinition(
    RoomObjectHeaderId Header,
    SpecialAirReactionSetup Setup);
