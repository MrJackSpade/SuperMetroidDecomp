namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Samus's liquid-damage rates.</summary>
internal static class SamusLiquidDamageDefinitions
{
    /// <summary>Lava rate: one half energy per frame.</summary>
    internal static readonly EnergyAmount Lava = EnergyAmount.FromEnergyFraction(1, 2);

    /// <summary>Acid rate: one and one-half energy per frame.</summary>
    internal static readonly EnergyAmount Acid = EnergyAmount.FromEnergyFraction(3, 2);

    /// <summary>The least energy at which lava or acid damage plays its periodic sound; below it the damage is silent.</summary>
    internal const int DamageSoundLeastEnergy = 71;
}
