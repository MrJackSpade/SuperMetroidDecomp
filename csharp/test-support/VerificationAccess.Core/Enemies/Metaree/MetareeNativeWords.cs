using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>
/// Verification's native view of the Metaree identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class MetareeNativeWords
{
    /// <summary>The routine word naming each <see cref="MetareeEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<MetareeEnemyFunction> MetareeEnemyFunctions = new(new Dictionary<MetareeEnemyFunction, ushort>
    {
        [MetareeEnemyFunction.Idling] = 0x8987,
        [MetareeEnemyFunction.PreparingAttack] = 0x89d4,
        [MetareeEnemyFunction.LaunchedAttack] = 0x89f3,
        [MetareeEnemyFunction.Burrowing] = 0x8a5c,
    });
}
