namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Verification access to <see cref="PeriodicDamageEnergyFraction"/>: a fraction a check or a recorded state states, reached
/// as periodic damage reaches it.
/// </summary>
internal static class PeriodicDamageEnergyFractionAccess
{
    /// <summary>States a held fraction through the damage arithmetic.</summary>
    extension(PeriodicDamageEnergyFraction)
    {
        /// <summary>
        /// A held fraction of <paramref name="parts"/>, 0 through one part short of an energy: no fraction held, after the
        /// periodic damage that borrows an energy and leaves exactly that much of it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="parts"/> is outside one energy's parts.</exception>
        internal static PeriodicDamageEnergyFraction Holding(int parts) => PeriodicDamageEnergyFraction.None.After(EnergyAmount.FromEnergyFraction(
            (PeriodicDamageEnergyFraction.PartsPerEnergy - Ensure.BetweenInclusive(0, PeriodicDamageEnergyFraction.PartsPerEnergy - 1, parts)) %
                PeriodicDamageEnergyFraction.PartsPerEnergy, PeriodicDamageEnergyFraction.PartsPerEnergy));
    }
}
