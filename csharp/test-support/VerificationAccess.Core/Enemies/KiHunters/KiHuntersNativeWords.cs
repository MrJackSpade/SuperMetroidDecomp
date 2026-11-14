using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// Verification's native view of the KiHunters identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class KiHuntersNativeWords
{
    /// <summary>The routine word naming each <see cref="KiHunterAction"/>.</summary>
    internal static readonly NativeWordIdentities<KiHunterAction> KiHunterActions = new(new Dictionary<KiHunterAction, ushort>
    {
        [KiHunterAction.FlyingPatrol] = 0xf268,
        [KiHunterAction.Swooping] = 0xf3b8,
        [KiHunterAction.RecoveringFromSwoop] = 0xf4ed,
        [KiHunterAction.FallingAfterWingLoss] = 0xf55a,
        [KiHunterAction.StartGroundJump] = 0xf58b,
        [KiHunterAction.AnimatingOnGround] = 0xf5e3,
        [KiHunterAction.GroundJump] = 0xf5f0,
        [KiHunterAction.GroundWait] = 0xf68b,
        [KiHunterAction.SelectAcidSpit] = 0xf6b3,
        [KiHunterAction.FollowBody] = 0xf6f3,
        [KiHunterAction.DetachedWing] = 0xf7cf,
    });

    /// <summary>The routine word naming each <see cref="KiHunterWingMovement"/>.</summary>
    internal static readonly NativeWordIdentities<KiHunterWingMovement> KiHunterWingMovements = new(new Dictionary<KiHunterWingMovement, ushort>
    {
        [KiHunterWingMovement.Orbit] = 0xf7db,
        [KiHunterWingMovement.FallingCollisionArc] = 0xf8ad,
    });
}
