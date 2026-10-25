using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// Verification's native view of the Cacatac identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class CacatacNativeWords
{
    /// <summary>The routine word naming each <see cref="CacatacEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<CacatacEnemyFunction> CacatacEnemyFunctions = new(new Dictionary<CacatacEnemyFunction, ushort>
    {
        [CacatacEnemyFunction.MovingLeft] = 0x9fba,
        [CacatacEnemyFunction.MovingRight] = 0x9fec,
        [CacatacEnemyFunction.Stopped] = 0xa01b,
    });
}
