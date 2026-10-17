namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One fixed-point environmental damage contribution per accepted frame.</summary>
/// <param name="SubDamage">Fraction of one energy unit, in 1/65536 units: 0 through 65535.</param>
/// <param name="WholeDamage">Whole energy units: 0 through 32767.</param>
internal readonly record struct SamusLiquidDamageRate(int SubDamage, int WholeDamage);
