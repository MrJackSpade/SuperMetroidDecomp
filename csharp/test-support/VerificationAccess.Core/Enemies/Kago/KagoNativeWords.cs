using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// Verification's native view of the Kago identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class KagoNativeWords
{
    /// <summary>The routine word naming each <see cref="KagoEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<KagoEnemyFunction> KagoEnemyFunctions = new(new Dictionary<KagoEnemyFunction, ushort>
    {
        [KagoEnemyFunction.InstallNoOp] = 0xab7b,
        [KagoEnemyFunction.NoOp] = 0xab81,
    });
}
