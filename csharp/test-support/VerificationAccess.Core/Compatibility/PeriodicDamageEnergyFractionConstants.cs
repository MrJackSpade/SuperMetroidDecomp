namespace SuperMetroid.Core.Compatibility;

/// <summary>Cartridge values and helpers of <see cref="PeriodicDamageEnergyFraction"/> that only verification reads.</summary>
internal static class PeriodicDamageEnergyFractionConstants
{
    /// <summary>A held fraction of <paramref name="parts"/>, 0 through one part short of an energy, as a recorded state or a check states one.</summary>
    public static PeriodicDamageEnergyFraction Holding(int parts) => new(FractionalEnergyDrainConstants.HoldingOver(parts));
}
