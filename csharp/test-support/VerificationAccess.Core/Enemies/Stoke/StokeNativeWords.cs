using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// Verification's native view of the Stoke identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class StokeNativeWords
{
    /// <summary>The routine word naming each <see cref="StokeProjectileFunction"/>.</summary>
    internal static readonly NativeWordIdentities<StokeProjectileFunction> StokeProjectileFunctions = new(new Dictionary<StokeProjectileFunction, ushort>
    {
        [StokeProjectileFunction.MoveLeft] = 0xdb62,
        [StokeProjectileFunction.MoveRight] = 0xdb8c,
    });
}
