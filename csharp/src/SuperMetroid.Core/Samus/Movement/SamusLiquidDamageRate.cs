namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One fixed-point environmental damage contribution per frame.</summary>
/// <param name="SubDamage">Fraction of one energy unit, 0 through one part short of an energy, in <see cref="PeriodicDamageEnergyFraction.PartsPerEnergy"/>ths.</param>
/// <param name="WholeDamage">Whole energy units, zero or more, never so many that they read as below zero.</param>
internal readonly record struct SamusLiquidDamageRate(int SubDamage, int WholeDamage)
{
    /// <summary>A rate of <paramref name="halves"/> half energy units per frame.</summary>
    internal static SamusLiquidDamageRate FromHalfEnergies(int halves) =>
        new(halves % 2 * (PeriodicDamageEnergyFraction.PartsPerEnergy / 2), halves / 2);
}
