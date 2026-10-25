using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Verification's native view of the MaridiaSnails identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class MaridiaSnailsNativeWords
{
    /// <summary>The routine word naming each <see cref="MaridiaLargeSnailBounceFunction"/>.</summary>
    internal static readonly NativeWordIdentities<MaridiaLargeSnailBounceFunction> MaridiaLargeSnailBounceFunctions = new(new Dictionary<MaridiaLargeSnailBounceFunction, ushort>
    {
        [MaridiaLargeSnailBounceFunction.Falling] = 0xcf66,
        [MaridiaLargeSnailBounceFunction.Rising] = 0xcfa9,
    });

    /// <summary>The routine word naming each <see cref="MaridiaLargeSnailEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<MaridiaLargeSnailEnemyFunction> MaridiaLargeSnailEnemyFunctions = new(new Dictionary<MaridiaLargeSnailEnemyFunction, ushort>
    {
        [MaridiaLargeSnailEnemyFunction.Idle] = 0xcde6,
        [MaridiaLargeSnailEnemyFunction.Rolling] = 0xce2b,
        [MaridiaLargeSnailEnemyFunction.Attacking] = 0xcf40,
    });
}
