namespace SuperMetroid.Core.Saves;

/// <summary>Current and acquired player-resource quantities persisted in a save slot.</summary>
/// <param name="Health">Current energy in energy-point units.</param>
/// <param name="MaxHealth">Acquired energy capacity in energy-point units.</param>
/// <param name="Missiles">Current ordinary missile count.</param>
/// <param name="MaxMissiles">Acquired ordinary missile capacity.</param>
/// <param name="SuperMissiles">Current super missile count.</param>
/// <param name="MaxSuperMissiles">Acquired super missile capacity.</param>
/// <param name="PowerBombs">Current power bomb count.</param>
/// <param name="MaxPowerBombs">Acquired power bomb capacity.</param>
/// <param name="ReserveEnergy">Energy points currently stored in reserve tanks.</param>
/// <param name="MaxReserveEnergy">Acquired reserve-tank capacity in energy points.</param>
/// <param name="ReserveMissiles">Persistent excess-missile accumulator from WRAM <c>$09D8</c>.</param>
public sealed record SaveResourcesJsonDocument(
    ushort Health,
    ushort MaxHealth,
    ushort Missiles,
    ushort MaxMissiles,
    ushort SuperMissiles,
    ushort MaxSuperMissiles,
    ushort PowerBombs,
    ushort MaxPowerBombs,
    ushort ReserveEnergy,
    ushort MaxReserveEnergy,
    ushort ReserveMissiles);
