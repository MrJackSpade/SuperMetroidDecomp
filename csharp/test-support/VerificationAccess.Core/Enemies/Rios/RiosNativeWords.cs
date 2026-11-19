using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// Verification's native view of the Rios identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class RiosNativeWords
{
    /// <summary>The routine word naming each <see cref="HoltzAction"/>.</summary>
    internal static readonly NativeWordIdentities<HoltzAction> HoltzActions = new(new Dictionary<HoltzAction, ushort>
    {
        [HoltzAction.WaitForAttackOpportunity] = 0xc771,
        [HoltzAction.WaitForTakeoffAnimation] = 0xc7bb,
        [HoltzAction.Dive] = 0xc7d6,
        [HoltzAction.ReturnToPerch] = 0xc82d,
        [HoltzAction.WaitForLandingAnimation] = 0xc888,
    });

    /// <summary>The routine word naming each <see cref="GerutaAction"/>.</summary>
    internal static readonly NativeWordIdentities<GerutaAction> GerutaActions = new(new Dictionary<GerutaAction, ushort>
    {
        [GerutaAction.WaitForAttackOpportunity] = 0xc2e7,
        [GerutaAction.WaitForTakeoffAnimation] = 0xc33f,
        [GerutaAction.Dive] = 0xc361,
        [GerutaAction.ReturnToPerch] = 0xc3b1,
        [GerutaAction.FinishLanding] = 0xc406,
    });

    /// <summary>The routine word naming each <see cref="RioAction"/>.</summary>
    internal static readonly NativeWordIdentities<RioAction> RioActions = new(new Dictionary<RioAction, ushort>
    {
        [RioAction.WaitingForSamus] = 0xbbed,
        [RioAction.WaitingForLandingAnimation] = 0xbc32,
        [RioAction.Diving] = 0xbc48,
        [RioAction.BouncingBackToPerch] = 0xbcb7,
        [RioAction.HoveringTowardSamus] = 0xbcff,
    });
}
