using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The loadout a Torizo grants Samus as it appears while the player holds its secret chord (#2410): Golden Torizo's. The
/// loadout replaces both the collected and equipped items, even if this lowers a capacity or equips beams that are
/// normally incompatible.
/// </summary>
/// <param name="Chord">The chord: exactly these actions held, with no other.</param>
/// <param name="Energy">Samus's current and maximum energy.</param>
/// <param name="ReserveEnergy">Her current and maximum reserve energy; the reserve mode stays as it was.</param>
/// <param name="Missiles">Her current and maximum missiles.</param>
/// <param name="SuperMissiles">Her current and maximum Super Missiles.</param>
/// <param name="PowerBombs">Her current and maximum Power Bombs.</param>
/// <param name="Items">Her collected and equipped items.</param>
/// <param name="Beams">Her collected and equipped beams.</param>
internal sealed partial record TorizoSecretCode(
    GameplayAction Chord, int Energy, int ReserveEnergy, int Missiles, int SuperMissiles, int PowerBombs,
    SamusEquipmentFlags Items, SamusBeamFlags Beams)
{
    /// <summary>Gives <paramref name="samus"/> the loadout.</summary>
    internal void GrantTo(SamusState samus)
    {
        Ensure.NotNull(samus);
        samus.Health = samus.MaxHealth = Energy;
        samus.ReserveEnergy = samus.MaxReserveEnergy = ReserveEnergy;
        samus.Missiles = samus.MaxMissiles = Missiles;
        samus.SuperMissiles = samus.MaxSuperMissiles = SuperMissiles;
        samus.PowerBombs = samus.MaxPowerBombs = PowerBombs;
        samus.EquippedItems = samus.CollectedItems = Items;
        samus.EquippedBeams = samus.CollectedBeams = Beams;
    }
}
