using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// Verification's native view of the Hibashi identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class HibashiNativeWords
{
    /// <summary>The routine word naming each <see cref="HibashiEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<HibashiEnemyFunction> HibashiEnemyFunctions = new(new Dictionary<HibashiEnemyFunction, ushort>
    {
        [HibashiEnemyFunction.Inactive] = 0x902f,
        [HibashiEnemyFunction.Active] = 0x9062,
    });
}
