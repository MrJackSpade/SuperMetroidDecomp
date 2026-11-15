using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// Verification's native view of the Magdollite identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class MagdolliteNativeWords
{
    /// <summary>The routine word naming each <see cref="MagdolliteHeadAction"/>.</summary>
    internal static readonly NativeWordIdentities<MagdolliteHeadAction> MagdolliteHeadActions = new(new Dictionary<MagdolliteHeadAction, ushort>
    {
        [MagdolliteHeadAction.Waiting] = 0xb11a,
        [MagdolliteHeadAction.WaitingForAttackAnimation] = 0xb175,
        [MagdolliteHeadAction.WaitingForBodyLanding] = 0xb193,
        [MagdolliteHeadAction.WaitingForReturnAnimation] = 0xb1b8,
    });

    /// <summary>The routine word naming each <see cref="MagdolliteBodyAction"/>.</summary>
    internal static readonly NativeWordIdentities<MagdolliteBodyAction> MagdolliteBodyActions = new(new Dictionary<MagdolliteBodyAction, ushort>
    {
        [MagdolliteBodyAction.Dormant] = 0xb1dd,
        [MagdolliteBodyAction.Rising] = 0xb204,
        [MagdolliteBodyAction.HoldingAtApex] = 0xb291,
        [MagdolliteBodyAction.Falling] = 0xb295,
    });

    /// <summary>The routine word naming each <see cref="MagdolliteOverlayAction"/>.</summary>
    internal static readonly NativeWordIdentities<MagdolliteOverlayAction> MagdolliteOverlayActions = new(new Dictionary<MagdolliteOverlayAction, ushort>
    {
        [MagdolliteOverlayAction.WaitingForAttack] = 0xb30d,
        [MagdolliteOverlayAction.WaitingForThrowAnimation] = 0xb31f,
        [MagdolliteOverlayAction.TrackingRisingBody] = 0xb356,
        [MagdolliteOverlayAction.TrackingFallingBody] = 0xb3a7,
    });
}
