namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>What a Crystal Flash asks of Samus and what it gives her back.</summary>
internal static class CrystalFlashDefinitions
{
    /// <summary>Samus's energy must be below this for the flash to start.</summary>
    public const int AdmittingEnergyBelow = 51;

    /// <summary>
    /// The Missiles, Super Missiles and Power Bombs the flash spends of each kind, one per update: each count must hold at
    /// least this many for the flash to start.
    /// </summary>
    public const int AmmunitionPerKind = 10;

    /// <summary>The energy each spent round of ammunition restores.</summary>
    public const int EnergyPerAmmunition = 50;

    /// <summary>The updates the flash raises Samus for before it starts spending ammunition.</summary>
    public const int RaiseUpdates = 10;
}
