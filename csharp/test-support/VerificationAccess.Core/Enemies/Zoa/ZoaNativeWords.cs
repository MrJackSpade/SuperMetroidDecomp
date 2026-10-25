using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// Verification's native view of the Zoa identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class ZoaNativeWords
{
    /// <summary>The routine word naming each <see cref="ZoaEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<ZoaEnemyFunction> ZoaEnemyFunctions = new(new Dictionary<ZoaEnemyFunction, ushort>
    {
        [ZoaEnemyFunction.WaitForSamus] = 0xb482,
        [ZoaEnemyFunction.Rising] = 0xb4a8,
        [ZoaEnemyFunction.Shooting] = 0xb4d6,
    });
}
