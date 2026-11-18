using SuperMetroid.Core.Compatibility;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Compiled liquid-damage rates from Samus's fixed physics constants.</summary>
internal static class SamusLiquidDamageDefinitions
{
    /// <summary>Lava rate: one half energy per frame.</summary>
    internal static readonly UnsignedSixteenSixteen Lava = new(whole: 0, fraction: PeriodicDamageEnergyFraction.PartsPerEnergy / 2);

    /// <summary>Acid rate: one and one-half energy per frame.</summary>
    internal static readonly UnsignedSixteenSixteen Acid = new(whole: 1, fraction: PeriodicDamageEnergyFraction.PartsPerEnergy / 2);

    /// <summary>The least energy at which lava or acid damage plays its periodic sound; below it the damage is silent.</summary>
    internal const int DamageSoundLeastEnergy = 71;
}
