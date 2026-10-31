using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One fixed special-air dispatch result paired with the setup callback stored
/// by its PLM header.
/// </summary>
public readonly record struct SpecialAirReactionDefinition(
    PlmHeaderId Header,
    SpecialAirReactionSetup Setup);
