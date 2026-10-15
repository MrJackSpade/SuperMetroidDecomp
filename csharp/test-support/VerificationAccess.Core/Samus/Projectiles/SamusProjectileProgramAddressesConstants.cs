using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Cartridge values and helpers of <see cref="SamusProjectileProgramAddresses"/> that only verification reads.</summary>
internal static class SamusProjectileProgramAddressesConstants
{
    /// <summary>The list entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(SamusProjectileProgram program) => SamusProjectileProgramAddresses.StepAddresses[program][0];
}
