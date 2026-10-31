namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One fixed-point environmental damage contribution per frame.</summary>
/// <param name="SubDamage">Fraction of one energy unit, in 1/65536 units: 0 through 65535.</param>
/// <param name="WholeDamage">Whole energy units: 0 through 32767.</param>
internal readonly record struct SamusLiquidDamageRate(int SubDamage, int WholeDamage)
{
    /// <summary>Fractional damage units in one energy unit.</summary>
    private const int SubDamagePerEnergy = 65536;

    /// <summary>A rate of <paramref name="halves"/> half energy units per frame.</summary>
    internal static SamusLiquidDamageRate FromHalfEnergies(int halves) => new(halves % 2 * (SubDamagePerEnergy / 2), halves / 2);
}
