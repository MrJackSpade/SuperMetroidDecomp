namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Compiled liquid-damage rates from Samus's fixed bank-$90 physics constants.</summary>
internal static class SamusLiquidDamageDefinitions
{
    /// <summary>Lava rate at <c>$90:9E8B-$9E8E</c>: one half energy per frame.</summary>
    internal static readonly SamusLiquidDamageRate Lava = new(0x8000, 0x0000);

    /// <summary>Acid rate at <c>$90:9E8F-$9E92</c>: one and one-half energy per frame.</summary>
    internal static readonly SamusLiquidDamageRate Acid = new(0x8000, 0x0001);
}
