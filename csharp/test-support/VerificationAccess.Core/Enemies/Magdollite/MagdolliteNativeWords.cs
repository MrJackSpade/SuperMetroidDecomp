using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// Verification's native view of the Magdollite identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class MagdolliteNativeWords
{
    /// <summary>The routine word naming each <see cref="MagdolliteEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<MagdolliteEnemyFunction> MagdolliteEnemyFunctions = new(new Dictionary<MagdolliteEnemyFunction, ushort>
    {
        [MagdolliteEnemyFunction.HeadWaiting] = 0xb11a,
        [MagdolliteEnemyFunction.HeadWaitingForAttackAnimation] = 0xb175,
        [MagdolliteEnemyFunction.HeadWaitingForBodyLanding] = 0xb193,
        [MagdolliteEnemyFunction.HeadWaitingForReturnAnimation] = 0xb1b8,
        [MagdolliteEnemyFunction.BodyDormant] = 0xb1dd,
        [MagdolliteEnemyFunction.BodyRising] = 0xb204,
        [MagdolliteEnemyFunction.NoOpAtApex] = 0xb291,
        [MagdolliteEnemyFunction.BodyFalling] = 0xb295,
        [MagdolliteEnemyFunction.OverlayWaitingForAttack] = 0xb30d,
        [MagdolliteEnemyFunction.OverlayWaitingForThrowAnimation] = 0xb31f,
        [MagdolliteEnemyFunction.OverlayTrackingRisingBody] = 0xb356,
        [MagdolliteEnemyFunction.OverlayTrackingFallingBody] = 0xb3a7,
    });
}
