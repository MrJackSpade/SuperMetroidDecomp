using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>
/// Verification's native view of the Sparks identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class SparksNativeWords
{
    /// <summary>The routine word naming each <see cref="SparkEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<SparkEnemyFunction> SparkEnemyFunctions = new(new Dictionary<SparkEnemyFunction, ushort>
    {
        [SparkEnemyFunction.AlwaysActive] = 0xe694,
        [SparkEnemyFunction.IntermittentInactive] = 0xe695,
        [SparkEnemyFunction.IntermittentActive] = 0xe6b7,
        [SparkEnemyFunction.EmitFallingSparks] = 0xe6dc,
        [SparkEnemyFunction.AdjacentCode] = 0x54ae,
    });
}
