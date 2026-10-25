using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// Verification's native view of the Skree identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class SkreeNativeWords
{
    /// <summary>The routine word naming each <see cref="SkreeEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<SkreeEnemyFunction> SkreeEnemyFunctions = new(new Dictionary<SkreeEnemyFunction, ushort>
    {
        [SkreeEnemyFunction.Idling] = 0xc6d5,
        [SkreeEnemyFunction.PreparingAttack] = 0xc6f7,
        [SkreeEnemyFunction.Diving] = 0xc716,
        [SkreeEnemyFunction.Burrowing] = 0xc77f,
    });
}
