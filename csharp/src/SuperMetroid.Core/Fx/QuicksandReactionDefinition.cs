using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Fx;

/// <summary>One quicksand PLM header paired with its setup routine and first program.</summary>
internal readonly record struct QuicksandReactionDefinition(
    SpecialAirReactionSetup Setup,
    RoomObjectProgramStart Program);
