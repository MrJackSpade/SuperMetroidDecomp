namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One fixed-point environmental damage contribution per accepted frame.</summary>
internal readonly record struct SamusLiquidDamageRate(ushort SubDamage, ushort WholeDamage);
