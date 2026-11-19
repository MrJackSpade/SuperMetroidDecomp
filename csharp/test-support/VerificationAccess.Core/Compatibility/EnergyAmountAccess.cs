namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The stored words of <see cref="EnergyAmount"/> that only verification reads and writes (#2555): checks compare the
/// game's periodic damage with the cartridge's whole and fraction words, which the game holds as one energy amount.
/// </summary>
internal static class EnergyAmountAccess
{
    /// <summary>Builds energy amounts from the cartridge's words.</summary>
    extension(EnergyAmount)
    {
        /// <summary>The amount of <paramref name="whole"/> energy and a fraction word of <paramref name="fraction"/>, each 0 through 65535.</summary>
        internal static EnergyAmount FromStoredParts(int whole, int fraction) =>
            EnergyAmount.FromEnergy(whole) + EnergyAmount.FromEnergyFraction(fraction, 65536);
    }

    /// <summary>Reads an energy amount's words.</summary>
    extension(EnergyAmount amount)
    {
        /// <summary>The whole energy word.</summary>
        internal int Whole => amount.WholeEnergy;

        /// <summary>The fraction word.</summary>
        internal int Fraction => amount.FractionParts;
    }
}
