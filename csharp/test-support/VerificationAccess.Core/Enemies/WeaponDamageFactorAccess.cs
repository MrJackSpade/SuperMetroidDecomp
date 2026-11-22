

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The cartridge's damage multiplier bytes as <see cref="WeaponDamageFactor"/> values (#2555), which only verification
/// builds: checks compare the game's named factors with the halves the original's vulnerability tables store.
/// </summary>
internal static class WeaponDamageFactorAccess
{
    /// <summary>Builds factors from the halves the original's tables store.</summary>
    extension(WeaponDamageFactor)
    {
        /// <summary>The factor of <paramref name="halves"/> halves of the weapon's damage, zero or more, as a table byte stores it.</summary>
        internal static WeaponDamageFactor FromStoredHalves(int halves) => new(halves);
    }
}
