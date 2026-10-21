using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One fixed bank-$94 special-air dispatch result paired with the setup callback stored
/// by its bank-$84 PLM header.
/// </summary>
public readonly record struct SpecialAirReactionDefinition(
    PlmHeaderId Header,
    SpecialAirReactionSetup Setup);
