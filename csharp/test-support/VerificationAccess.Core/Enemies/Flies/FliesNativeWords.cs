using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>
/// Verification's native view of the Flies identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class FliesNativeWords
{
    /// <summary>The routine word naming each <see cref="FlyEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<FlyEnemyFunction> FlyEnemyFunctions = new(new Dictionary<FlyEnemyFunction, ushort>
    {
        [FlyEnemyFunction.ClockwiseCircle] = 0xb14e,
        [FlyEnemyFunction.AntiClockwiseCircle] = 0xb17c,
        [FlyEnemyFunction.AttackSamus] = 0xb1aa,
        [FlyEnemyFunction.Retreat] = 0xb1d2,
    });
}
