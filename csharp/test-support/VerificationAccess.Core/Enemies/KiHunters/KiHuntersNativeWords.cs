using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// Verification's native view of the KiHunters identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class KiHuntersNativeWords
{
    /// <summary>The routine word naming each <see cref="KiHunterEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<KiHunterEnemyFunction> KiHunterEnemyFunctions = new(new Dictionary<KiHunterEnemyFunction, ushort>
    {
        [KiHunterEnemyFunction.FlyingPatrol] = 0xf268,
        [KiHunterEnemyFunction.Swooping] = 0xf3b8,
        [KiHunterEnemyFunction.RecoveringFromSwoop] = 0xf4ed,
        [KiHunterEnemyFunction.FallingAfterWingLoss] = 0xf55a,
        [KiHunterEnemyFunction.StartGroundJump] = 0xf58b,
        [KiHunterEnemyFunction.NoOp] = 0xf5e3,
        [KiHunterEnemyFunction.GroundJump] = 0xf5f0,
        [KiHunterEnemyFunction.GroundWait] = 0xf68b,
        [KiHunterEnemyFunction.SelectAcidSpit] = 0xf6b3,
        [KiHunterEnemyFunction.FollowBody] = 0xf6f3,
        [KiHunterEnemyFunction.DetachedWing] = 0xf7cf,
    });

    /// <summary>The routine word naming each <see cref="KiHunterWingFunction"/>.</summary>
    internal static readonly NativeWordIdentities<KiHunterWingFunction> KiHunterWingFunctions = new(new Dictionary<KiHunterWingFunction, ushort>
    {
        [KiHunterWingFunction.Orbit] = 0xf7db,
        [KiHunterWingFunction.FallingCollisionArc] = 0xf8ad,
    });
}
