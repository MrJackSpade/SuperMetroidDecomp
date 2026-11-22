using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// What Samus carries (#2557): her items and beams, collected and equipped, her reserve tanks' mode, her energy and its
/// capacity, her ammunition with each capacity, and her reserve energy with its capacity. It travels whole between Samus, a
/// save snapshot, a saved slot and the save file, and a Samus set up again takes it back whole.
/// </summary>
/// <param name="EquippedItems">The collected items Samus has equipped.</param>
/// <param name="CollectedItems">The items Samus has collected, equipped or not.</param>
/// <param name="EquippedBeams">The collected beams Samus has equipped.</param>
/// <param name="CollectedBeams">The beams Samus has collected, equipped or not.</param>
/// <param name="ReserveMode">The reserve tanks' mode: none, auto or manual.</param>
/// <param name="Health">Samus's energy in energy-point units.</param>
/// <param name="MaxHealth">Samus's energy capacity in energy-point units.</param>
/// <param name="Missiles">The ordinary missile count.</param>
/// <param name="MaxMissiles">The ordinary missile capacity.</param>
/// <param name="SuperMissiles">The super missile count.</param>
/// <param name="MaxSuperMissiles">The super missile capacity.</param>
/// <param name="PowerBombs">The power bomb count.</param>
/// <param name="MaxPowerBombs">The power bomb capacity.</param>
/// <param name="MaxReserveEnergy">The reserve tanks' capacity in energy-point units.</param>
/// <param name="ReserveEnergy">The energy stored in the reserve tanks.</param>
public readonly partial record struct SamusInventory(
    SamusEquipmentFlags EquippedItems,
    SamusEquipmentFlags CollectedItems,
    SamusBeamFlags EquippedBeams,
    SamusBeamFlags CollectedBeams,
    ReserveTankMode ReserveMode,
    int Health,
    int MaxHealth,
    int Missiles,
    int MaxMissiles,
    int SuperMissiles,
    int MaxSuperMissiles,
    int PowerBombs,
    int MaxPowerBombs,
    int MaxReserveEnergy,
    int ReserveEnergy);
