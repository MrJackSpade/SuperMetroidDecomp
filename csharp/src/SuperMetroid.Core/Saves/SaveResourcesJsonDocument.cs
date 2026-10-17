using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>Current and acquired player-resource quantities persisted in a save slot.</summary>
/// <remarks>
/// Each quantity is a JSON number the codec accepts only in Samus's count domain,
/// <c>[0, <see cref="SamusQuantity.MaximumValue"/>]</c>, with each current value at most its capacity.
/// </remarks>
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
/// <param name="ReserveMissiles">Persistent excess-missile accumulator.</param>
public sealed record SaveResourcesJsonDocument(
    int Health,
    int MaxHealth,
    int Missiles,
    int MaxMissiles,
    int SuperMissiles,
    int MaxSuperMissiles,
    int PowerBombs,
    int MaxPowerBombs,
    int ReserveEnergy,
    int MaxReserveEnergy,
    int ReserveMissiles);
