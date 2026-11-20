namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The ammunition Crystal Flash is currently converting into energy. The drain empties its share of each kind
/// (<see cref="CrystalFlashDefinitions.AmmunitionPerKind"/>) in this order, missiles first and power bombs last, and finishes after the last power bomb.
/// </summary>
public enum CrystalFlashAmmoKind
{
    /// <summary>Missiles are consumed first.</summary>
    Missiles,

    /// <summary>Super missiles follow the missiles.</summary>
    SuperMissiles,

    /// <summary>Power bombs are consumed last; the last of them ends the drain.</summary>
    PowerBombs,
}
