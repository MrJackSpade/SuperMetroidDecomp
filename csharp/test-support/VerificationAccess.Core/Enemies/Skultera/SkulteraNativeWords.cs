using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// Verification's native view of the Skultera identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class SkulteraNativeWords
{
    /// <summary>The routine word naming each <see cref="SkulteraEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<SkulteraEnemyFunction> SkulteraEnemyFunctions = new(new Dictionary<SkulteraEnemyFunction, ushort>
    {
        [SkulteraEnemyFunction.SwimmingLeft] = 0x9132,
        [SkulteraEnemyFunction.SwimmingRight] = 0x91ab,
        [SkulteraEnemyFunction.TurningRight] = 0x9224,
        [SkulteraEnemyFunction.TurningLeft] = 0x9256,
    });
}
