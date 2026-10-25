using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Verification's native view of the Dragon identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class DragonNativeWords
{
    /// <summary>The routine word naming each <see cref="DragonEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<DragonEnemyFunction> DragonEnemyFunctions = new(new Dictionary<DragonEnemyFunction, ushort>
    {
        [DragonEnemyFunction.WaitToRise] = 0xe654,
        [DragonEnemyFunction.Rising] = 0xe6ad,
        [DragonEnemyFunction.Attacking] = 0xe6f1,
        [DragonEnemyFunction.WaitToSink] = 0xe734,
        [DragonEnemyFunction.Sinking] = 0xe749,
        [DragonEnemyFunction.WingNoOp] = 0xe781,
    });
}
