using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Fx;

/// <summary>One quicksand PLM header paired with its setup routine and first program.</summary>
internal readonly record struct QuicksandReactionDefinition(
    SpecialAirReactionSetup Setup,
    PlmProgramStart Program);
