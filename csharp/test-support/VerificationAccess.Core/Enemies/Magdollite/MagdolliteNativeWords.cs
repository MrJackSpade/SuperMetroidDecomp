using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// Verification's native view of the Magdollite identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class MagdolliteNativeWords
{
    /// <summary>The routine word naming each <see cref="MagdolliteAction"/>.</summary>
    internal static readonly NativeWordIdentities<MagdolliteAction> MagdolliteActions = new(new Dictionary<MagdolliteAction, ushort>
    {
        [MagdolliteAction.HeadWaiting] = 0xb11a,
        [MagdolliteAction.HeadWaitingForAttackAnimation] = 0xb175,
        [MagdolliteAction.HeadWaitingForBodyLanding] = 0xb193,
        [MagdolliteAction.HeadWaitingForReturnAnimation] = 0xb1b8,
        [MagdolliteAction.BodyDormant] = 0xb1dd,
        [MagdolliteAction.BodyRising] = 0xb204,
        [MagdolliteAction.NoOpAtApex] = 0xb291,
        [MagdolliteAction.BodyFalling] = 0xb295,
        [MagdolliteAction.OverlayWaitingForAttack] = 0xb30d,
        [MagdolliteAction.OverlayWaitingForThrowAnimation] = 0xb31f,
        [MagdolliteAction.OverlayTrackingRisingBody] = 0xb356,
        [MagdolliteAction.OverlayTrackingFallingBody] = 0xb3a7,
    });
}
